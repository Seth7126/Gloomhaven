using UnityEngine.Experimental.Rendering;
using UnityEngine.Internal;
using UnityEngine.Rendering;

namespace UnityEngine;

public struct RenderTextureDescriptor
{
	private GraphicsFormat _graphicsFormat;

	private RenderTextureCreationFlags _flags;

	public int width { get; set; }

	public int height { get; set; }

	public int msaaSamples { get; set; }

	public int volumeDepth { get; set; }

	public int mipCount { get; set; }

	public GraphicsFormat graphicsFormat
	{
		get
		{
			return _graphicsFormat;
		}
		set
		{
			_graphicsFormat = value;
			SetOrClearRenderTextureCreationFlag(GraphicsFormatUtility.IsSRGBFormat(value), RenderTextureCreationFlags.SRGB);
			depthBufferBits = depthBufferBits;
		}
	}

	public GraphicsFormat stencilFormat { get; set; }

	public GraphicsFormat depthStencilFormat { get; set; }

	public RenderTextureFormat colorFormat
	{
		get
		{
			return GraphicsFormatUtility.GetRenderTextureFormat(graphicsFormat);
		}
		set
		{
			GraphicsFormat graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(value, sRGB);
			if (graphicsFormat == GraphicsFormat.DepthAuto || graphicsFormat == GraphicsFormat.ShadowAuto)
			{
				this.graphicsFormat = graphicsFormat;
			}
			else
			{
				this.graphicsFormat = SystemInfo.GetCompatibleFormat(graphicsFormat, FormatUsage.Render);
			}
		}
	}

	public bool sRGB
	{
		get
		{
			return GraphicsFormatUtility.IsSRGBFormat(graphicsFormat);
		}
		set
		{
			graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(colorFormat, value);
		}
	}

	public int depthBufferBits
	{
		get
		{
			return GraphicsFormatUtility.GetDepthBits(depthStencilFormat);
		}
		set
		{
			depthStencilFormat = RenderTexture.GetDepthStencilFormatLegacy(value, graphicsFormat);
		}
	}

	public TextureDimension dimension { get; set; }

	public ShadowSamplingMode shadowSamplingMode { get; set; }

	public VRTextureUsage vrUsage { get; set; }

	public RenderTextureCreationFlags flags => _flags;

	public RenderTextureMemoryless memoryless { get; set; }

	public bool useMipMap
	{
		get
		{
			return (_flags & RenderTextureCreationFlags.MipMap) != 0;
		}
		set
		{
			SetOrClearRenderTextureCreationFlag(value, RenderTextureCreationFlags.MipMap);
		}
	}

	public bool autoGenerateMips
	{
		get
		{
			return (_flags & RenderTextureCreationFlags.AutoGenerateMips) != 0;
		}
		set
		{
			SetOrClearRenderTextureCreationFlag(value, RenderTextureCreationFlags.AutoGenerateMips);
		}
	}

	public bool enableRandomWrite
	{
		get
		{
			return (_flags & RenderTextureCreationFlags.EnableRandomWrite) != 0;
		}
		set
		{
			SetOrClearRenderTextureCreationFlag(value, RenderTextureCreationFlags.EnableRandomWrite);
		}
	}

	public bool bindMS
	{
		get
		{
			return (_flags & RenderTextureCreationFlags.BindMS) != 0;
		}
		set
		{
			SetOrClearRenderTextureCreationFlag(value, RenderTextureCreationFlags.BindMS);
		}
	}

	internal bool createdFromScript
	{
		get
		{
			return (_flags & RenderTextureCreationFlags.CreatedFromScript) != 0;
		}
		set
		{
			SetOrClearRenderTextureCreationFlag(value, RenderTextureCreationFlags.CreatedFromScript);
		}
	}

	public bool useDynamicScale
	{
		get
		{
			return (_flags & RenderTextureCreationFlags.DynamicallyScalable) != 0;
		}
		set
		{
			SetOrClearRenderTextureCreationFlag(value, RenderTextureCreationFlags.DynamicallyScalable);
		}
	}

	public RenderTextureDescriptor(int width, int height)
		: this(width, height, RenderTextureFormat.Default)
	{
	}

	public RenderTextureDescriptor(int width, int height, RenderTextureFormat colorFormat)
		: this(width, height, colorFormat, 0)
	{
	}

	public RenderTextureDescriptor(int width, int height, RenderTextureFormat colorFormat, int depthBufferBits)
		: this(width, height, colorFormat, depthBufferBits, Texture.GenerateAllMips)
	{
	}

	[ExcludeFromDocs]
	public RenderTextureDescriptor(int width, int height, GraphicsFormat colorFormat, int depthBufferBits)
		: this(width, height, colorFormat, depthBufferBits, Texture.GenerateAllMips)
	{
	}

	public RenderTextureDescriptor(int width, int height, RenderTextureFormat colorFormat, int depthBufferBits, int mipCount)
	{
		GraphicsFormat graphicsFormat = GraphicsFormatUtility.GetGraphicsFormat(colorFormat, isSRGB: false);
		if (graphicsFormat == GraphicsFormat.DepthAuto || graphicsFormat == GraphicsFormat.ShadowAuto)
		{
			this = new RenderTextureDescriptor(width, height, graphicsFormat, depthBufferBits, mipCount);
		}
		else
		{
			this = new RenderTextureDescriptor(width, height, SystemInfo.GetCompatibleFormat(graphicsFormat, FormatUsage.Render), depthBufferBits, mipCount);
		}
	}

	[ExcludeFromDocs]
	public RenderTextureDescriptor(int width, int height, GraphicsFormat colorFormat, int depthBufferBits, int mipCount)
	{
		this = default(RenderTextureDescriptor);
		_flags = RenderTextureCreationFlags.AutoGenerateMips | RenderTextureCreationFlags.AllowVerticalFlip;
		this.width = width;
		this.height = height;
		volumeDepth = 1;
		msaaSamples = 1;
		graphicsFormat = colorFormat;
		depthStencilFormat = RenderTexture.GetDepthStencilFormatLegacy(depthBufferBits, colorFormat);
		this.mipCount = mipCount;
		dimension = TextureDimension.Tex2D;
		shadowSamplingMode = ShadowSamplingMode.None;
		vrUsage = VRTextureUsage.None;
		memoryless = RenderTextureMemoryless.None;
	}

	[ExcludeFromDocs]
	public RenderTextureDescriptor(int width, int height, GraphicsFormat colorFormat, GraphicsFormat depthStencilFormat)
		: this(width, height, colorFormat, depthStencilFormat, Texture.GenerateAllMips)
	{
	}

	[ExcludeFromDocs]
	public RenderTextureDescriptor(int width, int height, GraphicsFormat colorFormat, GraphicsFormat depthStencilFormat, int mipCount)
	{
		this = default(RenderTextureDescriptor);
		_flags = RenderTextureCreationFlags.AutoGenerateMips | RenderTextureCreationFlags.AllowVerticalFlip;
		this.width = width;
		this.height = height;
		volumeDepth = 1;
		msaaSamples = 1;
		graphicsFormat = colorFormat;
		this.depthStencilFormat = depthStencilFormat;
		this.mipCount = mipCount;
		dimension = TextureDimension.Tex2D;
		shadowSamplingMode = ShadowSamplingMode.None;
		vrUsage = VRTextureUsage.None;
		memoryless = RenderTextureMemoryless.None;
	}

	private void SetOrClearRenderTextureCreationFlag(bool value, RenderTextureCreationFlags flag)
	{
		if (value)
		{
			_flags |= flag;
		}
		else
		{
			_flags &= ~flag;
		}
	}
}
