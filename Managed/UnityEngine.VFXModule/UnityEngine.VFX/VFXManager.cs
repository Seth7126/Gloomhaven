using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine.VFX;

[NativeHeader("Modules/VFX/Public/VFXManager.h")]
[StaticAccessor("GetVFXManager()", StaticAccessorType.Dot)]
[RequiredByNativeCode]
public static class VFXManager
{
	private static readonly VFXCameraXRSettings kDefaultCameraXRSettings = new VFXCameraXRSettings
	{
		viewTotal = 1u,
		viewCount = 1u,
		viewOffset = 0u
	};

	internal static extern ScriptableObject runtimeResources
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static extern float fixedTimeStep
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern float maxDeltaTime
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	internal static extern string renderPipeSettingsPath
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern VisualEffect[] GetComponents();

	public static void ProcessCamera(Camera cam)
	{
		PrepareCamera(cam, kDefaultCameraXRSettings);
		ProcessCameraCommand(cam, null, kDefaultCameraXRSettings);
	}

	public static void PrepareCamera(Camera cam)
	{
		PrepareCamera(cam, kDefaultCameraXRSettings);
	}

	public static void PrepareCamera([NotNull("NullExceptionObject")] Camera cam, VFXCameraXRSettings camXRSettings)
	{
		PrepareCamera_Injected(cam, ref camXRSettings);
	}

	public static void ProcessCameraCommand(Camera cam, CommandBuffer cmd)
	{
		ProcessCameraCommand(cam, cmd, kDefaultCameraXRSettings);
	}

	public static void ProcessCameraCommand([NotNull("NullExceptionObject")] Camera cam, CommandBuffer cmd, VFXCameraXRSettings camXRSettings)
	{
		ProcessCameraCommand_Injected(cam, cmd, ref camXRSettings);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern VFXCameraBufferTypes IsCameraBufferNeeded([NotNull("NullExceptionObject")] Camera cam);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void SetCameraBuffer([NotNull("NullExceptionObject")] Camera cam, VFXCameraBufferTypes type, Texture buffer, int x, int y, int width, int height);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void PrepareCamera_Injected(Camera cam, ref VFXCameraXRSettings camXRSettings);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ProcessCameraCommand_Injected(Camera cam, CommandBuffer cmd, ref VFXCameraXRSettings camXRSettings);
}
