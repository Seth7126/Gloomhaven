using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Formats.Alembic.Sdk;

internal struct aiPolyMeshSample
{
	[NativeDisableUnsafePtrRestriction]
	public IntPtr self;

	public static implicit operator bool(aiPolyMeshSample v)
	{
		return v.self != IntPtr.Zero;
	}

	public static implicit operator aiSample(aiPolyMeshSample v)
	{
		aiSample result = default(aiSample);
		result.self = v.self;
		return result;
	}

	public void GetSummary(ref aiMeshSampleSummary dst)
	{
		NativeMethods.aiPolyMeshGetSampleSummary(self, ref dst);
	}

	public unsafe void GetSplitSummaries(NativeArray<aiMeshSplitSummary> dst)
	{
		NativeMethods.aiPolyMeshGetSplitSummaries(self, new IntPtr(dst.GetUnsafePtr()));
	}

	public unsafe void GetSubmeshSummaries(NativeArray<aiSubmeshSummary> dst)
	{
		NativeMethods.aiPolyMeshGetSubmeshSummaries(self, new IntPtr(dst.GetUnsafePtr()));
	}

	internal unsafe void FillVertexBuffer(NativeArray<aiPolyMeshData> vbs, NativeArray<aiSubmeshData> ibs)
	{
		NativeMethods.aiPolyMeshFillVertexBuffer(self, new IntPtr(vbs.GetUnsafePtr()), new IntPtr(ibs.GetUnsafePtr()));
	}
}
