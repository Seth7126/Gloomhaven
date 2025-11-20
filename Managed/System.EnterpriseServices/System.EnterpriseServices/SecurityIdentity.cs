namespace System.EnterpriseServices;

/// <summary>Contains information that regards an identity in a COM+ call chain.</summary>
public sealed class SecurityIdentity
{
	/// <summary>Gets the name of the user described by this identity.</summary>
	/// <returns>The name of the user described by this identity.</returns>
	public string AccountName
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the authentication level of the user described by this identity.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.AuthenticationOption" /> values.</returns>
	public AuthenticationOption AuthenticationLevel
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the authentication service described by this identity.</summary>
	/// <returns>The authentication service described by this identity.</returns>
	public int AuthenticationService
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the impersonation level of the user described by this identity.</summary>
	/// <returns>A <see cref="T:System.EnterpriseServices.ImpersonationLevelOption" /> value.</returns>
	public ImpersonationLevelOption ImpersonationLevel
	{
		[System.MonoTODO]
		get
		{
			throw new NotImplementedException();
		}
	}

	[System.MonoTODO]
	internal SecurityIdentity()
	{
	}

	[System.MonoTODO]
	internal SecurityIdentity(ISecurityIdentityColl collection)
	{
	}
}
