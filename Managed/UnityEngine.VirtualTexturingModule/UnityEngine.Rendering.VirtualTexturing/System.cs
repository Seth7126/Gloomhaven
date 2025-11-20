using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Rendering.VirtualTexturing;

[StaticAccessor("VirtualTexturing::System", StaticAccessorType.DoubleColon)]
[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
public static class System
{
	public const int AllMips = int.MaxValue;

	internal static extern bool enabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void Update();

	[NativeThrows]
	internal static void SetDebugFlag(Guid guid, bool enabled)
	{
		SetDebugFlag(guid.ToByteArray(), enabled);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	private static extern void SetDebugFlag(byte[] guid, bool enabled);
}
