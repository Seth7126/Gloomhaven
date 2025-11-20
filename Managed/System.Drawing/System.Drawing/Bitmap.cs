using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Drawing;

/// <summary>Encapsulates a GDI+ bitmap, which consists of the pixel data for a graphics image and its attributes. A <see cref="T:System.Drawing.Bitmap" /> is an object used to work with images defined by pixel data.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
[Editor("System.Drawing.Design.BitmapEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
public sealed class Bitmap : Image
{
	private Bitmap()
	{
	}

	internal Bitmap(IntPtr ptr)
	{
		nativeObject = ptr;
	}

	internal Bitmap(IntPtr ptr, Stream stream)
	{
		if (GDIPlus.RunningOnWindows())
		{
			base.stream = stream;
		}
		nativeObject = ptr;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class with the specified size.</summary>
	/// <param name="width">The width, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="height">The height, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	public Bitmap(int width, int height)
		: this(width, height, PixelFormat.Format32bppArgb)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class with the specified size and with the resolution of the specified <see cref="T:System.Drawing.Graphics" /> object.</summary>
	/// <param name="width">The width, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="height">The height, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="g">The <see cref="T:System.Drawing.Graphics" /> object that specifies the resolution for the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="g" /> is null.</exception>
	public Bitmap(int width, int height, Graphics g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateBitmapFromGraphics(width, height, g.nativeObject, out var bitmap));
		nativeObject = bitmap;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class with the specified size and format.</summary>
	/// <param name="width">The width, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="height">The height, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="format">The pixel format for the new <see cref="T:System.Drawing.Bitmap" />. This must specify a value that begins with <paramref name="Format" />.</param>
	/// <exception cref="T:System.ArgumentException">A <see cref="T:System.Drawing.Imaging.PixelFormat" /> value is specified whose name does not start with Format. For example, specifying <see cref="F:System.Drawing.Imaging.PixelFormat.Gdi" /> will cause an <see cref="T:System.ArgumentException" />, but <see cref="F:System.Drawing.Imaging.PixelFormat.Format48bppRgb" /> will not.</exception>
	public Bitmap(int width, int height, PixelFormat format)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateBitmapFromScan0(width, height, 0, format, IntPtr.Zero, out var bmp));
		nativeObject = bmp;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class from the specified existing image.</summary>
	/// <param name="original">The <see cref="T:System.Drawing.Image" /> from which to create the new <see cref="T:System.Drawing.Bitmap" />. </param>
	public Bitmap(Image original)
		: this(original, original.Width, original.Height)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class from the specified data stream.</summary>
	/// <param name="stream">The data stream used to load the image. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not contain image data or is null.-or-<paramref name="stream" /> contains a PNG image file with a single dimension greater than 65,535 pixels.</exception>
	public Bitmap(Stream stream)
		: this(stream, useIcm: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class from the specified file.</summary>
	/// <param name="filename">The bitmap file name and path. </param>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified file is not found.</exception>
	public Bitmap(string filename)
		: this(filename, useIcm: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class from the specified existing image, scaled to the specified size.</summary>
	/// <param name="original">The <see cref="T:System.Drawing.Image" /> from which to create the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="newSize">The <see cref="T:System.Drawing.Size" /> structure that represent the size of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	public Bitmap(Image original, Size newSize)
		: this(original, newSize.Width, newSize.Height)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class from the specified data stream.</summary>
	/// <param name="stream">The data stream used to load the image. </param>
	/// <param name="useIcm">true to use color correction for this <see cref="T:System.Drawing.Bitmap" />; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> does not contain image data or is null.-or-<paramref name="stream" /> contains a PNG image file with a single dimension greater than 65,535 pixels.</exception>
	public Bitmap(Stream stream, bool useIcm)
	{
		nativeObject = Image.InitFromStream(stream);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class from the specified file.</summary>
	/// <param name="filename">The name of the bitmap file. </param>
	/// <param name="useIcm">true to use color correction for this <see cref="T:System.Drawing.Bitmap" />; otherwise, false. </param>
	public Bitmap(string filename, bool useIcm)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		GDIPlus.CheckStatus((!useIcm) ? GDIPlus.GdipCreateBitmapFromFile(filename, out var bitmap) : GDIPlus.GdipCreateBitmapFromFileICM(filename, out bitmap));
		nativeObject = bitmap;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class from a specified resource.</summary>
	/// <param name="type">The class used to extract the resource. </param>
	/// <param name="resource">The name of the resource. </param>
	public Bitmap(Type type, string resource)
	{
		if (resource == null)
		{
			throw new ArgumentException("resource");
		}
		if (type == null)
		{
			throw new NullReferenceException();
		}
		Stream manifestResourceStream = type.GetTypeInfo().Assembly.GetManifestResourceStream(type, resource);
		if (manifestResourceStream == null)
		{
			throw new FileNotFoundException(global::Locale.GetText("Resource '{0}' was not found.", resource));
		}
		nativeObject = Image.InitFromStream(manifestResourceStream);
		if (GDIPlus.RunningOnWindows())
		{
			stream = manifestResourceStream;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class from the specified existing image, scaled to the specified size.</summary>
	/// <param name="original">The <see cref="T:System.Drawing.Image" /> from which to create the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="width">The width, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="height">The height, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	public Bitmap(Image original, int width, int height)
		: this(width, height, PixelFormat.Format32bppArgb)
	{
		Graphics graphics = Graphics.FromImage(this);
		graphics.DrawImage(original, 0, 0, width, height);
		graphics.Dispose();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Bitmap" /> class with the specified size, pixel format, and pixel data.</summary>
	/// <param name="width">The width, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="height">The height, in pixels, of the new <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="stride">Integer that specifies the byte offset between the beginning of one scan line and the next. This is usually (but not necessarily) the number of bytes in the pixel format (for example, 2 for 16 bits per pixel) multiplied by the width of the bitmap. The value passed to this parameter must be a multiple of four.. </param>
	/// <param name="format">The pixel format for the new <see cref="T:System.Drawing.Bitmap" />. This must specify a value that begins with <paramref name="Format" />.</param>
	/// <param name="scan0">Pointer to an array of bytes that contains the pixel data.</param>
	/// <exception cref="T:System.ArgumentException">A <see cref="T:System.Drawing.Imaging.PixelFormat" /> value is specified whose name does not start with Format. For example, specifying <see cref="F:System.Drawing.Imaging.PixelFormat.Gdi" /> will cause an <see cref="T:System.ArgumentException" />, but <see cref="F:System.Drawing.Imaging.PixelFormat.Format48bppRgb" /> will not.</exception>
	public Bitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateBitmapFromScan0(width, height, stride, format, scan0, out var bmp));
		nativeObject = bmp;
	}

	private Bitmap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Gets the color of the specified pixel in this <see cref="T:System.Drawing.Bitmap" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> structure that represents the color of the specified pixel.</returns>
	/// <param name="x">The x-coordinate of the pixel to retrieve. </param>
	/// <param name="y">The y-coordinate of the pixel to retrieve. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="x" /> is less than 0, or greater than or equal to <see cref="P:System.Drawing.Image.Width" />. -or-<paramref name="y" /> is less than 0, or greater than or equal to <see cref="P:System.Drawing.Image.Height" />.</exception>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	public Color GetPixel(int x, int y)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipBitmapGetPixel(nativeObject, x, y, out var argb));
		return Color.FromArgb(argb);
	}

	/// <summary>Sets the color of the specified pixel in this <see cref="T:System.Drawing.Bitmap" />.</summary>
	/// <param name="x">The x-coordinate of the pixel to set. </param>
	/// <param name="y">The y-coordinate of the pixel to set. </param>
	/// <param name="color">A <see cref="T:System.Drawing.Color" /> structure that represents the color to assign to the specified pixel. </param>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetPixel(int x, int y, Color color)
	{
		Status num = GDIPlus.GdipBitmapSetPixel(nativeObject, x, y, color.ToArgb());
		if (num == Status.InvalidParameter && (base.PixelFormat & PixelFormat.Indexed) != PixelFormat.Undefined)
		{
			throw new InvalidOperationException(global::Locale.GetText("SetPixel cannot be called on indexed bitmaps."));
		}
		GDIPlus.CheckStatus(num);
	}

	/// <summary>Creates a copy of the section of this <see cref="T:System.Drawing.Bitmap" /> defined by <see cref="T:System.Drawing.Rectangle" /> structure and with a specified <see cref="T:System.Drawing.Imaging.PixelFormat" /> enumeration.</summary>
	/// <returns>The new <see cref="T:System.Drawing.Bitmap" /> that this method creates.</returns>
	/// <param name="rect">Defines the portion of this <see cref="T:System.Drawing.Bitmap" /> to copy. Coordinates are relative to this <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="format">The pixel format for the new <see cref="T:System.Drawing.Bitmap" />. This must specify a value that begins with <paramref name="Format" />.</param>
	/// <exception cref="T:System.OutOfMemoryException">
	///   <paramref name="rect" /> is outside of the source bitmap bounds.</exception>
	/// <exception cref="T:System.ArgumentException">The height or width of <paramref name="rect" /> is 0. -or-A <see cref="T:System.Drawing.Imaging.PixelFormat" /> value is specified whose name does not start with Format. For example, specifying <see cref="F:System.Drawing.Imaging.PixelFormat.Gdi" /> will cause an <see cref="T:System.ArgumentException" />, but <see cref="F:System.Drawing.Imaging.PixelFormat.Format48bppRgb" /> will not.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Bitmap Clone(Rectangle rect, PixelFormat format)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCloneBitmapAreaI(rect.X, rect.Y, rect.Width, rect.Height, format, nativeObject, out var bitmap));
		return new Bitmap(bitmap);
	}

	/// <summary>Creates a copy of the section of this <see cref="T:System.Drawing.Bitmap" /> defined with a specified <see cref="T:System.Drawing.Imaging.PixelFormat" /> enumeration.</summary>
	/// <returns>The <see cref="T:System.Drawing.Bitmap" /> that this method creates.</returns>
	/// <param name="rect">Defines the portion of this <see cref="T:System.Drawing.Bitmap" /> to copy. </param>
	/// <param name="format">Specifies the <see cref="T:System.Drawing.Imaging.PixelFormat" /> enumeration for the destination <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <exception cref="T:System.OutOfMemoryException">
	///   <paramref name="rect" /> is outside of the source bitmap bounds.</exception>
	/// <exception cref="T:System.ArgumentException">The height or width of <paramref name="rect" /> is 0. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Bitmap Clone(RectangleF rect, PixelFormat format)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCloneBitmapArea(rect.X, rect.Y, rect.Width, rect.Height, format, nativeObject, out var bitmap));
		return new Bitmap(bitmap);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Bitmap" /> from a Windows handle to an icon.</summary>
	/// <returns>The <see cref="T:System.Drawing.Bitmap" /> that this method creates.</returns>
	/// <param name="hicon">A handle to an icon. </param>
	/// <filterpriority>1</filterpriority>
	public static Bitmap FromHicon(IntPtr hicon)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateBitmapFromHICON(hicon, out var bitmap));
		return new Bitmap(bitmap);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Bitmap" /> from the specified Windows resource.</summary>
	/// <returns>The <see cref="T:System.Drawing.Bitmap" /> that this method creates.</returns>
	/// <param name="hinstance">A handle to an instance of the executable file that contains the resource. </param>
	/// <param name="bitmapName">A string that contains the name of the resource bitmap. </param>
	/// <filterpriority>1</filterpriority>
	public static Bitmap FromResource(IntPtr hinstance, string bitmapName)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateBitmapFromResource(hinstance, bitmapName, out var bitmap));
		return new Bitmap(bitmap);
	}

	/// <summary>Creates a GDI bitmap object from this <see cref="T:System.Drawing.Bitmap" />.</summary>
	/// <returns>A handle to the GDI bitmap object that this method creates.</returns>
	/// <exception cref="T:System.ArgumentException">The height or width of the bitmap is greater than <see cref="F:System.Int16.MaxValue" />.</exception>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public IntPtr GetHbitmap()
	{
		return GetHbitmap(Color.Gray);
	}

	/// <summary>Creates a GDI bitmap object from this <see cref="T:System.Drawing.Bitmap" />.</summary>
	/// <returns>A handle to the GDI bitmap object that this method creates.</returns>
	/// <param name="background">A <see cref="T:System.Drawing.Color" /> structure that specifies the background color. This parameter is ignored if the bitmap is totally opaque. </param>
	/// <exception cref="T:System.ArgumentException">The height or width of the bitmap is greater than <see cref="F:System.Int16.MaxValue" />.</exception>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public IntPtr GetHbitmap(Color background)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateHBITMAPFromBitmap(nativeObject, out var HandleBmp, background.ToArgb()));
		return HandleBmp;
	}

	/// <summary>Returns the handle to an icon.</summary>
	/// <returns>A Windows handle to an icon with the same image as the <see cref="T:System.Drawing.Bitmap" />.</returns>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public IntPtr GetHicon()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateHICONFromBitmap(nativeObject, out var HandleIcon));
		return HandleIcon;
	}

	/// <summary>Locks a <see cref="T:System.Drawing.Bitmap" /> into system memory.</summary>
	/// <returns>A <see cref="T:System.Drawing.Imaging.BitmapData" /> that contains information about this lock operation.</returns>
	/// <param name="rect">A <see cref="T:System.Drawing.Rectangle" /> structure that specifies the portion of the <see cref="T:System.Drawing.Bitmap" /> to lock. </param>
	/// <param name="flags">An <see cref="T:System.Drawing.Imaging.ImageLockMode" /> enumeration that specifies the access level (read/write) for the <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="format">A <see cref="T:System.Drawing.Imaging.PixelFormat" /> enumeration that specifies the data format of this <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Drawing.Imaging.PixelFormat" /> is not a specific bits-per-pixel value.-or-The incorrect <see cref="T:System.Drawing.Imaging.PixelFormat" /> is passed in for a bitmap.</exception>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format)
	{
		BitmapData bitmapData = new BitmapData();
		return LockBits(rect, flags, format, bitmapData);
	}

	/// <summary>Locks a <see cref="T:System.Drawing.Bitmap" /> into system memory </summary>
	/// <returns>A <see cref="T:System.Drawing.Imaging.BitmapData" /> that contains information about the lock operation.</returns>
	/// <param name="rect">A rectangle structure that specifies the portion of the <see cref="T:System.Drawing.Bitmap" /> to lock.</param>
	/// <param name="flags">One of the <see cref="T:System.Drawing.Imaging.ImageLockMode" /> values that specifies the access level (read/write) for the <see cref="T:System.Drawing.Bitmap" />.</param>
	/// <param name="format">One of the <see cref="T:System.Drawing.Imaging.PixelFormat" /> values that specifies the data format of the <see cref="T:System.Drawing.Bitmap" />.</param>
	/// <param name="bitmapData">A <see cref="T:System.Drawing.Imaging.BitmapData" /> that contains information about the lock operation.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="T:System.Drawing.Imaging.PixelFormat" /> value is not a specific bits-per-pixel value.-or-The incorrect <see cref="T:System.Drawing.Imaging.PixelFormat" /> is passed in for a bitmap.</exception>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format, BitmapData bitmapData)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipBitmapLockBits(nativeObject, ref rect, flags, format, bitmapData));
		return bitmapData;
	}

	/// <summary>Makes the default transparent color transparent for this <see cref="T:System.Drawing.Bitmap" />.</summary>
	/// <exception cref="T:System.InvalidOperationException">The image format of the <see cref="T:System.Drawing.Bitmap" /> is an icon format.</exception>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void MakeTransparent()
	{
		Color pixel = GetPixel(0, 0);
		MakeTransparent(pixel);
	}

	/// <summary>Makes the specified color transparent for this <see cref="T:System.Drawing.Bitmap" />.</summary>
	/// <param name="transparentColor">The <see cref="T:System.Drawing.Color" /> structure that represents the color to make transparent. </param>
	/// <exception cref="T:System.InvalidOperationException">The image format of the <see cref="T:System.Drawing.Bitmap" /> is an icon format.</exception>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void MakeTransparent(Color transparentColor)
	{
		Bitmap bitmap = new Bitmap(base.Width, base.Height, PixelFormat.Format32bppArgb);
		Graphics graphics = Graphics.FromImage(bitmap);
		Rectangle destRect = new Rectangle(0, 0, base.Width, base.Height);
		ImageAttributes imageAttributes = new ImageAttributes();
		imageAttributes.SetColorKey(transparentColor, transparentColor);
		graphics.DrawImage(this, destRect, 0, 0, base.Width, base.Height, GraphicsUnit.Pixel, imageAttributes);
		IntPtr intPtr = nativeObject;
		nativeObject = bitmap.nativeObject;
		bitmap.nativeObject = intPtr;
		graphics.Dispose();
		bitmap.Dispose();
		imageAttributes.Dispose();
	}

	/// <summary>Sets the resolution for this <see cref="T:System.Drawing.Bitmap" />.</summary>
	/// <param name="xDpi">The horizontal resolution, in dots per inch, of the <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="yDpi">The vertical resolution, in dots per inch, of the <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetResolution(float xDpi, float yDpi)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipBitmapSetResolution(nativeObject, xDpi, yDpi));
	}

	/// <summary>Unlocks this <see cref="T:System.Drawing.Bitmap" /> from system memory.</summary>
	/// <param name="bitmapdata">A <see cref="T:System.Drawing.Imaging.BitmapData" /> that specifies information about the lock operation. </param>
	/// <exception cref="T:System.Exception">The operation failed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void UnlockBits(BitmapData bitmapdata)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipBitmapUnlockBits(nativeObject, bitmapdata));
	}
}
