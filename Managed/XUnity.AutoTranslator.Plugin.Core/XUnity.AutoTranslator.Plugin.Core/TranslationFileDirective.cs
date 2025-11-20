using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace XUnity.AutoTranslator.Plugin.Core;

internal abstract class TranslationFileDirective
{
	public static TranslationFileDirective Create(string directive)
	{
		int num = directive.IndexOf("//", StringComparison.Ordinal);
		if (num > -1)
		{
			directive = directive.Substring(0, num);
		}
		directive = directive.Trim();
		if (directive.Length > 0 && directive[0] == '#')
		{
			string[] array = directive.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 2)
			{
				string text = array[0].ToLowerInvariant();
				string text2 = array[1];
				string argument = directive.Substring(directive.IndexOf(text2, StringComparison.Ordinal) + text2.Length).Trim();
				text2 = text2.ToLowerInvariant();
				switch (text)
				{
				case "#set":
					return CreateSetCommand(text2, argument);
				case "#unset":
					return CreateUnsetCommand(text2, argument);
				case "#enable":
					return CreateEnableCommand(text2, argument);
				}
			}
		}
		return null;
	}

	private static TranslationFileDirective CreateSetCommand(string setType, string argument)
	{
		return setType switch
		{
			"required-resolution" => new TranslationFileLoadingContext.SetRequiredResolutionTranslationFileDirective(argument), 
			"level" => new TranslationFileLoadingContext.SetLevelTranslationFileDirective(ParseCommaSeperatedListAsIntArray(argument)), 
			"exe" => new TranslationFileLoadingContext.SetExeTranslationFileDirective(ParseCommaSeperatedListAsStringArray(argument)), 
			_ => null, 
		};
	}

	private static TranslationFileDirective CreateUnsetCommand(string setType, string argument)
	{
		return setType switch
		{
			"required-resolution" => new TranslationFileLoadingContext.UnsetRequiredResolutionTranslationFileDirective(), 
			"level" => new TranslationFileLoadingContext.UnsetLevelTranslationFileDirective(ParseCommaSeperatedListAsIntArray(argument)), 
			"exe" => new TranslationFileLoadingContext.UnsetExeTranslationFileDirective(ParseCommaSeperatedListAsStringArray(argument)), 
			_ => null, 
		};
	}

	private static TranslationFileDirective CreateEnableCommand(string setType, string argument)
	{
		return new TranslationFileLoadingContext.EnableTranslationFileDirective(setType);
	}

	private static int[] ParseCommaSeperatedListAsIntArray(string argument)
	{
		if (string.IsNullOrEmpty(argument))
		{
			return new int[0];
		}
		List<int> list = new List<int>();
		string[] array = argument.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				list.Add(result);
			}
		}
		return list.ToArray();
	}

	private static string[] ParseCommaSeperatedListAsStringArray(string argument)
	{
		if (string.IsNullOrEmpty(argument))
		{
			return new string[0];
		}
		return (from x in argument.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
			select x.Trim()).ToArray();
	}

	public abstract void ModifyContext(TranslationFileLoadingContext context);
}
