using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal;

/// <summary>Processes authenticated, encrypted SOAP components on servers. This class cannot be inherited.</summary>
[Guid("F6B6768F-F99E-4152-8ED2-0412F78517FB")]
public sealed class SoapServerTlb : ISoapServerTlb
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.Internal.SoapServerTlb" /> class.</summary>
	[System.MonoTODO]
	public SoapServerTlb()
	{
		throw new NotImplementedException();
	}

	/// <summary>Adds the entries for a server type library to the Web.config and Default.disco files, depending on security options, and generates a proxy if necessary.</summary>
	/// <param name="progId">The programmatic identifier of the class. </param>
	/// <param name="classId">The class identifier (CLSID) for the type library. </param>
	/// <param name="interfaceId">The IID for the type library. </param>
	/// <param name="srcTlbPath">The path for the file containing the type library. </param>
	/// <param name="rootWebServer">The root Web server. </param>
	/// <param name="inBaseUrl">The base URL that contains the virtual root. </param>
	/// <param name="inVirtualRoot">The name of the virtual root. </param>
	/// <param name="clientActivated">true if client activated; otherwise, false. </param>
	/// <param name="wellKnown">true if well-known; otherwise, false. </param>
	/// <param name="discoFile">true if a discovery file; otherwise, false.</param>
	/// <param name="operation">The operation to perform. Specify either "delete" or an empty string. </param>
	/// <param name="strAssemblyName">When this method returns, contains the name of the assembly. </param>
	/// <param name="typeName">When this method returns, contains the type of the assembly. </param>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call chain does not have permission to access unmanaged code. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The SOAP utility is not available. </exception>
	/// <exception cref="T:System.EnterpriseServices.ServicedComponentException">The call to get the system directory failed. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlAppDomain, RemotingConfiguration" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void AddServerTlb(string progId, string classId, string interfaceId, string srcTlbPath, string rootWebServer, string inBaseUrl, string inVirtualRoot, string clientActivated, string wellKnown, string discoFile, string operation, out string strAssemblyName, out string typeName)
	{
		throw new NotImplementedException();
	}

	/// <summary>Removes entries for a server type library from the Web.config and Default.disco files, depending on security options.</summary>
	/// <param name="progId">The programmatic identifier of the class. </param>
	/// <param name="classId">The class identifier (CLSID) for the type library. </param>
	/// <param name="interfaceId">The IID for the type library. </param>
	/// <param name="srcTlbPath">The path for the file containing the type library. </param>
	/// <param name="rootWebServer">The root Web server. </param>
	/// <param name="baseUrl">The base URL that contains the virtual root. </param>
	/// <param name="virtualRoot">The name of the virtual root. </param>
	/// <param name="operation">Not used. Specify null for this parameter.</param>
	/// <param name="assemblyName">The name of the assembly. </param>
	/// <param name="typeName">The type of the assembly. </param>
	/// <exception cref="T:System.Security.SecurityException">A caller in the call chain does not have permission to access unmanaged code. </exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The SOAP utility is not available. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlAppDomain, RemotingConfiguration" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Diagnostics.EventLogPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void DeleteServerTlb(string progId, string classId, string interfaceId, string srcTlbPath, string rootWebServer, string baseUrl, string virtualRoot, string operation, string assemblyName, string typeName)
	{
		throw new NotImplementedException();
	}
}
