using System.Runtime.InteropServices;

namespace System.Drawing.Imaging;

/// <summary>Contains information about a windows-format (WMF) metafile.</summary>
[StructLayout(LayoutKind.Sequential)]
public sealed class MetaHeader
{
	private WmfMetaHeader wmf;

	/// <summary>Gets or sets the size, in bytes, of the header file.</summary>
	/// <returns>The size, in bytes, of the header file.</returns>
	public short HeaderSize
	{
		get
		{
			return wmf.header_size;
		}
		set
		{
			wmf.header_size = value;
		}
	}

	/// <summary>Gets or sets the size, in bytes, of the largest record in the associated <see cref="T:System.Drawing.Imaging.Metafile" /> object.</summary>
	/// <returns>The size, in bytes, of the largest record in the associated <see cref="T:System.Drawing.Imaging.Metafile" /> object.</returns>
	public int MaxRecord
	{
		get
		{
			return wmf.max_record_size;
		}
		set
		{
			wmf.max_record_size = value;
		}
	}

	/// <summary>Gets or sets the maximum number of objects that exist in the <see cref="T:System.Drawing.Imaging.Metafile" /> object at the same time.</summary>
	/// <returns>The maximum number of objects that exist in the <see cref="T:System.Drawing.Imaging.Metafile" /> object at the same time.</returns>
	public short NoObjects
	{
		get
		{
			return wmf.num_of_objects;
		}
		set
		{
			wmf.num_of_objects = value;
		}
	}

	/// <summary>Not used. Always returns 0.</summary>
	/// <returns>Always 0.</returns>
	public short NoParameters
	{
		get
		{
			return wmf.num_of_params;
		}
		set
		{
			wmf.num_of_params = value;
		}
	}

	/// <summary>Gets or sets the size, in bytes, of the associated <see cref="T:System.Drawing.Imaging.Metafile" /> object.</summary>
	/// <returns>The size, in bytes, of the associated <see cref="T:System.Drawing.Imaging.Metafile" /> object.</returns>
	public int Size
	{
		get
		{
			if (BitConverter.IsLittleEndian)
			{
				return (wmf.file_size_high << 16) | wmf.file_size_low;
			}
			return (wmf.file_size_low << 16) | wmf.file_size_high;
		}
		set
		{
			if (BitConverter.IsLittleEndian)
			{
				wmf.file_size_high = (ushort)(value >> 16);
				wmf.file_size_low = (ushort)value;
			}
			else
			{
				wmf.file_size_high = (ushort)value;
				wmf.file_size_low = (ushort)(value >> 16);
			}
		}
	}

	/// <summary>Gets or sets the type of the associated <see cref="T:System.Drawing.Imaging.Metafile" /> object.</summary>
	/// <returns>The type of the associated <see cref="T:System.Drawing.Imaging.Metafile" /> object.</returns>
	public short Type
	{
		get
		{
			return wmf.file_type;
		}
		set
		{
			wmf.file_type = value;
		}
	}

	/// <summary>Gets or sets the version number of the header format.</summary>
	/// <returns>The version number of the header format.</returns>
	public short Version
	{
		get
		{
			return wmf.version;
		}
		set
		{
			wmf.version = value;
		}
	}

	/// <summary>Initializes a new instance of the MetaHeader class.</summary>
	public MetaHeader()
	{
	}

	internal MetaHeader(WmfMetaHeader header)
	{
		wmf.file_type = header.file_type;
		wmf.header_size = header.header_size;
		wmf.version = header.version;
		wmf.file_size_low = header.file_size_low;
		wmf.file_size_high = header.file_size_high;
		wmf.num_of_objects = header.num_of_objects;
		wmf.max_record_size = header.max_record_size;
		wmf.num_of_params = header.num_of_params;
	}
}
