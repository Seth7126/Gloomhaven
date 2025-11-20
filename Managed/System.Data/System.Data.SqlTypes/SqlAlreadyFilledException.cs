using System.Runtime.Serialization;

namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.SqlAlreadyFilledException" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
[Serializable]
public sealed class SqlAlreadyFilledException : SqlTypeException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlAlreadyFilledException" /> class.</summary>
	public SqlAlreadyFilledException()
		: this(SQLResource.AlreadyFilledMessage, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlAlreadyFilledException" /> class.</summary>
	/// <param name="message">The string to display when the exception is thrown.</param>
	public SqlAlreadyFilledException(string message)
		: this(message, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlAlreadyFilledException" /> class.</summary>
	/// <param name="message">The string to display when the exception is thrown.</param>
	/// <param name="e">A reference to an inner exception.</param>
	public SqlAlreadyFilledException(string message, Exception e)
		: base(message, e)
	{
		base.HResult = -2146232015;
	}

	private SqlAlreadyFilledException(SerializationInfo si, StreamingContext sc)
		: base(si, sc)
	{
	}
}
