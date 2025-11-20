using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline;

public static class BlitUtility
{
	public struct Vertex
	{
		public Vector4 Position;

		public Vector3 Normal;
	}

	public struct TriangleIndex
	{
		public ushort Index;

		public static implicit operator int(TriangleIndex index)
		{
			return index.Index;
		}
	}

	private static readonly int MainTexHash = Shader.PropertyToID("_MainTex");

	private static Vector4[] normals = new Vector4[8]
	{
		new Vector4(-1f, -1f, -1f),
		new Vector4(1f, -1f, -1f),
		new Vector4(1f, 1f, -1f),
		new Vector4(-1f, 1f, -1f),
		new Vector4(-1f, 1f, 1f),
		new Vector4(1f, 1f, 1f),
		new Vector4(1f, -1f, 1f),
		new Vector4(-1f, -1f, 1f)
	};

	private static ushort[] triangles = new ushort[36]
	{
		0, 2, 1, 0, 3, 2, 2, 3, 4, 2,
		4, 5, 1, 2, 5, 1, 5, 6, 0, 7,
		4, 0, 4, 3, 5, 4, 7, 5, 7, 6,
		0, 6, 7, 0, 1, 6
	};

	private static Vector4[] tempVertecies = new Vector4[8]
	{
		new Vector4(-1f, -1f, -1f, 1f),
		new Vector4(1f, -1f, -1f, 1f),
		new Vector4(1f, 1f, -1f, 1f),
		new Vector4(-1f, 1f, -1f, 1f),
		new Vector4(-1f, 1f, 1f, 1f),
		new Vector4(1f, 1f, 1f, 1f),
		new Vector4(1f, -1f, 1f, 1f),
		new Vector4(-1f, -1f, 1f, 1f)
	};

	private static int[] indecies = new int[20480];

	private static List<int> indeciesList = new List<int>();

	private static List<Vector3> verteciesList = new List<Vector3>();

	private static List<Vector3> normalsList = new List<Vector3>();

	private static Vertex[] vertices = new Vertex[4096];

	private static void UpdateBounds(Renderer renderer, OutlineTarget target)
	{
		if (target.renderer is MeshRenderer)
		{
			MeshFilter component = renderer.GetComponent<MeshFilter>();
			if (component.sharedMesh != null)
			{
				component.sharedMesh.RecalculateBounds();
			}
		}
		else if (target.renderer is SkinnedMeshRenderer)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
			if (skinnedMeshRenderer.sharedMesh != null)
			{
				skinnedMeshRenderer.sharedMesh.RecalculateBounds();
			}
		}
	}

	public static void SetupMesh(OutlineParameters parameters, float baseShift)
	{
		if (parameters.BlitMesh == null)
		{
			parameters.BlitMesh = parameters.MeshPool.AllocateMesh();
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (Outlinable item in parameters.OutlinablesToRender)
		{
			num3 += 8 * item.OutlineTargets.Count;
		}
		if (vertices.Length < num3)
		{
			Array.Resize(ref vertices, num3 * 2);
			Array.Resize(ref indecies, vertices.Length * 5);
		}
		verteciesList.Clear();
		normalsList.Clear();
		indeciesList.Clear();
		foreach (Outlinable item2 in parameters.OutlinablesToRender)
		{
			if (item2.DrawingMode != OutlinableDrawingMode.Normal)
			{
				continue;
			}
			Outlinable.OutlineProperties outlineProperties = ((item2.RenderStyle == RenderStyle.FrontBack) ? item2.FrontParameters : item2.OutlineParameters);
			Outlinable.OutlineProperties outlineProperties2 = ((item2.RenderStyle == RenderStyle.FrontBack) ? item2.BackParameters : item2.OutlineParameters);
			if (parameters.UseInfoBuffer && (outlineProperties.DilateShift > 0.01f || outlineProperties2.DilateShift > 0.01f))
			{
				_ = 1;
			}
			else
				_ = !parameters.UseInfoBuffer;
			if (parameters.UseInfoBuffer && (outlineProperties.BlurShift > 0.01f || outlineProperties2.BlurShift > 0.01f))
			{
				_ = 1;
			}
			else
				_ = !parameters.UseInfoBuffer;
			foreach (OutlineTarget outlineTarget in item2.OutlineTargets)
			{
				Renderer renderer = outlineTarget.Renderer;
				if (!outlineTarget.IsVisible)
				{
					continue;
				}
				bool flag = false;
				Bounds bounds = default(Bounds);
				if (outlineTarget.BoundsMode == BoundsMode.Manual)
				{
					bounds = outlineTarget.Bounds;
					Vector3 size = bounds.size;
					Vector3 localScale = renderer.transform.localScale;
					size.x /= localScale.x;
					size.y /= localScale.y;
					size.z /= localScale.z;
					bounds.size = size;
				}
				else
				{
					if (outlineTarget.BoundsMode == BoundsMode.ForceRecalculate)
					{
						UpdateBounds(outlineTarget.Renderer, outlineTarget);
					}
					MeshRenderer meshRenderer = renderer as MeshRenderer;
					if (!(meshRenderer == null))
					{
						_ = meshRenderer.subMeshStartIndex;
					}
					_ = outlineTarget.SubmeshIndex;
					MeshFilter meshFilter = ((meshRenderer == null) ? null : meshRenderer.GetComponent<MeshFilter>());
					if (!(meshFilter == null))
					{
						_ = meshFilter.sharedMesh;
					}
					flag = true;
					bounds = renderer.bounds;
				}
				float num4 = 0.5f;
				Vector4 vector = bounds.size * num4;
				vector.w = 1f;
				Vector4 vector2 = bounds.center;
				Matrix4x4 matrix4x = Matrix4x4.identity;
				Matrix4x4 matrix4x2 = Matrix4x4.identity;
				if (!flag && (outlineTarget.BoundsMode == BoundsMode.Manual || !renderer.isPartOfStaticBatch))
				{
					matrix4x = outlineTarget.renderer.transform.localToWorldMatrix;
					matrix4x2 = Matrix4x4.Rotate(renderer.transform.rotation);
				}
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)num
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 2)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 1)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)num
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 3)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 2)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 2)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 3)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 4)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 2)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 4)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 5)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 1)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 2)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 5)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 1)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 5)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 6)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)num
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 7)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 4)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)num
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 4)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 3)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 5)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 4)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 7)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 5)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 7)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 6)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)num
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 6)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 7)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)num
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 1)
				};
				indecies[num2++] = new TriangleIndex
				{
					Index = (ushort)(num + 6)
				};
				for (int i = 0; i < 8; i++)
				{
					Vector4 vector3 = matrix4x2 * normals[i];
					Vector3 normal = new Vector3(vector3.x, vector3.y, vector3.z);
					Vector4 vector4 = tempVertecies[i];
					Vector4 vector5 = new Vector4(vector4.x * vector.x, vector4.y * vector.y, vector4.z * vector.z, 1f);
					Vertex vertex = new Vertex
					{
						Position = matrix4x * (vector2 + vector5),
						Normal = normal
					};
					vertices[num++] = vertex;
					verteciesList.Add(vertex.Position);
					normalsList.Add(vertex.Normal);
				}
			}
		}
		for (int j = 0; j < num2; j++)
		{
			indeciesList.Add(indecies[j]);
		}
		parameters.BlitMesh.Clear(keepVertexLayout: true);
		parameters.BlitMesh.SetVertices(verteciesList);
		parameters.BlitMesh.SetNormals(normalsList);
		parameters.BlitMesh.SetTriangles(indeciesList, 0);
	}

	public static void Blit(OutlineParameters parameters, RenderTargetIdentifier source, RenderTargetIdentifier destination, RenderTargetIdentifier destinationDepth, Material material, CommandBuffer targetBuffer, int pass = -1, Rect? viewport = null)
	{
		CommandBuffer obj = ((targetBuffer == null) ? parameters.Buffer : targetBuffer);
		obj.SetRenderTarget(destination, destinationDepth);
		if (viewport.HasValue)
		{
			parameters.Buffer.SetViewport(viewport.Value);
		}
		obj.SetGlobalTexture(MainTexHash, source);
		obj.DrawMesh(parameters.BlitMesh, Matrix4x4.identity, material, 0, pass);
	}

	public static void Draw(OutlineParameters parameters, RenderTargetIdentifier target, RenderTargetIdentifier depth, Material material, Rect? viewport = null)
	{
		parameters.Buffer.SetRenderTarget(target, depth);
		if (viewport.HasValue)
		{
			parameters.Buffer.SetViewport(viewport.Value);
		}
		parameters.Buffer.DrawMesh(parameters.BlitMesh, Matrix4x4.identity, material, 0, -1);
	}
}
