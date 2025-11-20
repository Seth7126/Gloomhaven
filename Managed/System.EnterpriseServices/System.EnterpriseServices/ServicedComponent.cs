namespace System.EnterpriseServices;

/// <summary>Represents the base class of all classes using COM+ services.</summary>
[Serializable]
public abstract class ServicedComponent : ContextBoundObject, IDisposable, IRemoteDispatch, IServicedComponentInfo
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ServicedComponent" /> class.</summary>
	public ServicedComponent()
	{
	}

	/// <summary>Called by the infrastructure when the object is created or allocated from a pool. Override this method to add custom initialization code to objects.</summary>
	[System.MonoTODO]
	protected internal virtual void Activate()
	{
		throw new NotImplementedException();
	}

	/// <summary>This method is called by the infrastructure before the object is put back into the pool. Override this method to vote on whether the object is put back into the pool.</summary>
	/// <returns>true if the serviced component can be pooled; otherwise, false.</returns>
	[System.MonoTODO]
	protected internal virtual bool CanBePooled()
	{
		throw new NotImplementedException();
	}

	/// <summary>Called by the infrastructure just after the constructor is called, passing in the constructor string. Override this method to make use of the construction string value.</summary>
	/// <param name="s">The construction string. </param>
	[System.MonoTODO]
	protected internal virtual void Construct(string s)
	{
		throw new NotImplementedException();
	}

	/// <summary>Called by the infrastructure when the object is about to be deactivated. Override this method to add custom finalization code to objects when just-in-time (JIT) compiled code or object pooling is used.</summary>
	[System.MonoTODO]
	protected internal virtual void Deactivate()
	{
		throw new NotImplementedException();
	}

	/// <summary>Releases all resources used by the <see cref="T:System.EnterpriseServices.ServicedComponent" />.</summary>
	[System.MonoTODO]
	public void Dispose()
	{
		throw new NotImplementedException();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.EnterpriseServices.ServicedComponent" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; otherwise, false to release only unmanaged resources. </param>
	[System.MonoTODO]
	protected virtual void Dispose(bool disposing)
	{
		throw new NotImplementedException();
	}

	/// <summary>Finalizes the object and removes the associated COM+ reference.</summary>
	/// <param name="sc">The object to dispose. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlThread" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static void DisposeObject(ServicedComponent sc)
	{
		throw new NotImplementedException();
	}

	/// <summary>Ensures that, in the COM+ context, the <see cref="T:System.EnterpriseServices.ServicedComponent" /> class object's done bit is set to true after a remote method invocation</summary>
	/// <returns>A string converted from a response object that implements the <see cref="T:System.Runtime.Remoting.Messaging.IMethodReturnMessage" /> interface.</returns>
	/// <param name="s">A string to be converted into a request object that implements the <see cref="T:System.Runtime.Remoting.Messaging.IMessage" /> interface.</param>
	[System.MonoTODO]
	string IRemoteDispatch.RemoteDispatchAutoDone(string s)
	{
		throw new NotImplementedException();
	}

	/// <summary>Does not ensure that, in the COM+ context, the <see cref="T:System.EnterpriseServices.ServicedComponent" /> class object's done bit is set to true after a remote method invocation.</summary>
	/// <returns>A string converted from a response object implementing the <see cref="T:System.Runtime.Remoting.Messaging.IMethodReturnMessage" /> interface.</returns>
	/// <param name="s">A string to be converted into a request object implementing the <see cref="T:System.Runtime.Remoting.Messaging.IMessage" /> interface.</param>
	[System.MonoTODO]
	string IRemoteDispatch.RemoteDispatchNotAutoDone(string s)
	{
		throw new NotImplementedException();
	}

	/// <summary>Obtains information about the <see cref="T:System.EnterpriseServices.ServicedComponent" /> class instance.</summary>
	/// <param name="infoMask">A bitmask where 0x00000001 is a key for the serviced component's process ID, 0x00000002 is a key for the application domain ID, and 0x00000004 is a key for the serviced component's remote URI.</param>
	/// <param name="infoArray">A string array that may contain any or all of the following, in order: the serviced component's process ID, the application domain ID, and the serviced component's remote URI.</param>
	[System.MonoTODO]
	void IServicedComponentInfo.GetComponentInfo(ref int infoMask, out string[] infoArray)
	{
		throw new NotImplementedException();
	}
}
