using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class ClipboardHelper
{
	public static void CopyToClipboard(IEnumerable<string> lines, int maxCharacters)
	{
		List<string> list = lines.ToList();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			if (text.Length + stringBuilder.Length > maxCharacters)
			{
				break;
			}
			if (i == list.Count - 1)
			{
				stringBuilder.Append(text);
			}
			else
			{
				stringBuilder.AppendLine(text);
			}
		}
		CopyToClipboard(stringBuilder.ToString());
	}

	public static void CopyToClipboard(string text)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			TextEditor val = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			val.text = text;
			val.SelectAll();
			val.Copy();
		}
		catch (Exception e)
		{
			XuaLogger.Common.Error(e, "An error while copying text to clipboard.");
		}
	}
}
