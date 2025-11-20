using System.Runtime.Serialization;

namespace System.Transactions;

/// <summary>The exception that is thrown when an operation is attempted on a transaction that is in doubt, or an attempt is made to commit the transaction and the transaction becomes InDoubt. </summary>
[Serializable]
public class TransactionInDoubtException : TransactionException
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionInDoubtException" /> class.</summary>
	public TransactionInDoubtException()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionInDoubtException" /> class with the specified message.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that contains a message that explains why the exception occurred.</param>
	public TransactionInDoubtException(string message)
		: base(message)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionInDoubtException" /> class with the specified message and inner exception.</summary>
	/// <param name="message">A <see cref="T:System.String" /> that contains a message that explains why the exception occurred.</param>
	/// <param name="innerException">Gets the exception instance that causes the current exception. For more information, see the <see cref="P:System.Exception.InnerException" /> property.</param>
	public TransactionInDoubtException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.TransactionInDoubtException" /> class with the specified serialization and streaming context information.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that describes a failed serialization.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that describes a failed serialization context.</param>
	protected TransactionInDoubtException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
