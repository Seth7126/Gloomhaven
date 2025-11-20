using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>Represents a mutable reference type that wraps either a <see cref="P:System.Data.SqlTypes.SqlBytes.Buffer" /> or a <see cref="P:System.Data.SqlTypes.SqlBytes.Stream" />.</summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public sealed class SqlBytes : INullable, IXmlSerializable, ISerializable
{
	internal byte[] _rgbBuf;

	private long _lCurLen;

	internal Stream _stream;

	private SqlBytesCharsState _state;

	private byte[] _rgbWorkBuf;

	private const long x_lMaxLen = 2147483647L;

	private const long x_lNull = -1L;

	/// <summary>Gets a Boolean value that indicates whether this <see cref="T:System.Data.SqlTypes.SqlBytes" /> is null.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlTypes.SqlBytes" /> is null, false otherwise.</returns>
	public bool IsNull => _state == SqlBytesCharsState.Null;

	/// <summary>Returns a reference to the internal buffer. </summary>
	/// <returns>Returns a reference to the internal buffer. For <see cref="T:System.Data.SqlTypes.SqlBytes" /> instances created on top of unmanaged pointers, it returns a managed copy of the internal buffer.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] Buffer
	{
		get
		{
			if (FStream())
			{
				CopyStreamToBuffer();
			}
			return _rgbBuf;
		}
	}

	/// <summary>Gets the length of the value that is contained in the <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance.</summary>
	/// <returns>A <see cref="T:System.Int64" /> value representing the length of the value that is contained in the <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance. Returns -1 if no buffer is available to the instance or if the value is null. Returns a <see cref="P:System.IO.Stream.Length" /> for a stream-wrapped instance.</returns>
	public long Length => _state switch
	{
		SqlBytesCharsState.Null => throw new SqlNullValueException(), 
		SqlBytesCharsState.Stream => _stream.Length, 
		_ => _lCurLen, 
	};

	/// <summary>Gets the maximum length of the value of the internal buffer of this <see cref="T:System.Data.SqlTypes.SqlBytes" />.</summary>
	/// <returns>A long representing the maximum length of the value of the internal buffer. Returns -1 for a stream-wrapped <see cref="T:System.Data.SqlTypes.SqlBytes" />.</returns>
	public long MaxLength
	{
		get
		{
			if (_state == SqlBytesCharsState.Stream)
			{
				return -1L;
			}
			if (_rgbBuf != null)
			{
				return _rgbBuf.Length;
			}
			return -1L;
		}
	}

	/// <summary>Returns a managed copy of the value held by this <see cref="T:System.Data.SqlTypes.SqlBytes" />.</summary>
	/// <returns>The value of this <see cref="T:System.Data.SqlTypes.SqlBytes" /> as an array of bytes.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] Value
	{
		get
		{
			byte[] array;
			switch (_state)
			{
			case SqlBytesCharsState.Null:
				throw new SqlNullValueException();
			case SqlBytesCharsState.Stream:
				if (_stream.Length > int.MaxValue)
				{
					throw new SqlTypeException("The buffer is insufficient. Read or write operation failed.");
				}
				array = new byte[_stream.Length];
				if (_stream.Position != 0L)
				{
					_stream.Seek(0L, SeekOrigin.Begin);
				}
				_stream.Read(array, 0, checked((int)_stream.Length));
				break;
			default:
				array = new byte[_lCurLen];
				Array.Copy(_rgbBuf, 0, array, 0, (int)_lCurLen);
				break;
			}
			return array;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance at the specified index.</summary>
	/// <returns>A <see cref="T:System.Byte" /> value. </returns>
	/// <param name="offset">A <see cref="T:System.Int64" /> value.</param>
	public byte this[long offset]
	{
		get
		{
			if (offset < 0 || offset >= Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (_rgbWorkBuf == null)
			{
				_rgbWorkBuf = new byte[1];
			}
			Read(offset, _rgbWorkBuf, 0, 1);
			return _rgbWorkBuf[0];
		}
		set
		{
			if (_rgbWorkBuf == null)
			{
				_rgbWorkBuf = new byte[1];
			}
			_rgbWorkBuf[0] = value;
			Write(offset, _rgbWorkBuf, 0, 1);
		}
	}

	/// <summary>Returns information about the storage state of this <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.StorageState" /> enumeration.</returns>
	public StorageState Storage => _state switch
	{
		SqlBytesCharsState.Null => throw new SqlNullValueException(), 
		SqlBytesCharsState.Stream => StorageState.Stream, 
		SqlBytesCharsState.Buffer => StorageState.Buffer, 
		_ => StorageState.UnmanagedBuffer, 
	};

	/// <summary>Gets or sets the data of this <see cref="T:System.Data.SqlTypes.SqlBytes" /> as a stream.</summary>
	/// <returns>The stream that contains the SqlBytes data.</returns>
	public Stream Stream
	{
		get
		{
			if (!FStream())
			{
				return new StreamOnSqlBytes(this);
			}
			return _stream;
		}
		set
		{
			_lCurLen = -1L;
			_stream = value;
			_state = ((value != null) ? SqlBytesCharsState.Stream : SqlBytesCharsState.Null);
		}
	}

	/// <summary>Returns a null instance of this <see cref="T:System.Data.SqlTypes.SqlBytes" />.</summary>
	/// <returns>Returns an instance in such a way that <see cref="P:System.Data.SqlTypes.SqlBytes.IsNull" /> returns true.</returns>
	public static SqlBytes Null => new SqlBytes((byte[])null);

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlBytes" /> class.</summary>
	public SqlBytes()
	{
		SetNull();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlBytes" /> class based on the specified byte array.</summary>
	/// <param name="buffer">The array of unsigned bytes. </param>
	public SqlBytes(byte[] buffer)
	{
		_rgbBuf = buffer;
		_stream = null;
		if (_rgbBuf == null)
		{
			_state = SqlBytesCharsState.Null;
			_lCurLen = -1L;
		}
		else
		{
			_state = SqlBytesCharsState.Buffer;
			_lCurLen = _rgbBuf.Length;
		}
		_rgbWorkBuf = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlBytes" /> class based on the specified <see cref="T:System.Data.SqlTypes.SqlBinary" /> value.</summary>
	/// <param name="value">A <see cref="T:System.Data.SqlTypes.SqlBinary" /> value.</param>
	public SqlBytes(SqlBinary value)
		: this(value.IsNull ? null : value.Value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlBytes" /> class based on the specified <see cref="T:System.IO.Stream" /> value.</summary>
	/// <param name="s">A <see cref="T:System.IO.Stream" />. </param>
	public SqlBytes(Stream s)
	{
		_rgbBuf = null;
		_lCurLen = -1L;
		_stream = s;
		_state = ((s != null) ? SqlBytesCharsState.Stream : SqlBytesCharsState.Null);
		_rgbWorkBuf = null;
	}

	/// <summary>Sets this <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance to null.</summary>
	public void SetNull()
	{
		_lCurLen = -1L;
		_stream = null;
		_state = SqlBytesCharsState.Null;
	}

	/// <summary>Sets the length of this <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance.</summary>
	/// <param name="value">The <see cref="T:System.Int64" /> long value representing the length.</param>
	public void SetLength(long value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		if (FStream())
		{
			_stream.SetLength(value);
			return;
		}
		if (_rgbBuf == null)
		{
			throw new SqlTypeException("There is no buffer. Read or write operation failed.");
		}
		if (value > _rgbBuf.Length)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		if (IsNull)
		{
			_state = SqlBytesCharsState.Buffer;
		}
		_lCurLen = value;
	}

	/// <summary>Copies bytes from this <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance to the passed-in buffer and returns the number of copied bytes.</summary>
	/// <returns>An <see cref="T:System.Int64" /> long value representing the number of copied bytes.</returns>
	/// <param name="offset">An <see cref="T:System.Int64" /> long value offset into the value that is contained in the <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance.</param>
	/// <param name="buffer">The byte array buffer to copy into.</param>
	/// <param name="offsetInBuffer">An <see cref="T:System.Int32" /> integer offset into the buffer to start copying into.</param>
	/// <param name="count">An <see cref="T:System.Int32" /> integer representing the number of bytes to copy.</param>
	public long Read(long offset, byte[] buffer, int offsetInBuffer, int count)
	{
		if (IsNull)
		{
			throw new SqlNullValueException();
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset > Length || offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (offsetInBuffer > buffer.Length || offsetInBuffer < 0)
		{
			throw new ArgumentOutOfRangeException("offsetInBuffer");
		}
		if (count < 0 || count > buffer.Length - offsetInBuffer)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count > Length - offset)
		{
			count = (int)(Length - offset);
		}
		if (count != 0)
		{
			if (_state == SqlBytesCharsState.Stream)
			{
				if (_stream.Position != offset)
				{
					_stream.Seek(offset, SeekOrigin.Begin);
				}
				_stream.Read(buffer, offsetInBuffer, count);
			}
			else
			{
				Array.Copy(_rgbBuf, offset, buffer, offsetInBuffer, count);
			}
		}
		return count;
	}

	/// <summary>Copies bytes from the passed-in buffer to this <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance.</summary>
	/// <param name="offset">An <see cref="T:System.Int64" /> long value offset into the value that is contained in the <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance.</param>
	/// <param name="buffer">The byte array buffer to copy into.</param>
	/// <param name="offsetInBuffer">An <see cref="T:System.Int32" /> integer offset into the buffer to start copying into.</param>
	/// <param name="count">An <see cref="T:System.Int32" /> integer representing the number of bytes to copy.</param>
	public void Write(long offset, byte[] buffer, int offsetInBuffer, int count)
	{
		if (FStream())
		{
			if (_stream.Position != offset)
			{
				_stream.Seek(offset, SeekOrigin.Begin);
			}
			_stream.Write(buffer, offsetInBuffer, count);
			return;
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (_rgbBuf == null)
		{
			throw new SqlTypeException("There is no buffer. Read or write operation failed.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (offset > _rgbBuf.Length)
		{
			throw new SqlTypeException("The buffer is insufficient. Read or write operation failed.");
		}
		if (offsetInBuffer < 0 || offsetInBuffer > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offsetInBuffer");
		}
		if (count < 0 || count > buffer.Length - offsetInBuffer)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count > _rgbBuf.Length - offset)
		{
			throw new SqlTypeException("The buffer is insufficient. Read or write operation failed.");
		}
		if (IsNull)
		{
			if (offset != 0L)
			{
				throw new SqlTypeException("Cannot write to non-zero offset, because current value is Null.");
			}
			_lCurLen = 0L;
			_state = SqlBytesCharsState.Buffer;
		}
		else if (offset > _lCurLen)
		{
			throw new SqlTypeException("Cannot write from an offset that is larger than current length. It would leave uninitialized data in the buffer.");
		}
		if (count != 0)
		{
			Array.Copy(buffer, offsetInBuffer, _rgbBuf, offset, count);
			if (_lCurLen < offset + count)
			{
				_lCurLen = offset + count;
			}
		}
	}

	/// <summary>Constructs and returns a <see cref="T:System.Data.SqlTypes.SqlBinary" /> from this <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBinary" /> from this instance.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public SqlBinary ToSqlBinary()
	{
		if (!IsNull)
		{
			return new SqlBinary(Value);
		}
		return SqlBinary.Null;
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlBytes" /> structure to a <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure.</returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlBytes" /> structure to be converted.</param>
	public static explicit operator SqlBinary(SqlBytes value)
	{
		return value.ToSqlBinary();
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure to a <see cref="T:System.Data.SqlTypes.SqlBytes" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlBytes" /> structure.</returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlBinary" /> structure to be converted.</param>
	public static explicit operator SqlBytes(SqlBinary value)
	{
		return new SqlBytes(value);
	}

	[Conditional("DEBUG")]
	private void AssertValid()
	{
		_ = IsNull;
	}

	private void CopyStreamToBuffer()
	{
		long length = _stream.Length;
		if (length >= int.MaxValue)
		{
			throw new SqlTypeException("Cannot write from an offset that is larger than current length. It would leave uninitialized data in the buffer.");
		}
		if (_rgbBuf == null || _rgbBuf.Length < length)
		{
			_rgbBuf = new byte[length];
		}
		if (_stream.Position != 0L)
		{
			_stream.Seek(0L, SeekOrigin.Begin);
		}
		_stream.Read(_rgbBuf, 0, (int)length);
		_stream = null;
		_lCurLen = length;
		_state = SqlBytesCharsState.Buffer;
	}

	internal bool FStream()
	{
		return _state == SqlBytesCharsState.Stream;
	}

	private void SetBuffer(byte[] buffer)
	{
		_rgbBuf = buffer;
		_lCurLen = ((_rgbBuf == null) ? (-1) : _rgbBuf.Length);
		_stream = null;
		_state = ((_rgbBuf != null) ? SqlBytesCharsState.Buffer : SqlBytesCharsState.Null);
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</returns>
	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="r">XmlReader</param>
	void IXmlSerializable.ReadXml(XmlReader r)
	{
		byte[] buffer = null;
		string attribute = r.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
		if (attribute != null && XmlConvert.ToBoolean(attribute))
		{
			r.ReadElementString();
			SetNull();
		}
		else
		{
			string text = r.ReadElementString();
			if (text == null)
			{
				buffer = Array.Empty<byte>();
			}
			else
			{
				text = text.Trim();
				buffer = ((text.Length != 0) ? Convert.FromBase64String(text) : Array.Empty<byte>());
			}
		}
		SetBuffer(buffer);
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="writer">XmlWriter</param>
	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		if (IsNull)
		{
			writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
			return;
		}
		byte[] buffer = Buffer;
		writer.WriteString(Convert.ToBase64String(buffer, 0, (int)Length));
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>A string that indicates the XSD of the specified XmlSchemaSet.</returns>
	/// <param name="schemaSet">A <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("base64Binary", "http://www.w3.org/2001/XMLSchema");
	}

	/// <summary>Gets serialization information with all the data needed to reinstantiate this <see cref="T:System.Data.SqlTypes.SqlBytes" /> instance.</summary>
	/// <param name="info">The object to be populated with serialization information. </param>
	/// <param name="context">The destination context of the serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new PlatformNotSupportedException();
	}
}
