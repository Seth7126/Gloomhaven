using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data.SqlTypes;

/// <summary>
///   <see cref="T:System.Data.SqlTypes.SqlChars" /> is a mutable reference type that wraps a <see cref="T:System.Char" /> array or a <see cref="T:System.Data.SqlTypes.SqlString" /> instance.</summary>
[Serializable]
[XmlSchemaProvider("GetXsdType")]
public sealed class SqlChars : INullable, IXmlSerializable, ISerializable
{
	internal char[] _rgchBuf;

	private long _lCurLen;

	internal SqlStreamChars _stream;

	private SqlBytesCharsState _state;

	private char[] _rgchWorkBuf;

	private const long x_lMaxLen = 2147483647L;

	private const long x_lNull = -1L;

	/// <summary>Gets a Boolean value that indicates whether this <see cref="T:System.Data.SqlTypes.SqlChars" /> is null.</summary>
	/// <returns>true if the <see cref="T:System.Data.SqlTypes.SqlChars" /> is null. Otherwise, false. </returns>
	public bool IsNull => _state == SqlBytesCharsState.Null;

	/// <summary>Returns a reference to the internal buffer. </summary>
	/// <returns>Returns a reference to the internal buffer. For <see cref="T:System.Data.SqlTypes.SqlChars" /> instances created on top of unmanaged pointers, it returns a managed copy of the internal buffer.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public char[] Buffer
	{
		get
		{
			if (FStream())
			{
				CopyStreamToBuffer();
			}
			return _rgchBuf;
		}
	}

	/// <summary>Gets the length of the value that is contained in the <see cref="T:System.Data.SqlTypes.SqlChars" /> instance.</summary>
	/// <returns>A <see cref="T:System.Int64" /> value that indicates the length in characters of the value that is contained in the <see cref="T:System.Data.SqlTypes.SqlChars" /> instance.Returns -1 if no buffer is available to the instance, or if the value is null. Returns a <see cref="P:System.IO.Stream.Length" /> for a stream-wrapped instance.</returns>
	public long Length => _state switch
	{
		SqlBytesCharsState.Null => throw new SqlNullValueException(), 
		SqlBytesCharsState.Stream => _stream.Length, 
		_ => _lCurLen, 
	};

	/// <summary>Gets the maximum length in two-byte characters of the value the internal buffer can hold.</summary>
	/// <returns>An <see cref="T:System.Int64" /> value representing the maximum length in two-byte characters of the value of the internal buffer. Returns -1 for a stream-wrapped <see cref="T:System.Data.SqlTypes.SqlChars" />.</returns>
	public long MaxLength
	{
		get
		{
			if (_state == SqlBytesCharsState.Stream)
			{
				return -1L;
			}
			if (_rgchBuf != null)
			{
				return _rgchBuf.Length;
			}
			return -1L;
		}
	}

	/// <summary>Returns a managed copy of the value held by this <see cref="T:System.Data.SqlTypes.SqlChars" />.</summary>
	/// <returns>The value of this <see cref="T:System.Data.SqlTypes.SqlChars" /> as an array of characters.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public char[] Value
	{
		get
		{
			char[] array;
			switch (_state)
			{
			case SqlBytesCharsState.Null:
				throw new SqlNullValueException();
			case SqlBytesCharsState.Stream:
				if (_stream.Length > int.MaxValue)
				{
					throw new SqlTypeException("The buffer is insufficient. Read or write operation failed.");
				}
				array = new char[_stream.Length];
				if (_stream.Position != 0L)
				{
					_stream.Seek(0L, SeekOrigin.Begin);
				}
				_stream.Read(array, 0, checked((int)_stream.Length));
				break;
			default:
				array = new char[_lCurLen];
				Array.Copy(_rgchBuf, 0, array, 0, (int)_lCurLen);
				break;
			}
			return array;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.SqlTypes.SqlChars" /> instance at the specified index.</summary>
	/// <returns>A <see cref="T:System.Char" /> value. </returns>
	/// <param name="offset">An <see cref="T:System.Int64" /> value.</param>
	public char this[long offset]
	{
		get
		{
			if (offset < 0 || offset >= Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (_rgchWorkBuf == null)
			{
				_rgchWorkBuf = new char[1];
			}
			Read(offset, _rgchWorkBuf, 0, 1);
			return _rgchWorkBuf[0];
		}
		set
		{
			if (_rgchWorkBuf == null)
			{
				_rgchWorkBuf = new char[1];
			}
			_rgchWorkBuf[0] = value;
			Write(offset, _rgchWorkBuf, 0, 1);
		}
	}

	internal SqlStreamChars Stream
	{
		get
		{
			if (!FStream())
			{
				return new StreamOnSqlChars(this);
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

	/// <summary>Returns information about the storage state of this <see cref="T:System.Data.SqlTypes.SqlChars" /> instance.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.StorageState" /> enumeration.</returns>
	public StorageState Storage => _state switch
	{
		SqlBytesCharsState.Null => throw new SqlNullValueException(), 
		SqlBytesCharsState.Stream => StorageState.Stream, 
		SqlBytesCharsState.Buffer => StorageState.Buffer, 
		_ => StorageState.UnmanagedBuffer, 
	};

	/// <summary>Returns a null instance of this <see cref="T:System.Data.SqlTypes.SqlChars" />.</summary>
	/// <returns>Returns an instance in such a way that <see cref="P:System.Data.SqlTypes.SqlChars.IsNull" /> returns true. For more information, see Handling Null Values (ADO.NET).</returns>
	public static SqlChars Null => new SqlChars((char[])null);

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlChars" /> class.</summary>
	public SqlChars()
	{
		SetNull();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlChars" /> class based on the specified character array.</summary>
	/// <param name="buffer">A <see cref="T:System.Char" /> array.</param>
	public SqlChars(char[] buffer)
	{
		_rgchBuf = buffer;
		_stream = null;
		if (_rgchBuf == null)
		{
			_state = SqlBytesCharsState.Null;
			_lCurLen = -1L;
		}
		else
		{
			_state = SqlBytesCharsState.Buffer;
			_lCurLen = _rgchBuf.Length;
		}
		_rgchWorkBuf = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlChars" /> class based on the specified <see cref="T:System.Data.SqlTypes.SqlString" /> value.</summary>
	/// <param name="value">A <see cref="T:System.Data.SqlTypes.SqlString" />.</param>
	public SqlChars(SqlString value)
		: this(value.IsNull ? null : value.Value.ToCharArray())
	{
	}

	internal SqlChars(SqlStreamChars s)
	{
		_rgchBuf = null;
		_lCurLen = -1L;
		_stream = s;
		_state = ((s != null) ? SqlBytesCharsState.Stream : SqlBytesCharsState.Null);
		_rgchWorkBuf = null;
	}

	/// <summary>Sets this <see cref="T:System.Data.SqlTypes.SqlChars" /> instance to null.</summary>
	public void SetNull()
	{
		_lCurLen = -1L;
		_stream = null;
		_state = SqlBytesCharsState.Null;
	}

	/// <summary>Sets the length of this <see cref="T:System.Data.SqlTypes.SqlChars" /> instance.</summary>
	/// <param name="value">The <see cref="T:System.Int64" />long value representing the length.</param>
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
		if (_rgchBuf == null)
		{
			throw new SqlTypeException("There is no buffer. Read or write operation failed.");
		}
		if (value > _rgchBuf.Length)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		if (IsNull)
		{
			_state = SqlBytesCharsState.Buffer;
		}
		_lCurLen = value;
	}

	/// <summary>Copies characters from this <see cref="T:System.Data.SqlTypes.SqlChars" /> instance to the passed-in buffer and returns the number of copied characters.</summary>
	/// <returns>An <see cref="T:System.Int64" />long value representing the number of copied bytes.</returns>
	/// <param name="offset">An <see cref="T:System.Int64" />long value offset into the value that is contained in the <see cref="T:System.Data.SqlTypes.SqlChars" /> instance.</param>
	/// <param name="buffer">The character array buffer to copy into.</param>
	/// <param name="offsetInBuffer">An <see cref="T:System.Int32" /> integer offset into the buffer to start copying into.</param>
	/// <param name="count">An <see cref="T:System.Int32" /> integer value representing the number of characters to copy.</param>
	public long Read(long offset, char[] buffer, int offsetInBuffer, int count)
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
				Array.Copy(_rgchBuf, offset, buffer, offsetInBuffer, count);
			}
		}
		return count;
	}

	/// <summary>Copies characters from the passed-in buffer to this <see cref="T:System.Data.SqlTypes.SqlChars" /> instance.</summary>
	/// <param name="offset">A long value offset into the value that is contained in the <see cref="T:System.Data.SqlTypes.SqlChars" /> instance.</param>
	/// <param name="buffer">The character array buffer to copy into.</param>
	/// <param name="offsetInBuffer">An <see cref="T:System.Int32" /> integer offset into the buffer to start copying into.</param>
	/// <param name="count">An <see cref="T:System.Int32" /> integer representing the number of characters to copy.</param>
	public void Write(long offset, char[] buffer, int offsetInBuffer, int count)
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
		if (_rgchBuf == null)
		{
			throw new SqlTypeException("There is no buffer. Read or write operation failed.");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (offset > _rgchBuf.Length)
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
		if (count > _rgchBuf.Length - offset)
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
			Array.Copy(buffer, offsetInBuffer, _rgchBuf, offset, count);
			if (_lCurLen < offset + count)
			{
				_lCurLen = offset + count;
			}
		}
	}

	/// <summary>Converts this <see cref="T:System.Data.SqlTypes.SqlChars" /> instance to its equivalent <see cref="T:System.Data.SqlTypes.SqlString" /> representation.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlString" /> representation of this type.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public SqlString ToSqlString()
	{
		if (!IsNull)
		{
			return new string(Value);
		}
		return SqlString.Null;
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlChars" /> structure to a <see cref="T:System.Data.SqlTypes.SqlString" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlString" /> structure.</returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlChars" /> structure to be converted.</param>
	public static explicit operator SqlString(SqlChars value)
	{
		return value.ToSqlString();
	}

	/// <summary>Converts a <see cref="T:System.Data.SqlTypes.SqlString" /> structure to a <see cref="T:System.Data.SqlTypes.SqlChars" /> structure.</summary>
	/// <returns>A <see cref="T:System.Data.SqlTypes.SqlChars" /> structure.</returns>
	/// <param name="value">The <see cref="T:System.Data.SqlTypes.SqlString" /> structure to be converted.</param>
	public static explicit operator SqlChars(SqlString value)
	{
		return new SqlChars(value);
	}

	[Conditional("DEBUG")]
	private void AssertValid()
	{
		_ = IsNull;
	}

	internal bool FStream()
	{
		return _state == SqlBytesCharsState.Stream;
	}

	private void CopyStreamToBuffer()
	{
		long length = _stream.Length;
		if (length >= int.MaxValue)
		{
			throw new SqlTypeException("The buffer is insufficient. Read or write operation failed.");
		}
		if (_rgchBuf == null || _rgchBuf.Length < length)
		{
			_rgchBuf = new char[length];
		}
		if (_stream.Position != 0L)
		{
			_stream.Seek(0L, SeekOrigin.Begin);
		}
		_stream.Read(_rgchBuf, 0, (int)length);
		_stream = null;
		_lCurLen = length;
		_state = SqlBytesCharsState.Buffer;
	}

	private void SetBuffer(char[] buffer)
	{
		_rgchBuf = buffer;
		_lCurLen = ((_rgchBuf == null) ? (-1) : _rgchBuf.Length);
		_stream = null;
		_state = ((_rgchBuf != null) ? SqlBytesCharsState.Buffer : SqlBytesCharsState.Null);
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
		char[] array = null;
		string attribute = r.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
		if (attribute != null && XmlConvert.ToBoolean(attribute))
		{
			r.ReadElementString();
			SetNull();
		}
		else
		{
			array = r.ReadElementString().ToCharArray();
			SetBuffer(array);
		}
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
		char[] buffer = Buffer;
		writer.WriteString(new string(buffer, 0, (int)Length));
	}

	/// <summary>Returns the XML Schema definition language (XSD) of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</summary>
	/// <returns>A string value that indicates the XSD of the specified <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</returns>
	/// <param name="schemaSet">A <see cref="T:System.Xml.Schema.XmlSchemaSet" />.</param>
	public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
	{
		return new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
	}

	/// <summary>Gets serialization information with all the data needed to reinstantiate this <see cref="T:System.Data.SqlTypes.SqlChars" /> instance.</summary>
	/// <param name="info">The object to be populated with serialization information. </param>
	/// <param name="context">The destination context of the serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new PlatformNotSupportedException();
	}
}
