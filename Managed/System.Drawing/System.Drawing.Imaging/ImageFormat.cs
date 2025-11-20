using System.ComponentModel;

namespace System.Drawing.Imaging;

/// <summary>Specifies the file format of the image. Not inheritable.</summary>
[TypeConverter(typeof(ImageFormatConverter))]
public sealed class ImageFormat
{
	private Guid guid;

	private string name;

	private const string BmpGuid = "b96b3cab-0728-11d3-9d7b-0000f81ef32e";

	private const string EmfGuid = "b96b3cac-0728-11d3-9d7b-0000f81ef32e";

	private const string ExifGuid = "b96b3cb2-0728-11d3-9d7b-0000f81ef32e";

	private const string GifGuid = "b96b3cb0-0728-11d3-9d7b-0000f81ef32e";

	private const string TiffGuid = "b96b3cb1-0728-11d3-9d7b-0000f81ef32e";

	private const string PngGuid = "b96b3caf-0728-11d3-9d7b-0000f81ef32e";

	private const string MemoryBmpGuid = "b96b3caa-0728-11d3-9d7b-0000f81ef32e";

	private const string IconGuid = "b96b3cb5-0728-11d3-9d7b-0000f81ef32e";

	private const string JpegGuid = "b96b3cae-0728-11d3-9d7b-0000f81ef32e";

	private const string WmfGuid = "b96b3cad-0728-11d3-9d7b-0000f81ef32e";

	private static object locker = new object();

	private static ImageFormat BmpImageFormat;

	private static ImageFormat EmfImageFormat;

	private static ImageFormat ExifImageFormat;

	private static ImageFormat GifImageFormat;

	private static ImageFormat TiffImageFormat;

	private static ImageFormat PngImageFormat;

	private static ImageFormat MemoryBmpImageFormat;

	private static ImageFormat IconImageFormat;

	private static ImageFormat JpegImageFormat;

	private static ImageFormat WmfImageFormat;

	/// <summary>Gets a <see cref="T:System.Guid" /> structure that represents this <see cref="T:System.Drawing.Imaging.ImageFormat" /> object.</summary>
	/// <returns>A <see cref="T:System.Guid" /> structure that represents this <see cref="T:System.Drawing.Imaging.ImageFormat" /> object.</returns>
	public Guid Guid => guid;

	/// <summary>Gets the bitmap (BMP) image format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the bitmap image format.</returns>
	public static ImageFormat Bmp
	{
		get
		{
			lock (locker)
			{
				if (BmpImageFormat == null)
				{
					BmpImageFormat = new ImageFormat("Bmp", "b96b3cab-0728-11d3-9d7b-0000f81ef32e");
				}
				return BmpImageFormat;
			}
		}
	}

	/// <summary>Gets the enhanced metafile (EMF) image format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the enhanced metafile image format.</returns>
	public static ImageFormat Emf
	{
		get
		{
			lock (locker)
			{
				if (EmfImageFormat == null)
				{
					EmfImageFormat = new ImageFormat("Emf", "b96b3cac-0728-11d3-9d7b-0000f81ef32e");
				}
				return EmfImageFormat;
			}
		}
	}

	/// <summary>Gets the Exchangeable Image File (Exif) format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the Exif format.</returns>
	public static ImageFormat Exif
	{
		get
		{
			lock (locker)
			{
				if (ExifImageFormat == null)
				{
					ExifImageFormat = new ImageFormat("Exif", "b96b3cb2-0728-11d3-9d7b-0000f81ef32e");
				}
				return ExifImageFormat;
			}
		}
	}

	/// <summary>Gets the Graphics Interchange Format (GIF) image format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the GIF image format.</returns>
	public static ImageFormat Gif
	{
		get
		{
			lock (locker)
			{
				if (GifImageFormat == null)
				{
					GifImageFormat = new ImageFormat("Gif", "b96b3cb0-0728-11d3-9d7b-0000f81ef32e");
				}
				return GifImageFormat;
			}
		}
	}

	/// <summary>Gets the Windows icon image format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the Windows icon image format.</returns>
	public static ImageFormat Icon
	{
		get
		{
			lock (locker)
			{
				if (IconImageFormat == null)
				{
					IconImageFormat = new ImageFormat("Icon", "b96b3cb5-0728-11d3-9d7b-0000f81ef32e");
				}
				return IconImageFormat;
			}
		}
	}

	/// <summary>Gets the Joint Photographic Experts Group (JPEG) image format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the JPEG image format.</returns>
	public static ImageFormat Jpeg
	{
		get
		{
			lock (locker)
			{
				if (JpegImageFormat == null)
				{
					JpegImageFormat = new ImageFormat("Jpeg", "b96b3cae-0728-11d3-9d7b-0000f81ef32e");
				}
				return JpegImageFormat;
			}
		}
	}

	/// <summary>Gets the format of a bitmap in memory.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the format of a bitmap in memory.</returns>
	public static ImageFormat MemoryBmp
	{
		get
		{
			lock (locker)
			{
				if (MemoryBmpImageFormat == null)
				{
					MemoryBmpImageFormat = new ImageFormat("MemoryBMP", "b96b3caa-0728-11d3-9d7b-0000f81ef32e");
				}
				return MemoryBmpImageFormat;
			}
		}
	}

	/// <summary>Gets the W3C Portable Network Graphics (PNG) image format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the PNG image format.</returns>
	public static ImageFormat Png
	{
		get
		{
			lock (locker)
			{
				if (PngImageFormat == null)
				{
					PngImageFormat = new ImageFormat("Png", "b96b3caf-0728-11d3-9d7b-0000f81ef32e");
				}
				return PngImageFormat;
			}
		}
	}

	/// <summary>Gets the Tagged Image File Format (TIFF) image format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the TIFF image format.</returns>
	public static ImageFormat Tiff
	{
		get
		{
			lock (locker)
			{
				if (TiffImageFormat == null)
				{
					TiffImageFormat = new ImageFormat("Tiff", "b96b3cb1-0728-11d3-9d7b-0000f81ef32e");
				}
				return TiffImageFormat;
			}
		}
	}

	/// <summary>Gets the Windows metafile (WMF) image format.</summary>
	/// <returns>An <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that indicates the Windows metafile image format.</returns>
	public static ImageFormat Wmf
	{
		get
		{
			lock (locker)
			{
				if (WmfImageFormat == null)
				{
					WmfImageFormat = new ImageFormat("Wmf", "b96b3cad-0728-11d3-9d7b-0000f81ef32e");
				}
				return WmfImageFormat;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.ImageFormat" /> class by using the specified <see cref="T:System.Guid" /> structure.</summary>
	/// <param name="guid">The <see cref="T:System.Guid" /> structure that specifies a particular image format. </param>
	public ImageFormat(Guid guid)
	{
		this.guid = guid;
	}

	private ImageFormat(string name, string guid)
	{
		this.name = name;
		this.guid = new Guid(guid);
	}

	/// <summary>Returns a value that indicates whether the specified object is an <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that is equivalent to this <see cref="T:System.Drawing.Imaging.ImageFormat" /> object.</summary>
	/// <returns>true if <paramref name="o" /> is an <see cref="T:System.Drawing.Imaging.ImageFormat" /> object that is equivalent to this <see cref="T:System.Drawing.Imaging.ImageFormat" /> object; otherwise, false.</returns>
	/// <param name="o">The object to test. </param>
	public override bool Equals(object o)
	{
		if (!(o is ImageFormat { Guid: var guid }))
		{
			return false;
		}
		return guid.Equals(this.guid);
	}

	/// <summary>Returns a hash code value that represents this object.</summary>
	/// <returns>A hash code that represents this object.</returns>
	public override int GetHashCode()
	{
		return guid.GetHashCode();
	}

	/// <summary>Converts this <see cref="T:System.Drawing.Imaging.ImageFormat" /> object to a human-readable string.</summary>
	/// <returns>A string that represents this <see cref="T:System.Drawing.Imaging.ImageFormat" /> object.</returns>
	public override string ToString()
	{
		if (name != null)
		{
			return name;
		}
		return "[ImageFormat: " + guid.ToString() + "]";
	}
}
