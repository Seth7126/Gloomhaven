using System.Runtime.Serialization;

namespace System.Reflection;

/// <summary>The exception that is thrown when the number of parameters for an invocation does not match the number expected. This class cannot be inherited.</summary>
[Serializable]
public sealed class TargetParameterCountException : ApplicationException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.TargetParameterCountException" /> class with an empty message string and the root cause of the exception.</summary>
	public TargetParameterCountException()
		: base("Number of parameters specified does not match the expected number.")
	{
		base.HResult = -2147352562;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.TargetParameterCountException" /> class with its message string set to the given message and the root cause exception.</summary>
	/// <param name="message">A String describing the reason this exception was thrown. </param>
	public TargetParameterCountException(string message)
		: base(message)
	{
		base.HResult = -2147352562;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.TargetParameterCountException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public TargetParameterCountException(string message, Exception inner)
		: base(message, inner)
	{
		base.HResult = -2147352562;
	}

	internal TargetParameterCountException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
