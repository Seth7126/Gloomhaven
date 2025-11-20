using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Transactions;

/// <summary>The permission that is demanded by <see cref="N:System.Transactions" /> when management of a transaction is escalated to MSDTC. This class cannot be inherited.</summary>
[Serializable]
public sealed class DistributedTransactionPermission : CodeAccessPermission, IUnrestrictedPermission
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.DistributedTransactionPermission" /> class. </summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values.</param>
	public DistributedTransactionPermission(PermissionState state)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Creates and returns an identical copy of the current permission.</summary>
	/// <returns>A copy of the current permission.</returns>
	public override IPermission Copy()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Reconstructs a permission with a specified state from an XML encoding.</summary>
	/// <param name="securityElement">The XML encoding used to reconstruct the permission.</param>
	public override void FromXml(SecurityElement securityElement)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission and the specified permission.</summary>
	/// <returns>A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.</returns>
	/// <param name="target">A permission to intersect with the current permission. It must be the same type as the current permission.</param>
	public override IPermission Intersect(IPermission target)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Returns a value that indicates whether the current permission is a subset of the specified permission.</summary>
	/// <returns>true if the current <see cref="T:System.Security.IPermission" /> is a subset of the specified <see cref="T:System.Security.IPermission" />; otherwise, false.</returns>
	/// <param name="target">A permission to test for the subset relationship. This permission must be the same type as the current permission.</param>
	public override bool IsSubsetOf(IPermission target)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Returns a value that indicates whether unrestricted access to the resource that is protected by the current permission is allowed.</summary>
	/// <returns>true if unrestricted use of the resource protected by the permission is allowed; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> that contains the XML encoding of the security object, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
