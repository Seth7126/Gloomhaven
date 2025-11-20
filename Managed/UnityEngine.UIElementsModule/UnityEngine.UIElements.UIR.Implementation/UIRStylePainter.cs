#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements.UIR.Implementation;

internal class UIRStylePainter : IStylePainter, IDisposable
{
	internal struct Entry
	{
		public NativeSlice<Vertex> vertices;

		public NativeSlice<ushort> indices;

		public Material material;

		public Texture custom;

		public Texture font;

		public float fontTexSDFScale;

		public TextureId texture;

		public RenderChainCommand customCommand;

		public BMPAlloc clipRectID;

		public VertexFlags addFlags;

		public bool uvIsDisplacement;

		public bool isTextEntry;

		public bool isClipRegisterEntry;

		public int stencilRef;

		public int maskDepth;
	}

	internal struct ClosingInfo
	{
		public bool needsClosing;

		public bool popViewMatrix;

		public bool popScissorClip;

		public bool blitAndPopRenderTexture;

		public bool PopDefaultMaterial;

		public RenderChainCommand clipUnregisterDrawCommand;

		public NativeSlice<Vertex> clipperRegisterVertices;

		public NativeSlice<ushort> clipperRegisterIndices;

		public int clipperRegisterIndexOffset;

		public int maskStencilRef;
	}

	internal struct TempDataAlloc<T> : IDisposable where T : struct
	{
		private int maxPoolElemCount;

		private NativeArray<T> pool;

		private List<NativeArray<T>> excess;

		private uint takenFromPool;

		public TempDataAlloc(int maxPoolElems)
		{
			maxPoolElemCount = maxPoolElems;
			pool = default(NativeArray<T>);
			excess = new List<NativeArray<T>>();
			takenFromPool = 0u;
		}

		public void Dispose()
		{
			foreach (NativeArray<T> item in excess)
			{
				item.Dispose();
			}
			excess.Clear();
			if (pool.IsCreated)
			{
				pool.Dispose();
			}
		}

		internal NativeSlice<T> Alloc(uint count)
		{
			if (takenFromPool + count <= pool.Length)
			{
				NativeSlice<T> result = pool.Slice((int)takenFromPool, (int)count);
				takenFromPool += count;
				return result;
			}
			NativeArray<T> nativeArray = new NativeArray<T>((int)count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			excess.Add(nativeArray);
			return nativeArray;
		}

		internal void SessionDone()
		{
			int num = pool.Length;
			foreach (NativeArray<T> item in excess)
			{
				if (item.Length < maxPoolElemCount)
				{
					num += item.Length;
				}
				item.Dispose();
			}
			excess.Clear();
			if (num > pool.Length)
			{
				if (pool.IsCreated)
				{
					pool.Dispose();
				}
				pool = new NativeArray<T>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			}
			takenFromPool = 0u;
		}
	}

	private RenderChain m_Owner;

	private List<Entry> m_Entries = new List<Entry>();

	private AtlasBase m_Atlas;

	private VectorImageManager m_VectorImageManager;

	private Entry m_CurrentEntry;

	private ClosingInfo m_ClosingInfo;

	private int m_MaskDepth;

	private int m_StencilRef;

	private BMPAlloc m_ClipRectID = UIRVEShaderInfoAllocator.infiniteClipRect;

	private int m_SVGBackgroundEntryIndex = -1;

	private TempDataAlloc<Vertex> m_VertsPool = new TempDataAlloc<Vertex>(8192);

	private TempDataAlloc<ushort> m_IndicesPool = new TempDataAlloc<ushort>(16384);

	private List<MeshWriteData> m_MeshWriteDataPool;

	private int m_NextMeshWriteDataPoolItem;

	private MeshBuilder.AllocMeshData.Allocator m_AllocRawVertsIndicesDelegate;

	private MeshBuilder.AllocMeshData.Allocator m_AllocThroughDrawMeshDelegate;

	private MeshBuilder.AllocMeshData.Allocator m_AllocThroughDrawGradientsDelegate;

	public MeshGenerationContext meshGenerationContext { get; }

	public VisualElement currentElement { get; private set; }

	public List<Entry> entries => m_Entries;

	public ClosingInfo closingInfo => m_ClosingInfo;

	public int totalVertices { get; private set; }

	public int totalIndices { get; private set; }

	protected bool disposed { get; private set; }

	public VisualElement visualElement => currentElement;

	private MeshWriteData GetPooledMeshWriteData()
	{
		if (m_NextMeshWriteDataPoolItem == m_MeshWriteDataPool.Count)
		{
			m_MeshWriteDataPool.Add(new MeshWriteData());
		}
		return m_MeshWriteDataPool[m_NextMeshWriteDataPoolItem++];
	}

	private MeshWriteData AllocRawVertsIndices(uint vertexCount, uint indexCount, ref MeshBuilder.AllocMeshData allocatorData)
	{
		m_CurrentEntry.vertices = m_VertsPool.Alloc(vertexCount);
		m_CurrentEntry.indices = m_IndicesPool.Alloc(indexCount);
		MeshWriteData pooledMeshWriteData = GetPooledMeshWriteData();
		pooledMeshWriteData.Reset(m_CurrentEntry.vertices, m_CurrentEntry.indices);
		return pooledMeshWriteData;
	}

	private MeshWriteData AllocThroughDrawMesh(uint vertexCount, uint indexCount, ref MeshBuilder.AllocMeshData allocatorData)
	{
		return DrawMesh((int)vertexCount, (int)indexCount, allocatorData.texture, allocatorData.material, allocatorData.flags);
	}

	private MeshWriteData AllocThroughDrawGradients(uint vertexCount, uint indexCount, ref MeshBuilder.AllocMeshData allocatorData)
	{
		return AddGradientsEntry((int)vertexCount, (int)indexCount, allocatorData.svgTexture, allocatorData.material, allocatorData.flags);
	}

	public UIRStylePainter(RenderChain renderChain)
	{
		m_Owner = renderChain;
		meshGenerationContext = new MeshGenerationContext(this);
		m_Atlas = renderChain.atlas;
		m_VectorImageManager = renderChain.vectorImageManager;
		m_AllocRawVertsIndicesDelegate = AllocRawVertsIndices;
		m_AllocThroughDrawMeshDelegate = AllocThroughDrawMesh;
		m_AllocThroughDrawGradientsDelegate = AllocThroughDrawGradients;
		int num = 32;
		m_MeshWriteDataPool = new List<MeshWriteData>(num);
		for (int i = 0; i < num; i++)
		{
			m_MeshWriteDataPool.Add(new MeshWriteData());
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				m_IndicesPool.Dispose();
				m_VertsPool.Dispose();
			}
			disposed = true;
		}
	}

	public void Begin(VisualElement ve)
	{
		currentElement = ve;
		m_NextMeshWriteDataPoolItem = 0;
		m_SVGBackgroundEntryIndex = -1;
		currentElement.renderChainData.usesLegacyText = (currentElement.renderChainData.disableNudging = false);
		currentElement.renderChainData.displacementUVStart = (currentElement.renderChainData.displacementUVEnd = 0);
		m_MaskDepth = 0;
		m_StencilRef = 0;
		VisualElement parent = currentElement.hierarchy.parent;
		if (parent != null)
		{
			m_MaskDepth = parent.renderChainData.childrenMaskDepth;
			m_StencilRef = parent.renderChainData.childrenStencilRef;
		}
		bool flag = (currentElement.renderHints & RenderHints.GroupTransform) != 0;
		if (flag)
		{
			RenderChainCommand renderChainCommand = m_Owner.AllocCommand();
			renderChainCommand.owner = currentElement;
			renderChainCommand.type = CommandType.PushView;
			m_Entries.Add(new Entry
			{
				customCommand = renderChainCommand
			});
			m_ClosingInfo.needsClosing = (m_ClosingInfo.popViewMatrix = true);
		}
		if (parent != null)
		{
			m_ClipRectID = (flag ? UIRVEShaderInfoAllocator.infiniteClipRect : parent.renderChainData.clipRectID);
		}
		else
		{
			m_ClipRectID = UIRVEShaderInfoAllocator.infiniteClipRect;
		}
		if (ve.subRenderTargetMode != VisualElement.RenderTargetMode.None)
		{
			RenderChainCommand renderChainCommand2 = m_Owner.AllocCommand();
			renderChainCommand2.owner = currentElement;
			renderChainCommand2.type = CommandType.PushRenderTexture;
			m_Entries.Add(new Entry
			{
				customCommand = renderChainCommand2
			});
			m_ClosingInfo.needsClosing = (m_ClosingInfo.blitAndPopRenderTexture = true);
			if (m_MaskDepth > 0 || m_StencilRef > 0)
			{
				Debug.LogError("The RenderTargetMode feature must not be used within a stencil mask.");
			}
		}
		if (ve.defaultMaterial != null)
		{
			RenderChainCommand renderChainCommand3 = m_Owner.AllocCommand();
			renderChainCommand3.owner = currentElement;
			renderChainCommand3.type = CommandType.PushDefaultMaterial;
			renderChainCommand3.state.material = ve.defaultMaterial;
			m_Entries.Add(new Entry
			{
				customCommand = renderChainCommand3
			});
			m_ClosingInfo.needsClosing = (m_ClosingInfo.PopDefaultMaterial = true);
		}
	}

	public void LandClipUnregisterMeshDrawCommand(RenderChainCommand cmd)
	{
		Debug.Assert(m_ClosingInfo.needsClosing);
		m_ClosingInfo.clipUnregisterDrawCommand = cmd;
	}

	public void LandClipRegisterMesh(NativeSlice<Vertex> vertices, NativeSlice<ushort> indices, int indexOffset)
	{
		Debug.Assert(m_ClosingInfo.needsClosing);
		m_ClosingInfo.clipperRegisterVertices = vertices;
		m_ClosingInfo.clipperRegisterIndices = indices;
		m_ClosingInfo.clipperRegisterIndexOffset = indexOffset;
	}

	public MeshWriteData AddGradientsEntry(int vertexCount, int indexCount, TextureId texture, Material material, MeshGenerationContext.MeshFlags flags)
	{
		MeshWriteData pooledMeshWriteData = GetPooledMeshWriteData();
		if (vertexCount == 0 || indexCount == 0)
		{
			pooledMeshWriteData.Reset(default(NativeSlice<Vertex>), default(NativeSlice<ushort>));
			return pooledMeshWriteData;
		}
		m_CurrentEntry = new Entry
		{
			vertices = m_VertsPool.Alloc((uint)vertexCount),
			indices = m_IndicesPool.Alloc((uint)indexCount),
			material = material,
			texture = texture,
			clipRectID = m_ClipRectID,
			stencilRef = m_StencilRef,
			maskDepth = m_MaskDepth,
			addFlags = VertexFlags.IsSvgGradients
		};
		Debug.Assert(m_CurrentEntry.vertices.Length == vertexCount);
		Debug.Assert(m_CurrentEntry.indices.Length == indexCount);
		pooledMeshWriteData.Reset(m_CurrentEntry.vertices, m_CurrentEntry.indices, new Rect(0f, 0f, 1f, 1f));
		m_Entries.Add(m_CurrentEntry);
		totalVertices += m_CurrentEntry.vertices.Length;
		totalIndices += m_CurrentEntry.indices.Length;
		m_CurrentEntry = default(Entry);
		return pooledMeshWriteData;
	}

	public MeshWriteData DrawMesh(int vertexCount, int indexCount, Texture texture, Material material, MeshGenerationContext.MeshFlags flags)
	{
		MeshWriteData pooledMeshWriteData = GetPooledMeshWriteData();
		if (vertexCount == 0 || indexCount == 0)
		{
			pooledMeshWriteData.Reset(default(NativeSlice<Vertex>), default(NativeSlice<ushort>));
			return pooledMeshWriteData;
		}
		m_CurrentEntry = new Entry
		{
			vertices = m_VertsPool.Alloc((uint)vertexCount),
			indices = m_IndicesPool.Alloc((uint)indexCount),
			material = material,
			uvIsDisplacement = ((flags & MeshGenerationContext.MeshFlags.UVisDisplacement) == MeshGenerationContext.MeshFlags.UVisDisplacement),
			clipRectID = m_ClipRectID,
			stencilRef = m_StencilRef,
			maskDepth = m_MaskDepth,
			addFlags = VertexFlags.IsSolid
		};
		Debug.Assert(m_CurrentEntry.vertices.Length == vertexCount);
		Debug.Assert(m_CurrentEntry.indices.Length == indexCount);
		Rect uvRegion = new Rect(0f, 0f, 1f, 1f);
		if (texture != null)
		{
			if ((flags & MeshGenerationContext.MeshFlags.SkipDynamicAtlas) != MeshGenerationContext.MeshFlags.SkipDynamicAtlas && m_Atlas != null && m_Atlas.TryGetAtlas(currentElement, texture as Texture2D, out var atlas, out var atlasRect))
			{
				m_CurrentEntry.addFlags = VertexFlags.IsDynamic;
				uvRegion = new Rect(atlasRect.x, atlasRect.y, atlasRect.width, atlasRect.height);
				m_CurrentEntry.texture = atlas;
				m_Owner.AppendTexture(currentElement, texture, atlas, isAtlas: true);
			}
			else
			{
				TextureId textureId = TextureRegistry.instance.Acquire(texture);
				m_CurrentEntry.addFlags = VertexFlags.IsTextured;
				m_CurrentEntry.texture = textureId;
				m_Owner.AppendTexture(currentElement, texture, textureId, isAtlas: false);
			}
		}
		pooledMeshWriteData.Reset(m_CurrentEntry.vertices, m_CurrentEntry.indices, uvRegion);
		m_Entries.Add(m_CurrentEntry);
		totalVertices += m_CurrentEntry.vertices.Length;
		totalIndices += m_CurrentEntry.indices.Length;
		m_CurrentEntry = default(Entry);
		return pooledMeshWriteData;
	}

	public void DrawText(MeshGenerationContextUtils.TextParams textParams, ITextHandle handle, float pixelsPerPoint)
	{
		if (TextUtilities.IsFontAssigned(textParams))
		{
			if (handle.IsLegacy())
			{
				DrawTextNative(textParams, handle, pixelsPerPoint);
			}
			else
			{
				DrawTextCore(textParams, handle, pixelsPerPoint);
			}
		}
	}

	internal void DrawTextNative(MeshGenerationContextUtils.TextParams textParams, ITextHandle handle, float pixelsPerPoint)
	{
		float scaling = TextUtilities.ComputeTextScaling(currentElement.worldTransform, pixelsPerPoint);
		using NativeArray<TextVertex> uiVertices = ((TextNativeHandle)(object)handle).GetVertices(textParams, scaling);
		if (uiVertices.Length != 0)
		{
			TextNativeSettings textNativeSettings = MeshGenerationContextUtils.TextParams.GetTextNativeSettings(textParams, scaling);
			Vector2 offset = TextNative.GetOffset(textNativeSettings, textParams.rect);
			m_CurrentEntry.isTextEntry = true;
			m_CurrentEntry.clipRectID = m_ClipRectID;
			m_CurrentEntry.stencilRef = m_StencilRef;
			m_CurrentEntry.maskDepth = m_MaskDepth;
			MeshBuilder.MakeText(uiVertices, offset, new MeshBuilder.AllocMeshData
			{
				alloc = m_AllocRawVertsIndicesDelegate
			});
			m_CurrentEntry.font = textParams.font.material.mainTexture;
			m_Entries.Add(m_CurrentEntry);
			totalVertices += m_CurrentEntry.vertices.Length;
			totalIndices += m_CurrentEntry.indices.Length;
			m_CurrentEntry = default(Entry);
			currentElement.renderChainData.usesLegacyText = true;
			currentElement.renderChainData.disableNudging = true;
		}
	}

	internal void DrawTextCore(MeshGenerationContextUtils.TextParams textParams, ITextHandle handle, float pixelsPerPoint)
	{
		TextInfo textInfo = handle.Update(textParams, pixelsPerPoint);
		for (int i = 0; i < textInfo.materialCount; i++)
		{
			if (textInfo.meshInfo[i].vertexCount != 0)
			{
				m_CurrentEntry.clipRectID = m_ClipRectID;
				m_CurrentEntry.stencilRef = m_StencilRef;
				m_CurrentEntry.maskDepth = m_MaskDepth;
				if (textInfo.meshInfo[i].material.name.Contains("Sprite"))
				{
					Texture mainTexture = textInfo.meshInfo[i].material.mainTexture;
					TextureId textureId = TextureRegistry.instance.Acquire(mainTexture);
					m_CurrentEntry.texture = textureId;
					m_Owner.AppendTexture(currentElement, mainTexture, textureId, isAtlas: false);
					MeshBuilder.MakeText(textInfo.meshInfo[i], textParams.rect.min, new MeshBuilder.AllocMeshData
					{
						alloc = m_AllocRawVertsIndicesDelegate
					}, VertexFlags.IsTextured);
				}
				else
				{
					m_CurrentEntry.isTextEntry = true;
					m_CurrentEntry.fontTexSDFScale = textInfo.meshInfo[i].material.GetFloat(TextShaderUtilities.ID_GradientScale);
					m_CurrentEntry.font = textInfo.meshInfo[i].material.mainTexture;
					bool isDynamicColor = RenderEvents.NeedsColorID(currentElement);
					MeshBuilder.MakeText(textInfo.meshInfo[i], textParams.rect.min, new MeshBuilder.AllocMeshData
					{
						alloc = m_AllocRawVertsIndicesDelegate
					}, VertexFlags.IsText, isDynamicColor);
				}
				m_Entries.Add(m_CurrentEntry);
				totalVertices += m_CurrentEntry.vertices.Length;
				totalIndices += m_CurrentEntry.indices.Length;
				m_CurrentEntry = default(Entry);
			}
		}
	}

	public void DrawRectangle(MeshGenerationContextUtils.RectangleParams rectParams)
	{
		if (!(rectParams.rect.width < 1E-30f) && !(rectParams.rect.height < 1E-30f))
		{
			MeshBuilder.AllocMeshData meshAlloc = new MeshBuilder.AllocMeshData
			{
				alloc = m_AllocThroughDrawMeshDelegate,
				texture = rectParams.texture,
				material = rectParams.material,
				flags = rectParams.meshFlags
			};
			if (rectParams.vectorImage != null)
			{
				DrawVectorImage(rectParams);
			}
			else if (rectParams.sprite != null)
			{
				DrawSprite(rectParams);
			}
			else if (rectParams.texture != null)
			{
				MeshBuilder.MakeTexturedRect(rectParams, 0f, meshAlloc, rectParams.colorPage);
			}
			else
			{
				MeshBuilder.MakeSolidRect(rectParams, 0f, meshAlloc);
			}
		}
	}

	public void DrawBorder(MeshGenerationContextUtils.BorderParams borderParams)
	{
		MeshBuilder.MakeBorder(borderParams, 0f, new MeshBuilder.AllocMeshData
		{
			alloc = m_AllocThroughDrawMeshDelegate,
			material = borderParams.material,
			texture = null
		});
	}

	public void DrawImmediate(Action callback, bool cullingEnabled)
	{
		RenderChainCommand renderChainCommand = m_Owner.AllocCommand();
		renderChainCommand.type = (cullingEnabled ? CommandType.ImmediateCull : CommandType.Immediate);
		renderChainCommand.owner = currentElement;
		renderChainCommand.callback = callback;
		m_Entries.Add(new Entry
		{
			customCommand = renderChainCommand
		});
	}

	public void DrawVisualElementBackground()
	{
		if (currentElement.layout.width <= 1E-30f || currentElement.layout.height <= 1E-30f)
		{
			return;
		}
		ComputedStyle computedStyle = currentElement.computedStyle;
		if (computedStyle.backgroundColor != Color.clear)
		{
			MeshGenerationContextUtils.RectangleParams rectParams = new MeshGenerationContextUtils.RectangleParams
			{
				rect = GUIUtility.AlignRectToDevice(currentElement.rect),
				color = computedStyle.backgroundColor,
				colorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.backgroundColorID),
				playmodeTintColor = ((currentElement.panel.contextType == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white)
			};
			MeshGenerationContextUtils.GetVisualElementRadii(currentElement, out rectParams.topLeftRadius, out rectParams.bottomLeftRadius, out rectParams.topRightRadius, out rectParams.bottomRightRadius);
			MeshGenerationContextUtils.AdjustBackgroundSizeForBorders(currentElement, ref rectParams.rect);
			DrawRectangle(rectParams);
		}
		Vector4 slices = new Vector4(computedStyle.unitySliceLeft, computedStyle.unitySliceTop, computedStyle.unitySliceRight, computedStyle.unitySliceBottom);
		MeshGenerationContextUtils.RectangleParams rectangleParams = default(MeshGenerationContextUtils.RectangleParams);
		MeshGenerationContextUtils.GetVisualElementRadii(currentElement, out rectangleParams.topLeftRadius, out rectangleParams.bottomLeftRadius, out rectangleParams.topRightRadius, out rectangleParams.bottomRightRadius);
		Background backgroundImage = computedStyle.backgroundImage;
		if (backgroundImage.texture != null || backgroundImage.sprite != null || backgroundImage.vectorImage != null || backgroundImage.renderTexture != null)
		{
			MeshGenerationContextUtils.RectangleParams rectParams2 = default(MeshGenerationContextUtils.RectangleParams);
			float num = 1f;
			if (backgroundImage.texture != null)
			{
				rectParams2 = MeshGenerationContextUtils.RectangleParams.MakeTextured(GUIUtility.AlignRectToDevice(currentElement.rect), new Rect(0f, 0f, 1f, 1f), backgroundImage.texture, computedStyle.unityBackgroundScaleMode, currentElement.panel.contextType);
			}
			else if (backgroundImage.sprite != null)
			{
				rectParams2 = MeshGenerationContextUtils.RectangleParams.MakeSprite(GUIUtility.AlignRectToDevice(currentElement.rect), backgroundImage.sprite, computedStyle.unityBackgroundScaleMode, currentElement.panel.contextType, rectangleParams.HasRadius(Tessellation.kEpsilon), ref slices);
				num *= UIElementsUtility.PixelsPerUnitScaleForElement(visualElement, backgroundImage.sprite);
			}
			else if (backgroundImage.renderTexture != null)
			{
				rectParams2 = MeshGenerationContextUtils.RectangleParams.MakeTextured(GUIUtility.AlignRectToDevice(currentElement.rect), new Rect(0f, 0f, 1f, 1f), backgroundImage.renderTexture, computedStyle.unityBackgroundScaleMode, currentElement.panel.contextType);
			}
			else if (backgroundImage.vectorImage != null)
			{
				rectParams2 = MeshGenerationContextUtils.RectangleParams.MakeVectorTextured(GUIUtility.AlignRectToDevice(currentElement.rect), new Rect(0f, 0f, 1f, 1f), backgroundImage.vectorImage, computedStyle.unityBackgroundScaleMode, currentElement.panel.contextType);
			}
			rectParams2.topLeftRadius = rectangleParams.topLeftRadius;
			rectParams2.topRightRadius = rectangleParams.topRightRadius;
			rectParams2.bottomRightRadius = rectangleParams.bottomRightRadius;
			rectParams2.bottomLeftRadius = rectangleParams.bottomLeftRadius;
			if (slices != Vector4.zero)
			{
				rectParams2.leftSlice = Mathf.RoundToInt(slices.x);
				rectParams2.topSlice = Mathf.RoundToInt(slices.y);
				rectParams2.rightSlice = Mathf.RoundToInt(slices.z);
				rectParams2.bottomSlice = Mathf.RoundToInt(slices.w);
			}
			rectParams2.color = computedStyle.unityBackgroundImageTintColor;
			rectParams2.colorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.tintColorID);
			rectParams2.sliceScale = num;
			MeshGenerationContextUtils.AdjustBackgroundSizeForBorders(currentElement, ref rectParams2.rect);
			DrawRectangle(rectParams2);
		}
	}

	public void DrawVisualElementBorder()
	{
		if (currentElement.layout.width >= 1E-30f && currentElement.layout.height >= 1E-30f)
		{
			IResolvedStyle resolvedStyle = currentElement.resolvedStyle;
			if ((resolvedStyle.borderLeftColor != Color.clear && resolvedStyle.borderLeftWidth > 0f) || (resolvedStyle.borderTopColor != Color.clear && resolvedStyle.borderTopWidth > 0f) || (resolvedStyle.borderRightColor != Color.clear && resolvedStyle.borderRightWidth > 0f) || (resolvedStyle.borderBottomColor != Color.clear && resolvedStyle.borderBottomWidth > 0f))
			{
				MeshGenerationContextUtils.BorderParams borderParams = new MeshGenerationContextUtils.BorderParams
				{
					rect = GUIUtility.AlignRectToDevice(currentElement.rect),
					leftColor = resolvedStyle.borderLeftColor,
					topColor = resolvedStyle.borderTopColor,
					rightColor = resolvedStyle.borderRightColor,
					bottomColor = resolvedStyle.borderBottomColor,
					leftWidth = resolvedStyle.borderLeftWidth,
					topWidth = resolvedStyle.borderTopWidth,
					rightWidth = resolvedStyle.borderRightWidth,
					bottomWidth = resolvedStyle.borderBottomWidth,
					leftColorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.borderLeftColorID),
					topColorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.borderTopColorID),
					rightColorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.borderRightColorID),
					bottomColorPage = ColorPage.Init(m_Owner, currentElement.renderChainData.borderBottomColorID),
					playmodeTintColor = ((currentElement.panel.contextType == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white)
				};
				MeshGenerationContextUtils.GetVisualElementRadii(currentElement, out borderParams.topLeftRadius, out borderParams.bottomLeftRadius, out borderParams.topRightRadius, out borderParams.bottomRightRadius);
				DrawBorder(borderParams);
			}
		}
	}

	public void ApplyVisualElementClipping()
	{
		if (currentElement.renderChainData.clipMethod == ClipMethod.Scissor)
		{
			RenderChainCommand renderChainCommand = m_Owner.AllocCommand();
			renderChainCommand.type = CommandType.PushScissor;
			renderChainCommand.owner = currentElement;
			m_Entries.Add(new Entry
			{
				customCommand = renderChainCommand
			});
			m_ClosingInfo.needsClosing = (m_ClosingInfo.popScissorClip = true);
		}
		else if (currentElement.renderChainData.clipMethod == ClipMethod.Stencil)
		{
			if (m_MaskDepth > m_StencilRef)
			{
				m_StencilRef++;
				Debug.Assert(m_MaskDepth == m_StencilRef);
			}
			m_ClosingInfo.maskStencilRef = m_StencilRef;
			if (UIRUtility.IsVectorImageBackground(currentElement))
			{
				GenerateStencilClipEntryForSVGBackground();
			}
			else
			{
				GenerateStencilClipEntryForRoundedRectBackground();
			}
			m_MaskDepth++;
		}
		m_ClipRectID = currentElement.renderChainData.clipRectID;
	}

	private ushort[] AdjustSpriteWinding(Vector2[] vertices, ushort[] indices)
	{
		ushort[] array = new ushort[indices.Length];
		for (int i = 0; i < indices.Length; i += 3)
		{
			Vector3 vector = vertices[indices[i]];
			Vector3 vector2 = vertices[indices[i + 1]];
			Vector3 vector3 = vertices[indices[i + 2]];
			Vector3 normalized = (vector2 - vector).normalized;
			Vector3 normalized2 = (vector3 - vector).normalized;
			if (Vector3.Cross(normalized, normalized2).z >= 0f)
			{
				array[i] = indices[i + 1];
				array[i + 1] = indices[i];
				array[i + 2] = indices[i + 2];
			}
			else
			{
				array[i] = indices[i];
				array[i + 1] = indices[i + 1];
				array[i + 2] = indices[i + 2];
			}
		}
		return array;
	}

	public void DrawSprite(MeshGenerationContextUtils.RectangleParams rectParams)
	{
		Sprite sprite = rectParams.sprite;
		if (!(sprite.texture == null) && sprite.triangles.Length != 0)
		{
			MeshBuilder.AllocMeshData allocMeshData = new MeshBuilder.AllocMeshData
			{
				alloc = m_AllocThroughDrawMeshDelegate,
				texture = sprite.texture,
				flags = rectParams.meshFlags
			};
			Vector2[] vertices = sprite.vertices;
			ushort[] triangles = sprite.triangles;
			Vector2[] uv = sprite.uv;
			int num = sprite.vertices.Length;
			Vertex[] array = new Vertex[num];
			ushort[] array2 = AdjustSpriteWinding(vertices, triangles);
			MeshWriteData meshWriteData = allocMeshData.Allocate((uint)array.Length, (uint)array2.Length);
			Rect uvRegion = meshWriteData.uvRegion;
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = vertices[i];
				vector -= rectParams.spriteGeomRect.position;
				vector /= rectParams.spriteGeomRect.size;
				vector.y = 1f - vector.y;
				vector *= rectParams.rect.size;
				vector += rectParams.rect.position;
				Vector2 uv2 = uv[i];
				uv2 *= uvRegion.size;
				uv2 += uvRegion.position;
				array[i] = new Vertex
				{
					position = new Vector3(vector.x, vector.y, Vertex.nearZ),
					tint = rectParams.color,
					uv = uv2
				};
			}
			meshWriteData.SetAllVertices(array);
			meshWriteData.SetAllIndices(array2);
		}
	}

	public void DrawVectorImage(MeshGenerationContextUtils.RectangleParams rectParams)
	{
		VectorImage vectorImage = rectParams.vectorImage;
		Debug.Assert(vectorImage != null);
		int settingIndexOffset = 0;
		MeshBuilder.AllocMeshData meshAlloc = default(MeshBuilder.AllocMeshData);
		if (vectorImage.atlas != null && m_VectorImageManager != null)
		{
			GradientRemap gradientRemap = m_VectorImageManager.AddUser(vectorImage, currentElement);
			settingIndexOffset = gradientRemap.destIndex;
			if (gradientRemap.atlas != TextureId.invalid)
			{
				meshAlloc.svgTexture = gradientRemap.atlas;
			}
			else
			{
				meshAlloc.svgTexture = TextureRegistry.instance.Acquire(vectorImage.atlas);
				m_Owner.AppendTexture(currentElement, vectorImage.atlas, meshAlloc.svgTexture, isAtlas: false);
			}
			meshAlloc.alloc = m_AllocThroughDrawGradientsDelegate;
		}
		else
		{
			meshAlloc.alloc = m_AllocThroughDrawMeshDelegate;
		}
		int count = m_Entries.Count;
		MeshBuilder.MakeVectorGraphics(rectParams, settingIndexOffset, meshAlloc, out var finalVertexCount, out var finalIndexCount);
		Debug.Assert(count <= m_Entries.Count + 1);
		if (count != m_Entries.Count)
		{
			m_SVGBackgroundEntryIndex = m_Entries.Count - 1;
			if (finalVertexCount != 0 && finalIndexCount != 0)
			{
				Entry value = m_Entries[m_SVGBackgroundEntryIndex];
				value.vertices = value.vertices.Slice(0, finalVertexCount);
				value.indices = value.indices.Slice(0, finalIndexCount);
				m_Entries[m_SVGBackgroundEntryIndex] = value;
			}
		}
	}

	internal void Reset()
	{
		if (disposed)
		{
			DisposeHelper.NotifyDisposedUsed(this);
			return;
		}
		ValidateMeshWriteData();
		m_Entries.Clear();
		m_VertsPool.SessionDone();
		m_IndicesPool.SessionDone();
		m_ClosingInfo = default(ClosingInfo);
		m_NextMeshWriteDataPoolItem = 0;
		currentElement = null;
		int num = (totalIndices = 0);
		totalVertices = num;
	}

	private void ValidateMeshWriteData()
	{
		for (int i = 0; i < m_NextMeshWriteDataPoolItem; i++)
		{
			MeshWriteData meshWriteData = m_MeshWriteDataPool[i];
			if (meshWriteData.vertexCount > 0 && meshWriteData.currentVertex < meshWriteData.vertexCount)
			{
				Debug.LogError("Not enough vertices written in generateVisualContent callback (asked for " + meshWriteData.vertexCount + " but only wrote " + meshWriteData.currentVertex + ")");
				Vertex nextVertex = meshWriteData.m_Vertices[0];
				while (meshWriteData.currentVertex < meshWriteData.vertexCount)
				{
					meshWriteData.SetNextVertex(nextVertex);
				}
			}
			if (meshWriteData.indexCount > 0 && meshWriteData.currentIndex < meshWriteData.indexCount)
			{
				Debug.LogError("Not enough indices written in generateVisualContent callback (asked for " + meshWriteData.indexCount + " but only wrote " + meshWriteData.currentIndex + ")");
				while (meshWriteData.currentIndex < meshWriteData.indexCount)
				{
					meshWriteData.SetNextIndex(0);
				}
			}
		}
	}

	private void GenerateStencilClipEntryForRoundedRectBackground()
	{
		if (!(currentElement.layout.width <= 1E-30f) && !(currentElement.layout.height <= 1E-30f))
		{
			IResolvedStyle resolvedStyle = currentElement.resolvedStyle;
			MeshGenerationContextUtils.GetVisualElementRadii(currentElement, out var topLeft, out var bottomLeft, out var topRight, out var bottomRight);
			float borderTopWidth = resolvedStyle.borderTopWidth;
			float borderLeftWidth = resolvedStyle.borderLeftWidth;
			float borderBottomWidth = resolvedStyle.borderBottomWidth;
			float borderRightWidth = resolvedStyle.borderRightWidth;
			MeshGenerationContextUtils.RectangleParams rectParams = new MeshGenerationContextUtils.RectangleParams
			{
				rect = GUIUtility.AlignRectToDevice(currentElement.rect),
				color = Color.white,
				topLeftRadius = Vector2.Max(Vector2.zero, topLeft - new Vector2(borderLeftWidth, borderTopWidth)),
				topRightRadius = Vector2.Max(Vector2.zero, topRight - new Vector2(borderRightWidth, borderTopWidth)),
				bottomLeftRadius = Vector2.Max(Vector2.zero, bottomLeft - new Vector2(borderLeftWidth, borderBottomWidth)),
				bottomRightRadius = Vector2.Max(Vector2.zero, bottomRight - new Vector2(borderRightWidth, borderBottomWidth)),
				playmodeTintColor = ((currentElement.panel.contextType == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white)
			};
			rectParams.rect.x += borderLeftWidth;
			rectParams.rect.y += borderTopWidth;
			rectParams.rect.width -= borderLeftWidth + borderRightWidth;
			rectParams.rect.height -= borderTopWidth + borderBottomWidth;
			if (currentElement.computedStyle.unityOverflowClipBox == OverflowClipBox.ContentBox)
			{
				rectParams.rect.x += resolvedStyle.paddingLeft;
				rectParams.rect.y += resolvedStyle.paddingTop;
				rectParams.rect.width -= resolvedStyle.paddingLeft + resolvedStyle.paddingRight;
				rectParams.rect.height -= resolvedStyle.paddingTop + resolvedStyle.paddingBottom;
			}
			m_CurrentEntry.clipRectID = m_ClipRectID;
			m_CurrentEntry.stencilRef = m_StencilRef;
			m_CurrentEntry.maskDepth = m_MaskDepth;
			m_CurrentEntry.isClipRegisterEntry = true;
			MeshBuilder.MakeSolidRect(rectParams, 1f, new MeshBuilder.AllocMeshData
			{
				alloc = m_AllocRawVertsIndicesDelegate
			});
			if (m_CurrentEntry.vertices.Length > 0 && m_CurrentEntry.indices.Length > 0)
			{
				m_Entries.Add(m_CurrentEntry);
				totalVertices += m_CurrentEntry.vertices.Length;
				totalIndices += m_CurrentEntry.indices.Length;
				m_ClosingInfo.needsClosing = true;
			}
			m_CurrentEntry = default(Entry);
		}
	}

	private void GenerateStencilClipEntryForSVGBackground()
	{
		if (m_SVGBackgroundEntryIndex != -1)
		{
			Entry entry = m_Entries[m_SVGBackgroundEntryIndex];
			Debug.Assert(entry.vertices.Length > 0);
			Debug.Assert(entry.indices.Length > 0);
			m_CurrentEntry.vertices = entry.vertices;
			m_CurrentEntry.indices = entry.indices;
			m_CurrentEntry.uvIsDisplacement = entry.uvIsDisplacement;
			m_CurrentEntry.clipRectID = m_ClipRectID;
			m_CurrentEntry.stencilRef = m_StencilRef;
			m_CurrentEntry.maskDepth = m_MaskDepth;
			m_CurrentEntry.isClipRegisterEntry = true;
			m_ClosingInfo.needsClosing = true;
			int length = m_CurrentEntry.vertices.Length;
			NativeSlice<Vertex> vertices = m_VertsPool.Alloc((uint)length);
			for (int i = 0; i < length; i++)
			{
				Vertex value = m_CurrentEntry.vertices[i];
				value.position.z = 1f;
				vertices[i] = value;
			}
			m_CurrentEntry.vertices = vertices;
			totalVertices += m_CurrentEntry.vertices.Length;
			totalIndices += m_CurrentEntry.indices.Length;
			m_Entries.Add(m_CurrentEntry);
			m_CurrentEntry = default(Entry);
		}
	}
}
