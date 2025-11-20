namespace System.Transactions;

/// <summary>Provides additional information regarding a transaction.</summary>
public class TransactionInformation
{
	private string local_id;

	private Guid dtcId = Guid.Empty;

	private DateTime creation_time;

	private TransactionStatus status;

	/// <summary>Gets the creation time of the transaction.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> that contains the creation time of the transaction.</returns>
	public DateTime CreationTime => creation_time;

	/// <summary>Gets a unique identifier of the escalated transaction.</summary>
	/// <returns>A <see cref="T:System.Guid" /> that contains the unique identifier of the escalated transaction.</returns>
	public Guid DistributedIdentifier
	{
		get
		{
			return dtcId;
		}
		internal set
		{
			dtcId = value;
		}
	}

	/// <summary>Gets a unique identifier of the transaction.</summary>
	/// <returns>A unique identifier of the transaction.</returns>
	public string LocalIdentifier => local_id;

	/// <summary>Gets the status of the transaction.</summary>
	/// <returns>A <see cref="T:System.Transactions.TransactionStatus" /> that contains the status of the transaction.</returns>
	public TransactionStatus Status
	{
		get
		{
			return status;
		}
		internal set
		{
			status = value;
		}
	}

	internal TransactionInformation()
	{
		status = TransactionStatus.Active;
		creation_time = DateTime.Now.ToUniversalTime();
		local_id = Guid.NewGuid().ToString() + ":1";
	}

	private TransactionInformation(TransactionInformation other)
	{
		local_id = other.local_id;
		dtcId = other.dtcId;
		creation_time = other.creation_time;
		status = other.status;
	}

	internal TransactionInformation Clone(TransactionInformation other)
	{
		return new TransactionInformation(other);
	}
}
