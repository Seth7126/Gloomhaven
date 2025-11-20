namespace System.Drawing.Imaging;

/// <summary>Provides attributes of an image encoder/decoder (codec).</summary>
[Flags]
public enum ImageCodecFlags
{
	/// <summary>The codec supports encoding (saving).</summary>
	Encoder = 1,
	/// <summary>The codec supports decoding (reading).</summary>
	Decoder = 2,
	/// <summary>The codec supports raster images (bitmaps).</summary>
	SupportBitmap = 4,
	/// <summary>The codec supports vector images (metafiles).</summary>
	SupportVector = 8,
	/// <summary>The encoder requires a seekable output stream.</summary>
	SeekableEncode = 0x10,
	/// <summary>The decoder has blocking behavior during the decoding process.</summary>
	BlockingDecode = 0x20,
	/// <summary>The codec is built into GDI+.</summary>
	Builtin = 0x10000,
	/// <summary>Not used.</summary>
	System = 0x20000,
	/// <summary>Not used.</summary>
	User = 0x40000
}
