namespace System.EnterpriseServices;

/// <summary>Wraps the COM+ ByotServerEx class and the COM+ DTC interfaces ICreateWithTransactionEx and ICreateWithTipTransactionEx. This class cannot be inherited.</summary>
public sealed class BYOT
{
	private BYOT()
	{
	}

	/// <summary>Creates an object that is enlisted within a manual transaction using the Transaction Internet Protocol (TIP).</summary>
	/// <returns>The requested transaction.</returns>
	/// <param name="url">A TIP URL that specifies a transaction. </param>
	/// <param name="t">The type. </param>
	[System.MonoTODO]
	public static object CreateWithTipTransaction(string url, Type t)
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates an object that is enlisted within a manual transaction.</summary>
	/// <returns>The requested transaction.</returns>
	/// <param name="transaction">The <see cref="T:System.EnterpriseServices.ITransaction" /> or <see cref="T:System.Transactions.Transaction" /> object that specifies a transaction. </param>
	/// <param name="t">The specified type. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static object CreateWithTransaction(object transaction, Type t)
	{
		throw new NotImplementedException();
	}
}
