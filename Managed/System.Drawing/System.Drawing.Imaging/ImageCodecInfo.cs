using System.Runtime.InteropServices;

namespace System.Drawing.Imaging;

/// <summary>The <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> class provides the necessary storage members and methods to retrieve all pertinent information about the installed image encoders and decoders (called codecs). Not inheritable. </summary>
public sealed class ImageCodecInfo
{
	private Guid _clsid;

	private Guid _formatID;

	private string _codecName;

	private string _dllName;

	private string _formatDescription;

	private string _filenameExtension;

	private string _mimeType;

	private ImageCodecFlags _flags;

	private int _version;

	private byte[][] _signaturePatterns;

	private byte[][] _signatureMasks;

	/// <summary>Gets or sets a <see cref="T:System.Guid" /> structure that contains a GUID that identifies a specific codec.</summary>
	/// <returns>A <see cref="T:System.Guid" /> structure that contains a GUID that identifies a specific codec.</returns>
	public Guid Clsid
	{
		get
		{
			return _clsid;
		}
		set
		{
			_clsid = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Guid" /> structure that contains a GUID that identifies the codec's format.</summary>
	/// <returns>A <see cref="T:System.Guid" /> structure that contains a GUID that identifies the codec's format.</returns>
	public Guid FormatID
	{
		get
		{
			return _formatID;
		}
		set
		{
			_formatID = value;
		}
	}

	/// <summary>Gets or sets a string that contains the name of the codec.</summary>
	/// <returns>A string that contains the name of the codec.</returns>
	public string CodecName
	{
		get
		{
			return _codecName;
		}
		set
		{
			_codecName = value;
		}
	}

	/// <summary>Gets or sets string that contains the path name of the DLL that holds the codec. If the codec is not in a DLL, this pointer is null.</summary>
	/// <returns>A string that contains the path name of the DLL that holds the codec.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string DllName
	{
		get
		{
			return _dllName;
		}
		set
		{
			_dllName = value;
		}
	}

	/// <summary>Gets or sets a string that describes the codec's file format.</summary>
	/// <returns>A string that describes the codec's file format.</returns>
	public string FormatDescription
	{
		get
		{
			return _formatDescription;
		}
		set
		{
			_formatDescription = value;
		}
	}

	/// <summary>Gets or sets string that contains the file name extension(s) used in the codec. The extensions are separated by semicolons.</summary>
	/// <returns>A string that contains the file name extension(s) used in the codec.</returns>
	public string FilenameExtension
	{
		get
		{
			return _filenameExtension;
		}
		set
		{
			_filenameExtension = value;
		}
	}

	/// <summary>Gets or sets a string that contains the codec's Multipurpose Internet Mail Extensions (MIME) type.</summary>
	/// <returns>A string that contains the codec's Multipurpose Internet Mail Extensions (MIME) type.</returns>
	public string MimeType
	{
		get
		{
			return _mimeType;
		}
		set
		{
			_mimeType = value;
		}
	}

	/// <summary>Gets or sets 32-bit value used to store additional information about the codec. This property returns a combination of flags from the <see cref="T:System.Drawing.Imaging.ImageCodecFlags" /> enumeration.</summary>
	/// <returns>A 32-bit value used to store additional information about the codec.</returns>
	public ImageCodecFlags Flags
	{
		get
		{
			return _flags;
		}
		set
		{
			_flags = value;
		}
	}

	/// <summary>Gets or sets the version number of the codec.</summary>
	/// <returns>The version number of the codec.</returns>
	public int Version
	{
		get
		{
			return _version;
		}
		set
		{
			_version = value;
		}
	}

	/// <summary>Gets or sets a two dimensional array of bytes that represents the signature of the codec.</summary>
	/// <returns>A two dimensional array of bytes that represents the signature of the codec.</returns>
	[CLSCompliant(false)]
	public byte[][] SignaturePatterns
	{
		get
		{
			return _signaturePatterns;
		}
		set
		{
			_signaturePatterns = value;
		}
	}

	/// <summary>Gets or sets a two dimensional array of bytes that can be used as a filter.</summary>
	/// <returns>A two dimensional array of bytes that can be used as a filter.</returns>
	[CLSCompliant(false)]
	public byte[][] SignatureMasks
	{
		get
		{
			return _signatureMasks;
		}
		set
		{
			_signatureMasks = value;
		}
	}

	internal ImageCodecInfo()
	{
	}

	/// <summary>Returns an array of <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> objects that contain information about the image decoders built into GDI+.</summary>
	/// <returns>An array of <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> objects. Each <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> object in the array contains information about one of the built-in image decoders.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static ImageCodecInfo[] GetImageDecoders()
	{
		int num = GDIPlus.GdipGetImageDecodersSize(out var decoderNums, out var arraySize);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		IntPtr intPtr = Marshal.AllocHGlobal(arraySize);
		try
		{
			num = GDIPlus.GdipGetImageDecoders(decoderNums, arraySize, intPtr);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			return ConvertFromMemory(intPtr, decoderNums);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Returns an array of <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> objects that contain information about the image encoders built into GDI+.</summary>
	/// <returns>An array of <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> objects. Each <see cref="T:System.Drawing.Imaging.ImageCodecInfo" /> object in the array contains information about one of the built-in image encoders.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static ImageCodecInfo[] GetImageEncoders()
	{
		int num = GDIPlus.GdipGetImageEncodersSize(out var encoderNums, out var arraySize);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		IntPtr intPtr = Marshal.AllocHGlobal(arraySize);
		try
		{
			num = GDIPlus.GdipGetImageEncoders(encoderNums, arraySize, intPtr);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			return ConvertFromMemory(intPtr, encoderNums);
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	private static ImageCodecInfo[] ConvertFromMemory(IntPtr memoryStart, int numCodecs)
	{
		ImageCodecInfo[] array = new ImageCodecInfo[numCodecs];
		for (int i = 0; i < numCodecs; i++)
		{
			IntPtr ptr = (IntPtr)((long)memoryStart + Marshal.SizeOf(typeof(ImageCodecInfoPrivate)) * i);
			ImageCodecInfoPrivate imageCodecInfoPrivate = new ImageCodecInfoPrivate();
			Marshal.PtrToStructure(ptr, imageCodecInfoPrivate);
			array[i] = new ImageCodecInfo();
			array[i].Clsid = imageCodecInfoPrivate.Clsid;
			array[i].FormatID = imageCodecInfoPrivate.FormatID;
			array[i].CodecName = Marshal.PtrToStringUni(imageCodecInfoPrivate.CodecName);
			array[i].DllName = Marshal.PtrToStringUni(imageCodecInfoPrivate.DllName);
			array[i].FormatDescription = Marshal.PtrToStringUni(imageCodecInfoPrivate.FormatDescription);
			array[i].FilenameExtension = Marshal.PtrToStringUni(imageCodecInfoPrivate.FilenameExtension);
			array[i].MimeType = Marshal.PtrToStringUni(imageCodecInfoPrivate.MimeType);
			array[i].Flags = (ImageCodecFlags)imageCodecInfoPrivate.Flags;
			array[i].Version = imageCodecInfoPrivate.Version;
			array[i].SignaturePatterns = new byte[imageCodecInfoPrivate.SigCount][];
			array[i].SignatureMasks = new byte[imageCodecInfoPrivate.SigCount][];
			for (int j = 0; j < imageCodecInfoPrivate.SigCount; j++)
			{
				array[i].SignaturePatterns[j] = new byte[imageCodecInfoPrivate.SigSize];
				array[i].SignatureMasks[j] = new byte[imageCodecInfoPrivate.SigSize];
				Marshal.Copy((IntPtr)((long)imageCodecInfoPrivate.SigMask + j * imageCodecInfoPrivate.SigSize), array[i].SignatureMasks[j], 0, imageCodecInfoPrivate.SigSize);
				Marshal.Copy((IntPtr)((long)imageCodecInfoPrivate.SigPattern + j * imageCodecInfoPrivate.SigSize), array[i].SignaturePatterns[j], 0, imageCodecInfoPrivate.SigSize);
			}
		}
		return array;
	}
}
