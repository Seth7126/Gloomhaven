using System;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core;

public static class PluginLoader
{
	internal class Bootstrapper : MonoBehaviour
	{
		public event Action Destroyed = delegate
		{
		};

		private void Start()
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}

		private void OnDestroy()
		{
			this.Destroyed?.Invoke();
		}
	}

	internal static AutoTranslationPlugin Plugin;

	internal static MonoBehaviour MonoBehaviour;

	private static bool _loaded;

	private static bool _bootstrapped;

	public static IMonoBehaviour LoadWithConfig(IPluginEnvironment config)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!_loaded)
		{
			_loaded = true;
			PluginEnvironment.Current = config;
			GameObject val = new GameObject("___XUnityAutoTranslator")
			{
				hideFlags = (HideFlags)61
			};
			Plugin = val.AddComponent<AutoTranslationPlugin>();
			MonoBehaviour = (MonoBehaviour)(object)Plugin;
			Object.DontDestroyOnLoad((Object)val);
		}
		return Plugin;
	}

	public static IMonoBehaviour Load()
	{
		return LoadWithConfig(new DefaultPluginEnvironment(allowDefaultInitializeHarmonyDetourBridge: true));
	}

	public static IMonoBehaviour Load(bool allowDefaultInitializeHarmonyDetourBridge)
	{
		return LoadWithConfig(new DefaultPluginEnvironment(allowDefaultInitializeHarmonyDetourBridge));
	}

	public static void LoadThroughBootstrapper()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!_bootstrapped)
		{
			_bootstrapped = true;
			new GameObject("Bootstrapper").AddComponent<Bootstrapper>().Destroyed += Bootstrapper_Destroyed;
		}
	}

	private static void Bootstrapper_Destroyed()
	{
		Load();
	}
}
