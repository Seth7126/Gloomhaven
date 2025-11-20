using System.Runtime.Serialization;

namespace System.Data;

/// <summary>Represents the exception that is thrown when you try to perform an operation on a <see cref="T:System.Data.DataRow" /> that is not in a <see cref="T:System.Data.DataTable" />.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public class RowNotInTableException : DataException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.RowNotInTableException" /> class with serialization information.</summary>
	/// <param name="info">The data that is required to serialize or deserialize an object. </param>
	/// <param name="context">Description of the source and destination of the specified serialized stream. </param>
	protected RowNotInTableException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.RowNotInTableException" /> class.</summary>
	public RowNotInTableException()
		: base("Row not found in table.")
	{
		base.HResult = -2146232024;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.RowNotInTableException" /> class with the specified string.</summary>
	/// <param name="s">The string to display when the exception is thrown. </param>
	public RowNotInTableException(string s)
		: base(s)
	{
		base.HResult = -2146232024;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.RowNotInTableException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
	public RowNotInTableException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232024;
	}
}
