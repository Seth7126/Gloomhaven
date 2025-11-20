using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Parsing;
using XUnity.AutoTranslator.Plugin.Core.Properties;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core;

internal sealed class TextTranslationCache : IReadOnlyTextTranslationCache, IDisposable
{
	private struct TranslationCharacterToken
	{
		public char Character { get; set; }

		public bool IsVariable { get; set; }

		public TranslationCharacterToken(char c, bool isVariable)
		{
			Character = c;
			IsVariable = isVariable;
		}
	}

	public class TranslationDictionaries
	{
		public Dictionary<string, string> TokenTranslations { get; }

		public Dictionary<string, string> ReverseTokenTranslations { get; }

		public Dictionary<string, string> Translations { get; }

		public Dictionary<string, string> ReverseTranslations { get; }

		public List<RegexTranslation> DefaultRegexes { get; }

		public HashSet<string> RegisteredRegexes { get; }

		public List<RegexTranslationSplitter> SplitterRegexes { get; }

		public HashSet<string> RegisteredSplitterRegexes { get; }

		public HashSet<string> FailedRegexLookups { get; set; }

		public TranslationDictionaries()
		{
			Translations = new Dictionary<string, string>();
			ReverseTranslations = new Dictionary<string, string>();
			DefaultRegexes = new List<RegexTranslation>();
			RegisteredRegexes = new HashSet<string>();
			TokenTranslations = new Dictionary<string, string>();
			ReverseTokenTranslations = new Dictionary<string, string>();
			SplitterRegexes = new List<RegexTranslationSplitter>();
			RegisteredSplitterRegexes = new HashSet<string>();
			FailedRegexLookups = new HashSet<string>();
		}
	}

	private Dictionary<IReadOnlyTextTranslationCache, CompositeTextTranslationCache> _compositeCaches = new Dictionary<IReadOnlyTextTranslationCache, CompositeTextTranslationCache>();

	private static readonly List<KeyValuePairTranslationPackage> _kvpPackages = new List<KeyValuePairTranslationPackage>();

	private static readonly List<StreamTranslationPackage> _streamPackages = new List<StreamTranslationPackage>();

	private Dictionary<string, string> _staticTranslations = new Dictionary<string, string>();

	private Dictionary<string, string> _translations = new Dictionary<string, string>();

	private Dictionary<string, string> _reverseTranslations = new Dictionary<string, string>();

	private Dictionary<string, string> _tokenTranslations = new Dictionary<string, string>();

	private Dictionary<string, string> _reverseTokenTranslations = new Dictionary<string, string>();

	private HashSet<string> _partialTranslations = new HashSet<string>();

	private List<RegexTranslation> _defaultRegexes = new List<RegexTranslation>();

	private HashSet<string> _registeredRegexes = new HashSet<string>();

	private HashSet<string> _failedRegexLookups = new HashSet<string>();

	private List<RegexTranslationSplitter> _splitterRegexes = new List<RegexTranslationSplitter>();

	private HashSet<string> _registeredSplitterRegexes = new HashSet<string>();

	private Dictionary<int, TranslationDictionaries> _scopedTranslations = new Dictionary<int, TranslationDictionaries>();

	private object _writeToFileSync = new object();

	private Dictionary<string, string> _newTranslations = new Dictionary<string, string>();

	private bool disposedValue;

	private readonly DirectoryInfo _pluginDirectory;

	private SafeFileWatcher _fileWatcher;

	public bool DefaultAllowFallback { get; internal set; }

	public bool AllowFallback { get; internal set; }

	public bool AllowGeneratingNewTranslations { get; private set; }

	public bool HasLoadedInMemoryTranslations
	{
		get
		{
			if (_kvpPackages.Count <= 0)
			{
				return _streamPackages.Count > 0;
			}
			return true;
		}
	}

	public event Action TextTranslationFileChanged;

	public TextTranslationCache()
	{
		AllowGeneratingNewTranslations = true;
		AllowFallback = false;
		DefaultAllowFallback = false;
		LoadStaticTranslations();
		if (Settings.ReloadTranslationsOnFileChange)
		{
			try
			{
				Directory.CreateDirectory(Settings.TranslationsPath);
				_fileWatcher = new SafeFileWatcher(Settings.TranslationsPath);
				_fileWatcher.DirectoryUpdated += FileWatcher_DirectoryUpdated;
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while initializing translation file watching for text.");
			}
		}
		MaintenanceHelper.AddMaintenanceFunction(SaveNewTranslationsToDisk, 1);
	}

	private void FileWatcher_DirectoryUpdated()
	{
		this.TextTranslationFileChanged?.Invoke();
	}

	public TextTranslationCache(DirectoryInfo pluginDirectory)
	{
		AllowGeneratingNewTranslations = false;
		AllowFallback = false;
		DefaultAllowFallback = false;
		_pluginDirectory = pluginDirectory;
	}

	public TextTranslationCache(string pluginDirectory)
	{
		AllowGeneratingNewTranslations = false;
		AllowFallback = false;
		DefaultAllowFallback = false;
		_pluginDirectory = new DirectoryInfo(Path.Combine(Path.Combine(Settings.TranslationsPath, "plugins"), pluginDirectory));
	}

	private IEnumerable<string> GetTranslationFiles()
	{
		return from x in (from x in Directory.GetFiles(_pluginDirectory?.FullName ?? Settings.TranslationsPath, "*", SearchOption.AllDirectories)
				where x.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
				where !x.EndsWith("resizer.txt", StringComparison.OrdinalIgnoreCase)
				select new FileInfo(x) into fi
				select new
				{
					IsZipped = fi.FullName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase),
					FileInfo = fi
				} into x
				orderby x.IsZipped descending
				select x).ThenByDescending(x => x.FileInfo.FullName, StringComparer.OrdinalIgnoreCase)
			select x.FileInfo.FullName;
	}

	internal CompositeTextTranslationCache GetOrCreateCompositeCache(IReadOnlyTextTranslationCache primary)
	{
		if (!_compositeCaches.TryGetValue(primary, out var value))
		{
			value = new CompositeTextTranslationCache(primary, this);
			_compositeCaches[primary] = value;
		}
		return value;
	}

	public void PruneMainTranslationFile()
	{
		XuaLogger.AutoTranslator.Debug("Pruning text translations in main translation file...");
		FileInfo fileInfo = new FileInfo(Settings.AutoTranslationsFilePath);
		using MemoryStream memoryStream = new MemoryStream();
		bool flag;
		using (FileStream inputStream = fileInfo.OpenRead())
		{
			flag = PruneTranslationFile(inputStream, memoryStream);
		}
		if (flag)
		{
			string text = fileInfo.FullName + "." + DateTime.Now.ToString("yyyyMMddHHmmssfff.bak");
			File.Move(fileInfo.FullName, text);
			using (FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Create))
			{
				memoryStream.Seek(0L, SeekOrigin.Begin);
				memoryStream.WriteTo(fileStream);
				fileStream.Flush();
				fileStream.Close();
			}
			XuaLogger.AutoTranslator.Warn("Generated backup translation file: " + text);
		}
	}

	internal void LoadTranslationFiles()
	{
		try
		{
			AllowFallback = DefaultAllowFallback;
			if (_pluginDirectory != null)
			{
				XuaLogger.AutoTranslator.Debug("--- Loading Plugin Translations (" + _pluginDirectory.Name + ") ---");
			}
			else
			{
				XuaLogger.AutoTranslator.Debug("--- Loading Global Translations ---");
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			lock (_writeToFileSync)
			{
				string text = Path.Combine(Settings.TranslationsPath, "plugins");
				Directory.CreateDirectory(Settings.TranslationsPath);
				if (_pluginDirectory != null)
				{
					Directory.CreateDirectory(text);
					Directory.CreateDirectory(_pluginDirectory.FullName);
				}
				else
				{
					Directory.CreateDirectory(Path.GetDirectoryName(Settings.AutoTranslationsFilePath));
				}
				_registeredRegexes.Clear();
				_defaultRegexes.Clear();
				_translations.Clear();
				_reverseTranslations.Clear();
				_partialTranslations.Clear();
				_tokenTranslations.Clear();
				_reverseTokenTranslations.Clear();
				_registeredSplitterRegexes.Clear();
				_splitterRegexes.Clear();
				_scopedTranslations.Clear();
				_failedRegexLookups.Clear();
				string fullName = new FileInfo(Settings.AutoTranslationsFilePath).FullName;
				string fullName2 = new FileInfo(Settings.SubstitutionFilePath).FullName;
				string fullName3 = new FileInfo(Settings.PreprocessorsFilePath).FullName;
				string fullName4 = new FileInfo(Settings.PostprocessorsFilePath).FullName;
				if (_pluginDirectory == null)
				{
					LoadTranslationsInFile(fullName, isOutputFile: true, isLoad: true, AddTranslationSplitterRegex, AddTranslationRegex, AddTranslation);
				}
				foreach (string item in GetTranslationFiles().Except(new string[4] { fullName, fullName2, fullName3, fullName4 }, StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						if (_pluginDirectory != null || !item.StartsWith(text, StringComparison.OrdinalIgnoreCase))
						{
							LoadTranslationsInFile(item, isOutputFile: false, isLoad: true, AddTranslationSplitterRegex, AddTranslationRegex, AddTranslation);
						}
					}
					catch (Exception e)
					{
						XuaLogger.AutoTranslator.Error(e, "An error occurred while loading translations in file: " + item);
					}
				}
			}
			foreach (StreamTranslationPackage streamPackage in _streamPackages)
			{
				try
				{
					Stream readableStream = streamPackage.GetReadableStream();
					if (readableStream.CanSeek)
					{
						readableStream.Seek(0L, SeekOrigin.Begin);
					}
					LoadTranslationsInStream(readableStream, streamPackage.Name, isOutputFile: false, isLoad: true, AddTranslationSplitterRegex, AddTranslationRegex, AddTranslation);
				}
				catch (Exception e2)
				{
					XuaLogger.AutoTranslator.Error(e2, "An error occurred while loading translations in stream translation package: " + streamPackage.Name);
				}
			}
			foreach (KeyValuePairTranslationPackage kvpPackage in _kvpPackages)
			{
				try
				{
					IEnumerable<KeyValuePair<string, string>> iterableEntries = kvpPackage.GetIterableEntries();
					LoadTranslationsInKeyValuePairs(iterableEntries, kvpPackage.Name);
				}
				catch (Exception e3)
				{
					XuaLogger.AutoTranslator.Error(e3, "An error occurred while loading translations in KVP translation package: " + kvpPackage.Name);
				}
			}
			float realtimeSinceStartup2 = Time.realtimeSinceStartup;
			XuaLogger.AutoTranslator.Debug($"Loaded translation text files (took {Math.Round(realtimeSinceStartup2 - realtimeSinceStartup, 2)} seconds)");
			realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (KeyValuePair<string, string> item2 in _translations.ToList())
			{
				UntranslatedText untranslatedText = new UntranslatedText(item2.Key, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
				UntranslatedText untranslatedText2 = new UntranslatedText(item2.Value, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.ToLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
				if (untranslatedText.Original_Text_ExternallyTrimmed != item2.Key && !HasTranslated(untranslatedText.Original_Text_ExternallyTrimmed, -1, checkAll: false))
				{
					AddTranslation(untranslatedText.Original_Text_ExternallyTrimmed, untranslatedText2.Original_Text_ExternallyTrimmed, -1);
				}
				if (untranslatedText.Original_Text_ExternallyTrimmed != untranslatedText.Original_Text_FullyTrimmed && !HasTranslated(untranslatedText.Original_Text_FullyTrimmed, -1, checkAll: false))
				{
					AddTranslation(untranslatedText.Original_Text_FullyTrimmed, untranslatedText2.Original_Text_FullyTrimmed, -1);
				}
			}
			foreach (KeyValuePair<int, TranslationDictionaries> item3 in _scopedTranslations.ToList())
			{
				int key = item3.Key;
				foreach (KeyValuePair<string, string> item4 in item3.Value.Translations.ToList())
				{
					UntranslatedText untranslatedText3 = new UntranslatedText(item4.Key, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
					UntranslatedText untranslatedText4 = new UntranslatedText(item4.Value, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.ToLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
					if (untranslatedText3.Original_Text_ExternallyTrimmed != item4.Key && !HasTranslated(untranslatedText3.Original_Text_ExternallyTrimmed, key, checkAll: false))
					{
						AddTranslation(untranslatedText3.Original_Text_ExternallyTrimmed, untranslatedText4.Original_Text_ExternallyTrimmed, key);
					}
					if (untranslatedText3.Original_Text_ExternallyTrimmed != untranslatedText3.Original_Text_FullyTrimmed && !HasTranslated(untranslatedText3.Original_Text_FullyTrimmed, key, checkAll: false))
					{
						AddTranslation(untranslatedText3.Original_Text_FullyTrimmed, untranslatedText4.Original_Text_FullyTrimmed, key);
					}
				}
			}
			XuaLogger.AutoTranslator.Debug($"Created variation translations (took {Math.Round(realtimeSinceStartup2 - realtimeSinceStartup, 2)} seconds)");
			realtimeSinceStartup2 = Time.realtimeSinceStartup;
			if (Settings.GeneratePartialTranslations)
			{
				realtimeSinceStartup = Time.realtimeSinceStartup;
				foreach (KeyValuePair<string, string> item5 in _translations.ToList())
				{
					CreatePartialTranslationsFor(item5.Key, item5.Value);
				}
				XuaLogger.AutoTranslator.Debug($"Created partial translations (took {Math.Round(realtimeSinceStartup2 - realtimeSinceStartup, 2)} seconds)");
				realtimeSinceStartup2 = Time.realtimeSinceStartup;
			}
			RichTextParser richTextParser = new RichTextParser();
			realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (KeyValuePair<string, string> item6 in _translations.ToList())
			{
				ParserResult parserResult = richTextParser.Parse(item6.Key, -1);
				if (parserResult == null)
				{
					continue;
				}
				ParserResult parserResult2 = richTextParser.Parse(item6.Value, -1);
				if (parserResult2 == null || parserResult.Arguments.Count != parserResult2.Arguments.Count)
				{
					continue;
				}
				foreach (ArgumentedUntranslatedTextInfo argument in parserResult.Arguments)
				{
					string key2 = argument.Key;
					string untranslatedText5 = argument.Info.UntranslatedText;
					ArgumentedUntranslatedTextInfo argumentedUntranslatedTextInfo = parserResult2.Arguments.FirstOrDefault((ArgumentedUntranslatedTextInfo x) => x.Key == key2);
					if (argumentedUntranslatedTextInfo != null)
					{
						AddTokenTranslation(untranslatedText5, argumentedUntranslatedTextInfo.Info.UntranslatedText, -1);
					}
				}
			}
			foreach (KeyValuePair<int, TranslationDictionaries> item7 in _scopedTranslations.ToList())
			{
				int key3 = item7.Key;
				foreach (KeyValuePair<string, string> item8 in item7.Value.Translations.ToList())
				{
					ParserResult parserResult3 = richTextParser.Parse(item8.Key, key3);
					if (parserResult3 == null)
					{
						continue;
					}
					ParserResult parserResult4 = richTextParser.Parse(item8.Value, key3);
					if (parserResult4 == null || parserResult3.Arguments.Count != parserResult4.Arguments.Count)
					{
						continue;
					}
					foreach (ArgumentedUntranslatedTextInfo argument2 in parserResult3.Arguments)
					{
						string key4 = argument2.Key;
						string untranslatedText6 = argument2.Info.UntranslatedText;
						ArgumentedUntranslatedTextInfo argumentedUntranslatedTextInfo2 = parserResult4.Arguments.FirstOrDefault((ArgumentedUntranslatedTextInfo x) => x.Key == key4);
						if (argumentedUntranslatedTextInfo2 != null)
						{
							AddTokenTranslation(untranslatedText6, argumentedUntranslatedTextInfo2.Info.UntranslatedText, key3);
						}
					}
				}
			}
			XuaLogger.AutoTranslator.Debug($"Created token translations (took {Math.Round(realtimeSinceStartup2 - realtimeSinceStartup, 2)} seconds)");
			realtimeSinceStartup2 = Time.realtimeSinceStartup;
			if (!Settings.EnableSilentMode)
			{
				XuaLogger.AutoTranslator.Debug($"Translations generated: {_translations.Count}");
			}
			if (!Settings.EnableSilentMode)
			{
				XuaLogger.AutoTranslator.Debug($"Regex translations generated: {_defaultRegexes.Count}");
			}
			if (!Settings.EnableSilentMode)
			{
				XuaLogger.AutoTranslator.Debug($"Regex splitters generated: {_splitterRegexes.Count}");
			}
			if (!Settings.EnableSilentMode)
			{
				XuaLogger.AutoTranslator.Debug($"Token translations generated: {_tokenTranslations.Count}");
			}
			if (Settings.GeneratePartialTranslations)
			{
				XuaLogger.AutoTranslator.Debug($"Partial translations generated: {_partialTranslations.Count}");
			}
			foreach (KeyValuePair<int, TranslationDictionaries> item9 in _scopedTranslations.OrderBy((KeyValuePair<int, TranslationDictionaries> x) => x.Key))
			{
				int key5 = item9.Key;
				TranslationDictionaries value = item9.Value;
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Debug($"Scene {key5} translations generated: {value.Translations.Count}");
				}
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Debug($"Scene {key5} regex translations generated: {value.DefaultRegexes.Count}");
				}
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Debug($"Scene {key5} regex splitter generated: {value.SplitterRegexes.Count}");
				}
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Debug($"Scene {key5} token translations generated: {value.TokenTranslations.Count}");
				}
			}
		}
		catch (Exception e4)
		{
			XuaLogger.AutoTranslator.Error(e4, "An error occurred while loading translations.");
		}
	}

	private void CreatePartialTranslationsFor(string originalText, string translatedText)
	{
		List<TranslationCharacterToken> list = Tokenify(originalText);
		List<TranslationCharacterToken> list2 = Tokenify(translatedText);
		double num = (double)list2.Count / (double)list.Count;
		int num2 = 0;
		string text = string.Empty;
		string text2 = string.Empty;
		for (int i = 0; i < list.Count; i++)
		{
			TranslationCharacterToken translationCharacterToken = list[i];
			text = ((!translationCharacterToken.IsVariable) ? (text + translationCharacterToken.Character) : (text + "{{" + translationCharacterToken.Character + "}}"));
			int num3 = (int)Math.Round((double)(i + 1) * num, 0, MidpointRounding.AwayFromZero);
			for (int j = num2; j < num3; j++)
			{
				TranslationCharacterToken translationCharacterToken2 = list2[j];
				text2 = ((!translationCharacterToken2.IsVariable) ? (text2 + translationCharacterToken2.Character) : (text2 + "{{" + translationCharacterToken2.Character + "}}"));
			}
			num2 = num3;
			if (!HasTranslated(text, -1, checkAll: false))
			{
				AddTranslation(text, text2, -1);
				_partialTranslations.Add(text);
			}
		}
	}

	private static List<TranslationCharacterToken> Tokenify(string text)
	{
		List<TranslationCharacterToken> list = new List<TranslationCharacterToken>(text.Length);
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '{' && text.Length > i + 4 && text[i + 1] == '{' && text[i + 3] == '}' && text[i + 4] == '}')
			{
				c = text[i + 2];
				list.Add(new TranslationCharacterToken(c, isVariable: true));
				i += 4;
			}
			else
			{
				list.Add(new TranslationCharacterToken(c, isVariable: false));
			}
		}
		return list;
	}

	private bool PruneTranslationFile(Stream inputStream, Stream outputStream)
	{
		HashSet<string> registerdRegexes = new HashSet<string>();
		List<RegexTranslation> regexes = new List<RegexTranslation>();
		Action<RegexTranslation, int> addTranslationRegex = delegate(RegexTranslation regex, int level)
		{
			if (level == -1 && !registerdRegexes.Contains(regex.Original))
			{
				registerdRegexes.Add(regex.Original);
				regexes.Add(regex);
			}
		};
		string value = Path.Combine(Settings.TranslationsPath, "plugins");
		string fullName = new FileInfo(Settings.AutoTranslationsFilePath).FullName;
		string fullName2 = new FileInfo(Settings.SubstitutionFilePath).FullName;
		string fullName3 = new FileInfo(Settings.PreprocessorsFilePath).FullName;
		string fullName4 = new FileInfo(Settings.PostprocessorsFilePath).FullName;
		foreach (string item in GetTranslationFiles().Except(new string[4] { fullName, fullName2, fullName3, fullName4 }, StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				if (_pluginDirectory != null || !item.StartsWith(value, StringComparison.OrdinalIgnoreCase))
				{
					LoadTranslationsInFile(item, isOutputFile: false, isLoad: false, null, addTranslationRegex, null);
				}
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while loading translations in file: " + item);
			}
		}
		bool result = false;
		StreamReader streamReader = new StreamReader(inputStream, Encoding.UTF8);
		StreamWriter streamWriter = new StreamWriter(outputStream, Encoding.UTF8);
		string[] array = streamReader.ReadToEnd().Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			string[] array2 = TextHelper.ReadTranslationLineAndDecode(text);
			if (array2 == null)
			{
				continue;
			}
			string key = array2[0];
			string text2 = array2[1];
			if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(text2) && !key.StartsWith("sr:") && !key.StartsWith("r:"))
			{
				if (!regexes.Any((RegexTranslation x) => x.CompiledRegex.IsMatch(key)))
				{
					streamWriter.WriteLine(TextHelper.Encode(key) + "=" + TextHelper.Encode(text2));
					continue;
				}
				XuaLogger.AutoTranslator.Warn("Pruned translation: " + text);
				result = true;
			}
		}
		streamWriter.Flush();
		return result;
	}

	private void LoadTranslationsInStream(Stream stream, string fullFileName, bool isOutputFile, bool isLoad, Action<RegexTranslationSplitter, int> addTranslationSplitterRegex, Action<RegexTranslation, int> addTranslationRegex, Action<string, string, int> addTranslation)
	{
		if (!Settings.EnableSilentMode && isLoad)
		{
			XuaLogger.AutoTranslator.Debug("Loading texts: " + fullFileName + ".");
		}
		StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
		TranslationFileLoadingContext translationFileLoadingContext = new TranslationFileLoadingContext();
		string[] array = streamReader.ReadToEnd().Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			try
			{
				if (isOutputFile)
				{
					goto IL_00a9;
				}
				TranslationFileDirective translationFileDirective = TranslationFileDirective.Create(text);
				if (translationFileDirective == null)
				{
					goto IL_00a9;
				}
				translationFileLoadingContext.Apply(translationFileDirective);
				if (!Settings.EnableSilentMode && isLoad)
				{
					XuaLogger.AutoTranslator.Debug("Directive in file: " + fullFileName + ": " + translationFileDirective.ToString());
				}
				goto end_IL_005f;
				IL_00a9:
				if (!translationFileLoadingContext.IsApplicable())
				{
					continue;
				}
				string[] array2 = TextHelper.ReadTranslationLineAndDecode(text);
				if (array2 == null)
				{
					continue;
				}
				string text2 = array2[0];
				string text3 = array2[1];
				if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3))
				{
					continue;
				}
				if (text2.StartsWith("sr:"))
				{
					try
					{
						RegexTranslationSplitter arg = new RegexTranslationSplitter(text2, text3);
						HashSet<int> levels = translationFileLoadingContext.GetLevels();
						if (levels.Count == 0)
						{
							addTranslationSplitterRegex?.Invoke(arg, -1);
							continue;
						}
						foreach (int item in levels)
						{
							addTranslationSplitterRegex?.Invoke(arg, item);
						}
					}
					catch (Exception e)
					{
						XuaLogger.AutoTranslator.Warn(e, "An error occurred while constructing regex translation splitter: '" + text + "'.");
					}
					continue;
				}
				if (text2.StartsWith("r:"))
				{
					try
					{
						RegexTranslation arg2 = new RegexTranslation(text2, text3);
						HashSet<int> levels2 = translationFileLoadingContext.GetLevels();
						if (levels2.Count == 0)
						{
							addTranslationRegex?.Invoke(arg2, -1);
							continue;
						}
						foreach (int item2 in levels2)
						{
							addTranslationRegex?.Invoke(arg2, item2);
						}
					}
					catch (Exception e2)
					{
						XuaLogger.AutoTranslator.Warn(e2, "An error occurred while constructing regex translation: '" + text + "'.");
					}
					continue;
				}
				HashSet<int> levels3 = translationFileLoadingContext.GetLevels();
				if (levels3.Count == 0)
				{
					addTranslation?.Invoke(text2, text3, -1);
					continue;
				}
				foreach (int item3 in levels3)
				{
					addTranslation?.Invoke(text2, text3, item3);
				}
				end_IL_005f:;
			}
			catch (Exception e3)
			{
				XuaLogger.AutoTranslator.Warn(e3, "An error occurred while reading translation: '" + text + "'.");
			}
		}
		if (isLoad)
		{
			AllowFallback = AllowFallback || translationFileLoadingContext.IsEnabled("fallback");
		}
	}

	private void LoadTranslationsInKeyValuePairs(IEnumerable<KeyValuePair<string, string>> pairs, string fullFileName)
	{
		if (!Settings.EnableSilentMode)
		{
			XuaLogger.AutoTranslator.Debug("Loading texts: " + fullFileName + ".");
		}
		foreach (KeyValuePair<string, string> pair in pairs)
		{
			string key = pair.Key;
			string value = pair.Value;
			if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
			{
				continue;
			}
			if (key.StartsWith("sr:"))
			{
				try
				{
					RegexTranslationSplitter regex = new RegexTranslationSplitter(key, value);
					AddTranslationSplitterRegex(regex, -1);
				}
				catch (Exception e)
				{
					XuaLogger.AutoTranslator.Warn(e, "An error occurred while constructing regex translation splitter: '" + key + "=" + value + "'.");
				}
			}
			else if (key.StartsWith("r:"))
			{
				try
				{
					RegexTranslation regex2 = new RegexTranslation(key, value);
					AddTranslationRegex(regex2, -1);
				}
				catch (Exception e2)
				{
					XuaLogger.AutoTranslator.Warn(e2, "An error occurred while constructing regex translation: '" + key + "=" + value + "'.");
				}
			}
			else
			{
				AddTranslation(key, value, -1);
			}
		}
	}

	private void LoadTranslationsInFile(string fullFileName, bool isOutputFile, bool isLoad, Action<RegexTranslationSplitter, int> addTranslationSplitterRegex, Action<RegexTranslation, int> addTranslationRegex, Action<string, string, int> addTranslation)
	{
		bool flag = File.Exists(fullFileName);
		if (!flag || !flag)
		{
			return;
		}
		using FileStream fileStream = File.OpenRead(fullFileName);
		if (fullFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
		{
			ZipFile zipFile = new ZipFile(fileStream);
			foreach (ZipEntry item in zipFile.GetEntries().OrderByDescending((ZipEntry x) => x.Name, StringComparer.OrdinalIgnoreCase))
			{
				if (item.IsFile && item.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && !item.Name.EndsWith("resizer.txt", StringComparison.OrdinalIgnoreCase))
				{
					Stream inputStream = zipFile.GetInputStream(item);
					LoadTranslationsInStream(inputStream, fullFileName + Path.DirectorySeparatorChar + item.Name, isOutputFile, isLoad, addTranslationSplitterRegex, addTranslationRegex, addTranslation);
				}
			}
			zipFile.Close();
		}
		else
		{
			LoadTranslationsInStream(fileStream, fullFileName, isOutputFile, isLoad, addTranslationSplitterRegex, addTranslationRegex, addTranslation);
		}
	}

	private void LoadStaticTranslations()
	{
		if (!Settings.UseStaticTranslations || !(Settings.FromLanguage == Settings.DefaultFromLanguage) || !(Settings.Language == Settings.DefaultLanguage))
		{
			return;
		}
		string[] array = Resources.StaticTranslations.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = TextHelper.ReadTranslationLineAndDecode(array[i]);
			if (array2 != null)
			{
				string text = array2[0];
				string value = array2[1];
				if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(value))
				{
					_staticTranslations[text] = value;
				}
			}
		}
	}

	internal void RegisterPackage(StreamTranslationPackage package)
	{
		_streamPackages.Add(package);
	}

	internal void RegisterPackage(KeyValuePairTranslationPackage package)
	{
		_kvpPackages.Add(package);
	}

	private void SaveNewTranslationsToDisk()
	{
		if (_newTranslations.Count <= 0)
		{
			return;
		}
		lock (_writeToFileSync)
		{
			if (_newTranslations.Count <= 0)
			{
				return;
			}
			_fileWatcher?.Disable();
			try
			{
				using (FileStream stream = File.Open(Settings.AutoTranslationsFilePath, FileMode.Append, FileAccess.Write))
				{
					using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
					foreach (KeyValuePair<string, string> newTranslation in _newTranslations)
					{
						streamWriter.WriteLine(TextHelper.Encode(newTranslation.Key) + "=" + TextHelper.Encode(newTranslation.Value));
					}
					streamWriter.Flush();
				}
				_newTranslations.Clear();
			}
			finally
			{
				_fileWatcher?.Enable();
			}
		}
	}

	private void AddTranslationSplitterRegex(RegexTranslationSplitter regex, int scope)
	{
		if (scope != -1)
		{
			if (!_scopedTranslations.TryGetValue(scope, out var value))
			{
				value = new TranslationDictionaries();
				_scopedTranslations.Add(scope, value);
			}
			if (!value.RegisteredSplitterRegexes.Contains(regex.Original))
			{
				value.RegisteredSplitterRegexes.Add(regex.Original);
				value.SplitterRegexes.Add(regex);
			}
		}
		else if (!_registeredSplitterRegexes.Contains(regex.Original))
		{
			_registeredSplitterRegexes.Add(regex.Original);
			_splitterRegexes.Add(regex);
		}
	}

	private void AddTranslationRegex(RegexTranslation regex, int scope)
	{
		if (scope != -1)
		{
			if (!_scopedTranslations.TryGetValue(scope, out var value))
			{
				value = new TranslationDictionaries();
				_scopedTranslations.Add(scope, value);
			}
			if (!value.RegisteredRegexes.Contains(regex.Original))
			{
				value.RegisteredRegexes.Add(regex.Original);
				value.DefaultRegexes.Add(regex);
			}
		}
		else if (!_registeredRegexes.Contains(regex.Original))
		{
			_registeredRegexes.Add(regex.Original);
			_defaultRegexes.Add(regex);
		}
	}

	private bool HasTranslated(string key, int scope, bool checkAll)
	{
		if (checkAll)
		{
			if (!_translations.ContainsKey(key))
			{
				if (scope != -1 && _scopedTranslations.TryGetValue(scope, out var value))
				{
					return value.Translations.ContainsKey(key);
				}
				return false;
			}
			return true;
		}
		if (scope != -1)
		{
			if (scope != -1 && _scopedTranslations.TryGetValue(scope, out var value2))
			{
				return value2.Translations.ContainsKey(key);
			}
			return false;
		}
		return _translations.ContainsKey(key);
	}

	private bool IsTranslation(string translation, int scope)
	{
		if (HasTranslated(translation, scope, checkAll: true))
		{
			return false;
		}
		if (_reverseTranslations.ContainsKey(translation))
		{
			return true;
		}
		if (scope != -1 && _scopedTranslations.TryGetValue(scope, out var value) && value.ReverseTranslations.ContainsKey(translation))
		{
			return true;
		}
		return false;
	}

	private bool IsTokenTranslation(string translation, int scope)
	{
		if (_reverseTokenTranslations.ContainsKey(translation))
		{
			return true;
		}
		if (scope != -1 && _scopedTranslations.TryGetValue(scope, out var value) && value.ReverseTokenTranslations.ContainsKey(translation))
		{
			return true;
		}
		return false;
	}

	private void AddTranslation(string key, string value, int scope)
	{
		if (key == null || value == null)
		{
			return;
		}
		if (scope != -1)
		{
			if (!_scopedTranslations.TryGetValue(scope, out var value2))
			{
				value2 = new TranslationDictionaries();
				_scopedTranslations.Add(scope, value2);
			}
			value2.Translations[key] = value;
			value2.ReverseTranslations[value] = key;
		}
		else
		{
			_translations[key] = value;
			_reverseTranslations[value] = key;
		}
	}

	private void AddTokenTranslation(string key, string value, int scope)
	{
		if (key == null || value == null)
		{
			return;
		}
		if (scope != -1)
		{
			if (!_scopedTranslations.TryGetValue(scope, out var value2))
			{
				value2 = new TranslationDictionaries();
				_scopedTranslations.Add(scope, value2);
			}
			value2.TokenTranslations[key] = value;
			value2.ReverseTokenTranslations[value] = key;
		}
		else
		{
			_tokenTranslations[key] = value;
			_reverseTokenTranslations[value] = key;
		}
	}

	private void QueueNewTranslationForDisk(string key, string value)
	{
		lock (_writeToFileSync)
		{
			_newTranslations[key] = value;
		}
	}

	internal void AddTranslationToCache(string key, string value, bool persistToDisk, TranslationType type, int scope)
	{
		if ((type & TranslationType.Token) == TranslationType.Token && !persistToDisk)
		{
			AddTokenTranslation(key, value, scope);
		}
		if (!((type & TranslationType.Full) == TranslationType.Full || persistToDisk) || HasTranslated(key, scope, checkAll: false))
		{
			return;
		}
		AddTranslation(key, value, scope);
		UntranslatedText untranslatedText = new UntranslatedText(key, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
		UntranslatedText untranslatedText2 = new UntranslatedText(value, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.ToLanguageUsesWhitespaceBetweenWords, enableTemplating: true, Settings.TemplateAllNumberAway);
		if (untranslatedText.Original_Text_ExternallyTrimmed != key && !HasTranslated(untranslatedText.Original_Text_ExternallyTrimmed, scope, checkAll: false))
		{
			AddTranslation(untranslatedText.Original_Text_ExternallyTrimmed, untranslatedText2.Original_Text_ExternallyTrimmed, scope);
		}
		if (untranslatedText.Original_Text_ExternallyTrimmed != untranslatedText.Original_Text_FullyTrimmed && !HasTranslated(untranslatedText.Original_Text_FullyTrimmed, scope, checkAll: false))
		{
			AddTranslation(untranslatedText.Original_Text_FullyTrimmed, untranslatedText2.Original_Text_FullyTrimmed, scope);
		}
		if (persistToDisk)
		{
			if (scope != -1)
			{
				XuaLogger.AutoTranslator.Error("Stored scoped translation to cache, even though this is not supported!");
			}
			QueueNewTranslationForDisk(key, value);
		}
	}

	public bool TryGetTranslationSplitter(string text, int scope, out Match match, out RegexTranslationSplitter splitter)
	{
		if (scope != -1 && _scopedTranslations.TryGetValue(scope, out var value) && value.SplitterRegexes.Count > 0)
		{
			for (int num = value.SplitterRegexes.Count - 1; num > -1; num--)
			{
				RegexTranslationSplitter regexTranslationSplitter = value.SplitterRegexes[num];
				try
				{
					match = regexTranslationSplitter.CompiledRegex.Match(text);
					splitter = regexTranslationSplitter;
					if (match.Success)
					{
						return true;
					}
				}
				catch (Exception e)
				{
					value.SplitterRegexes.RemoveAt(num);
					XuaLogger.AutoTranslator.Error(e, "Failed while attempting to replace or match text of splitter regex '" + regexTranslationSplitter.Original + "'. Removing that regex from the cache.");
				}
			}
		}
		for (int num2 = _splitterRegexes.Count - 1; num2 > -1; num2--)
		{
			RegexTranslationSplitter regexTranslationSplitter2 = _splitterRegexes[num2];
			try
			{
				match = regexTranslationSplitter2.CompiledRegex.Match(text);
				splitter = regexTranslationSplitter2;
				if (match.Success)
				{
					return true;
				}
			}
			catch (Exception e2)
			{
				_splitterRegexes.RemoveAt(num2);
				XuaLogger.AutoTranslator.Error(e2, "Failed while attempting to replace or match text of splitter regex '" + regexTranslationSplitter2.Original + "'. Removing that regex from the cache.");
			}
		}
		match = null;
		splitter = null;
		return false;
	}

	public bool TryGetTranslation(UntranslatedText key, bool allowRegex, bool allowToken, int scope, out string value)
	{
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		TranslationDictionaries value2 = null;
		bool flag;
		if (scope != -1 && _scopedTranslations.TryGetValue(scope, out value2))
		{
			if (allowToken && value2.TokenTranslations.Count > 0)
			{
				if (key.IsTemplated && !key.IsFromSpammingComponent)
				{
					string key2 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
					flag = value2.TokenTranslations.TryGetValue(key2, out value);
					if (flag)
					{
						return flag;
					}
					if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
					{
						key2 = text2 ?? (text2 = key.Untemplate(key.TemplatedOriginal_Text_InternallyTrimmed));
						flag = value2.TokenTranslations.TryGetValue(key2, out value);
						if (flag)
						{
							return flag;
						}
					}
				}
				flag = value2.TokenTranslations.TryGetValue(key.TemplatedOriginal_Text, out value);
				if (flag)
				{
					return flag;
				}
				if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
				{
					flag = value2.TokenTranslations.TryGetValue(key.TemplatedOriginal_Text_InternallyTrimmed, out value);
					if (flag)
					{
						return flag;
					}
				}
			}
			if (key.IsTemplated && !key.IsFromSpammingComponent && value2.Translations.Count > 0)
			{
				string key2 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
				flag = value2.Translations.TryGetValue(key2, out value);
				if (flag)
				{
					return flag;
				}
				if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_ExternallyTrimmed)
				{
					key2 = text3 ?? (text3 = key.Untemplate(key.TemplatedOriginal_Text_ExternallyTrimmed));
					flag = value2.Translations.TryGetValue(key2, out value);
					if (flag)
					{
						string text5 = key.LeadingWhitespace + value + key.TrailingWhitespace;
						string text6 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
						if (!Settings.EnableSilentMode)
						{
							XuaLogger.AutoTranslator.Info("Whitespace difference (c1): '" + text6 + "' => '" + text5 + "'");
						}
						AddTranslationToCache(text6, text5, persistToDisk: false, TranslationType.Full, scope);
						value = text5;
						return flag;
					}
				}
				if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
				{
					key2 = text2 ?? (text2 = key.Untemplate(key.TemplatedOriginal_Text_InternallyTrimmed));
					flag = value2.Translations.TryGetValue(key2, out value);
					if (flag)
					{
						string text5 = value;
						string text6 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
						if (!Settings.EnableSilentMode)
						{
							XuaLogger.AutoTranslator.Info("Whitespace difference (c2): '" + text6 + "' => '" + text5 + "'");
						}
						AddTranslationToCache(text6, text5, persistToDisk: false, TranslationType.Full, scope);
						value = text5;
						return flag;
					}
				}
				if ((object)key.TemplatedOriginal_Text_InternallyTrimmed != key.TemplatedOriginal_Text_FullyTrimmed)
				{
					key2 = text4 ?? (text4 = key.Untemplate(key.TemplatedOriginal_Text_FullyTrimmed));
					flag = value2.Translations.TryGetValue(key2, out value);
					if (flag)
					{
						string text5 = key.LeadingWhitespace + value + key.TrailingWhitespace;
						string text6 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
						if (!Settings.EnableSilentMode)
						{
							XuaLogger.AutoTranslator.Info("Whitespace difference (c3): '" + text6 + "' => '" + text5 + "'");
						}
						AddTranslationToCache(text6, text5, persistToDisk: false, TranslationType.Full, scope);
						value = text5;
						return flag;
					}
				}
			}
			if (value2.Translations.Count > 0)
			{
				flag = value2.Translations.TryGetValue(key.TemplatedOriginal_Text, out value);
				if (flag)
				{
					return flag;
				}
				if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_ExternallyTrimmed)
				{
					flag = value2.Translations.TryGetValue(key.TemplatedOriginal_Text_ExternallyTrimmed, out value);
					if (flag)
					{
						string text5 = key.LeadingWhitespace + value + key.TrailingWhitespace;
						if (!Settings.EnableSilentMode)
						{
							XuaLogger.AutoTranslator.Info("Whitespace difference (c7): '" + key.TemplatedOriginal_Text + "' => '" + text5 + "'");
						}
						AddTranslationToCache(key.TemplatedOriginal_Text, text5, persistToDisk: false, TranslationType.Full, scope);
						value = text5;
						return flag;
					}
				}
				if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
				{
					flag = value2.Translations.TryGetValue(key.TemplatedOriginal_Text_InternallyTrimmed, out value);
					if (flag)
					{
						if (!Settings.EnableSilentMode)
						{
							XuaLogger.AutoTranslator.Info("Whitespace difference (c8): '" + key.TemplatedOriginal_Text + "' => '" + value + "'");
						}
						AddTranslationToCache(key.TemplatedOriginal_Text, value, persistToDisk: false, TranslationType.Full, scope);
						return flag;
					}
				}
				if ((object)key.TemplatedOriginal_Text_InternallyTrimmed != key.TemplatedOriginal_Text_FullyTrimmed)
				{
					flag = value2.Translations.TryGetValue(key.TemplatedOriginal_Text_FullyTrimmed, out value);
					if (flag)
					{
						string text5 = key.LeadingWhitespace + value + key.TrailingWhitespace;
						if (!Settings.EnableSilentMode)
						{
							XuaLogger.AutoTranslator.Info("Whitespace difference (c9): '" + key.TemplatedOriginal_Text + "' => '" + text5 + "'");
						}
						AddTranslationToCache(key.TemplatedOriginal_Text, text5, persistToDisk: false, TranslationType.Full, scope);
						value = text5;
						return flag;
					}
				}
			}
		}
		if (allowToken && _tokenTranslations.Count > 0)
		{
			if (key.IsTemplated && !key.IsFromSpammingComponent)
			{
				string key2 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
				flag = _tokenTranslations.TryGetValue(key2, out value);
				if (flag)
				{
					return flag;
				}
				if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
				{
					key2 = text2 ?? (text2 = key.Untemplate(key.TemplatedOriginal_Text_InternallyTrimmed));
					flag = _tokenTranslations.TryGetValue(key2, out value);
					if (flag)
					{
						return flag;
					}
				}
			}
			flag = _tokenTranslations.TryGetValue(key.TemplatedOriginal_Text, out value);
			if (flag)
			{
				return flag;
			}
			if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
			{
				flag = _tokenTranslations.TryGetValue(key.TemplatedOriginal_Text_InternallyTrimmed, out value);
				if (flag)
				{
					return flag;
				}
			}
		}
		if (key.IsTemplated && !key.IsFromSpammingComponent)
		{
			string key2 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
			flag = _translations.TryGetValue(key2, out value);
			if (flag)
			{
				return flag;
			}
			if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_ExternallyTrimmed)
			{
				key2 = text3 ?? (text3 = key.Untemplate(key.TemplatedOriginal_Text_ExternallyTrimmed));
				flag = _translations.TryGetValue(key2, out value);
				if (flag)
				{
					string text5 = key.LeadingWhitespace + value + key.TrailingWhitespace;
					string text6 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
					if (!Settings.EnableSilentMode)
					{
						XuaLogger.AutoTranslator.Info("Whitespace difference (c4): '" + text6 + "' => '" + text5 + "'");
					}
					AddTranslationToCache(text6, text5, Settings.CacheWhitespaceDifferences, TranslationType.Full, -1);
					value = text5;
					return flag;
				}
			}
			if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
			{
				key2 = text2 ?? (text2 = key.Untemplate(key.TemplatedOriginal_Text_InternallyTrimmed));
				flag = _translations.TryGetValue(key2, out value);
				if (flag)
				{
					string text5 = value;
					string text6 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
					if (!Settings.EnableSilentMode)
					{
						XuaLogger.AutoTranslator.Info("Whitespace difference (c5): '" + text6 + "' => '" + text5 + "'");
					}
					AddTranslationToCache(text6, text5, Settings.CacheWhitespaceDifferences, TranslationType.Full, -1);
					value = text5;
					return flag;
				}
			}
			if ((object)key.TemplatedOriginal_Text_InternallyTrimmed != key.TemplatedOriginal_Text_FullyTrimmed)
			{
				key2 = text4 ?? (text4 = key.Untemplate(key.TemplatedOriginal_Text_FullyTrimmed));
				flag = _translations.TryGetValue(key2, out value);
				if (flag)
				{
					string text5 = key.LeadingWhitespace + value + key.TrailingWhitespace;
					string text6 = text ?? (text = key.Untemplate(key.TemplatedOriginal_Text));
					if (!Settings.EnableSilentMode)
					{
						XuaLogger.AutoTranslator.Info("Whitespace difference (c6): '" + text6 + "' => '" + text5 + "'");
					}
					AddTranslationToCache(text6, text5, Settings.CacheWhitespaceDifferences, TranslationType.Full, -1);
					value = text5;
					return flag;
				}
			}
		}
		flag = _translations.TryGetValue(key.TemplatedOriginal_Text, out value);
		if (flag)
		{
			return flag;
		}
		if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_ExternallyTrimmed)
		{
			flag = _translations.TryGetValue(key.TemplatedOriginal_Text_ExternallyTrimmed, out value);
			if (flag)
			{
				string text5 = key.LeadingWhitespace + value + key.TrailingWhitespace;
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Info("Whitespace difference (c10): '" + key.TemplatedOriginal_Text + "' => '" + text5 + "'");
				}
				AddTranslationToCache(key.TemplatedOriginal_Text, text5, Settings.CacheWhitespaceDifferences, TranslationType.Full, -1);
				value = text5;
				return flag;
			}
		}
		if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
		{
			flag = _translations.TryGetValue(key.TemplatedOriginal_Text_InternallyTrimmed, out value);
			if (flag)
			{
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Info("Whitespace difference (c11): '" + key.TemplatedOriginal_Text + "' => '" + value + "'");
				}
				AddTranslationToCache(key.TemplatedOriginal_Text, value, Settings.CacheWhitespaceDifferences, TranslationType.Full, -1);
				return flag;
			}
		}
		if ((object)key.TemplatedOriginal_Text_InternallyTrimmed != key.TemplatedOriginal_Text_FullyTrimmed)
		{
			flag = _translations.TryGetValue(key.TemplatedOriginal_Text_FullyTrimmed, out value);
			if (flag)
			{
				string text5 = key.LeadingWhitespace + value + key.TrailingWhitespace;
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Info("Whitespace difference (c12): '" + key.TemplatedOriginal_Text + "' => '" + text5 + "'");
				}
				AddTranslationToCache(key.TemplatedOriginal_Text, text5, Settings.CacheWhitespaceDifferences, TranslationType.Full, -1);
				value = text5;
				return flag;
			}
		}
		if (allowRegex)
		{
			if (value2 != null && value2.DefaultRegexes.Count > 0 && !value2.FailedRegexLookups.Contains(key.TemplatedOriginal_Text))
			{
				for (int num = value2.DefaultRegexes.Count - 1; num > -1; num--)
				{
					RegexTranslation regexTranslation = value2.DefaultRegexes[num];
					try
					{
						Match match = regexTranslation.CompiledRegex.Match(key.TemplatedOriginal_Text);
						if (match.Success)
						{
							value = match.Result(regexTranslation.Translation);
							value = RomanizationHelper.PostProcess(value, Settings.RegexPostProcessing);
							if (!Settings.EnableSilentMode)
							{
								XuaLogger.AutoTranslator.Info("Regex lookup: '" + key.TemplatedOriginal_Text + "' => '" + value + "'");
							}
							AddTranslationToCache(key.TemplatedOriginal_Text, value, persistToDisk: false, TranslationType.Full, scope);
							return true;
						}
					}
					catch (Exception e)
					{
						value2.DefaultRegexes.RemoveAt(num);
						XuaLogger.AutoTranslator.Error(e, "Failed while attempting to replace or match text of regex '" + regexTranslation.Original + "'. Removing that regex from the cache.");
					}
				}
				if (value2.FailedRegexLookups.Add(key.TemplatedOriginal_Text) && value2.FailedRegexLookups.Count > 10000)
				{
					value2.FailedRegexLookups = new HashSet<string>();
				}
			}
			if (!_failedRegexLookups.Contains(key.TemplatedOriginal_Text))
			{
				for (int num2 = _defaultRegexes.Count - 1; num2 > -1; num2--)
				{
					RegexTranslation regexTranslation2 = _defaultRegexes[num2];
					try
					{
						Match match2 = regexTranslation2.CompiledRegex.Match(key.TemplatedOriginal_Text);
						if (match2.Success)
						{
							value = match2.Result(regexTranslation2.Translation);
							value = RomanizationHelper.PostProcess(value, Settings.RegexPostProcessing);
							if (!Settings.EnableSilentMode)
							{
								XuaLogger.AutoTranslator.Info("Regex lookup: '" + key.TemplatedOriginal_Text + "' => '" + value + "'");
							}
							AddTranslationToCache(key.TemplatedOriginal_Text, value, Settings.CacheRegexLookups, TranslationType.Full, -1);
							return true;
						}
					}
					catch (Exception e2)
					{
						_defaultRegexes.RemoveAt(num2);
						XuaLogger.AutoTranslator.Error(e2, "Failed while attempting to replace or match text of regex '" + regexTranslation2.Original + "'. Removing that regex from the cache.");
					}
				}
				if (_failedRegexLookups.Add(key.TemplatedOriginal_Text) && _failedRegexLookups.Count > 10000)
				{
					_failedRegexLookups = new HashSet<string>();
				}
			}
		}
		if (_staticTranslations.Count > 0)
		{
			flag = _staticTranslations.TryGetValue(key.TemplatedOriginal_Text, out value);
			if (flag)
			{
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Info("Static lookup: '" + key.TemplatedOriginal_Text + "' => '" + value + "'");
				}
				AddTranslationToCache(key.TemplatedOriginal_Text, value, persistToDisk: true, TranslationType.Full, -1);
				return flag;
			}
			if ((object)key.TemplatedOriginal_Text != key.TemplatedOriginal_Text_InternallyTrimmed)
			{
				flag = _staticTranslations.TryGetValue(key.TemplatedOriginal_Text_InternallyTrimmed, out value);
				if (flag)
				{
					if (!Settings.EnableSilentMode)
					{
						XuaLogger.AutoTranslator.Info("Static lookup: '" + key.TemplatedOriginal_Text_InternallyTrimmed + "' => '" + value + "'");
					}
					AddTranslationToCache(key.TemplatedOriginal_Text, value, persistToDisk: true, TranslationType.Full, -1);
					return flag;
				}
			}
		}
		return flag;
	}

	internal bool TryGetReverseTranslation(string value, int scope, out string key)
	{
		if (scope == -1 || !_scopedTranslations.TryGetValue(scope, out var value2) || !value2.ReverseTranslations.TryGetValue(value, out key))
		{
			return _reverseTranslations.TryGetValue(value, out key);
		}
		return true;
	}

	public bool IsTranslatable(string text, bool isToken, int scope)
	{
		bool flag = !IsTranslation(text, scope);
		if (isToken && flag)
		{
			flag = !IsTokenTranslation(text, scope);
		}
		return flag;
	}

	public bool IsPartial(string text, int scope)
	{
		return _partialTranslations.Contains(text);
	}

	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_fileWatcher?.Dispose();
				_fileWatcher = null;
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
