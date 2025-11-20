namespace System.EnterpriseServices;

/// <summary>Describes the chain of callers leading up to the current method call.</summary>
public sealed class SecurityCallContext
{
	/// <summary>Gets a <see cref="T:System.EnterpriseServices.SecurityCallers" /> object that describes the caller.</summary>
	/// <returns>The <see cref="T:System.EnterpriseServices.SecurityCallers" /> object that describes the caller.</returns>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">There is no security context. </exception>
	public SecurityCallers Callers
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a <see cref="T:System.EnterpriseServices.SecurityCallContext" /> object that describes the security call context.</summary>
	/// <returns>The <see cref="T:System.EnterpriseServices.SecurityCallContext" /> object that describes the security call context.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static SecurityCallContext CurrentCall
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a <see cref="T:System.EnterpriseServices.SecurityIdentity" /> object that describes the direct caller of this method.</summary>
	/// <returns>A <see cref="T:System.EnterpriseServices.SecurityIdentity" /> value.</returns>
	public SecurityIdentity DirectCaller
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Determines whether security checks are enabled in the current context.</summary>
	/// <returns>true if security checks are enabled in the current context; otherwise, false.</returns>
	public bool IsSecurityEnabled
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the MinAuthenticationLevel value from the ISecurityCallContext collection in COM+.</summary>
	/// <returns>The MinAuthenticationLevel value from the ISecurityCallContext collection in COM+.</returns>
	public int MinAuthenticationLevel
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the NumCallers value from the ISecurityCallContext collection in COM+.</summary>
	/// <returns>The NumCallers value from the ISecurityCallContext collection in COM+.</returns>
	public int NumCallers
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a <see cref="T:System.EnterpriseServices.SecurityIdentity" /> that describes the original caller.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.SecurityIdentity" /> values.</returns>
	public SecurityIdentity OriginalCaller
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	internal SecurityCallContext()
	{
	}

	internal SecurityCallContext(ISecurityCallContext context)
	{
	}

	/// <summary>Verifies that the direct caller is a member of the specified role.</summary>
	/// <returns>true if the direct caller is a member of the specified role; otherwise, false.</returns>
	/// <param name="role">The specified role. </param>
	[System.MonoTODO]
	public bool IsCallerInRole(string role)
	{
		throw new NotImplementedException();
	}

	/// <summary>Verifies that the specified user is in the specified role.</summary>
	/// <returns>true if the specified user is a member of the specified role; otherwise, false.</returns>
	/// <param name="user">The specified user. </param>
	/// <param name="role">The specified role. </param>
	[System.MonoTODO]
	public bool IsUserInRole(string user, string role)
	{
		throw new NotImplementedException();
	}
}
