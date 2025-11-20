using System.Runtime.Serialization;

namespace System.Data;

/// <summary>Represents the exception that is thrown when attempting an action that violates a constraint.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public class ConstraintException : DataException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.ConstraintException" /> class using the specified serialization and stream context.</summary>
	/// <param name="info">The data necessary to serialize or deserialize an object. </param>
	/// <param name="context">Description of the source and destination of the specified serialized stream. </param>
	protected ConstraintException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.ConstraintException" /> class. This is the default constructor.</summary>
	public ConstraintException()
		: base("Constraint Exception.")
	{
		base.HResult = -2146232022;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.ConstraintException" /> class with the specified string.</summary>
	/// <param name="s">The string to display when the exception is thrown. </param>
	public ConstraintException(string s)
		: base(s)
	{
		base.HResult = -2146232022;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.ConstraintException" /> class using the specified string and inner exception.</summary>
	/// <param name="message">The string to display when the exception is thrown. </param>
	/// <param name="innerException">Gets the Exception instance that caused the current exception.</param>
	public ConstraintException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232022;
	}
}
