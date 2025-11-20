using System;
using System.IO;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core;

internal static class SettingsTranslationsInitializer
{
	public static void LoadTranslations()
	{
		Settings.Replacements.Clear();
		Settings.Preprocessors.Clear();
		Directory.CreateDirectory(Settings.TranslationsPath);
		string fullName = new FileInfo(Settings.SubstitutionFilePath).FullName;
		string fullName2 = new FileInfo(Settings.PreprocessorsFilePath).FullName;
		string fullName3 = new FileInfo(Settings.PostprocessorsFilePath).FullName;
		LoadTranslationsInFile(fullName, isSubstitutionFile: true, isPreprocessorFile: false, isPostprocessorsFile: false);
		LoadTranslationsInFile(fullName2, isSubstitutionFile: false, isPreprocessorFile: true, isPostprocessorsFile: false);
		LoadTranslationsInFile(fullName3, isSubstitutionFile: false, isPreprocessorFile: false, isPostprocessorsFile: true);
	}

	private static void LoadTranslationsInFile(string fullFileName, bool isSubstitutionFile, bool isPreprocessorFile, bool isPostprocessorsFile)
	{
		if (File.Exists(fullFileName))
		{
			using (FileStream stream = File.OpenRead(fullFileName))
			{
				LoadTranslationsInStream(stream, fullFileName, isSubstitutionFile, isPreprocessorFile, isPostprocessorsFile);
				return;
			}
		}
		Directory.CreateDirectory(new FileInfo(fullFileName).Directory.FullName);
		using FileStream fileStream = File.Create(fullFileName);
		fileStream.Write(new byte[3] { 239, 187, 191 }, 0, 3);
		fileStream.Close();
	}

	private static void LoadTranslationsInStream(Stream stream, string fullFileName, bool isSubstitutionFile, bool isPreprocessorFile, bool isPostProcessorFile)
	{
		if (!Settings.EnableSilentMode)
		{
			XuaLogger.AutoTranslator.Debug("Loading texts: " + fullFileName + ".");
		}
		StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
		TranslationFileLoadingContext translationFileLoadingContext = new TranslationFileLoadingContext();
		string[] array = streamReader.ReadToEnd().Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
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
				if (string.IsNullOrEmpty(text2))
				{
					continue;
				}
				if (isSubstitutionFile)
				{
					if (!string.IsNullOrEmpty(value))
					{
						Settings.Replacements[text2] = value;
					}
				}
				else if (isPreprocessorFile)
				{
					Settings.Preprocessors[text2] = value;
				}
				else if (isPostProcessorFile)
				{
					Settings.Postprocessors[text2] = value;
				}
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Warn(e, "An error occurred while reading translation: '" + text + "'.");
			}
		}
	}
}
