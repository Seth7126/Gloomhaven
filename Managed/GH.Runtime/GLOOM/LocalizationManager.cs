using System;
using System.Collections.Generic;
using System.IO;
using I2.Loc;
using ScenarioRuleLibrary;
using UnityEngine;

namespace GLOOM;

public static class LocalizationManager
{
	public static string GetTranslation(string Term, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool skipWarnings = false, bool useDefaultIfMissing = false, bool returnNullIfNotFound = false)
	{
		if (string.IsNullOrEmpty(Term))
		{
			Debug.LogError("A null or empty term was sent to GetTranslation.\n" + Environment.StackTrace);
			return "UNDEFINED";
		}
		if (Term.StartsWith("$", StringComparison.Ordinal) && Term.EndsWith("$", StringComparison.Ordinal))
		{
			Term = CardLayoutRow.GetKey(Term, '$');
		}
		string term = Term?.ToLowerInvariant();
		string text = I2.Loc.LocalizationManager.GetTranslation(term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage);
		if (text == null)
		{
			if (useDefaultIfMissing && I2.Loc.LocalizationManager.CurrentLanguage != "English")
			{
				text = I2.Loc.LocalizationManager.GetTranslation(term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, "English");
			}
			if (text == null)
			{
				if (!skipWarnings)
				{
					Debug.LogError("GetTranslation term not found for " + Term);
				}
				if (returnNullIfNotFound)
				{
					return null;
				}
				text = "UNDEFINED <color=\"red\">" + Term;
			}
		}
		return text;
	}

	public static bool TryGetTranslation(string Term, out string Translation, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null)
	{
		Translation = null;
		if (Term.StartsWith("$", StringComparison.Ordinal) && Term.EndsWith("$", StringComparison.Ordinal))
		{
			Term = CardLayoutRow.GetKey(Term, '$');
		}
		if (I2.Loc.LocalizationManager.TryGetTranslation(Term?.ToLowerInvariant(), out Translation, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage))
		{
			return true;
		}
		return I2.Loc.LocalizationManager.TryGetTranslation(Term?.Replace(" ", string.Empty).Replace("'", string.Empty).ToLowerInvariant(), out Translation, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage);
	}

	public static bool AddCSVSource(string csv)
	{
		LanguageSourceData languageSourceData = ImportCSV(csv);
		if (languageSourceData != null)
		{
			I2.Loc.LocalizationManager.Sources.Add(languageSourceData);
			return true;
		}
		return false;
	}

	public static bool InsertCSVSource(string csv, int index = 0)
	{
		LanguageSourceData languageSourceData = ImportCSV(csv);
		if (languageSourceData != null)
		{
			I2.Loc.LocalizationManager.Sources.Insert(index, languageSourceData);
			return true;
		}
		return false;
	}

	public static LanguageSourceData ImportCSV(string csv)
	{
		LanguageSourceData languageSourceData = GenerateLanguageSourceSource();
		if (!(languageSourceData.Import_CSV(string.Empty, File.ReadAllText(csv), eSpreadsheetUpdateMode.Merge) == string.Empty))
		{
			return null;
		}
		return languageSourceData;
	}

	public static LanguageSourceData GenerateLanguageSourceSource()
	{
		return new LanguageSourceData
		{
			CaseInsensitiveTerms = true,
			mDictionary = new Dictionary<string, TermData>(StringComparer.OrdinalIgnoreCase)
		};
	}
}
