using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Creates an activity to do synchronous or asynchronous batch work that can use COM+ services without needing to create a COM+ component. This class cannot be inherited.</summary>
[ComVisible(false)]
public sealed class Activity
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.Activity" /> class.</summary>
	/// <param name="cfg">A <see cref="T:System.EnterpriseServices.ServiceConfig" /> that contains the configuration information for the services to be used. </param>
	/// <exception cref="T:System.PlatformNotSupportedException">
	///   <see cref="T:System.EnterpriseServices.Activity" /> is not supported on the current platform. </exception>
	[System.MonoTODO]
	public Activity(ServiceConfig cfg)
	{
		throw new NotImplementedException();
	}

	/// <summary>Runs the specified user-defined batch work asynchronously.</summary>
	/// <param name="serviceCall">A <see cref="T:System.EnterpriseServices.IServiceCall" /> object that is used to implement the batch work. </param>
	[System.MonoTODO]
	public void AsynchronousCall(IServiceCall serviceCall)
	{
		throw new NotImplementedException();
	}

	/// <summary>Binds the user-defined work to the current thread.</summary>
	[System.MonoTODO]
	public void BindToCurrentThread()
	{
		throw new NotImplementedException();
	}

	/// <summary>Runs the specified user-defined batch work synchronously.</summary>
	/// <param name="serviceCall">A <see cref="T:System.EnterpriseServices.IServiceCall" /> object that is used to implement the batch work. </param>
	[System.MonoTODO]
	public void SynchronousCall(IServiceCall serviceCall)
	{
		throw new NotImplementedException();
	}

	/// <summary>Unbinds the batch work that is submitted by the <see cref="M:System.EnterpriseServices.Activity.SynchronousCall(System.EnterpriseServices.IServiceCall)" /> or <see cref="M:System.EnterpriseServices.Activity.AsynchronousCall(System.EnterpriseServices.IServiceCall)" /> methods from the thread on which the batch work is running.</summary>
	[System.MonoTODO]
	public void UnbindFromThread()
	{
		throw new NotImplementedException();
	}
}
