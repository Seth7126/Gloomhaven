using System.Data.Common;
using System.Data.ProviderBase;
using System.Runtime.InteropServices;

namespace System.Data.Odbc;

internal sealed class CNativeBuffer : DbBuffer
{
	internal short ShortLength => checked((short)base.Length);

	internal CNativeBuffer(int initialSize)
		: base(initialSize)
	{
	}

	internal object MarshalToManaged(int offset, ODBC32.SQL_C sqlctype, int cb)
	{
		switch (sqlctype)
		{
		case ODBC32.SQL_C.WCHAR:
			if (cb == -3)
			{
				return PtrToStringUni(offset);
			}
			cb = Math.Min(cb / 2, (base.Length - 2) / 2);
			return PtrToStringUni(offset, cb);
		case ODBC32.SQL_C.BINARY:
		case ODBC32.SQL_C.CHAR:
			cb = Math.Min(cb, base.Length);
			return ReadBytes(offset, cb);
		case ODBC32.SQL_C.SSHORT:
			return ReadInt16(offset);
		case ODBC32.SQL_C.SLONG:
			return ReadInt32(offset);
		case ODBC32.SQL_C.SBIGINT:
			return ReadInt64(offset);
		case ODBC32.SQL_C.BIT:
			return ReadByte(offset) != 0;
		case ODBC32.SQL_C.REAL:
			return ReadSingle(offset);
		case ODBC32.SQL_C.DOUBLE:
			return ReadDouble(offset);
		case ODBC32.SQL_C.UTINYINT:
			return ReadByte(offset);
		case ODBC32.SQL_C.GUID:
			return ReadGuid(offset);
		case ODBC32.SQL_C.TYPE_TIMESTAMP:
			return ReadDateTime(offset);
		case ODBC32.SQL_C.TYPE_DATE:
			return ReadDate(offset);
		case ODBC32.SQL_C.TYPE_TIME:
			return ReadTime(offset);
		case ODBC32.SQL_C.NUMERIC:
			return ReadNumeric(offset);
		default:
			return null;
		}
	}

	internal void MarshalToNative(int offset, object value, ODBC32.SQL_C sqlctype, int sizeorprecision, int valueOffset)
	{
		switch (sqlctype)
		{
		case ODBC32.SQL_C.WCHAR:
			if (value is string)
			{
				int num2 = Math.Max(0, ((string)value).Length - valueOffset);
				if (sizeorprecision > 0 && sizeorprecision < num2)
				{
					num2 = sizeorprecision;
				}
				char[] array2 = ((string)value).ToCharArray(valueOffset, num2);
				WriteCharArray(offset, array2, 0, array2.Length);
				WriteInt16(offset + array2.Length * 2, 0);
			}
			else
			{
				int num2 = Math.Max(0, ((char[])value).Length - valueOffset);
				if (sizeorprecision > 0 && sizeorprecision < num2)
				{
					num2 = sizeorprecision;
				}
				char[] array2 = (char[])value;
				WriteCharArray(offset, array2, valueOffset, num2);
				WriteInt16(offset + array2.Length * 2, 0);
			}
			break;
		case ODBC32.SQL_C.BINARY:
		case ODBC32.SQL_C.CHAR:
		{
			byte[] array = (byte[])value;
			int num = array.Length;
			num -= valueOffset;
			if (sizeorprecision > 0 && sizeorprecision < num)
			{
				num = sizeorprecision;
			}
			WriteBytes(offset, array, valueOffset, num);
			break;
		}
		case ODBC32.SQL_C.UTINYINT:
			WriteByte(offset, (byte)value);
			break;
		case ODBC32.SQL_C.SSHORT:
			WriteInt16(offset, (short)value);
			break;
		case ODBC32.SQL_C.SLONG:
			WriteInt32(offset, (int)value);
			break;
		case ODBC32.SQL_C.REAL:
			WriteSingle(offset, (float)value);
			break;
		case ODBC32.SQL_C.SBIGINT:
			WriteInt64(offset, (long)value);
			break;
		case ODBC32.SQL_C.DOUBLE:
			WriteDouble(offset, (double)value);
			break;
		case ODBC32.SQL_C.GUID:
			WriteGuid(offset, (Guid)value);
			break;
		case ODBC32.SQL_C.BIT:
			WriteByte(offset, (byte)(((bool)value) ? 1u : 0u));
			break;
		case ODBC32.SQL_C.TYPE_TIMESTAMP:
			WriteODBCDateTime(offset, (DateTime)value);
			break;
		case ODBC32.SQL_C.TYPE_DATE:
			WriteDate(offset, (DateTime)value);
			break;
		case ODBC32.SQL_C.TYPE_TIME:
			WriteTime(offset, (TimeSpan)value);
			break;
		case ODBC32.SQL_C.NUMERIC:
			WriteNumeric(offset, (decimal)value, checked((byte)sizeorprecision));
			break;
		}
	}

	internal HandleRef PtrOffset(int offset, int length)
	{
		Validate(offset, length);
		IntPtr intPtr = ADP.IntPtrOffset(DangerousGetHandle(), offset);
		return new HandleRef(this, intPtr);
	}

	internal void WriteODBCDateTime(int offset, DateTime value)
	{
		short[] source = new short[6]
		{
			(short)value.Year,
			(short)value.Month,
			(short)value.Day,
			(short)value.Hour,
			(short)value.Minute,
			(short)value.Second
		};
		WriteInt16Array(offset, source, 0, 6);
		WriteInt32(offset + 12, value.Millisecond * 1000000);
	}
}
