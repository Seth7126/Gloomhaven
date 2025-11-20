#define UNITY_ASSERTIONS
using System.Collections.Generic;
using Unity.Collections;
using Unity.Profiling;

namespace UnityEngine.UIElements.UIR.Implementation;

internal static class CommandGenerator
{
	private static readonly ProfilerMarker k_ConvertEntriesToCommandsMarker = new ProfilerMarker("UIR.ConvertEntriesToCommands");

	private static readonly ProfilerMarker k_NudgeVerticesMarker = new ProfilerMarker("UIR.NudgeVertices");

	private static Material s_blitMaterial_LinearToGamma;

	private static Material s_blitMaterial_GammaToLinear;

	private static Material s_blitMaterial_NoChange;

	private static Shader s_blitShader;

	private static void GetVerticesTransformInfo(VisualElement ve, out Matrix4x4 transform)
	{
		if (RenderChainVEData.AllocatesID(ve.renderChainData.transformID) || (ve.renderHints & RenderHints.GroupTransform) != RenderHints.None)
		{
			transform = Matrix4x4.identity;
		}
		else if (ve.renderChainData.boneTransformAncestor != null)
		{
			VisualElement.MultiplyMatrix34(ref ve.renderChainData.boneTransformAncestor.worldTransformInverse, ref ve.worldTransformRef, out transform);
		}
		else if (ve.renderChainData.groupTransformAncestor != null)
		{
			VisualElement.MultiplyMatrix34(ref ve.renderChainData.groupTransformAncestor.worldTransformInverse, ref ve.worldTransformRef, out transform);
		}
		else
		{
			transform = ve.worldTransform;
		}
		transform.m22 = 1f;
	}

	private static bool IsParentOrAncestorOf(this VisualElement ve, VisualElement child)
	{
		while (child.hierarchy.parent != null)
		{
			if (child.hierarchy.parent == ve)
			{
				return true;
			}
			child = child.hierarchy.parent;
		}
		return false;
	}

	public static UIRStylePainter.ClosingInfo PaintElement(RenderChain renderChain, VisualElement ve, ref ChainBuilderStats stats)
	{
		UIRenderDevice device = renderChain.device;
		bool flag = ve.renderChainData.clipMethod == ClipMethod.Stencil;
		bool flag2 = ve.renderChainData.clipMethod == ClipMethod.Scissor;
		bool flag3 = (ve.renderHints & RenderHints.GroupTransform) != 0;
		if ((UIRUtility.IsElementSelfHidden(ve) && !flag && !flag2 && !flag3) || ve.renderChainData.isHierarchyHidden)
		{
			if (ve.renderChainData.data != null)
			{
				device.Free(ve.renderChainData.data);
				ve.renderChainData.data = null;
			}
			if (ve.renderChainData.firstCommand != null)
			{
				ResetCommands(renderChain, ve);
			}
			renderChain.ResetTextures(ve);
			return default(UIRStylePainter.ClosingInfo);
		}
		RenderChainCommand renderChainCommand = ve.renderChainData.firstCommand?.prev;
		RenderChainCommand renderChainCommand2 = ve.renderChainData.lastCommand?.next;
		bool flag4 = ve.renderChainData.firstClosingCommand != null && renderChainCommand2 == ve.renderChainData.firstClosingCommand;
		RenderChainCommand renderChainCommand3;
		RenderChainCommand renderChainCommand4;
		if (flag4)
		{
			renderChainCommand2 = ve.renderChainData.lastClosingCommand.next;
			renderChainCommand3 = (renderChainCommand4 = null);
		}
		else
		{
			renderChainCommand3 = ve.renderChainData.firstClosingCommand?.prev;
			renderChainCommand4 = ve.renderChainData.lastClosingCommand?.next;
		}
		Debug.Assert(renderChainCommand?.owner != ve);
		Debug.Assert(renderChainCommand2?.owner != ve);
		Debug.Assert(renderChainCommand3?.owner != ve);
		Debug.Assert(renderChainCommand4?.owner != ve);
		ResetCommands(renderChain, ve);
		renderChain.ResetTextures(ve);
		UIRStylePainter painter = renderChain.painter;
		painter.Begin(ve);
		if (ve.visible)
		{
			painter.DrawVisualElementBackground();
			painter.DrawVisualElementBorder();
			painter.ApplyVisualElementClipping();
			ve.InvokeGenerateVisualContent(painter.meshGenerationContext);
		}
		else if (flag2 || flag)
		{
			painter.ApplyVisualElementClipping();
		}
		MeshHandle data = ve.renderChainData.data;
		if (painter.totalVertices > device.maxVerticesPerPage)
		{
			Debug.LogError(string.Format("A {0} must not allocate more than {1} vertices.", "VisualElement", device.maxVerticesPerPage));
			if (data != null)
			{
				device.Free(data);
				data = null;
			}
			renderChain.ResetTextures(ve);
			painter.Reset();
			painter.Begin(ve);
		}
		List<UIRStylePainter.Entry> entries = painter.entries;
		if (entries.Count > 0)
		{
			NativeSlice<Vertex> verts = default(NativeSlice<Vertex>);
			NativeSlice<ushort> indices = default(NativeSlice<ushort>);
			ushort indexOffset = 0;
			if (painter.totalVertices > 0)
			{
				UpdateOrAllocate(ref data, painter.totalVertices, painter.totalIndices, device, out verts, out indices, out indexOffset, ref stats);
			}
			int num = 0;
			int num2 = 0;
			RenderChainCommand prev = renderChainCommand;
			RenderChainCommand next = renderChainCommand2;
			if (renderChainCommand == null && renderChainCommand2 == null)
			{
				FindCommandInsertionPoint(ve, out prev, out next);
			}
			bool flag5 = false;
			Matrix4x4 transform = Matrix4x4.identity;
			Color32 xformClipPages = new Color32(0, 0, 0, 0);
			Color32 ids = new Color32(0, 0, 0, 0);
			Color32 addFlags = new Color32(0, 0, 0, 0);
			Color32 opacityPage = new Color32(0, 0, 0, 0);
			Color32 textCoreSettingsPage = new Color32(0, 0, 0, 0);
			int num3 = -1;
			int num4 = -1;
			foreach (UIRStylePainter.Entry entry in painter.entries)
			{
				NativeSlice<Vertex> vertices = entry.vertices;
				if (vertices.Length > 0)
				{
					NativeSlice<ushort> indices2 = entry.indices;
					if (indices2.Length > 0)
					{
						if (!flag5)
						{
							flag5 = true;
							GetVerticesTransformInfo(ve, out transform);
							ve.renderChainData.verticesSpace = transform;
						}
						Color32 color = renderChain.shaderInfoAllocator.TransformAllocToVertexData(ve.renderChainData.transformID);
						Color32 color2 = renderChain.shaderInfoAllocator.OpacityAllocToVertexData(ve.renderChainData.opacityID);
						Color32 color3 = renderChain.shaderInfoAllocator.TextCoreSettingsToVertexData(ve.renderChainData.textCoreSettingsID);
						xformClipPages.r = color.r;
						xformClipPages.g = color.g;
						ids.r = color.b;
						opacityPage.r = color2.r;
						opacityPage.g = color2.g;
						ids.b = color2.b;
						if (entry.isTextEntry)
						{
							textCoreSettingsPage.r = color3.r;
							textCoreSettingsPage.g = color3.g;
							ids.a = color3.b;
						}
						Color32 color4 = renderChain.shaderInfoAllocator.ClipRectAllocToVertexData(entry.clipRectID);
						xformClipPages.b = color4.r;
						xformClipPages.a = color4.g;
						ids.g = color4.b;
						addFlags.r = (byte)entry.addFlags;
						TextureId texture = entry.texture;
						float textureId = texture.ConvertToGpu();
						NativeSlice<Vertex> thisSlice = verts;
						int start = num;
						vertices = entry.vertices;
						NativeSlice<Vertex> nativeSlice = thisSlice.Slice(start, vertices.Length);
						if (entry.uvIsDisplacement)
						{
							if (num3 < 0)
							{
								num3 = num;
								int num5 = num;
								vertices = entry.vertices;
								num4 = num5 + vertices.Length;
							}
							else if (num4 == num)
							{
								int num6 = num4;
								vertices = entry.vertices;
								num4 = num6 + vertices.Length;
							}
							else
							{
								ve.renderChainData.disableNudging = true;
							}
							CopyTransformVertsPosAndVec(entry.vertices, nativeSlice, transform, xformClipPages, ids, addFlags, opacityPage, textCoreSettingsPage, entry.isTextEntry, textureId);
						}
						else
						{
							CopyTransformVertsPos(entry.vertices, nativeSlice, transform, xformClipPages, ids, addFlags, opacityPage, textCoreSettingsPage, entry.isTextEntry, textureId);
						}
						indices2 = entry.indices;
						int length = indices2.Length;
						int indexOffset2 = num + indexOffset;
						NativeSlice<ushort> nativeSlice2 = indices.Slice(num2, length);
						bool flag6 = UIRUtility.ShapeWindingIsClockwise(entry.maskDepth, entry.stencilRef);
						bool worldFlipsWinding = ve.renderChainData.worldFlipsWinding;
						if (flag6 ^ worldFlipsWinding)
						{
							CopyTriangleIndices(entry.indices, nativeSlice2, indexOffset2);
						}
						else
						{
							CopyTriangleIndicesFlipWindingOrder(entry.indices, nativeSlice2, indexOffset2);
						}
						if (entry.isClipRegisterEntry)
						{
							painter.LandClipRegisterMesh(nativeSlice, nativeSlice2, indexOffset2);
						}
						RenderChainCommand renderChainCommand5 = InjectMeshDrawCommand(renderChain, ve, ref prev, ref next, data, length, num2, entry.material, entry.texture, entry.font, entry.stencilRef);
						if (entry.isTextEntry && ve.renderChainData.usesLegacyText)
						{
							if (ve.renderChainData.textEntries == null)
							{
								ve.renderChainData.textEntries = new List<RenderChainTextEntry>(1);
							}
							List<RenderChainTextEntry> textEntries = ve.renderChainData.textEntries;
							RenderChainTextEntry item = new RenderChainTextEntry
							{
								command = renderChainCommand5,
								firstVertex = num
							};
							vertices = entry.vertices;
							item.vertexCount = vertices.Length;
							textEntries.Add(item);
						}
						else if (entry.isTextEntry)
						{
							renderChainCommand5.state.fontTexSDFScale = entry.fontTexSDFScale;
						}
						int num7 = num;
						vertices = entry.vertices;
						num = num7 + vertices.Length;
						num2 += length;
						continue;
					}
				}
				if (entry.customCommand != null)
				{
					InjectCommandInBetween(renderChain, entry.customCommand, ref prev, ref next);
				}
				else
				{
					Debug.Assert(condition: false);
				}
			}
			if (!ve.renderChainData.disableNudging && num3 >= 0)
			{
				ve.renderChainData.displacementUVStart = num3;
				ve.renderChainData.displacementUVEnd = num4;
			}
		}
		else if (data != null)
		{
			device.Free(data);
			data = null;
		}
		ve.renderChainData.data = data;
		if (ve.renderChainData.usesLegacyText)
		{
			renderChain.AddTextElement(ve);
		}
		if (painter.closingInfo.clipperRegisterIndices.Length == 0 && ve.renderChainData.closingData != null)
		{
			device.Free(ve.renderChainData.closingData);
			ve.renderChainData.closingData = null;
		}
		if (painter.closingInfo.needsClosing)
		{
			RenderChainCommand prev2 = renderChainCommand3;
			RenderChainCommand next2 = renderChainCommand4;
			if (flag4)
			{
				prev2 = ve.renderChainData.lastCommand;
				next2 = prev2.next;
			}
			else if (prev2 == null && next2 == null)
			{
				FindClosingCommandInsertionPoint(ve, out prev2, out next2);
			}
			if (painter.closingInfo.PopDefaultMaterial)
			{
				RenderChainCommand renderChainCommand6 = renderChain.AllocCommand();
				renderChainCommand6.type = CommandType.PopDefaultMaterial;
				renderChainCommand6.closing = true;
				renderChainCommand6.owner = ve;
				InjectClosingCommandInBetween(renderChain, renderChainCommand6, ref prev2, ref next2);
			}
			if (painter.closingInfo.blitAndPopRenderTexture)
			{
				RenderChainCommand renderChainCommand7 = renderChain.AllocCommand();
				renderChainCommand7.type = CommandType.BlitToPreviousRT;
				renderChainCommand7.closing = true;
				renderChainCommand7.owner = ve;
				renderChainCommand7.state.material = GetBlitMaterial(ve.subRenderTargetMode);
				Debug.Assert(renderChainCommand7.state.material != null);
				InjectClosingCommandInBetween(renderChain, renderChainCommand7, ref prev2, ref next2);
				RenderChainCommand renderChainCommand8 = renderChain.AllocCommand();
				renderChainCommand8.type = CommandType.PopRenderTexture;
				renderChainCommand8.closing = true;
				renderChainCommand8.owner = ve;
				InjectClosingCommandInBetween(renderChain, renderChainCommand8, ref prev2, ref next2);
			}
			if (painter.closingInfo.clipperRegisterIndices.Length > 0)
			{
				RenderChainCommand cmd = InjectClosingMeshDrawCommand(renderChain, ve, ref prev2, ref next2, null, 0, 0, null, TextureId.invalid, null, painter.closingInfo.maskStencilRef);
				painter.LandClipUnregisterMeshDrawCommand(cmd);
			}
			if (painter.closingInfo.popViewMatrix)
			{
				RenderChainCommand renderChainCommand9 = renderChain.AllocCommand();
				renderChainCommand9.type = CommandType.PopView;
				renderChainCommand9.closing = true;
				renderChainCommand9.owner = ve;
				InjectClosingCommandInBetween(renderChain, renderChainCommand9, ref prev2, ref next2);
			}
			if (painter.closingInfo.popScissorClip)
			{
				RenderChainCommand renderChainCommand10 = renderChain.AllocCommand();
				renderChainCommand10.type = CommandType.PopScissor;
				renderChainCommand10.closing = true;
				renderChainCommand10.owner = ve;
				InjectClosingCommandInBetween(renderChain, renderChainCommand10, ref prev2, ref next2);
			}
		}
		Debug.Assert(ve.renderChainData.closingData == null || ve.renderChainData.data != null);
		UIRStylePainter.ClosingInfo closingInfo = painter.closingInfo;
		painter.Reset();
		return closingInfo;
	}

	private static Material CreateBlitShader(float colorConversion)
	{
		if (s_blitShader == null)
		{
			s_blitShader = Shader.Find("Hidden/UIE-ColorConversionBlit");
		}
		Debug.Assert(s_blitShader != null, "UI Tollkit Render Event: Shader Not found");
		Material material = new Material(s_blitShader);
		material.hideFlags |= HideFlags.DontSaveInEditor;
		material.SetFloat("_ColorConversion", colorConversion);
		return material;
	}

	private static Material GetBlitMaterial(VisualElement.RenderTargetMode mode)
	{
		switch (mode)
		{
		case VisualElement.RenderTargetMode.GammaToLinear:
			if (s_blitMaterial_GammaToLinear == null)
			{
				s_blitMaterial_GammaToLinear = CreateBlitShader(-1f);
			}
			return s_blitMaterial_GammaToLinear;
		case VisualElement.RenderTargetMode.LinearToGamma:
			if (s_blitMaterial_LinearToGamma == null)
			{
				s_blitMaterial_LinearToGamma = CreateBlitShader(1f);
			}
			return s_blitMaterial_LinearToGamma;
		case VisualElement.RenderTargetMode.NoColorConversion:
			if (s_blitMaterial_NoChange == null)
			{
				s_blitMaterial_NoChange = CreateBlitShader(0f);
			}
			return s_blitMaterial_NoChange;
		default:
			Debug.LogError($"No Shader for Unsupported RenderTargetMode: {mode}");
			return null;
		}
	}

	public static void ClosePaintElement(VisualElement ve, UIRStylePainter.ClosingInfo closingInfo, RenderChain renderChain, ref ChainBuilderStats stats)
	{
		if (closingInfo.clipperRegisterIndices.Length > 0)
		{
			NativeSlice<Vertex> verts = default(NativeSlice<Vertex>);
			NativeSlice<ushort> indices = default(NativeSlice<ushort>);
			ushort indexOffset = 0;
			UpdateOrAllocate(ref ve.renderChainData.closingData, closingInfo.clipperRegisterVertices.Length, closingInfo.clipperRegisterIndices.Length, renderChain.device, out verts, out indices, out indexOffset, ref stats);
			verts.CopyFrom(closingInfo.clipperRegisterVertices);
			CopyTriangleIndicesFlipWindingOrder(closingInfo.clipperRegisterIndices, indices, indexOffset - closingInfo.clipperRegisterIndexOffset);
			closingInfo.clipUnregisterDrawCommand.mesh = ve.renderChainData.closingData;
			closingInfo.clipUnregisterDrawCommand.indexCount = indices.Length;
		}
	}

	private static void UpdateOrAllocate(ref MeshHandle data, int vertexCount, int indexCount, UIRenderDevice device, out NativeSlice<Vertex> verts, out NativeSlice<ushort> indices, out ushort indexOffset, ref ChainBuilderStats stats)
	{
		if (data != null)
		{
			if (data.allocVerts.size >= vertexCount && data.allocIndices.size >= indexCount)
			{
				device.Update(data, (uint)vertexCount, (uint)indexCount, out verts, out indices, out indexOffset);
				stats.updatedMeshAllocations++;
			}
			else
			{
				device.Free(data);
				data = device.Allocate((uint)vertexCount, (uint)indexCount, out verts, out indices, out indexOffset);
				stats.newMeshAllocations++;
			}
		}
		else
		{
			data = device.Allocate((uint)vertexCount, (uint)indexCount, out verts, out indices, out indexOffset);
			stats.newMeshAllocations++;
		}
	}

	private static void CopyTransformVertsPos(NativeSlice<Vertex> source, NativeSlice<Vertex> target, Matrix4x4 mat, Color32 xformClipPages, Color32 ids, Color32 addFlags, Color32 opacityPage, Color32 textCoreSettingsPage, bool isText, float textureId)
	{
		int length = source.Length;
		for (int i = 0; i < length; i++)
		{
			Vertex value = source[i];
			value.position = mat.MultiplyPoint3x4(value.position);
			value.xformClipPages = xformClipPages;
			value.ids.r = ids.r;
			value.ids.g = ids.g;
			value.ids.b = ids.b;
			value.flags.r += addFlags.r;
			value.opacityColorPages.r = opacityPage.r;
			value.opacityColorPages.g = opacityPage.g;
			if (isText)
			{
				value.opacityColorPages.b = textCoreSettingsPage.r;
				value.opacityColorPages.a = textCoreSettingsPage.g;
				value.ids.a = ids.a;
			}
			value.textureId = textureId;
			target[i] = value;
		}
	}

	private static void CopyTransformVertsPosAndVec(NativeSlice<Vertex> source, NativeSlice<Vertex> target, Matrix4x4 mat, Color32 xformClipPages, Color32 ids, Color32 addFlags, Color32 opacityPage, Color32 textCoreSettingsPage, bool isText, float textureId)
	{
		int length = source.Length;
		Vector3 vector = new Vector3(0f, 0f, 0f);
		for (int i = 0; i < length; i++)
		{
			Vertex value = source[i];
			value.position = mat.MultiplyPoint3x4(value.position);
			vector.x = value.uv.x;
			vector.y = value.uv.y;
			value.uv = mat.MultiplyVector(vector);
			value.xformClipPages = xformClipPages;
			value.ids.r = ids.r;
			value.ids.g = ids.g;
			value.ids.b = ids.b;
			value.flags.r += addFlags.r;
			value.opacityColorPages.r = opacityPage.r;
			value.opacityColorPages.g = opacityPage.g;
			if (isText)
			{
				value.opacityColorPages.b = textCoreSettingsPage.r;
				value.opacityColorPages.a = textCoreSettingsPage.g;
				value.ids.a = ids.a;
			}
			value.textureId = textureId;
			target[i] = value;
		}
	}

	private static void CopyTriangleIndicesFlipWindingOrder(NativeSlice<ushort> source, NativeSlice<ushort> target)
	{
		Debug.Assert(source != target);
		int length = source.Length;
		for (int i = 0; i < length; i += 3)
		{
			ushort value = source[i];
			target[i] = source[i + 1];
			target[i + 1] = value;
			target[i + 2] = source[i + 2];
		}
	}

	private static void CopyTriangleIndicesFlipWindingOrder(NativeSlice<ushort> source, NativeSlice<ushort> target, int indexOffset)
	{
		Debug.Assert(source != target);
		int length = source.Length;
		for (int i = 0; i < length; i += 3)
		{
			ushort value = (ushort)(source[i] + indexOffset);
			target[i] = (ushort)(source[i + 1] + indexOffset);
			target[i + 1] = value;
			target[i + 2] = (ushort)(source[i + 2] + indexOffset);
		}
	}

	private static void CopyTriangleIndices(NativeSlice<ushort> source, NativeSlice<ushort> target, int indexOffset)
	{
		int length = source.Length;
		for (int i = 0; i < length; i++)
		{
			target[i] = (ushort)(source[i] + indexOffset);
		}
	}

	public static bool NudgeVerticesToNewSpace(VisualElement ve, UIRenderDevice device)
	{
		Debug.Assert(!ve.renderChainData.disableNudging);
		GetVerticesTransformInfo(ve, out var transform);
		Matrix4x4 nudgeTransform = transform * ve.renderChainData.verticesSpace.inverse;
		Matrix4x4 matrix4x = nudgeTransform * ve.renderChainData.verticesSpace;
		float num = Mathf.Abs(transform.m00 - matrix4x.m00);
		num += Mathf.Abs(transform.m01 - matrix4x.m01);
		num += Mathf.Abs(transform.m02 - matrix4x.m02);
		num += Mathf.Abs(transform.m03 - matrix4x.m03);
		num += Mathf.Abs(transform.m10 - matrix4x.m10);
		num += Mathf.Abs(transform.m11 - matrix4x.m11);
		num += Mathf.Abs(transform.m12 - matrix4x.m12);
		num += Mathf.Abs(transform.m13 - matrix4x.m13);
		num += Mathf.Abs(transform.m20 - matrix4x.m20);
		num += Mathf.Abs(transform.m21 - matrix4x.m21);
		num += Mathf.Abs(transform.m22 - matrix4x.m22);
		num += Mathf.Abs(transform.m23 - matrix4x.m23);
		if (num > 0.0001f)
		{
			return false;
		}
		ve.renderChainData.verticesSpace = transform;
		DoNudgeVertices(ve, device, ve.renderChainData.data, ref nudgeTransform);
		if (ve.renderChainData.closingData != null)
		{
			DoNudgeVertices(ve, device, ve.renderChainData.closingData, ref nudgeTransform);
		}
		return true;
	}

	private static void DoNudgeVertices(VisualElement ve, UIRenderDevice device, MeshHandle mesh, ref Matrix4x4 nudgeTransform)
	{
		int size = (int)mesh.allocVerts.size;
		NativeSlice<Vertex> nativeSlice = mesh.allocPage.vertices.cpuData.Slice((int)mesh.allocVerts.start, size);
		device.Update(mesh, (uint)size, out var vertexData);
		int displacementUVStart = ve.renderChainData.displacementUVStart;
		int displacementUVEnd = ve.renderChainData.displacementUVEnd;
		for (int i = 0; i < displacementUVStart; i++)
		{
			Vertex value = nativeSlice[i];
			value.position = nudgeTransform.MultiplyPoint3x4(value.position);
			vertexData[i] = value;
		}
		for (int j = displacementUVStart; j < displacementUVEnd; j++)
		{
			Vertex value2 = nativeSlice[j];
			value2.position = nudgeTransform.MultiplyPoint3x4(value2.position);
			value2.uv = nudgeTransform.MultiplyVector(value2.uv);
			vertexData[j] = value2;
		}
		for (int k = displacementUVEnd; k < size; k++)
		{
			Vertex value3 = nativeSlice[k];
			value3.position = nudgeTransform.MultiplyPoint3x4(value3.position);
			vertexData[k] = value3;
		}
	}

	private static RenderChainCommand InjectMeshDrawCommand(RenderChain renderChain, VisualElement ve, ref RenderChainCommand cmdPrev, ref RenderChainCommand cmdNext, MeshHandle mesh, int indexCount, int indexOffset, Material material, TextureId texture, Texture font, int stencilRef)
	{
		RenderChainCommand renderChainCommand = renderChain.AllocCommand();
		renderChainCommand.type = CommandType.Draw;
		renderChainCommand.state = new State
		{
			material = material,
			texture = texture,
			font = font,
			stencilRef = stencilRef
		};
		renderChainCommand.mesh = mesh;
		renderChainCommand.indexOffset = indexOffset;
		renderChainCommand.indexCount = indexCount;
		renderChainCommand.owner = ve;
		InjectCommandInBetween(renderChain, renderChainCommand, ref cmdPrev, ref cmdNext);
		return renderChainCommand;
	}

	private static RenderChainCommand InjectClosingMeshDrawCommand(RenderChain renderChain, VisualElement ve, ref RenderChainCommand cmdPrev, ref RenderChainCommand cmdNext, MeshHandle mesh, int indexCount, int indexOffset, Material material, TextureId texture, Texture font, int stencilRef)
	{
		RenderChainCommand renderChainCommand = renderChain.AllocCommand();
		renderChainCommand.type = CommandType.Draw;
		renderChainCommand.closing = true;
		renderChainCommand.state = new State
		{
			material = material,
			texture = texture,
			font = font,
			stencilRef = stencilRef
		};
		renderChainCommand.mesh = mesh;
		renderChainCommand.indexOffset = indexOffset;
		renderChainCommand.indexCount = indexCount;
		renderChainCommand.owner = ve;
		InjectClosingCommandInBetween(renderChain, renderChainCommand, ref cmdPrev, ref cmdNext);
		return renderChainCommand;
	}

	private static void FindCommandInsertionPoint(VisualElement ve, out RenderChainCommand prev, out RenderChainCommand next)
	{
		VisualElement prev2 = ve.renderChainData.prev;
		while (prev2 != null && prev2.renderChainData.lastCommand == null)
		{
			prev2 = prev2.renderChainData.prev;
		}
		if (prev2 != null && prev2.renderChainData.lastCommand != null)
		{
			if (prev2.hierarchy.parent == ve.hierarchy.parent)
			{
				prev = prev2.renderChainData.lastClosingOrLastCommand;
			}
			else if (prev2.IsParentOrAncestorOf(ve))
			{
				prev = prev2.renderChainData.lastCommand;
			}
			else
			{
				RenderChainCommand renderChainCommand = prev2.renderChainData.lastClosingOrLastCommand;
				do
				{
					prev = renderChainCommand;
					renderChainCommand = renderChainCommand.next;
				}
				while (renderChainCommand != null && renderChainCommand.owner != ve && renderChainCommand.closing && !renderChainCommand.owner.IsParentOrAncestorOf(ve));
			}
			next = prev.next;
		}
		else
		{
			VisualElement next2 = ve.renderChainData.next;
			while (next2 != null && next2.renderChainData.firstCommand == null)
			{
				next2 = next2.renderChainData.next;
			}
			next = next2?.renderChainData.firstCommand;
			prev = null;
			Debug.Assert(next == null || next.prev == null);
		}
	}

	private static void FindClosingCommandInsertionPoint(VisualElement ve, out RenderChainCommand prev, out RenderChainCommand next)
	{
		VisualElement visualElement = ve.renderChainData.next;
		while (visualElement != null && visualElement.renderChainData.firstCommand == null)
		{
			visualElement = visualElement.renderChainData.next;
		}
		if (visualElement != null && visualElement.renderChainData.firstCommand != null)
		{
			if (visualElement.hierarchy.parent == ve.hierarchy.parent)
			{
				next = visualElement.renderChainData.firstCommand;
				prev = next.prev;
			}
			else if (ve.IsParentOrAncestorOf(visualElement))
			{
				do
				{
					prev = visualElement.renderChainData.lastClosingOrLastCommand;
					visualElement = prev.next?.owner;
				}
				while (visualElement != null && ve.IsParentOrAncestorOf(visualElement));
				next = prev.next;
			}
			else
			{
				prev = ve.renderChainData.lastCommand;
				next = prev.next;
			}
		}
		else
		{
			prev = ve.renderChainData.lastCommand;
			next = prev.next;
		}
	}

	private static void InjectCommandInBetween(RenderChain renderChain, RenderChainCommand cmd, ref RenderChainCommand prev, ref RenderChainCommand next)
	{
		if (prev != null)
		{
			cmd.prev = prev;
			prev.next = cmd;
		}
		if (next != null)
		{
			cmd.next = next;
			next.prev = cmd;
		}
		VisualElement owner = cmd.owner;
		owner.renderChainData.lastCommand = cmd;
		if (owner.renderChainData.firstCommand == null)
		{
			owner.renderChainData.firstCommand = cmd;
		}
		renderChain.OnRenderCommandAdded(cmd);
		prev = cmd;
		next = cmd.next;
	}

	private static void InjectClosingCommandInBetween(RenderChain renderChain, RenderChainCommand cmd, ref RenderChainCommand prev, ref RenderChainCommand next)
	{
		Debug.Assert(cmd.closing);
		if (prev != null)
		{
			cmd.prev = prev;
			prev.next = cmd;
		}
		if (next != null)
		{
			cmd.next = next;
			next.prev = cmd;
		}
		VisualElement owner = cmd.owner;
		owner.renderChainData.lastClosingCommand = cmd;
		if (owner.renderChainData.firstClosingCommand == null)
		{
			owner.renderChainData.firstClosingCommand = cmd;
		}
		renderChain.OnRenderCommandAdded(cmd);
		prev = cmd;
		next = cmd.next;
	}

	public static void ResetCommands(RenderChain renderChain, VisualElement ve)
	{
		if (ve.renderChainData.firstCommand != null)
		{
			renderChain.OnRenderCommandsRemoved(ve.renderChainData.firstCommand, ve.renderChainData.lastCommand);
		}
		RenderChainCommand renderChainCommand = ((ve.renderChainData.firstCommand != null) ? ve.renderChainData.firstCommand.prev : null);
		RenderChainCommand renderChainCommand2 = ((ve.renderChainData.lastCommand != null) ? ve.renderChainData.lastCommand.next : null);
		Debug.Assert(renderChainCommand == null || renderChainCommand.owner != ve);
		Debug.Assert(renderChainCommand2 == null || renderChainCommand2 == ve.renderChainData.firstClosingCommand || renderChainCommand2.owner != ve);
		if (renderChainCommand != null)
		{
			renderChainCommand.next = renderChainCommand2;
		}
		if (renderChainCommand2 != null)
		{
			renderChainCommand2.prev = renderChainCommand;
		}
		if (ve.renderChainData.firstCommand != null)
		{
			RenderChainCommand renderChainCommand3 = ve.renderChainData.firstCommand;
			while (renderChainCommand3 != ve.renderChainData.lastCommand)
			{
				RenderChainCommand next = renderChainCommand3.next;
				renderChain.FreeCommand(renderChainCommand3);
				renderChainCommand3 = next;
			}
			renderChain.FreeCommand(renderChainCommand3);
		}
		ve.renderChainData.firstCommand = (ve.renderChainData.lastCommand = null);
		renderChainCommand = ((ve.renderChainData.firstClosingCommand != null) ? ve.renderChainData.firstClosingCommand.prev : null);
		renderChainCommand2 = ((ve.renderChainData.lastClosingCommand != null) ? ve.renderChainData.lastClosingCommand.next : null);
		Debug.Assert(renderChainCommand == null || renderChainCommand.owner != ve);
		Debug.Assert(renderChainCommand2 == null || renderChainCommand2.owner != ve);
		if (renderChainCommand != null)
		{
			renderChainCommand.next = renderChainCommand2;
		}
		if (renderChainCommand2 != null)
		{
			renderChainCommand2.prev = renderChainCommand;
		}
		if (ve.renderChainData.firstClosingCommand != null)
		{
			renderChain.OnRenderCommandsRemoved(ve.renderChainData.firstClosingCommand, ve.renderChainData.lastClosingCommand);
			RenderChainCommand renderChainCommand4 = ve.renderChainData.firstClosingCommand;
			while (renderChainCommand4 != ve.renderChainData.lastClosingCommand)
			{
				RenderChainCommand next2 = renderChainCommand4.next;
				renderChain.FreeCommand(renderChainCommand4);
				renderChainCommand4 = next2;
			}
			renderChain.FreeCommand(renderChainCommand4);
		}
		ve.renderChainData.firstClosingCommand = (ve.renderChainData.lastClosingCommand = null);
		if (ve.renderChainData.usesLegacyText)
		{
			Debug.Assert(ve.renderChainData.textEntries.Count > 0);
			renderChain.RemoveTextElement(ve);
			ve.renderChainData.textEntries.Clear();
			ve.renderChainData.usesLegacyText = false;
		}
	}
}
