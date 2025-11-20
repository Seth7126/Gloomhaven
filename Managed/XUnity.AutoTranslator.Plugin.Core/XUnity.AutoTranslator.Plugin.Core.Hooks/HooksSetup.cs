using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class HooksSetup
{
	private static bool _textAssetHooksInstalled = false;

	private static bool _installedPluginTranslationHooks = false;

	private static HashSet<Assembly> _installedAssemblies = new HashSet<Assembly>();

	public static void InstallTextGetterCompatHooks()
	{
		try
		{
			if (Settings.TextGetterCompatibilityMode)
			{
				HookingHelper.PatchAll(TextGetterCompatHooks.All, Settings.ForceMonoModHooks);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while setting up text getter compat hooks.");
		}
	}

	public static void InstallImageHooks()
	{
		try
		{
			if (Settings.EnableTextureTranslation || Settings.EnableTextureDumping)
			{
				HookingHelper.PatchAll(ImageHooks.All, Settings.ForceMonoModHooks);
				if (Settings.EnableLegacyTextureLoading || Settings.EnableSpriteHooking)
				{
					HookingHelper.PatchAll(ImageHooks.Sprite, Settings.ForceMonoModHooks);
				}
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while setting up image hooks.");
		}
	}

	public static void InstallSpriteRendererHooks()
	{
		try
		{
			if (Settings.EnableSpriteRendererHooking && (Settings.EnableTextureTranslation || Settings.EnableTextureDumping))
			{
				HookingHelper.PatchAll(ImageHooks.SpriteRenderer, Settings.ForceMonoModHooks);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while setting up image hooks.");
		}
	}

	public static void InstallTextAssetHooks()
	{
		try
		{
			if (!_textAssetHooksInstalled)
			{
				_textAssetHooksInstalled = true;
				HookingHelper.PatchAll(TextAssetHooks.All, Settings.ForceMonoModHooks);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while setting up text asset hooks.");
		}
	}

	public static void InstallTextHooks()
	{
		try
		{
			if (Settings.EnableUGUI)
			{
				HookingHelper.PatchAll(UGUIHooks.All, Settings.ForceMonoModHooks);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while setting up hooks for UGUI.");
		}
		try
		{
			if (Settings.EnableTextMeshPro)
			{
				HookingHelper.PatchAll(TextMeshProHooks.All, Settings.ForceMonoModHooks);
				if (Settings.DisableTextMeshProScrollInEffects)
				{
					HookingHelper.PatchAll(TextMeshProHooks.DisableScrollInTmp, Settings.ForceMonoModHooks);
				}
			}
		}
		catch (Exception e2)
		{
			XuaLogger.AutoTranslator.Error(e2, "An error occurred while setting up hooks for TextMeshPro.");
		}
		try
		{
			if (Settings.EnableNGUI)
			{
				HookingHelper.PatchAll(NGUIHooks.All, Settings.ForceMonoModHooks);
			}
		}
		catch (Exception e3)
		{
			XuaLogger.AutoTranslator.Error(e3, "An error occurred while setting up hooks for NGUI.");
		}
		try
		{
			if (Settings.EnableIMGUI)
			{
				HookingHelper.PatchAll(IMGUIHooks.All, Settings.ForceMonoModHooks);
			}
		}
		catch (Exception e4)
		{
			XuaLogger.AutoTranslator.Error(e4, "An error occurred while setting up hooks for IMGUI.");
		}
		try
		{
			HookingHelper.PatchAll(UtageHooks.All, Settings.ForceMonoModHooks);
		}
		catch (Exception e5)
		{
			XuaLogger.AutoTranslator.Error(e5, "An error occurred while setting up hooks for Utage.");
		}
		try
		{
			if (Settings.EnableTextMesh)
			{
				HookingHelper.PatchAll(TextMeshHooks.All, Settings.ForceMonoModHooks);
			}
		}
		catch (Exception e6)
		{
			XuaLogger.AutoTranslator.Error(e6, "An error occurred while setting up hooks for TextMesh.");
		}
		try
		{
			if (Settings.EnableFairyGUI)
			{
				HookingHelper.PatchAll(FairyGUIHooks.All, Settings.ForceMonoModHooks);
			}
		}
		catch (Exception e7)
		{
			XuaLogger.AutoTranslator.Error(e7, "An error occurred while setting up hooks for FairyGUI.");
		}
	}

	public static void InstallComponentBasedPluginTranslationHooks()
	{
		if (AutoTranslationPlugin.Current.PluginTextCaches.Count > 0 && !_installedPluginTranslationHooks)
		{
			_installedPluginTranslationHooks = true;
			try
			{
				HookingHelper.PatchAll(PluginTranslationHooks.All, Settings.ForceMonoModHooks);
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while setting up hooks for Plugin translations.");
			}
		}
	}

	public static void InstallIMGUIBasedPluginTranslationHooks(Assembly assembly, bool final)
	{
		if (!Settings.EnableIMGUI || _installedAssemblies.Contains(assembly))
		{
			return;
		}
		if (final)
		{
			IMGUIPluginTranslationHooks.ResetHandledForAllInAssembly(assembly);
		}
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			try
			{
				if (typeof(MonoBehaviour).IsAssignableFrom(type) && !type.IsAbstract)
				{
					MethodInfo method = type.GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
					if (method != null)
					{
						IMGUIPluginTranslationHooks.HookIfConfigured(method);
					}
				}
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Warn(e, "An error occurred while hooking type: " + type.FullName);
			}
		}
		if (final)
		{
			_installedAssemblies.Add(assembly);
		}
	}
}
