using System.Configuration;
using System.Transactions.Configuration;

namespace System.Transactions;

/// <summary>Contains methods used for transaction management. This class cannot be inherited.</summary>
public static class TransactionManager
{
	private static DefaultSettingsSection defaultSettings;

	private static MachineSettingsSection machineSettings;

	private static TimeSpan defaultTimeout;

	private static TimeSpan maxTimeout;

	/// <summary>Gets the default timeout interval for new transactions.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that specifies the timeout interval for new transactions.</returns>
	public static TimeSpan DefaultTimeout
	{
		get
		{
			if (defaultSettings != null)
			{
				return defaultSettings.Timeout;
			}
			return defaultTimeout;
		}
	}

	/// <summary>Gets or sets a custom transaction factory.</summary>
	/// <returns>A <see cref="T:System.Transactions.HostCurrentTransactionCallback" /> that contains a custom transaction factory.</returns>
	[System.MonoTODO("Not implemented")]
	public static HostCurrentTransactionCallback HostCurrentCallback
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the default maximum timeout interval for new transactions.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that specifies the maximum timeout interval that is allowed when creating new transactions.</returns>
	public static TimeSpan MaximumTimeout
	{
		get
		{
			if (machineSettings != null)
			{
				return machineSettings.MaxTimeout;
			}
			return maxTimeout;
		}
	}

	/// <summary>Indicates that a distributed transaction has started.</summary>
	public static event TransactionStartedEventHandler DistributedTransactionStarted;

	static TransactionManager()
	{
		defaultTimeout = new TimeSpan(0, 1, 0);
		maxTimeout = new TimeSpan(0, 10, 0);
		defaultSettings = ConfigurationManager.GetSection("system.transactions/defaultSettings") as DefaultSettingsSection;
		machineSettings = ConfigurationManager.GetSection("system.transactions/machineSettings") as MachineSettingsSection;
	}

	/// <summary>Notifies the transaction manager that a resource manager recovering from failure has finished reenlisting in all unresolved transactions.</summary>
	/// <param name="resourceManagerIdentifier">A <see cref="T:System.Guid" /> that uniquely identifies the resource to be recovered from.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="resourceManagerIdentifier" /> parameter is null.</exception>
	[System.MonoTODO("Not implemented")]
	public static void RecoveryComplete(Guid resourceManagerIdentifier)
	{
		throw new NotImplementedException();
	}

	/// <summary>Reenlists a durable participant in a transaction.</summary>
	/// <returns>An <see cref="T:System.Transactions.Enlistment" /> that describes the enlistment.</returns>
	/// <param name="resourceManagerIdentifier">A <see cref="T:System.Guid" /> that uniquely identifies the resource manager.</param>
	/// <param name="recoveryInformation">Contains additional information of recovery information.</param>
	/// <param name="enlistmentNotification">A resource object that implements <see cref="T:System.Transactions.IEnlistmentNotification" /> to receive notifications.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="recoveryInformation" /> is invalid.-or-Transaction Manager information in <paramref name="recoveryInformation" /> does not match the configured transaction manager.-or-<paramref name="RecoveryInformation" /> is not recognized by <see cref="N:System.Transactions" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Transactions.TransactionManager.RecoveryComplete(System.Guid)" /> has already been called for the specified <paramref name="resourceManagerIdentifier" />. The reenlistment is rejected.</exception>
	/// <exception cref="T:System.Transactions.TransactionException">The <paramref name="resourceManagerIdentifier" /> does not match the content of the specified recovery information in <paramref name="recoveryInformation" />. </exception>
	[System.MonoTODO("Not implemented")]
	public static Enlistment Reenlist(Guid resourceManagerIdentifier, byte[] recoveryInformation, IEnlistmentNotification enlistmentNotification)
	{
		throw new NotImplementedException();
	}
}
