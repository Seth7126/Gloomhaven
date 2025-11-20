using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Drawing;

/// <summary>An abstract base class that provides functionality for the <see cref="T:System.Drawing.Bitmap" /> and <see cref="T:System.Drawing.Imaging.Metafile" /> descended classes.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ImmutableObject(true)]
[TypeConverter(typeof(ImageConverter))]
[Editor("System.Drawing.Design.ImageEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
[ComVisible(true)]
public abstract class Image : MarshalByRefObject, IDisposable, ICloneable, ISerializable
{
	/// <summary>Provides a callback method for determining when the <see cref="M:System.Drawing.Image.GetThumbnailImage(System.Int32,System.Int32,System.Drawing.Image.GetThumbnailImageAbort,System.IntPtr)" /> method should prematurely cancel execution.</summary>
	/// <returns>This method returns true if it decides that the <see cref="M:System.Drawing.Image.GetThumbnailImage(System.Int32,System.Int32,System.Drawing.Image.GetThumbnailImageAbort,System.IntPtr)" /> method should prematurely stop execution; otherwise, it returns false.</returns>
	public delegate bool GetThumbnailImageAbort();

	private object tag;

	internal IntPtr nativeObject = IntPtr.Zero;

	internal Stream stream;

	/// <summary>Gets attribute flags for the pixel data of this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>The integer representing a bitwise combination of <see cref="T:System.Drawing.Imaging.ImageFlags" /> for this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	public int Flags
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImageFlags(nativeObject, out var flag));
			return flag;
		}
	}

	/// <summary>Gets an array of GUIDs that represent the dimensions of frames within this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>An array of GUIDs that specify the dimensions of frames within this <see cref="T:System.Drawing.Image" /> from most significant to least significant.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	public Guid[] FrameDimensionsList
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipImageGetFrameDimensionsCount(nativeObject, out var count));
			Guid[] array = new Guid[count];
			GDIPlus.CheckStatus(GDIPlus.GdipImageGetFrameDimensionsList(nativeObject, array, count));
			return array;
		}
	}

	/// <summary>Gets the height, in pixels, of this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>The height, in pixels, of this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[DefaultValue(false)]
	public int Height
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImageHeight(nativeObject, out var height));
			return (int)height;
		}
	}

	/// <summary>Gets the horizontal resolution, in pixels per inch, of this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>The horizontal resolution, in pixels per inch, of this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float HorizontalResolution
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImageHorizontalResolution(nativeObject, out var resolution));
			return resolution;
		}
	}

	/// <summary>Gets or sets the color palette used for this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Imaging.ColorPalette" /> that represents the color palette used for this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	public ColorPalette Palette
	{
		get
		{
			return retrieveGDIPalette();
		}
		set
		{
			storeGDIPalette(value);
		}
	}

	/// <summary>Gets the width and height of this image.</summary>
	/// <returns>A <see cref="T:System.Drawing.SizeF" /> structure that represents the width and height of this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public SizeF PhysicalDimension
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImageDimension(nativeObject, out var width, out var height));
			return new SizeF(width, height);
		}
	}

	/// <summary>Gets the pixel format for this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Imaging.PixelFormat" /> that represents the pixel format for this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	public PixelFormat PixelFormat
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImagePixelFormat(nativeObject, out var format));
			return format;
		}
	}

	/// <summary>Gets IDs of the property items stored in this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>An array of the property IDs, one for each property item stored in this image.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	public int[] PropertyIdList
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPropertyCount(nativeObject, out var propNumbers));
			int[] array = new int[propNumbers];
			GDIPlus.CheckStatus(GDIPlus.GdipGetPropertyIdList(nativeObject, propNumbers, array));
			return array;
		}
	}

	/// <summary>Gets all the property items (pieces of metadata) stored in this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>An array of <see cref="T:System.Drawing.Imaging.PropertyItem" /> objects, one for each property item stored in the image.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	public PropertyItem[] PropertyItems
	{
		get
		{
			GdipPropertyItem structure = default(GdipPropertyItem);
			GDIPlus.CheckStatus(GDIPlus.GdipGetPropertySize(nativeObject, out var bufferSize, out var propNumbers));
			PropertyItem[] array = new PropertyItem[propNumbers];
			if (propNumbers == 0)
			{
				return array;
			}
			IntPtr intPtr = Marshal.AllocHGlobal(bufferSize * propNumbers);
			try
			{
				GDIPlus.CheckStatus(GDIPlus.GdipGetAllPropertyItems(nativeObject, bufferSize, propNumbers, intPtr));
				int num = Marshal.SizeOf(structure);
				IntPtr ptr = intPtr;
				int num2 = 0;
				while (num2 < propNumbers)
				{
					structure = (GdipPropertyItem)Marshal.PtrToStructure(ptr, typeof(GdipPropertyItem));
					array[num2] = new PropertyItem();
					GdipPropertyItem.MarshalTo(structure, array[num2]);
					num2++;
					ptr = new IntPtr(ptr.ToInt64() + num);
				}
				return array;
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	/// <summary>Gets the file format of this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Imaging.ImageFormat" /> that represents the file format of this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public ImageFormat RawFormat
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImageRawFormat(nativeObject, out var format));
			return new ImageFormat(format);
		}
	}

	/// <summary>Gets the width and height, in pixels, of this image.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> structure that represents the width and height, in pixels, of this image.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Size Size => new Size(Width, Height);

	/// <summary>Gets or sets an object that provides additional data about the image.</summary>
	/// <returns>The <see cref="T:System.Object" /> that provides additional data about the image.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue(null)]
	[Localizable(false)]
	[Bindable(true)]
	[TypeConverter(typeof(StringConverter))]
	public object Tag
	{
		get
		{
			return tag;
		}
		set
		{
			tag = value;
		}
	}

	/// <summary>Gets the vertical resolution, in pixels per inch, of this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>The vertical resolution, in pixels per inch, of this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float VerticalResolution
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImageVerticalResolution(nativeObject, out var resolution));
			return resolution;
		}
	}

	/// <summary>Gets the width, in pixels, of this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>The width, in pixels, of this <see cref="T:System.Drawing.Image" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DefaultValue(false)]
	public int Width
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImageWidth(nativeObject, out var width));
			return (int)width;
		}
	}

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

	internal IntPtr nativeImage => nativeObject;

	internal Image()
	{
	}

	internal Image(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			if (string.Compare(current.Name, "Data", ignoreCase: true) != 0)
			{
				continue;
			}
			byte[] array = (byte[])current.Value;
			if (array != null)
			{
				MemoryStream memoryStream = new MemoryStream(array);
				nativeObject = InitFromStream(memoryStream);
				if (GDIPlus.RunningOnWindows())
				{
					stream = memoryStream;
				}
			}
		}
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.</summary>
	/// <param name="si">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
	/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
	{
		using MemoryStream memoryStream = new MemoryStream();
		if (RawFormat.Equals(ImageFormat.Icon))
		{
			Save(memoryStream, ImageFormat.Png);
		}
		else
		{
			Save(memoryStream, RawFormat);
		}
		si.AddValue("Data", memoryStream.ToArray());
	}

	/// <summary>Creates an <see cref="T:System.Drawing.Image" /> from the specified file.</summary>
	/// <returns>The <see cref="T:System.Drawing.Image" /> this method creates.</returns>
	/// <param name="filename">A string that contains the name of the file from which to create the <see cref="T:System.Drawing.Image" />. </param>
	/// <exception cref="T:System.OutOfMemoryException">The file does not have a valid image format.-or-GDI+ does not support the pixel format of the file.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified file does not exist.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="filename" /> is a <see cref="T:System.Uri" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Image FromFile(string filename)
	{
		return FromFile(filename, useEmbeddedColorManagement: false);
	}

	/// <summary>Creates an <see cref="T:System.Drawing.Image" /> from the specified file using embedded color management information in that file.</summary>
	/// <returns>The <see cref="T:System.Drawing.Image" /> this method creates.</returns>
	/// <param name="filename">A string that contains the name of the file from which to create the <see cref="T:System.Drawing.Image" />. </param>
	/// <param name="useEmbeddedColorManagement">Set to true to use color management information embedded in the image file; otherwise, false. </param>
	/// <exception cref="T:System.OutOfMemoryException">The file does not have a valid image format.-or-GDI+ does not support the pixel format of the file.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified file does not exist.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="filename" /> is a <see cref="T:System.Uri" />.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Image FromFile(string filename, bool useEmbeddedColorManagement)
	{
		if (!File.Exists(filename))
		{
			throw new FileNotFoundException(filename);
		}
		IntPtr image;
		Status status = ((!useEmbeddedColorManagement) ? GDIPlus.GdipLoadImageFromFile(filename, out image) : GDIPlus.GdipLoadImageFromFileICM(filename, out image));
		GDIPlus.CheckStatus(status);
		return CreateFromHandle(image);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Bitmap" /> from a handle to a GDI bitmap.</summary>
	/// <returns>The <see cref="T:System.Drawing.Bitmap" /> this method creates.</returns>
	/// <param name="hbitmap">The GDI bitmap handle from which to create the <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <filterpriority>1</filterpriority>
	public static Bitmap FromHbitmap(IntPtr hbitmap)
	{
		return FromHbitmap(hbitmap, IntPtr.Zero);
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Bitmap" /> from a handle to a GDI bitmap and a handle to a GDI palette.</summary>
	/// <returns>The <see cref="T:System.Drawing.Bitmap" /> this method creates.</returns>
	/// <param name="hbitmap">The GDI bitmap handle from which to create the <see cref="T:System.Drawing.Bitmap" />. </param>
	/// <param name="hpalette">A handle to a GDI palette used to define the bitmap colors if the bitmap specified in the <paramref name="hBitmap" /> parameter is not a device-independent bitmap (DIB). </param>
	/// <filterpriority>1</filterpriority>
	public static Bitmap FromHbitmap(IntPtr hbitmap, IntPtr hpalette)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateBitmapFromHBITMAP(hbitmap, hpalette, out var image));
		return new Bitmap(image);
	}

	/// <summary>Creates an <see cref="T:System.Drawing.Image" /> from the specified data stream.</summary>
	/// <returns>The <see cref="T:System.Drawing.Image" /> this method creates.</returns>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Image" />. </param>
	/// <exception cref="T:System.ArgumentException">The stream does not have a valid image format-or-<paramref name="stream" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static Image FromStream(Stream stream)
	{
		return LoadFromStream(stream, keepAlive: false);
	}

	/// <summary>Creates an <see cref="T:System.Drawing.Image" /> from the specified data stream, optionally using embedded color management information in that stream.</summary>
	/// <returns>The <see cref="T:System.Drawing.Image" /> this method creates.</returns>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Image" />. </param>
	/// <param name="useEmbeddedColorManagement">true to use color management information embedded in the data stream; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">The stream does not have a valid image format -or-<paramref name="stream" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoLimitation("useEmbeddedColorManagement  isn't supported.")]
	public static Image FromStream(Stream stream, bool useEmbeddedColorManagement)
	{
		return LoadFromStream(stream, keepAlive: false);
	}

	/// <summary>Creates an <see cref="T:System.Drawing.Image" /> from the specified data stream, optionally using embedded color management information and validating the image data.</summary>
	/// <returns>The <see cref="T:System.Drawing.Image" /> this method creates.</returns>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Image" />. </param>
	/// <param name="useEmbeddedColorManagement">true to use color management information embedded in the data stream; otherwise, false. </param>
	/// <param name="validateImageData">true to validate the image data; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentException">The stream does not have a valid image format.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoLimitation("useEmbeddedColorManagement  and validateImageData aren't supported.")]
	public static Image FromStream(Stream stream, bool useEmbeddedColorManagement, bool validateImageData)
	{
		return LoadFromStream(stream, keepAlive: false);
	}

	internal static Image LoadFromStream(Stream stream, bool keepAlive)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Image image = CreateFromHandle(InitFromStream(stream));
		if (keepAlive && GDIPlus.RunningOnWindows())
		{
			image.stream = stream;
		}
		return image;
	}

	internal static Image CreateImageObject(IntPtr nativeImage)
	{
		return CreateFromHandle(nativeImage);
	}

	internal static Image CreateFromHandle(IntPtr handle)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetImageType(handle, out var type));
		return type switch
		{
			ImageType.Bitmap => new Bitmap(handle), 
			ImageType.Metafile => new Metafile(handle), 
			_ => throw new NotSupportedException(global::Locale.GetText("Unknown image type.")), 
		};
	}

	/// <summary>Returns the color depth, in number of bits per pixel, of the specified pixel format.</summary>
	/// <returns>The color depth of the specified pixel format.</returns>
	/// <param name="pixfmt">The <see cref="T:System.Drawing.Imaging.PixelFormat" /> member that specifies the format for which to find the size. </param>
	/// <filterpriority>1</filterpriority>
	public static int GetPixelFormatSize(PixelFormat pixfmt)
	{
		int result = 0;
		switch (pixfmt)
		{
		case PixelFormat.Format16bppRgb555:
		case PixelFormat.Format16bppRgb565:
		case PixelFormat.Format16bppArgb1555:
		case PixelFormat.Format16bppGrayScale:
			result = 16;
			break;
		case PixelFormat.Format1bppIndexed:
			result = 1;
			break;
		case PixelFormat.Format24bppRgb:
			result = 24;
			break;
		case PixelFormat.Format32bppRgb:
		case PixelFormat.Format32bppPArgb:
		case PixelFormat.Format32bppArgb:
			result = 32;
			break;
		case PixelFormat.Format48bppRgb:
			result = 48;
			break;
		case PixelFormat.Format4bppIndexed:
			result = 4;
			break;
		case PixelFormat.Format64bppPArgb:
		case PixelFormat.Format64bppArgb:
			result = 64;
			break;
		case PixelFormat.Format8bppIndexed:
			result = 8;
			break;
		}
		return result;
	}

	/// <summary>Returns a value that indicates whether the pixel format for this <see cref="T:System.Drawing.Image" /> contains alpha information.</summary>
	/// <returns>true if <paramref name="pixfmt" /> contains alpha information; otherwise, false.</returns>
	/// <param name="pixfmt">The <see cref="T:System.Drawing.Imaging.PixelFormat" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsAlphaPixelFormat(PixelFormat pixfmt)
	{
		bool result = false;
		switch (pixfmt)
		{
		case PixelFormat.Format16bppArgb1555:
		case PixelFormat.Format32bppPArgb:
		case PixelFormat.Format64bppPArgb:
		case PixelFormat.Format32bppArgb:
		case PixelFormat.Format64bppArgb:
			result = true;
			break;
		case PixelFormat.Format16bppRgb555:
		case PixelFormat.Format16bppRgb565:
		case PixelFormat.Format24bppRgb:
		case PixelFormat.Format32bppRgb:
		case PixelFormat.Format1bppIndexed:
		case PixelFormat.Format4bppIndexed:
		case PixelFormat.Format8bppIndexed:
		case PixelFormat.Format16bppGrayScale:
		case PixelFormat.Format48bppRgb:
			result = false;
			break;
		}
		return result;
	}

	/// <summary>Returns a value that indicates whether the pixel format is 32 bits per pixel.</summary>
	/// <returns>true if <paramref name="pixfmt" /> is canonical; otherwise, false.</returns>
	/// <param name="pixfmt">The <see cref="T:System.Drawing.Imaging.PixelFormat" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsCanonicalPixelFormat(PixelFormat pixfmt)
	{
		return (pixfmt & PixelFormat.Canonical) != 0;
	}

	/// <summary>Returns a value that indicates whether the pixel format is 64 bits per pixel.</summary>
	/// <returns>true if <paramref name="pixfmt" /> is extended; otherwise, false.</returns>
	/// <param name="pixfmt">The <see cref="T:System.Drawing.Imaging.PixelFormat" /> enumeration to test. </param>
	/// <filterpriority>1</filterpriority>
	public static bool IsExtendedPixelFormat(PixelFormat pixfmt)
	{
		return (pixfmt & PixelFormat.Extended) != 0;
	}

	internal static IntPtr InitFromStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentException("stream");
		}
		if (!stream.CanSeek)
		{
			byte[] array = new byte[256];
			int num = 0;
			int num2;
			do
			{
				if (array.Length < num + 256)
				{
					byte[] array2 = new byte[array.Length * 2];
					Array.Copy(array, array2, array.Length);
					array = array2;
				}
				num2 = stream.Read(array, num, 256);
				num += num2;
			}
			while (num2 != 0);
			stream = new MemoryStream(array, 0, num);
		}
		Status status;
		IntPtr image;
		if (GDIPlus.RunningOnUnix())
		{
			GDIPlus.GdiPlusStreamHelper gdiPlusStreamHelper = new GDIPlus.GdiPlusStreamHelper(stream, seekToOrigin: true);
			status = GDIPlus.GdipLoadImageFromDelegate_linux(gdiPlusStreamHelper.GetHeaderDelegate, gdiPlusStreamHelper.GetBytesDelegate, gdiPlusStreamHelper.PutBytesDelegate, gdiPlusStreamHelper.SeekDelegate, gdiPlusStreamHelper.CloseDelegate, gdiPlusStreamHelper.SizeDelegate, out image);
		}
		else
		{
			status = GDIPlus.GdipLoadImageFromStream(new ComIStreamWrapper(stream), out image);
		}
		if (status != Status.Ok)
		{
			return IntPtr.Zero;
		}
		return image;
	}

	/// <summary>Gets the bounds of the image in the specified unit.</summary>
	/// <returns>The <see cref="T:System.Drawing.RectangleF" /> that represents the bounds of the image, in the specified unit.</returns>
	/// <param name="pageUnit">One of the <see cref="T:System.Drawing.GraphicsUnit" /> values indicating the unit of measure for the bounding rectangle.</param>
	/// <filterpriority>1</filterpriority>
	public RectangleF GetBounds(ref GraphicsUnit pageUnit)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetImageBounds(nativeObject, out var source, ref pageUnit));
		return source;
	}

	/// <summary>Returns information about the parameters supported by the specified image encoder.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.EncoderParameters" /> that contains an array of <see cref="T:System.Drawing.Imaging.EncoderParameter" /> objects. Each <see cref="T:System.Drawing.Imaging.EncoderParameter" /> contains information about one of the parameters supported by the specified image encoder.</returns>
	/// <param name="encoder">A GUID that specifies the image encoder. </param>
	/// <filterpriority>1</filterpriority>
	public EncoderParameters GetEncoderParameterList(Guid encoder)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetEncoderParameterListSize(nativeObject, ref encoder, out var size));
		IntPtr intPtr = Marshal.AllocHGlobal((int)size);
		try
		{
			Status status = GDIPlus.GdipGetEncoderParameterList(nativeObject, ref encoder, size, intPtr);
			EncoderParameters result = EncoderParameters.ConvertFromMemory(intPtr);
			GDIPlus.CheckStatus(status);
			return result;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Returns the number of frames of the specified dimension.</summary>
	/// <returns>The number of frames in the specified dimension.</returns>
	/// <param name="dimension">A <see cref="T:System.Drawing.Imaging.FrameDimension" /> that specifies the identity of the dimension type. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int GetFrameCount(FrameDimension dimension)
	{
		Guid guidDimension = dimension.Guid;
		GDIPlus.CheckStatus(GDIPlus.GdipImageGetFrameCount(nativeObject, ref guidDimension, out var count));
		return (int)count;
	}

	/// <summary>Gets the specified property item from this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Imaging.PropertyItem" /> this method gets.</returns>
	/// <param name="propid">The ID of the property item to get. </param>
	/// <exception cref="T:System.ArgumentException">The image format of this image does not support property items.</exception>
	/// <filterpriority>1</filterpriority>
	public PropertyItem GetPropertyItem(int propid)
	{
		PropertyItem propertyItem = new PropertyItem();
		GDIPlus.CheckStatus(GDIPlus.GdipGetPropertyItemSize(nativeObject, propid, out var propertySize));
		IntPtr intPtr = Marshal.AllocHGlobal(propertySize);
		try
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPropertyItem(nativeObject, propid, propertySize, intPtr));
			GdipPropertyItem.MarshalTo((GdipPropertyItem)Marshal.PtrToStructure(intPtr, typeof(GdipPropertyItem)), propertyItem);
			return propertyItem;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Returns a thumbnail for this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>An <see cref="T:System.Drawing.Image" /> that represents the thumbnail.</returns>
	/// <param name="thumbWidth">The width, in pixels, of the requested thumbnail image. </param>
	/// <param name="thumbHeight">The height, in pixels, of the requested thumbnail image. </param>
	/// <param name="callback">A <see cref="T:System.Drawing.Image.GetThumbnailImageAbort" /> delegate. Note   You must create a delegate and pass a reference to the delegate as the <paramref name="callback" /> parameter, but the delegate is not used.</param>
	/// <param name="callbackData">Must be <see cref="F:System.IntPtr.Zero" />. </param>
	/// <filterpriority>1</filterpriority>
	public Image GetThumbnailImage(int thumbWidth, int thumbHeight, GetThumbnailImageAbort callback, IntPtr callbackData)
	{
		if (thumbWidth <= 0 || thumbHeight <= 0)
		{
			throw new OutOfMemoryException("Invalid thumbnail size");
		}
		Image image = new Bitmap(thumbWidth, thumbHeight);
		using Graphics graphics = Graphics.FromImage(image);
		GDIPlus.CheckStatus(GDIPlus.GdipDrawImageRectRectI(graphics.nativeObject, nativeObject, 0, 0, thumbWidth, thumbHeight, 0, 0, Width, Height, GraphicsUnit.Pixel, IntPtr.Zero, null, IntPtr.Zero));
		return image;
	}

	/// <summary>Removes the specified property item from this <see cref="T:System.Drawing.Image" />.</summary>
	/// <param name="propid">The ID of the property item to remove. </param>
	/// <exception cref="T:System.ArgumentException">The image does not contain the requested property item.-or-The image format for this image does not support property items.</exception>
	/// <filterpriority>1</filterpriority>
	public void RemovePropertyItem(int propid)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRemovePropertyItem(nativeObject, propid));
	}

	/// <summary>Rotates, flips, or rotates and flips the <see cref="T:System.Drawing.Image" />.</summary>
	/// <param name="rotateFlipType">A <see cref="T:System.Drawing.RotateFlipType" /> member that specifies the type of rotation and flip to apply to the image. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void RotateFlip(RotateFlipType rotateFlipType)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipImageRotateFlip(nativeObject, rotateFlipType));
	}

	internal ImageCodecInfo findEncoderForFormat(ImageFormat format)
	{
		ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
		ImageCodecInfo result = null;
		if (format.Guid.Equals(ImageFormat.MemoryBmp.Guid))
		{
			format = ImageFormat.Png;
		}
		for (int i = 0; i < imageEncoders.Length; i++)
		{
			if (imageEncoders[i].FormatID.Equals(format.Guid))
			{
				result = imageEncoders[i];
				break;
			}
		}
		return result;
	}

	/// <summary>Saves this <see cref="T:System.Drawing.Image" /> to the specified file or stream.</summary>
	/// <param name="filename">A string that contains the name of the file to which to save this <see cref="T:System.Drawing.Image" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="filename" /> is null.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The image was saved with the wrong image format.-or- The image was saved to the same file it was created from.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Save(string filename)
	{
		Save(filename, RawFormat);
	}

	/// <summary>Saves this <see cref="T:System.Drawing.Image" /> to the specified file in the specified format.</summary>
	/// <param name="filename">A string that contains the name of the file to which to save this <see cref="T:System.Drawing.Image" />. </param>
	/// <param name="format">The <see cref="T:System.Drawing.Imaging.ImageFormat" /> for this <see cref="T:System.Drawing.Image" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="filename" /> or <paramref name="format" /> is null.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The image was saved with the wrong image format.-or- The image was saved to the same file it was created from.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Save(string filename, ImageFormat format)
	{
		ImageCodecInfo imageCodecInfo = findEncoderForFormat(format);
		if (imageCodecInfo == null)
		{
			imageCodecInfo = findEncoderForFormat(RawFormat);
			if (imageCodecInfo == null)
			{
				throw new ArgumentException(global::Locale.GetText("No codec available for saving format '{0}'.", format.Guid), "format");
			}
		}
		Save(filename, imageCodecInfo, null);
	}

	/// <summary>Saves this <see cref="T:System.Drawing.Image" /> to the specified file, with the specified encoder and image-encoder parameters.</summary>
	/// <param name="filename">A string that contains the name of the file to which to save this <see cref="T:System.Drawing.Image" />. </param>
	/// <param name="encoder">The <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> for this <see cref="T:System.Drawing.Image" />. </param>
	/// <param name="encoderParams">An <see cref="T:System.Drawing.Imaging.EncoderParameters" /> to use for this <see cref="T:System.Drawing.Image" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="filename" /> or <paramref name="encoder" /> is null.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The image was saved with the wrong image format.-or- The image was saved to the same file it was created from.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		Guid encoderClsID = encoder.Clsid;
		Status status;
		if (encoderParams == null)
		{
			status = GDIPlus.GdipSaveImageToFile(nativeObject, filename, ref encoderClsID, IntPtr.Zero);
		}
		else
		{
			IntPtr intPtr = encoderParams.ConvertToMemory();
			status = GDIPlus.GdipSaveImageToFile(nativeObject, filename, ref encoderClsID, intPtr);
			Marshal.FreeHGlobal(intPtr);
		}
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Saves this image to the specified stream in the specified format.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> where the image will be saved. </param>
	/// <param name="format">An <see cref="T:System.Drawing.Imaging.ImageFormat" /> that specifies the format of the saved image. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> or <paramref name="format" /> is null.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The image was saved with the wrong image format</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Save(Stream stream, ImageFormat format)
	{
		ImageCodecInfo imageCodecInfo = findEncoderForFormat(format);
		if (imageCodecInfo == null)
		{
			throw new ArgumentException("No codec available for format:" + format.Guid.ToString());
		}
		Save(stream, imageCodecInfo, null);
	}

	/// <summary>Saves this image to the specified stream, with the specified encoder and image encoder parameters.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> where the image will be saved. </param>
	/// <param name="encoder">The <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> for this <see cref="T:System.Drawing.Image" />.</param>
	/// <param name="encoderParams">An <see cref="T:System.Drawing.Imaging.EncoderParameters" /> that specifies parameters used by the image encoder. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.ExternalException">The image was saved with the wrong image format.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		Guid clsidEncoder = encoder.Clsid;
		IntPtr intPtr = encoderParams?.ConvertToMemory() ?? IntPtr.Zero;
		Status status;
		try
		{
			if (GDIPlus.RunningOnUnix())
			{
				GDIPlus.GdiPlusStreamHelper gdiPlusStreamHelper = new GDIPlus.GdiPlusStreamHelper(stream, seekToOrigin: false);
				status = GDIPlus.GdipSaveImageToDelegate_linux(nativeObject, gdiPlusStreamHelper.GetBytesDelegate, gdiPlusStreamHelper.PutBytesDelegate, gdiPlusStreamHelper.SeekDelegate, gdiPlusStreamHelper.CloseDelegate, gdiPlusStreamHelper.SizeDelegate, ref clsidEncoder, intPtr);
			}
			else
			{
				status = GDIPlus.GdipSaveImageToStream(new HandleRef(this, nativeObject), new ComIStreamWrapper(stream), ref clsidEncoder, new HandleRef(encoderParams, intPtr));
			}
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Adds a frame to the file or stream specified in a previous call to the <see cref="Overload:System.Drawing.Image.Save" /> method. Use this method to save selected frames from a multiple-frame image to another multiple-frame image.</summary>
	/// <param name="encoderParams">An <see cref="T:System.Drawing.Imaging.EncoderParameters" /> that holds parameters required by the image encoder that is used by the save-add operation. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SaveAdd(EncoderParameters encoderParams)
	{
		IntPtr intPtr = encoderParams.ConvertToMemory();
		Status status = GDIPlus.GdipSaveAdd(nativeObject, intPtr);
		Marshal.FreeHGlobal(intPtr);
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Adds a frame to the file or stream specified in a previous call to the <see cref="Overload:System.Drawing.Image.Save" /> method.</summary>
	/// <param name="image">An <see cref="T:System.Drawing.Image" /> that contains the frame to add. </param>
	/// <param name="encoderParams">An <see cref="T:System.Drawing.Imaging.EncoderParameters" /> that holds parameters required by the image encoder that is used by the save-add operation. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SaveAdd(Image image, EncoderParameters encoderParams)
	{
		IntPtr intPtr = encoderParams.ConvertToMemory();
		Status status = GDIPlus.GdipSaveAddImage(nativeObject, image.NativeObject, intPtr);
		Marshal.FreeHGlobal(intPtr);
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Selects the frame specified by the dimension and index.</summary>
	/// <returns>Always returns 0.</returns>
	/// <param name="dimension">A <see cref="T:System.Drawing.Imaging.FrameDimension" /> that specifies the identity of the dimension type. </param>
	/// <param name="frameIndex">The index of the active frame. </param>
	/// <filterpriority>1</filterpriority>
	public int SelectActiveFrame(FrameDimension dimension, int frameIndex)
	{
		Guid guidDimension = dimension.Guid;
		GDIPlus.CheckStatus(GDIPlus.GdipImageSelectActiveFrame(nativeObject, ref guidDimension, frameIndex));
		return frameIndex;
	}

	/// <summary>Stores a property item (piece of metadata) in this <see cref="T:System.Drawing.Image" />.</summary>
	/// <param name="propitem">The <see cref="T:System.Drawing.Imaging.PropertyItem" /> to be stored. </param>
	/// <exception cref="T:System.ArgumentException">The image format of this image does not support property items.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public unsafe void SetPropertyItem(PropertyItem propitem)
	{
		if (propitem == null)
		{
			throw new ArgumentNullException("propitem");
		}
		int num = Marshal.SizeOf(propitem.Value[0]) * propitem.Value.Length;
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		try
		{
			GdipPropertyItem gdipPropertyItem = new GdipPropertyItem
			{
				id = propitem.Id,
				len = propitem.Len,
				type = propitem.Type
			};
			Marshal.Copy(propitem.Value, 0, intPtr, num);
			gdipPropertyItem.value = intPtr;
			GDIPlus.CheckStatus(GDIPlus.GdipSetPropertyItem(nativeObject, &gdipPropertyItem));
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	internal ColorPalette retrieveGDIPalette()
	{
		ColorPalette colorPalette = new ColorPalette();
		GDIPlus.CheckStatus(GDIPlus.GdipGetImagePaletteSize(nativeObject, out var size));
		IntPtr intPtr = Marshal.AllocHGlobal(size);
		try
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetImagePalette(nativeObject, intPtr, size));
			colorPalette.ConvertFromMemory(intPtr);
			return colorPalette;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	internal void storeGDIPalette(ColorPalette palette)
	{
		if (palette == null)
		{
			throw new ArgumentNullException("palette");
		}
		IntPtr intPtr = palette.ConvertToMemory();
		if (intPtr == IntPtr.Zero)
		{
			return;
		}
		try
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetImagePalette(nativeObject, intPtr));
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Image" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~Image()
	{
		Dispose(disposing: false);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Drawing.Image" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (GDIPlus.GdiPlusToken != 0L && nativeObject != IntPtr.Zero)
		{
			Status status = GDIPlus.GdipDisposeImage(nativeObject);
			if (stream != null)
			{
				stream.Dispose();
				stream = null;
			}
			nativeObject = IntPtr.Zero;
			GDIPlus.CheckStatus(status);
		}
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Image" /> this method creates, cast as an object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public object Clone()
	{
		if (GDIPlus.RunningOnWindows() && stream != null)
		{
			return CloneFromStream();
		}
		IntPtr imageclone = IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipCloneImage(NativeObject, out imageclone));
		if (this is Bitmap)
		{
			return new Bitmap(imageclone);
		}
		return new Metafile(imageclone);
	}

	private object CloneFromStream()
	{
		MemoryStream memoryStream = new MemoryStream(new byte[stream.Length]);
		int num = (int)((stream.Length < 4096) ? stream.Length : 4096);
		byte[] buffer = new byte[num];
		stream.Position = 0L;
		do
		{
			num = stream.Read(buffer, 0, num);
			memoryStream.Write(buffer, 0, num);
		}
		while (num == 4096);
		IntPtr zero = IntPtr.Zero;
		zero = InitFromStream(memoryStream);
		if (this is Bitmap)
		{
			return new Bitmap(zero, memoryStream);
		}
		return new Metafile(zero, memoryStream);
	}
}
