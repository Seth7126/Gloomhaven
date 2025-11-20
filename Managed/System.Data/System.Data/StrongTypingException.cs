using System.Runtime.Serialization;

namespace System.Data;

/// <summary>The exception that is thrown by a strongly typed <see cref="T:System.Data.DataSet" /> when the user accesses a DBNull value.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public class StrongTypingException : DataException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.StrongTypingException" /> class using the specified serialization information and streaming context.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object. </param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure. </param>
	protected StrongTypingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.StrongTypingException" /> class.</summary>
	public StrongTypingException()
	{
		base.HResult = -2146232021;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.StrongTypingException" /> class with the specified string.</summary>
	/// <param name="message">The string to display when the exception is thrown. </param>
	public StrongTypingException(string message)
		: base(message)
	{
		base.HResult = -2146232021;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.StrongTypingException" /> class with the specified string and inner exception.</summary>
	/// <param name="s">The string to display when the exception is thrown. </param>
	/// <param name="innerException">A reference to an inner exception. </param>
	public StrongTypingException(string s, Exception innerException)
		: base(s, innerException)
	{
		base.HResult = -2146232021;
	}
}
