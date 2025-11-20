using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine.Rendering;

namespace UnityEngine.Formats.Alembic.Importer;

[RequireComponent(typeof(AlembicCurves))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class AlembicCurvesRenderer : MonoBehaviour
{
	private enum RenderMethod
	{
		Line,
		Strip
	}

	[BurstCompile]
	private struct GenerateLinesJob : IJobParallelFor
	{
		[WriteOnly]
		public NativeArray<int> indices;

		[WriteOnly]
		public NativeArray<Vector3> particleTangent;

		[WriteOnly]
		public NativeArray<Vector2> particleUV;

		[Unity.Collections.ReadOnly]
		public NativeArray<int> curveCounts;

		[Unity.Collections.ReadOnly]
		public NativeArray<int> strideArray;

		[Unity.Collections.ReadOnly]
		public NativeArray<Vector3> vertices;

		public unsafe void Execute(int curveIdx)
		{
			int* unsafePtr = (int*)indices.GetUnsafePtr();
			Vector3* unsafePtr2 = (Vector3*)particleTangent.GetUnsafePtr();
			Vector2* unsafePtr3 = (Vector2*)particleUV.GetUnsafePtr();
			int num = curveCounts[curveIdx];
			int num2 = strideArray[curveIdx];
			int num3 = num2 + num;
			int num4 = num - 1;
			for (int i = 0; i < num4; i++)
			{
				int num5 = (strideArray[curveIdx] - curveIdx + i) * 2;
				unsafePtr[num5] = num2 + i;
				unsafePtr[num5 + 1] = num2 + i + 1;
			}
			for (int j = num2; j < num3 - 1; j++)
			{
				Vector3 vector = vertices[j];
				Vector3 vector2 = vertices[j + 1];
				unsafePtr2[j] = Vector3.Normalize(vector2 - vector);
			}
			unsafePtr2[num3 - 1] = unsafePtr2[num3 - 2];
			for (int k = num2; k < num3; k++)
			{
				unsafePtr3[k] = new Vector2(k, curveIdx);
			}
		}
	}

	private AlembicCurves curves;

	private Mesh mesh;

	private ProfilerMarker setMeshProperties = new ProfilerMarker("SetMeshProperties");

	private void OnEnable()
	{
		curves = GetComponent<AlembicCurves>();
		mesh = new Mesh
		{
			hideFlags = HideFlags.DontSave
		};
		GetComponent<MeshFilter>().sharedMesh = mesh;
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component.sharedMaterial == null)
		{
			component.sharedMaterial = GetDefaultMaterial();
		}
		UpdateMesh(curves);
	}

	private void UpdateMesh(AlembicCurves curves)
	{
		if (curves.Positions.Length != 0)
		{
			GenerateLineMesh(mesh, curves.Positions, curves.CurveOffsets);
		}
	}

	private void LateUpdate()
	{
		UpdateMesh(curves);
	}

	private void OnDisable()
	{
		Object.DestroyImmediate(mesh);
	}

	private unsafe void GeneratePlaneMesh(Mesh theMesh, IReadOnlyList<Vector3> positions, IReadOnlyList<int> curveCounts, IReadOnlyList<float> widths)
	{
		int count = curveCounts.Count;
		int num = (positions.Count - count) * 4;
		using NativeArray<Vector3> nativeArray = new NativeArray<Vector3>(positions.Count * 2, Allocator.Temp);
		using NativeArray<int> nativeArray2 = new NativeArray<int>(num, Allocator.Temp);
		Vector3* unsafePtr = (Vector3*)nativeArray.GetUnsafePtr();
		int* unsafePtr2 = (int*)nativeArray2.GetUnsafePtr();
		int num2 = 0;
		Vector3 vector = new Vector3(1f, 0f, 0f);
		int num3 = 0;
		for (int i = 0; i < count; i++)
		{
			int num4 = curveCounts[i];
			for (int j = 0; j < num4; j++)
			{
				Vector3 vector2 = widths[num3 + j] / 2f * vector;
				Vector3 vector3 = positions[num3 + j] + vector2;
				Vector3 vector4 = positions[num3 + j] - vector2;
				*(unsafePtr++) = vector3;
				*(unsafePtr++) = vector4;
			}
			num3 += num4;
			int num5 = num4 - 1;
			int num6 = 0;
			for (int k = 0; k < num5; k++)
			{
				*(unsafePtr2++) = num6 + num2 + k;
				*(unsafePtr2++) = num6 + num2 + (k + 1);
				*(unsafePtr2++) = num6 + num2 + (k + 3);
				*(unsafePtr2++) = num6 + num2 + (k + 2);
				num6++;
			}
			num2 += num4;
		}
		theMesh.indexFormat = ((num > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
		theMesh.SetVertices(nativeArray);
		theMesh.SetIndices(nativeArray2, MeshTopology.Quads, 0);
		theMesh.RecalculateBounds();
		theMesh.RecalculateNormals();
	}

	private void GenerateLineMesh(Mesh theMesh, Vector3[] positionsM, int[] curveOffsetM)
	{
		int arrayLength = curveOffsetM.Length;
		int num = (positionsM.Length - curveOffsetM.Length) * 2;
		using NativeArray<int> nativeArray = new NativeArray<int>(curveOffsetM.Length, Allocator.TempJob);
		using NativeArray<Vector3> vertices = new NativeArray<Vector3>(positionsM.Length, Allocator.TempJob);
		using NativeArray<int> strideArray = new NativeArray<int>(curveOffsetM.Length, Allocator.TempJob);
		using NativeArray<Vector3> nativeArray2 = new NativeArray<Vector3>(positionsM.Length, Allocator.TempJob);
		using NativeArray<Vector2> nativeArray3 = new NativeArray<Vector2>(positionsM.Length, Allocator.TempJob);
		using NativeArray<int> indices = new NativeArray<int>(num, Allocator.TempJob);
		vertices.CopyFrom(positionsM);
		strideArray.CopyFrom(curveOffsetM);
		if (curveOffsetM.Length > 1)
		{
			NativeArray<int> nativeArray4 = nativeArray;
			nativeArray4[0] = curveOffsetM[1];
		}
		else
		{
			NativeArray<int> nativeArray5 = nativeArray;
			nativeArray5[0] = positionsM.Length;
		}
		for (int i = 1; i < strideArray.Length; i++)
		{
			NativeArray<int> nativeArray6 = nativeArray;
			nativeArray6[i] = strideArray[i] - strideArray[i - 1];
		}
		new GenerateLinesJob
		{
			indices = indices,
			curveCounts = nativeArray,
			strideArray = strideArray,
			particleTangent = nativeArray2,
			vertices = vertices,
			particleUV = nativeArray3
		}.Schedule(arrayLength, 1).Complete();
		using (setMeshProperties.Auto())
		{
			theMesh.indexFormat = ((num > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			theMesh.SetVertices(vertices);
			theMesh.SetIndices(indices, MeshTopology.Lines, 0);
			theMesh.SetNormals(nativeArray2);
			theMesh.SetUVs(0, nativeArray3);
			if (curves.Velocities.Length != 0)
			{
				theMesh.SetUVs(5, curves.Velocities);
			}
			theMesh.RecalculateBounds();
			theMesh.Optimize();
		}
	}

	private static Material GetDefaultMaterial()
	{
		if (!(GraphicsSettings.renderPipelineAsset != null))
		{
			return new Material(Shader.Find("Diffuse"));
		}
		return GraphicsSettings.renderPipelineAsset.defaultMaterial;
	}
}
