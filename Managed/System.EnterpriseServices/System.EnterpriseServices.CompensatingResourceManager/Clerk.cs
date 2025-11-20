namespace System.EnterpriseServices.CompensatingResourceManager;

/// <summary>Writes records of transactional actions to a log.</summary>
public sealed class Clerk
{
	/// <summary>Gets the number of log records.</summary>
	/// <returns>The number of log records.</returns>
	public int LogRecordCount
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value representing the transaction unit of work (UOW).</summary>
	/// <returns>A GUID representing the UOW.</returns>
	public string TransactionUOW
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.CompensatingResourceManager.Clerk" /> class.</summary>
	/// <param name="compensator">The name of the compensator. </param>
	/// <param name="description">The description of the compensator. </param>
	/// <param name="flags">A bitwise combination of the <see cref="T:System.EnterpriseServices.CompensatingResourceManager.CompensatorOptions" /> values. </param>
	[System.MonoTODO]
	public Clerk(string compensator, string description, CompensatorOptions flags)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.CompensatingResourceManager.Clerk" /> class.</summary>
	/// <param name="compensator">A type that represents the compensator. </param>
	/// <param name="description">The description of the compensator. </param>
	/// <param name="flags">A bitwise combination of the <see cref="T:System.EnterpriseServices.CompensatingResourceManager.CompensatorOptions" /> values. </param>
	[System.MonoTODO]
	public Clerk(Type compensator, string description, CompensatorOptions flags)
	{
		throw new NotImplementedException();
	}

	[System.MonoTODO]
	~Clerk()
	{
		throw new NotImplementedException();
	}

	/// <summary>Forces all log records to disk.</summary>
	[System.MonoTODO]
	public void ForceLog()
	{
		throw new NotImplementedException();
	}

	/// <summary>Performs an immediate abort call on the transaction.</summary>
	[System.MonoTODO]
	public void ForceTransactionToAbort()
	{
		throw new NotImplementedException();
	}

	/// <summary>Does not deliver the last log record that was written by this instance of this interface.</summary>
	[System.MonoTODO]
	public void ForgetLogRecord()
	{
		throw new NotImplementedException();
	}

	/// <summary>Writes unstructured log records to the log.</summary>
	/// <param name="record">The log record to write to the log. </param>
	[System.MonoTODO]
	public void WriteLogRecord(object record)
	{
		throw new NotImplementedException();
	}
}
