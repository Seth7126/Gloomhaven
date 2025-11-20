using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.UIElements;

[NativeHeader("Modules/UIElementsNative/UIElementsRuntimeUtilityNative.h")]
[VisibleToOtherModules(new string[] { "Unity.UIElements" })]
internal static class UIElementsRuntimeUtilityNative
{
	internal static Action RepaintOverlayPanelsCallback;

	internal static Action UpdateRuntimePanelsCallback;

	[RequiredByNativeCode]
	public static void RepaintOverlayPanels()
	{
		RepaintOverlayPanelsCallback?.Invoke();
	}

	[RequiredByNativeCode]
	public static void UpdateRuntimePanels()
	{
		UpdateRuntimePanelsCallback?.Invoke();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void RegisterPlayerloopCallback();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void UnregisterPlayerloopCallback();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VisualElementCreation();
}
