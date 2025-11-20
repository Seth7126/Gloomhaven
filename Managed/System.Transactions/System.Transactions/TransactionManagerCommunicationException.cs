using System.Runtime.Serialization;

namespace System.Transactions;

/// <summary>The exception that is thrown when a resource manager cannot communicate with the transaction manager.</summary>
[Serializable]
public class TransactionManagerCommunicationException : TransactionException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionManagerCommunicationException" /> class.</summary>
	public TransactionManagerCommunicationException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionManagerCommunicationException" /> class with the specified message.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that contains a message that explains why the exception occurred.</param>
	public TransactionManagerCommunicationException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionManagerCommunicationException" /> class with the specified message and inner exception.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that contains a message that explains why the exception occurred.</param>
	/// <param name="innerException">Gets the exception instance that causes the current exception. For more information, see the <see cref="P:System.Exception.InnerException" /> property.</param>
	public TransactionManagerCommunicationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionManagerCommunicationException" /> class with the specified serialization and streaming context information.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that describes a failed serialization.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that describes a failed serialization context.</param>
	protected TransactionManagerCommunicationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
