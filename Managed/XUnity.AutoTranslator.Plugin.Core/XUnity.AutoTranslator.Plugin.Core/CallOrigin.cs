using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core;

internal static class CallOrigin
{
	public static bool ImageHooksEnabled;

	public static bool ExpectsTextToBeReturned;

	public static IReadOnlyTextTranslationCache TextCache;

	private static readonly HashSet<Assembly> BreakingAssemblies;

	static CallOrigin()
	{
		ImageHooksEnabled = true;
		ExpectsTextToBeReturned = false;
		TextCache = null;
		BreakingAssemblies = new HashSet<Assembly>();
		try
		{
			BreakingAssemblies.AddRange(from x in AppDomain.CurrentDomain.GetAssemblies()
				where x.IsAssemblyCsharp() || x.IsAssemblyCsharpFirstpass()
				select x);
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while scanning for game assemblies.");
		}
	}

	internal static IReadOnlyTextTranslationCache GetTextCache(TextTranslationInfo info, TextTranslationCache generic)
	{
		if (info != null)
		{
			return info.TextCache ?? generic;
		}
		return TextCache ?? generic;
	}

	public static void AssociateSubHierarchyWithTransformInfo(Transform root, TransformInfo info)
	{
		Transform[] componentsInChildren = ((Component)root).GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetExtensionData(info);
		}
		((Component)root).gameObject.SetTextCacheForAllObjectsInHierachy(info.TextCache);
	}

	public static void SetTextCacheForAllObjectsInHierachy(this GameObject go, IReadOnlyTextTranslationCache cache)
	{
		try
		{
			foreach (Component allTextComponentsInChild in go.GetAllTextComponentsInChildren())
			{
				allTextComponentsInChild.CreateDerivedProxyIfRequiredAndPossible().GetOrCreateTextTranslationInfo().TextCache = cache;
			}
			TransformInfo t = new TransformInfo
			{
				TextCache = cache
			};
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetExtensionData(t);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while scanning object hierarchy for text components.");
		}
	}

	public static IReadOnlyTextTranslationCache CalculateTextCacheFromStackTrace(GameObject parent)
	{
		try
		{
			StackTrace stackTrace = new StackTrace(2);
			Dictionary<string, TextTranslationCache> pluginTextCaches = AutoTranslationPlugin.Current.PluginTextCaches;
			if (pluginTextCaches == null)
			{
				return null;
			}
			StackFrame[] frames = stackTrace.GetFrames();
			int num = frames.Length;
			for (int i = 0; i < num; i++)
			{
				MethodBase method = frames[i].GetMethod();
				if (method != null)
				{
					Assembly assembly = method.DeclaringType.Assembly;
					if (BreakingAssemblies.Contains(assembly))
					{
						break;
					}
					string name = assembly.GetName().Name;
					if (pluginTextCaches.TryGetValue(name, out var value))
					{
						return AutoTranslationPlugin.Current.TextCache.GetOrCreateCompositeCache(value);
					}
				}
			}
			if ((Object)(object)parent != (Object)null)
			{
				return parent.transform.GetExtensionData<TransformInfo>()?.TextCache;
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while calculating text translation cache from stack trace.");
		}
		return null;
	}
}
