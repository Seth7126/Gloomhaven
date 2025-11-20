using System.Runtime.Serialization;

namespace System.Data;

/// <summary>Represents the exception that is thrown when an action is tried on a <see cref="T:System.Data.DataRow" /> that has been deleted.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public class DeletedRowInaccessibleException : DataException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DeletedRowInaccessibleException" /> class with serialization information.</summary>
	/// <param name="info">The data that is required to serialize or deserialize an object. </param>
	/// <param name="context">Description of the source and destination of the specified serialized stream. </param>
	protected DeletedRowInaccessibleException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DeletedRowInaccessibleException" /> class.</summary>
	public DeletedRowInaccessibleException()
		: base("Deleted rows inaccessible.")
	{
		base.HResult = -2146232031;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DeletedRowInaccessibleException" /> class with the specified string.</summary>
	/// <param name="s">The string to display when the exception is thrown. </param>
	public DeletedRowInaccessibleException(string s)
		: base(s)
	{
		base.HResult = -2146232031;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DeletedRowInaccessibleException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
	public DeletedRowInaccessibleException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232031;
	}
}
