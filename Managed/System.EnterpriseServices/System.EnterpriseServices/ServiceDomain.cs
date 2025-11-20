using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Allows a code segment identified by <see cref="M:System.EnterpriseServices.ServiceDomain.Enter(System.EnterpriseServices.ServiceConfig)" /> and <see cref="M:System.EnterpriseServices.ServiceDomain.Leave" /> to run in its own context and behave as if it were a method that is called on an object created within the context. This class cannot be inherited.</summary>
[ComVisible(false)]
public sealed class ServiceDomain
{
	private ServiceDomain()
	{
	}

	/// <summary>Creates the context specified by the <see cref="T:System.EnterpriseServices.ServiceConfig" /> object and pushes it onto the context stack to become the current context.</summary>
	/// <param name="cfg">A <see cref="T:System.EnterpriseServices.ServiceConfig" /> that contains the configuration information for the services to be used within the enclosed code. </param>
	/// <exception cref="T:System.PlatformNotSupportedException">
	///   <see cref="T:System.EnterpriseServices.ServiceConfig" /> is not supported on the current platform. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static void Enter(ServiceConfig cfg)
	{
		throw new NotImplementedException();
	}

	/// <summary>Triggers the server and then the client side policies as if a method call were returning. The current context is then popped from the context stack, and the context that was running when <see cref="M:System.EnterpriseServices.ServiceDomain.Enter(System.EnterpriseServices.ServiceConfig)" /> was called becomes the current context.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.TransactionStatus" /> values.</returns>
	/// <exception cref="T:System.PlatformNotSupportedException">
	///   <see cref="T:System.EnterpriseServices.ServiceConfig" /> is not supported on the current platform. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static TransactionStatus Leave()
	{
		throw new NotImplementedException();
	}
}
