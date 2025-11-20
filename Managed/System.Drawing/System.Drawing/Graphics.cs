using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Unity;

namespace System.Drawing;

/// <summary>Encapsulates a GDI+ drawing surface. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class Graphics : MarshalByRefObject, IDisposable, IDeviceContext
{
	/// <summary>Provides a callback method for the <see cref="Overload:System.Drawing.Graphics.EnumerateMetafile" /> method.</summary>
	/// <returns>Return true if you want to continue enumerating records; otherwise, false.</returns>
	/// <param name="recordType">Member of the <see cref="T:System.Drawing.Imaging.EmfPlusRecordType" /> enumeration that specifies the type of metafile record. </param>
	/// <param name="flags">Set of flags that specify attributes of the record. </param>
	/// <param name="dataSize">Number of bytes in the record data. </param>
	/// <param name="data">Pointer to a buffer that contains the record data. </param>
	/// <param name="callbackData">Not used. </param>
	public delegate bool EnumerateMetafileProc(EmfPlusRecordType recordType, int flags, int dataSize, IntPtr data, PlayRecordCallback callbackData);

	/// <summary>Provides a callback method for deciding when the <see cref="Overload:System.Drawing.Graphics.DrawImage" /> method should prematurely cancel execution and stop drawing an image.</summary>
	/// <returns>This method returns true if it decides that the <see cref="Overload:System.Drawing.Graphics.DrawImage" /> method should prematurely stop execution. Otherwise it returns false to indicate that the <see cref="Overload:System.Drawing.Graphics.DrawImage" /> method should continue execution.</returns>
	/// <param name="callbackdata">Internal pointer that specifies data for the callback method. This parameter is not passed by all <see cref="Overload:System.Drawing.Graphics.DrawImage" /> overloads. You can test for its absence by checking for the value <see cref="F:System.IntPtr.Zero" />. </param>
	public delegate bool DrawImageAbort(IntPtr callbackdata);

	internal IntPtr nativeObject;

	internal IMacContext maccontext;

	private bool disposed;

	private static float defDpiX;

	private static float defDpiY;

	private IntPtr deviceContextHdc;

	private Metafile.MetafileHolder _metafileHolder;

	private const string MetafileEnumeration = "Metafiles enumeration, for both WMF and EMF formats, isn't supported.";

	internal static float systemDpiX
	{
		get
		{
			if (defDpiX == 0f)
			{
				Graphics graphics = FromImage(new Bitmap(1, 1));
				defDpiX = graphics.DpiX;
				defDpiY = graphics.DpiY;
			}
			return defDpiX;
		}
	}

	internal static float systemDpiY
	{
		get
		{
			if (defDpiY == 0f)
			{
				Graphics graphics = FromImage(new Bitmap(1, 1));
				defDpiX = graphics.DpiX;
				defDpiY = graphics.DpiY;
			}
			return defDpiY;
		}
	}

	internal IntPtr NativeGraphics => nativeObject;

	internal IntPtr NativeObject
	{
		get
		{
			return nativeObject;
		}
		set
		{
			nativeObject = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Drawing.Region" /> that limits the drawing region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Region" /> that limits the portion of this <see cref="T:System.Drawing.Graphics" /> that is currently available for drawing.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Region Clip
	{
		get
		{
			Region region = new Region();
			GDIPlus.CheckStatus(GDIPlus.GdipGetClip(nativeObject, region.NativeObject));
			return region;
		}
		set
		{
			SetClip(value, CombineMode.Replace);
		}
	}

	/// <summary>Gets a <see cref="T:System.Drawing.RectangleF" /> structure that bounds the clipping region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.RectangleF" /> structure that represents a bounding rectangle for the clipping region of this <see cref="T:System.Drawing.Graphics" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RectangleF ClipBounds
	{
		get
		{
			RectangleF rect = default(RectangleF);
			GDIPlus.CheckStatus(GDIPlus.GdipGetClipBounds(nativeObject, out rect));
			return rect;
		}
	}

	/// <summary>Gets a value that specifies how composited images are drawn to this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>This property specifies a member of the <see cref="T:System.Drawing.Drawing2D.CompositingMode" /> enumeration. The default is <see cref="F:System.Drawing.Drawing2D.CompositingMode.SourceOver" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public CompositingMode CompositingMode
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetCompositingMode(nativeObject, out var compositingMode));
			return compositingMode;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetCompositingMode(nativeObject, value));
		}
	}

	/// <summary>Gets or sets the rendering quality of composited images drawn to this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>This property specifies a member of the <see cref="T:System.Drawing.Drawing2D.CompositingQuality" /> enumeration. The default is <see cref="F:System.Drawing.Drawing2D.CompositingQuality.Default" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public CompositingQuality CompositingQuality
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetCompositingQuality(nativeObject, out var compositingQuality));
			return compositingQuality;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetCompositingQuality(nativeObject, value));
		}
	}

	/// <summary>Gets the horizontal resolution of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>The value, in dots per inch, for the horizontal resolution supported by this <see cref="T:System.Drawing.Graphics" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float DpiX
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetDpiX(nativeObject, out var dpi));
			return dpi;
		}
	}

	/// <summary>Gets the vertical resolution of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>The value, in dots per inch, for the vertical resolution supported by this <see cref="T:System.Drawing.Graphics" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float DpiY
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetDpiY(nativeObject, out var dpi));
			return dpi;
		}
	}

	/// <summary>Gets or sets the interpolation mode associated with this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Drawing2D.InterpolationMode" /> values.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public InterpolationMode InterpolationMode
	{
		get
		{
			InterpolationMode interpolationMode = InterpolationMode.Invalid;
			GDIPlus.CheckStatus(GDIPlus.GdipGetInterpolationMode(nativeObject, out interpolationMode));
			return interpolationMode;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetInterpolationMode(nativeObject, value));
		}
	}

	/// <summary>Gets a value indicating whether the clipping region of this <see cref="T:System.Drawing.Graphics" /> is empty.</summary>
	/// <returns>true if the clipping region of this <see cref="T:System.Drawing.Graphics" /> is empty; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsClipEmpty
	{
		get
		{
			bool result = false;
			GDIPlus.CheckStatus(GDIPlus.GdipIsClipEmpty(nativeObject, out result));
			return result;
		}
	}

	/// <summary>Gets a value indicating whether the visible clipping region of this <see cref="T:System.Drawing.Graphics" /> is empty.</summary>
	/// <returns>true if the visible portion of the clipping region of this <see cref="T:System.Drawing.Graphics" /> is empty; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisibleClipEmpty
	{
		get
		{
			bool result = false;
			GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleClipEmpty(nativeObject, out result));
			return result;
		}
	}

	/// <summary>Gets or sets the scaling between world units and page units for this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>This property specifies a value for the scaling between world units and page units for this <see cref="T:System.Drawing.Graphics" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float PageScale
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPageScale(nativeObject, out var scale));
			return scale;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetPageScale(nativeObject, value));
		}
	}

	/// <summary>Gets or sets the unit of measure used for page coordinates in this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.GraphicsUnit" /> values other than <see cref="F:System.Drawing.GraphicsUnit.World" />.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <see cref="P:System.Drawing.Graphics.PageUnit" /> is set to <see cref="F:System.Drawing.GraphicsUnit.World" />, which is not a physical unit. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public GraphicsUnit PageUnit
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPageUnit(nativeObject, out var unit));
			return unit;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetPageUnit(nativeObject, value));
		}
	}

	/// <summary>Gets or set a value specifying how pixels are offset during rendering of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>This property specifies a member of the <see cref="T:System.Drawing.Drawing2D.PixelOffsetMode" /> enumeration </returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO("This property does not do anything when used with libgdiplus.")]
	public PixelOffsetMode PixelOffsetMode
	{
		get
		{
			PixelOffsetMode pixelOffsetMode = PixelOffsetMode.Invalid;
			GDIPlus.CheckStatus(GDIPlus.GdipGetPixelOffsetMode(nativeObject, out pixelOffsetMode));
			return pixelOffsetMode;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetPixelOffsetMode(nativeObject, value));
		}
	}

	/// <summary>Gets or sets the rendering origin of this <see cref="T:System.Drawing.Graphics" /> for dithering and for hatch brushes.</summary>
	/// <returns>A <see cref="T:System.Drawing.Point" /> structure that represents the dither origin for 8-bits-per-pixel and 16-bits-per-pixel dithering and is also used to set the origin for hatch brushes.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Point RenderingOrigin
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetRenderingOrigin(nativeObject, out var x, out var y));
			return new Point(x, y);
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetRenderingOrigin(nativeObject, value.X, value.Y));
		}
	}

	/// <summary>Gets or sets the rendering quality for this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Drawing2D.SmoothingMode" /> values.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public SmoothingMode SmoothingMode
	{
		get
		{
			SmoothingMode smoothingMode = SmoothingMode.Invalid;
			GDIPlus.CheckStatus(GDIPlus.GdipGetSmoothingMode(nativeObject, out smoothingMode));
			return smoothingMode;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetSmoothingMode(nativeObject, value));
		}
	}

	/// <summary>Gets or sets the gamma correction value for rendering text.</summary>
	/// <returns>The gamma correction value used for rendering antialiased and ClearType text.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO("This property does not do anything when used with libgdiplus.")]
	public int TextContrast
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetTextContrast(nativeObject, out var contrast));
			return contrast;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetTextContrast(nativeObject, value));
		}
	}

	/// <summary>Gets or sets the rendering mode for text associated with this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Text.TextRenderingHint" /> values.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public TextRenderingHint TextRenderingHint
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetTextRenderingHint(nativeObject, out var mode));
			return mode;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetTextRenderingHint(nativeObject, value));
		}
	}

	/// <summary>Gets or sets a copy of the geometric world transformation for this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>A copy of the <see cref="T:System.Drawing.Drawing2D.Matrix" /> that represents the geometric world transformation for this <see cref="T:System.Drawing.Graphics" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Matrix Transform
	{
		get
		{
			Matrix matrix = new Matrix();
			GDIPlus.CheckStatus(GDIPlus.GdipGetWorldTransform(nativeObject, matrix.nativeMatrix));
			return matrix;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetWorldTransform(nativeObject, value.nativeMatrix));
		}
	}

	/// <summary>Gets the bounding rectangle of the visible clipping region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.RectangleF" /> structure that represents a bounding rectangle for the visible clipping region of this <see cref="T:System.Drawing.Graphics" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public RectangleF VisibleClipBounds
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetVisibleClipBounds(nativeObject, out var rect));
			return rect;
		}
	}

	internal Graphics(IntPtr nativeGraphics)
	{
		nativeObject = IntPtr.Zero;
		base._002Ector();
		nativeObject = nativeGraphics;
	}

	internal Graphics(IntPtr nativeGraphics, Image image)
		: this(nativeGraphics)
	{
		if (image is Metafile metafile)
		{
			_metafileHolder = metafile.AddMetafileHolder();
		}
	}

	~Graphics()
	{
		Dispose();
	}

	/// <summary>Adds a comment to the current <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="data">Array of bytes that contains the comment. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles, both WMF and EMF formats, aren't supported.")]
	public void AddMetafileComment(byte[] data)
	{
		throw new NotImplementedException();
	}

	/// <summary>Saves a graphics container with the current state of this <see cref="T:System.Drawing.Graphics" /> and opens and uses a new graphics container.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.Drawing2D.GraphicsContainer" /> that represents the state of this <see cref="T:System.Drawing.Graphics" /> at the time of the method call.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public GraphicsContainer BeginContainer()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipBeginContainer2(nativeObject, out var state));
		return new GraphicsContainer(state);
	}

	/// <summary>Saves a graphics container with the current state of this <see cref="T:System.Drawing.Graphics" /> and opens and uses a new graphics container with the specified scale transformation.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.Drawing2D.GraphicsContainer" /> that represents the state of this <see cref="T:System.Drawing.Graphics" /> at the time of the method call.</returns>
	/// <param name="dstrect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that, together with the <paramref name="srcrect" /> parameter, specifies a scale transformation for the container. </param>
	/// <param name="srcrect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that, together with the <paramref name="dstrect" /> parameter, specifies a scale transformation for the container. </param>
	/// <param name="unit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure for the container. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("The rectangles and unit parameters aren't supported in libgdiplus")]
	public GraphicsContainer BeginContainer(Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipBeginContainerI(nativeObject, ref dstrect, ref srcrect, unit, out var state));
		return new GraphicsContainer(state);
	}

	/// <summary>Saves a graphics container with the current state of this <see cref="T:System.Drawing.Graphics" /> and opens and uses a new graphics container with the specified scale transformation.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.Drawing2D.GraphicsContainer" /> that represents the state of this <see cref="T:System.Drawing.Graphics" /> at the time of the method call.</returns>
	/// <param name="dstrect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that, together with the <paramref name="srcrect" /> parameter, specifies a scale transformation for the new graphics container. </param>
	/// <param name="srcrect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that, together with the <paramref name="dstrect" /> parameter, specifies a scale transformation for the new graphics container. </param>
	/// <param name="unit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure for the container. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("The rectangles and unit parameters aren't supported in libgdiplus")]
	public GraphicsContainer BeginContainer(RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipBeginContainer(nativeObject, ref dstrect, ref srcrect, unit, out var state));
		return new GraphicsContainer(state);
	}

	/// <summary>Clears the entire drawing surface and fills it with the specified background color.</summary>
	/// <param name="color">
	///   <see cref="T:System.Drawing.Color" /> structure that represents the background color of the drawing surface. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Clear(Color color)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGraphicsClear(nativeObject, color.ToArgb()));
	}

	/// <summary>Performs a bit-block transfer of color data, corresponding to a rectangle of pixels, from the screen to the drawing surface of the <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="upperLeftSource">The point at the upper-left corner of the source rectangle.</param>
	/// <param name="upperLeftDestination">The point at the upper-left corner of the destination rectangle.</param>
	/// <param name="blockRegionSize">The size of the area to be transferred.</param>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="AllWindows" />
	/// </PermissionSet>
	[System.MonoLimitation("Works on Win32 and on X11 (but not on Cocoa and Quartz)")]
	public void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize)
	{
		CopyFromScreen(upperLeftSource.X, upperLeftSource.Y, upperLeftDestination.X, upperLeftDestination.Y, blockRegionSize, CopyPixelOperation.SourceCopy);
	}

	/// <summary>Performs a bit-block transfer of color data, corresponding to a rectangle of pixels, from the screen to the drawing surface of the <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="upperLeftSource">The point at the upper-left corner of the source rectangle.</param>
	/// <param name="upperLeftDestination">The point at the upper-left corner of the destination rectangle.</param>
	/// <param name="blockRegionSize">The size of the area to be transferred.</param>
	/// <param name="copyPixelOperation">One of the <see cref="T:System.Drawing.CopyPixelOperation" /> values.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="copyPixelOperation" /> is not a member of <see cref="T:System.Drawing.CopyPixelOperation" />.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="AllWindows" />
	/// </PermissionSet>
	[System.MonoLimitation("Works on Win32 and (for CopyPixelOperation.SourceCopy only) on X11 but not on Cocoa and Quartz")]
	public void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
	{
		CopyFromScreen(upperLeftSource.X, upperLeftSource.Y, upperLeftDestination.X, upperLeftDestination.Y, blockRegionSize, copyPixelOperation);
	}

	/// <summary>Performs a bit-block transfer of the color data, corresponding to a rectangle of pixels, from the screen to the drawing surface of the <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="sourceX">The x-coordinate of the point at the upper-left corner of the source rectangle.</param>
	/// <param name="sourceY">The y-coordinate of the point at the upper-left corner of the source rectangle.</param>
	/// <param name="destinationX">The x-coordinate of the point at the upper-left corner of the destination rectangle.</param>
	/// <param name="destinationY">The y-coordinate of the point at the upper-left corner of the destination rectangle.</param>
	/// <param name="blockRegionSize">The size of the area to be transferred.</param>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="AllWindows" />
	/// </PermissionSet>
	[System.MonoLimitation("Works on Win32 and on X11 (but not on Cocoa and Quartz)")]
	public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize)
	{
		CopyFromScreen(sourceX, sourceY, destinationX, destinationY, blockRegionSize, CopyPixelOperation.SourceCopy);
	}

	/// <summary>Performs a bit-block transfer of the color data, corresponding to a rectangle of pixels, from the screen to the drawing surface of the <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="sourceX">The x-coordinate of the point at the upper-left corner of the source rectangle.</param>
	/// <param name="sourceY">The y-coordinate of the point at the upper-left corner of the source rectangle</param>
	/// <param name="destinationX">The x-coordinate of the point at the upper-left corner of the destination rectangle.</param>
	/// <param name="destinationY">The y-coordinate of the point at the upper-left corner of the destination rectangle.</param>
	/// <param name="blockRegionSize">The size of the area to be transferred.</param>
	/// <param name="copyPixelOperation">One of the <see cref="T:System.Drawing.CopyPixelOperation" /> values.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="copyPixelOperation" /> is not a member of <see cref="T:System.Drawing.CopyPixelOperation" />.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="AllWindows" />
	/// </PermissionSet>
	[System.MonoLimitation("Works on Win32 and (for CopyPixelOperation.SourceCopy only) on X11 but not on Cocoa and Quartz")]
	public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
	{
		if (!Enum.IsDefined(typeof(CopyPixelOperation), copyPixelOperation))
		{
			throw new InvalidEnumArgumentException(global::Locale.GetText("Enum argument value '{0}' is not valid for CopyPixelOperation", copyPixelOperation));
		}
		if (GDIPlus.UseX11Drawable)
		{
			CopyFromScreenX11(sourceX, sourceY, destinationX, destinationY, blockRegionSize, copyPixelOperation);
		}
		else if (GDIPlus.UseCarbonDrawable)
		{
			CopyFromScreenMac(sourceX, sourceY, destinationX, destinationY, blockRegionSize, copyPixelOperation);
		}
		else if (GDIPlus.UseCocoaDrawable)
		{
			CopyFromScreenMac(sourceX, sourceY, destinationX, destinationY, blockRegionSize, copyPixelOperation);
		}
		else
		{
			CopyFromScreenWin32(sourceX, sourceY, destinationX, destinationY, blockRegionSize, copyPixelOperation);
		}
	}

	private void CopyFromScreenWin32(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
	{
		IntPtr dC = GDIPlus.GetDC(GDIPlus.GetDesktopWindow());
		IntPtr hdc = GetHdc();
		GDIPlus.BitBlt(hdc, destinationX, destinationY, blockRegionSize.Width, blockRegionSize.Height, dC, sourceX, sourceY, (int)copyPixelOperation);
		GDIPlus.ReleaseDC(IntPtr.Zero, dC);
		ReleaseHdc(hdc);
	}

	private void CopyFromScreenMac(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
	{
		throw new NotImplementedException();
	}

	private void CopyFromScreenX11(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
	{
		int pane = -1;
		int nitems = 0;
		if (copyPixelOperation != CopyPixelOperation.SourceCopy)
		{
			throw new NotImplementedException("Operation not implemented under X11");
		}
		if (GDIPlus.Display == IntPtr.Zero)
		{
			GDIPlus.Display = GDIPlus.XOpenDisplay(IntPtr.Zero);
		}
		IntPtr drawable = GDIPlus.XRootWindow(GDIPlus.Display, 0);
		IntPtr visual = GDIPlus.XDefaultVisual(GDIPlus.Display, 0);
		XVisualInfo vinfo_template = new XVisualInfo
		{
			visualid = GDIPlus.XVisualIDFromVisual(visual)
		};
		IntPtr intPtr = GDIPlus.XGetVisualInfo(GDIPlus.Display, 1, ref vinfo_template, ref nitems);
		vinfo_template = (XVisualInfo)Marshal.PtrToStructure(intPtr, typeof(XVisualInfo));
		IntPtr intPtr2 = GDIPlus.XGetImage(GDIPlus.Display, drawable, sourceX, sourceY, blockRegionSize.Width, blockRegionSize.Height, pane, 2);
		if (intPtr2 == IntPtr.Zero)
		{
			throw new InvalidOperationException($"XGetImage returned NULL when asked to for a {blockRegionSize.Width}x{blockRegionSize.Height} region block");
		}
		Bitmap bitmap = new Bitmap(blockRegionSize.Width, blockRegionSize.Height);
		int num = (int)vinfo_template.red_mask;
		int num2 = (int)vinfo_template.blue_mask;
		int num3 = (int)vinfo_template.green_mask;
		for (int i = 0; i < blockRegionSize.Height; i++)
		{
			for (int j = 0; j < blockRegionSize.Width; j++)
			{
				int num4 = GDIPlus.XGetPixel(intPtr2, j, i);
				int red;
				int green;
				int blue;
				switch (vinfo_template.depth)
				{
				case 16u:
					red = ((num4 & num) >> 8) & 0xFF;
					green = ((num4 & num3) >> 3) & 0xFF;
					blue = ((num4 & num2) << 3) & 0xFF;
					break;
				case 24u:
				case 32u:
					red = ((num4 & num) >> 16) & 0xFF;
					green = ((num4 & num3) >> 8) & 0xFF;
					blue = num4 & num2 & 0xFF;
					break;
				default:
					throw new NotImplementedException(global::Locale.GetText("{0}bbp depth not supported.", vinfo_template.depth));
				}
				bitmap.SetPixel(j, i, Color.FromArgb(255, red, green, blue));
			}
		}
		DrawImage(bitmap, destinationX, destinationY);
		bitmap.Dispose();
		GDIPlus.XDestroyImage(intPtr2);
		GDIPlus.XFree(intPtr);
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Dispose()
	{
		if (!disposed)
		{
			if (deviceContextHdc != IntPtr.Zero)
			{
				ReleaseHdc();
			}
			if (GDIPlus.UseCarbonDrawable || GDIPlus.UseCocoaDrawable)
			{
				Flush();
				if (maccontext != null)
				{
					maccontext.Release();
				}
			}
			Status status = GDIPlus.GdipDeleteGraphics(nativeObject);
			nativeObject = IntPtr.Zero;
			GDIPlus.CheckStatus(status);
			if (_metafileHolder != null)
			{
				Metafile.MetafileHolder metafileHolder = _metafileHolder;
				_metafileHolder = null;
				metafileHolder.GraphicsDisposed();
			}
			disposed = true;
		}
		GC.SuppressFinalize(this);
	}

	/// <summary>Draws an arc representing a portion of an ellipse specified by a <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the arc. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that defines the boundaries of the ellipse. </param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of the arc. </param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle" /> parameter to ending point of the arc. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
	{
		DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
	}

	/// <summary>Draws an arc representing a portion of an ellipse specified by a <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the arc. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that defines the boundaries of the ellipse. </param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of the arc. </param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle" /> parameter to ending point of the arc. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
	{
		DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
	}

	/// <summary>Draws an arc representing a portion of an ellipse specified by a pair of coordinates, a width, and a height.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the arc. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle that defines the ellipse. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle that defines the ellipse. </param>
	/// <param name="width">Width of the rectangle that defines the ellipse. </param>
	/// <param name="height">Height of the rectangle that defines the ellipse. </param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of the arc. </param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle" /> parameter to ending point of the arc. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawArc(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawArc(nativeObject, pen.NativePen, x, y, width, height, startAngle, sweepAngle));
	}

	/// <summary>Draws an arc representing a portion of an ellipse specified by a pair of coordinates, a width, and a height.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the arc. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle that defines the ellipse. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle that defines the ellipse. </param>
	/// <param name="width">Width of the rectangle that defines the ellipse. </param>
	/// <param name="height">Height of the rectangle that defines the ellipse. </param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of the arc. </param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle" /> parameter to ending point of the arc. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="rects" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rects" /> is a zero-length array.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawArc(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawArcI(nativeObject, pen.NativePen, x, y, width, height, startAngle, sweepAngle));
	}

	/// <summary>Draws a Bézier spline defined by four <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="pt1">
	///   <see cref="T:System.Drawing.PointF" /> structure that represents the starting point of the curve. </param>
	/// <param name="pt2">
	///   <see cref="T:System.Drawing.PointF" /> structure that represents the first control point for the curve. </param>
	/// <param name="pt3">
	///   <see cref="T:System.Drawing.PointF" /> structure that represents the second control point for the curve. </param>
	/// <param name="pt4">
	///   <see cref="T:System.Drawing.PointF" /> structure that represents the ending point of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawBezier(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawBezier(nativeObject, pen.NativePen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y));
	}

	/// <summary>Draws a Bézier spline defined by four <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> structure that determines the color, width, and style of the curve. </param>
	/// <param name="pt1">
	///   <see cref="T:System.Drawing.Point" /> structure that represents the starting point of the curve. </param>
	/// <param name="pt2">
	///   <see cref="T:System.Drawing.Point" /> structure that represents the first control point for the curve. </param>
	/// <param name="pt3">
	///   <see cref="T:System.Drawing.Point" /> structure that represents the second control point for the curve. </param>
	/// <param name="pt4">
	///   <see cref="T:System.Drawing.Point" /> structure that represents the ending point of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawBezier(Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawBezierI(nativeObject, pen.NativePen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y));
	}

	/// <summary>Draws a Bézier spline defined by four ordered pairs of coordinates that represent points.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="x1">The x-coordinate of the starting point of the curve. </param>
	/// <param name="y1">The y-coordinate of the starting point of the curve. </param>
	/// <param name="x2">The x-coordinate of the first control point of the curve. </param>
	/// <param name="y2">The y-coordinate of the first control point of the curve. </param>
	/// <param name="x3">The x-coordinate of the second control point of the curve. </param>
	/// <param name="y3">The y-coordinate of the second control point of the curve. </param>
	/// <param name="x4">The x-coordinate of the ending point of the curve. </param>
	/// <param name="y4">The y-coordinate of the ending point of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawBezier(Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawBezier(nativeObject, pen.NativePen, x1, y1, x2, y2, x3, y3, x4, y4));
	}

	/// <summary>Draws a series of Bézier splines from an array of <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that represent the points that determine the curve. The number of points in the array should be a multiple of 3 plus 1, such as 4, 7, or 10.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawBeziers(Pen pen, Point[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		int num = points.Length;
		if (num >= 4)
		{
			for (int i = 0; i < num - 1; i += 3)
			{
				Point point = points[i];
				Point point2 = points[i + 1];
				Point point3 = points[i + 2];
				Point point4 = points[i + 3];
				GDIPlus.CheckStatus(GDIPlus.GdipDrawBezier(nativeObject, pen.NativePen, point.X, point.Y, point2.X, point2.Y, point3.X, point3.Y, point4.X, point4.Y));
			}
		}
	}

	/// <summary>Draws a series of Bézier splines from an array of <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that represent the points that determine the curve. The number of points in the array should be a multiple of 3 plus 1, such as 4, 7, or 10.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawBeziers(Pen pen, PointF[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		int num = points.Length;
		if (num >= 4)
		{
			for (int i = 0; i < num - 1; i += 3)
			{
				PointF pointF = points[i];
				PointF pointF2 = points[i + 1];
				PointF pointF3 = points[i + 2];
				PointF pointF4 = points[i + 3];
				GDIPlus.CheckStatus(GDIPlus.GdipDrawBezier(nativeObject, pen.NativePen, pointF.X, pointF.Y, pointF2.X, pointF2.Y, pointF3.X, pointF3.Y, pointF4.X, pointF4.Y));
			}
		}
	}

	/// <summary>Draws a closed cardinal spline defined by an array of <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and height of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that define the spline. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawClosedCurve(Pen pen, PointF[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawClosedCurve(nativeObject, pen.NativePen, points, points.Length));
	}

	/// <summary>Draws a closed cardinal spline defined by an array of <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and height of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that define the spline. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawClosedCurve(Pen pen, Point[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawClosedCurveI(nativeObject, pen.NativePen, points, points.Length));
	}

	/// <summary>Draws a closed cardinal spline defined by an array of <see cref="T:System.Drawing.Point" /> structures using a specified tension.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and height of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that define the spline. </param>
	/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve. </param>
	/// <param name="fillmode">Member of the <see cref="T:System.Drawing.Drawing2D.FillMode" /> enumeration that determines how the curve is filled. This parameter is required but ignored. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawClosedCurve(Pen pen, Point[] points, float tension, FillMode fillmode)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawClosedCurve2I(nativeObject, pen.NativePen, points, points.Length, tension));
	}

	/// <summary>Draws a closed cardinal spline defined by an array of <see cref="T:System.Drawing.PointF" /> structures using a specified tension.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and height of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that define the spline. </param>
	/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve. </param>
	/// <param name="fillmode">Member of the <see cref="T:System.Drawing.Drawing2D.FillMode" /> enumeration that determines how the curve is filled. This parameter is required but is ignored. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawClosedCurve(Pen pen, PointF[] points, float tension, FillMode fillmode)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawClosedCurve2(nativeObject, pen.NativePen, points, points.Length, tension));
	}

	/// <summary>Draws a cardinal spline through a specified array of <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and height of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that define the spline. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawCurve(Pen pen, Point[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawCurveI(nativeObject, pen.NativePen, points, points.Length));
	}

	/// <summary>Draws a cardinal spline through a specified array of <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that define the spline. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawCurve(Pen pen, PointF[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawCurve(nativeObject, pen.NativePen, points, points.Length));
	}

	/// <summary>Draws a cardinal spline through a specified array of <see cref="T:System.Drawing.PointF" /> structures using a specified tension.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that represent the points that define the curve. </param>
	/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawCurve(Pen pen, PointF[] points, float tension)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawCurve2(nativeObject, pen.NativePen, points, points.Length, tension));
	}

	/// <summary>Draws a cardinal spline through a specified array of <see cref="T:System.Drawing.Point" /> structures using a specified tension.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that define the spline. </param>
	/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawCurve(Pen pen, Point[] points, float tension)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawCurve2I(nativeObject, pen.NativePen, points, points.Length, tension));
	}

	/// <summary>Draws a cardinal spline through a specified array of <see cref="T:System.Drawing.PointF" /> structures. The drawing begins offset from the beginning of the array.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that define the spline. </param>
	/// <param name="offset">Offset from the first element in the array of the <paramref name="points" /> parameter to the starting point in the curve. </param>
	/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawCurve3(nativeObject, pen.NativePen, points, points.Length, offset, numberOfSegments, 0.5f));
	}

	/// <summary>Draws a cardinal spline through a specified array of <see cref="T:System.Drawing.Point" /> structures using a specified tension.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that define the spline. </param>
	/// <param name="offset">Offset from the first element in the array of the <paramref name="points" /> parameter to the starting point in the curve. </param>
	/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve. </param>
	/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawCurve(Pen pen, Point[] points, int offset, int numberOfSegments, float tension)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawCurve3I(nativeObject, pen.NativePen, points, points.Length, offset, numberOfSegments, tension));
	}

	/// <summary>Draws a cardinal spline through a specified array of <see cref="T:System.Drawing.PointF" /> structures using a specified tension. The drawing begins offset from the beginning of the array.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the curve. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that define the spline. </param>
	/// <param name="offset">Offset from the first element in the array of the <paramref name="points" /> parameter to the starting point in the curve. </param>
	/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve. </param>
	/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments, float tension)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawCurve3(nativeObject, pen.NativePen, points, points.Length, offset, numberOfSegments, tension));
	}

	/// <summary>Draws an ellipse specified by a bounding <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the ellipse. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that defines the boundaries of the ellipse. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawEllipse(Pen pen, Rectangle rect)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
	}

	/// <summary>Draws an ellipse defined by a bounding <see cref="T:System.Drawing.RectangleF" />.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the ellipse. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that defines the boundaries of the ellipse. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawEllipse(Pen pen, RectangleF rect)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
	}

	/// <summary>Draws an ellipse defined by a bounding rectangle specified by coordinates for the upper-left corner of the rectangle, a height, and a width.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the ellipse. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse. </param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse. </param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawEllipse(Pen pen, int x, int y, int width, int height)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawEllipseI(nativeObject, pen.NativePen, x, y, width, height));
	}

	/// <summary>Draws an ellipse defined by a bounding rectangle specified by a pair of coordinates, a height, and a width.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the ellipse. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse. </param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse. </param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawEllipse(Pen pen, float x, float y, float width, float height)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawEllipse(nativeObject, pen.NativePen, x, y, width, height));
	}

	/// <summary>Draws the image represented by the specified <see cref="T:System.Drawing.Icon" /> within the area specified by a <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="icon">
	///   <see cref="T:System.Drawing.Icon" /> to draw. </param>
	/// <param name="targetRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the resulting image on the display surface. The image contained in the <paramref name="icon" /> parameter is scaled to the dimensions of this rectangular area. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="icon" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawIcon(Icon icon, Rectangle targetRect)
	{
		if (icon == null)
		{
			throw new ArgumentNullException("icon");
		}
		DrawImage(icon.GetInternalBitmap(), targetRect);
	}

	/// <summary>Draws the image represented by the specified <see cref="T:System.Drawing.Icon" /> at the specified coordinates.</summary>
	/// <param name="icon">
	///   <see cref="T:System.Drawing.Icon" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="icon" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawIcon(Icon icon, int x, int y)
	{
		if (icon == null)
		{
			throw new ArgumentNullException("icon");
		}
		DrawImage(icon.GetInternalBitmap(), x, y);
	}

	/// <summary>Draws the image represented by the specified <see cref="T:System.Drawing.Icon" /> without scaling the image.</summary>
	/// <param name="icon">
	///   <see cref="T:System.Drawing.Icon" /> to draw. </param>
	/// <param name="targetRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the resulting image. The image is not scaled to fit this rectangle, but retains its original size. If the image is larger than the rectangle, it is clipped to fit inside it. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="icon" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawIconUnstretched(Icon icon, Rectangle targetRect)
	{
		if (icon == null)
		{
			throw new ArgumentNullException("icon");
		}
		DrawImageUnscaled(icon.GetInternalBitmap(), targetRect);
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location and size of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawImage(Image image, RectangleF rect)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRect(nativeObject, image.NativeObject, rect.X, rect.Y, rect.Width, rect.Height));
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" />, using its original physical size, at the specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="point">
	///   <see cref="T:System.Drawing.PointF" /> structure that represents the upper-left corner of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawImage(Image image, PointF point)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImage(nativeObject, image.NativeObject, point.X, point.Y));
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified shape and size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Point[] destPoints)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsI(nativeObject, image.NativeObject, destPoints, destPoints.Length));
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" />, using its original physical size, at the specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="point">
	///   <see cref="T:System.Drawing.Point" /> structure that represents the location of the upper-left corner of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawImage(Image image, Point point)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		DrawImage(image, point.X, point.Y);
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawImage(Image image, Rectangle rect)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified shape and size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, PointF[] destPoints)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePoints(nativeObject, image.NativeObject, destPoints, destPoints.Length));
	}

	/// <summary>Draws the specified image, using its original physical size, at the location specified by a coordinate pair.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, int x, int y)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageI(nativeObject, image.NativeObject, x, y));
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" />, using its original physical size, at the specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, float x, float y)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImage(nativeObject, image.NativeObject, x, y));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRectI(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRect(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsRectI(nativeObject, image.NativeObject, destPoints, destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsRect(nativeObject, image.NativeObject, destPoints, destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsRectI(nativeObject, image.NativeObject, destPoints, destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, imageAttr?.nativeImageAttributes ?? IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="width">Width of the drawn image. </param>
	/// <param name="height">Height of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, float x, float y, float width, float height)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRect(nativeObject, image.NativeObject, x, y, width, height));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsRect(nativeObject, image.NativeObject, destPoints, destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, imageAttr?.nativeImageAttributes ?? IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws a portion of an image at a specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointRectI(nativeObject, image.NativeObject, x, y, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit));
	}

	/// <summary>Draws the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="width">Width of the drawn image. </param>
	/// <param name="height">Height of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, int x, int y, int width, int height)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectI(nativeObject, image.nativeObject, x, y, width, height));
	}

	/// <summary>Draws a portion of an image at a specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointRect(nativeObject, image.nativeObject, x, y, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate that specifies a method to call during the drawing of the image. This method is called frequently to check whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.PointF[],System.Drawing.RectangleF,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort)" />  method according to application-determined criteria. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsRect(nativeObject, image.NativeObject, destPoints, destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, imageAttr?.nativeImageAttributes ?? IntPtr.Zero, callback, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate that specifies a method to call during the drawing of the image. This method is called frequently to check whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Point[],System.Drawing.Rectangle,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort)" /> method according to application-determined criteria. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsRectI(nativeObject, image.NativeObject, destPoints, destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, imageAttr?.nativeImageAttributes ?? IntPtr.Zero, callback, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate that specifies a method to call during the drawing of the image. This method is called frequently to check whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Point[],System.Drawing.Rectangle,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.Int32)" /> method according to application-determined criteria. </param>
	/// <param name="callbackData">Value specifying additional data for the <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate to use when checking whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Point[],System.Drawing.Rectangle,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.Int32)" /> method. </param>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (destPoints == null)
		{
			throw new ArgumentNullException("destPoints");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsRectI(nativeObject, image.NativeObject, destPoints, destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, imageAttr?.nativeImageAttributes ?? IntPtr.Zero, callback, (IntPtr)callbackData));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcWidth">Width of the portion of the source image to draw. </param>
	/// <param name="srcHeight">Height of the portion of the source image to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used to determine the source rectangle. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRect(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, srcUnit, IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the <paramref name="image" /> object to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used by the <paramref name="srcRect" /> parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate that specifies a method to call during the drawing of the image. This method is called frequently to check whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.PointF[],System.Drawing.RectangleF,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.Int32)" /> method according to application-determined criteria. </param>
	/// <param name="callbackData">Value specifying additional data for the <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate to use when checking whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.PointF[],System.Drawing.RectangleF,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.Int32)" /> method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImagePointsRect(nativeObject, image.NativeObject, destPoints, destPoints.Length, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit, imageAttr?.nativeImageAttributes ?? IntPtr.Zero, callback, (IntPtr)callbackData));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcWidth">Width of the portion of the source image to draw. </param>
	/// <param name="srcHeight">Height of the portion of the source image to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used to determine the source rectangle. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRectI(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, srcUnit, IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcWidth">Width of the portion of the source image to draw. </param>
	/// <param name="srcHeight">Height of the portion of the source image to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used to determine the source rectangle. </param>
	/// <param name="imageAttrs">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRect(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs?.nativeImageAttributes ?? IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcWidth">Width of the portion of the source image to draw. </param>
	/// <param name="srcHeight">Height of the portion of the source image to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used to determine the source rectangle. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRectI(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr?.nativeImageAttributes ?? IntPtr.Zero, null, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcWidth">Width of the portion of the source image to draw. </param>
	/// <param name="srcHeight">Height of the portion of the source image to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used to determine the source rectangle. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for <paramref name="image" />. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate that specifies a method to call during the drawing of the image. This method is called frequently to check whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Rectangle,System.Int32,System.Int32,System.Int32,System.Int32,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort)" /> method according to application-determined criteria. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRectI(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr?.nativeImageAttributes ?? IntPtr.Zero, callback, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcWidth">Width of the portion of the source image to draw. </param>
	/// <param name="srcHeight">Height of the portion of the source image to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used to determine the source rectangle. </param>
	/// <param name="imageAttrs">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate that specifies a method to call during the drawing of the image. This method is called frequently to check whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Rectangle,System.Single,System.Single,System.Single,System.Single,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort)" /> method according to application-determined criteria. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRect(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs?.nativeImageAttributes ?? IntPtr.Zero, callback, IntPtr.Zero));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcWidth">Width of the portion of the source image to draw. </param>
	/// <param name="srcHeight">Height of the portion of the source image to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used to determine the source rectangle. </param>
	/// <param name="imageAttrs">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate that specifies a method to call during the drawing of the image. This method is called frequently to check whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Rectangle,System.Single,System.Single,System.Single,System.Single,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.IntPtr)" /> method according to application-determined criteria. </param>
	/// <param name="callbackData">Value specifying additional data for the <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate to use when checking whether to stop execution of the DrawImage method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRect(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs?.nativeImageAttributes ?? IntPtr.Zero, callback, callbackData));
	}

	/// <summary>Draws the specified portion of the specified <see cref="T:System.Drawing.Image" /> at the specified location and with the specified size.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
	/// <param name="srcX">The x-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcY">The y-coordinate of the upper-left corner of the portion of the source image to draw. </param>
	/// <param name="srcWidth">Width of the portion of the source image to draw. </param>
	/// <param name="srcHeight">Height of the portion of the source image to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the units of measure used to determine the source rectangle. </param>
	/// <param name="imageAttrs">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies recoloring and gamma information for the <paramref name="image" /> object. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate that specifies a method to call during the drawing of the image. This method is called frequently to check whether to stop execution of the <see cref="M:System.Drawing.Graphics.DrawImage(System.Drawing.Image,System.Drawing.Rectangle,System.Int32,System.Int32,System.Int32,System.Int32,System.Drawing.GraphicsUnit,System.Drawing.Imaging.ImageAttributes,System.Drawing.Graphics.DrawImageAbort,System.IntPtr)" /> method according to application-determined criteria. </param>
	/// <param name="callbackData">Value specifying additional data for the <see cref="T:System.Drawing.Graphics.DrawImageAbort" /> delegate to use when checking whether to stop execution of the DrawImage method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRect(nativeObject, image.NativeObject, destRect.X, destRect.Y, destRect.Width, destRect.Height, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs?.nativeImageAttributes ?? IntPtr.Zero, callback, callbackData));
	}

	/// <summary>Draws a specified image using its original physical size at a specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="point">
	///   <see cref="T:System.Drawing.Point" /> structure that specifies the upper-left corner of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawImageUnscaled(Image image, Point point)
	{
		DrawImageUnscaled(image, point.X, point.Y);
	}

	/// <summary>Draws a specified image using its original physical size at a specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> that specifies the upper-left corner of the drawn image. The X and Y properties of the rectangle specify the upper-left corner. The Width and Height properties are ignored. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawImageUnscaled(Image image, Rectangle rect)
	{
		DrawImageUnscaled(image, rect.X, rect.Y, rect.Width, rect.Height);
	}

	/// <summary>Draws the specified image using its original physical size at the location specified by a coordinate pair.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImageUnscaled(Image image, int x, int y)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		DrawImage(image, x, y, image.Width, image.Height);
	}

	/// <summary>Draws a specified image using its original physical size at a specified location.</summary>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> to draw. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn image. </param>
	/// <param name="width">Not used. </param>
	/// <param name="height">Not used. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawImageUnscaled(Image image, int x, int y, int width, int height)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (width <= 0 || height <= 0)
		{
			return;
		}
		using Image image2 = new Bitmap(width, height);
		using Graphics graphics = FromImage(image2);
		graphics.DrawImage(image, 0, 0, image.Width, image.Height);
		DrawImage(image2, x, y, width, height);
	}

	/// <summary>Draws the specified image without scaling and clips it, if necessary, to fit in the specified rectangle.</summary>
	/// <param name="image">The <see cref="T:System.Drawing.Image" /> to draw.</param>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> in which to draw the image.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawImageUnscaledAndClipped(Image image, Rectangle rect)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		int width = ((image.Width > rect.Width) ? rect.Width : image.Width);
		int height = ((image.Height > rect.Height) ? rect.Height : image.Height);
		DrawImageUnscaled(image, rect.X, rect.Y, width, height);
	}

	/// <summary>Draws a line connecting two <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the line. </param>
	/// <param name="pt1">
	///   <see cref="T:System.Drawing.PointF" /> structure that represents the first point to connect. </param>
	/// <param name="pt2">
	///   <see cref="T:System.Drawing.PointF" /> structure that represents the second point to connect. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawLine(Pen pen, PointF pt1, PointF pt2)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawLine(nativeObject, pen.NativePen, pt1.X, pt1.Y, pt2.X, pt2.Y));
	}

	/// <summary>Draws a line connecting two <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the line. </param>
	/// <param name="pt1">
	///   <see cref="T:System.Drawing.Point" /> structure that represents the first point to connect. </param>
	/// <param name="pt2">
	///   <see cref="T:System.Drawing.Point" /> structure that represents the second point to connect. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawLine(Pen pen, Point pt1, Point pt2)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawLineI(nativeObject, pen.NativePen, pt1.X, pt1.Y, pt2.X, pt2.Y));
	}

	/// <summary>Draws a line connecting the two points specified by the coordinate pairs.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the line. </param>
	/// <param name="x1">The x-coordinate of the first point. </param>
	/// <param name="y1">The y-coordinate of the first point. </param>
	/// <param name="x2">The x-coordinate of the second point. </param>
	/// <param name="y2">The y-coordinate of the second point. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawLineI(nativeObject, pen.NativePen, x1, y1, x2, y2));
	}

	/// <summary>Draws a line connecting the two points specified by the coordinate pairs.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the line. </param>
	/// <param name="x1">The x-coordinate of the first point. </param>
	/// <param name="y1">The y-coordinate of the first point. </param>
	/// <param name="x2">The x-coordinate of the second point. </param>
	/// <param name="y2">The y-coordinate of the second point. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (!float.IsNaN(x1) && !float.IsNaN(y1) && !float.IsNaN(x2) && !float.IsNaN(y2))
		{
			GDIPlus.CheckStatus(GDIPlus.GdipDrawLine(nativeObject, pen.NativePen, x1, y1, x2, y2));
		}
	}

	/// <summary>Draws a series of line segments that connect an array of <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the line segments. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that represent the points to connect. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawLines(Pen pen, PointF[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawLines(nativeObject, pen.NativePen, points, points.Length));
	}

	/// <summary>Draws a series of line segments that connect an array of <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the line segments. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that represent the points to connect. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawLines(Pen pen, Point[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawLinesI(nativeObject, pen.NativePen, points, points.Length));
	}

	/// <summary>Draws a <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the path. </param>
	/// <param name="path">
	///   <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> to draw. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="path" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawPath(Pen pen, GraphicsPath path)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawPath(nativeObject, pen.NativePen, path.nativePath));
	}

	/// <summary>Draws a pie shape defined by an ellipse specified by a <see cref="T:System.Drawing.Rectangle" /> structure and two radial lines.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the pie shape. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that represents the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape. </param>
	/// <param name="sweepAngle">Angle measured in degrees clockwise from the <paramref name="startAngle" /> parameter to the second side of the pie shape. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawPie(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		DrawPie(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
	}

	/// <summary>Draws a pie shape defined by an ellipse specified by a <see cref="T:System.Drawing.RectangleF" /> structure and two radial lines.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the pie shape. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that represents the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape. </param>
	/// <param name="sweepAngle">Angle measured in degrees clockwise from the <paramref name="startAngle" /> parameter to the second side of the pie shape. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawPie(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		DrawPie(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
	}

	/// <summary>Draws a pie shape defined by an ellipse specified by a coordinate pair, a width, a height, and two radial lines.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the pie shape. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape. </param>
	/// <param name="sweepAngle">Angle measured in degrees clockwise from the <paramref name="startAngle" /> parameter to the second side of the pie shape. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawPie(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawPie(nativeObject, pen.NativePen, x, y, width, height, startAngle, sweepAngle));
	}

	/// <summary>Draws a pie shape defined by an ellipse specified by a coordinate pair, a width, a height, and two radial lines.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the pie shape. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie shape comes. </param>
	/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape. </param>
	/// <param name="sweepAngle">Angle measured in degrees clockwise from the <paramref name="startAngle" /> parameter to the second side of the pie shape. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawPie(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawPieI(nativeObject, pen.NativePen, x, y, width, height, startAngle, sweepAngle));
	}

	/// <summary>Draws a polygon defined by an array of <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the polygon. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that represent the vertices of the polygon. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawPolygon(Pen pen, Point[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawPolygonI(nativeObject, pen.NativePen, points, points.Length));
	}

	/// <summary>Draws a polygon defined by an array of <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the polygon. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that represent the vertices of the polygon. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawPolygon(Pen pen, PointF[] points)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawPolygon(nativeObject, pen.NativePen, points, points.Length));
	}

	/// <summary>Draws a rectangle specified by a <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="pen">A <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the rectangle. </param>
	/// <param name="rect">A <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle to draw. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void DrawRectangle(Pen pen, Rectangle rect)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		DrawRectangle(pen, rect.Left, rect.Top, rect.Width, rect.Height);
	}

	/// <summary>Draws a rectangle specified by a coordinate pair, a width, and a height.</summary>
	/// <param name="pen">A <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the rectangle. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw. </param>
	/// <param name="width">The width of the rectangle to draw. </param>
	/// <param name="height">The height of the rectangle to draw. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawRectangle(Pen pen, float x, float y, float width, float height)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawRectangle(nativeObject, pen.NativePen, x, y, width, height));
	}

	/// <summary>Draws a rectangle specified by a coordinate pair, a width, and a height.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the rectangle. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw. </param>
	/// <param name="width">Width of the rectangle to draw. </param>
	/// <param name="height">Height of the rectangle to draw. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawRectangle(Pen pen, int x, int y, int width, int height)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawRectangleI(nativeObject, pen.NativePen, x, y, width, height));
	}

	/// <summary>Draws a series of rectangles specified by <see cref="T:System.Drawing.RectangleF" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the outlines of the rectangles. </param>
	/// <param name="rects">Array of <see cref="T:System.Drawing.RectangleF" /> structures that represent the rectangles to draw. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="rects" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rects" /> is a zero-length array.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawRectangles(Pen pen, RectangleF[] rects)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("image");
		}
		if (rects == null)
		{
			throw new ArgumentNullException("rects");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawRectangles(nativeObject, pen.NativePen, rects, rects.Length));
	}

	/// <summary>Draws a series of rectangles specified by <see cref="T:System.Drawing.Rectangle" /> structures.</summary>
	/// <param name="pen">
	///   <see cref="T:System.Drawing.Pen" /> that determines the color, width, and style of the outlines of the rectangles. </param>
	/// <param name="rects">Array of <see cref="T:System.Drawing.Rectangle" /> structures that represent the rectangles to draw. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pen" /> is null.-or-<paramref name="rects" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rects" /> is a zero-length array.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawRectangles(Pen pen, Rectangle[] rects)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("image");
		}
		if (rects == null)
		{
			throw new ArgumentNullException("rects");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipDrawRectanglesI(nativeObject, pen.NativePen, rects, rects.Length));
	}

	/// <summary>Draws the specified text string in the specified rectangle with the specified <see cref="T:System.Drawing.Brush" /> and <see cref="T:System.Drawing.Font" /> objects.</summary>
	/// <param name="s">String to draw. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the color and texture of the drawn text. </param>
	/// <param name="layoutRectangle">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location of the drawn text. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="s" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle)
	{
		DrawString(s, font, brush, layoutRectangle, null);
	}

	/// <summary>Draws the specified text string at the specified location with the specified <see cref="T:System.Drawing.Brush" /> and <see cref="T:System.Drawing.Font" /> objects.</summary>
	/// <param name="s">String to draw. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the color and texture of the drawn text. </param>
	/// <param name="point">
	///   <see cref="T:System.Drawing.PointF" /> structure that specifies the upper-left corner of the drawn text. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="s" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawString(string s, Font font, Brush brush, PointF point)
	{
		DrawString(s, font, brush, new RectangleF(point.X, point.Y, 0f, 0f), null);
	}

	/// <summary>Draws the specified text string at the specified location with the specified <see cref="T:System.Drawing.Brush" /> and <see cref="T:System.Drawing.Font" /> objects using the formatting attributes of the specified <see cref="T:System.Drawing.StringFormat" />.</summary>
	/// <param name="s">String to draw. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the color and texture of the drawn text. </param>
	/// <param name="point">
	///   <see cref="T:System.Drawing.PointF" /> structure that specifies the upper-left corner of the drawn text. </param>
	/// <param name="format">
	///   <see cref="T:System.Drawing.StringFormat" /> that specifies formatting attributes, such as line spacing and alignment, that are applied to the drawn text. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="s" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
	{
		DrawString(s, font, brush, new RectangleF(point.X, point.Y, 0f, 0f), format);
	}

	/// <summary>Draws the specified text string at the specified location with the specified <see cref="T:System.Drawing.Brush" /> and <see cref="T:System.Drawing.Font" /> objects.</summary>
	/// <param name="s">String to draw. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the color and texture of the drawn text. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn text. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn text. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="s" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawString(string s, Font font, Brush brush, float x, float y)
	{
		DrawString(s, font, brush, new RectangleF(x, y, 0f, 0f), null);
	}

	/// <summary>Draws the specified text string at the specified location with the specified <see cref="T:System.Drawing.Brush" /> and <see cref="T:System.Drawing.Font" /> objects using the formatting attributes of the specified <see cref="T:System.Drawing.StringFormat" />.</summary>
	/// <param name="s">String to draw. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the color and texture of the drawn text. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the drawn text. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the drawn text. </param>
	/// <param name="format">
	///   <see cref="T:System.Drawing.StringFormat" /> that specifies formatting attributes, such as line spacing and alignment, that are applied to the drawn text. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="s" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawString(string s, Font font, Brush brush, float x, float y, StringFormat format)
	{
		DrawString(s, font, brush, new RectangleF(x, y, 0f, 0f), format);
	}

	/// <summary>Draws the specified text string in the specified rectangle with the specified <see cref="T:System.Drawing.Brush" /> and <see cref="T:System.Drawing.Font" /> objects using the formatting attributes of the specified <see cref="T:System.Drawing.StringFormat" />.</summary>
	/// <param name="s">String to draw. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the color and texture of the drawn text. </param>
	/// <param name="layoutRectangle">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location of the drawn text. </param>
	/// <param name="format">
	///   <see cref="T:System.Drawing.StringFormat" /> that specifies formatting attributes, such as line spacing and alignment, that are applied to the drawn text. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="s" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (s != null && s.Length != 0)
		{
			GDIPlus.CheckStatus(GDIPlus.GdipDrawString(nativeObject, s, s.Length, font.NativeObject, ref layoutRectangle, format?.NativeObject ?? IntPtr.Zero, brush.NativeBrush));
		}
	}

	/// <summary>Closes the current graphics container and restores the state of this <see cref="T:System.Drawing.Graphics" /> to the state saved by a call to the <see cref="M:System.Drawing.Graphics.BeginContainer" /> method.</summary>
	/// <param name="container">
	///   <see cref="T:System.Drawing.Drawing2D.GraphicsContainer" /> that represents the container this method restores. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void EndContainer(GraphicsContainer container)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipEndContainer(nativeObject, container.NativeObject));
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.Point" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point destPoint, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.PointF" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF destPoint, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.PointF" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.Point" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.PointF" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.Point" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structures that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.Point" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.PointF" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point[] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of the specified <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.PointF" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.Point" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="srcUnit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.Point" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="unit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point[] destPoints, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="unit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.Point" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="unit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records of a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified rectangle using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the location and size of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="unit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display in a specified parallelogram using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoints">Array of three <see cref="T:System.Drawing.PointF" /> structures that define a parallelogram that determines the size and location of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="unit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF[] destPoints, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends the records in a selected rectangle from a <see cref="T:System.Drawing.Imaging.Metafile" />, one at a time, to a callback method for display at a specified point using specified image attributes.</summary>
	/// <param name="metafile">
	///   <see cref="T:System.Drawing.Imaging.Metafile" /> to enumerate. </param>
	/// <param name="destPoint">
	///   <see cref="T:System.Drawing.PointF" /> structure that specifies the location of the upper-left corner of the drawn metafile. </param>
	/// <param name="srcRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the portion of the metafile, relative to its upper-left corner, to draw. </param>
	/// <param name="unit">Member of the <see cref="T:System.Drawing.GraphicsUnit" /> enumeration that specifies the unit of measure used to determine the portion of the metafile that the rectangle specified by the <paramref name="srcRect" /> parameter contains. </param>
	/// <param name="callback">
	///   <see cref="T:System.Drawing.Graphics.EnumerateMetafileProc" /> delegate that specifies the method to which the metafile records are sent. </param>
	/// <param name="callbackData">Internal pointer that is required, but ignored. You can pass <see cref="F:System.IntPtr.Zero" /> for this parameter. </param>
	/// <param name="imageAttr">
	///   <see cref="T:System.Drawing.Imaging.ImageAttributes" /> that specifies image attribute information for the drawn image. </param>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO("Metafiles enumeration, for both WMF and EMF formats, isn't supported.")]
	public void EnumerateMetafile(Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
	{
		throw new NotImplementedException();
	}

	/// <summary>Updates the clip region of this <see cref="T:System.Drawing.Graphics" /> to exclude the area specified by a <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that specifies the rectangle to exclude from the clip region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ExcludeClip(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipRectI(nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Exclude));
	}

	/// <summary>Updates the clip region of this <see cref="T:System.Drawing.Graphics" /> to exclude the area specified by a <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="region">
	///   <see cref="T:System.Drawing.Region" /> that specifies the region to exclude from the clip region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ExcludeClip(Region region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipRegion(nativeObject, region.NativeObject, CombineMode.Exclude));
	}

	/// <summary>Fills the interior of a closed cardinal spline curve defined by an array of <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that define the spline. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillClosedCurve(Brush brush, PointF[] points)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillClosedCurve(nativeObject, brush.NativeBrush, points, points.Length));
	}

	/// <summary>Fills the interior of a closed cardinal spline curve defined by an array of <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that define the spline. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillClosedCurve(Brush brush, Point[] points)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillClosedCurveI(nativeObject, brush.NativeBrush, points, points.Length));
	}

	/// <summary>Fills the interior of a closed cardinal spline curve defined by an array of <see cref="T:System.Drawing.PointF" /> structures using the specified fill mode.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that define the spline. </param>
	/// <param name="fillmode">Member of the <see cref="T:System.Drawing.Drawing2D.FillMode" /> enumeration that determines how the curve is filled. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillmode)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		FillClosedCurve(brush, points, fillmode, 0.5f);
	}

	/// <summary>Fills the interior of a closed cardinal spline curve defined by an array of <see cref="T:System.Drawing.Point" /> structures using the specified fill mode.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that define the spline. </param>
	/// <param name="fillmode">Member of the <see cref="T:System.Drawing.Drawing2D.FillMode" /> enumeration that determines how the curve is filled. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillClosedCurve(Brush brush, Point[] points, FillMode fillmode)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		FillClosedCurve(brush, points, fillmode, 0.5f);
	}

	/// <summary>Fills the interior of a closed cardinal spline curve defined by an array of <see cref="T:System.Drawing.PointF" /> structures using the specified fill mode and tension.</summary>
	/// <param name="brush">A <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that define the spline. </param>
	/// <param name="fillmode">Member of the <see cref="T:System.Drawing.Drawing2D.FillMode" /> enumeration that determines how the curve is filled. </param>
	/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillmode, float tension)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillClosedCurve2(nativeObject, brush.NativeBrush, points, points.Length, tension, fillmode));
	}

	/// <summary>Fills the interior of a closed cardinal spline curve defined by an array of <see cref="T:System.Drawing.Point" /> structures using the specified fill mode and tension.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that define the spline. </param>
	/// <param name="fillmode">Member of the <see cref="T:System.Drawing.Drawing2D.FillMode" /> enumeration that determines how the curve is filled. </param>
	/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillClosedCurve(Brush brush, Point[] points, FillMode fillmode, float tension)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillClosedCurve2I(nativeObject, brush.NativeBrush, points, points.Length, tension, fillmode));
	}

	/// <summary>Fills the interior of an ellipse defined by a bounding rectangle specified by a <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that represents the bounding rectangle that defines the ellipse. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void FillEllipse(Brush brush, Rectangle rect)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
	}

	/// <summary>Fills the interior of an ellipse defined by a bounding rectangle specified by a <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that represents the bounding rectangle that defines the ellipse. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void FillEllipse(Brush brush, RectangleF rect)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
	}

	/// <summary>Fills the interior of an ellipse defined by a bounding rectangle specified by a pair of coordinates, a width, and a height.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse. </param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse. </param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillEllipse(Brush brush, float x, float y, float width, float height)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillEllipse(nativeObject, brush.NativeBrush, x, y, width, height));
	}

	/// <summary>Fills the interior of an ellipse defined by a bounding rectangle specified by a pair of coordinates, a width, and a height.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse. </param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse. </param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillEllipse(Brush brush, int x, int y, int width, int height)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillEllipseI(nativeObject, brush.NativeBrush, x, y, width, height));
	}

	/// <summary>Fills the interior of a <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="path">
	///   <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> that represents the path to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="path" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void FillPath(Brush brush, GraphicsPath path)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillPath(nativeObject, brush.NativeBrush, path.nativePath));
	}

	/// <summary>Fills the interior of a pie section defined by an ellipse specified by a <see cref="T:System.Drawing.RectangleF" /> structure and two radial lines.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that represents the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the first side of the pie section. </param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle" /> parameter to the second side of the pie section. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillPie(Brush brush, Rectangle rect, float startAngle, float sweepAngle)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillPie(nativeObject, brush.NativeBrush, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle));
	}

	/// <summary>Fills the interior of a pie section defined by an ellipse specified by a pair of coordinates, a width, a height, and two radial lines.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the first side of the pie section. </param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle" /> parameter to the second side of the pie section. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillPie(Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillPieI(nativeObject, brush.NativeBrush, x, y, width, height, startAngle, sweepAngle));
	}

	/// <summary>Fills the interior of a pie section defined by an ellipse specified by a pair of coordinates, a width, a height, and two radial lines.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie section comes. </param>
	/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the first side of the pie section. </param>
	/// <param name="sweepAngle">Angle in degrees measured clockwise from the <paramref name="startAngle" /> parameter to the second side of the pie section. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillPie(Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillPie(nativeObject, brush.NativeBrush, x, y, width, height, startAngle, sweepAngle));
	}

	/// <summary>Fills the interior of a polygon defined by an array of points specified by <see cref="T:System.Drawing.PointF" /> structures.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that represent the vertices of the polygon to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillPolygon(Brush brush, PointF[] points)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillPolygon2(nativeObject, brush.NativeBrush, points, points.Length));
	}

	/// <summary>Fills the interior of a polygon defined by an array of points specified by <see cref="T:System.Drawing.Point" /> structures.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that represent the vertices of the polygon to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillPolygon(Brush brush, Point[] points)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillPolygon2I(nativeObject, brush.NativeBrush, points, points.Length));
	}

	/// <summary>Fills the interior of a polygon defined by an array of points specified by <see cref="T:System.Drawing.Point" /> structures using the specified fill mode.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.Point" /> structures that represent the vertices of the polygon to fill. </param>
	/// <param name="fillMode">Member of the <see cref="T:System.Drawing.Drawing2D.FillMode" /> enumeration that determines the style of the fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillPolygon(Brush brush, Point[] points, FillMode fillMode)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillPolygonI(nativeObject, brush.NativeBrush, points, points.Length, fillMode));
	}

	/// <summary>Fills the interior of a polygon defined by an array of points specified by <see cref="T:System.Drawing.PointF" /> structures using the specified fill mode.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="points">Array of <see cref="T:System.Drawing.PointF" /> structures that represent the vertices of the polygon to fill. </param>
	/// <param name="fillMode">Member of the <see cref="T:System.Drawing.Drawing2D.FillMode" /> enumeration that determines the style of the fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="points" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillPolygon(Brush brush, PointF[] points, FillMode fillMode)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillPolygon(nativeObject, brush.NativeBrush, points, points.Length, fillMode));
	}

	/// <summary>Fills the interior of a rectangle specified by a <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void FillRectangle(Brush brush, RectangleF rect)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		FillRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height);
	}

	/// <summary>Fills the interior of a rectangle specified by a <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void FillRectangle(Brush brush, Rectangle rect)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		FillRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height);
	}

	/// <summary>Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to fill. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to fill. </param>
	/// <param name="width">Width of the rectangle to fill. </param>
	/// <param name="height">Height of the rectangle to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillRectangle(Brush brush, int x, int y, int width, int height)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillRectangleI(nativeObject, brush.NativeBrush, x, y, width, height));
	}

	/// <summary>Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to fill. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to fill. </param>
	/// <param name="width">Width of the rectangle to fill. </param>
	/// <param name="height">Height of the rectangle to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillRectangle(Brush brush, float x, float y, float width, float height)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillRectangle(nativeObject, brush.NativeBrush, x, y, width, height));
	}

	/// <summary>Fills the interiors of a series of rectangles specified by <see cref="T:System.Drawing.Rectangle" /> structures.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="rects">Array of <see cref="T:System.Drawing.Rectangle" /> structures that represent the rectangles to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="rects" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rects" /> is a zero-length array.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillRectangles(Brush brush, Rectangle[] rects)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (rects == null)
		{
			throw new ArgumentNullException("rects");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillRectanglesI(nativeObject, brush.NativeBrush, rects, rects.Length));
	}

	/// <summary>Fills the interiors of a series of rectangles specified by <see cref="T:System.Drawing.RectangleF" /> structures.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="rects">Array of <see cref="T:System.Drawing.RectangleF" /> structures that represent the rectangles to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="rects" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="Rects" /> is a zero-length array.</exception>
	/// <filterpriority>1</filterpriority>
	public void FillRectangles(Brush brush, RectangleF[] rects)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (rects == null)
		{
			throw new ArgumentNullException("rects");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillRectangles(nativeObject, brush.NativeBrush, rects, rects.Length));
	}

	/// <summary>Fills the interior of a <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="brush">
	///   <see cref="T:System.Drawing.Brush" /> that determines the characteristics of the fill. </param>
	/// <param name="region">
	///   <see cref="T:System.Drawing.Region" /> that represents the area to fill. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.-or-<paramref name="region" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void FillRegion(Brush brush, Region region)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipFillRegion(nativeObject, brush.NativeBrush, region.NativeObject));
	}

	/// <summary>Forces execution of all pending graphics operations and returns immediately without waiting for the operations to finish.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Flush()
	{
		Flush(FlushIntention.Flush);
	}

	/// <summary>Forces execution of all pending graphics operations with the method waiting or not waiting, as specified, to return before the operations finish.</summary>
	/// <param name="intention">Member of the <see cref="T:System.Drawing.Drawing2D.FlushIntention" /> enumeration that specifies whether the method returns immediately or waits for any existing operations to finish. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Flush(FlushIntention intention)
	{
		if (!(nativeObject == IntPtr.Zero))
		{
			GDIPlus.CheckStatus(GDIPlus.GdipFlush(nativeObject, intention));
			if (maccontext != null)
			{
				maccontext.Synchronize();
			}
		}
	}

	/// <summary>Creates a new <see cref="T:System.Drawing.Graphics" /> from the specified handle to a device context.</summary>
	/// <returns>This method returns a new <see cref="T:System.Drawing.Graphics" /> for the specified device context.</returns>
	/// <param name="hdc">Handle to a device context. </param>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static Graphics FromHdc(IntPtr hdc)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateFromHDC(hdc, out var graphics));
		return new Graphics(graphics);
	}

	/// <summary>Creates a new <see cref="T:System.Drawing.Graphics" /> from the specified handle to a device context and handle to a device.</summary>
	/// <returns>This method returns a new <see cref="T:System.Drawing.Graphics" /> for the specified device context and device.</returns>
	/// <param name="hdc">Handle to a device context. </param>
	/// <param name="hdevice">Handle to a device. </param>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[System.MonoTODO]
	public static Graphics FromHdc(IntPtr hdc, IntPtr hdevice)
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a <see cref="T:System.Drawing.Graphics" /> for the specified device context.</summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> for the specified device context.</returns>
	/// <param name="hdc">Handle to a device context. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static Graphics FromHdcInternal(IntPtr hdc)
	{
		GDIPlus.Display = hdc;
		return null;
	}

	/// <summary>Creates a new <see cref="T:System.Drawing.Graphics" /> from the specified handle to a window.</summary>
	/// <returns>This method returns a new <see cref="T:System.Drawing.Graphics" /> for the specified window handle.</returns>
	/// <param name="hwnd">Handle to a window. </param>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static Graphics FromHwnd(IntPtr hwnd)
	{
		IntPtr graphics;
		if (GDIPlus.UseCocoaDrawable)
		{
			if (hwnd == IntPtr.Zero)
			{
				throw new NotSupportedException("Opening display graphics is not supported");
			}
			CocoaContext cGContextForNSView = MacSupport.GetCGContextForNSView(hwnd);
			GDIPlus.GdipCreateFromContext_macosx(cGContextForNSView.ctx, cGContextForNSView.width, cGContextForNSView.height, out graphics);
			return new Graphics(graphics)
			{
				maccontext = cGContextForNSView
			};
		}
		if (GDIPlus.UseCarbonDrawable)
		{
			CarbonContext cGContextForView = MacSupport.GetCGContextForView(hwnd);
			GDIPlus.GdipCreateFromContext_macosx(cGContextForView.ctx, cGContextForView.width, cGContextForView.height, out graphics);
			return new Graphics(graphics)
			{
				maccontext = cGContextForView
			};
		}
		if (GDIPlus.UseX11Drawable)
		{
			if (GDIPlus.Display == IntPtr.Zero)
			{
				GDIPlus.Display = GDIPlus.XOpenDisplay(IntPtr.Zero);
				if (GDIPlus.Display == IntPtr.Zero)
				{
					throw new NotSupportedException("Could not open display (X-Server required. Check your DISPLAY environment variable)");
				}
			}
			if (hwnd == IntPtr.Zero)
			{
				hwnd = GDIPlus.XRootWindow(GDIPlus.Display, GDIPlus.XDefaultScreen(GDIPlus.Display));
			}
			return FromXDrawable(hwnd, GDIPlus.Display);
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateFromHWND(hwnd, out graphics));
		return new Graphics(graphics);
	}

	/// <summary>Creates a new <see cref="T:System.Drawing.Graphics" /> for the specified windows handle.</summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> for the specified window handle.</returns>
	/// <param name="hwnd">Handle to a window. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static Graphics FromHwndInternal(IntPtr hwnd)
	{
		return FromHwnd(hwnd);
	}

	/// <summary>Creates a new <see cref="T:System.Drawing.Graphics" /> from the specified <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>This method returns a new <see cref="T:System.Drawing.Graphics" /> for the specified <see cref="T:System.Drawing.Image" />.</returns>
	/// <param name="image">
	///   <see cref="T:System.Drawing.Image" /> from which to create the new <see cref="T:System.Drawing.Graphics" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <exception cref="T:System.Exception">
	///   <paramref name="image" /> has an indexed pixel format or its format is undefined.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Graphics FromImage(Image image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if ((image.PixelFormat & PixelFormat.Indexed) != PixelFormat.Undefined)
		{
			throw new Exception(global::Locale.GetText("Cannot create Graphics from an indexed bitmap."));
		}
		GDIPlus.CheckStatus(GDIPlus.GdipGetImageGraphicsContext(image.nativeObject, out var graphics));
		Graphics graphics2 = new Graphics(graphics, image);
		if (GDIPlus.RunningOnUnix())
		{
			Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
			GDIPlus.GdipSetVisibleClip_linux(graphics2.NativeObject, ref rect);
		}
		return graphics2;
	}

	internal static Graphics FromXDrawable(IntPtr drawable, IntPtr display)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateFromXDrawable_linux(drawable, display, out var graphics));
		return new Graphics(graphics);
	}

	/// <summary>Gets a handle to the current Windows halftone palette.</summary>
	/// <returns>Internal pointer that specifies the handle to the palette.</returns>
	/// <filterpriority>1</filterpriority>
	[System.MonoTODO]
	public static IntPtr GetHalftonePalette()
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets the handle to the device context associated with this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>Handle to the device context associated with this <see cref="T:System.Drawing.Graphics" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public IntPtr GetHdc()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetDC(nativeObject, out deviceContextHdc));
		return deviceContextHdc;
	}

	/// <summary>Gets the nearest color to the specified <see cref="T:System.Drawing.Color" /> structure.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> structure that represents the nearest color to the one specified with the <paramref name="color" /> parameter.</returns>
	/// <param name="color">
	///   <see cref="T:System.Drawing.Color" /> structure for which to find a match. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Color GetNearestColor(Color color)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetNearestColor(nativeObject, out var argb));
		return Color.FromArgb(argb);
	}

	/// <summary>Updates the clip region of this <see cref="T:System.Drawing.Graphics" /> to the intersection of the current clip region and the specified <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="region">
	///   <see cref="T:System.Drawing.Region" /> to intersect with the current region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void IntersectClip(Region region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipRegion(nativeObject, region.NativeObject, CombineMode.Intersect));
	}

	/// <summary>Updates the clip region of this <see cref="T:System.Drawing.Graphics" /> to the intersection of the current clip region and the specified <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure to intersect with the current clip region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void IntersectClip(RectangleF rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipRect(nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect));
	}

	/// <summary>Updates the clip region of this <see cref="T:System.Drawing.Graphics" /> to the intersection of the current clip region and the specified <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure to intersect with the current clip region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void IntersectClip(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipRectI(nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect));
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Drawing.Point" /> structure is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true if the point specified by the <paramref name="point" /> parameter is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />; otherwise, false.</returns>
	/// <param name="point">
	///   <see cref="T:System.Drawing.Point" /> structure to test for visibility. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(Point point)
	{
		bool result = false;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisiblePointI(nativeObject, point.X, point.Y, out result));
		return result;
	}

	/// <summary>Indicates whether the rectangle specified by a <see cref="T:System.Drawing.RectangleF" /> structure is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true if the rectangle specified by the <paramref name="rect" /> parameter is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />; otherwise, false.</returns>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure to test for visibility. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(RectangleF rect)
	{
		bool result = false;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRect(nativeObject, rect.X, rect.Y, rect.Width, rect.Height, out result));
		return result;
	}

	/// <summary>Indicates whether the specified <see cref="T:System.Drawing.PointF" /> structure is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true if the point specified by the <paramref name="point" /> parameter is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />; otherwise, false.</returns>
	/// <param name="point">
	///   <see cref="T:System.Drawing.PointF" /> structure to test for visibility. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(PointF point)
	{
		bool result = false;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisiblePoint(nativeObject, point.X, point.Y, out result));
		return result;
	}

	/// <summary>Indicates whether the rectangle specified by a <see cref="T:System.Drawing.Rectangle" /> structure is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true if the rectangle specified by the <paramref name="rect" /> parameter is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />; otherwise, false.</returns>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure to test for visibility. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(Rectangle rect)
	{
		bool result = false;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRectI(nativeObject, rect.X, rect.Y, rect.Width, rect.Height, out result));
		return result;
	}

	/// <summary>Indicates whether the point specified by a pair of coordinates is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true if the point defined by the <paramref name="x" /> and <paramref name="y" /> parameters is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the point to test for visibility. </param>
	/// <param name="y">The y-coordinate of the point to test for visibility. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(float x, float y)
	{
		return IsVisible(new PointF(x, y));
	}

	/// <summary>Indicates whether the point specified by a pair of coordinates is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true if the point defined by the <paramref name="x" /> and <paramref name="y" /> parameters is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the point to test for visibility. </param>
	/// <param name="y">The y-coordinate of the point to test for visibility. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(int x, int y)
	{
		return IsVisible(new Point(x, y));
	}

	/// <summary>Indicates whether the rectangle specified by a pair of coordinates, a width, and a height is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true if the rectangle defined by the <paramref name="x" />, <paramref name="y" />, <paramref name="width" />, and <paramref name="height" /> parameters is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test for visibility. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test for visibility. </param>
	/// <param name="width">Width of the rectangle to test for visibility. </param>
	/// <param name="height">Height of the rectangle to test for visibility. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(float x, float y, float width, float height)
	{
		return IsVisible(new RectangleF(x, y, width, height));
	}

	/// <summary>Indicates whether the rectangle specified by a pair of coordinates, a width, and a height is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true if the rectangle defined by the <paramref name="x" />, <paramref name="y" />, <paramref name="width" />, and <paramref name="height" /> parameters is contained within the visible clip region of this <see cref="T:System.Drawing.Graphics" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test for visibility. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test for visibility. </param>
	/// <param name="width">Width of the rectangle to test for visibility. </param>
	/// <param name="height">Height of the rectangle to test for visibility. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(int x, int y, int width, int height)
	{
		return IsVisible(new Rectangle(x, y, width, height));
	}

	/// <summary>Gets an array of <see cref="T:System.Drawing.Region" /> objects, each of which bounds a range of character positions within the specified string.</summary>
	/// <returns>This method returns an array of <see cref="T:System.Drawing.Region" /> objects, each of which bounds a range of character positions within the specified string.</returns>
	/// <param name="text">String to measure. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="layoutRect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that specifies the layout rectangle for the string. </param>
	/// <param name="stringFormat">
	///   <see cref="T:System.Drawing.StringFormat" /> that represents formatting information, such as line spacing, for the string. </param>
	/// <filterpriority>1</filterpriority>
	public Region[] MeasureCharacterRanges(string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
	{
		if (text == null || text.Length == 0)
		{
			return new Region[0];
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (stringFormat == null)
		{
			throw new ArgumentException("stringFormat");
		}
		int measurableCharacterRangeCount = stringFormat.GetMeasurableCharacterRangeCount();
		if (measurableCharacterRangeCount == 0)
		{
			return new Region[0];
		}
		IntPtr[] array = new IntPtr[measurableCharacterRangeCount];
		Region[] array2 = new Region[measurableCharacterRangeCount];
		for (int i = 0; i < measurableCharacterRangeCount; i++)
		{
			array2[i] = new Region();
			array[i] = array2[i].NativeObject;
		}
		GDIPlus.CheckStatus(GDIPlus.GdipMeasureCharacterRanges(nativeObject, text, text.Length, font.NativeObject, ref layoutRect, stringFormat.NativeObject, measurableCharacterRangeCount, out array[0]));
		return array2;
	}

	private unsafe SizeF GdipMeasureString(IntPtr graphics, string text, Font font, ref RectangleF layoutRect, IntPtr stringFormat)
	{
		if (text == null || text.Length == 0)
		{
			return SizeF.Empty;
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		RectangleF boundingBox = default(RectangleF);
		GDIPlus.CheckStatus(GDIPlus.GdipMeasureString(nativeObject, text, text.Length, font.NativeObject, ref layoutRect, stringFormat, out boundingBox, null, null));
		return new SizeF(boundingBox.Width, boundingBox.Height);
	}

	/// <summary>Measures the specified string when drawn with the specified <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.SizeF" /> structure that represents the size, in the units specified by the <see cref="P:System.Drawing.Graphics.PageUnit" /> property, of the string specified by the <paramref name="text" /> parameter as drawn with the <paramref name="font" /> parameter.</returns>
	/// <param name="text">String to measure. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="font" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public SizeF MeasureString(string text, Font font)
	{
		return MeasureString(text, font, SizeF.Empty);
	}

	/// <summary>Measures the specified string when drawn with the specified <see cref="T:System.Drawing.Font" /> within the specified layout area.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.SizeF" /> structure that represents the size, in the units specified by the <see cref="P:System.Drawing.Graphics.PageUnit" /> property, of the string specified by the <paramref name="text" /> parameter as drawn with the <paramref name="font" /> parameter.</returns>
	/// <param name="text">String to measure. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> defines the text format of the string. </param>
	/// <param name="layoutArea">
	///   <see cref="T:System.Drawing.SizeF" /> structure that specifies the maximum layout area for the text. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="font" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public SizeF MeasureString(string text, Font font, SizeF layoutArea)
	{
		RectangleF layoutRect = new RectangleF(0f, 0f, layoutArea.Width, layoutArea.Height);
		return GdipMeasureString(nativeObject, text, font, ref layoutRect, IntPtr.Zero);
	}

	/// <summary>Measures the specified string when drawn with the specified <see cref="T:System.Drawing.Font" />.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.SizeF" /> structure that represents the size, in the units specified by the <see cref="P:System.Drawing.Graphics.PageUnit" /> property, of the string specified in the <paramref name="text" /> parameter as drawn with the <paramref name="font" /> parameter.</returns>
	/// <param name="text">String to measure. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the format of the string. </param>
	/// <param name="width">Maximum width of the string in pixels. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="font" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public SizeF MeasureString(string text, Font font, int width)
	{
		RectangleF layoutRect = new RectangleF(0f, 0f, width, 2.1474836E+09f);
		return GdipMeasureString(nativeObject, text, font, ref layoutRect, IntPtr.Zero);
	}

	/// <summary>Measures the specified string when drawn with the specified <see cref="T:System.Drawing.Font" /> and formatted with the specified <see cref="T:System.Drawing.StringFormat" />.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.SizeF" /> structure that represents the size, in the units specified by the <see cref="P:System.Drawing.Graphics.PageUnit" /> property, of the string specified in the <paramref name="text" /> parameter as drawn with the <paramref name="font" /> parameter and the <paramref name="stringFormat" /> parameter.</returns>
	/// <param name="text">String to measure. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> defines the text format of the string. </param>
	/// <param name="layoutArea">
	///   <see cref="T:System.Drawing.SizeF" /> structure that specifies the maximum layout area for the text. </param>
	/// <param name="stringFormat">
	///   <see cref="T:System.Drawing.StringFormat" /> that represents formatting information, such as line spacing, for the string. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="font" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat)
	{
		RectangleF layoutRect = new RectangleF(0f, 0f, layoutArea.Width, layoutArea.Height);
		IntPtr stringFormat2 = stringFormat?.NativeObject ?? IntPtr.Zero;
		return GdipMeasureString(nativeObject, text, font, ref layoutRect, stringFormat2);
	}

	/// <summary>Measures the specified string when drawn with the specified <see cref="T:System.Drawing.Font" /> and formatted with the specified <see cref="T:System.Drawing.StringFormat" />.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.SizeF" /> structure that represents the size, in the units specified by the <see cref="P:System.Drawing.Graphics.PageUnit" /> property, of the string specified in the <paramref name="text" /> parameter as drawn with the <paramref name="font" /> parameter and the <paramref name="stringFormat" /> parameter.</returns>
	/// <param name="text">String to measure. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="width">Maximum width of the string. </param>
	/// <param name="format">
	///   <see cref="T:System.Drawing.StringFormat" /> that represents formatting information, such as line spacing, for the string. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="font" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public SizeF MeasureString(string text, Font font, int width, StringFormat format)
	{
		RectangleF layoutRect = new RectangleF(0f, 0f, width, 2.1474836E+09f);
		IntPtr stringFormat = format?.NativeObject ?? IntPtr.Zero;
		return GdipMeasureString(nativeObject, text, font, ref layoutRect, stringFormat);
	}

	/// <summary>Measures the specified string when drawn with the specified <see cref="T:System.Drawing.Font" /> and formatted with the specified <see cref="T:System.Drawing.StringFormat" />.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.SizeF" /> structure that represents the size, in the units specified by the <see cref="P:System.Drawing.Graphics.PageUnit" /> property, of the string specified by the <paramref name="text" /> parameter as drawn with the <paramref name="font" /> parameter and the <paramref name="stringFormat" /> parameter.</returns>
	/// <param name="text">String to measure. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> defines the text format of the string. </param>
	/// <param name="origin">
	///   <see cref="T:System.Drawing.PointF" /> structure that represents the upper-left corner of the string. </param>
	/// <param name="stringFormat">
	///   <see cref="T:System.Drawing.StringFormat" /> that represents formatting information, such as line spacing, for the string. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="font" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public SizeF MeasureString(string text, Font font, PointF origin, StringFormat stringFormat)
	{
		RectangleF layoutRect = new RectangleF(origin.X, origin.Y, 0f, 0f);
		IntPtr stringFormat2 = stringFormat?.NativeObject ?? IntPtr.Zero;
		return GdipMeasureString(nativeObject, text, font, ref layoutRect, stringFormat2);
	}

	/// <summary>Measures the specified string when drawn with the specified <see cref="T:System.Drawing.Font" /> and formatted with the specified <see cref="T:System.Drawing.StringFormat" />.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.SizeF" /> structure that represents the size of the string, in the units specified by the <see cref="P:System.Drawing.Graphics.PageUnit" /> property, of the <paramref name="text" /> parameter as drawn with the <paramref name="font" /> parameter and the <paramref name="stringFormat" /> parameter.</returns>
	/// <param name="text">String to measure. </param>
	/// <param name="font">
	///   <see cref="T:System.Drawing.Font" /> that defines the text format of the string. </param>
	/// <param name="layoutArea">
	///   <see cref="T:System.Drawing.SizeF" /> structure that specifies the maximum layout area for the text. </param>
	/// <param name="stringFormat">
	///   <see cref="T:System.Drawing.StringFormat" /> that represents formatting information, such as line spacing, for the string. </param>
	/// <param name="charactersFitted">Number of characters in the string. </param>
	/// <param name="linesFilled">Number of text lines in the string. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="font" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public unsafe SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled)
	{
		charactersFitted = 0;
		linesFilled = 0;
		if (text == null || text.Length == 0)
		{
			return SizeF.Empty;
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		RectangleF boundingBox = default(RectangleF);
		RectangleF layoutRect = new RectangleF(0f, 0f, layoutArea.Width, layoutArea.Height);
		IntPtr stringFormat2 = stringFormat?.NativeObject ?? IntPtr.Zero;
		fixed (int* codepointsFitted = &charactersFitted)
		{
			fixed (int* linesFilled2 = &linesFilled)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipMeasureString(nativeObject, text, text.Length, font.NativeObject, ref layoutRect, stringFormat2, out boundingBox, codepointsFitted, linesFilled2));
			}
		}
		return new SizeF(boundingBox.Width, boundingBox.Height);
	}

	/// <summary>Multiplies the world transformation of this <see cref="T:System.Drawing.Graphics" /> and specified the <see cref="T:System.Drawing.Drawing2D.Matrix" />.</summary>
	/// <param name="matrix">4x4 <see cref="T:System.Drawing.Drawing2D.Matrix" /> that multiplies the world transformation. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void MultiplyTransform(Matrix matrix)
	{
		MultiplyTransform(matrix, MatrixOrder.Prepend);
	}

	/// <summary>Multiplies the world transformation of this <see cref="T:System.Drawing.Graphics" /> and specified the <see cref="T:System.Drawing.Drawing2D.Matrix" /> in the specified order.</summary>
	/// <param name="matrix">4x4 <see cref="T:System.Drawing.Drawing2D.Matrix" /> that multiplies the world transformation. </param>
	/// <param name="order">Member of the <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> enumeration that determines the order of the multiplication. </param>
	/// <filterpriority>1</filterpriority>
	public void MultiplyTransform(Matrix matrix, MatrixOrder order)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipMultiplyWorldTransform(nativeObject, matrix.nativeMatrix, order));
	}

	/// <summary>Releases a device context handle obtained by a previous call to the <see cref="M:System.Drawing.Graphics.GetHdc" /> method of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="hdc">Handle to a device context obtained by a previous call to the <see cref="M:System.Drawing.Graphics.GetHdc" /> method of this <see cref="T:System.Drawing.Graphics" />. </param>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public void ReleaseHdc(IntPtr hdc)
	{
		ReleaseHdcInternal(hdc);
	}

	/// <summary>Releases a device context handle obtained by a previous call to the <see cref="M:System.Drawing.Graphics.GetHdc" /> method of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public void ReleaseHdc()
	{
		ReleaseHdcInternal(deviceContextHdc);
	}

	/// <summary>Releases a handle to a device context.</summary>
	/// <param name="hdc">Handle to a device context. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoLimitation("Can only be used when hdc was provided by Graphics.GetHdc() method")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void ReleaseHdcInternal(IntPtr hdc)
	{
		Status status = Status.InvalidParameter;
		if (hdc == deviceContextHdc)
		{
			status = GDIPlus.GdipReleaseDC(nativeObject, deviceContextHdc);
			deviceContextHdc = IntPtr.Zero;
		}
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Resets the clip region of this <see cref="T:System.Drawing.Graphics" /> to an infinite region.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ResetClip()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipResetClip(nativeObject));
	}

	/// <summary>Resets the world transformation matrix of this <see cref="T:System.Drawing.Graphics" /> to the identity matrix.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ResetTransform()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipResetWorldTransform(nativeObject));
	}

	/// <summary>Restores the state of this <see cref="T:System.Drawing.Graphics" /> to the state represented by a <see cref="T:System.Drawing.Drawing2D.GraphicsState" />.</summary>
	/// <param name="gstate">
	///   <see cref="T:System.Drawing.Drawing2D.GraphicsState" /> that represents the state to which to restore this <see cref="T:System.Drawing.Graphics" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Restore(GraphicsState gstate)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRestoreGraphics(nativeObject, (uint)gstate.nativeState));
	}

	/// <summary>Applies the specified rotation to the transformation matrix of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="angle">Angle of rotation in degrees. </param>
	/// <filterpriority>1</filterpriority>
	public void RotateTransform(float angle)
	{
		RotateTransform(angle, MatrixOrder.Prepend);
	}

	/// <summary>Applies the specified rotation to the transformation matrix of this <see cref="T:System.Drawing.Graphics" /> in the specified order.</summary>
	/// <param name="angle">Angle of rotation in degrees. </param>
	/// <param name="order">Member of the <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> enumeration that specifies whether the rotation is appended or prepended to the matrix transformation. </param>
	/// <filterpriority>1</filterpriority>
	public void RotateTransform(float angle, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRotateWorldTransform(nativeObject, angle, order));
	}

	/// <summary>Saves the current state of this <see cref="T:System.Drawing.Graphics" /> and identifies the saved state with a <see cref="T:System.Drawing.Drawing2D.GraphicsState" />.</summary>
	/// <returns>This method returns a <see cref="T:System.Drawing.Drawing2D.GraphicsState" /> that represents the saved state of this <see cref="T:System.Drawing.Graphics" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public GraphicsState Save()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSaveGraphics(nativeObject, out var state));
		return new GraphicsState((int)state);
	}

	/// <summary>Applies the specified scaling operation to the transformation matrix of this <see cref="T:System.Drawing.Graphics" /> by prepending it to the object's transformation matrix.</summary>
	/// <param name="sx">Scale factor in the x direction. </param>
	/// <param name="sy">Scale factor in the y direction. </param>
	/// <filterpriority>1</filterpriority>
	public void ScaleTransform(float sx, float sy)
	{
		ScaleTransform(sx, sy, MatrixOrder.Prepend);
	}

	/// <summary>Applies the specified scaling operation to the transformation matrix of this <see cref="T:System.Drawing.Graphics" /> in the specified order.</summary>
	/// <param name="sx">Scale factor in the x direction. </param>
	/// <param name="sy">Scale factor in the y direction. </param>
	/// <param name="order">Member of the <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> enumeration that specifies whether the scaling operation is prepended or appended to the transformation matrix. </param>
	/// <filterpriority>1</filterpriority>
	public void ScaleTransform(float sx, float sy, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipScaleWorldTransform(nativeObject, sx, sy, order));
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the rectangle specified by a <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure that represents the new clip region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetClip(RectangleF rect)
	{
		SetClip(rect, CombineMode.Replace);
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="path">
	///   <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> that represents the new clip region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetClip(GraphicsPath path)
	{
		SetClip(path, CombineMode.Replace);
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the rectangle specified by a <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure that represents the new clip region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetClip(Rectangle rect)
	{
		SetClip(rect, CombineMode.Replace);
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the Clip property of the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="g">
	///   <see cref="T:System.Drawing.Graphics" /> from which to take the new clip region. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetClip(Graphics g)
	{
		SetClip(g, CombineMode.Replace);
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the result of the specified combining operation of the current clip region and the <see cref="P:System.Drawing.Graphics.Clip" /> property of the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="g">
	///   <see cref="T:System.Drawing.Graphics" /> that specifies the clip region to combine. </param>
	/// <param name="combineMode">Member of the <see cref="T:System.Drawing.Drawing2D.CombineMode" /> enumeration that specifies the combining operation to use. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetClip(Graphics g, CombineMode combineMode)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipGraphics(nativeObject, g.NativeObject, combineMode));
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the result of the specified operation combining the current clip region and the rectangle specified by a <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.Rectangle" /> structure to combine. </param>
	/// <param name="combineMode">Member of the <see cref="T:System.Drawing.Drawing2D.CombineMode" /> enumeration that specifies the combining operation to use. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetClip(Rectangle rect, CombineMode combineMode)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipRectI(nativeObject, rect.X, rect.Y, rect.Width, rect.Height, combineMode));
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the result of the specified operation combining the current clip region and the rectangle specified by a <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">
	///   <see cref="T:System.Drawing.RectangleF" /> structure to combine. </param>
	/// <param name="combineMode">Member of the <see cref="T:System.Drawing.Drawing2D.CombineMode" /> enumeration that specifies the combining operation to use. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetClip(RectangleF rect, CombineMode combineMode)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipRect(nativeObject, rect.X, rect.Y, rect.Width, rect.Height, combineMode));
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the result of the specified operation combining the current clip region and the specified <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="region">
	///   <see cref="T:System.Drawing.Region" /> to combine. </param>
	/// <param name="combineMode">Member from the <see cref="T:System.Drawing.Drawing2D.CombineMode" /> enumeration that specifies the combining operation to use. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetClip(Region region, CombineMode combineMode)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipRegion(nativeObject, region.NativeObject, combineMode));
	}

	/// <summary>Sets the clipping region of this <see cref="T:System.Drawing.Graphics" /> to the result of the specified operation combining the current clip region and the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="path">
	///   <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> to combine. </param>
	/// <param name="combineMode">Member of the <see cref="T:System.Drawing.Drawing2D.CombineMode" /> enumeration that specifies the combining operation to use. </param>
	/// <filterpriority>1</filterpriority>
	public void SetClip(GraphicsPath path, CombineMode combineMode)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetClipPath(nativeObject, path.nativePath, combineMode));
	}

	/// <summary>Transforms an array of points from one coordinate space to another using the current world and page transformations of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="destSpace">Member of the <see cref="T:System.Drawing.Drawing2D.CoordinateSpace" /> enumeration that specifies the destination coordinate space. </param>
	/// <param name="srcSpace">Member of the <see cref="T:System.Drawing.Drawing2D.CoordinateSpace" /> enumeration that specifies the source coordinate space. </param>
	/// <param name="pts">Array of <see cref="T:System.Drawing.PointF" /> structures that represent the points to transform. </param>
	/// <filterpriority>1</filterpriority>
	public void TransformPoints(CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF[] pts)
	{
		if (pts == null)
		{
			throw new ArgumentNullException("pts");
		}
		IntPtr intPtr = GDIPlus.FromPointToUnManagedMemory(pts);
		GDIPlus.CheckStatus(GDIPlus.GdipTransformPoints(nativeObject, destSpace, srcSpace, intPtr, pts.Length));
		GDIPlus.FromUnManagedMemoryToPoint(intPtr, pts);
	}

	/// <summary>Transforms an array of points from one coordinate space to another using the current world and page transformations of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="destSpace">Member of the <see cref="T:System.Drawing.Drawing2D.CoordinateSpace" /> enumeration that specifies the destination coordinate space. </param>
	/// <param name="srcSpace">Member of the <see cref="T:System.Drawing.Drawing2D.CoordinateSpace" /> enumeration that specifies the source coordinate space. </param>
	/// <param name="pts">Array of <see cref="T:System.Drawing.Point" /> structures that represents the points to transformation. </param>
	/// <filterpriority>1</filterpriority>
	public void TransformPoints(CoordinateSpace destSpace, CoordinateSpace srcSpace, Point[] pts)
	{
		if (pts == null)
		{
			throw new ArgumentNullException("pts");
		}
		IntPtr intPtr = GDIPlus.FromPointToUnManagedMemoryI(pts);
		GDIPlus.CheckStatus(GDIPlus.GdipTransformPointsI(nativeObject, destSpace, srcSpace, intPtr, pts.Length));
		GDIPlus.FromUnManagedMemoryToPointI(intPtr, pts);
	}

	/// <summary>Translates the clipping region of this <see cref="T:System.Drawing.Graphics" /> by specified amounts in the horizontal and vertical directions.</summary>
	/// <param name="dx">The x-coordinate of the translation. </param>
	/// <param name="dy">The y-coordinate of the translation. </param>
	/// <filterpriority>1</filterpriority>
	public void TranslateClip(int dx, int dy)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipTranslateClipI(nativeObject, dx, dy));
	}

	/// <summary>Translates the clipping region of this <see cref="T:System.Drawing.Graphics" /> by specified amounts in the horizontal and vertical directions.</summary>
	/// <param name="dx">The x-coordinate of the translation. </param>
	/// <param name="dy">The y-coordinate of the translation. </param>
	/// <filterpriority>1</filterpriority>
	public void TranslateClip(float dx, float dy)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipTranslateClip(nativeObject, dx, dy));
	}

	/// <summary>Changes the origin of the coordinate system by prepending the specified translation to the transformation matrix of this <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <param name="dx">The x-coordinate of the translation. </param>
	/// <param name="dy">The y-coordinate of the translation. </param>
	/// <filterpriority>1</filterpriority>
	public void TranslateTransform(float dx, float dy)
	{
		TranslateTransform(dx, dy, MatrixOrder.Prepend);
	}

	/// <summary>Changes the origin of the coordinate system by applying the specified translation to the transformation matrix of this <see cref="T:System.Drawing.Graphics" /> in the specified order.</summary>
	/// <param name="dx">The x-coordinate of the translation. </param>
	/// <param name="dy">The y-coordinate of the translation. </param>
	/// <param name="order">Member of the <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> enumeration that specifies whether the translation is prepended or appended to the transformation matrix. </param>
	/// <filterpriority>1</filterpriority>
	public void TranslateTransform(float dx, float dy, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipTranslateWorldTransform(nativeObject, dx, dy, order));
	}

	/// <summary>Gets the cumulative graphics context.</summary>
	/// <returns>An <see cref="T:System.Object" /> representing the cumulative graphics context.</returns>
	[System.MonoTODO]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public object GetContextInfo()
	{
		throw new NotImplementedException();
	}

	internal Graphics()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
