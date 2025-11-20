using System;
using SRDebugger.Gamepad;
using SRDebugger.Internal;
using SRDebugger.UI;
using SRDebugger.VirtualMouse;
using SRF;
using SRF.Service;
using SRF.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SRDebugger.Services.Implementation;

[Service(typeof(IDebugService))]
public class SRDebugService : IDebugService
{
	private readonly IDebugPanelService _debugPanelService;

	private readonly IDebugTriggerService _debugTrigger;

	private readonly ISystemInformationService _informationService;

	private readonly IOptionsService _optionsService;

	private readonly IPinnedUIService _pinnedUiService;

	private IConsoleFilterState _consoleFilterState;

	private EntryCode? _entryCode;

	private bool _hasAuthorised;

	private DefaultTabs? _queuedTab;

	private RectTransform _worldSpaceTransform;

	private DynamicOptionContainer _looseOptionContainer;

	private IGamepad _gamepad;

	private IVirtualMouse _virtualMouse;

	public IDockConsoleService DockConsole => Service.DockConsole;

	public IConsoleFilterState ConsoleFilter
	{
		get
		{
			if (_consoleFilterState == null)
			{
				_consoleFilterState = SRServiceManager.GetService<IConsoleFilterState>();
			}
			return _consoleFilterState;
		}
	}

	public Settings Settings => Settings.Instance;

	public IGamepad Gamepad => _gamepad;

	public IVirtualMouse VirtualMouse => _virtualMouse;

	public bool IsDebugPanelVisible => _debugPanelService.IsVisible;

	public bool IsTriggerEnabled
	{
		get
		{
			return _debugTrigger.IsEnabled;
		}
		set
		{
			_debugTrigger.IsEnabled = value;
		}
	}

	public bool IsTriggerErrorNotificationEnabled
	{
		get
		{
			return _debugTrigger.ShowErrorNotification;
		}
		set
		{
			_debugTrigger.ShowErrorNotification = value;
		}
	}

	public bool IsProfilerDocked
	{
		get
		{
			return Service.PinnedUI.IsProfilerPinned;
		}
		set
		{
			Service.PinnedUI.IsProfilerPinned = value;
		}
	}

	public event VisibilityChangedDelegate PanelVisibilityChanged;

	public event PinnedUiCanvasCreated PinnedUiCanvasCreated;

	public SRDebugService()
	{
		SRServiceManager.RegisterService<IDebugService>(this);
		SRServiceManager.GetService<IProfilerService>();
		_debugTrigger = SRServiceManager.GetService<IDebugTriggerService>();
		_informationService = SRServiceManager.GetService<ISystemInformationService>();
		_pinnedUiService = SRServiceManager.GetService<IPinnedUIService>();
		_pinnedUiService.OptionsCanvasCreated += delegate(RectTransform transform)
		{
			if (this.PinnedUiCanvasCreated == null)
			{
				return;
			}
			try
			{
				this.PinnedUiCanvasCreated(transform);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		};
		_optionsService = SRServiceManager.GetService<IOptionsService>();
		_debugPanelService = SRServiceManager.GetService<IDebugPanelService>();
		_debugPanelService.VisibilityChanged += DebugPanelServiceOnVisibilityChanged;
		_debugTrigger.IsEnabled = Settings.EnableTrigger == Settings.TriggerEnableModes.Enabled || (Settings.EnableTrigger == Settings.TriggerEnableModes.MobileOnly && Application.isMobilePlatform) || (Settings.EnableTrigger == Settings.TriggerEnableModes.DevelopmentBuildsOnly && Debug.isDebugBuild);
		_debugTrigger.Position = Settings.TriggerPosition;
		if (Keyboard.current == null)
		{
			Debug.LogWarning("[SRDebugService] There is no current keyboard => keyboard shortcuts would not work");
		}
		else if (Settings.EnableKeyboardShortcuts)
		{
			SRServiceManager.GetService<KeyboardShortcutListenerService>();
		}
		SRServiceManager.GetService<GamepadShortcutsListenerService>();
		if (Settings.Instance.RequireCode)
		{
			if (Settings.Instance.EntryCode.Count != 4)
			{
				Debug.LogError("[SRDebugger] RequireCode is enabled, but pin is not 4 digits");
			}
			else
			{
				_entryCode = new EntryCode(Settings.Instance.EntryCode[0], Settings.Instance.EntryCode[1], Settings.Instance.EntryCode[2], Settings.Instance.EntryCode[3]);
			}
		}
		_gamepad = new DefaultGamepad();
		_virtualMouse = new DefaultVirtualMouse();
		UnityEngine.Object.DontDestroyOnLoad(Hierarchy.Get("SRDebugger").gameObject);
		SRServiceManager.GetService<InternalOptionsRegistry>().SetHandler(_optionsService.AddContainer);
	}

	public void AddSystemInfo(InfoEntry entry, string category = "Default")
	{
		_informationService.Add(entry, category);
	}

	public void ConnectAnotherGamepad(IGamepad gamepad)
	{
		_gamepad = gamepad;
	}

	public void ConnectVirtualMouse(IVirtualMouse virtualMouse)
	{
		_virtualMouse = virtualMouse;
	}

	public void ShowDebugPanel(bool requireEntryCode = true)
	{
		if (requireEntryCode && _entryCode.HasValue && !_hasAuthorised)
		{
			PromptEntryCode();
		}
		else
		{
			_debugPanelService.IsVisible = true;
		}
	}

	public void ShowDebugPanel(DefaultTabs tab, bool requireEntryCode = true)
	{
		if (requireEntryCode && _entryCode.HasValue && !_hasAuthorised)
		{
			_queuedTab = tab;
			PromptEntryCode();
		}
		else
		{
			_debugPanelService.IsVisible = true;
			_debugPanelService.OpenTab(tab);
		}
	}

	public void HideDebugPanel()
	{
		_debugPanelService.IsVisible = false;
	}

	public void SetEntryCode(EntryCode newCode)
	{
		_hasAuthorised = false;
		_entryCode = newCode;
	}

	public void DisableEntryCode()
	{
		_entryCode = null;
	}

	public void DestroyDebugPanel()
	{
		_debugPanelService.IsVisible = false;
		_debugPanelService.Unload();
	}

	public void AddOptionContainer(object container)
	{
		_optionsService.AddContainer(container);
	}

	public void RemoveOptionContainer(object container)
	{
		_optionsService.RemoveContainer(container);
	}

	public void AddOption(OptionDefinition option)
	{
		if (_looseOptionContainer == null)
		{
			_looseOptionContainer = new DynamicOptionContainer();
			_optionsService.AddContainer(_looseOptionContainer);
		}
		_looseOptionContainer.AddOption(option);
	}

	public bool RemoveOption(OptionDefinition option)
	{
		if (_looseOptionContainer != null)
		{
			return _looseOptionContainer.RemoveOption(option);
		}
		return false;
	}

	public void PinAllOptions(string category)
	{
		foreach (OptionDefinition option in _optionsService.Options)
		{
			if (option.Category == category)
			{
				_pinnedUiService.Pin(option);
			}
		}
	}

	public void UnpinAllOptions(string category)
	{
		foreach (OptionDefinition option in _optionsService.Options)
		{
			if (option.Category == category)
			{
				_pinnedUiService.Unpin(option);
			}
		}
	}

	public void PinOption(string name)
	{
		foreach (OptionDefinition option in _optionsService.Options)
		{
			if (option.Name == name)
			{
				_pinnedUiService.Pin(option);
			}
		}
	}

	public void UnpinOption(string name)
	{
		foreach (OptionDefinition option in _optionsService.Options)
		{
			if (option.Name == name)
			{
				_pinnedUiService.Unpin(option);
			}
		}
	}

	public void ClearPinnedOptions()
	{
		_pinnedUiService.UnpinAll();
	}

	public void ShowBugReportSheet(ActionCompleteCallback onComplete = null, bool takeScreenshot = true, string descriptionContent = null)
	{
		BugReportPopoverService service = SRServiceManager.GetService<BugReportPopoverService>();
		if (service.IsShowingPopover)
		{
			return;
		}
		service.ShowBugReporter(delegate(bool succeed, string message)
		{
			if (onComplete != null)
			{
				onComplete(succeed);
			}
		}, takeScreenshot, descriptionContent);
	}

	private void DebugPanelServiceOnVisibilityChanged(IDebugPanelService debugPanelService, bool b)
	{
		if (this.PanelVisibilityChanged == null)
		{
			return;
		}
		try
		{
			this.PanelVisibilityChanged(b);
		}
		catch (Exception exception)
		{
			Debug.LogError("[SRDebugger] Event target threw exception (IDebugService.PanelVisiblityChanged)");
			Debug.LogException(exception);
		}
	}

	private void PromptEntryCode()
	{
		SRServiceManager.GetService<IPinEntryService>().ShowPinEntry(_entryCode.Value, SRDebugStrings.Current.PinEntryPrompt, delegate(bool entered)
		{
			if (entered)
			{
				if (!Settings.Instance.RequireEntryCodeEveryTime)
				{
					_hasAuthorised = true;
				}
				if (_queuedTab.HasValue)
				{
					DefaultTabs value = _queuedTab.Value;
					_queuedTab = null;
					ShowDebugPanel(value, requireEntryCode: false);
				}
				else
				{
					ShowDebugPanel(requireEntryCode: false);
				}
			}
			_queuedTab = null;
		});
	}

	public RectTransform EnableWorldSpaceMode()
	{
		if (_worldSpaceTransform != null)
		{
			return _worldSpaceTransform;
		}
		if (Settings.Instance.UseDebugCamera)
		{
			throw new InvalidOperationException("UseDebugCamera cannot be enabled at the same time as EnableWorldSpaceMode.");
		}
		_debugPanelService.IsVisible = true;
		DebugPanelRoot rootObject = ((DebugPanelServiceImpl)_debugPanelService).RootObject;
		rootObject.Canvas.gameObject.RemoveComponentIfExists<SRRetinaScaler>();
		rootObject.Canvas.gameObject.RemoveComponentIfExists<CanvasScaler>();
		rootObject.Canvas.renderMode = RenderMode.WorldSpace;
		RectTransform component = rootObject.Canvas.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(1024f, 768f);
		component.position = Vector3.zero;
		return _worldSpaceTransform = component;
	}

	public void SetBugReporterHandler(IBugReporterHandler bugReporterHandler)
	{
		SRServiceManager.GetService<IBugReportService>().SetHandler(bugReporterHandler);
	}
}
