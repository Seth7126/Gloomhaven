using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MonoMod.RuntimeDetour;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Debugging;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Fonts;
using XUnity.AutoTranslator.Plugin.Core.Hooks;
using XUnity.AutoTranslator.Plugin.Core.Parsing;
using XUnity.AutoTranslator.Plugin.Core.UI;
using XUnity.AutoTranslator.Plugin.Core.UIResize;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web.Internal;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;
using XUnity.ResourceRedirector;

namespace XUnity.AutoTranslator.Plugin.Core;

public class AutoTranslationPlugin : MonoBehaviour, IMonoBehaviour, IMonoBehaviour_Update, IInternalTranslator, ITranslator, ITranslationRegistry
{
	internal static AutoTranslationPlugin Current;

	private bool _hasResizedCurrentComponentDuringDiscovery;

	internal XuaWindow MainWindow;

	internal TranslationAggregatorWindow TranslationAggregatorWindow;

	internal TranslationAggregatorOptionsWindow TranslationAggregatorOptionsWindow;

	internal TranslationManager TranslationManager;

	internal TextTranslationCache TextCache;

	internal Dictionary<string, TextTranslationCache> PluginTextCaches = new Dictionary<string, TextTranslationCache>(StringComparer.OrdinalIgnoreCase);

	internal TextureTranslationCache TextureCache;

	internal UIResizeCache ResizeCache;

	internal SpamChecker SpamChecker;

	internal static RegexOptions RegexCompiledSupportedFlag = RegexOptions.None;

	private Dictionary<string, UntranslatedText> CachedKeys = new Dictionary<string, UntranslatedText>(StringComparer.Ordinal);

	private List<Action<ComponentTranslationContext>> _shouldIgnore = new List<Action<ComponentTranslationContext>>();

	private List<string> _textsToCopyToClipboardOrdered = new List<string>();

	private HashSet<string> _textsToCopyToClipboard = new HashSet<string>();

	private float _clipboardUpdated;

	private HashSet<string> _immediatelyTranslating = new HashSet<string>();

	private bool _isInTranslatedMode = true;

	private bool _textHooksEnabled = true;

	private float _batchOperationSecondCounter;

	private bool _hasValidOverrideFont;

	private bool _hasOverridenFont;

	private bool _initialized;

	private bool _started;

	private bool _temporarilyDisabled;

	private string _requireSpriteRendererCheckCausedBy;

	private int _lastSpriteUpdateFrame = -1;

	private bool _isCalledFromSceneManager;

	private bool _translationReloadRequest;

	private bool _hasUiBeenSetup;

	private static bool _inputSupported = true;

	public void Initialize()
	{
		Current = this;
		Paths.Initialize();
		HarmonyLoader.Load();
		Settings.Configure();
		DebugConsole.Enable();
		InitializeHarmonyDetourBridge();
		CheckRegexCompiledSupport();
		InitializeTextTranslationCaches();
		HooksSetup.InstallTextHooks();
		HooksSetup.InstallImageHooks();
		HooksSetup.InstallSpriteRendererHooks();
		HooksSetup.InstallTextGetterCompatHooks();
		HooksSetup.InstallComponentBasedPluginTranslationHooks();
		TextureCache = new TextureTranslationCache();
		TextureCache.TextureTranslationFileChanged += TextureCache_TextureTranslationFileChanged;
		ResizeCache = new UIResizeCache();
		TranslationManager = new TranslationManager();
		TranslationManager.JobCompleted += OnJobCompleted;
		TranslationManager.JobFailed += OnJobFailed;
		TranslationManager.InitializeEndpoints();
		SpamChecker = new SpamChecker(TranslationManager);
		UnityTextParsers.Initialize();
		InitializeResourceRedirector();
		ValidateConfiguration();
		EnableSceneLoadScan();
		LoadFallbackFont();
		LoadTranslations(reload: false);
		XuaLogger.AutoTranslator.Info("Loaded XUnity.AutoTranslator into Unity [" + Application.unityVersion + "] game.");
		AutoTranslatorState.OnPluginInitializationCompleted();
	}

	private void CheckRegexCompiledSupport()
	{
		try
		{
			string input = "She believed";
			if (new Regex(".he ..lie..d", RegexOptions.Compiled).Match(input).Success)
			{
				RegexCompiledSupportedFlag = RegexOptions.Compiled;
			}
			else
			{
				XuaLogger.AutoTranslator.Info("Unknown Error at CheckRegexCompiledSupport");
			}
		}
		catch (Exception)
		{
			XuaLogger.AutoTranslator.Info("The current version of the game doesn't support compiled Regex");
		}
	}

	private static void LoadFallbackFont()
	{
		try
		{
			if (!string.IsNullOrEmpty(Settings.FallbackFontTextMeshPro))
			{
				Object orCreateFallbackFontTextMeshPro = FontCache.GetOrCreateFallbackFontTextMeshPro();
				if (UnityTypes.TMP_Settings_Properties.FallbackFontAssets == null)
				{
					XuaLogger.AutoTranslator.Info("Cannot use fallback font because it is not supported in this version.");
					return;
				}
				if (orCreateFallbackFontTextMeshPro == (Object)null)
				{
					XuaLogger.AutoTranslator.Warn("Could not load fallback font for TextMesh Pro: " + Settings.FallbackFontTextMeshPro);
					return;
				}
				((IList)UnityTypes.TMP_Settings_Properties.FallbackFontAssets.Get(null)).Add(orCreateFallbackFontTextMeshPro);
				XuaLogger.AutoTranslator.Info("Loaded fallback font for TextMesh Pro: " + Settings.FallbackFontTextMeshPro);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while trying to load fallback font for TextMesh Pro.");
		}
	}

	private static void InitializeHarmonyDetourBridge()
	{
		try
		{
			if (Settings.InitializeHarmonyDetourBridge)
			{
				InitializeHarmonyDetourBridgeSafe();
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while initializing harmony detour bridge.");
		}
	}

	private static void InitializeHarmonyDetourBridgeSafe()
	{
		HarmonyDetourBridge.Init();
	}

	private void InitializeTextTranslationCaches()
	{
		try
		{
			TextCache = new TextTranslationCache();
			TextCache.TextTranslationFileChanged += TextCache_TextTranslationFileChanged;
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Settings.TranslationsPath, "plugins"));
			if (directoryInfo.Exists)
			{
				DirectoryInfo[] directories = directoryInfo.GetDirectories();
				foreach (DirectoryInfo directoryInfo2 in directories)
				{
					TextTranslationCache value = new TextTranslationCache(directoryInfo2);
					PluginTextCaches.Add(directoryInfo2.Name, value);
				}
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while initializing text translation caches.");
		}
	}

	private void TextCache_TextTranslationFileChanged()
	{
		_translationReloadRequest = true;
	}

	private void TextureCache_TextureTranslationFileChanged()
	{
		_translationReloadRequest = true;
	}

	private static void EnableLogAllLoadedResources()
	{
		ResourceRedirection.LogAllLoadedResources = true;
	}

	private void EnableTextAssetLoadedHandler()
	{
		new TextAssetLoadedHandler();
	}

	private void InitializeResourceRedirector()
	{
		try
		{
			if (Settings.LogAllLoadedResources)
			{
				EnableLogAllLoadedResources();
			}
			if (Settings.EnableTextAssetRedirector)
			{
				EnableTextAssetLoadedHandler();
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while initializing resource redirectors.");
		}
	}

	private void InitializeGUI()
	{
		DisableAutoTranslator();
		try
		{
			if (!_hasUiBeenSetup)
			{
				_hasUiBeenSetup = true;
				MainWindow = new XuaWindow(CreateXuaViewModel());
				TranslationAggregatorViewModel viewModel = CreateTranslationAggregatorViewModel();
				TranslationAggregatorWindow = new TranslationAggregatorWindow(viewModel);
				TranslationAggregatorOptionsWindow = new TranslationAggregatorOptionsWindow(viewModel);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while setting up UI.");
		}
		finally
		{
			EnableAutoTranslator();
		}
	}

	private TranslationAggregatorViewModel CreateTranslationAggregatorViewModel()
	{
		return new TranslationAggregatorViewModel(TranslationManager);
	}

	private XuaViewModel CreateXuaViewModel()
	{
		return new XuaViewModel(new List<ToggleViewModel>
		{
			new ToggleViewModel(" Translated", "<b>TRANSLATED</b>\nThe plugin currently displays translated texts. Disabling this does not mean the plugin will no longer perform translations, just that they will not be displayed.", "<b>NOT TRANSLATED</b>\nThe plugin currently displays untranslated texts.", ToggleTranslation, () => _isInTranslatedMode),
			new ToggleViewModel(" Silent Logging", "<b>SILENT</b>\nThe plugin will not print out success messages to the log in relation to translations.", "<b>VERBOSE</b>\nThe plugin will print out success messages to the log in relation to translations.", ToggleSilentMode, () => Settings.EnableSilentMode),
			new ToggleViewModel(" Translation Aggregator", "<b>SHOWN</b>\nThe translation aggregator window is shown.", "<b>HIDDEN</b>\nThe translation aggregator window is not shown.", ToggleTranslationAggregator, () => TranslationAggregatorWindow != null && TranslationAggregatorWindow.IsShown)
		}, new DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager>("----", "<b>SELECT TRANSLATOR</b>\nNo translator is currently selected, which means no new translations will be performed. Please select one from the dropdown.", "----", "<b>UNSELECT TRANSLATOR</b>\nThis will unselect the current translator, which means no new translations will be performed.", TranslationManager.AllEndpoints.Select((TranslationEndpointManager x) => new TranslatorDropdownOptionViewModel(fallback: false, () => x == TranslationManager.CurrentEndpoint, x)).ToList(), OnEndpointSelected), new DropdownViewModel<TranslatorDropdownOptionViewModel, TranslationEndpointManager>("----", "<b>SELECT FALLBACK TRANSLATOR</b>\nNo fallback translator is currently selected, which means if the primary translator fails no translation will be provided for the failing text. Please select one from the dropdown.", "----", "<b>UNSELECT FALLBACK TRANSLATOR</b>\nThis will unselect the current fallback translator.", TranslationManager.AllEndpoints.Select((TranslationEndpointManager x) => new TranslatorDropdownOptionViewModel(fallback: true, () => x == TranslationManager.FallbackEndpoint, x)).ToList(), OnFallbackEndpointSelected), new List<ButtonViewModel>
		{
			new ButtonViewModel("Reboot", "<b>REBOOT PLUGIN</b>\nReboots the plugin if it has been shutdown. This only works if the plugin was shut down due to consequtive errors towards the translation endpoint.", RebootPlugin, null),
			new ButtonViewModel("Reload", "<b>RELOAD TRANSLATION</b>\nReloads all translation text files and texture files from disk.", ReloadTranslations, null),
			new ButtonViewModel("Hook", "<b>MANUAL HOOK</b>\nTraverses the unity object tree for looking for anything that can be translated. Performs a translation if something is found.", ManualHook, null)
		}, new List<LabelViewModel>
		{
			new LabelViewModel("Version: ", () => "5.5"),
			new LabelViewModel("Plugin status: ", () => (!Settings.IsShutdown) ? "Running" : "Shutdown"),
			new LabelViewModel("Translator status: ", GetCurrentEndpointStatus),
			new LabelViewModel("Running translations: ", () => $"{TranslationManager.OngoingTranslations}"),
			new LabelViewModel("Served translations: ", () => $"{Settings.TranslationCount} / {Settings.MaxTranslationsBeforeShutdown}"),
			new LabelViewModel("Queued translations: ", () => $"{TranslationManager.UnstartedTranslations} / {Settings.MaxUnstartedJobs}"),
			new LabelViewModel("Error'ed translations: ", () => $"{TranslationManager.CurrentEndpoint?.ConsecutiveErrors ?? 0} / {Settings.MaxErrors}")
		});
	}

	private void ToggleTranslationAggregator()
	{
		if (TranslationAggregatorWindow != null)
		{
			TranslationAggregatorWindow.IsShown = !TranslationAggregatorWindow.IsShown;
		}
	}

	private void ToggleSilentMode()
	{
		Settings.SetSlientMode(!Settings.EnableSilentMode);
	}

	private string GetCurrentEndpointStatus()
	{
		TranslationEndpointManager currentEndpoint = TranslationManager.CurrentEndpoint;
		if (currentEndpoint == null)
		{
			return "Not selected";
		}
		if (currentEndpoint.HasFailedDueToConsecutiveErrors)
		{
			return "Shutdown";
		}
		return "Running";
	}

	private void ValidateConfiguration()
	{
		try
		{
			if (!string.IsNullOrEmpty(Settings.OverrideFont))
			{
				string[] supportedFonts = GetSupportedFonts();
				if (supportedFonts == null)
				{
					XuaLogger.AutoTranslator.Warn("Unable to validate OverrideFont validity due to shimmed APIs.");
				}
				else if (!supportedFonts.Contains(Settings.OverrideFont))
				{
					XuaLogger.AutoTranslator.Error("The specified override font is not available. Available fonts: " + string.Join(", ", supportedFonts));
					Settings.OverrideFont = null;
				}
				else
				{
					_hasValidOverrideFont = true;
				}
			}
			if (!string.IsNullOrEmpty(Settings.OverrideFontTextMeshPro))
			{
				_hasValidOverrideFont = true;
			}
			_hasOverridenFont = _hasValidOverrideFont;
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while checking supported fonts.");
		}
	}

	internal static string[] GetSupportedFonts()
	{
		try
		{
			return FontHelper.GetOSInstalledFontNames();
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "Unable to retrieve OS installed fonts.");
			return null;
		}
	}

	private void OnEndpointSelected(TranslationEndpointManager endpoint)
	{
		if (TranslationManager.CurrentEndpoint == endpoint)
		{
			return;
		}
		TranslationManager.CurrentEndpoint = endpoint;
		if (TranslationManager.CurrentEndpoint != null)
		{
			if (!Settings.IsShutdown)
			{
				if (TranslationManager.CurrentEndpoint.HasFailedDueToConsecutiveErrors)
				{
					RebootPlugin();
				}
				ManualHook();
			}
			if (TranslationManager.CurrentEndpoint == TranslationManager.FallbackEndpoint)
			{
				XuaLogger.AutoTranslator.Warn("Cannot use same fallback endpoint as primary.");
			}
		}
		Settings.SetEndpoint(TranslationManager.CurrentEndpoint?.Endpoint.Id);
		XuaLogger.AutoTranslator.Info("Set translator endpoint to '" + TranslationManager.CurrentEndpoint?.Endpoint.Id + "'.");
	}

	private void OnFallbackEndpointSelected(TranslationEndpointManager endpoint)
	{
		if (TranslationManager.FallbackEndpoint != endpoint)
		{
			TranslationManager.FallbackEndpoint = endpoint;
			Settings.SetFallback(TranslationManager.FallbackEndpoint?.Endpoint.Id);
			XuaLogger.AutoTranslator.Info("Set fallback endpoint to '" + TranslationManager.FallbackEndpoint?.Endpoint.Id + "'.");
			if (TranslationManager.CurrentEndpoint != null && TranslationManager.CurrentEndpoint == TranslationManager.FallbackEndpoint)
			{
				XuaLogger.AutoTranslator.Warn("Cannot use same fallback endpoint as primary.");
			}
		}
	}

	private void EnableSceneLoadScan()
	{
		try
		{
			XuaLogger.AutoTranslator.Debug("Probing whether OnLevelWasLoaded or SceneManager is supported in this version of Unity. Any warnings related to OnLevelWasLoaded coming from Unity can safely be ignored.");
			if (UnityFeatures.SupportsSceneManager)
			{
				TranslationScopeHelper.RegisterSceneLoadCallback(OnLevelWasLoadedFromSceneManager);
				XuaLogger.AutoTranslator.Debug("SceneManager is supported in this version of Unity.");
			}
			else
			{
				XuaLogger.AutoTranslator.Debug("SceneManager is not supported in this version of Unity. Falling back to OnLevelWasLoaded and Application level API.");
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while settings up scene-load scans.");
		}
	}

	internal void OnLevelWasLoadedFromSceneManager(int id)
	{
		try
		{
			_isCalledFromSceneManager = true;
			OnLevelWasLoaded(id);
		}
		finally
		{
			_isCalledFromSceneManager = false;
		}
	}

	private void OnLevelWasLoaded(int id)
	{
		if ((!UnityFeatures.SupportsSceneManager || (UnityFeatures.SupportsSceneManager && _isCalledFromSceneManager)) && Settings.EnableTextureScanOnSceneLoad && (Settings.EnableTextureDumping || Settings.EnableTextureTranslation))
		{
			XuaLogger.AutoTranslator.Info("Performing texture lookup during scene load...");
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			ManualHookForTextures();
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			XuaLogger.AutoTranslator.Info($"Finished texture lookup (took {Math.Round(realtimeSinceStartup2 - realtimeSinceStartup, 2)} seconds)");
		}
	}

	private void LoadTranslations(bool reload)
	{
		ResizeCache.LoadResizeCommandsInFiles();
		SettingsTranslationsInitializer.LoadTranslations();
		TextCache.LoadTranslationFiles();
		if (reload)
		{
			Dictionary<string, DirectoryInfo> dictionary = new Dictionary<string, DirectoryInfo>(StringComparer.OrdinalIgnoreCase);
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Settings.TranslationsPath, "plugins"));
			if (directoryInfo.Exists)
			{
				DirectoryInfo[] directories = directoryInfo.GetDirectories();
				foreach (DirectoryInfo directoryInfo2 in directories)
				{
					dictionary.Add(directoryInfo2.Name, directoryInfo2);
				}
			}
			foreach (KeyValuePair<string, TextTranslationCache> pluginTextCache in PluginTextCaches)
			{
				pluginTextCache.Value.LoadTranslationFiles();
				dictionary.Remove(pluginTextCache.Key);
			}
			foreach (KeyValuePair<string, DirectoryInfo> item in dictionary)
			{
				string assemblyName = item.Value.Name;
				TextTranslationCache textTranslationCache = new TextTranslationCache(item.Value);
				PluginTextCaches.Add(assemblyName, textTranslationCache);
				textTranslationCache.LoadTranslationFiles();
				Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly x) => x.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
				if (assembly != null)
				{
					HooksSetup.InstallIMGUIBasedPluginTranslationHooks(assembly, final: true);
				}
			}
			HooksSetup.InstallComponentBasedPluginTranslationHooks();
		}
		else
		{
			foreach (KeyValuePair<string, TextTranslationCache> pluginTextCache2 in PluginTextCaches)
			{
				pluginTextCache2.Value.LoadTranslationFiles();
			}
		}
		TextureCache.LoadTranslationFiles();
	}

	private void CreateTranslationJobFor(TranslationEndpointManager endpoint, object ui, UntranslatedText key, InternalTranslationResult translationResult, ParserTranslationContext context, bool checkOtherEndpoints, bool checkSpam, bool saveResultGlobally, bool isTranslatable, bool allowFallback, UntranslatedTextInfo untranslatedTextContext)
	{
		if (endpoint.EnqueueTranslation(ui, key, translationResult, context, untranslatedTextContext, checkOtherEndpoints, saveResultGlobally, isTranslatable, allowFallback) != null && isTranslatable && checkSpam && !(endpoint.Endpoint is PassthroughTranslateEndpoint))
		{
			SpamChecker.PerformChecks(key.TemplatedOriginal_Text_FullyTrimmed, endpoint);
		}
	}

	private void IncrementBatchOperations()
	{
		_batchOperationSecondCounter += Time.deltaTime;
		if (!(_batchOperationSecondCounter > Settings.IncreaseBatchOperationsEvery))
		{
			return;
		}
		foreach (TranslationEndpointManager configuredEndpoint in TranslationManager.ConfiguredEndpoints)
		{
			if (configuredEndpoint.AvailableBatchOperations < Settings.MaxAvailableBatchOperations)
			{
				configuredEndpoint.AvailableBatchOperations++;
			}
		}
		_batchOperationSecondCounter = 0f;
	}

	private void UpdateSpriteRenderers()
	{
		if (!Settings.EnableSpriteRendererHooking || (!Settings.EnableTextureTranslation && !Settings.EnableTextureDumping) || _requireSpriteRendererCheckCausedBy == null)
		{
			return;
		}
		try
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			SpriteRenderer[] array = ComponentHelper.FindObjectsOfType<SpriteRenderer>();
			foreach (SpriteRenderer source in array)
			{
				Texture2D texture = null;
				Hook_ImageChangedOnComponent(source, ref texture, isPrefixHooked: false, onEnable: false);
			}
			double num = Math.Round(Time.realtimeSinceStartup - realtimeSinceStartup, 2);
			XuaLogger.AutoTranslator.Debug("Update SpriteRenderers caused by " + _requireSpriteRendererCheckCausedBy + " component (took " + num + " seconds)");
		}
		finally
		{
			_requireSpriteRendererCheckCausedBy = null;
		}
	}

	private void QueueNewUntranslatedForClipboard(string untranslatedText)
	{
		if (Settings.CopyToClipboard && UnityFeatures.SupportsClipboard && !_textsToCopyToClipboard.Contains(untranslatedText))
		{
			_textsToCopyToClipboard.Add(untranslatedText);
			_textsToCopyToClipboardOrdered.Add(untranslatedText);
			_clipboardUpdated = Time.realtimeSinceStartup;
		}
	}

	internal string Hook_TextChanged_WithResult(object ui, string text, bool onEnable)
	{
		try
		{
			string result = null;
			if (_textHooksEnabled && !_temporarilyDisabled)
			{
				try
				{
					TextTranslationInfo orCreateTextTranslationInfo = ui.GetOrCreateTextTranslationInfo();
					bool ignoreComponentState = DiscoverComponent(ui, orCreateTextTranslationInfo);
					if (onEnable && orCreateTextTranslationInfo != null && CallOrigin.TextCache != null)
					{
						orCreateTextTranslationInfo.TextCache = CallOrigin.TextCache;
					}
					CallOrigin.ExpectsTextToBeReturned = true;
					result = TranslateOrQueueWebJob(ui, text, ignoreComponentState, orCreateTextTranslationInfo);
				}
				catch (Exception e)
				{
					XuaLogger.AutoTranslator.Warn(e, "An unexpected error occurred.");
				}
				finally
				{
					_hasResizedCurrentComponentDuringDiscovery = false;
				}
			}
			if (onEnable)
			{
				CheckSpriteRenderer(ui);
			}
			return result;
		}
		finally
		{
			CallOrigin.ExpectsTextToBeReturned = false;
		}
	}

	internal void Hook_TextChanged(object ui, bool onEnable)
	{
		if (_textHooksEnabled && !_temporarilyDisabled)
		{
			try
			{
				TextTranslationInfo orCreateTextTranslationInfo = ui.GetOrCreateTextTranslationInfo();
				bool ignoreComponentState = DiscoverComponent(ui, orCreateTextTranslationInfo);
				if (onEnable && orCreateTextTranslationInfo != null && CallOrigin.TextCache != null)
				{
					orCreateTextTranslationInfo.TextCache = CallOrigin.TextCache;
				}
				TranslateOrQueueWebJob(ui, null, ignoreComponentState, orCreateTextTranslationInfo);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Warn(e, "An unexpected error occurred.");
			}
			finally
			{
				_hasResizedCurrentComponentDuringDiscovery = false;
			}
		}
		if (onEnable)
		{
			CheckSpriteRenderer(ui);
		}
	}

	internal void Hook_ImageChangedOnComponent(object source, ref Texture2D texture, bool isPrefixHooked, bool onEnable)
	{
		if (CallOrigin.ImageHooksEnabled && source.IsKnownImageType())
		{
			Sprite sprite = null;
			HandleImage(source, ref sprite, ref texture, isPrefixHooked);
			if (onEnable)
			{
				CheckSpriteRenderer(source);
			}
		}
	}

	internal void Hook_ImageChangedOnComponent(object source, ref Sprite sprite, ref Texture2D texture, bool isPrefixHooked, bool onEnable)
	{
		if (CallOrigin.ImageHooksEnabled && source.IsKnownImageType())
		{
			HandleImage(source, ref sprite, ref texture, isPrefixHooked);
			if (onEnable)
			{
				CheckSpriteRenderer(source);
			}
		}
	}

	internal void Hook_ImageChanged(ref Texture2D texture, bool isPrefixHooked)
	{
		if (CallOrigin.ImageHooksEnabled && !((Object)(object)texture == (Object)null))
		{
			Sprite sprite = null;
			HandleImage(null, ref sprite, ref texture, isPrefixHooked);
		}
	}

	private bool DiscoverComponent(object ui, TextTranslationInfo info)
	{
		if (info == null)
		{
			return true;
		}
		try
		{
			bool flag = ui.IsComponentActive();
			if ((_hasValidOverrideFont || Settings.ForceUIResizing) && flag)
			{
				if (_hasValidOverrideFont)
				{
					if (_hasOverridenFont)
					{
						info.ChangeFont(ui);
					}
					else
					{
						info.UnchangeFont(ui);
					}
				}
				if (Settings.ForceUIResizing)
				{
					info.ResizeUI(ui, ResizeCache);
					_hasResizedCurrentComponentDuringDiscovery = true;
				}
				return true;
			}
			return flag;
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Warn(e, "An error occurred while handling the UI discovery.");
		}
		return false;
	}

	private void CheckSpriteRenderer(object ui)
	{
		if (Settings.EnableSpriteRendererHooking)
		{
			int frameCount = Time.frameCount;
			if (frameCount - 1 != _lastSpriteUpdateFrame && frameCount != _lastSpriteUpdateFrame)
			{
				_requireSpriteRendererCheckCausedBy = ui?.GetType().Name;
			}
			_lastSpriteUpdateFrame = frameCount;
		}
	}

	internal void SetTranslatedText(object ui, string translatedText, string originalText, TextTranslationInfo info)
	{
		info?.SetTranslatedText(translatedText);
		if (_isInTranslatedMode && !CallOrigin.ExpectsTextToBeReturned)
		{
			SetText(ui, translatedText, isTranslated: true, originalText, info);
		}
	}

	private void SetText(object ui, string text, bool isTranslated, string originalText, TextTranslationInfo info)
	{
		if (info != null && info.IsCurrentlySettingText)
		{
			return;
		}
		try
		{
			_textHooksEnabled = false;
			if (info != null)
			{
				info.IsCurrentlySettingText = true;
			}
			if (Settings.EnableTextPathLogging)
			{
				string path = ui.GetPath();
				if (path != null)
				{
					int scope = TranslationScopeHelper.GetScope(ui);
					XuaLogger.AutoTranslator.Info("Setting text on '" + ui.GetType().FullName + "' to '" + text + "'");
					XuaLogger.AutoTranslator.Info("Path : " + path);
					XuaLogger.AutoTranslator.Info("Level: " + scope);
				}
			}
			if (!_hasResizedCurrentComponentDuringDiscovery && info != null && (Settings.EnableUIResizing || Settings.ForceUIResizing))
			{
				if (isTranslated || Settings.ForceUIResizing)
				{
					info.ResizeUI(ui, ResizeCache);
				}
				else
				{
					info.UnresizeUI(ui);
				}
			}
			ui.SetText(text, info);
			info?.ResetScrollIn(ui);
			if (info.GetIsKnownTextComponent() && originalText != null && ui != null && !ui.IsSpammingComponent())
			{
				if (_isInTranslatedMode && isTranslated)
				{
					TranslationHelper.DisplayTranslationInfo(originalText, text);
				}
				QueueNewUntranslatedForClipboard(originalText);
				if (TranslationAggregatorWindow != null)
				{
					TranslationAggregatorWindow.OnNewTranslationAdded(originalText, text);
				}
			}
		}
		catch (TargetInvocationException)
		{
		}
		catch (NullReferenceException)
		{
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while setting text on a component.");
		}
		finally
		{
			_textHooksEnabled = true;
			if (info != null)
			{
				info.IsCurrentlySettingText = false;
			}
		}
	}

	private bool IsBelowMaxLength(string str)
	{
		return str.Length <= Settings.MaxCharactersPerTranslation;
	}

	private string TranslateOrQueueWebJob(object ui, string text, bool ignoreComponentState, TextTranslationInfo info)
	{
		IReadOnlyTextTranslationCache textCache = CallOrigin.GetTextCache(info, TextCache);
		if (info != null && info.IsStabilizingText)
		{
			return TranslateImmediate(ui, text, info, ignoreComponentState, textCache);
		}
		return TranslateOrQueueWebJobImmediate(ui, text, -1, info, info.GetSupportsStabilization(), ignoreComponentState, allowStartTranslationImmediate: false, textCache.AllowGeneratingNewTranslations, textCache, null, null);
	}

	private void HandleImage(object source, ref Sprite sprite, ref Texture2D texture, bool isPrefixHooked)
	{
		if (Settings.EnableTextureDumping)
		{
			try
			{
				DumpTexture(source, texture);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while dumping texture.");
			}
		}
		if (Settings.EnableTextureTranslation)
		{
			try
			{
				TranslateTexture(source, ref sprite, ref texture, isPrefixHooked, null);
			}
			catch (Exception e2)
			{
				XuaLogger.AutoTranslator.Error(e2, "An error occurred while translating texture.");
			}
		}
	}

	private void TranslateTexture(object ui, ref Sprite sprite, TextureReloadContext context)
	{
		if (ui.TryCastTo<Texture2D>(out var castedObject))
		{
			TranslateTexture(null, ref sprite, ref castedObject, isPrefixHooked: false, context);
			return;
		}
		Texture2D texture = null;
		TranslateTexture(ui, ref sprite, ref texture, isPrefixHooked: false, context);
	}

	private void TranslateTexture(object ui, TextureReloadContext context)
	{
		Sprite sprite = null;
		if (ui.TryCastTo<Texture2D>(out var castedObject))
		{
			TranslateTexture(null, ref sprite, ref castedObject, isPrefixHooked: false, context);
			return;
		}
		Texture2D texture = null;
		TranslateTexture(ui, ref sprite, ref texture, isPrefixHooked: false, context);
	}

	private void TranslateTexture(object source, ref Sprite sprite, ref Texture2D texture, bool isPrefixHooked, TextureReloadContext context)
	{
		try
		{
			CallOrigin.ImageHooksEnabled = false;
			Texture2D val = texture;
			texture = texture ?? source.GetTexture();
			if ((Object)(object)texture == (Object)null)
			{
				return;
			}
			TextureTranslationInfo orCreateTextureTranslationInfo = texture.GetOrCreateTextureTranslationInfo();
			ImageTranslationInfo orCreateImageTranslationInfo = source.GetOrCreateImageTranslationInfo(texture);
			string key = orCreateTextureTranslationInfo.GetKey();
			if (string.IsNullOrEmpty(key))
			{
				return;
			}
			bool flag = context != null;
			bool flag2 = false;
			bool flag3 = false;
			if (flag)
			{
				flag2 = context.RegisterTextureInContextAndDetermineWhetherToReload(texture);
			}
			if (Settings.EnableLegacyTextureLoading && Settings.EnableSpriteRendererHooking && orCreateImageTranslationInfo != null && orCreateImageTranslationInfo.IsTranslated && source.TryCastTo<SpriteRenderer>(out var castedObject))
			{
				Texture2D target = orCreateTextureTranslationInfo.Original.Target;
				Texture2D translated = orCreateTextureTranslationInfo.Translated;
				if (object.Equals(texture, target) && orCreateTextureTranslationInfo.IsTranslated)
				{
					if ((Object)(object)orCreateTextureTranslationInfo.TranslatedSprite != (Object)null)
					{
						if (isPrefixHooked)
						{
							if ((Object)(object)sprite != (Object)null)
							{
								sprite = orCreateTextureTranslationInfo.TranslatedSprite;
							}
						}
						else
						{
							castedObject.sprite = orCreateTextureTranslationInfo.TranslatedSprite;
						}
					}
				}
				else if (!object.Equals(texture, translated))
				{
					orCreateImageTranslationInfo.Reset(texture);
					if (orCreateTextureTranslationInfo.IsTranslated && isPrefixHooked && (Object)(object)sprite != (Object)null && (Object)(object)orCreateTextureTranslationInfo.TranslatedSprite != (Object)null)
					{
						sprite = orCreateTextureTranslationInfo.TranslatedSprite;
					}
				}
			}
			if (TextureCache.TryGetTranslatedImage(key, out var data, out var image))
			{
				if (_isInTranslatedMode)
				{
					bool flag4 = texture.IsCompatible(image.ImageFormat);
					if (!orCreateTextureTranslationInfo.IsTranslated || flag2)
					{
						try
						{
							if (Settings.EnableLegacyTextureLoading || !flag4)
							{
								orCreateTextureTranslationInfo.CreateTranslatedTexture(data, image.ImageFormat);
								flag3 = true;
							}
							else
							{
								texture.LoadImageEx(data, image.ImageFormat, null);
								flag3 = true;
							}
						}
						finally
						{
							orCreateTextureTranslationInfo.IsTranslated = true;
						}
					}
					if (orCreateImageTranslationInfo != null && (!orCreateImageTranslationInfo.IsTranslated || flag))
					{
						try
						{
							if (Settings.EnableLegacyTextureLoading || !flag4)
							{
								Sprite val2 = source.SetTexture(orCreateTextureTranslationInfo.Translated, sprite, isPrefixHooked);
								if ((Object)(object)val2 != (Object)null)
								{
									orCreateTextureTranslationInfo.TranslatedSprite = val2;
									if (isPrefixHooked && (Object)(object)sprite != (Object)null)
									{
										sprite = val2;
									}
								}
							}
							if (!isPrefixHooked)
							{
								source.SetAllDirtyEx();
							}
						}
						finally
						{
							orCreateImageTranslationInfo.IsTranslated = true;
						}
					}
				}
			}
			else
			{
				byte[] originalData = orCreateTextureTranslationInfo.GetOriginalData();
				if (originalData != null)
				{
					if (orCreateTextureTranslationInfo.IsTranslated)
					{
						try
						{
							if (Settings.EnableLegacyTextureLoading)
							{
								orCreateTextureTranslationInfo.CreateOriginalTexture();
								flag3 = true;
							}
							else
							{
								texture.LoadImageEx(originalData, ImageFormat.PNG, null);
								flag3 = true;
							}
						}
						finally
						{
							orCreateTextureTranslationInfo.IsTranslated = true;
						}
					}
					if (orCreateImageTranslationInfo != null && orCreateImageTranslationInfo.IsTranslated)
					{
						try
						{
							Texture2D target2 = orCreateTextureTranslationInfo.Original.Target;
							if (Settings.EnableLegacyTextureLoading && (Object)(object)target2 != (Object)null)
							{
								source.SetTexture(target2, null, isPrefixHooked);
							}
							if (!isPrefixHooked)
							{
								source.SetAllDirtyEx();
							}
						}
						finally
						{
							orCreateImageTranslationInfo.IsTranslated = true;
						}
					}
				}
			}
			if (!_isInTranslatedMode)
			{
				byte[] originalData2 = orCreateTextureTranslationInfo.GetOriginalData();
				if (originalData2 != null)
				{
					if (orCreateTextureTranslationInfo.IsTranslated)
					{
						try
						{
							if (Settings.EnableLegacyTextureLoading)
							{
								orCreateTextureTranslationInfo.CreateOriginalTexture();
								flag3 = true;
							}
							else
							{
								texture.LoadImageEx(originalData2, ImageFormat.PNG, null);
								flag3 = true;
							}
						}
						finally
						{
							orCreateTextureTranslationInfo.IsTranslated = false;
						}
					}
					if (orCreateImageTranslationInfo != null && orCreateImageTranslationInfo.IsTranslated)
					{
						try
						{
							Texture2D target3 = orCreateTextureTranslationInfo.Original.Target;
							if (Settings.EnableLegacyTextureLoading && (Object)(object)target3 != (Object)null)
							{
								source.SetTexture(target3, null, isPrefixHooked);
							}
							if (!isPrefixHooked)
							{
								source.SetAllDirtyEx();
							}
						}
						finally
						{
							orCreateImageTranslationInfo.IsTranslated = false;
						}
					}
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				texture = null;
			}
			else if (orCreateTextureTranslationInfo.UsingReplacedTexture)
			{
				if (orCreateTextureTranslationInfo.IsTranslated)
				{
					Texture2D translated2 = orCreateTextureTranslationInfo.Translated;
					if ((Object)(object)translated2 != (Object)null)
					{
						texture = translated2;
					}
				}
				else
				{
					Texture2D target4 = orCreateTextureTranslationInfo.Original.Target;
					if ((Object)(object)target4 != (Object)null)
					{
						texture = target4;
					}
				}
			}
			else
			{
				texture = val;
			}
			if (flag2 && flag3)
			{
				XuaLogger.AutoTranslator.Info("Reloaded texture: " + ((Object)texture).name + " (" + key + ").");
			}
		}
		finally
		{
			CallOrigin.ImageHooksEnabled = true;
		}
	}

	private void DumpTexture(object source, Texture2D texture)
	{
		try
		{
			CallOrigin.ImageHooksEnabled = false;
			texture = texture ?? source.GetTexture();
			if ((Object)(object)texture == (Object)null)
			{
				return;
			}
			TextureTranslationInfo orCreateTextureTranslationInfo = texture.GetOrCreateTextureTranslationInfo();
			if (orCreateTextureTranslationInfo.IsDumped)
			{
				return;
			}
			try
			{
				if (ShouldTranslate(texture))
				{
					string key = orCreateTextureTranslationInfo.GetKey();
					if (!string.IsNullOrEmpty(key) && !TextureCache.IsImageRegistered(key))
					{
						string textureName = texture.GetTextureName("Unnamed");
						byte[] orCreateOriginalData = orCreateTextureTranslationInfo.GetOrCreateOriginalData();
						TextureCache.RegisterImageFromData(textureName, key, orCreateOriginalData);
					}
				}
			}
			finally
			{
				orCreateTextureTranslationInfo.IsDumped = true;
			}
		}
		finally
		{
			CallOrigin.ImageHooksEnabled = true;
		}
	}

	private bool ShouldTranslate(Texture2D texture)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		int num = (int)texture.format;
		if (num != 1 && num != 9)
		{
			return num != 63;
		}
		return false;
	}

	private string TranslateImmediate(object ui, string text, TextTranslationInfo info, bool ignoreComponentState, IReadOnlyTextTranslationCache tc)
	{
		if (info != null && info.IsCurrentlySettingText)
		{
			return null;
		}
		text = text ?? ui.GetText(info);
		string text2 = text;
		if (info != null && info.IsTranslated && text2 == info.TranslatedText)
		{
			return null;
		}
		bool flag = false;
		if (info != null)
		{
			info.Reset(text2);
			flag = info.ShouldIgnore;
		}
		if (text.IsNullOrWhiteSpace())
		{
			return null;
		}
		if (CheckAndFixRedirected(ui, text, info))
		{
			return null;
		}
		int scope = TranslationScopeHelper.GetScope(ui);
		if (!flag && tc.IsTranslatable(text, isToken: false, scope) && (ignoreComponentState || ui.IsComponentActive()))
		{
			bool isFromSpammingComponent = ui.IsSpammingComponent();
			UntranslatedText cacheKey = GetCacheKey(text, isFromSpammingComponent);
			if ((cacheKey.IsTemplated && !tc.IsTranslatable(cacheKey.TemplatedOriginal_Text, isToken: false, scope)) || cacheKey.IsOnlyTemplate)
			{
				string text3 = cacheKey.Untemplate(cacheKey.TemplatedOriginal_Text);
				bool flag2 = tc.IsPartial(cacheKey.TemplatedOriginal_Text, scope);
				SetTranslatedText(ui, text3, (!flag2) ? text2 : null, info);
				return text3;
			}
			if (tc.TryGetTranslation(cacheKey, allowRegex: false, allowToken: false, scope, out var value))
			{
				string text4 = cacheKey.Untemplate(value);
				bool flag3 = tc.IsPartial(cacheKey.TemplatedOriginal_Text, scope);
				SetTranslatedText(ui, text4, (!flag3) ? text2 : null, info);
				return text4;
			}
			if (UnityTextParsers.GameLogTextParser.CanApply(ui))
			{
				ParserResult parserResult = UnityTextParsers.GameLogTextParser.Parse(text, scope, tc);
				if (parserResult != null)
				{
					bool flag4 = LanguageHelper.IsTranslatable(cacheKey.TemplatedOriginal_Text);
					value = TranslateOrQueueWebJobImmediateByParserResult(ui, parserResult, scope, allowStartTranslationImmediate: false, allowStartTranslationLater: false, flag4 || Settings.OutputUntranslatableText, tc, null);
					if (value != null)
					{
						tc.IsPartial(cacheKey.TemplatedOriginal_Text, scope);
						SetTranslatedText(ui, value, null, info);
						return value;
					}
				}
			}
		}
		return null;
	}

	private ComponentTranslationContext InvokeOnTranslatingCallback(object textComponent, string untranslatedText, TextTranslationInfo info)
	{
		int count = _shouldIgnore.Count;
		if (info != null && !info.IsCurrentlySettingText && count > 0)
		{
			try
			{
				ComponentTranslationContext componentTranslationContext = new ComponentTranslationContext(textComponent, untranslatedText);
				info.IsCurrentlySettingText = true;
				for (int i = 0; i < count; i++)
				{
					_shouldIgnore[i](componentTranslationContext);
					if (componentTranslationContext.Behaviour != ComponentTranslationBehaviour.Default)
					{
						return componentTranslationContext;
					}
				}
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred during a on-translating callback.");
			}
			finally
			{
				info.IsCurrentlySettingText = false;
			}
		}
		return null;
	}

	void ITranslator.IgnoreTextComponent(object textComponent)
	{
		TextTranslationInfo orCreateTextTranslationInfo = textComponent.GetOrCreateTextTranslationInfo();
		if (orCreateTextTranslationInfo != null)
		{
			orCreateTextTranslationInfo.ShouldIgnore = true;
		}
	}

	void ITranslator.UnignoreTextComponent(object textComponent)
	{
		TextTranslationInfo orCreateTextTranslationInfo = textComponent.GetOrCreateTextTranslationInfo();
		if (orCreateTextTranslationInfo != null)
		{
			orCreateTextTranslationInfo.ShouldIgnore = false;
		}
	}

	void ITranslator.RegisterOnTranslatingCallback(Action<ComponentTranslationContext> shouldIgnore)
	{
		_shouldIgnore.Add(shouldIgnore);
	}

	void ITranslator.UnregisterOnTranslatingCallback(Action<ComponentTranslationContext> shouldIgnore)
	{
		_shouldIgnore.Remove(shouldIgnore);
	}

	void IInternalTranslator.TranslateAsync(TranslationEndpointManager endpoint, string untranslatedText, Action<TranslationResult> onCompleted)
	{
		Translate(untranslatedText, -1, endpoint, null, onCompleted, isGlobal: false, allowStartTranslateImmediate: true, allowFallback: false, null);
	}

	void ITranslator.TranslateAsync(string untranslatedText, Action<TranslationResult> onCompleted)
	{
		Translate(untranslatedText, -1, TranslationManager.CurrentEndpoint, null, onCompleted, isGlobal: true, allowStartTranslateImmediate: true, allowFallback: true, null);
	}

	void ITranslator.TranslateAsync(string untranslatedText, int scope, Action<TranslationResult> onCompleted)
	{
		Translate(untranslatedText, scope, TranslationManager.CurrentEndpoint, null, onCompleted, isGlobal: true, allowStartTranslateImmediate: true, allowFallback: true, null);
	}

	bool ITranslator.TryTranslate(string text, out string translatedText)
	{
		return TryTranslate(text, -1, out translatedText);
	}

	bool ITranslator.TryTranslate(string text, int scope, out string translatedText)
	{
		return TryTranslate(text, scope, out translatedText);
	}

	private bool TryTranslate(string text, int scope, out string translatedText)
	{
		if (scope == -1)
		{
			scope = TranslationScopeHelper.GetScope(null);
		}
		if (!text.IsNullOrWhiteSpace() && TextCache.IsTranslatable(text, isToken: false, scope))
		{
			UntranslatedText cacheKey = GetCacheKey(text, isFromSpammingComponent: false);
			if ((cacheKey.IsTemplated && !TextCache.IsTranslatable(cacheKey.TemplatedOriginal_Text, isToken: false, scope)) || cacheKey.IsOnlyTemplate)
			{
				string text2 = cacheKey.Untemplate(cacheKey.TemplatedOriginal_Text);
				translatedText = text2;
				return true;
			}
			if (TextCache.TryGetTranslation(cacheKey, allowRegex: true, allowToken: false, scope, out var value))
			{
				translatedText = value;
				return true;
			}
			ParserResult parserResult = UnityTextParsers.RegexSplittingTextParser.Parse(text, scope, TextCache) ?? UnityTextParsers.RichTextParser.Parse(text, scope);
			if (parserResult != null)
			{
				translatedText = TranslateByParserResult(null, parserResult, scope, null, allowStartTranslateImmediate: false, isGlobal: true, allowFallback: false, null);
				return translatedText != null;
			}
		}
		translatedText = null;
		return false;
	}

	private InternalTranslationResult Translate(string text, int scope, TranslationEndpointManager endpoint, ParserTranslationContext context, Action<TranslationResult> onCompleted, bool isGlobal, bool allowStartTranslateImmediate, bool allowFallback, UntranslatedTextInfo untranslatedTextContext)
	{
		InternalTranslationResult internalTranslationResult = new InternalTranslationResult(isGlobal, onCompleted);
		if (isGlobal)
		{
			if (scope == -1 && context == null)
			{
				scope = TranslationScopeHelper.GetScope(null);
			}
			if (!text.IsNullOrWhiteSpace() && TextCache.IsTranslatable(text, isToken: false, scope))
			{
				UntranslatedText cacheKey = GetCacheKey(text, isFromSpammingComponent: false);
				if ((cacheKey.IsTemplated && !TextCache.IsTranslatable(cacheKey.TemplatedOriginal_Text, isToken: false, scope)) || cacheKey.IsOnlyTemplate)
				{
					string completed = cacheKey.Untemplate(cacheKey.TemplatedOriginal_Text);
					internalTranslationResult.SetCompleted(completed);
					return internalTranslationResult;
				}
				if (TextCache.TryGetTranslation(cacheKey, allowRegex: true, allowToken: false, scope, out var value))
				{
					internalTranslationResult.SetCompleted(cacheKey.Untemplate(value));
					return internalTranslationResult;
				}
				if (context.GetLevelsOfRecursion() < Settings.MaxTextParserRecursion)
				{
					ParserResult parserResult = UnityTextParsers.RegexSplittingTextParser.Parse(text, scope, TextCache);
					if (parserResult != null)
					{
						value = TranslateByParserResult(endpoint, parserResult, scope, internalTranslationResult, allowStartTranslateImmediate, internalTranslationResult.IsGlobal, allowFallback, context);
						if (value != null)
						{
							internalTranslationResult.SetCompleted(value);
						}
						return internalTranslationResult;
					}
					if (!context.HasBeenParsedBy(ParserResultOrigin.RichTextParser))
					{
						parserResult = UnityTextParsers.RichTextParser.Parse(text, scope);
						if (parserResult != null)
						{
							value = TranslateByParserResult(endpoint, parserResult, scope, internalTranslationResult, allowStartTranslateImmediate, internalTranslationResult.IsGlobal, allowFallback, context);
							if (value != null)
							{
								internalTranslationResult.SetCompleted(value);
							}
							return internalTranslationResult;
						}
					}
				}
				else if (Settings.MaxTextParserRecursion != 1)
				{
					XuaLogger.AutoTranslator.Warn("Attempted to exceed maximum allowed levels of text parsing recursion!");
				}
				bool flag = LanguageHelper.IsTranslatable(cacheKey.TemplatedOriginal_Text);
				if (!flag && !Settings.OutputUntranslatableText && !cacheKey.IsTemplated)
				{
					internalTranslationResult.SetCompleted(text);
				}
				else if (Settings.IsShutdown)
				{
					internalTranslationResult.SetErrorWithMessage("The plugin is shutdown.");
				}
				else if (endpoint == null)
				{
					internalTranslationResult.SetErrorWithMessage("No translator is selected.");
				}
				else if (endpoint.HasFailedDueToConsecutiveErrors)
				{
					internalTranslationResult.SetErrorWithMessage("The translation endpoint is shutdown.");
				}
				else if (!allowStartTranslateImmediate)
				{
					internalTranslationResult.SetErrorWithMessage("Could not resolve a translation at this time.");
				}
				else if (IsBelowMaxLength(text) || endpoint == TranslationManager.PassthroughEndpoint)
				{
					CreateTranslationJobFor(endpoint, null, cacheKey, internalTranslationResult, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, flag, allowFallback, untranslatedTextContext);
				}
				else if (Settings.OutputTooLongText)
				{
					CreateTranslationJobFor(TranslationManager.PassthroughEndpoint, null, cacheKey, internalTranslationResult, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, flag, allowFallback: false, untranslatedTextContext);
				}
				else
				{
					internalTranslationResult.SetErrorWithMessage("The provided text exceeds the maximum length.");
				}
				return internalTranslationResult;
			}
			internalTranslationResult.SetErrorWithMessage("The provided text (" + text + ") cannot be translated.");
		}
		else if (endpoint == null)
		{
			internalTranslationResult.SetErrorWithMessage("No translator is selected.");
		}
		else
		{
			if (!text.IsNullOrWhiteSpace() && endpoint.IsTranslatable(text))
			{
				UntranslatedText cacheKey2 = GetCacheKey(text, isFromSpammingComponent: false);
				if (cacheKey2.IsTemplated && !endpoint.IsTranslatable(cacheKey2.TemplatedOriginal_Text))
				{
					internalTranslationResult.SetErrorWithMessage("This text is already considered a translation for something else.");
					return internalTranslationResult;
				}
				if (cacheKey2.IsOnlyTemplate)
				{
					string completed2 = cacheKey2.Untemplate(cacheKey2.TemplatedOriginal_Text);
					internalTranslationResult.SetCompleted(completed2);
					return internalTranslationResult;
				}
				if (endpoint.TryGetTranslation(cacheKey2, out var value2))
				{
					internalTranslationResult.SetCompleted(cacheKey2.Untemplate(value2));
					return internalTranslationResult;
				}
				if (context.GetLevelsOfRecursion() < Settings.MaxTextParserRecursion)
				{
					ParserResult parserResult2 = UnityTextParsers.RegexSplittingTextParser.Parse(text, -1, TextCache);
					if (parserResult2 != null)
					{
						value2 = TranslateByParserResult(endpoint, parserResult2, -1, internalTranslationResult, allowStartTranslateImmediate, internalTranslationResult.IsGlobal, allowFallback, context);
						if (value2 != null)
						{
							internalTranslationResult.SetCompleted(value2);
						}
						return internalTranslationResult;
					}
					if (!context.HasBeenParsedBy(ParserResultOrigin.RichTextParser))
					{
						parserResult2 = UnityTextParsers.RichTextParser.Parse(text, -1);
						if (parserResult2 != null)
						{
							value2 = TranslateByParserResult(endpoint, parserResult2, -1, internalTranslationResult, allowStartTranslateImmediate, internalTranslationResult.IsGlobal, allowFallback, context);
							if (value2 != null)
							{
								internalTranslationResult.SetCompleted(value2);
							}
							return internalTranslationResult;
						}
					}
				}
				else if (Settings.MaxTextParserRecursion != 1)
				{
					XuaLogger.AutoTranslator.Warn("Attempted to exceed maximum allowed levels of text parsing recursion!");
				}
				bool flag2 = LanguageHelper.IsTranslatable(cacheKey2.TemplatedOriginal_Text);
				if (!flag2 && !Settings.OutputUntranslatableText && !cacheKey2.IsTemplated)
				{
					internalTranslationResult.SetCompleted(text);
				}
				else if (Settings.IsShutdown)
				{
					internalTranslationResult.SetErrorWithMessage("The plugin is shutdown.");
				}
				else if (endpoint.HasFailedDueToConsecutiveErrors)
				{
					internalTranslationResult.SetErrorWithMessage("The translation endpoint is shutdown.");
				}
				else if (!allowStartTranslateImmediate)
				{
					internalTranslationResult.SetErrorWithMessage("Could not resolve a translation at this time.");
				}
				else if (IsBelowMaxLength(text) || endpoint == TranslationManager.PassthroughEndpoint)
				{
					CreateTranslationJobFor(endpoint, null, cacheKey2, internalTranslationResult, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, flag2, allowFallback, untranslatedTextContext);
				}
				else if (Settings.OutputTooLongText)
				{
					CreateTranslationJobFor(TranslationManager.PassthroughEndpoint, null, cacheKey2, internalTranslationResult, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, flag2, allowFallback: false, untranslatedTextContext);
				}
				else
				{
					internalTranslationResult.SetErrorWithMessage("The provided text exceeds the maximum length.");
				}
				return internalTranslationResult;
			}
			internalTranslationResult.SetErrorWithMessage("The provided text (" + text + ") cannot be translated.");
		}
		return internalTranslationResult;
	}

	private string TranslateByParserResult(TranslationEndpointManager endpoint, ParserResult result, int scope, InternalTranslationResult translationResult, bool allowStartTranslateImmediate, bool isGlobal, bool allowFallback, ParserTranslationContext parentContext)
	{
		bool allowPartial = endpoint == null && result.AllowPartialTranslation;
		ParserTranslationContext context = new ParserTranslationContext(null, endpoint, translationResult, result, parentContext);
		if (isGlobal)
		{
			return result.GetTranslationFromParts(delegate(UntranslatedTextInfo untranslatedTextInfoPart)
			{
				string untranslatedText = untranslatedTextInfoPart.UntranslatedText;
				if (untranslatedText.IsNullOrWhiteSpace() || !TextCache.IsTranslatable(untranslatedText, isToken: true, scope))
				{
					return untranslatedText ?? string.Empty;
				}
				UntranslatedText untranslatedText2 = new UntranslatedText(untranslatedText, isFromSpammingComponent: false, removeInternalWhitespace: false, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
				if (!TextCache.IsTranslatable(untranslatedText2.TemplatedOriginal_Text, isToken: true, scope))
				{
					return untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty;
				}
				if (TextCache.TryGetTranslation(untranslatedText2, allowRegex: false, allowToken: true, scope, out var value))
				{
					return untranslatedText2.Untemplate(value) ?? string.Empty;
				}
				if ((!Settings.OutputUntranslatableText && !LanguageHelper.IsTranslatable(untranslatedText2.TemplatedOriginal_Text) && !untranslatedText2.IsTemplated) || untranslatedText2.IsOnlyTemplate)
				{
					return untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty;
				}
				InternalTranslationResult internalTranslationResult = Translate(untranslatedText, scope, endpoint, context, null, isGlobal, allowStartTranslateImmediate, allowFallback, untranslatedTextInfoPart);
				if (internalTranslationResult.TranslatedText != null)
				{
					return untranslatedText2.Untemplate(internalTranslationResult.TranslatedText) ?? string.Empty;
				}
				return allowPartial ? (untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty) : null;
			});
		}
		if (endpoint == null)
		{
			return null;
		}
		return result.GetTranslationFromParts(delegate(UntranslatedTextInfo untranslatedTextInfoPart)
		{
			string untranslatedText = untranslatedTextInfoPart.UntranslatedText;
			if (untranslatedText.IsNullOrWhiteSpace() || !endpoint.IsTranslatable(untranslatedText))
			{
				return untranslatedText ?? string.Empty;
			}
			UntranslatedText untranslatedText2 = new UntranslatedText(untranslatedText, isFromSpammingComponent: false, removeInternalWhitespace: false, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
			if (!endpoint.IsTranslatable(untranslatedText2.TemplatedOriginal_Text))
			{
				return untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty;
			}
			if (endpoint.TryGetTranslation(untranslatedText2, out var value))
			{
				return untranslatedText2.Untemplate(value) ?? string.Empty;
			}
			if ((!Settings.OutputUntranslatableText && !LanguageHelper.IsTranslatable(untranslatedText2.TemplatedOriginal_Text) && !untranslatedText2.IsTemplated) || untranslatedText2.IsOnlyTemplate)
			{
				return untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty;
			}
			InternalTranslationResult internalTranslationResult = Translate(untranslatedText, scope, endpoint, context, null, isGlobal, allowStartTranslateImmediate, allowFallback, untranslatedTextInfoPart);
			if (internalTranslationResult.TranslatedText != null)
			{
				return untranslatedText2.Untemplate(internalTranslationResult.TranslatedText) ?? string.Empty;
			}
			return allowPartial ? (untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty) : null;
		});
	}

	private bool CheckAndFixRedirected(object ui, string text, TextTranslationInfo info)
	{
		if (Settings.RedirectedResourceDetectionStrategy == RedirectedResourceDetection.None || !LanguageHelper.HasRedirectedTexts)
		{
			return false;
		}
		if (info != null && _textHooksEnabled && !info.IsCurrentlySettingText)
		{
			if (text.IsRedirected())
			{
				info.IsCurrentlySettingText = true;
				_textHooksEnabled = false;
				try
				{
					string text2 = text.FixRedirected();
					info.RedirectedTranslations.Add(text2);
					ui.SetText(text2, info);
					return true;
				}
				finally
				{
					_textHooksEnabled = true;
					info.IsCurrentlySettingText = false;
				}
			}
			if (info.RedirectedTranslations.Contains(text))
			{
				return true;
			}
		}
		return false;
	}

	private string TranslateOrQueueWebJobImmediate(object ui, string text, int scope, TextTranslationInfo info, bool allowStabilizationOnTextComponent, bool ignoreComponentState, bool allowStartTranslationImmediate, bool allowStartTranslationLater, IReadOnlyTextTranslationCache tc, UntranslatedTextInfo untranslatedTextContext, ParserTranslationContext context)
	{
		if (info != null && info.IsCurrentlySettingText)
		{
			return null;
		}
		text = text ?? ui.GetText(info);
		if (info != null && info.IsTranslated && text == info.TranslatedText)
		{
			return null;
		}
		bool flag = false;
		if (info != null)
		{
			info.Reset(text);
			flag = info.ShouldIgnore;
		}
		if (scope == -1 && context == null)
		{
			scope = TranslationScopeHelper.GetScope(ui);
		}
		if (text.IsNullOrWhiteSpace())
		{
			return null;
		}
		if (context == null && CheckAndFixRedirected(ui, text, info))
		{
			return null;
		}
		if (!flag && (ignoreComponentState || ui.IsComponentActive()))
		{
			if (context == null && info != null)
			{
				ComponentTranslationContext componentTranslationContext = InvokeOnTranslatingCallback(ui, text, info);
				if (componentTranslationContext != null)
				{
					switch (componentTranslationContext.Behaviour)
					{
					case ComponentTranslationBehaviour.OverrideTranslatedText:
					{
						string overriddenTranslatedText = componentTranslationContext.OverriddenTranslatedText;
						if (overriddenTranslatedText != null)
						{
							SetTranslatedText(ui, overriddenTranslatedText, text, info);
						}
						return overriddenTranslatedText;
					}
					case ComponentTranslationBehaviour.IgnoreComponent:
						return null;
					}
				}
			}
			if (!tc.IsTranslatable(text, isToken: false, scope))
			{
				return null;
			}
			bool flag2 = ui.IsSpammingComponent();
			if (flag2 && !IsBelowMaxLength(text))
			{
				return null;
			}
			UntranslatedText cacheKey = GetCacheKey(text, flag2);
			if ((cacheKey.IsTemplated && !tc.IsTranslatable(cacheKey.TemplatedOriginal_Text, isToken: false, scope)) || cacheKey.IsOnlyTemplate)
			{
				string text2 = cacheKey.Untemplate(cacheKey.TemplatedOriginal_Text);
				if (context == null)
				{
					SetTranslatedText(ui, text2, text, info);
				}
				return text2;
			}
			if (tc.TryGetTranslation(cacheKey, !flag2, allowToken: false, scope, out var value))
			{
				string text3 = cacheKey.Untemplate(value);
				if (context == null)
				{
					bool flag3 = tc.IsPartial(cacheKey.TemplatedOriginal_Text, scope);
					SetTranslatedText(ui, text3, (!flag3) ? text : null, info);
				}
				return text3;
			}
			bool flag4 = LanguageHelper.IsTranslatable(cacheKey.TemplatedOriginal_Text);
			if (!flag2)
			{
				if (context.GetLevelsOfRecursion() < Settings.MaxTextParserRecursion)
				{
					if (UnityTextParsers.GameLogTextParser.CanApply(ui) && context == null)
					{
						ParserResult parserResult = UnityTextParsers.GameLogTextParser.Parse(text, scope, tc);
						if (parserResult != null)
						{
							value = TranslateOrQueueWebJobImmediateByParserResult(ui, parserResult, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, flag4 || Settings.OutputUntranslatableText, tc, context);
							if (value != null)
							{
								if (context == null)
								{
									SetTranslatedText(ui, value, null, info);
								}
								return value;
							}
							if (context != null)
							{
								return null;
							}
						}
					}
					if (UnityTextParsers.RegexSplittingTextParser.CanApply(ui))
					{
						ParserResult parserResult2 = UnityTextParsers.RegexSplittingTextParser.Parse(text, scope, tc);
						if (parserResult2 != null)
						{
							value = TranslateOrQueueWebJobImmediateByParserResult(ui, parserResult2, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, flag4 || Settings.OutputUntranslatableText, tc, context);
							if (value != null)
							{
								if (context == null)
								{
									SetTranslatedText(ui, value, text, info);
								}
								return value;
							}
							if (context != null)
							{
								return null;
							}
						}
					}
					if (UnityTextParsers.RichTextParser.CanApply(ui) && !context.HasBeenParsedBy(ParserResultOrigin.RichTextParser))
					{
						ParserResult parserResult3 = UnityTextParsers.RichTextParser.Parse(text, scope);
						if (parserResult3 != null)
						{
							value = TranslateOrQueueWebJobImmediateByParserResult(ui, parserResult3, scope, allowStartTranslationImmediate, allowStartTranslationLater && !allowStabilizationOnTextComponent, flag4 || Settings.OutputUntranslatableText, tc, context);
							if (value != null)
							{
								if (context == null)
								{
									SetTranslatedText(ui, value, text, info);
								}
								return value;
							}
							if (context != null)
							{
								return null;
							}
						}
					}
				}
				else if (Settings.MaxTextParserRecursion != 1)
				{
					XuaLogger.AutoTranslator.Warn("Attempted to exceed maximum allowed levels of text parsing recursion!");
				}
			}
			if (!flag4 && !Settings.OutputUntranslatableText && (!cacheKey.IsTemplated || flag2))
			{
				if (_isInTranslatedMode && !flag2)
				{
					TranslationHelper.DisplayTranslationInfo(text, null);
				}
				return text;
			}
			TranslateByEndpoint(ui, text, cacheKey, flag4, flag2, scope, info, allowStabilizationOnTextComponent, allowStartTranslationImmediate, allowStartTranslationLater, tc, untranslatedTextContext, context);
		}
		return null;
	}

	private void TranslateByEndpoint(object ui, string text, UntranslatedText textKey, bool isTranslatable, bool isSpammer, int scope, TextTranslationInfo info, bool allowStabilizationOnTextComponent, bool allowStartTranslationImmediate, bool allowStartTranslationLater, IReadOnlyTextTranslationCache tc, UntranslatedTextInfo untranslatedTextContext, ParserTranslationContext context)
	{
		TranslationEndpointManager endpoint = GetTranslationEndpoint(context, allowFallback: true);
		if (allowStartTranslationImmediate)
		{
			if (endpoint != null && !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors)
			{
				if (IsBelowMaxLength(text) || endpoint == TranslationManager.PassthroughEndpoint)
				{
					CreateTranslationJobFor(endpoint, ui, textKey, null, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, isTranslatable, allowFallback: true, untranslatedTextContext);
				}
				else if (Settings.OutputTooLongText)
				{
					CreateTranslationJobFor(TranslationManager.PassthroughEndpoint, ui, textKey, null, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, isTranslatable, allowFallback: false, untranslatedTextContext);
				}
			}
			return;
		}
		if (allowStabilizationOnTextComponent && allowStartTranslationLater && context == null)
		{
			info.IsStabilizingText = true;
			try
			{
				string translation;
				Action<string> action = delegate(string stabilizedText)
				{
					info.IsStabilizingText = false;
					text = stabilizedText;
					TextTranslationInfo textTranslationInfo = info;
					if (textTranslationInfo == null || !textTranslationInfo.IsTranslated)
					{
						info?.Reset(text);
						if (!stabilizedText.IsNullOrWhiteSpace() && (context != null || !CheckAndFixRedirected(ui, text, info)) && tc.IsTranslatable(stabilizedText, isToken: false, scope))
						{
							UntranslatedText cacheKey = GetCacheKey(stabilizedText, isFromSpammingComponent: false);
							if ((cacheKey.IsTemplated && !tc.IsTranslatable(cacheKey.TemplatedOriginal_Text, isToken: false, scope)) || cacheKey.IsOnlyTemplate)
							{
								string translatedText = cacheKey.Untemplate(cacheKey.TemplatedOriginal_Text);
								SetTranslatedText(ui, translatedText, text, info);
							}
							else if (tc.TryGetTranslation(cacheKey, allowRegex: true, allowToken: false, scope, out translation))
							{
								bool flag = tc.IsPartial(cacheKey.TemplatedOriginal_Text, scope);
								SetTranslatedText(ui, cacheKey.Untemplate(translation), (!flag) ? text : null, info);
							}
							else
							{
								bool flag2 = LanguageHelper.IsTranslatable(cacheKey.TemplatedOriginal_Text);
								if (UnityTextParsers.GameLogTextParser.CanApply(ui) && context == null)
								{
									ParserResult parserResult = UnityTextParsers.GameLogTextParser.Parse(stabilizedText, scope, tc);
									if (parserResult != null)
									{
										string text2 = TranslateOrQueueWebJobImmediateByParserResult(ui, parserResult, scope, allowStartTranslationImmediate: true, allowStartTranslationLater: false, flag2 || Settings.OutputUntranslatableText, tc, context);
										if (text2 != null && context == null)
										{
											SetTranslatedText(ui, text2, null, info);
										}
										return;
									}
								}
								if (UnityTextParsers.RegexSplittingTextParser.CanApply(ui))
								{
									ParserResult parserResult2 = UnityTextParsers.RegexSplittingTextParser.Parse(stabilizedText, scope, tc);
									if (parserResult2 != null)
									{
										string text3 = TranslateOrQueueWebJobImmediateByParserResult(ui, parserResult2, scope, allowStartTranslationImmediate: true, allowStartTranslationLater: false, flag2 || Settings.OutputUntranslatableText, tc, context);
										if (text3 != null && context == null)
										{
											SetTranslatedText(ui, text3, text, info);
										}
										return;
									}
								}
								if (UnityTextParsers.RichTextParser.CanApply(ui) && !context.HasBeenParsedBy(ParserResultOrigin.RichTextParser))
								{
									ParserResult parserResult3 = UnityTextParsers.RichTextParser.Parse(stabilizedText, scope);
									if (parserResult3 != null)
									{
										string text4 = TranslateOrQueueWebJobImmediateByParserResult(ui, parserResult3, scope, allowStartTranslationImmediate: true, allowStartTranslationLater: false, flag2 || Settings.OutputUntranslatableText, tc, context);
										if (text4 != null && context == null)
										{
											SetTranslatedText(ui, text4, text, info);
										}
										return;
									}
								}
								if (!flag2 && !Settings.OutputUntranslatableText && !cacheKey.IsTemplated)
								{
									if (_isInTranslatedMode && !isSpammer)
									{
										TranslationHelper.DisplayTranslationInfo(text, null);
									}
								}
								else if (endpoint != null && !Settings.IsShutdown && !endpoint.HasFailedDueToConsecutiveErrors)
								{
									if (IsBelowMaxLength(text) || endpoint == TranslationManager.PassthroughEndpoint)
									{
										CreateTranslationJobFor(endpoint, ui, cacheKey, null, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, flag2, allowFallback: true, untranslatedTextContext);
									}
									else if (Settings.OutputTooLongText)
									{
										CreateTranslationJobFor(TranslationManager.PassthroughEndpoint, ui, cacheKey, null, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, flag2, allowFallback: false, untranslatedTextContext);
									}
								}
							}
						}
					}
				};
				if (endpoint?.Endpoint is PassthroughTranslateEndpoint)
				{
					action(text);
					return;
				}
				float delay = endpoint?.TranslationDelay ?? Settings.DefaultTranslationDelay;
				int maxTries = endpoint?.MaxRetries ?? Settings.DefaultMaxRetries;
				CoroutineHelper.Start(WaitForTextStablization(ui, info, delay, maxTries, 0, action, delegate
				{
					info.IsStabilizingText = false;
				}));
				return;
			}
			catch (Exception)
			{
				info.IsStabilizingText = false;
				return;
			}
		}
		if (!allowStartTranslationLater)
		{
			return;
		}
		float delay2 = endpoint?.TranslationDelay ?? Settings.DefaultTranslationDelay;
		CoroutineHelper.Start(WaitForTextStablization(textKey, delay2, delegate
		{
			if (!tc.TryGetTranslation(textKey, allowRegex: true, allowToken: false, scope, out var _))
			{
				TranslationEndpointManager translationEndpoint = GetTranslationEndpoint(context, allowFallback: true);
				if (translationEndpoint != null && !Settings.IsShutdown && !translationEndpoint.HasFailedDueToConsecutiveErrors)
				{
					if (IsBelowMaxLength(text) || translationEndpoint == TranslationManager.PassthroughEndpoint)
					{
						CreateTranslationJobFor(translationEndpoint, ui, textKey, null, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, isTranslatable, allowFallback: true, untranslatedTextContext);
					}
					else if (Settings.OutputTooLongText)
					{
						CreateTranslationJobFor(TranslationManager.PassthroughEndpoint, ui, textKey, null, context, checkOtherEndpoints: true, checkSpam: true, saveResultGlobally: true, isTranslatable, allowFallback: false, untranslatedTextContext);
					}
				}
			}
		}));
	}

	private TranslationEndpointManager GetTranslationEndpoint(ParserTranslationContext context, bool allowFallback)
	{
		TranslationEndpointManager translationEndpointManager = context?.Endpoint ?? TranslationManager.CurrentEndpoint;
		if (allowFallback && translationEndpointManager != null && translationEndpointManager.HasFailedDueToConsecutiveErrors && TranslationManager.IsFallbackAvailableFor(translationEndpointManager))
		{
			XuaLogger.AutoTranslator.Warn("Falling back to fallback translator in order to perform translation.");
			translationEndpointManager = TranslationManager.FallbackEndpoint;
		}
		return translationEndpointManager;
	}

	private string TranslateOrQueueWebJobImmediateByParserResult(object ui, ParserResult result, int scope, bool allowStartTranslationImmediate, bool allowStartTranslationLater, bool allowImmediateCaching, IReadOnlyTextTranslationCache tc, ParserTranslationContext parentContext)
	{
		bool allowPartial = TranslationManager.CurrentEndpoint == null && result.AllowPartialTranslation;
		ParserTranslationContext context = new ParserTranslationContext(ui, TranslationManager.CurrentEndpoint, null, result, parentContext);
		string translationFromParts = result.GetTranslationFromParts(delegate(UntranslatedTextInfo untranslatedTextInfoPart)
		{
			string untranslatedText = untranslatedTextInfoPart.UntranslatedText;
			if (untranslatedText.IsNullOrWhiteSpace() || !tc.IsTranslatable(untranslatedText, isToken: true, scope))
			{
				return untranslatedText ?? string.Empty;
			}
			UntranslatedText untranslatedText2 = new UntranslatedText(untranslatedText, isFromSpammingComponent: false, removeInternalWhitespace: false, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
			if (!tc.IsTranslatable(untranslatedText2.TemplatedOriginal_Text, isToken: true, scope))
			{
				return untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty;
			}
			if (tc.TryGetTranslation(untranslatedText2, allowRegex: false, allowToken: true, scope, out var value))
			{
				return untranslatedText2.Untemplate(value) ?? string.Empty;
			}
			if ((!Settings.OutputUntranslatableText && !LanguageHelper.IsTranslatable(untranslatedText2.TemplatedOriginal_Text) && !untranslatedText2.IsTemplated) || untranslatedText2.IsOnlyTemplate)
			{
				return untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty;
			}
			value = TranslateOrQueueWebJobImmediate(ui, untranslatedText, scope, null, allowStabilizationOnTextComponent: false, ignoreComponentState: true, allowStartTranslationImmediate, allowStartTranslationLater, tc, untranslatedTextInfoPart, context);
			if (value != null)
			{
				return untranslatedText2.Untemplate(value) ?? string.Empty;
			}
			return allowPartial ? (untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text) ?? string.Empty) : null;
		});
		try
		{
			if (Settings.CacheParsedTranslations && allowImmediateCaching && parentContext == null && translationFromParts != null && context.CachedCombinedResult())
			{
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Debug("Parsed translation cached: '" + context.Result.OriginalText + "' => '" + translationFromParts + "'");
				}
				TextCache.AddTranslationToCache(context.Result.OriginalText, translationFromParts, persistToDisk: false, TranslationType.Full, scope);
				context.Endpoint.AddTranslationToCache(context.Result.OriginalText, translationFromParts);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while attempting to cache a parsed translation.");
		}
		return translationFromParts;
	}

	private IEnumerator WaitForTextStablization(object ui, TextTranslationInfo info, float delay, int maxTries, int currentTries, Action<string> onTextStabilized, Action onMaxTriesExceeded)
	{
		yield return null;
		bool succeeded = false;
		while (currentTries < maxTries)
		{
			string beforeText = ui.GetText(info);
			object obj = CoroutineHelper.CreateWaitForSecondsRealtime(delay);
			if (obj != null)
			{
				yield return obj;
			}
			else
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				float end = realtimeSinceStartup + delay;
				while (Time.realtimeSinceStartup < end)
				{
					yield return null;
				}
			}
			string text = ui.GetText(info);
			if (beforeText == text)
			{
				onTextStabilized(text);
				succeeded = true;
				break;
			}
			currentTries++;
		}
		if (!succeeded)
		{
			onMaxTriesExceeded();
		}
	}

	private IEnumerator WaitForTextStablization(UntranslatedText textKey, float delay, Action onTextStabilized, Action onFailed = null)
	{
		string text = textKey.TemplatedOriginal_Text_FullyTrimmed;
		if (_immediatelyTranslating.Contains(text))
		{
			yield break;
		}
		_immediatelyTranslating.Add(text);
		try
		{
			object obj = CoroutineHelper.CreateWaitForSecondsRealtime(delay);
			if (obj != null)
			{
				yield return obj;
			}
			else
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				float end = realtimeSinceStartup + delay;
				while (Time.realtimeSinceStartup < end)
				{
					yield return null;
				}
			}
			bool flag = true;
			foreach (string item in _immediatelyTranslating)
			{
				if (text != item && text.RemindsOf(item))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				onTextStabilized();
			}
			else
			{
				onFailed?.Invoke();
			}
		}
		finally
		{
			_immediatelyTranslating.Remove(text);
		}
	}

	private void Awake()
	{
		if (!_initialized)
		{
			_initialized = true;
			try
			{
				Initialize();
				ManualHook();
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An unexpected error occurred during plugin initialization.");
			}
		}
	}

	private TextTranslationCache GetTextCacheFor(string assemblyName)
	{
		if (!PluginTextCaches.TryGetValue(assemblyName, out var value))
		{
			value = new TextTranslationCache(assemblyName);
			PluginTextCaches[assemblyName] = value;
		}
		return value;
	}

	void ITranslationRegistry.RegisterPluginSpecificTranslations(Assembly assembly, StreamTranslationPackage package)
	{
		TextTranslationCache textCacheFor = GetTextCacheFor(assembly.GetName().Name);
		textCacheFor.RegisterPackage(package);
		textCacheFor.LoadTranslationFiles();
		HooksSetup.InstallComponentBasedPluginTranslationHooks();
		HooksSetup.InstallIMGUIBasedPluginTranslationHooks(assembly, final: true);
	}

	void ITranslationRegistry.RegisterPluginSpecificTranslations(Assembly assembly, KeyValuePairTranslationPackage package)
	{
		TextTranslationCache textCacheFor = GetTextCacheFor(assembly.GetName().Name);
		textCacheFor.RegisterPackage(package);
		textCacheFor.LoadTranslationFiles();
		HooksSetup.InstallComponentBasedPluginTranslationHooks();
		HooksSetup.InstallIMGUIBasedPluginTranslationHooks(assembly, final: true);
	}

	void ITranslationRegistry.EnablePluginTranslationFallback(Assembly assembly)
	{
		TextTranslationCache textCacheFor = GetTextCacheFor(assembly.GetName().Name);
		textCacheFor.AllowFallback = true;
		textCacheFor.DefaultAllowFallback = true;
		HooksSetup.InstallComponentBasedPluginTranslationHooks();
		HooksSetup.InstallIMGUIBasedPluginTranslationHooks(assembly, final: true);
	}

	private IEnumerator HookLoadedPlugins()
	{
		yield return null;
		if (PluginTextCaches.Count == 0)
		{
			XuaLogger.AutoTranslator.Info("Skipping plugin scan because no plugin-specific translations has been registered.");
			yield break;
		}
		XuaLogger.AutoTranslator.Info("Scanning for plugins to hook for translations...");
		string value = Application.dataPath.UseCorrectDirectorySeparators();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			try
			{
				if (!assembly.FullName.StartsWith("XUnity") && !assembly.ManifestModule.GetType().FullName.Contains("Emit") && !assembly.Location.UseCorrectDirectorySeparators().StartsWith(value, StringComparison.OrdinalIgnoreCase) && PluginTextCaches.TryGetValue(assembly.GetName().Name, out var _))
				{
					HooksSetup.InstallIMGUIBasedPluginTranslationHooks(assembly, final: false);
				}
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Warn(e, "An error occurred while scanning assembly: " + assembly.FullName);
			}
		}
	}

	public void Start()
	{
		if (!_started)
		{
			_started = true;
			Awake();
			try
			{
				CoroutineHelper.Start(HookLoadedPlugins());
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An unexpected error occurred during plugin start.");
			}
		}
	}

	private void HandleInputSafe()
	{
		if (_inputSupported)
		{
			try
			{
				HandleInput();
			}
			catch (Exception e)
			{
				_inputSupported = false;
				XuaLogger.AutoTranslator.Warn(e, "Input API is not available!");
			}
		}
	}

	private void HandleInput()
	{
		if (!Input.GetKey((KeyCode)308) && !Input.GetKey((KeyCode)307))
		{
			return;
		}
		bool key = Input.GetKey((KeyCode)306);
		if (Input.GetKeyDown((KeyCode)116))
		{
			ToggleTranslation();
		}
		else if (Input.GetKeyDown((KeyCode)102))
		{
			ToggleFont();
		}
		else if (Input.GetKeyDown((KeyCode)114))
		{
			ReloadTranslations();
		}
		else if (Input.GetKeyDown((KeyCode)117))
		{
			ManualHook();
		}
		else if (Input.GetKeyDown((KeyCode)113))
		{
			RebootPlugin();
		}
		else if (Input.GetKeyDown((KeyCode)48) || Input.GetKeyDown((KeyCode)256))
		{
			if (MainWindow != null)
			{
				MainWindow.IsShown = !MainWindow.IsShown;
			}
		}
		else if (Input.GetKeyDown((KeyCode)49) || Input.GetKeyDown((KeyCode)257))
		{
			ToggleTranslationAggregator();
		}
		else if (key)
		{
			if (Input.GetKeyDown((KeyCode)265))
			{
				Settings.SimulateError = !Settings.SimulateError;
			}
			else if (Input.GetKeyDown((KeyCode)264))
			{
				Settings.SimulateDelayedError = !Settings.SimulateDelayedError;
			}
			else if (Input.GetKeyDown((KeyCode)263))
			{
				PrintSceneInformation();
			}
			else if (Input.GetKeyDown((KeyCode)262))
			{
				PrintObjects();
			}
		}
	}

	public void Update()
	{
		try
		{
			int frameCount = Time.frameCount;
			TranslationManager.Update();
			if (frameCount % 36000 == 0 && CachedKeys.Count > 0)
			{
				CachedKeys.Clear();
			}
			if (UnityFeatures.SupportsClipboard)
			{
				CopyToClipboard();
			}
			if (!Settings.IsShutdown)
			{
				EnableAutoTranslator();
				SpamChecker.Update();
				UpdateSpriteRenderers();
				IncrementBatchOperations();
				KickoffTranslations();
				TranslationAggregatorWindow?.Update();
			}
			if (_translationReloadRequest)
			{
				_translationReloadRequest = false;
				ReloadTranslations();
			}
			if (frameCount % 100 == 0 && TranslationManager.OngoingTranslations == 0 && TranslationManager.UnstartedTranslations == 0)
			{
				ConnectionTrackingWebClient.CheckServicePoints();
			}
			HandleInputSafe();
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred in Update callback. ");
		}
	}

	private void PrintSceneInformation()
	{
		SceneLoadInformation sceneLoadInformation = new SceneLoadInformation();
		XuaLogger.AutoTranslator.Info("Active Scene: " + sceneLoadInformation.ActiveScene.Name + " (" + sceneLoadInformation.ActiveScene.Id + ")");
		XuaLogger.AutoTranslator.Info("Loaded Scenes:");
		for (int i = 0; i < sceneLoadInformation.LoadedScenes.Count; i++)
		{
			SceneInformation sceneInformation = sceneLoadInformation.LoadedScenes[i];
			XuaLogger.AutoTranslator.Info(i + ": " + sceneInformation.Name + " (" + sceneInformation.Id + ")");
		}
	}

	public void OnGUI()
	{
		InitializeGUI();
		try
		{
			DisableAutoTranslator();
			if (MainWindow != null)
			{
				try
				{
					if (MainWindow.IsShown)
					{
						MainWindow.OnGUI();
					}
				}
				catch (Exception e)
				{
					XuaLogger.AutoTranslator.Error(e, "An error occurred in XUnity.AutoTranslator UI. Disabling the UI.");
					MainWindow = null;
				}
			}
			if (TranslationAggregatorWindow != null)
			{
				try
				{
					if (TranslationAggregatorWindow.IsShown)
					{
						TranslationAggregatorWindow.OnGUI();
					}
				}
				catch (Exception e2)
				{
					XuaLogger.AutoTranslator.Error(e2, "An error occurred in Translation Aggregator UI. Disabling the UI.");
					TranslationAggregatorWindow = null;
				}
			}
			if (TranslationAggregatorOptionsWindow == null)
			{
				return;
			}
			try
			{
				if (TranslationAggregatorOptionsWindow.IsShown)
				{
					TranslationAggregatorOptionsWindow.OnGUI();
				}
			}
			catch (Exception e3)
			{
				XuaLogger.AutoTranslator.Error(e3, "An error occurred in Translation Aggregator Options UI. Disabling the UI.");
				TranslationAggregatorOptionsWindow = null;
			}
		}
		finally
		{
			EnableAutoTranslator();
		}
	}

	private void RebootPlugin()
	{
		foreach (TranslationEndpointManager configuredEndpoint in TranslationManager.ConfiguredEndpoints)
		{
			configuredEndpoint.ConsecutiveErrors = 0;
		}
		XuaLogger.AutoTranslator.Info("Rebooted Auto Translator.");
	}

	private void KickoffTranslations()
	{
		TranslationManager.KickoffTranslations();
	}

	private void OnJobFailed(TranslationJob job)
	{
		foreach (KeyAnd<InternalTranslationResult> translationResult2 in job.TranslationResults)
		{
			translationResult2.Item.SetErrorWithMessage(job.ErrorMessage ?? "Unknown error.");
		}
		foreach (ParserTranslationContext context in job.Contexts)
		{
			InternalTranslationResult translationResult = context.TranslationResult;
			if (translationResult != null && context.Jobs.Any((TranslationJob x) => x.State == TranslationJobState.Failed))
			{
				translationResult.SetErrorWithMessage(job.ErrorMessage ?? "Unknown error.");
			}
		}
	}

	private void OnJobCompleted(TranslationJob job)
	{
		TranslationType translationType = job.TranslationType;
		bool shouldPersistTranslation = job.ShouldPersistTranslation;
		if (job.Key.IsTemplated && Settings.GenerateStaticSubstitutionTranslations)
		{
			bool flag = false;
			if (job.Key.IsFromSpammingComponent)
			{
				flag = true;
				if (job.SaveResultGlobally)
				{
					TextCache.AddTranslationToCache(job.Key.TemplatedOriginal_Text, job.TranslatedText, shouldPersistTranslation, translationType, -1);
				}
				job.Endpoint.AddTranslationToCache(job.Key.TemplatedOriginal_Text, job.TranslatedText);
			}
			foreach (KeyAnd<object> component in job.Components)
			{
				UntranslatedText key = component.Key;
				if (key.IsFromSpammingComponent)
				{
					if (!flag)
					{
						flag = true;
						if (job.SaveResultGlobally)
						{
							TextCache.AddTranslationToCache(job.Key.TemplatedOriginal_Text, job.TranslatedText, shouldPersistTranslation, translationType, -1);
						}
						job.Endpoint.AddTranslationToCache(job.Key.TemplatedOriginal_Text, job.TranslatedText);
					}
				}
				else
				{
					string key2 = key.Untemplate(key.TemplatedOriginal_Text);
					string value = key.Untemplate(job.TranslatedText);
					if (job.SaveResultGlobally)
					{
						TextCache.AddTranslationToCache(key2, value, shouldPersistTranslation, translationType, -1);
					}
					job.Endpoint.AddTranslationToCache(key2, value);
				}
			}
			foreach (KeyAnd<InternalTranslationResult> translationResult in job.TranslationResults)
			{
				UntranslatedText key3 = translationResult.Key;
				if (key3.IsFromSpammingComponent)
				{
					if (!flag)
					{
						flag = true;
						if (job.SaveResultGlobally)
						{
							TextCache.AddTranslationToCache(job.Key.TemplatedOriginal_Text, job.TranslatedText, shouldPersistTranslation, translationType, -1);
						}
						job.Endpoint.AddTranslationToCache(job.Key.TemplatedOriginal_Text, job.TranslatedText);
					}
				}
				else
				{
					string key4 = key3.Untemplate(key3.TemplatedOriginal_Text);
					string value2 = key3.Untemplate(job.TranslatedText);
					if (job.SaveResultGlobally)
					{
						TextCache.AddTranslationToCache(key4, value2, shouldPersistTranslation, translationType, -1);
					}
					job.Endpoint.AddTranslationToCache(key4, value2);
				}
			}
		}
		else
		{
			if (job.SaveResultGlobally)
			{
				TextCache.AddTranslationToCache(job.Key.TemplatedOriginal_Text, job.TranslatedText, shouldPersistTranslation, translationType, -1);
			}
			job.Endpoint.AddTranslationToCache(job.Key.TemplatedOriginal_Text, job.TranslatedText);
		}
		foreach (KeyAnd<InternalTranslationResult> translationResult2 in job.TranslationResults)
		{
			if (!string.IsNullOrEmpty(job.TranslatedText))
			{
				translationResult2.Item.SetCompleted(translationResult2.Key.Untemplate(job.TranslatedText));
			}
			else
			{
				translationResult2.Item.SetEmptyResponse();
			}
		}
		foreach (KeyAnd<object> component2 in job.Components)
		{
			object item = component2.Item;
			UntranslatedText key5 = component2.Key;
			try
			{
				TextTranslationInfo orCreateTextTranslationInfo = item.GetOrCreateTextTranslationInfo();
				if (item.GetText(orCreateTextTranslationInfo) == key5.Original_Text && !string.IsNullOrEmpty(job.TranslatedText))
				{
					SetTranslatedText(item, key5.Untemplate(job.TranslatedText), key5.Original_Text, orCreateTextTranslationInfo);
				}
			}
			catch (NullReferenceException)
			{
			}
		}
		foreach (ParserTranslationContext context in job.Contexts)
		{
			ParserTranslationContext ancestorContext = context.GetAncestorContext();
			if (!ancestorContext.HasAllJobsCompleted())
			{
				continue;
			}
			try
			{
				TextTranslationInfo orCreateTextTranslationInfo2 = ancestorContext.Component.GetOrCreateTextTranslationInfo();
				string text = ancestorContext.Component.GetText(orCreateTextTranslationInfo2);
				ParserResult result = ancestorContext.Result;
				string text2 = ((ancestorContext.TranslationResult != null) ? TranslateByParserResult(ancestorContext.Endpoint, result, -1, null, allowStartTranslateImmediate: false, ancestorContext.TranslationResult.IsGlobal, allowFallback: false, null) : TranslateOrQueueWebJobImmediateByParserResult(ancestorContext.Component, result, -1, allowStartTranslationImmediate: false, allowStartTranslationLater: false, allowImmediateCaching: false, TextCache, null));
				if (!string.IsNullOrEmpty(text2))
				{
					if (ancestorContext.CachedCombinedResult())
					{
						if (job.SaveResultGlobally)
						{
							TextCache.AddTranslationToCache(ancestorContext.Result.OriginalText, text2, ancestorContext.PersistCombinedResult(), TranslationType.Full, -1);
						}
						job.Endpoint.AddTranslationToCache(ancestorContext.Result.OriginalText, text2);
					}
					if (text == result.OriginalText)
					{
						SetTranslatedText(ancestorContext.Component, text2, ancestorContext.Result.OriginalText, orCreateTextTranslationInfo2);
					}
					if (ancestorContext.TranslationResult != null)
					{
						ancestorContext.TranslationResult.SetCompleted(text2);
					}
				}
				else if (ancestorContext.TranslationResult != null)
				{
					ancestorContext.TranslationResult.SetEmptyResponse();
				}
			}
			catch (NullReferenceException)
			{
			}
		}
		Settings.TranslationCount++;
		if (!Settings.IsShutdown && Settings.TranslationCount > Settings.MaxTranslationsBeforeShutdown)
		{
			Settings.IsShutdown = true;
			XuaLogger.AutoTranslator.Error($"Maximum translations ({Settings.MaxTranslationsBeforeShutdown}) per session reached. Shutting plugin down.");
			TranslationManager.ClearAllJobs();
		}
	}

	private UntranslatedText GetCacheKey(string originalText, bool isFromSpammingComponent)
	{
		if (isFromSpammingComponent && CachedKeys.Count < Settings.MaxImguiKeyCacheCount)
		{
			if (!CachedKeys.TryGetValue(originalText, out var value))
			{
				value = new UntranslatedText(originalText, isFromSpammingComponent, !isFromSpammingComponent, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
				CachedKeys.Add(originalText, value);
			}
			return value;
		}
		return new UntranslatedText(originalText, isFromSpammingComponent, !isFromSpammingComponent, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
	}

	private void ReloadTranslations()
	{
		try
		{
			TextCache.PruneMainTranslationFile();
			LoadTranslations(reload: true);
			TextureReloadContext context = new TextureReloadContext();
			foreach (KeyValuePair<object, object> allRegisteredObject in ExtensionDataHelper.GetAllRegisteredObjects())
			{
				object key = allRegisteredObject.Key;
				try
				{
					TextTranslationInfo textTranslationInfo = allRegisteredObject.Value as TextTranslationInfo;
					if (textTranslationInfo.GetIsKnownTextComponent() && key.IsComponentActive())
					{
						int scope = TranslationScopeHelper.GetScope(key);
						if (textTranslationInfo != null && !textTranslationInfo.OriginalText.IsNullOrWhiteSpace())
						{
							bool flag = false;
							string originalText = textTranslationInfo.OriginalText;
							try
							{
								UntranslatedText cacheKey = GetCacheKey(originalText, isFromSpammingComponent: false);
								if (TextCache.TryGetTranslation(cacheKey, allowRegex: true, allowToken: false, scope, out var value))
								{
									textTranslationInfo.UnresizeUI(key);
									SetTranslatedText(allRegisteredObject.Key, cacheKey.Untemplate(value), null, textTranslationInfo);
									flag = true;
									continue;
								}
								if (!UnityTextParsers.GameLogTextParser.CanApply(key))
								{
									goto IL_0130;
								}
								ParserResult parserResult = UnityTextParsers.GameLogTextParser.Parse(originalText, scope, TextCache);
								if (parserResult == null)
								{
									goto IL_0130;
								}
								string text = TranslateOrQueueWebJobImmediateByParserResult(key, parserResult, scope, allowStartTranslationImmediate: false, allowStartTranslationLater: false, allowImmediateCaching: false, TextCache, null);
								if (text == null)
								{
									goto IL_0130;
								}
								textTranslationInfo.UnresizeUI(key);
								SetTranslatedText(key, text, null, textTranslationInfo);
								flag = true;
								goto end_IL_0038;
								IL_0130:
								if (!UnityTextParsers.RegexSplittingTextParser.CanApply(key))
								{
									goto IL_018e;
								}
								ParserResult parserResult2 = UnityTextParsers.RegexSplittingTextParser.Parse(originalText, scope, TextCache);
								if (parserResult2 == null)
								{
									goto IL_018e;
								}
								string text2 = TranslateOrQueueWebJobImmediateByParserResult(key, parserResult2, scope, allowStartTranslationImmediate: false, allowStartTranslationLater: false, allowImmediateCaching: false, TextCache, null);
								if (text2 == null)
								{
									goto IL_018e;
								}
								textTranslationInfo.UnresizeUI(key);
								SetTranslatedText(key, text2, null, textTranslationInfo);
								flag = true;
								goto end_IL_0038;
								IL_018e:
								if (UnityTextParsers.RichTextParser.CanApply(key))
								{
									ParserResult parserResult3 = UnityTextParsers.RichTextParser.Parse(originalText, scope);
									if (parserResult3 != null)
									{
										string text3 = TranslateOrQueueWebJobImmediateByParserResult(key, parserResult3, scope, allowStartTranslationImmediate: false, allowStartTranslationLater: false, allowImmediateCaching: false, TextCache, null);
										if (text3 != null)
										{
											textTranslationInfo.UnresizeUI(key);
											SetTranslatedText(key, text3, null, textTranslationInfo);
											flag = true;
											continue;
										}
										goto end_IL_0089;
									}
									goto end_IL_0089;
								}
								end_IL_0089:;
							}
							finally
							{
								if (!flag)
								{
									SetText(key, textTranslationInfo.OriginalText, isTranslated: false, null, textTranslationInfo);
									Hook_TextChanged(key, onEnable: false);
								}
							}
						}
					}
					if (Settings.EnableTextureTranslation && (key is Texture2D || key.IsKnownImageType()))
					{
						TranslateTexture(key, context);
					}
					end_IL_0038:;
				}
				catch (Exception)
				{
					ExtensionDataHelper.Remove(key);
				}
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while reloading translations.");
		}
	}

	private void ToggleFont()
	{
		if (!_hasValidOverrideFont)
		{
			return;
		}
		_hasOverridenFont = !_hasOverridenFont;
		List<KeyValuePair<object, object>> allRegisteredObjects = ExtensionDataHelper.GetAllRegisteredObjects();
		XuaLogger.AutoTranslator.Info($"Toggling fonts of {allRegisteredObjects.Count} objects.");
		if (_hasOverridenFont)
		{
			foreach (KeyValuePair<object, object> item in allRegisteredObjects)
			{
				if (item.Value is TextTranslationInfo textTranslationInfo)
				{
					object key = item.Key;
					try
					{
						if (key.IsComponentActive())
						{
							textTranslationInfo.ChangeFont(key);
						}
					}
					catch (Exception)
					{
						ExtensionDataHelper.Remove(key);
					}
				}
			}
			return;
		}
		foreach (KeyValuePair<object, object> item2 in allRegisteredObjects)
		{
			if (!(item2.Value is TextTranslationInfo textTranslationInfo2))
			{
				continue;
			}
			object key2 = item2.Key;
			try
			{
				if (key2.IsComponentActive())
				{
					textTranslationInfo2.UnchangeFont(key2);
				}
			}
			catch (Exception)
			{
				ExtensionDataHelper.Remove(key2);
			}
		}
	}

	private void ToggleTranslation()
	{
		_isInTranslatedMode = !_isInTranslatedMode;
		List<KeyValuePair<object, object>> allRegisteredObjects = ExtensionDataHelper.GetAllRegisteredObjects();
		XuaLogger.AutoTranslator.Info($"Toggling translations of {allRegisteredObjects.Count} objects.");
		if (_isInTranslatedMode)
		{
			foreach (KeyValuePair<object, object> item in allRegisteredObjects)
			{
				object key = item.Key;
				try
				{
					TextTranslationInfo textTranslationInfo = item.Value as TextTranslationInfo;
					if (textTranslationInfo.GetIsKnownTextComponent() && key.IsComponentActive() && textTranslationInfo != null && textTranslationInfo.IsTranslated)
					{
						SetText(key, textTranslationInfo.TranslatedText, isTranslated: true, null, textTranslationInfo);
					}
					if (Settings.EnableTextureTranslation && Settings.EnableTextureToggling && (key is Texture2D || key.IsKnownImageType()))
					{
						TranslateTexture(key, null);
					}
				}
				catch (Exception)
				{
					ExtensionDataHelper.Remove(key);
				}
			}
			return;
		}
		foreach (KeyValuePair<object, object> item2 in allRegisteredObjects)
		{
			object key2 = item2.Key;
			try
			{
				TextTranslationInfo textTranslationInfo2 = item2.Value as TextTranslationInfo;
				if (textTranslationInfo2.GetIsKnownTextComponent() && key2.IsComponentActive() && textTranslationInfo2 != null && textTranslationInfo2.IsTranslated)
				{
					SetText(key2, textTranslationInfo2.OriginalText, isTranslated: false, null, textTranslationInfo2);
				}
				if (Settings.EnableTextureTranslation && Settings.EnableTextureToggling)
				{
					TranslateTexture(key2, null);
				}
			}
			catch (Exception)
			{
				ExtensionDataHelper.Remove(key2);
			}
		}
	}

	private void CopyToClipboard()
	{
		if (Settings.CopyToClipboard && _textsToCopyToClipboardOrdered.Count > 0 && (_textsToCopyToClipboardOrdered.Count > 5 || Time.realtimeSinceStartup - _clipboardUpdated > Settings.ClipboardDebounceTime))
		{
			try
			{
				ClipboardHelper.CopyToClipboard(_textsToCopyToClipboardOrdered, Settings.MaxClipboardCopyCharacters);
			}
			finally
			{
				_textsToCopyToClipboard.Clear();
				_textsToCopyToClipboardOrdered.Clear();
			}
		}
	}

	private void PrintObjects()
	{
		using FileStream stream = File.Open(Path.Combine(Paths.GameRoot, "hierarchy.txt"), FileMode.Create);
		using StreamWriter streamWriter = new StreamWriter(stream);
		foreach (GameObject allRoot in GetAllRoots())
		{
			TraverseChildren(streamWriter, allRoot, "");
		}
		streamWriter.Flush();
	}

	private void ManualHook()
	{
		ManualHookForComponents();
		ManualHookForTextures();
	}

	private void ManualHookForComponents()
	{
		foreach (GameObject allRoot in GetAllRoots())
		{
			TraverseChildrenManualHook(allRoot);
		}
	}

	private void ManualHookForTextures()
	{
		if (Settings.EnableTextureScanOnSceneLoad && (Settings.EnableTextureTranslation || Settings.EnableTextureDumping))
		{
			Texture2D[] array = ComponentHelper.FindObjectsOfType<Texture2D>();
			for (int i = 0; i < array.Length; i++)
			{
				Texture2D texture = array[i];
				Hook_ImageChanged(ref texture, isPrefixHooked: false);
			}
		}
	}

	private IEnumerable<GameObject> GetAllRoots()
	{
		GameObject[] array = ComponentHelper.FindObjectsOfType<GameObject>();
		GameObject[] array2 = array;
		foreach (GameObject val in array2)
		{
			if ((Object)(object)val.transform != (Object)null && (Object)(object)val.transform.parent == (Object)null)
			{
				yield return val;
			}
		}
	}

	private void TraverseChildren(StreamWriter writer, GameObject obj, string identation)
	{
		if (!((Object)(object)obj != (Object)null))
		{
			return;
		}
		string text = LayerMask.LayerToName(obj.layer);
		string arg = string.Join(", ", (from x in obj.GetComponents<Component>().Select(delegate(Component x)
			{
				string text2 = null;
				Type type = ((object)x)?.GetType();
				if (type != null)
				{
					text2 = type.Name;
					TextTranslationInfo orCreateTextTranslationInfo = x.GetOrCreateTextTranslationInfo();
					string text3 = x.GetText(orCreateTextTranslationInfo);
					if (!string.IsNullOrEmpty(text3))
					{
						text2 = text2 + " (" + text3 + ")";
					}
				}
				return text2;
			})
			where x != null
			select x).ToArray());
		string value = string.Format("{0,-50} {1,100}", identation + ((Object)obj).name + " [" + text + "]", arg);
		writer.WriteLine(value);
		if ((Object)(object)obj.transform != (Object)null)
		{
			for (int num = 0; num < obj.transform.childCount; num++)
			{
				Transform child = obj.transform.GetChild(num);
				TraverseChildren(writer, ((Component)child).gameObject, identation + " ");
			}
		}
	}

	private void TraverseChildrenManualHook(GameObject obj)
	{
		if (!((Object)(object)obj != (Object)null))
		{
			return;
		}
		Component[] components = obj.GetComponents<Component>();
		foreach (Component val in components)
		{
			object obj2 = val.CreateDerivedProxyIfRequiredAndPossible();
			if (obj2 != null)
			{
				Hook_TextChanged(obj2, onEnable: false);
			}
			if ((Settings.EnableTextureTranslation || Settings.EnableTextureDumping) && val.IsKnownImageType())
			{
				Texture2D texture = null;
				Hook_ImageChangedOnComponent(val, ref texture, isPrefixHooked: false, onEnable: false);
			}
		}
		if ((Object)(object)obj.transform != (Object)null)
		{
			for (int j = 0; j < obj.transform.childCount; j++)
			{
				Transform child = obj.transform.GetChild(j);
				TraverseChildrenManualHook(((Component)child).gameObject);
			}
		}
	}

	public void DisableAutoTranslator()
	{
		_temporarilyDisabled = true;
	}

	public void EnableAutoTranslator()
	{
		_temporarilyDisabled = false;
	}

	internal bool IsTemporarilyDisabled()
	{
		return _temporarilyDisabled;
	}

	private void OnDestroy()
	{
		try
		{
			RedirectedDirectory.Uninitialize();
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while uninitializing redirected directory cache.");
		}
		try
		{
			TextCache.Dispose();
		}
		catch (Exception e2)
		{
			XuaLogger.AutoTranslator.Error(e2, "An error occurred while disposing translation text cache.");
		}
		try
		{
			TextureCache.Dispose();
		}
		catch (Exception e3)
		{
			XuaLogger.AutoTranslator.Error(e3, "An error occurred while disposing translation texture cache.");
		}
		foreach (TranslationEndpointManager allEndpoint in TranslationManager.AllEndpoints)
		{
			try
			{
				if (allEndpoint.Endpoint is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			catch (Exception e4)
			{
				XuaLogger.AutoTranslator.Error(e4, "An error occurred while disposing endpoint.");
			}
		}
	}
}
