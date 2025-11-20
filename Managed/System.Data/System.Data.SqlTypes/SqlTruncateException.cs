using System.Runtime.Serialization;

namespace System.Data.SqlTypes;

/// <summary>The exception that is thrown when you set a value into a <see cref="N:System.Data.SqlTypes" /> structure would truncate that value.</summary>
[Serializable]
public sealed class SqlTruncateException : SqlTypeException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlTruncateException" /> class.</summary>
	public SqlTruncateException()
		: this(SQLResource.TruncationMessage, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlTruncateException" /> class with a specified error message.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	public SqlTruncateException(string message)
		: this(message, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlTruncateException" /> class with a specified error message and a reference to the <see cref="T:System.Exception" />.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="e">A reference to an inner <see cref="T:System.Exception" />. </param>
	public SqlTruncateException(string message, Exception e)
		: base(message, e)
	{
		base.HResult = -2146232014;
	}

	private SqlTruncateException(SerializationInfo si, StreamingContext sc)
		: base(SqlTruncateExceptionSerialization(si, sc), sc)
	{
	}

	private static SerializationInfo SqlTruncateExceptionSerialization(SerializationInfo si, StreamingContext sc)
	{
		if (si != null && 1 == si.MemberCount)
		{
			new SqlTruncateException(si.GetString("SqlTruncateExceptionMessage")).GetObjectData(si, sc);
		}
		return si;
	}
}
