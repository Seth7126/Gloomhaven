using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[NativeHeader("Runtime/Camera/BatchRendererGroup.h")]
[UsedByNativeCode]
public struct BatchCullingContext
{
	public readonly NativeArray<Plane> cullingPlanes;

	public NativeArray<BatchVisibility> batchVisibility;

	public NativeArray<int> visibleIndices;

	public NativeArray<int> visibleIndicesY;

	public readonly LODParameters lodParameters;

	public readonly Matrix4x4 cullingMatrix;

	public readonly float nearPlane;

	[Obsolete("For internal BatchRendererGroup use only")]
	public unsafe BatchCullingContext(NativeArray<Plane> inCullingPlanes, NativeArray<BatchVisibility> inOutBatchVisibility, NativeArray<int> outVisibleIndices, LODParameters inLodParameters)
	{
		cullingPlanes = inCullingPlanes;
		batchVisibility = inOutBatchVisibility;
		visibleIndices = outVisibleIndices;
		visibleIndicesY = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(null, 0, Allocator.Invalid);
		lodParameters = inLodParameters;
		cullingMatrix = Matrix4x4.identity;
		nearPlane = 0f;
	}

	[Obsolete("For internal BatchRendererGroup use only")]
	public unsafe BatchCullingContext(NativeArray<Plane> inCullingPlanes, NativeArray<BatchVisibility> inOutBatchVisibility, NativeArray<int> outVisibleIndices, LODParameters inLodParameters, Matrix4x4 inCullingMatrix, float inNearPlane)
	{
		cullingPlanes = inCullingPlanes;
		batchVisibility = inOutBatchVisibility;
		visibleIndices = outVisibleIndices;
		visibleIndicesY = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(null, 0, Allocator.Invalid);
		lodParameters = inLodParameters;
		cullingMatrix = inCullingMatrix;
		nearPlane = inNearPlane;
	}

	internal BatchCullingContext(NativeArray<Plane> inCullingPlanes, NativeArray<BatchVisibility> inOutBatchVisibility, NativeArray<int> outVisibleIndices, NativeArray<int> outVisibleIndicesY, LODParameters inLodParameters, Matrix4x4 inCullingMatrix, float inNearPlane)
	{
		cullingPlanes = inCullingPlanes;
		batchVisibility = inOutBatchVisibility;
		visibleIndices = outVisibleIndices;
		visibleIndicesY = outVisibleIndicesY;
		lodParameters = inLodParameters;
		cullingMatrix = inCullingMatrix;
		nearPlane = inNearPlane;
	}
}
