using System.Runtime.Serialization;

namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.SqlNotFilledException" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</summary>
[Serializable]
public sealed class SqlNotFilledException : SqlTypeException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlNotFilledException" /> class.</summary>
	public SqlNotFilledException()
		: this(SQLResource.NotFilledMessage, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlNotFilledException" /> class.</summary>
	/// <param name="message">The string to display when the exception is thrown.</param>
	public SqlNotFilledException(string message)
		: this(message, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.SqlNotFilledException" /> class.</summary>
	/// <param name="message">The string to display when the exception is thrown.</param>
	/// <param name="e">A reference to an inner exception.</param>
	public SqlNotFilledException(string message, Exception e)
		: base(message, e)
	{
		base.HResult = -2146232015;
	}

	private SqlNotFilledException(SerializationInfo si, StreamingContext sc)
		: base(si, sc)
	{
	}
}
