using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils;
using UnityEngine;

namespace AsmodeeNet.Foundation.Localization;

public class LocalizationManager
{
	[Serializable]
	public enum Language
	{
		unknown,
		zh_CN,
		zh_CHS,
		zh_CHT,
		nl_NL,
		en_US,
		fr_FR,
		de_DE,
		it_IT,
		ja_JP,
		ko_KR,
		pt_PT,
		ru_RU,
		es_ES,
		sv_SE,
		pl_PL
	}

	public delegate void LocalizationManagerInitialized();

	private const string _kModuleName = "LocalizationManager";

	private const string _kCurrentLanguagePreferenceKey = "CurrentLanguage";

	private const Language _kDefaultLanguage = Language.en_US;

	private Language? _currentLanguage;

	private bool _useDefaultLanguage;

	private List<Language> _supportedLanguages;

	public readonly Dictionary<SystemLanguage, Language> UnityLanguageToIsoLanguage = new Dictionary<SystemLanguage, Language>
	{
		{
			SystemLanguage.Chinese,
			Language.zh_CN
		},
		{
			SystemLanguage.ChineseSimplified,
			Language.zh_CHS
		},
		{
			SystemLanguage.ChineseTraditional,
			Language.zh_CHT
		},
		{
			SystemLanguage.Dutch,
			Language.nl_NL
		},
		{
			SystemLanguage.English,
			Language.en_US
		},
		{
			SystemLanguage.French,
			Language.fr_FR
		},
		{
			SystemLanguage.German,
			Language.de_DE
		},
		{
			SystemLanguage.Italian,
			Language.it_IT
		},
		{
			SystemLanguage.Japanese,
			Language.ja_JP
		},
		{
			SystemLanguage.Korean,
			Language.ko_KR
		},
		{
			SystemLanguage.Portuguese,
			Language.pt_PT
		},
		{
			SystemLanguage.Russian,
			Language.ru_RU
		},
		{
			SystemLanguage.Spanish,
			Language.es_ES
		},
		{
			SystemLanguage.Swedish,
			Language.sv_SE
		},
		{
			SystemLanguage.Polish,
			Language.pl_PL
		}
	};

	public TextAsset[] xliffFiles;

	private Dictionary<string, string> _keyToLocalizedText;

	private bool _ready;

	public static Language DefaultLanguage => Language.en_US;

	public Language CurrentLanguage
	{
		get
		{
			if (!_currentLanguage.HasValue)
			{
				return Language.en_US;
			}
			return _currentLanguage.Value;
		}
		set
		{
			if (_supportedLanguages.Contains(value))
			{
				_currentLanguage = value;
			}
			else
			{
				AsmoLogger.Warning("LocalizationManager", value.ToString() + " is not part of supported languages (Check CoreApplication > Supported Languages). Fall back to default: " + Language.en_US);
				_currentLanguage = Language.en_US;
			}
			_WritePreferredLanguage();
			_LoadLanguage();
			this.OnLanguageChanged?.Invoke(this);
		}
	}

	public string CurrentLanguageCode => CurrentLanguage.ToString().Substring(0, 2);

	public string DefaultLanguageCode => Language.en_US.ToString().Substring(0, 2);

	public event Action<LocalizationManager> OnLanguageChanged;

	public event LocalizationManagerInitialized LocalizationManagerInitializedEvent;

	public static Language GetLanguageFromString(string lang)
	{
		if (string.IsNullOrWhiteSpace(lang) || (lang.Length != 2 && lang.Length != 5))
		{
			return DefaultLanguage;
		}
		if (Enum.TryParse<Language>(lang, out var result))
		{
			return result;
		}
		string[] names = Enum.GetNames(typeof(Language));
		foreach (string text in names)
		{
			if (text.StartsWith(lang, StringComparison.Ordinal))
			{
				return (Language)Enum.Parse(typeof(Language), text);
			}
		}
		return DefaultLanguage;
	}

	public static string GetLanguageCode(Language l)
	{
		return l.ToString().Substring(0, 2);
	}

	public LocalizationManager(List<Language> supportedLanguages)
	{
		_supportedLanguages = supportedLanguages ?? new List<Language>();
		string text = "";
		foreach (Language supportedLanguage in _supportedLanguages)
		{
			text = text + supportedLanguage.ToString() + ", ";
		}
		AsmoLogger.Info("LocalizationManager", "Supported Languages (" + _supportedLanguages.Count + "): " + text);
	}

	public void Init()
	{
		if (KeyValueStore.Instance == null)
		{
			KeyValueStore.Instance = new PlayerPrefsKeyValueStore();
		}
		_LoadXliffFiles();
		_InitLanguage();
		this.LocalizationManagerInitializedEvent?.Invoke();
		this.OnLanguageChanged?.Invoke(this);
	}

	public bool HasTranslationInCurrentLanguage(string key)
	{
		if (_ready && !_useDefaultLanguage)
		{
			return _keyToLocalizedText.ContainsKey(key);
		}
		return false;
	}

	public string GetLocalizedText(string key)
	{
		if (!_ready)
		{
			return key;
		}
		if (_keyToLocalizedText.ContainsKey(key))
		{
			if (_keyToLocalizedText[key] == null)
			{
				AsmoLogger.Warning("LocalizationManager", "The key: \"" + key + "\" is defined but has no translation available");
				return key;
			}
			return _keyToLocalizedText[key];
		}
		AsmoLogger.Warning("LocalizationManager", "The key: \"" + key + "\" is not defined in the .xliff file and therefore doesn't have any localization");
		return key;
	}

	private void _WritePreferredLanguage()
	{
		string text = CurrentLanguage.ToString();
		AsmoLogger.Info("LocalizationManager", "_WritePreferredLanguage() Save language preference: " + text);
		KeyValueStore.SetString("CurrentLanguage", text);
		KeyValueStore.Save();
	}

	private Language? _ReadPreferredLanguage()
	{
		string text = KeyValueStore.GetString("CurrentLanguage");
		Debug.Log("[LocalizationManager] Just _ReadPreferredLanguage(). Result: " + text + ".");
		if (string.IsNullOrEmpty(text))
		{
			AsmoLogger.Warning("LocalizationManager", "_ReadPreferredLanguage() Couldn't find a saved language in PlayerPrefs");
		}
		AsmoLogger.Info("LocalizationManager", "Using saved language preference: " + text);
		return _GetLanguageFromString(text);
	}

	private Language? _GetSystemLanguage()
	{
		SystemLanguage systemLanguage = Application.systemLanguage;
		if (UnityLanguageToIsoLanguage.ContainsKey(systemLanguage))
		{
			Language value = UnityLanguageToIsoLanguage[systemLanguage];
			AsmoLogger.Info("LocalizationManager", "[LocalizationManager] _GetSystemLanguage() systemLanguage returned: " + systemLanguage.ToString() + " Using system language: " + value);
			return value;
		}
		AsmoLogger.Warning("LocalizationManager", "[LocalizationManager] _GetSystemLanguage() System language is not supported: " + systemLanguage);
		return null;
	}

	private void _LoadLanguage()
	{
		_ready = false;
		_keyToLocalizedText = null;
		Debug.Log($"[LocalizationManager] _LoadLanguage() called. Current Language: {CurrentLanguage.ToString()}, available localizations ({xliffFiles.Length}):");
		TextAsset[] array = xliffFiles;
		foreach (TextAsset textAsset in array)
		{
			Debug.Log($"===LocFile== Name: {textAsset.name}, Bytes: {textAsset.bytes}, Lang: {XliffUtility.GetXliffTargetLangFromXml(textAsset.text).ToString()}");
		}
		IEnumerable<TextAsset> enumerable = xliffFiles.Where((TextAsset x) => XliffUtility.GetXliffTargetLangFromXml(x.text) == CurrentLanguage);
		if (enumerable.Count() == 0)
		{
			_useDefaultLanguage = true;
			enumerable = xliffFiles.Where((TextAsset x) => XliffUtility.GetXliffTargetLangFromXml(x.text) == Language.en_US);
			AsmoLogger.Warning("LocalizationManager", $"No translation file for the language: {CurrentLanguage} has been found, English ({Language.en_US}) will be used instead");
		}
		else
		{
			_useDefaultLanguage = false;
		}
		foreach (TextAsset item in enumerable)
		{
			LocalizationDataModel localizationDataModel = LocalizationDataModel.CreateModelFromTextAsset(item);
			if (_keyToLocalizedText == null)
			{
				_keyToLocalizedText = localizationDataModel.Parse();
				continue;
			}
			Dictionary<string, string> dictionary = localizationDataModel.Parse();
			IEnumerable<string> enumerable2 = _keyToLocalizedText.Keys.Intersect(dictionary.Keys);
			if (enumerable2.Count() != 0)
			{
				AsmoLogger.Warning("LocalizationManager", "2 files managing the same \"target-language\" have duplicated keys. Keys are");
				foreach (string item2 in enumerable2)
				{
					AsmoLogger.Warning("LocalizationManager", item2);
				}
			}
			_keyToLocalizedText = _keyToLocalizedText.Union(dictionary, new CustomDictionnaryComparer()).ToDictionary((KeyValuePair<string, string> x) => x.Key, (KeyValuePair<string, string> x) => x.Value);
		}
		_ready = _keyToLocalizedText != null;
	}

	private void _LoadXliffFiles()
	{
		xliffFiles = Resources.LoadAll<TextAsset>("Localization/").ToArray();
		Debug.Log("[LocalizationManager] Loaded " + xliffFiles.Length + " localization xliffFiles.");
	}

	private void _InitLanguage()
	{
		if (!_currentLanguage.HasValue)
		{
			_currentLanguage = _ReadPreferredLanguage();
		}
		if (!_currentLanguage.HasValue)
		{
			_currentLanguage = _GetSystemLanguage();
		}
		if (!_currentLanguage.HasValue)
		{
			AsmoLogger.Warning("LocalizationManager", "Couldn't detect language, falling back on default: " + Language.en_US);
			_currentLanguage = Language.en_US;
		}
		_LoadLanguage();
	}

	private static Language _GetLanguageFromString(string lang)
	{
		Language result;
		try
		{
			result = (Language)Enum.Parse(typeof(Language), lang);
			Debug.Log("[LocalizationManager] _GetLanguageFromString(" + lang + ") parsed to " + result.ToString() + " successfully.");
		}
		catch
		{
			result = Language.en_US;
			Debug.LogWarning("[LocalizationManager] _GetLanguageFromString(" + lang + ") failed to parse. Returned " + result);
		}
		return result;
	}
}
