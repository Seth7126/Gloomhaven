#define UNITY_ASSERTIONS
using Unity.Collections;
using Unity.Profiling;

namespace UnityEngine.UIElements.UIR;

internal static class Tessellation
{
	internal enum Edges
	{
		None = 0,
		Left = 1,
		Top = 2,
		Right = 4,
		Bottom = 8,
		All = 15
	}

	internal static float kEpsilon = 0.001f;

	internal static float kUnusedArc = -9999.9f;

	internal static ushort kSubdivisions = 6;

	private static ProfilerMarker s_MarkerTessellateRect = new ProfilerMarker("TessellateRect");

	private static ProfilerMarker s_MarkerTessellateBorder = new ProfilerMarker("TessellateBorder");

	internal const int kMaxEdgeBit = 4;

	private static Edges[] s_AllEdges = new Edges[4]
	{
		Edges.Left,
		Edges.Top,
		Edges.Right,
		Edges.Bottom
	};

	public static void TessellateRect(MeshGenerationContextUtils.RectangleParams rectParams, float posZ, MeshBuilder.AllocMeshData meshAlloc, bool computeUVs)
	{
		if (!(rectParams.rect.width < kEpsilon) && !(rectParams.rect.height < kEpsilon))
		{
			Vector2 rhs = new Vector2(rectParams.rect.width * 0.5f, rectParams.rect.height * 0.5f);
			rectParams.topLeftRadius = Vector2.Min(rectParams.topLeftRadius, rhs);
			rectParams.topRightRadius = Vector2.Min(rectParams.topRightRadius, rhs);
			rectParams.bottomRightRadius = Vector2.Min(rectParams.bottomRightRadius, rhs);
			rectParams.bottomLeftRadius = Vector2.Min(rectParams.bottomLeftRadius, rhs);
			ushort vertexCount = 0;
			ushort indexCount = 0;
			TessellateRoundedCorners(ref rectParams, 0f, null, rectParams.colorPage, ref vertexCount, ref indexCount, countOnly: true);
			MeshWriteData meshWriteData = meshAlloc.Allocate(vertexCount, indexCount);
			vertexCount = 0;
			indexCount = 0;
			TessellateRoundedCorners(ref rectParams, posZ, meshWriteData, rectParams.colorPage, ref vertexCount, ref indexCount, countOnly: false);
			if (computeUVs)
			{
				ComputeUVs(rectParams.rect, rectParams.uv, meshWriteData.uvRegion, meshWriteData.m_Vertices);
			}
			Debug.Assert(vertexCount == meshWriteData.vertexCount);
			Debug.Assert(indexCount == meshWriteData.indexCount);
		}
	}

	public static void TessellateQuad(MeshGenerationContextUtils.RectangleParams rectParams, float posZ, MeshBuilder.AllocMeshData meshAlloc)
	{
		if (!(rectParams.rect.width < kEpsilon) && !(rectParams.rect.height < kEpsilon))
		{
			ushort vertexCount = 0;
			ushort indexCount = 0;
			TessellateQuad(rectParams.rect, Edges.All, rectParams.color, posZ, null, rectParams.colorPage, ref vertexCount, ref indexCount, countOnly: true);
			MeshWriteData meshWriteData = meshAlloc.Allocate(vertexCount, indexCount);
			vertexCount = 0;
			indexCount = 0;
			TessellateQuad(rectParams.rect, Edges.All, rectParams.color, posZ, meshWriteData, rectParams.colorPage, ref vertexCount, ref indexCount, countOnly: false);
			Debug.Assert(vertexCount == meshWriteData.vertexCount);
			Debug.Assert(indexCount == meshWriteData.indexCount);
		}
	}

	public static void TessellateBorder(MeshGenerationContextUtils.BorderParams borderParams, float posZ, MeshBuilder.AllocMeshData meshAlloc)
	{
		if (!(borderParams.rect.width < kEpsilon) && !(borderParams.rect.height < kEpsilon))
		{
			Vector2 rhs = new Vector2(borderParams.rect.width * 0.5f, borderParams.rect.height * 0.5f);
			borderParams.topLeftRadius = Vector2.Min(borderParams.topLeftRadius, rhs);
			borderParams.topRightRadius = Vector2.Min(borderParams.topRightRadius, rhs);
			borderParams.bottomRightRadius = Vector2.Min(borderParams.bottomRightRadius, rhs);
			borderParams.bottomLeftRadius = Vector2.Min(borderParams.bottomLeftRadius, rhs);
			borderParams.leftWidth = Mathf.Min(borderParams.leftWidth, rhs.x);
			borderParams.topWidth = Mathf.Min(borderParams.topWidth, rhs.y);
			borderParams.rightWidth = Mathf.Min(borderParams.rightWidth, rhs.x);
			borderParams.bottomWidth = Mathf.Min(borderParams.bottomWidth, rhs.y);
			ushort vertexCount = 0;
			ushort indexCount = 0;
			TessellateRoundedBorders(ref borderParams, 0f, null, ref vertexCount, ref indexCount, countOnly: true);
			MeshWriteData meshWriteData = meshAlloc.Allocate(vertexCount, indexCount);
			vertexCount = 0;
			indexCount = 0;
			TessellateRoundedBorders(ref borderParams, posZ, meshWriteData, ref vertexCount, ref indexCount, countOnly: false);
			Debug.Assert(vertexCount == meshWriteData.vertexCount);
			Debug.Assert(indexCount == meshWriteData.indexCount);
		}
	}

	private static void TessellateRoundedCorners(ref MeshGenerationContextUtils.RectangleParams rectParams, float posZ, MeshWriteData mesh, ColorPage colorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		ushort num = 0;
		ushort num2 = 0;
		Vector2 vector = new Vector2(rectParams.rect.width * 0.5f, rectParams.rect.height * 0.5f);
		Rect rect = new Rect(rectParams.rect.x, rectParams.rect.y, vector.x, vector.y);
		TessellateRoundedCorner(rect, rectParams.color, posZ, rectParams.topLeftRadius, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
		num = vertexCount;
		num2 = indexCount;
		TessellateRoundedCorner(rect, rectParams.color, posZ, rectParams.topRightRadius, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
		if (!countOnly)
		{
			MirrorVertices(rect, mesh.m_Vertices, num, vertexCount - num, flipHorizontal: true);
			FlipWinding(mesh.m_Indices, num2, indexCount - num2);
		}
		num = vertexCount;
		num2 = indexCount;
		TessellateRoundedCorner(rect, rectParams.color, posZ, rectParams.bottomRightRadius, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
		if (!countOnly)
		{
			MirrorVertices(rect, mesh.m_Vertices, num, vertexCount - num, flipHorizontal: true);
			MirrorVertices(rect, mesh.m_Vertices, num, vertexCount - num, flipHorizontal: false);
		}
		num = vertexCount;
		num2 = indexCount;
		TessellateRoundedCorner(rect, rectParams.color, posZ, rectParams.bottomLeftRadius, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
		if (!countOnly)
		{
			MirrorVertices(rect, mesh.m_Vertices, num, vertexCount - num, flipHorizontal: false);
			FlipWinding(mesh.m_Indices, num2, indexCount - num2);
		}
	}

	private static void TessellateRoundedBorders(ref MeshGenerationContextUtils.BorderParams border, float posZ, MeshWriteData mesh, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		ushort num = 0;
		ushort num2 = 0;
		Vector2 vector = new Vector2(border.rect.width * 0.5f, border.rect.height * 0.5f);
		Rect rect = new Rect(border.rect.x, border.rect.y, vector.x, vector.y);
		Color32 leftColor = border.leftColor;
		Color32 topColor = border.topColor;
		Color32 topColor2 = border.bottomColor;
		Color32 leftColor2 = border.rightColor;
		TessellateRoundedBorder(rect, leftColor, topColor, posZ, border.topLeftRadius, border.leftWidth, border.topWidth, mesh, border.leftColorPage, border.topColorPage, ref vertexCount, ref indexCount, countOnly);
		num = vertexCount;
		num2 = indexCount;
		TessellateRoundedBorder(rect, leftColor2, topColor, posZ, border.topRightRadius, border.rightWidth, border.topWidth, mesh, border.rightColorPage, border.topColorPage, ref vertexCount, ref indexCount, countOnly);
		if (!countOnly)
		{
			MirrorVertices(rect, mesh.m_Vertices, num, vertexCount - num, flipHorizontal: true);
			FlipWinding(mesh.m_Indices, num2, indexCount - num2);
		}
		num = vertexCount;
		num2 = indexCount;
		TessellateRoundedBorder(rect, leftColor2, topColor2, posZ, border.bottomRightRadius, border.rightWidth, border.bottomWidth, mesh, border.rightColorPage, border.bottomColorPage, ref vertexCount, ref indexCount, countOnly);
		if (!countOnly)
		{
			MirrorVertices(rect, mesh.m_Vertices, num, vertexCount - num, flipHorizontal: true);
			MirrorVertices(rect, mesh.m_Vertices, num, vertexCount - num, flipHorizontal: false);
		}
		num = vertexCount;
		num2 = indexCount;
		TessellateRoundedBorder(rect, leftColor, topColor2, posZ, border.bottomLeftRadius, border.leftWidth, border.bottomWidth, mesh, border.leftColorPage, border.bottomColorPage, ref vertexCount, ref indexCount, countOnly);
		if (!countOnly)
		{
			MirrorVertices(rect, mesh.m_Vertices, num, vertexCount - num, flipHorizontal: false);
			FlipWinding(mesh.m_Indices, num2, indexCount - num2);
		}
	}

	private static void TessellateRoundedCorner(Rect rect, Color32 color, float posZ, Vector2 radius, MeshWriteData mesh, ColorPage colorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		Vector2 center = rect.position + radius;
		Rect rect2 = Rect.zero;
		if (radius == Vector2.zero)
		{
			TessellateQuad(rect, (Edges)3, color, posZ, mesh, default(ColorPage), ref vertexCount, ref indexCount, countOnly);
			return;
		}
		TessellateFilledFan(center, radius, Vector2.zero, 0f, 0f, color, color, posZ, mesh, colorPage, colorPage, ref vertexCount, ref indexCount, countOnly);
		if (radius.x < rect.width)
		{
			rect2 = new Rect(rect.x + radius.x, rect.y, rect.width - radius.x, rect.height);
			TessellateQuad(rect2, Edges.Top, color, posZ, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
		}
		if (radius.y < rect.height)
		{
			rect2 = new Rect(rect.x, rect.y + radius.y, (radius.x < rect.width) ? radius.x : rect.width, rect.height - radius.y);
			TessellateQuad(rect2, Edges.Left, color, posZ, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
		}
	}

	private static void TessellateRoundedBorder(Rect rect, Color32 leftColor, Color32 topColor, float posZ, Vector2 radius, float leftWidth, float topWidth, MeshWriteData mesh, ColorPage leftColorPage, ColorPage topColorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		if (leftWidth < kEpsilon && topWidth < kEpsilon)
		{
			return;
		}
		leftWidth = Mathf.Max(0f, leftWidth);
		topWidth = Mathf.Max(0f, topWidth);
		radius.x = Mathf.Clamp(radius.x, 0f, rect.width);
		radius.y = Mathf.Clamp(radius.y, 0f, rect.height);
		Vector2 center = rect.position + radius;
		Rect zero = Rect.zero;
		if (radius.x < kEpsilon || radius.y < kEpsilon)
		{
			if (leftWidth > kEpsilon)
			{
				zero = new Rect(rect.x, rect.y, leftWidth, rect.height);
				TessellateStraightBorder(zero, Edges.Left, topWidth, leftColor, posZ, mesh, leftColorPage, ref vertexCount, ref indexCount, countOnly);
			}
			if (topWidth > kEpsilon)
			{
				zero = new Rect(rect.x, rect.y, rect.width, topWidth);
				TessellateStraightBorder(zero, Edges.Top, leftWidth, topColor, posZ, mesh, topColorPage, ref vertexCount, ref indexCount, countOnly);
			}
			return;
		}
		if (LooseCompare(radius.x, leftWidth) == 0 && LooseCompare(radius.y, topWidth) == 0)
		{
			TessellateFilledFan(center, radius, Vector2.zero, leftWidth, topWidth, leftColor, topColor, posZ, mesh, leftColorPage, topColorPage, ref vertexCount, ref indexCount, countOnly);
		}
		else if (LooseCompare(radius.x, leftWidth) > 0 && LooseCompare(radius.y, topWidth) > 0)
		{
			TessellateBorderedFan(center, radius, leftWidth, topWidth, leftColor, topColor, posZ, mesh, leftColorPage, topColorPage, ref vertexCount, ref indexCount, countOnly);
		}
		else
		{
			zero = new Rect(rect.x, rect.y, Mathf.Max(radius.x, leftWidth), Mathf.Max(radius.y, topWidth));
			TessellateComplexBorderCorner(zero, radius, leftWidth, topWidth, leftColor, topColor, posZ, mesh, leftColorPage, topColorPage, ref vertexCount, ref indexCount, countOnly);
		}
		float num = Mathf.Max(radius.y, topWidth);
		zero = new Rect(rect.x, rect.y + num, leftWidth, rect.height - num);
		TessellateStraightBorder(zero, Edges.Left, 0f, leftColor, posZ, mesh, leftColorPage, ref vertexCount, ref indexCount, countOnly);
		num = Mathf.Max(radius.x, leftWidth);
		zero = new Rect(rect.x + num, rect.y, rect.width - num, topWidth);
		TessellateStraightBorder(zero, Edges.Top, 0f, topColor, posZ, mesh, topColorPage, ref vertexCount, ref indexCount, countOnly);
	}

	private static Vector2 IntersectLines(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		Vector2 vector = p3 - p2;
		Vector2 vector2 = p2 - p0;
		Vector2 vector3 = p1 - p0;
		float num = vector.x * vector3.y - vector3.x * vector.y;
		if (Mathf.Approximately(num, 0f))
		{
			return new Vector2(float.NaN, float.NaN);
		}
		float num2 = vector.x * vector2.y - vector2.x * vector.y;
		float num3 = num2 / num;
		return p0 + vector3 * num3;
	}

	private static int LooseCompare(float a, float b)
	{
		if (a < b - kEpsilon)
		{
			return -1;
		}
		if (a > b + kEpsilon)
		{
			return 1;
		}
		return 0;
	}

	private unsafe static void TessellateComplexBorderCorner(Rect rect, Vector2 radius, float leftWidth, float topWidth, Color32 leftColor, Color32 topColor, float posZ, MeshWriteData mesh, ColorPage leftColorPage, ColorPage topColorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		if (!(rect.width < kEpsilon) && !(rect.height < kEpsilon))
		{
			Vector2 vector = rect.position + radius;
			Vector2 zero = Vector2.zero;
			float num = radius.x / radius.y;
			Vector2 p = vector - radius;
			Vector2 p2 = new Vector2(leftWidth, topWidth);
			Vector2 vector2 = IntersectLines(p, p2, new Vector2(0f, radius.y), radius);
			if (vector2.x >= 0f && LooseCompare(vector2.x, leftWidth) <= 0)
			{
				zero.x = Mathf.Min(0f, vector2.x - vector.x);
			}
			vector2 = IntersectLines(p, p2, new Vector2(radius.x, 0f), radius);
			if (vector2.y >= 0f && LooseCompare(vector2.y, topWidth) <= 0)
			{
				zero.y = Mathf.Min(0f, vector2.y - vector.y);
			}
			TessellateFilledFan(vector, radius, zero, leftWidth, topWidth, leftColor, topColor, posZ, mesh, leftColorPage, topColorPage, ref vertexCount, ref indexCount, countOnly);
			if (LooseCompare(rect.height, radius.y) > 0)
			{
				Rect rect2 = new Rect(rect.x, rect.y + radius.y, leftWidth, rect.height - radius.y);
				Vector2* ptr = stackalloc Vector2[4];
				ptr[2] = new Vector2(radius.x - leftWidth + zero.x, zero.y);
				TessellateQuad(rect2, (Edges)5, ptr, leftColor, posZ, mesh, leftColorPage, ref vertexCount, ref indexCount, countOnly);
			}
			else if (zero.y < 0f - kEpsilon)
			{
				Rect rect3 = new Rect(rect.x, rect.y + radius.y + zero.y, leftWidth, 0f - zero.y);
				Vector2* ptr2 = stackalloc Vector2[4];
				ptr2[1] = new Vector2(radius.x + zero.x, 0f);
				TessellateQuad(rect3, Edges.Right, ptr2, leftColor, posZ, mesh, leftColorPage, ref vertexCount, ref indexCount, countOnly);
			}
			if (LooseCompare(rect.width, radius.x) > 0)
			{
				Rect rect4 = new Rect(rect.x + radius.x, rect.y, rect.width - radius.x, topWidth);
				Vector2* ptr3 = stackalloc Vector2[4];
				*ptr3 = new Vector2(zero.x, radius.y - topWidth + zero.y);
				TessellateQuad(rect4, (Edges)10, ptr3, topColor, posZ, mesh, topColorPage, ref vertexCount, ref indexCount, countOnly);
			}
			else if (zero.x < 0f - kEpsilon)
			{
				Rect rect5 = new Rect(rect.x + radius.x + zero.x, rect.y, 0f - zero.x, topWidth);
				Vector2* ptr4 = stackalloc Vector2[4];
				*ptr4 = new Vector2(leftWidth - (radius.x + zero.x), 0f);
				ptr4[1] = new Vector2(0f, radius.y);
				TessellateQuad(rect5, Edges.Bottom, ptr4, topColor, posZ, mesh, topColorPage, ref vertexCount, ref indexCount, countOnly);
			}
		}
	}

	private static void TessellateQuad(Rect rect, Color32 color, float posZ, MeshWriteData mesh, ColorPage colorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		if (!(rect.width < kEpsilon) && !(rect.height < kEpsilon))
		{
			if (countOnly)
			{
				vertexCount += 4;
				indexCount += 6;
				return;
			}
			Color32 flags = new Color32(0, 0, 0, (byte)(colorPage.isValid ? 1 : 0));
			Color32 opacityColorPages = new Color32(0, 0, colorPage.pageAndID.r, colorPage.pageAndID.g);
			Color32 ids = new Color32(0, 0, 0, colorPage.pageAndID.b);
			Vector3 position = new Vector3(rect.x, rect.y, posZ);
			Vector3 position2 = new Vector3(rect.xMax, rect.y, posZ);
			Vector3 position3 = new Vector3(rect.x, rect.yMax, posZ);
			Vector3 position4 = new Vector3(rect.xMax, rect.yMax, posZ);
			mesh.SetNextVertex(new Vertex
			{
				position = position,
				tint = color,
				flags = flags,
				opacityColorPages = opacityColorPages,
				ids = ids
			});
			mesh.SetNextVertex(new Vertex
			{
				position = position2,
				tint = color,
				flags = flags,
				opacityColorPages = opacityColorPages,
				ids = ids
			});
			mesh.SetNextVertex(new Vertex
			{
				position = position3,
				tint = color,
				flags = flags,
				opacityColorPages = opacityColorPages,
				ids = ids
			});
			mesh.SetNextVertex(new Vertex
			{
				position = position4,
				tint = color,
				flags = flags,
				opacityColorPages = opacityColorPages,
				ids = ids
			});
			mesh.SetNextIndex(vertexCount);
			mesh.SetNextIndex((ushort)(vertexCount + 1));
			mesh.SetNextIndex((ushort)(vertexCount + 2));
			mesh.SetNextIndex((ushort)(vertexCount + 3));
			mesh.SetNextIndex((ushort)(vertexCount + 2));
			mesh.SetNextIndex((ushort)(vertexCount + 1));
			vertexCount += 4;
			indexCount += 6;
		}
	}

	private unsafe static void TessellateQuad(Rect rect, Edges smoothedEdges, Color32 color, float posZ, MeshWriteData mesh, ColorPage colorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		TessellateQuad(rect, smoothedEdges, null, color, posZ, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
	}

	private static int EdgesCount(Edges edges)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			if (((uint)edges & (uint)(1 << i)) != 0)
			{
				num++;
			}
		}
		return num;
	}

	private unsafe static void TessellateQuad(Rect rect, Edges smoothedEdges, Vector2* offsets, Color32 color, float posZ, MeshWriteData mesh, ColorPage colorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		if (rect.width < kEpsilon || rect.height < kEpsilon)
		{
			return;
		}
		if (smoothedEdges == Edges.None && offsets == null)
		{
			TessellateQuad(rect, color, posZ, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
			return;
		}
		if (EdgesCount(smoothedEdges) == 1 && offsets == null)
		{
			TessellateQuadSingleEdge(rect, smoothedEdges, color, posZ, mesh, colorPage, ref vertexCount, ref indexCount, countOnly);
			return;
		}
		if (countOnly)
		{
			vertexCount += 12;
			indexCount += 12;
			return;
		}
		Vector3* ptr = stackalloc Vector3[4];
		*ptr = new Vector3(rect.xMin, rect.yMax, posZ);
		ptr[1] = new Vector3(rect.xMin, rect.yMin, posZ);
		ptr[2] = new Vector3(rect.xMax, rect.yMin, posZ);
		ptr[3] = new Vector3(rect.xMax, rect.yMax, posZ);
		Vector3 vector = Vector3.zero;
		if (offsets != null)
		{
			*ptr += (Vector3)(*offsets);
			ptr[1] += (Vector3)offsets[1];
			ptr[2] += (Vector3)offsets[2];
			ptr[3] += (Vector3)offsets[3];
			vector += *ptr;
			vector += ptr[1];
			vector += ptr[2];
			vector += ptr[3];
			vector /= 4f;
			vector.z = posZ;
		}
		else
		{
			vector = new Vector3(rect.xMin + rect.width / 2f, rect.yMin + rect.height / 2f, posZ);
		}
		Color32 flags = new Color32(0, 0, 0, (byte)(colorPage.isValid ? 1 : 0));
		Color32 opacityColorPages = new Color32(0, 0, colorPage.pageAndID.r, colorPage.pageAndID.g);
		Color32 ids = new Color32(0, 0, 0, colorPage.pageAndID.b);
		ushort num = vertexCount;
		for (int i = 0; i < s_AllEdges.Length; i++)
		{
			Edges edges = s_AllEdges[i];
			Vector3 vector2 = ptr[i];
			Vector3 vector3 = ptr[(i + 1) % 4];
			float magnitude = ((vector2 + vector3) / 2f - vector).magnitude;
			Vertex v = new Vertex
			{
				position = vector2,
				tint = color,
				flags = flags,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			Vertex v2 = new Vertex
			{
				position = vector3,
				tint = color,
				flags = flags,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			Vertex center = new Vertex
			{
				position = vector,
				tint = color,
				flags = flags,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			if ((smoothedEdges & edges) == edges)
			{
				EncodeStraightArc(ref v, ref v2, ref center, magnitude);
			}
			mesh.SetNextVertex(v);
			mesh.SetNextVertex(v2);
			mesh.SetNextVertex(center);
			mesh.SetNextIndex(num++);
			mesh.SetNextIndex(num++);
			mesh.SetNextIndex(num++);
		}
		vertexCount += 12;
		indexCount += 12;
	}

	private static void EncodeStraightArc(ref Vertex v0, ref Vertex v1, ref Vertex center, float radius)
	{
		ExpandTriangle(ref v0.position, ref v1.position, center.position, 2f);
		Vector3 vector = (v0.position + v1.position) / 2f;
		float magnitude = (center.position - vector).magnitude;
		float x = magnitude / radius;
		center.circle = new Vector4(0f, 0f, kUnusedArc, kUnusedArc);
		v0.circle = new Vector4(x, 0f, kUnusedArc, kUnusedArc);
		v1.circle = new Vector4(x, 0f, kUnusedArc, kUnusedArc);
		v0.flags.b = 1;
		v1.flags.b = 1;
		center.flags.b = 1;
	}

	private static void ExpandTriangle(ref Vector3 v0, ref Vector3 v1, Vector3 center, float factor)
	{
		v0 += (v0 - center).normalized * factor;
		v1 += (v1 - center).normalized * factor;
	}

	private static void TessellateQuadSingleEdge(Rect rect, Edges smoothedEdge, Color32 color, float posZ, MeshWriteData mesh, ColorPage colorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		if (countOnly)
		{
			vertexCount += 4;
			indexCount += 6;
			return;
		}
		Vector3 position = new Vector3(rect.x, rect.y, posZ);
		Vector3 position2 = new Vector3(rect.x + rect.width, rect.y, posZ);
		Vector3 position3 = new Vector3(rect.x + rect.width, rect.y + rect.height, posZ);
		Vector3 position4 = new Vector3(rect.x, rect.y + rect.height, posZ);
		Vector2 vector = new Vector2(Mathf.Abs(position2.x - position.x), Mathf.Abs(position3.y - position2.y));
		Vector2 vector2 = new Vector2((vector.x + 2f) / vector.x, (vector.y + 2f) / vector.y);
		Vector4 circle = Vector4.zero;
		Vector4 vector3 = Vector4.zero;
		Vector4 vector4 = Vector4.zero;
		Vector4 vector5 = Vector4.zero;
		switch (smoothedEdge)
		{
		case Edges.Left:
			position.x -= 2f;
			position4.x -= 2f;
			vector5 = new Vector4(vector2.x, 0f, kUnusedArc, kUnusedArc);
			circle = vector5;
			vector4 = new Vector4(0f, 0f, kUnusedArc, kUnusedArc);
			vector3 = vector4;
			break;
		case Edges.Top:
			position.y -= 2f;
			position2.y -= 2f;
			vector3 = new Vector4(0f, vector2.y, kUnusedArc, kUnusedArc);
			circle = vector3;
			vector5 = new Vector4(0f, 0f, kUnusedArc, kUnusedArc);
			vector4 = vector5;
			break;
		case Edges.Right:
			position2.x += 2f;
			position3.x += 2f;
			vector4 = new Vector4(vector2.x, 0f, kUnusedArc, kUnusedArc);
			vector3 = vector4;
			vector5 = new Vector4(0f, 0f, kUnusedArc, kUnusedArc);
			circle = vector5;
			break;
		case Edges.Bottom:
			position3.y += 2f;
			position4.y += 2f;
			vector5 = new Vector4(0f, vector2.y, kUnusedArc, kUnusedArc);
			vector4 = vector5;
			vector3 = new Vector4(0f, 0f, kUnusedArc, kUnusedArc);
			circle = vector3;
			break;
		}
		Color32 flags = new Color32(0, 0, 1, (byte)(colorPage.isValid ? 1 : 0));
		Color32 opacityColorPages = new Color32(0, 0, colorPage.pageAndID.r, colorPage.pageAndID.g);
		Color32 ids = new Color32(0, 0, 0, colorPage.pageAndID.b);
		ushort num = vertexCount;
		mesh.SetNextVertex(new Vertex
		{
			position = position,
			tint = color,
			flags = flags,
			circle = circle,
			opacityColorPages = opacityColorPages,
			ids = ids
		});
		mesh.SetNextVertex(new Vertex
		{
			position = position2,
			tint = color,
			flags = flags,
			circle = vector3,
			opacityColorPages = opacityColorPages,
			ids = ids
		});
		mesh.SetNextVertex(new Vertex
		{
			position = position3,
			tint = color,
			flags = flags,
			circle = vector4,
			opacityColorPages = opacityColorPages,
			ids = ids
		});
		mesh.SetNextVertex(new Vertex
		{
			position = position4,
			tint = color,
			flags = flags,
			circle = vector5,
			opacityColorPages = opacityColorPages,
			ids = ids
		});
		mesh.SetNextIndex(num);
		mesh.SetNextIndex((ushort)(num + 1));
		mesh.SetNextIndex((ushort)(num + 2));
		mesh.SetNextIndex(num);
		mesh.SetNextIndex((ushort)(num + 2));
		mesh.SetNextIndex((ushort)(num + 3));
		vertexCount += 4;
		indexCount += 6;
	}

	private static void TessellateStraightBorder(Rect rect, Edges smoothedEdge, float miterOffset, Color color, float posZ, MeshWriteData mesh, ColorPage colorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		Debug.Assert(smoothedEdge == Edges.Left || smoothedEdge == Edges.Top);
		if (rect.width < kEpsilon || rect.height < kEpsilon)
		{
			return;
		}
		if (countOnly)
		{
			vertexCount += 4;
			indexCount += 6;
			return;
		}
		Vector3 vector = new Vector3(rect.xMin, rect.yMin, posZ);
		Vector3 vector2 = new Vector3(rect.xMax, rect.yMin, posZ);
		Vector3 position = new Vector3(rect.xMax, rect.yMax, posZ);
		Vector3 vector3 = new Vector3(rect.xMin, rect.yMax, posZ);
		Color32 flags = new Color32(0, 0, 1, (byte)(colorPage.isValid ? 1 : 0));
		Color32 opacityColorPages = new Color32(0, 0, colorPage.pageAndID.r, colorPage.pageAndID.g);
		Color32 ids = new Color32(0, 0, 0, colorPage.pageAndID.b);
		if (smoothedEdge == Edges.Left)
		{
			Vector3 vector4 = vector;
			Vector3 vector5 = vector2;
			vector.x -= 2f;
			vector2.x += 2f;
			position.x += 2f;
			vector3.x -= 2f;
			float num = vector2.x - vector.x;
			Vector4 circle = new Vector4(num / (rect.width + 2f), 0f, num / 2f, 0f);
			Vector4 zero = Vector4.zero;
			Vertex v = new Vertex
			{
				position = vector,
				tint = color,
				flags = flags,
				circle = circle,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			Vertex v2 = new Vertex
			{
				position = vector2,
				tint = color,
				flags = flags,
				circle = zero,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			Vertex v3 = new Vertex
			{
				position = position,
				tint = color,
				flags = flags,
				circle = zero,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			Vertex nextVertex = new Vertex
			{
				position = vector3,
				tint = color,
				flags = flags,
				circle = circle,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			vector = vector4;
			vector2 = vector5;
			vector2.y += miterOffset;
			Vector3 vector6 = (vector2 - vector).normalized * 1.4142f * 2f;
			vector -= vector6;
			vector2 += vector6;
			v.circle = GetInterpolatedCircle(vector, ref v, ref v2, ref v3);
			v.position = vector;
			v2.circle = GetInterpolatedCircle(vector2, ref v, ref v2, ref v3);
			v2.position = vector2;
			mesh.SetNextVertex(v);
			mesh.SetNextVertex(v2);
			mesh.SetNextVertex(v3);
			mesh.SetNextVertex(nextVertex);
		}
		else
		{
			Vector3 vector7 = vector;
			Vector3 vector8 = vector3;
			vector.y -= 2f;
			vector2.y -= 2f;
			position.y += 2f;
			vector3.y += 2f;
			float num2 = vector3.y - vector.y;
			Vector4 circle2 = new Vector4(0f, num2 / (rect.height + 2f), 0f, num2 / 2f);
			Vector4 zero2 = Vector4.zero;
			Vertex v4 = new Vertex
			{
				position = vector,
				tint = color,
				flags = flags,
				circle = circle2,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			Vertex v5 = new Vertex
			{
				position = vector2,
				tint = color,
				flags = flags,
				circle = circle2,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			Vertex v6 = new Vertex
			{
				position = position,
				tint = color,
				flags = flags,
				circle = zero2,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			Vertex nextVertex2 = new Vertex
			{
				position = vector3,
				tint = color,
				flags = flags,
				circle = zero2,
				opacityColorPages = opacityColorPages,
				ids = ids
			};
			vector = vector7;
			vector3 = vector8;
			vector3.x += miterOffset;
			Vector3 vector9 = (vector3 - vector).normalized * 1.4142f * 2f;
			vector -= vector9;
			vector3 += vector9;
			v4.circle = GetInterpolatedCircle(vector, ref v4, ref v5, ref v6);
			v4.position = vector;
			nextVertex2.circle = GetInterpolatedCircle(vector3, ref v4, ref v5, ref v6);
			nextVertex2.position = vector3;
			mesh.SetNextVertex(v4);
			mesh.SetNextVertex(v5);
			mesh.SetNextVertex(v6);
			mesh.SetNextVertex(nextVertex2);
		}
		ushort num3 = vertexCount;
		mesh.SetNextIndex(num3);
		mesh.SetNextIndex((ushort)(num3 + 1));
		mesh.SetNextIndex((ushort)(num3 + 2));
		mesh.SetNextIndex((ushort)(num3 + 2));
		mesh.SetNextIndex((ushort)(num3 + 3));
		mesh.SetNextIndex(num3);
		vertexCount += 4;
		indexCount += 6;
	}

	private static Vector4 GetInterpolatedCircle(Vector2 p, ref Vertex v0, ref Vertex v1, ref Vertex v2)
	{
		ComputeBarycentricCoordinates(p, v0.position, v1.position, v2.position, out var u, out var v3, out var w);
		return v0.circle * u + v1.circle * v3 + v2.circle * w;
	}

	private static void ComputeBarycentricCoordinates(Vector2 p, Vector2 a, Vector2 b, Vector2 c, out float u, out float v, out float w)
	{
		Vector2 vector = b - a;
		Vector2 vector2 = c - a;
		Vector2 lhs = p - a;
		float num = Vector2.Dot(vector, vector);
		float num2 = Vector2.Dot(vector, vector2);
		float num3 = Vector2.Dot(vector2, vector2);
		float num4 = Vector2.Dot(lhs, vector);
		float num5 = Vector2.Dot(lhs, vector2);
		float num6 = num * num3 - num2 * num2;
		v = (num3 * num4 - num2 * num5) / num6;
		w = (num * num5 - num2 * num4) / num6;
		u = 1f - v - w;
	}

	private static void TessellateFilledFan(Vector2 center, Vector2 radius, Vector2 miterOffset, float leftWidth, float topWidth, Color32 leftColor, Color32 topColor, float posZ, MeshWriteData mesh, ColorPage leftColorPage, ColorPage topColorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		if (countOnly)
		{
			vertexCount += 6;
			indexCount += 6;
			return;
		}
		Color32 flags = new Color32(0, 0, 1, (byte)(leftColorPage.isValid ? 1 : 0));
		Color32 opacityColorPages = new Color32(0, 0, leftColorPage.pageAndID.r, leftColorPage.pageAndID.g);
		Color32 ids = new Color32(0, 0, 0, leftColorPage.pageAndID.b);
		Color32 flags2 = new Color32(0, 0, 1, (byte)(topColorPage.isValid ? 1 : 0));
		Color32 opacityColorPages2 = new Color32(0, 0, topColorPage.pageAndID.r, topColorPage.pageAndID.g);
		Color32 ids2 = new Color32(0, 0, 0, topColorPage.pageAndID.b);
		Vertex v = default(Vertex);
		Vertex v2 = v;
		Vertex v3 = v;
		Vertex nextVertex = v;
		v.position = new Vector3(center.x, center.y, posZ);
		v2.position = new Vector3(center.x - radius.x, center.y, posZ);
		v3.position = new Vector3(center.x - radius.x, center.y - radius.y, posZ);
		nextVertex.position = new Vector3(center.x, center.y - radius.y, posZ);
		v.circle = new Vector4(0f, 0f, kUnusedArc, kUnusedArc);
		v2.circle = new Vector4(1f, 0f, kUnusedArc, kUnusedArc);
		v3.circle = new Vector4(1f, 1f, kUnusedArc, kUnusedArc);
		nextVertex.circle = new Vector4(0f, 1f, kUnusedArc, kUnusedArc);
		if (miterOffset != Vector2.zero)
		{
			Vector3 vector = v.position + (Vector3)miterOffset;
			v.circle = GetInterpolatedCircle(vector, ref v, ref v2, ref v3);
			v.position = vector;
		}
		Vertex nextVertex2 = v3;
		Vertex nextVertex3 = v;
		v.tint = leftColor;
		v2.tint = leftColor;
		nextVertex2.tint = leftColor;
		v3.tint = topColor;
		nextVertex.tint = topColor;
		nextVertex3.tint = topColor;
		v.flags = flags;
		v.opacityColorPages = opacityColorPages;
		v.ids = ids;
		v2.flags = flags;
		v2.opacityColorPages = opacityColorPages;
		v2.ids = ids;
		nextVertex2.flags = flags;
		nextVertex2.opacityColorPages = opacityColorPages;
		nextVertex2.ids = ids;
		v3.flags = flags2;
		v3.opacityColorPages = opacityColorPages2;
		v3.ids = ids2;
		nextVertex.flags = flags2;
		nextVertex.opacityColorPages = opacityColorPages2;
		nextVertex.ids = ids2;
		nextVertex3.flags = flags2;
		nextVertex3.opacityColorPages = opacityColorPages2;
		nextVertex3.ids = ids2;
		mesh.SetNextVertex(v);
		mesh.SetNextVertex(v2);
		mesh.SetNextVertex(nextVertex2);
		mesh.SetNextVertex(v3);
		mesh.SetNextVertex(nextVertex);
		mesh.SetNextVertex(nextVertex3);
		mesh.SetNextIndex(vertexCount);
		mesh.SetNextIndex((ushort)(vertexCount + 1));
		mesh.SetNextIndex((ushort)(vertexCount + 2));
		mesh.SetNextIndex((ushort)(vertexCount + 3));
		mesh.SetNextIndex((ushort)(vertexCount + 4));
		mesh.SetNextIndex((ushort)(vertexCount + 5));
		vertexCount += 6;
		indexCount += 6;
	}

	private static void TessellateBorderedFan(Vector2 center, Vector2 outerRadius, float leftWidth, float topWidth, Color32 leftColor, Color32 topColor, float posZ, MeshWriteData mesh, ColorPage leftColorPage, ColorPage topColorPage, ref ushort vertexCount, ref ushort indexCount, bool countOnly)
	{
		if (countOnly)
		{
			vertexCount += 6;
			indexCount += 6;
			return;
		}
		Vector2 vector = new Vector2(outerRadius.x - leftWidth, outerRadius.y - topWidth);
		Color32 flags = new Color32(0, 0, 1, (byte)(leftColorPage.isValid ? 1 : 0));
		Color32 opacityColorPages = new Color32(0, 0, leftColorPage.pageAndID.r, leftColorPage.pageAndID.g);
		Color32 ids = new Color32(0, 0, 0, leftColorPage.pageAndID.b);
		Color32 flags2 = new Color32(0, 0, 1, (byte)(topColorPage.isValid ? 1 : 0));
		Color32 opacityColorPages2 = new Color32(0, 0, topColorPage.pageAndID.r, topColorPage.pageAndID.g);
		Color32 ids2 = new Color32(0, 0, 0, topColorPage.pageAndID.b);
		Vertex vertex = default(Vertex);
		Vertex nextVertex = vertex;
		Vertex vertex2 = vertex;
		Vertex nextVertex2 = vertex;
		vertex.position = new Vector3(center.x, center.y, posZ);
		nextVertex.position = new Vector3(center.x - outerRadius.x, center.y, posZ);
		vertex2.position = new Vector3(center.x - outerRadius.x, center.y - outerRadius.y, posZ);
		nextVertex2.position = new Vector3(center.x, center.y - outerRadius.y, posZ);
		Vector2 vector2 = outerRadius / vector;
		vertex.circle = new Vector4(0f, 0f, 0f, 0f);
		nextVertex.circle = new Vector4(1f, 0f, vector2.x, 0f);
		vertex2.circle = new Vector4(1f, 1f, vector2.x, vector2.y);
		nextVertex2.circle = new Vector4(0f, 1f, 0f, vector2.y);
		Vertex nextVertex3 = vertex2;
		Vertex nextVertex4 = vertex;
		vertex.tint = leftColor;
		nextVertex.tint = leftColor;
		nextVertex3.tint = leftColor;
		vertex2.tint = topColor;
		nextVertex2.tint = topColor;
		nextVertex4.tint = topColor;
		vertex.flags = flags;
		vertex.opacityColorPages = opacityColorPages;
		vertex.ids = ids;
		nextVertex.flags = flags;
		nextVertex.opacityColorPages = opacityColorPages;
		nextVertex.ids = ids;
		nextVertex3.flags = flags;
		nextVertex3.opacityColorPages = opacityColorPages;
		nextVertex3.ids = ids;
		vertex2.flags = flags2;
		vertex2.opacityColorPages = opacityColorPages2;
		vertex2.ids = ids2;
		nextVertex2.flags = flags2;
		nextVertex2.opacityColorPages = opacityColorPages2;
		nextVertex2.ids = ids2;
		nextVertex4.flags = flags2;
		nextVertex4.opacityColorPages = opacityColorPages2;
		nextVertex4.ids = ids2;
		mesh.SetNextVertex(vertex);
		mesh.SetNextVertex(nextVertex);
		mesh.SetNextVertex(nextVertex3);
		mesh.SetNextVertex(vertex2);
		mesh.SetNextVertex(nextVertex2);
		mesh.SetNextVertex(nextVertex4);
		mesh.SetNextIndex(vertexCount);
		mesh.SetNextIndex((ushort)(vertexCount + 1));
		mesh.SetNextIndex((ushort)(vertexCount + 2));
		mesh.SetNextIndex((ushort)(vertexCount + 3));
		mesh.SetNextIndex((ushort)(vertexCount + 4));
		mesh.SetNextIndex((ushort)(vertexCount + 5));
		vertexCount += 6;
		indexCount += 6;
	}

	private static void MirrorVertices(Rect rect, NativeSlice<Vertex> vertices, int vertexStart, int vertexCount, bool flipHorizontal)
	{
		if (flipHorizontal)
		{
			for (int i = 0; i < vertexCount; i++)
			{
				Vertex value = vertices[vertexStart + i];
				value.position.x = rect.xMax - (value.position.x - rect.xMax);
				value.uv.x = 0f - value.uv.x;
				vertices[vertexStart + i] = value;
			}
		}
		else
		{
			for (int j = 0; j < vertexCount; j++)
			{
				Vertex value2 = vertices[vertexStart + j];
				value2.position.y = rect.yMax - (value2.position.y - rect.yMax);
				value2.uv.y = 0f - value2.uv.y;
				vertices[vertexStart + j] = value2;
			}
		}
	}

	private static void FlipWinding(NativeSlice<ushort> indices, int indexStart, int indexCount)
	{
		for (int i = 0; i < indexCount; i += 3)
		{
			ushort value = indices[indexStart + i];
			indices[indexStart + i] = indices[indexStart + i + 1];
			indices[indexStart + i + 1] = value;
		}
	}

	private static void ComputeUVs(Rect tessellatedRect, Rect textureRect, Rect uvRegion, NativeSlice<Vertex> vertices)
	{
		Vector2 position = tessellatedRect.position;
		Vector2 vector = new Vector2(1f / tessellatedRect.width, 1f / tessellatedRect.height);
		for (int i = 0; i < vertices.Length; i++)
		{
			Vertex value = vertices[i];
			Vector2 vector2 = value.position;
			vector2 -= position;
			vector2 *= vector;
			value.uv.x = (vector2.x * textureRect.width + textureRect.xMin) * uvRegion.width + uvRegion.xMin;
			value.uv.y = ((1f - vector2.y) * textureRect.height + textureRect.yMin) * uvRegion.height + uvRegion.yMin;
			vertices[i] = value;
		}
	}
}
