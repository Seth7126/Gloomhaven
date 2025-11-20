using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal;

/// <summary>Activates SOAP-enabled COM+ application proxies from a client.</summary>
[Guid("ecabafd2-7f19-11d2-978e-0000f8757e2a")]
public interface IClrObjectFactory
{
	/// <summary>Activates a remote assembly through .NET remoting, using the assembly's configuration file.</summary>
	/// <returns>An instance of the <see cref="T:System.Object" /> representing the type, with culture, arguments, and binding and activation attributes set to null, or null if the <paramref name="type" /> parameter is not found.</returns>
	/// <param name="assembly">The name of the assembly to activate. </param>
	/// <param name="type">The name of the type to activate. </param>
	/// <param name="mode">Not used. </param>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call chain does not have permission to access unmanaged code. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">The class is not registered. </exception>
	[DispId(1)]
	[return: MarshalAs(UnmanagedType.IDispatch)]
	object CreateFromAssembly(string assembly, string type, string mode);

	/// <summary>Activates a remote assembly through .NET remoting, using the remote assembly's mailbox. Currently not implemented; throws a <see cref="T:System.Runtime.InteropServices.COMException" /> if called.</summary>
	/// <returns>This method throws an exception if called.</returns>
	/// <param name="Mailbox">A mailbox on the Web service. </param>
	/// <param name="Mode">Not used. </param>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">Simple Mail Transfer Protocol (SMTP) is not implemented. </exception>
	[DispId(4)]
	[return: MarshalAs(UnmanagedType.IDispatch)]
	object CreateFromMailbox(string Mailbox, string Mode);

	/// <summary>Activates a remote assembly through .NET remoting, using the virtual root URL of the remote assembly.</summary>
	/// <returns>An instance of the <see cref="T:System.Object" /> representing the type, with culture, arguments, and binding and activation attributes set to null, or null if the assembly identified by the <paramref name="VrootUrl" /> parameter is not found.</returns>
	/// <param name="VrootUrl">The virtual root URL of the remote object. </param>
	/// <param name="Mode">Not used. </param>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call chain does not have permission to access unmanaged code. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">The thread token could not be opened. </exception>
	[DispId(2)]
	[return: MarshalAs(UnmanagedType.IDispatch)]
	object CreateFromVroot(string VrootUrl, string Mode);

	/// <summary>Activates a remote assembly through .NET remoting, using the Web Services Description Language (WSDL) of the XML Web service.</summary>
	/// <returns>An instance of the <see cref="T:System.Object" /> representing the type, with culture, arguments, and binding and activation attributes set to null, or null if the assembly identified by the <paramref name="WsdlUrl" /> parameter is not found.</returns>
	/// <param name="WsdlUrl">The WSDL URL of the Web service. </param>
	/// <param name="Mode">Not used. </param>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call chain does not have permission to access unmanaged code. </exception>
	/// <exception cref="T:System.Runtime.InteropServices.COMException">The thread token could not be opened. </exception>
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.IDispatch)]
	object CreateFromWsdl(string WsdlUrl, string Mode);
}
