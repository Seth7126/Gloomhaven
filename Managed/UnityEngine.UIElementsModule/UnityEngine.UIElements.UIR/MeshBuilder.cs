#define UNITY_ASSERTIONS
using System;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements.UIR;

internal static class MeshBuilder
{
	internal struct AllocMeshData
	{
		internal delegate MeshWriteData Allocator(uint vertexCount, uint indexCount, ref AllocMeshData allocatorData);

		internal Allocator alloc;

		internal Texture texture;

		internal TextureId svgTexture;

		internal Material material;

		internal MeshGenerationContext.MeshFlags flags;

		internal BMPAlloc colorAlloc;

		internal MeshWriteData Allocate(uint vertexCount, uint indexCount)
		{
			return alloc(vertexCount, indexCount, ref this);
		}
	}

	private struct ClipCounts
	{
		public int firstClippedIndex;

		public int firstDegenerateIndex;

		public int lastClippedIndex;

		public int clippedTriangles;

		public int addedTriangles;

		public int degenerateTriangles;
	}

	private enum VertexClipEdge
	{
		None = 0,
		Left = 1,
		Top = 2,
		Right = 4,
		Bottom = 8
	}

	private static ProfilerMarker s_VectorGraphics9Slice = new ProfilerMarker("UIR.MakeVector9Slice");

	private static ProfilerMarker s_VectorGraphicsStretch = new ProfilerMarker("UIR.MakeVectorStretch");

	internal static readonly int s_MaxTextMeshVertices = 49152;

	private static readonly ushort[] slicedQuadIndices = new ushort[54]
	{
		0, 4, 1, 4, 5, 1, 1, 5, 2, 5,
		6, 2, 2, 6, 3, 6, 7, 3, 4, 8,
		5, 8, 9, 5, 5, 9, 6, 9, 10, 6,
		6, 10, 7, 10, 11, 7, 8, 12, 9, 12,
		13, 9, 9, 13, 10, 13, 14, 10, 10, 14,
		11, 14, 15, 11
	};

	private static readonly float[] k_TexCoordSlicesX = new float[4];

	private static readonly float[] k_TexCoordSlicesY = new float[4];

	private static readonly float[] k_PositionSlicesX = new float[4];

	private static readonly float[] k_PositionSlicesY = new float[4];

	private static VertexClipEdge[] s_AllClipEdges = new VertexClipEdge[4]
	{
		VertexClipEdge.Left,
		VertexClipEdge.Top,
		VertexClipEdge.Right,
		VertexClipEdge.Bottom
	};

	internal static void MakeBorder(MeshGenerationContextUtils.BorderParams borderParams, float posZ, AllocMeshData meshAlloc)
	{
		Tessellation.TessellateBorder(borderParams, posZ, meshAlloc);
	}

	internal static void MakeSolidRect(MeshGenerationContextUtils.RectangleParams rectParams, float posZ, AllocMeshData meshAlloc)
	{
		if (!rectParams.HasRadius(Tessellation.kEpsilon))
		{
			Tessellation.TessellateQuad(rectParams, posZ, meshAlloc);
		}
		else
		{
			Tessellation.TessellateRect(rectParams, posZ, meshAlloc, computeUVs: false);
		}
	}

	internal static void MakeTexturedRect(MeshGenerationContextUtils.RectangleParams rectParams, float posZ, AllocMeshData meshAlloc, ColorPage colorPage)
	{
		if ((float)rectParams.leftSlice <= 1E-30f && (float)rectParams.topSlice <= 1E-30f && (float)rectParams.rightSlice <= 1E-30f && (float)rectParams.bottomSlice <= 1E-30f)
		{
			if (!rectParams.HasRadius(Tessellation.kEpsilon))
			{
				MakeQuad(rectParams.rect, rectParams.uv, rectParams.color, posZ, meshAlloc, colorPage);
			}
			else
			{
				Tessellation.TessellateRect(rectParams, posZ, meshAlloc, computeUVs: true);
			}
		}
		else if (rectParams.texture == null)
		{
			MakeQuad(rectParams.rect, rectParams.uv, rectParams.color, posZ, meshAlloc, colorPage);
		}
		else
		{
			MakeSlicedQuad(ref rectParams, posZ, meshAlloc);
		}
	}

	private static Vertex ConvertTextVertexToUIRVertex(MeshInfo info, int index, Vector2 offset, VertexFlags flags = VertexFlags.IsText, bool isDynamicColor = false)
	{
		float num = 0f;
		if (info.uvs2[index].y < 0f)
		{
			num = 1f;
		}
		return new Vertex
		{
			position = new Vector3(info.vertices[index].x + offset.x, info.vertices[index].y + offset.y, 0f),
			uv = new Vector2(info.uvs0[index].x, info.uvs0[index].y),
			tint = info.colors32[index],
			flags = new Color32((byte)flags, (byte)(num * 255f), 0, (byte)(isDynamicColor ? 1 : 0))
		};
	}

	private static Vertex ConvertTextVertexToUIRVertex(TextVertex textVertex, Vector2 offset)
	{
		return new Vertex
		{
			position = new Vector3(textVertex.position.x + offset.x, textVertex.position.y + offset.y, 0f),
			uv = textVertex.uv0,
			tint = textVertex.color,
			flags = new Color32(1, 0, 0, 0)
		};
	}

	private static int LimitTextVertices(int vertexCount, bool logTruncation = true)
	{
		if (vertexCount <= s_MaxTextMeshVertices)
		{
			return vertexCount;
		}
		if (logTruncation)
		{
			Debug.LogWarning($"Generated text will be truncated because it exceeds {s_MaxTextMeshVertices} vertices.");
		}
		return s_MaxTextMeshVertices;
	}

	internal static void MakeText(MeshInfo meshInfo, Vector2 offset, AllocMeshData meshAlloc, VertexFlags flags = VertexFlags.IsText, bool isDynamicColor = false)
	{
		int num = LimitTextVertices(meshInfo.vertexCount);
		int num2 = num / 4;
		MeshWriteData meshWriteData = meshAlloc.Allocate((uint)(num2 * 4), (uint)(num2 * 6));
		int num3 = 0;
		int num4 = 0;
		while (num3 < num2)
		{
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(meshInfo, num4, offset, flags, isDynamicColor));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(meshInfo, num4 + 1, offset, flags, isDynamicColor));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(meshInfo, num4 + 2, offset, flags, isDynamicColor));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(meshInfo, num4 + 3, offset, flags, isDynamicColor));
			meshWriteData.SetNextIndex((ushort)num4);
			meshWriteData.SetNextIndex((ushort)(num4 + 1));
			meshWriteData.SetNextIndex((ushort)(num4 + 2));
			meshWriteData.SetNextIndex((ushort)(num4 + 2));
			meshWriteData.SetNextIndex((ushort)(num4 + 3));
			meshWriteData.SetNextIndex((ushort)num4);
			num3++;
			num4 += 4;
		}
	}

	internal static void MakeText(NativeArray<TextVertex> uiVertices, Vector2 offset, AllocMeshData meshAlloc)
	{
		int num = LimitTextVertices(uiVertices.Length);
		int num2 = num / 4;
		MeshWriteData meshWriteData = meshAlloc.Allocate((uint)(num2 * 4), (uint)(num2 * 6));
		int num3 = 0;
		int num4 = 0;
		while (num3 < num2)
		{
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(uiVertices[num4], offset));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(uiVertices[num4 + 1], offset));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(uiVertices[num4 + 2], offset));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(uiVertices[num4 + 3], offset));
			meshWriteData.SetNextIndex((ushort)num4);
			meshWriteData.SetNextIndex((ushort)(num4 + 1));
			meshWriteData.SetNextIndex((ushort)(num4 + 2));
			meshWriteData.SetNextIndex((ushort)(num4 + 2));
			meshWriteData.SetNextIndex((ushort)(num4 + 3));
			meshWriteData.SetNextIndex((ushort)num4);
			num3++;
			num4 += 4;
		}
	}

	internal static void UpdateText(NativeArray<TextVertex> uiVertices, Vector2 offset, Matrix4x4 transform, Color32 xformClipPages, Color32 ids, Color32 flags, Color32 opacityPageSettingIndex, NativeSlice<Vertex> vertices)
	{
		int num = LimitTextVertices(uiVertices.Length, logTruncation: false);
		Debug.Assert(num == vertices.Length);
		flags.r = 1;
		for (int i = 0; i < num; i++)
		{
			TextVertex textVertex = uiVertices[i];
			vertices[i] = new Vertex
			{
				position = transform.MultiplyPoint3x4(new Vector3(textVertex.position.x + offset.x, textVertex.position.y + offset.y, 0f)),
				uv = textVertex.uv0,
				tint = textVertex.color,
				xformClipPages = xformClipPages,
				ids = ids,
				flags = flags,
				opacityColorPages = opacityPageSettingIndex
			};
		}
	}

	private static void MakeQuad(Rect rcPosition, Rect rcTexCoord, Color color, float posZ, AllocMeshData meshAlloc, ColorPage colorPage)
	{
		MeshWriteData meshWriteData = meshAlloc.Allocate(4u, 6u);
		float x = rcPosition.x;
		float xMax = rcPosition.xMax;
		float yMax = rcPosition.yMax;
		float y = rcPosition.y;
		Rect uvRegion = meshWriteData.uvRegion;
		float x2 = rcTexCoord.x * uvRegion.width + uvRegion.xMin;
		float x3 = rcTexCoord.xMax * uvRegion.width + uvRegion.xMin;
		float y2 = rcTexCoord.y * uvRegion.height + uvRegion.yMin;
		float y3 = rcTexCoord.yMax * uvRegion.height + uvRegion.yMin;
		Color32 flags = new Color32(0, 0, 0, (byte)(colorPage.isValid ? 1 : 0));
		Color32 opacityColorPages = new Color32(0, 0, colorPage.pageAndID.r, colorPage.pageAndID.g);
		Color32 ids = new Color32(0, 0, 0, colorPage.pageAndID.b);
		meshWriteData.SetNextVertex(new Vertex
		{
			position = new Vector3(x, yMax, posZ),
			tint = color,
			uv = new Vector2(x2, y2),
			flags = flags,
			opacityColorPages = opacityColorPages,
			ids = ids
		});
		meshWriteData.SetNextVertex(new Vertex
		{
			position = new Vector3(xMax, yMax, posZ),
			tint = color,
			uv = new Vector2(x3, y2),
			flags = flags,
			opacityColorPages = opacityColorPages,
			ids = ids
		});
		meshWriteData.SetNextVertex(new Vertex
		{
			position = new Vector3(x, y, posZ),
			tint = color,
			uv = new Vector2(x2, y3),
			flags = flags,
			opacityColorPages = opacityColorPages,
			ids = ids
		});
		meshWriteData.SetNextVertex(new Vertex
		{
			position = new Vector3(xMax, y, posZ),
			tint = color,
			uv = new Vector2(x3, y3),
			flags = flags,
			opacityColorPages = opacityColorPages,
			ids = ids
		});
		meshWriteData.SetNextIndex(0);
		meshWriteData.SetNextIndex(2);
		meshWriteData.SetNextIndex(1);
		meshWriteData.SetNextIndex(1);
		meshWriteData.SetNextIndex(2);
		meshWriteData.SetNextIndex(3);
	}

	internal static void MakeSlicedQuad(ref MeshGenerationContextUtils.RectangleParams rectParams, float posZ, AllocMeshData meshAlloc)
	{
		MeshWriteData meshWriteData = meshAlloc.Allocate(16u, 54u);
		float num = 1f;
		float num2 = rectParams.texture.width;
		float num3 = rectParams.texture.height;
		float num4 = num / num2;
		float num5 = num / num3;
		float num6 = Mathf.Max(0f, rectParams.leftSlice);
		float num7 = Mathf.Max(0f, rectParams.rightSlice);
		float num8 = Mathf.Max(0f, rectParams.bottomSlice);
		float num9 = Mathf.Max(0f, rectParams.topSlice);
		float num10 = Mathf.Max(0.01f, rectParams.sliceScale);
		float num11 = Mathf.Clamp(num6 * num4, 0f, 1f);
		float num12 = Mathf.Clamp(num7 * num4, 0f, 1f);
		float num13 = Mathf.Clamp(num8 * num5, 0f, 1f);
		float num14 = Mathf.Clamp(num9 * num5, 0f, 1f);
		num6 *= rectParams.sliceScale;
		num7 *= rectParams.sliceScale;
		num8 *= rectParams.sliceScale;
		num9 *= rectParams.sliceScale;
		k_TexCoordSlicesX[0] = rectParams.uv.min.x;
		k_TexCoordSlicesX[1] = rectParams.uv.min.x + num11;
		k_TexCoordSlicesX[2] = rectParams.uv.max.x - num12;
		k_TexCoordSlicesX[3] = rectParams.uv.max.x;
		k_TexCoordSlicesY[0] = rectParams.uv.max.y;
		k_TexCoordSlicesY[1] = rectParams.uv.max.y - num13;
		k_TexCoordSlicesY[2] = rectParams.uv.min.y + num14;
		k_TexCoordSlicesY[3] = rectParams.uv.min.y;
		Rect uvRegion = meshWriteData.uvRegion;
		for (int i = 0; i < 4; i++)
		{
			k_TexCoordSlicesX[i] = k_TexCoordSlicesX[i] * uvRegion.width + uvRegion.xMin;
			k_TexCoordSlicesY[i] = (rectParams.uv.min.y + rectParams.uv.max.y - k_TexCoordSlicesY[i]) * uvRegion.height + uvRegion.yMin;
		}
		float num15 = num6 + num7;
		if (num15 > rectParams.rect.width)
		{
			float num16 = rectParams.rect.width / num15;
			num6 *= num16;
			num7 *= num16;
		}
		float num17 = num8 + num9;
		if (num17 > rectParams.rect.height)
		{
			float num18 = rectParams.rect.height / num17;
			num8 *= num18;
			num9 *= num18;
		}
		k_PositionSlicesX[0] = rectParams.rect.x;
		k_PositionSlicesX[1] = rectParams.rect.x + num6;
		k_PositionSlicesX[2] = rectParams.rect.xMax - num7;
		k_PositionSlicesX[3] = rectParams.rect.xMax;
		k_PositionSlicesY[0] = rectParams.rect.yMax;
		k_PositionSlicesY[1] = rectParams.rect.yMax - num8;
		k_PositionSlicesY[2] = rectParams.rect.y + num9;
		k_PositionSlicesY[3] = rectParams.rect.y;
		for (int j = 0; j < 16; j++)
		{
			int num19 = j % 4;
			int num20 = j / 4;
			meshWriteData.SetNextVertex(new Vertex
			{
				position = new Vector3(k_PositionSlicesX[num19], k_PositionSlicesY[num20], posZ),
				uv = new Vector2(k_TexCoordSlicesX[num19], k_TexCoordSlicesY[num20]),
				tint = rectParams.color
			});
		}
		meshWriteData.SetAllIndices(slicedQuadIndices);
	}

	internal static void MakeVectorGraphics(MeshGenerationContextUtils.RectangleParams rectParams, int settingIndexOffset, AllocMeshData meshAlloc, out int finalVertexCount, out int finalIndexCount)
	{
		VectorImage vectorImage = rectParams.vectorImage;
		Debug.Assert(vectorImage != null);
		finalVertexCount = 0;
		finalIndexCount = 0;
		int num = vectorImage.vertices.Length;
		Vertex[] array = new Vertex[num];
		for (int i = 0; i < num; i++)
		{
			VectorImageVertex vectorImageVertex = vectorImage.vertices[i];
			array[i] = new Vertex
			{
				position = vectorImageVertex.position,
				tint = vectorImageVertex.tint,
				uv = vectorImageVertex.uv,
				opacityColorPages = new Color32(0, 0, (byte)(vectorImageVertex.settingIndex >> 8), (byte)vectorImageVertex.settingIndex)
			};
		}
		if (!((float)rectParams.leftSlice <= 1E-30f) || !((float)rectParams.topSlice <= 1E-30f) || !((float)rectParams.rightSlice <= 1E-30f) || !((float)rectParams.bottomSlice <= 1E-30f))
		{
			MakeVectorGraphics9SliceBackground(sliceLTRB: new Vector4(rectParams.leftSlice, rectParams.topSlice, rectParams.rightSlice, rectParams.bottomSlice), svgVertices: array, svgIndices: vectorImage.indices, svgWidth: vectorImage.size.x, svgHeight: vectorImage.size.y, targetRect: rectParams.rect, stretch: true, tint: rectParams.color, settingIndexOffset: settingIndexOffset, meshAlloc: meshAlloc);
			return;
		}
		MakeVectorGraphicsStretchBackground(array, vectorImage.indices, vectorImage.size.x, vectorImage.size.y, rectParams.rect, rectParams.uv, rectParams.scaleMode, rectParams.color, settingIndexOffset, meshAlloc, out finalVertexCount, out finalIndexCount);
	}

	internal static void MakeVectorGraphicsStretchBackground(Vertex[] svgVertices, ushort[] svgIndices, float svgWidth, float svgHeight, Rect targetRect, Rect sourceUV, ScaleMode scaleMode, Color tint, int settingIndexOffset, AllocMeshData meshAlloc, out int finalVertexCount, out int finalIndexCount)
	{
		Vector2 size = new Vector2(svgWidth * sourceUV.width, svgHeight * sourceUV.height);
		Vector2 vector = new Vector2(sourceUV.xMin * svgWidth, sourceUV.yMin * svgHeight);
		Rect rect = new Rect(vector, size);
		bool flag = sourceUV.xMin != 0f || sourceUV.yMin != 0f || sourceUV.width != 1f || sourceUV.height != 1f;
		float num = size.x / size.y;
		float num2 = targetRect.width / targetRect.height;
		Vector2 vector3 = default(Vector2);
		Vector2 vector2 = default(Vector2);
		switch (scaleMode)
		{
		case ScaleMode.StretchToFill:
			vector3 = new Vector2(0f, 0f);
			vector2.x = targetRect.width / size.x;
			vector2.y = targetRect.height / size.y;
			break;
		case ScaleMode.ScaleAndCrop:
			vector3 = new Vector2(0f, 0f);
			if (num2 > num)
			{
				vector2.x = (vector2.y = targetRect.width / size.x);
				float num3 = targetRect.height / vector2.y;
				float num4 = rect.height / 2f - num3 / 2f;
				vector3.y -= num4 * vector2.y;
				rect.y += num4;
				rect.height = num3;
				flag = true;
			}
			else if (num2 < num)
			{
				vector2.x = (vector2.y = targetRect.height / size.y);
				float num5 = targetRect.width / vector2.x;
				float num6 = rect.width / 2f - num5 / 2f;
				vector3.x -= num6 * vector2.x;
				rect.x += num6;
				rect.width = num5;
				flag = true;
			}
			else
			{
				vector2.x = (vector2.y = targetRect.width / size.x);
			}
			break;
		case ScaleMode.ScaleToFit:
			if (num2 > num)
			{
				vector2.x = (vector2.y = targetRect.height / size.y);
				vector3.x = (targetRect.width - size.x * vector2.x) * 0.5f;
				vector3.y = 0f;
			}
			else
			{
				vector2.x = (vector2.y = targetRect.width / size.x);
				vector3.x = 0f;
				vector3.y = (targetRect.height - size.y * vector2.y) * 0.5f;
			}
			break;
		default:
			throw new NotImplementedException();
		}
		vector3 -= vector * vector2;
		int newVertexCount = svgVertices.Length;
		int num7 = svgIndices.Length;
		ClipCounts cc = default(ClipCounts);
		Vector4 clipRectMinMax = Vector4.zero;
		if (flag)
		{
			if (rect.width <= 0f || rect.height <= 0f)
			{
				finalVertexCount = (finalIndexCount = 0);
				return;
			}
			clipRectMinMax = new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax);
			cc = UpperBoundApproximateRectClippingResults(svgVertices, svgIndices, clipRectMinMax);
			newVertexCount += cc.clippedTriangles * 6;
			num7 += cc.addedTriangles * 3;
			num7 -= cc.degenerateTriangles * 3;
		}
		MeshWriteData meshWriteData = meshAlloc.alloc((uint)newVertexCount, (uint)num7, ref meshAlloc);
		if (flag)
		{
			RectClip(svgVertices, svgIndices, clipRectMinMax, meshWriteData, cc, ref newVertexCount);
		}
		else
		{
			meshWriteData.SetAllIndices(svgIndices);
		}
		Debug.Assert(meshWriteData.currentVertex == 0);
		Rect uvRegion = meshWriteData.uvRegion;
		int num8 = svgVertices.Length;
		for (int i = 0; i < num8; i++)
		{
			Vertex nextVertex = svgVertices[i];
			nextVertex.position.x = nextVertex.position.x * vector2.x + vector3.x;
			nextVertex.position.y = nextVertex.position.y * vector2.y + vector3.y;
			nextVertex.uv.x = nextVertex.uv.x * uvRegion.width + uvRegion.xMin;
			nextVertex.uv.y = nextVertex.uv.y * uvRegion.height + uvRegion.yMin;
			ref Color32 tint2 = ref nextVertex.tint;
			tint2 *= tint;
			uint num9 = (uint)(((nextVertex.opacityColorPages.b << 8) | nextVertex.opacityColorPages.a) + settingIndexOffset);
			nextVertex.opacityColorPages.b = (byte)(num9 >> 8);
			nextVertex.opacityColorPages.a = (byte)num9;
			meshWriteData.SetNextVertex(nextVertex);
		}
		for (int j = num8; j < newVertexCount; j++)
		{
			Vertex nextVertex2 = meshWriteData.m_Vertices[j];
			nextVertex2.position.x = nextVertex2.position.x * vector2.x + vector3.x;
			nextVertex2.position.y = nextVertex2.position.y * vector2.y + vector3.y;
			nextVertex2.uv.x = nextVertex2.uv.x * uvRegion.width + uvRegion.xMin;
			nextVertex2.uv.y = nextVertex2.uv.y * uvRegion.height + uvRegion.yMin;
			ref Color32 tint3 = ref nextVertex2.tint;
			tint3 *= tint;
			uint num10 = (uint)(((nextVertex2.opacityColorPages.b << 8) | nextVertex2.opacityColorPages.a) + settingIndexOffset);
			nextVertex2.opacityColorPages.b = (byte)(num10 >> 8);
			nextVertex2.opacityColorPages.a = (byte)num10;
			meshWriteData.SetNextVertex(nextVertex2);
		}
		finalVertexCount = meshWriteData.vertexCount;
		finalIndexCount = meshWriteData.indexCount;
	}

	private static void MakeVectorGraphics9SliceBackground(Vertex[] svgVertices, ushort[] svgIndices, float svgWidth, float svgHeight, Rect targetRect, Vector4 sliceLTRB, bool stretch, Color tint, int settingIndexOffset, AllocMeshData meshAlloc)
	{
		MeshWriteData meshWriteData = meshAlloc.alloc((uint)svgVertices.Length, (uint)svgIndices.Length, ref meshAlloc);
		meshWriteData.SetAllIndices(svgIndices);
		if (!stretch)
		{
			throw new NotImplementedException("Support for repeating 9-slices is not done yet");
		}
		Rect uvRegion = meshWriteData.uvRegion;
		int num = svgVertices.Length;
		Vector2 vector = new Vector2(1f / (svgWidth - sliceLTRB.z - sliceLTRB.x), 1f / (svgHeight - sliceLTRB.w - sliceLTRB.y));
		Vector2 vector2 = new Vector2(targetRect.width - svgWidth, targetRect.height - svgHeight);
		Vector2 vector3 = default(Vector2);
		for (int i = 0; i < num; i++)
		{
			Vertex nextVertex = svgVertices[i];
			vector3.x = Mathf.Clamp01((nextVertex.position.x - sliceLTRB.x) * vector.x);
			vector3.y = Mathf.Clamp01((nextVertex.position.y - sliceLTRB.y) * vector.y);
			nextVertex.position.x += vector3.x * vector2.x;
			nextVertex.position.y += vector3.y * vector2.y;
			nextVertex.uv.x = nextVertex.uv.x * uvRegion.width + uvRegion.xMin;
			nextVertex.uv.y = nextVertex.uv.y * uvRegion.height + uvRegion.yMin;
			ref Color32 tint2 = ref nextVertex.tint;
			tint2 *= tint;
			uint num2 = (uint)(((nextVertex.opacityColorPages.b << 8) | nextVertex.opacityColorPages.a) + settingIndexOffset);
			nextVertex.opacityColorPages.b = (byte)(num2 >> 8);
			nextVertex.opacityColorPages.a = (byte)num2;
			meshWriteData.SetNextVertex(nextVertex);
		}
	}

	private static ClipCounts UpperBoundApproximateRectClippingResults(Vertex[] vertices, ushort[] indices, Vector4 clipRectMinMax)
	{
		ClipCounts result = new ClipCounts
		{
			firstClippedIndex = int.MaxValue,
			firstDegenerateIndex = -1,
			lastClippedIndex = -1
		};
		int num = indices.Length;
		Vector4 vector = default(Vector4);
		for (int i = 0; i < num; i += 3)
		{
			Vector3 position = vertices[indices[i]].position;
			Vector3 position2 = vertices[indices[i + 1]].position;
			Vector3 position3 = vertices[indices[i + 2]].position;
			vector.x = ((position.x < position2.x) ? position.x : position2.x);
			vector.x = ((vector.x < position3.x) ? vector.x : position3.x);
			vector.y = ((position.y < position2.y) ? position.y : position2.y);
			vector.y = ((vector.y < position3.y) ? vector.y : position3.y);
			vector.z = ((position.x > position2.x) ? position.x : position2.x);
			vector.z = ((vector.z > position3.x) ? vector.z : position3.x);
			vector.w = ((position.y > position2.y) ? position.y : position2.y);
			vector.w = ((vector.w > position3.y) ? vector.w : position3.y);
			if (vector.x >= clipRectMinMax.x && vector.z <= clipRectMinMax.z && vector.y >= clipRectMinMax.y && vector.w <= clipRectMinMax.w)
			{
				result.firstDegenerateIndex = -1;
				continue;
			}
			result.firstClippedIndex = ((result.firstClippedIndex < i) ? result.firstClippedIndex : i);
			result.lastClippedIndex = i + 2;
			if (vector.x >= clipRectMinMax.z || vector.z <= clipRectMinMax.x || vector.y >= clipRectMinMax.w || vector.w <= clipRectMinMax.y)
			{
				result.firstDegenerateIndex = ((result.firstDegenerateIndex == -1) ? i : result.firstDegenerateIndex);
				result.degenerateTriangles++;
			}
			else
			{
				result.firstDegenerateIndex = -1;
			}
			result.clippedTriangles++;
			result.addedTriangles += 4;
		}
		return result;
	}

	private unsafe static void RectClip(Vertex[] vertices, ushort[] indices, Vector4 clipRectMinMax, MeshWriteData mwd, ClipCounts cc, ref int newVertexCount)
	{
		int num = cc.lastClippedIndex;
		if (cc.firstDegenerateIndex != -1 && cc.firstDegenerateIndex < num)
		{
			num = cc.firstDegenerateIndex;
		}
		ushort nextNewVertex = (ushort)vertices.Length;
		for (int i = 0; i < cc.firstClippedIndex; i++)
		{
			mwd.SetNextIndex(indices[i]);
		}
		ushort* ptr = stackalloc ushort[3];
		Vertex* ptr2 = stackalloc Vertex[3];
		Vector4 vector = default(Vector4);
		for (int j = cc.firstClippedIndex; j < num; j += 3)
		{
			*ptr = indices[j];
			ptr[1] = indices[j + 1];
			ptr[2] = indices[j + 2];
			*ptr2 = vertices[*ptr];
			ptr2[1] = vertices[ptr[1]];
			ptr2[2] = vertices[ptr[2]];
			vector.x = ((ptr2->position.x < ptr2[1].position.x) ? ptr2->position.x : ptr2[1].position.x);
			vector.x = ((vector.x < ptr2[2].position.x) ? vector.x : ptr2[2].position.x);
			vector.y = ((ptr2->position.y < ptr2[1].position.y) ? ptr2->position.y : ptr2[1].position.y);
			vector.y = ((vector.y < ptr2[2].position.y) ? vector.y : ptr2[2].position.y);
			vector.z = ((ptr2->position.x > ptr2[1].position.x) ? ptr2->position.x : ptr2[1].position.x);
			vector.z = ((vector.z > ptr2[2].position.x) ? vector.z : ptr2[2].position.x);
			vector.w = ((ptr2->position.y > ptr2[1].position.y) ? ptr2->position.y : ptr2[1].position.y);
			vector.w = ((vector.w > ptr2[2].position.y) ? vector.w : ptr2[2].position.y);
			if (vector.x >= clipRectMinMax.x && vector.z <= clipRectMinMax.z && vector.y >= clipRectMinMax.y && vector.w <= clipRectMinMax.w)
			{
				mwd.SetNextIndex(*ptr);
				mwd.SetNextIndex(ptr[1]);
				mwd.SetNextIndex(ptr[2]);
			}
			else if (!(vector.x >= clipRectMinMax.z) && !(vector.z <= clipRectMinMax.x) && !(vector.y >= clipRectMinMax.w) && !(vector.w <= clipRectMinMax.y))
			{
				RectClipTriangle(ptr2, ptr, clipRectMinMax, mwd, ref nextNewVertex);
			}
		}
		int num2 = indices.Length;
		for (int k = cc.lastClippedIndex + 1; k < num2; k++)
		{
			mwd.SetNextIndex(indices[k]);
		}
		newVertexCount = nextNewVertex;
		mwd.m_Vertices = mwd.m_Vertices.Slice(0, newVertexCount);
		mwd.m_Indices = mwd.m_Indices.Slice(0, mwd.currentIndex);
	}

	private unsafe static void RectClipTriangle(Vertex* vt, ushort* it, Vector4 clipRectMinMax, MeshWriteData mwd, ref ushort nextNewVertex)
	{
		Vertex* ptr = stackalloc Vertex[13];
		VertexClipEdge* ptr2 = stackalloc VertexClipEdge[3];
		Vector4* ptr3 = stackalloc Vector4[4];
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			VertexClipEdge vertexClipEdge = VertexClipEdge.None;
			if (vt[i].position.x < clipRectMinMax.x)
			{
				vertexClipEdge |= VertexClipEdge.Left;
			}
			if (vt[i].position.y < clipRectMinMax.y)
			{
				vertexClipEdge |= VertexClipEdge.Top;
			}
			if (vt[i].position.x > clipRectMinMax.z)
			{
				vertexClipEdge |= VertexClipEdge.Right;
			}
			if (vt[i].position.y > clipRectMinMax.w)
			{
				vertexClipEdge |= VertexClipEdge.Bottom;
			}
			if (vertexClipEdge == VertexClipEdge.None)
			{
				ptr[num++] = vt[i];
			}
			ptr2[i] = vertexClipEdge;
		}
		if (num == 3)
		{
			mwd.SetNextIndex(*it);
			mwd.SetNextIndex(it[1]);
			mwd.SetNextIndex(it[2]);
			return;
		}
		Vector3 vertexBaryCentricCoordinates = GetVertexBaryCentricCoordinates(vt, clipRectMinMax.x, clipRectMinMax.y);
		Vector3 vertexBaryCentricCoordinates2 = GetVertexBaryCentricCoordinates(vt, clipRectMinMax.z, clipRectMinMax.y);
		Vector3 vertexBaryCentricCoordinates3 = GetVertexBaryCentricCoordinates(vt, clipRectMinMax.x, clipRectMinMax.w);
		Vector3 vertexBaryCentricCoordinates4 = GetVertexBaryCentricCoordinates(vt, clipRectMinMax.z, clipRectMinMax.w);
		if (vertexBaryCentricCoordinates.x >= -1E-07f && vertexBaryCentricCoordinates.x <= 1.0000001f && vertexBaryCentricCoordinates.y >= -1E-07f && vertexBaryCentricCoordinates.y <= 1.0000001f && vertexBaryCentricCoordinates.z >= -1E-07f && vertexBaryCentricCoordinates.z <= 1.0000001f)
		{
			ptr[num++] = InterpolateVertexInTriangle(vt, clipRectMinMax.x, clipRectMinMax.y, vertexBaryCentricCoordinates);
		}
		if (vertexBaryCentricCoordinates2.x >= -1E-07f && vertexBaryCentricCoordinates2.x <= 1.0000001f && vertexBaryCentricCoordinates2.y >= -1E-07f && vertexBaryCentricCoordinates2.y <= 1.0000001f && vertexBaryCentricCoordinates2.z >= -1E-07f && vertexBaryCentricCoordinates2.z <= 1.0000001f)
		{
			ptr[num++] = InterpolateVertexInTriangle(vt, clipRectMinMax.z, clipRectMinMax.y, vertexBaryCentricCoordinates2);
		}
		if (vertexBaryCentricCoordinates3.x >= -1E-07f && vertexBaryCentricCoordinates3.x <= 1.0000001f && vertexBaryCentricCoordinates3.y >= -1E-07f && vertexBaryCentricCoordinates3.y <= 1.0000001f && vertexBaryCentricCoordinates3.z >= -1E-07f && vertexBaryCentricCoordinates3.z <= 1.0000001f)
		{
			ptr[num++] = InterpolateVertexInTriangle(vt, clipRectMinMax.x, clipRectMinMax.w, vertexBaryCentricCoordinates3);
		}
		if (vertexBaryCentricCoordinates4.x >= -1E-07f && vertexBaryCentricCoordinates4.x <= 1.0000001f && vertexBaryCentricCoordinates4.y >= -1E-07f && vertexBaryCentricCoordinates4.y <= 1.0000001f && vertexBaryCentricCoordinates4.z >= -1E-07f && vertexBaryCentricCoordinates4.z <= 1.0000001f)
		{
			ptr[num++] = InterpolateVertexInTriangle(vt, clipRectMinMax.z, clipRectMinMax.w, vertexBaryCentricCoordinates4);
		}
		*ptr3 = new Vector4(clipRectMinMax.x, clipRectMinMax.y, clipRectMinMax.x, clipRectMinMax.w);
		ptr3[1] = new Vector4(clipRectMinMax.x, clipRectMinMax.y, clipRectMinMax.z, clipRectMinMax.y);
		ptr3[2] = new Vector4(clipRectMinMax.z, clipRectMinMax.y, clipRectMinMax.z, clipRectMinMax.w);
		ptr3[3] = new Vector4(clipRectMinMax.x, clipRectMinMax.w, clipRectMinMax.z, clipRectMinMax.w);
		for (int j = 0; j < s_AllClipEdges.Length; j++)
		{
			VertexClipEdge vertexClipEdge2 = s_AllClipEdges[j];
			Vector4 vector = ptr3[j];
			for (int k = 0; k < 3; k++)
			{
				int num2 = (k + 1) % 3;
				if ((ptr2[k] & vertexClipEdge2) != (ptr2[num2] & vertexClipEdge2))
				{
					float num3 = IntersectSegments(vt[k].position.x, vt[k].position.y, vt[num2].position.x, vt[num2].position.y, vector.x, vector.y, vector.z, vector.w);
					if (num3 != float.MaxValue)
					{
						ptr[num++] = InterpolateVertexInTriangleEdge(vt, k, num2, num3);
					}
				}
			}
		}
		if (num == 0)
		{
			return;
		}
		float* ptr4 = stackalloc float[num];
		*ptr4 = 0f;
		for (int l = 1; l < num; l++)
		{
			ptr4[l] = Mathf.Atan2(ptr[l].position.y - ptr->position.y, ptr[l].position.x - ptr->position.x);
			if (ptr4[l] < 0f)
			{
				ptr4[l] += (float)Math.PI * 2f;
			}
		}
		int* ptr5 = stackalloc int[num];
		*ptr5 = 0;
		uint num4 = 0u;
		for (int m = 1; m < num; m++)
		{
			int num5 = -1;
			float num6 = float.MaxValue;
			for (int n = 1; n < num; n++)
			{
				if ((num4 & (1 << n)) == 0L && ptr4[n] < num6)
				{
					num6 = ptr4[n];
					num5 = n;
				}
			}
			ptr5[m] = num5;
			num4 |= (uint)(1 << num5);
		}
		ushort num7 = nextNewVertex;
		for (int num8 = 0; num8 < num; num8++)
		{
			mwd.m_Vertices[num7 + num8] = ptr[ptr5[num8]];
		}
		nextNewVertex += (ushort)num;
		int num9 = num - 2;
		bool flag = false;
		Vector3 position = mwd.m_Vertices[num7].position;
		for (int num10 = 0; num10 < num9; num10++)
		{
			int num11 = num7 + num10 + 1;
			int num12 = num7 + num10 + 2;
			if (!flag)
			{
				float num13 = ptr4[ptr5[num10 + 1]];
				float num14 = ptr4[ptr5[num10 + 2]];
				if (num14 - num13 >= (float)Math.PI)
				{
					num11 = num7 + 1;
					num12 = num7 + num - 1;
					flag = true;
				}
			}
			Vector3 position2 = mwd.m_Vertices[num11].position;
			Vector3 position3 = mwd.m_Vertices[num12].position;
			Vector3 vector2 = Vector3.Cross(position2 - position, position3 - position);
			mwd.SetNextIndex(num7);
			if (vector2.z < 0f)
			{
				mwd.SetNextIndex((ushort)num12);
				mwd.SetNextIndex((ushort)num11);
			}
			else
			{
				mwd.SetNextIndex((ushort)num11);
				mwd.SetNextIndex((ushort)num12);
			}
		}
	}

	private unsafe static Vector3 GetVertexBaryCentricCoordinates(Vertex* vt, float x, float y)
	{
		float num = vt[1].position.x - vt->position.x;
		float num2 = vt[1].position.y - vt->position.y;
		float num3 = vt[2].position.x - vt->position.x;
		float num4 = vt[2].position.y - vt->position.y;
		float num5 = x - vt->position.x;
		float num6 = y - vt->position.y;
		float num7 = num * num + num2 * num2;
		float num8 = num * num3 + num2 * num4;
		float num9 = num3 * num3 + num4 * num4;
		float num10 = num5 * num + num6 * num2;
		float num11 = num5 * num3 + num6 * num4;
		float num12 = num7 * num9 - num8 * num8;
		Vector3 result = default(Vector3);
		result.y = (num9 * num10 - num8 * num11) / num12;
		result.z = (num7 * num11 - num8 * num10) / num12;
		result.x = 1f - result.y - result.z;
		return result;
	}

	private unsafe static Vertex InterpolateVertexInTriangle(Vertex* vt, float x, float y, Vector3 uvw)
	{
		Vertex result = *vt;
		result.position.x = x;
		result.position.y = y;
		result.tint = (Color)vt->tint * uvw.x + (Color)vt[1].tint * uvw.y + (Color)vt[2].tint * uvw.z;
		result.uv = vt->uv * uvw.x + vt[1].uv * uvw.y + vt[2].uv * uvw.z;
		return result;
	}

	private unsafe static Vertex InterpolateVertexInTriangleEdge(Vertex* vt, int e0, int e1, float t)
	{
		Vertex result = *vt;
		result.position.x = vt[e0].position.x + t * (vt[e1].position.x - vt[e0].position.x);
		result.position.y = vt[e0].position.y + t * (vt[e1].position.y - vt[e0].position.y);
		result.tint = Color.LerpUnclamped(vt[e0].tint, vt[e1].tint, t);
		result.uv = Vector2.LerpUnclamped(vt[e0].uv, vt[e1].uv, t);
		return result;
	}

	private static float IntersectSegments(float ax, float ay, float bx, float by, float cx, float cy, float dx, float dy)
	{
		float num = (ax - dx) * (by - dy) - (ay - dy) * (bx - dx);
		float num2 = (ax - cx) * (by - cy) - (ay - cy) * (bx - cx);
		if (num * num2 >= -1E-05f)
		{
			return float.MaxValue;
		}
		float num3 = (cx - ax) * (dy - ay) - (cy - ay) * (dx - ax);
		float num4 = num3 + num2 - num;
		if (num3 * num4 >= -1E-05f)
		{
			return float.MaxValue;
		}
		return num3 / (num3 - num4);
	}
}
