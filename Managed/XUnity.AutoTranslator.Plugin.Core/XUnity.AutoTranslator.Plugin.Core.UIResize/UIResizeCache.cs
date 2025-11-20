using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize;

internal class UIResizeCache
{
	private UIResizeAttachment _root = new UIResizeAttachment();

	public bool HasAnyResizeCommands { get; private set; }

	private static IEnumerable<string> GetTranslationFiles()
	{
		return from x in Directory.GetFiles(Settings.TranslationsPath, "*.*", SearchOption.AllDirectories)
			where x.EndsWith("resizer.txt", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
			select x;
	}

	internal void LoadResizeCommandsInFiles()
	{
		try
		{
			Directory.CreateDirectory(Settings.TranslationsPath);
			Directory.CreateDirectory(Path.GetDirectoryName(Settings.AutoTranslationsFilePath));
			_root = new UIResizeAttachment();
			foreach (string item in GetTranslationFiles().Reverse())
			{
				LoadResizeCommandsInFile(item);
			}
			_root.Trim();
			XuaLogger.AutoTranslator.Debug("Loaded resize command text files.");
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while resize commands.");
		}
	}

	private void LoadResizeCommandsInStream(Stream stream, string fullFileName)
	{
		if (!Settings.EnableSilentMode)
		{
			XuaLogger.AutoTranslator.Debug("Loading resize commands: " + fullFileName + ".");
		}
		StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
		TranslationFileLoadingContext translationFileLoadingContext = new TranslationFileLoadingContext();
		string[] array = streamReader.ReadToEnd().Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			TranslationFileDirective translationFileDirective = TranslationFileDirective.Create(text);
			if (translationFileDirective != null)
			{
				translationFileLoadingContext.Apply(translationFileDirective);
				if (!Settings.EnableSilentMode)
				{
					XuaLogger.AutoTranslator.Debug("Directive in file: " + fullFileName + ": " + translationFileDirective.ToString());
				}
			}
			else
			{
				if (!translationFileLoadingContext.IsApplicable())
				{
					continue;
				}
				try
				{
					string[] array2 = TextHelper.ReadTranslationLineAndDecode(text);
					if (array2 == null)
					{
						continue;
					}
					string text2 = array2[0];
					string value = array2[1];
					if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(value))
					{
						continue;
					}
					HashSet<int> levels = translationFileLoadingContext.GetLevels();
					if (levels.Count == 0)
					{
						AddTranslation(text2, value, -1);
						continue;
					}
					foreach (int item in levels)
					{
						AddTranslation(text2, value, item);
					}
				}
				catch (Exception e)
				{
					XuaLogger.AutoTranslator.Warn(e, "An error occurred while reading UI resize directive: '" + text + "'.");
				}
			}
		}
	}

	private void LoadResizeCommandsInFile(string fullFileName)
	{
		if (!File.Exists(fullFileName))
		{
			return;
		}
		using FileStream fileStream = File.OpenRead(fullFileName);
		if (fullFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
		{
			using (ZipInputStream zipInputStream = new ZipInputStream(fileStream))
			{
				while (true)
				{
					ZipEntry nextEntry = zipInputStream.GetNextEntry();
					if (nextEntry == null)
					{
						break;
					}
					if (nextEntry.IsFile && nextEntry.Name.EndsWith("resizer.txt", StringComparison.OrdinalIgnoreCase))
					{
						LoadResizeCommandsInStream(zipInputStream, fullFileName + Path.DirectorySeparatorChar + nextEntry.Name);
					}
				}
				return;
			}
		}
		LoadResizeCommandsInStream(fileStream, fullFileName);
	}

	private void AddTranslation(string key, string value, int scope)
	{
		if (key != null && value != null)
		{
			bool flag = _root.AddResizeCommand(key, value, scope);
			HasAnyResizeCommands = flag || HasAnyResizeCommands;
		}
	}

	public bool TryGetUIResize(string[] paths, int scope, out UIResizeResult result)
	{
		return _root.TryGetUIResize(paths, 0, scope, out result);
	}
}
