using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging;

/// <summary>Defines a graphic metafile. A metafile contains records that describe a sequence of graphics operations that can be recorded (constructed) and played back (displayed). This class is not inheritable.</summary>
[Serializable]
[Editor("System.Drawing.Design.MetafileEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
[System.MonoTODO("Metafiles, both WMF and EMF formats, are only partially supported.")]
public sealed class Metafile : Image
{
	internal sealed class MetafileHolder : IDisposable
	{
		private bool _disposed;

		private IntPtr _nativeImage;

		internal bool Disposed => _disposed;

		internal MetafileHolder()
		{
			_disposed = false;
			_nativeImage = IntPtr.Zero;
		}

		~MetafileHolder()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		internal void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				IntPtr nativeImage = _nativeImage;
				_nativeImage = IntPtr.Zero;
				_disposed = true;
				if (nativeImage != IntPtr.Zero)
				{
					GDIPlus.CheckStatus(GDIPlus.GdipDisposeImage(nativeImage));
				}
			}
		}

		internal void MetafileDisposed(IntPtr nativeImage)
		{
			_nativeImage = nativeImage;
		}

		internal void GraphicsDisposed()
		{
			Dispose();
		}
	}

	private MetafileHolder _metafileHolder;

	internal MetafileHolder AddMetafileHolder()
	{
		if (_metafileHolder != null && !_metafileHolder.Disposed)
		{
			return null;
		}
		_metafileHolder = new MetafileHolder();
		return _metafileHolder;
	}

	internal Metafile(IntPtr ptr)
	{
		nativeObject = ptr;
	}

	internal Metafile(IntPtr ptr, Stream stream)
	{
		if (GDIPlus.RunningOnWindows())
		{
			base.stream = stream;
		}
		nativeObject = ptr;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> from which to create the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="stream" /> is null.</exception>
	public Metafile(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentException("stream");
		}
		Status status;
		if (GDIPlus.RunningOnUnix())
		{
			GDIPlus.GdiPlusStreamHelper gdiPlusStreamHelper = new GDIPlus.GdiPlusStreamHelper(stream, seekToOrigin: false);
			status = GDIPlus.GdipCreateMetafileFromDelegate_linux(gdiPlusStreamHelper.GetHeaderDelegate, gdiPlusStreamHelper.GetBytesDelegate, gdiPlusStreamHelper.PutBytesDelegate, gdiPlusStreamHelper.SeekDelegate, gdiPlusStreamHelper.CloseDelegate, gdiPlusStreamHelper.SizeDelegate, out nativeObject);
		}
		else
		{
			status = GDIPlus.GdipCreateMetafileFromStream(new ComIStreamWrapper(stream), out nativeObject);
		}
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified file name.</summary>
	/// <param name="filename">A <see cref="T:System.String" /> that represents the file name from which to create the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string filename)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		if (filename.Length == 0)
		{
			throw new ArgumentException("filename");
		}
		Status num = GDIPlus.GdipCreateMetafileFromFile(filename, out nativeObject);
		if (num == Status.GenericError)
		{
			throw new ExternalException("Couldn't load specified file.");
		}
		GDIPlus.CheckStatus(num);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified handle.</summary>
	/// <param name="henhmetafile">A handle to an enhanced metafile. </param>
	/// <param name="deleteEmf">true to delete the enhanced metafile handle when the <see cref="T:System.Drawing.Imaging.Metafile" /> is deleted; otherwise, false. </param>
	public Metafile(IntPtr henhmetafile, bool deleteEmf)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateMetafileFromEmf(henhmetafile, deleteEmf, out nativeObject));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified handle to a device context and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="emfType">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(IntPtr referenceHdc, EmfType emfType)
		: this(referenceHdc, default(RectangleF), MetafileFrameUnit.GdiCompatible, emfType, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified device context, bounded by the specified rectangle.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(IntPtr referenceHdc, Rectangle frameRect)
		: this(referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified device context, bounded by the specified rectangle.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(IntPtr referenceHdc, RectangleF frameRect)
		: this(referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified handle and a <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" />.</summary>
	/// <param name="hmetafile">A windows handle to a <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="wmfHeader">A <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" />. </param>
	public Metafile(IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateMetafileFromEmf(hmetafile, deleteEmf: false, out nativeObject));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	public Metafile(Stream stream, IntPtr referenceHdc)
		: this(stream, referenceHdc, default(RectangleF), MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	public Metafile(string fileName, IntPtr referenceHdc)
		: this(fileName, referenceHdc, default(RectangleF), MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified handle to a device context and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. A string can be supplied to name the file.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="emfType">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="description">A descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(IntPtr referenceHdc, EmfType emfType, string description)
		: this(referenceHdc, default(RectangleF), MetafileFrameUnit.GdiCompatible, emfType, description)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified device context, bounded by the specified rectangle that uses the supplied unit of measure.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	public Metafile(IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit)
		: this(referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified device context, bounded by the specified rectangle that uses the supplied unit of measure.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	public Metafile(IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit)
		: this(referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified handle and a <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" />. Also, the <paramref name="deleteWmf" /> parameter can be used to delete the handle when the metafile is deleted.</summary>
	/// <param name="hmetafile">A windows handle to a <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="wmfHeader">A <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" />. </param>
	/// <param name="deleteWmf">true to delete the handle to the new <see cref="T:System.Drawing.Imaging.Metafile" /> when the <see cref="T:System.Drawing.Imaging.Metafile" /> is deleted; otherwise, false. </param>
	public Metafile(IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader, bool deleteWmf)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateMetafileFromEmf(hmetafile, deleteWmf, out nativeObject));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, EmfType type)
		: this(stream, referenceHdc, default(RectangleF), MetafileFrameUnit.GdiCompatible, type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, and a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, Rectangle frameRect)
		: this(stream, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, and a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, RectangleF frameRect)
		: this(stream, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, EmfType type)
		: this(fileName, referenceHdc, default(RectangleF), MetafileFrameUnit.GdiCompatible, type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, and a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect)
		: this(fileName, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, and a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect)
		: this(fileName, referenceHdc, frameRect, MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified device context, bounded by the specified rectangle that uses the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type)
		: this(referenceHdc, frameRect, frameUnit, type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified device context, bounded by the specified rectangle that uses the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type)
		: this(referenceHdc, frameRect, frameUnit, type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. Also, a string that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" /> can be added.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="description">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, EmfType type, string description)
		: this(stream, referenceHdc, default(RectangleF), MetafileFrameUnit.GdiCompatible, type, description)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, and the supplied unit of measure.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit)
		: this(stream, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, and the supplied unit of measure.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit)
		: this(stream, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. A descriptive string can be added, as well.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="description">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, EmfType type, string description)
		: this(fileName, referenceHdc, default(RectangleF), MetafileFrameUnit.GdiCompatible, type, description)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, and the supplied unit of measure.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit)
		: this(fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, and the supplied unit of measure.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit)
		: this(fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified device context, bounded by the specified rectangle that uses the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. A string can be provided to name the file.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="desc">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string desc)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRecordMetafileI(referenceHdc, type, ref frameRect, frameUnit, desc, out nativeObject));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified device context, bounded by the specified rectangle that uses the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. A string can be provided to name the file.</summary>
	/// <param name="referenceHdc">The handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="description">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRecordMetafile(referenceHdc, type, ref frameRect, frameUnit, description, out nativeObject));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type)
		: this(stream, referenceHdc, frameRect, frameUnit, type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type)
		: this(stream, referenceHdc, frameRect, frameUnit, type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type)
		: this(fileName, referenceHdc, frameRect, frameUnit, type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, and the supplied unit of measure. A descriptive string can also be added.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="description">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, string description)
		: this(fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, description)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type)
		: this(fileName, referenceHdc, frameRect, frameUnit, type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, and the supplied unit of measure. A descriptive string can also be added.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="desc">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, string desc)
		: this(fileName, referenceHdc, frameRect, frameUnit, EmfType.EmfPlusDual, desc)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. A string that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" /> can be added.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="description">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
	{
		if (stream == null)
		{
			throw new NullReferenceException("stream");
		}
		Status status = Status.NotImplemented;
		if (GDIPlus.RunningOnUnix())
		{
			GDIPlus.GdiPlusStreamHelper gdiPlusStreamHelper = new GDIPlus.GdiPlusStreamHelper(stream, seekToOrigin: false);
			status = GDIPlus.GdipRecordMetafileFromDelegateI_linux(gdiPlusStreamHelper.GetHeaderDelegate, gdiPlusStreamHelper.GetBytesDelegate, gdiPlusStreamHelper.PutBytesDelegate, gdiPlusStreamHelper.SeekDelegate, gdiPlusStreamHelper.CloseDelegate, gdiPlusStreamHelper.SizeDelegate, referenceHdc, type, ref frameRect, frameUnit, description, out nativeObject);
		}
		else
		{
			status = GDIPlus.GdipRecordMetafileStreamI(new ComIStreamWrapper(stream), referenceHdc, type, ref frameRect, frameUnit, description, out nativeObject);
		}
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class from the specified data stream, a Windows handle to a device context, a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. A string that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" /> can be added.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> that contains the data for this <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="description">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(Stream stream, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
	{
		if (stream == null)
		{
			throw new NullReferenceException("stream");
		}
		Status status = Status.NotImplemented;
		if (GDIPlus.RunningOnUnix())
		{
			GDIPlus.GdiPlusStreamHelper gdiPlusStreamHelper = new GDIPlus.GdiPlusStreamHelper(stream, seekToOrigin: false);
			status = GDIPlus.GdipRecordMetafileFromDelegate_linux(gdiPlusStreamHelper.GetHeaderDelegate, gdiPlusStreamHelper.GetBytesDelegate, gdiPlusStreamHelper.PutBytesDelegate, gdiPlusStreamHelper.SeekDelegate, gdiPlusStreamHelper.CloseDelegate, gdiPlusStreamHelper.SizeDelegate, referenceHdc, type, ref frameRect, frameUnit, description, out nativeObject);
		}
		else
		{
			status = GDIPlus.GdipRecordMetafileStream(new ComIStreamWrapper(stream), referenceHdc, type, ref frameRect, frameUnit, description, out nativeObject);
		}
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, a <see cref="T:System.Drawing.Rectangle" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. A descriptive string can also be added.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.Rectangle" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="description">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, Rectangle frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRecordMetafileFileNameI(fileName, referenceHdc, type, ref frameRect, frameUnit, description, out nativeObject));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.Metafile" /> class with the specified file name, a Windows handle to a device context, a <see cref="T:System.Drawing.RectangleF" /> structure that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />, the supplied unit of measure, and an <see cref="T:System.Drawing.Imaging.EmfType" /> enumeration that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. A descriptive string can also be added.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that represents the file name of the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="referenceHdc">A Windows handle to a device context. </param>
	/// <param name="frameRect">A <see cref="T:System.Drawing.RectangleF" /> that represents the rectangle that bounds the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="frameUnit">A <see cref="T:System.Drawing.Imaging.MetafileFrameUnit" /> that specifies the unit of measure for <paramref name="frameRect" />. </param>
	/// <param name="type">An <see cref="T:System.Drawing.Imaging.EmfType" /> that specifies the format of the <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	/// <param name="description">A <see cref="T:System.String" /> that contains a descriptive name for the new <see cref="T:System.Drawing.Imaging.Metafile" />. </param>
	public Metafile(string fileName, IntPtr referenceHdc, RectangleF frameRect, MetafileFrameUnit frameUnit, EmfType type, string description)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRecordMetafileFileName(fileName, referenceHdc, type, ref frameRect, frameUnit, description, out nativeObject));
	}

	protected override void Dispose(bool disposing)
	{
		if (_metafileHolder != null && !_metafileHolder.Disposed)
		{
			_metafileHolder.MetafileDisposed(nativeObject);
			_metafileHolder = null;
			nativeObject = IntPtr.Zero;
		}
		base.Dispose(disposing);
	}

	/// <summary>Returns a Windows handle to an enhanced <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <returns>A Windows handle to this enhanced <see cref="T:System.Drawing.Imaging.Metafile" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public IntPtr GetHenhmetafile()
	{
		return nativeObject;
	}

	/// <summary>Returns the <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with this <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with this <see cref="T:System.Drawing.Imaging.Metafile" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoLimitation("Metafiles aren't only partially supported by libgdiplus.")]
	public MetafileHeader GetMetafileHeader()
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MetafileHeader)));
		try
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetMetafileHeaderFromMetafile(nativeObject, intPtr));
			return new MetafileHeader(intPtr);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Returns the <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with the specified <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with the specified <see cref="T:System.Drawing.Imaging.Metafile" />.</returns>
	/// <param name="henhmetafile">The handle to the enhanced <see cref="T:System.Drawing.Imaging.Metafile" /> for which a header is returned. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoLimitation("Metafiles aren't only partially supported by libgdiplus.")]
	public static MetafileHeader GetMetafileHeader(IntPtr henhmetafile)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MetafileHeader)));
		try
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetMetafileHeaderFromEmf(henhmetafile, intPtr));
			return new MetafileHeader(intPtr);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Returns the <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with the specified <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with the specified <see cref="T:System.Drawing.Imaging.Metafile" />.</returns>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> containing the <see cref="T:System.Drawing.Imaging.Metafile" /> for which a header is retrieved. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoLimitation("Metafiles aren't only partially supported by libgdiplus.")]
	public static MetafileHeader GetMetafileHeader(Stream stream)
	{
		if (stream == null)
		{
			throw new NullReferenceException("stream");
		}
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MetafileHeader)));
		try
		{
			Status status;
			if (GDIPlus.RunningOnUnix())
			{
				GDIPlus.GdiPlusStreamHelper gdiPlusStreamHelper = new GDIPlus.GdiPlusStreamHelper(stream, seekToOrigin: false);
				status = GDIPlus.GdipGetMetafileHeaderFromDelegate_linux(gdiPlusStreamHelper.GetHeaderDelegate, gdiPlusStreamHelper.GetBytesDelegate, gdiPlusStreamHelper.PutBytesDelegate, gdiPlusStreamHelper.SeekDelegate, gdiPlusStreamHelper.CloseDelegate, gdiPlusStreamHelper.SizeDelegate, intPtr);
			}
			else
			{
				status = GDIPlus.GdipGetMetafileHeaderFromStream(new ComIStreamWrapper(stream), intPtr);
			}
			GDIPlus.CheckStatus(status);
			return new MetafileHeader(intPtr);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Returns the <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with the specified <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with the specified <see cref="T:System.Drawing.Imaging.Metafile" />.</returns>
	/// <param name="fileName">A <see cref="T:System.String" /> containing the name of the <see cref="T:System.Drawing.Imaging.Metafile" /> for which a header is retrieved. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoLimitation("Metafiles aren't only partially supported by libgdiplus.")]
	public static MetafileHeader GetMetafileHeader(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MetafileHeader)));
		try
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetMetafileHeaderFromFile(fileName, intPtr));
			return new MetafileHeader(intPtr);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Returns the <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with the specified <see cref="T:System.Drawing.Imaging.Metafile" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Imaging.MetafileHeader" /> associated with the specified <see cref="T:System.Drawing.Imaging.Metafile" />.</returns>
	/// <param name="hmetafile">The handle to the <see cref="T:System.Drawing.Imaging.Metafile" /> for which to return a header. </param>
	/// <param name="wmfHeader">A <see cref="T:System.Drawing.Imaging.WmfPlaceableFileHeader" />. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoLimitation("Metafiles aren't only partially supported by libgdiplus.")]
	public static MetafileHeader GetMetafileHeader(IntPtr hmetafile, WmfPlaceableFileHeader wmfHeader)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MetafileHeader)));
		try
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetMetafileHeaderFromEmf(hmetafile, intPtr));
			return new MetafileHeader(intPtr);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Plays an individual metafile record.</summary>
	/// <param name="recordType">Element of the <see cref="T:System.Drawing.Imaging.EmfPlusRecordType" /> that specifies the type of metafile record being played. </param>
	/// <param name="flags">A set of flags that specify attributes of the record. </param>
	/// <param name="dataSize">The number of bytes in the record data. </param>
	/// <param name="data">An array of bytes that contains the record data. </param>
	[System.MonoLimitation("Metafiles aren't only partially supported by libgdiplus.")]
	public void PlayRecord(EmfPlusRecordType recordType, int flags, int dataSize, byte[] data)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipPlayMetafileRecord(nativeObject, recordType, flags, dataSize, data));
	}
}
