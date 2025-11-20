using System.Transactions;

namespace System.EnterpriseServices;

/// <summary>Obtains information about the COM+ object context. This class cannot be inherited.</summary>
public sealed class ContextUtil
{
	private static bool deactivateOnReturn;

	private static TransactionVote myTransactionVote;

	/// <summary>Gets a GUID representing the activity containing the component.</summary>
	/// <returns>The GUID for an activity if the current context is part of an activity; otherwise, GUID_NULL.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static Guid ActivityId
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a GUID for the current application.</summary>
	/// <returns>The GUID for the current application.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows XP or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static Guid ApplicationId
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a GUID for the current application instance.</summary>
	/// <returns>The GUID for the current application instance.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows XP or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static Guid ApplicationInstanceId
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a GUID for the current context.</summary>
	/// <returns>The GUID for the current context.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static Guid ContextId
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the done bit in the COM+ context.</summary>
	/// <returns>true if the object is to be deactivated when the method returns; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static bool DeactivateOnReturn
	{
		get
		{
			return deactivateOnReturn;
		}
		set
		{
			deactivateOnReturn = value;
		}
	}

	/// <summary>Gets a value that indicates whether the current context is transactional.</summary>
	/// <returns>true if the current context is transactional; otherwise, false.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	public static bool IsInTransaction
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value that indicates whether role-based security is active in the current context.</summary>
	/// <returns>true if the current context has security enabled; otherwise, false.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static bool IsSecurityEnabled
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the consistent bit in the COM+ context.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.TransactionVote" /> values, either Commit or Abort.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static TransactionVote MyTransactionVote
	{
		get
		{
			return myTransactionVote;
		}
		set
		{
			myTransactionVote = value;
		}
	}

	/// <summary>Gets a GUID for the current partition.</summary>
	/// <returns>The GUID for the current partition.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows XP or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static Guid PartitionId
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets an object describing the current COM+ DTC transaction.</summary>
	/// <returns>An object that represents the current transaction.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static object Transaction
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the current transaction context.</summary>
	/// <returns>A <see cref="T:System.Transactions.Transaction" /> that represents the current transaction context.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later. </exception>
	public static Transaction SystemTransaction
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the GUID of the current COM+ DTC transaction.</summary>
	/// <returns>A GUID representing the current COM+ DTC transaction, if one exists.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public static Guid TransactionId
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	internal ContextUtil()
	{
	}

	/// <summary>Sets both the consistent bit and the done bit to false in the COM+ context.</summary>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">No COM+ context is available.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static void DisableCommit()
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the consistent bit to true and the done bit to false in the COM+ context.</summary>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">No COM+ context is available. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static void EnableCommit()
	{
		throw new NotImplementedException();
	}

	/// <summary>Returns a named property from the COM+ context.</summary>
	/// <returns>The named property for the context.</returns>
	/// <param name="name">The name of the requested property. </param>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static object GetNamedProperty(string name)
	{
		throw new NotImplementedException();
	}

	/// <summary>Determines whether the caller is in the specified role.</summary>
	/// <returns>true if the caller is in the specified role; otherwise, false.</returns>
	/// <param name="role">The name of the role to check. </param>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static bool IsCallerInRole(string role)
	{
		throw new NotImplementedException();
	}

	/// <summary>Determines whether the serviced component is activated in the default context. Serviced components that do not have COM+ catalog information are activated in the default context.</summary>
	/// <returns>true if the serviced component is activated in the default context; otherwise, false.</returns>
	[System.MonoTODO]
	public static bool IsDefaultContext()
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the consistent bit to false and the done bit to true in the COM+ context.</summary>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static void SetAbort()
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the consistent bit and the done bit to true in the COM+ context.</summary>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static void SetComplete()
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the named property for the COM+ context.</summary>
	/// <param name="name">The name of the property to set. </param>
	/// <param name="value">Object that represents the property value to set.</param>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no COM+ context available. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The platform is not Windows 2000 or later. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static void SetNamedProperty(string name, object value)
	{
		throw new NotImplementedException();
	}
}
