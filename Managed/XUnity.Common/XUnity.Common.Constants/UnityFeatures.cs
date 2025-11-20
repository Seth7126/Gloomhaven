using System;
using System.Reflection;

namespace XUnity.Common.Constants;

public static class UnityFeatures
{
	private static readonly BindingFlags All;

	public static bool SupportsMouseScrollDelta { get; }

	public static bool SupportsClipboard { get; }

	public static bool SupportsCustomYieldInstruction { get; }

	public static bool SupportsSceneManager { get; }

	public static bool SupportsWaitForSecondsRealtime { get; set; }

	static UnityFeatures()
	{
		All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		SupportsMouseScrollDelta = false;
		SupportsClipboard = false;
		SupportsCustomYieldInstruction = false;
		SupportsSceneManager = false;
		SupportsWaitForSecondsRealtime = false;
		try
		{
			SupportsClipboard = UnityTypes.TextEditor?.ClrType.GetProperty("text")?.GetSetMethod() != null;
		}
		catch (Exception)
		{
		}
		try
		{
			SupportsCustomYieldInstruction = UnityTypes.CustomYieldInstruction != null;
		}
		catch (Exception)
		{
		}
		try
		{
			SupportsSceneManager = UnityTypes.Scene != null && UnityTypes.SceneManager != null && UnityTypes.SceneManager.ClrType.GetMethod("add_sceneLoaded", All) != null;
		}
		catch (Exception)
		{
		}
		try
		{
			SupportsMouseScrollDelta = UnityTypes.Input?.ClrType.GetProperty("mouseScrollDelta") != null;
		}
		catch (Exception)
		{
		}
		try
		{
			SupportsWaitForSecondsRealtime = UnityTypes.WaitForSecondsRealtime != null;
		}
		catch (Exception)
		{
		}
	}
}
