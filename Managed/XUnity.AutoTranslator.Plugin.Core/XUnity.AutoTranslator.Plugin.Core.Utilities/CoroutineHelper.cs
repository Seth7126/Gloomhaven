using System.Collections;
using UnityEngine;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities;

public static class CoroutineHelper
{
	public static WaitForSeconds CreateWaitForSeconds(float seconds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new WaitForSeconds(seconds);
	}

	public static object CreateWaitForSecondsRealtime(float delay)
	{
		if (UnityFeatures.SupportsWaitForSecondsRealtime)
		{
			return GetWaitForSecondsRealtimeInternal(delay);
		}
		return null;
	}

	private static object GetWaitForSecondsRealtimeInternal(float delay)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return (object)new WaitForSecondsRealtime(delay);
	}

	public static Coroutine Start(IEnumerator coroutine)
	{
		return PluginLoader.MonoBehaviour.StartCoroutine(coroutine);
	}

	public static void Stop(Coroutine coroutine)
	{
		PluginLoader.MonoBehaviour.StopCoroutine(coroutine);
	}
}
