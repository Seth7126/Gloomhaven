namespace System.Transactions;

/// <summary>Facilitates interaction between <see cref="N:System.Transactions" /> and components that were previously written to interact with MSDTC, COM+, or <see cref="N:System.EnterpriseServices" />. This class cannot be inherited.</summary>
[System.MonoTODO]
public static class TransactionInterop
{
	public static readonly Guid PromoterTypeDtc = new Guid("14229753-FFE1-428D-82B7-DF73045CB8DA");

	/// <summary>Gets an <see cref="T:System.Transactions.IDtcTransaction" /> instance that represents a <see cref="T:System.Transactions.Transaction" />.  </summary>
	/// <returns>An <see cref="T:System.Transactions.IDtcTransaction" /> instance that represents a <see cref="T:System.Transactions.Transaction" />.  The <see cref="T:System.Transactions.IDtcTransaction" /> instance is compatible with the unmanaged form of ITransaction used by MSDTC and with the Managed form of <see cref="T:System.EnterpriseServices.ITransaction" /> used by <see cref="N:System.EnterpriseServices" />.</returns>
	/// <param name="transaction">A <see cref="T:System.Transactions.Transaction" /> instance to be marshaled.</param>
	[System.MonoTODO]
	public static IDtcTransaction GetDtcTransaction(Transaction transaction)
	{
		throw new NotImplementedException();
	}

	/// <summary>Transforms a transaction object into an export transaction cookie.</summary>
	/// <returns>An export transaction cookie representing the specified <see cref="T:System.Transactions.Transaction" /> object.</returns>
	/// <param name="transaction">The <see cref="T:System.Transactions.Transaction" /> object to be marshaled.</param>
	/// <param name="whereabouts">An address that describes the location of the destination transaction manager. This permits two transaction managers to communicate with one another and thereby propagate a transaction from one system to the other.</param>
	[System.MonoTODO]
	public static byte[] GetExportCookie(Transaction transaction, byte[] whereabouts)
	{
		throw new NotImplementedException();
	}

	/// <summary>Generates a <see cref="T:System.Transactions.Transaction" /> from a specified <see cref="T:System.Transactions.IDtcTransaction" />.</summary>
	/// <returns>A <see cref="T:System.Transactions.Transaction" /> instance that represents the given <see cref="T:System.Transactions.IDtcTransaction" />.</returns>
	/// <param name="transactionNative">The <see cref="T:System.Transactions.IDtcTransaction" /> object to be marshaled.</param>
	[System.MonoTODO]
	public static Transaction GetTransactionFromDtcTransaction(IDtcTransaction transactionNative)
	{
		throw new NotImplementedException();
	}

	/// <summary>Generates a <see cref="T:System.Transactions.Transaction" /> from the specified an export cookie.  </summary>
	/// <returns>A <see cref="T:System.Transactions.Transaction" /> from the specified export cookie.</returns>
	/// <param name="cookie">A marshaled form of the transaction object.</param>
	[System.MonoTODO]
	public static Transaction GetTransactionFromExportCookie(byte[] cookie)
	{
		throw new NotImplementedException();
	}

	/// <summary>Generates a <see cref="T:System.Transactions.Transaction" /> instance from the specified transmitter propagation token. </summary>
	/// <returns>A <see cref="T:System.Transactions.Transaction" /> from the specified transmitter propagation token.</returns>
	/// <param name="propagationToken">A propagation token representing a transaction.</param>
	/// <exception cref="T:System.Transactions.TransactionManagerCommunicationException">The deserialization of a transaction fails because the transaction manager cannot be contacted. This may be caused by network firewall or security settings.</exception>
	[System.MonoTODO]
	public static Transaction GetTransactionFromTransmitterPropagationToken(byte[] propagationToken)
	{
		throw new NotImplementedException();
	}

	/// <summary>Generates a propagation token for the specified <see cref="T:System.Transactions.Transaction" />.</summary>
	/// <returns>This method, together with the <see cref="M:System.Transactions.TransactionInterop.GetTransactionFromTransmitterPropagationToken(System.Byte[])" /> method, provide functionality for Transmitter/Receiver propagation, in which the transaction is "pulled" from the remote machine when the latter is called to unmarshal the transaction. For more information on different propagation models, see the Remarks section of the <see cref="T:System.Transactions.TransactionInterop" /> class.</returns>
	/// <param name="transaction">A transaction to be marshaled into a propagation token.</param>
	[System.MonoTODO]
	public static byte[] GetTransmitterPropagationToken(Transaction transaction)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets the Whereabouts of the distributed transaction manager that <see cref="N:System.Transactions" /> uses.  </summary>
	/// <returns>The Whereabouts of the distributed transaction manager that <see cref="N:System.Transactions" /> uses.  </returns>
	[System.MonoTODO]
	public static byte[] GetWhereabouts()
	{
		throw new NotImplementedException();
	}
}
