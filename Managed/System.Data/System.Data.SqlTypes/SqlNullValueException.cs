using System.Runtime.Serialization;

namespace System.Data.SqlTypes;

/// <summary>The exception that is thrown when the Value property of a <see cref="N:System.Data.SqlTypes" /> structure is set to null.</summary>
[Serializable]
public sealed class SqlNullValueException : SqlTypeException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlNullValueException" /> class with a system-supplied message that describes the error.</summary>
	public SqlNullValueException()
		: this(SQLResource.NullValueMessage, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlNullValueException" /> class with a specified message that describes the error.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
	public SqlNullValueException(string message)
		: this(message, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlNullValueException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
	/// <param name="e">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public SqlNullValueException(string message, Exception e)
		: base(message, e)
	{
		base.HResult = -2146232015;
	}

	private SqlNullValueException(SerializationInfo si, StreamingContext sc)
		: base(SqlNullValueExceptionSerialization(si, sc), sc)
	{
	}

	private static SerializationInfo SqlNullValueExceptionSerialization(SerializationInfo si, StreamingContext sc)
	{
		if (si != null && 1 == si.MemberCount)
		{
			new SqlNullValueException(si.GetString("SqlNullValueExceptionMessage")).GetObjectData(si, sc);
		}
		return si;
	}
}
