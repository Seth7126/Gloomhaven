using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize;

internal class UIResizeAttachment
{
	private static readonly char[] CommandSplitters;

	private static readonly char[] PathSplitters;

	private static readonly char[] ArgSplitters;

	private static Regex CommandRegex;

	private static Dictionary<string, Type> CommandTypes;

	public Dictionary<string, UIResizeAttachment> Descendants { get; }

	public Dictionary<int, UIResizeResult> ScopedResults { get; }

	public UIResizeResult Result { get; private set; }

	static UIResizeAttachment()
	{
		CommandSplitters = new char[1] { ';' };
		PathSplitters = new char[1] { '/' };
		ArgSplitters = new char[2] { ',', ' ' };
		CommandRegex = new Regex("^\\s*(.+)\\s*\\(([\\s\\S]*)\\)\\s*$", AutoTranslationPlugin.RegexCompiledSupportedFlag);
		CommandTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		try
		{
			Type[] array = new Type[10]
			{
				typeof(ChangeFontSize),
				typeof(ChangeFontSizeByPercentage),
				typeof(IgnoreFontSize),
				typeof(AutoResize),
				typeof(UGUI_ChangeLineSpacing),
				typeof(UGUI_ChangeLineSpacingByPercentage),
				typeof(UGUI_HorizontalOverflow),
				typeof(UGUI_VerticalOverflow),
				typeof(TMP_Overflow),
				typeof(TMP_Alignment)
			};
			foreach (Type type in array)
			{
				CommandTypes[type.Name] = type;
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while loading ui resize commands.");
		}
	}

	public UIResizeAttachment()
	{
		Descendants = new Dictionary<string, UIResizeAttachment>();
		Result = new UIResizeResult();
		ScopedResults = new Dictionary<int, UIResizeResult>();
	}

	public bool AddResizeCommand(string path, string commands, int scope)
	{
		string[] segments = path.Split(PathSplitters, StringSplitOptions.RemoveEmptyEntries);
		UIResizeAttachment orCreateAttachment = GetOrCreateAttachment(segments);
		string[] array = commands.Split(CommandSplitters, StringSplitOptions.RemoveEmptyEntries);
		bool result = false;
		string[] array2 = array;
		foreach (string text in array2)
		{
			Match match = CommandRegex.Match(text);
			if (!match.Success)
			{
				XuaLogger.AutoTranslator.Warn("Could not understand command: " + text);
				continue;
			}
			try
			{
				string value = match.Groups[1].Value;
				string[] array3 = match.Groups[2].Value.Split(ArgSplitters, StringSplitOptions.RemoveEmptyEntries);
				UIResizeResult uIResizeResult = ((scope == -1) ? orCreateAttachment.Result : orCreateAttachment.GetOrCreateResultFor(scope));
				if (CommandTypes.TryGetValue(value, out var value2))
				{
					object obj = Activator.CreateInstance(value2, new object[1] { array3 });
					if (obj is IFontResizeCommand resizeCommand)
					{
						uIResizeResult.ResizeCommand = resizeCommand;
						uIResizeResult.IsResizeCommandScoped = scope != -1;
					}
					if (obj is IFontAutoResizeCommand autoResizeCommand)
					{
						uIResizeResult.AutoResizeCommand = autoResizeCommand;
						uIResizeResult.IsAutoResizeCommandScoped = scope != -1;
					}
					if (obj is IUGUI_LineSpacingCommand lineSpacingCommand)
					{
						uIResizeResult.LineSpacingCommand = lineSpacingCommand;
						uIResizeResult.IsLineSpacingCommandScoped = scope != -1;
					}
					if (obj is IUGUI_HorizontalOverflow horizontalOverflowCommand)
					{
						uIResizeResult.HorizontalOverflowCommand = horizontalOverflowCommand;
						uIResizeResult.IsHorizontalOverflowCommandScoped = scope != -1;
					}
					if (obj is IUGUI_VerticalOverflow verticalOverflowCommand)
					{
						uIResizeResult.VerticalOverflowCommand = verticalOverflowCommand;
						uIResizeResult.IsVerticalOverflowCommandScoped = scope != -1;
					}
					if (obj is ITMP_OverflowMode overflowCommand)
					{
						uIResizeResult.OverflowCommand = overflowCommand;
						uIResizeResult.IsOverflowCommandScoped = scope != -1;
					}
					if (obj is ITMP_Alignment alignmentCommand)
					{
						uIResizeResult.AlignmentCommand = alignmentCommand;
						uIResizeResult.IsAlignmentCommandScoped = scope != -1;
					}
					result = true;
					continue;
				}
				throw new ArgumentException("Unknown command: " + value);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while creating UI resize command.");
			}
		}
		return result;
	}

	private UIResizeResult GetOrCreateResultFor(int scope)
	{
		if (!ScopedResults.TryGetValue(scope, out var value))
		{
			value = new UIResizeResult();
			ScopedResults[scope] = value;
		}
		return value;
	}

	private UIResizeAttachment GetOrCreateAttachment(string[] segments)
	{
		UIResizeAttachment uIResizeAttachment = this;
		int num = segments.Length;
		for (int i = 0; i < num; i++)
		{
			string key = segments[i];
			if (!uIResizeAttachment.Descendants.TryGetValue(key, out var value))
			{
				value = new UIResizeAttachment();
				uIResizeAttachment.Descendants[key] = value;
			}
			uIResizeAttachment = value;
		}
		return uIResizeAttachment;
	}

	public void Trim()
	{
		if (Result != null && Result.IsEmpty())
		{
			Result = null;
		}
		foreach (UIResizeAttachment value in Descendants.Values)
		{
			value.Trim();
		}
	}

	public bool TryGetUIResize(string[] segments, int startIndex, int scope, out UIResizeResult result)
	{
		UIResizeAttachment uIResizeAttachment = this;
		result = null;
		int num = segments.Length;
		for (int i = startIndex; i < num; i++)
		{
			string key = segments[i];
			if (uIResizeAttachment.Descendants.TryGetValue(key, out var value))
			{
				if (result == null)
				{
					result = value.Result?.Copy();
				}
				else
				{
					result.MergeInto(value.Result);
				}
				if (scope != -1)
				{
					UIResizeResult value3;
					if (result == null)
					{
						if (value.ScopedResults.TryGetValue(scope, out var value2))
						{
							result = value2.Copy();
						}
					}
					else if (value.ScopedResults.TryGetValue(scope, out value3))
					{
						result.MergeInto(value3);
					}
				}
				uIResizeAttachment = value;
				continue;
			}
			return result != null;
		}
		return result != null;
	}
}
