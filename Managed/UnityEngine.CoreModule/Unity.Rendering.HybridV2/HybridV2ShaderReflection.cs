using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Bindings;

namespace Unity.Rendering.HybridV2;

public class HybridV2ShaderReflection
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetDOTSInstancingCbuffersPointer")]
	private static extern IntPtr GetDOTSInstancingCbuffersPointer([NotNull("ArgumentNullException")] Shader shader, ref int cbufferCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ShaderScripting::GetDOTSInstancingPropertiesPointer")]
	private static extern IntPtr GetDOTSInstancingPropertiesPointer([NotNull("ArgumentNullException")] Shader shader, ref int propertyCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("Shader::GetDOTSReflectionVersionNumber")]
	public static extern uint GetDOTSReflectionVersionNumber();

	public unsafe static NativeArray<DOTSInstancingCbuffer> GetDOTSInstancingCbuffers(Shader shader)
	{
		if (shader == null)
		{
			return default(NativeArray<DOTSInstancingCbuffer>);
		}
		int cbufferCount = 0;
		IntPtr dOTSInstancingCbuffersPointer = GetDOTSInstancingCbuffersPointer(shader, ref cbufferCount);
		if (dOTSInstancingCbuffersPointer == IntPtr.Zero)
		{
			return default(NativeArray<DOTSInstancingCbuffer>);
		}
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<DOTSInstancingCbuffer>((void*)dOTSInstancingCbuffersPointer, cbufferCount, Allocator.Temp);
	}

	public unsafe static NativeArray<DOTSInstancingProperty> GetDOTSInstancingProperties(Shader shader)
	{
		if (shader == null)
		{
			return default(NativeArray<DOTSInstancingProperty>);
		}
		int propertyCount = 0;
		IntPtr dOTSInstancingPropertiesPointer = GetDOTSInstancingPropertiesPointer(shader, ref propertyCount);
		if (dOTSInstancingPropertiesPointer == IntPtr.Zero)
		{
			return default(NativeArray<DOTSInstancingProperty>);
		}
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<DOTSInstancingProperty>((void*)dOTSInstancingPropertiesPointer, propertyCount, Allocator.Temp);
	}
}
