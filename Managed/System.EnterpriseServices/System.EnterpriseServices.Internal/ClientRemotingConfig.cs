namespace System.EnterpriseServices.Internal;

/// <summary>Defines a static <see cref="M:System.EnterpriseServices.Internal.ClientRemotingConfig.Write(System.String,System.String,System.String,System.String,System.String,System.String,System.String,System.String)" /> method that creates a client remoting configuration file for a client type library.</summary>
public class ClientRemotingConfig
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.Internal.ClientRemotingConfig" /> class.</summary>
	[System.MonoTODO]
	public ClientRemotingConfig()
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a client remoting configuration file for a client type library in a SOAP-enabled COM+ application.</summary>
	/// <returns>true if the client remoting configuration file was successfully created; otherwise false.</returns>
	/// <param name="DestinationDirectory">The folder in which to create the configuration file.</param>
	/// <param name="VRoot">The name of the virtual root.</param>
	/// <param name="BaseUrl">The base URL that contains the virtual root.</param>
	/// <param name="AssemblyName">The display name of the assembly that contains common language runtime (CLR) metadata corresponding to the type library.</param>
	/// <param name="TypeName">The fully qualified name of the assembly that contains CLR metadata corresponding to the type library.</param>
	/// <param name="ProgId">The programmatic identifier of the class.</param>
	/// <param name="Mode">The activation mode.</param>
	/// <param name="Transport">Not used. Specify null for this parameter.</param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration" />
	/// </PermissionSet>
	[System.MonoTODO]
	public static bool Write(string DestinationDirectory, string VRoot, string BaseUrl, string AssemblyName, string TypeName, string ProgId, string Mode, string Transport)
	{
		throw new NotImplementedException();
	}
}
