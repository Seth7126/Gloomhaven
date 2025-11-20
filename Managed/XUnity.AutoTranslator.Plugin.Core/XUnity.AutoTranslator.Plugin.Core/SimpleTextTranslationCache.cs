using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core;

public class SimpleTextTranslationCache
{
	protected readonly Dictionary<string, string> _translations = new Dictionary<string, string>();

	private List<RegexTranslation> _defaultRegexes = new List<RegexTranslation>();

	private HashSet<string> _registeredRegexes = new HashSet<string>();

	private static object _writeToFileSync = new object();

	private Dictionary<string, string> _newTranslations = new Dictionary<string, string>();

	private Coroutine _currentScheduledTask;

	private bool _shouldOverrideEntireFile;

	public string LoadedFile { get; }

	public bool IsDirectory { get; }

	public virtual bool IsEmpty => _translations.Count + _defaultRegexes.Count == 0;

	public SimpleTextTranslationCache(string file, bool loadTranslationsInFile)
	{
		LoadedFile = file;
		IsDirectory = false;
		if (loadTranslationsInFile)
		{
			LoadTranslationFiles(overrideLaterWithEarlier: true);
		}
	}

	public SimpleTextTranslationCache(string fileOrDirectory, bool loadTranslationsInFile, bool isDirectory, bool allowTranslationOverride)
	{
		LoadedFile = fileOrDirectory;
		IsDirectory = isDirectory;
		if (loadTranslationsInFile)
		{
			LoadTranslationFiles(allowTranslationOverride);
		}
	}

	public SimpleTextTranslationCache(string outputFile, IEnumerable<Stream> inputStreams, bool allowTranslationOverride, bool closeStreams)
	{
		LoadedFile = outputFile;
		IsDirectory = false;
		if (inputStreams != null)
		{
			LoadTranslationStreams(inputStreams, allowTranslationOverride, closeStreams);
		}
	}

	internal void LoadTranslationFiles(bool overrideLaterWithEarlier)
	{
		try
		{
			if (IsDirectory)
			{
				foreach (string item in from x in Directory.GetFiles(LoadedFile, "*.txt")
					orderby x
					select x)
				{
					LoadTranslationsInFile(item, overrideLaterWithEarlier);
				}
				return;
			}
			LoadTranslationsInFile(LoadedFile, overrideLaterWithEarlier);
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while loading translations.");
		}
	}

	internal void LoadTranslationStreams(IEnumerable<Stream> streams, bool overrideLaterWithEarlier, bool closeStreams)
	{
		foreach (Stream stream in streams)
		{
			try
			{
				LoadTranslationsInStream(stream, overrideLaterWithEarlier, closeStreams);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while loading translations.");
			}
		}
	}

	private void LoadTranslationsInFile(string fullFileName, bool allowOverride)
	{
		if (File.Exists(fullFileName))
		{
			LoadTranslationsInStream(File.OpenRead(fullFileName), allowOverride, closeStream: true);
		}
	}

	private void LoadTranslationsInStream(Stream stream, bool allowOverride, bool closeStream)
	{
		try
		{
			string[] array = new StreamReader(stream, Encoding.UTF8).ReadToEnd().Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				try
				{
					string[] array2 = TextHelper.ReadTranslationLineAndDecode(text);
					if (array2 == null)
					{
						continue;
					}
					string text2 = array2[0];
					string text3 = array2[1];
					if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3) || !(text2 != text3))
					{
						continue;
					}
					if (text2.StartsWith("r:"))
					{
						try
						{
							RegexTranslation regex = new RegexTranslation(text2, text3);
							AddTranslationRegex(regex);
						}
						catch (Exception e)
						{
							XuaLogger.AutoTranslator.Warn(e, "An error occurred while constructing regex translation: '" + text + "'.");
						}
					}
					else
					{
						AddTranslationInternal(text2, text3.MakeRedirected(), allowOverride);
					}
				}
				catch (Exception e2)
				{
					XuaLogger.AutoTranslator.Warn(e2, "An error occurred while reading translation: '" + text + "'.");
				}
			}
		}
		finally
		{
			if (closeStream)
			{
				stream.Dispose();
			}
		}
	}

	private void AddTranslationRegex(RegexTranslation regex)
	{
		if (!_registeredRegexes.Contains(regex.Original))
		{
			_registeredRegexes.Add(regex.Original);
			_defaultRegexes.Add(regex);
		}
	}

	private void AddTranslationInternal(string key, string value, bool allowOverride)
	{
		if (key != null && value != null && (allowOverride || !HasTranslated(key)))
		{
			AddTranslation(key, value);
		}
	}

	protected virtual bool HasTranslated(string key)
	{
		return _translations.ContainsKey(key);
	}

	protected virtual void AddTranslation(string key, string value)
	{
		_translations[key] = value;
	}

	[Obsolete("Do not use. Function only exists in case someone calls it through reflection.")]
	private void AddTranslation(string key, string value, bool allowOverride)
	{
		if (key != null && value != null && (allowOverride || !HasTranslated(key)))
		{
			AddTranslation(key, value);
		}
	}

	public virtual void AddTranslationToCache(string key, string value)
	{
		bool flag = HasTranslated(key);
		if (!flag)
		{
			AddTranslationInternal(key, value.MakeRedirected(), allowOverride: false);
			QueueNewTranslationForDisk(key, value, flag);
		}
	}

	private void QueueNewTranslationForDisk(string key, string value, bool hadTranslated)
	{
		lock (_newTranslations)
		{
			_newTranslations[key] = value;
			if (hadTranslated)
			{
				_shouldOverrideEntireFile = true;
			}
			if (_currentScheduledTask != null)
			{
				CoroutineHelper.Stop(_currentScheduledTask);
			}
			_currentScheduledTask = CoroutineHelper.Start(ScheduleFileWriting());
		}
	}

	private IEnumerator ScheduleFileWriting()
	{
		yield return CoroutineHelper.CreateWaitForSeconds(1f);
		_currentScheduledTask = null;
		ThreadPool.QueueUserWorkItem(SaveNewTranslationsToDisk);
	}

	internal void SaveNewTranslationsToDisk(object state)
	{
		bool flag = false;
		if (_newTranslations.Count <= 0)
		{
			return;
		}
		Dictionary<string, string> dictionary;
		lock (_newTranslations)
		{
			dictionary = _newTranslations.ToDictionary((KeyValuePair<string, string> x) => x.Key, (KeyValuePair<string, string> x) => x.Value);
			_newTranslations.Clear();
			if (_shouldOverrideEntireFile)
			{
				flag = true;
				_shouldOverrideEntireFile = false;
				lock (_translations)
				{
					foreach (KeyValuePair<string, string> translation in _translations)
					{
						if (!dictionary.ContainsKey(translation.Key))
						{
							dictionary[translation.Key] = translation.Value;
						}
					}
					foreach (RegexTranslation defaultRegex in _defaultRegexes)
					{
						if (!dictionary.ContainsKey(defaultRegex.Key))
						{
							dictionary[defaultRegex.Key] = defaultRegex.Value;
						}
					}
				}
			}
		}
		lock (_writeToFileSync)
		{
			Directory.CreateDirectory(new FileInfo(LoadedFile).Directory.FullName);
			using FileStream stream = File.Open(LoadedFile, flag ? FileMode.Create : FileMode.Append, FileAccess.Write);
			using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				streamWriter.WriteLine(TextHelper.Encode(item.Key) + "=" + TextHelper.Encode(item.Value));
			}
			streamWriter.Flush();
		}
	}

	public virtual bool TryGetTranslation(string untranslatedText, bool allowRegex, out string value)
	{
		UntranslatedText untranslatedText2 = new UntranslatedText(untranslatedText, isFromSpammingComponent: false, removeInternalWhitespace: true, Settings.FromLanguageUsesWhitespaceBetweenWords, enableTemplating: false, Settings.TemplateAllNumberAway);
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		bool flag;
		lock (_translations)
		{
			if (untranslatedText2.IsTemplated && !untranslatedText2.IsFromSpammingComponent)
			{
				string key = text ?? (text = untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text));
				flag = _translations.TryGetValue(key, out value);
				if (flag)
				{
					return flag;
				}
				if ((object)untranslatedText2.TemplatedOriginal_Text != untranslatedText2.TemplatedOriginal_Text_ExternallyTrimmed)
				{
					key = text3 ?? (text3 = untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text_ExternallyTrimmed));
					flag = _translations.TryGetValue(key, out value);
					if (flag)
					{
						string text5 = untranslatedText2.LeadingWhitespace + value + untranslatedText2.TrailingWhitespace;
						if (text == null)
						{
							text = untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text);
						}
						value = text5;
						return flag;
					}
				}
				if ((object)untranslatedText2.TemplatedOriginal_Text != untranslatedText2.TemplatedOriginal_Text_InternallyTrimmed)
				{
					key = text2 ?? (text2 = untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text_InternallyTrimmed));
					flag = _translations.TryGetValue(key, out value);
					if (flag)
					{
						string text5 = value;
						if (text == null)
						{
							text = untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text);
						}
						value = text5;
						return flag;
					}
				}
				if ((object)untranslatedText2.TemplatedOriginal_Text_InternallyTrimmed != untranslatedText2.TemplatedOriginal_Text_FullyTrimmed)
				{
					key = text4 ?? (text4 = untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text_FullyTrimmed));
					flag = _translations.TryGetValue(key, out value);
					if (flag)
					{
						string text5 = untranslatedText2.LeadingWhitespace + value + untranslatedText2.TrailingWhitespace;
						if (text == null)
						{
							text = untranslatedText2.Untemplate(untranslatedText2.TemplatedOriginal_Text);
						}
						value = text5;
						return flag;
					}
				}
			}
			flag = _translations.TryGetValue(untranslatedText2.TemplatedOriginal_Text, out value);
			if (flag)
			{
				return flag;
			}
			if ((object)untranslatedText2.TemplatedOriginal_Text != untranslatedText2.TemplatedOriginal_Text_ExternallyTrimmed)
			{
				flag = _translations.TryGetValue(untranslatedText2.TemplatedOriginal_Text_ExternallyTrimmed, out value);
				if (flag)
				{
					string text5 = untranslatedText2.LeadingWhitespace + value + untranslatedText2.TrailingWhitespace;
					value = text5;
					return flag;
				}
			}
			if ((object)untranslatedText2.TemplatedOriginal_Text != untranslatedText2.TemplatedOriginal_Text_InternallyTrimmed)
			{
				flag = _translations.TryGetValue(untranslatedText2.TemplatedOriginal_Text_InternallyTrimmed, out value);
				if (flag)
				{
					return flag;
				}
			}
			if ((object)untranslatedText2.TemplatedOriginal_Text_InternallyTrimmed != untranslatedText2.TemplatedOriginal_Text_FullyTrimmed)
			{
				flag = _translations.TryGetValue(untranslatedText2.TemplatedOriginal_Text_FullyTrimmed, out value);
				if (flag)
				{
					string text5 = untranslatedText2.LeadingWhitespace + value + untranslatedText2.TrailingWhitespace;
					value = text5;
					return flag;
				}
			}
			if (allowRegex)
			{
				for (int num = _defaultRegexes.Count - 1; num > -1; num--)
				{
					RegexTranslation regexTranslation = _defaultRegexes[num];
					try
					{
						if (regexTranslation.CompiledRegex.Match(untranslatedText2.TemplatedOriginal_Text).Success)
						{
							value = regexTranslation.CompiledRegex.Replace(untranslatedText2.TemplatedOriginal_Text, regexTranslation.Translation).MakeRedirected();
							return true;
						}
					}
					catch (Exception e)
					{
						_defaultRegexes.RemoveAt(num);
						XuaLogger.AutoTranslator.Error(e, "Failed while attempting to replace or match text of regex '" + regexTranslation.Original + "'. Removing that regex from the cache.");
					}
				}
			}
		}
		return flag;
	}
}
