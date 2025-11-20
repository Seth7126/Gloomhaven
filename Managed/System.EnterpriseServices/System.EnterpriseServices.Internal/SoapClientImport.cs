using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal;

/// <summary>Imports authenticated, encrypted SOAP client proxies. This class cannot be inherited.</summary>
[Guid("346D5B9F-45E1-45c0-AADF-1B7D221E9063")]
public sealed class SoapClientImport : ISoapClientImport
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.Internal.SoapClientImport" /> class.</summary>
	[System.MonoTODO]
	public SoapClientImport()
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a .NET remoting client configuration file that includes security and authentication options.</summary>
	/// <param name="progId">The programmatic identifier of the class. If an empty string (""), this method returns without doing anything. </param>
	/// <param name="virtualRoot">The name of the virtual root. </param>
	/// <param name="baseUrl">The base URL that contains the virtual root. </param>
	/// <param name="authentication">The type of ASP.NET authentication to use. </param>
	/// <param name="assemblyName">The name of the assembly. </param>
	/// <param name="typeName">The name of the type. </param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void ProcessClientTlbEx(string progId, string virtualRoot, string baseUrl, string authentication, string assemblyName, string typeName)
	{
		throw new NotImplementedException();
	}
}
