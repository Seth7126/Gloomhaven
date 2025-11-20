namespace System.EnterpriseServices;

/// <summary>Stores objects in the current transaction. This class cannot be inherited.</summary>
public sealed class ResourcePool
{
	/// <summary>Represents the method that handles the ending of a transaction.</summary>
	/// <param name="resource">The object that is passed back to the delegate. </param>
	public delegate void TransactionEndDelegate(object resource);

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ResourcePool" /> class.</summary>
	/// <param name="cb">A <see cref="T:System.EnterpriseServices.ResourcePool.TransactionEndDelegate" />, that is called when a transaction is finished. All items currently stored in the transaction are handed back to the user through the delegate. </param>
	[System.MonoTODO]
	public ResourcePool(TransactionEndDelegate cb)
	{
	}

	/// <summary>Gets a resource from the current transaction.</summary>
	/// <returns>The resource object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public object GetResource()
	{
		throw new NotImplementedException();
	}

	/// <summary>Adds a resource to the current transaction.</summary>
	/// <returns>true if the resource object was added to the pool; otherwise, false.</returns>
	/// <param name="resource">The resource to add. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public bool PutResource(object resource)
	{
		throw new NotImplementedException();
	}
}
