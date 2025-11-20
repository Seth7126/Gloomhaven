using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

internal static class UtageHelper
{
	private static object AdvManager;

	private static HashSet<string> Labels = new HashSet<string>();

	public static void FixLabel(ref string label)
	{
		object[] index = new object[0];
		if (AdvManager == null && UnityTypes.AdvDataManager != null)
		{
			try
			{
				AdvManager = Object.FindObjectOfType(UnityTypes.AdvDataManager.UnityType);
				UnityTypes.AdvDataManager.ClrType.GetProperty("ScenarioDataTbl").GetValue(AdvManager, index).TryCastTo<IEnumerable>(out var castedObject);
				foreach (object item3 in castedObject)
				{
					Type type = item3.GetType();
					string item = (string)type.GetProperty("Key").GetValue(item3, index);
					Labels.Add(item);
					object value = type.GetProperty("Value").GetValue(item3, index);
					if (value == null)
					{
						continue;
					}
					value.GetType().GetProperty("ScenarioLabels").GetValue(value, index)
						.TryCastTo<IEnumerable>(out var castedObject2);
					foreach (object item4 in castedObject2)
					{
						string item2 = (string)item4.GetType().GetProperty("Key").GetValue(item4, index);
						Labels.Add(item2);
					}
				}
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Warn(e, "An error occurred while setting up scenario set.");
			}
		}
		if (!Labels.Contains(label))
		{
			int scope = TranslationScopeHelper.GetScope(null);
			if (AutoTranslationPlugin.Current.TextCache.TryGetReverseTranslation(label, scope, out var key))
			{
				label = key;
			}
		}
	}
}
