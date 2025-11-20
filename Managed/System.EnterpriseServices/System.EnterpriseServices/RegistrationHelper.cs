using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Installs and configures assemblies in the COM+ catalog. This class cannot be inherited.</summary>
[Guid("89a86e7b-c229-4008-9baa-2f5c8411d7e0")]
public sealed class RegistrationHelper : MarshalByRefObject, IRegistrationHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.RegistrationHelper" /> class.</summary>
	public RegistrationHelper()
	{
	}

	/// <summary>Installs the named assembly in a COM+ application.</summary>
	/// <param name="assembly">The file name of the assembly to install. </param>
	/// <param name="application">The name of the COM+ application to install into. This parameter can be null. If the parameter is null and the assembly contains a <see cref="T:System.EnterpriseServices.ApplicationNameAttribute" />, then the attribute is used. Otherwise, the name of the application is generated based on the name of the assembly, then is returned.</param>
	/// <param name="tlb">The name of the output Type Library Exporter (Tlbexp.exe) file, or a string that contains null if the registration helper is expected to generate the name. The actual name used is placed in the parameter on call completion. </param>
	/// <param name="installFlags">A bitwise combination of the <see cref="T:System.EnterpriseServices.InstallationFlags" /> values. </param>
	/// <exception cref="T:System.EnterpriseServices.RegistrationException">The input assembly does not have a strong name. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void InstallAssembly(string assembly, ref string application, ref string tlb, InstallationFlags installFlags)
	{
		application = string.Empty;
		tlb = string.Empty;
		InstallAssembly(assembly, ref application, null, ref tlb, installFlags);
	}

	/// <summary>Installs the named assembly in a COM+ application.</summary>
	/// <param name="assembly">The file name of the assembly to install. </param>
	/// <param name="application">The name of the COM+ application to install into. This parameter can be null. If the parameter is null and the assembly contains a <see cref="T:System.EnterpriseServices.ApplicationNameAttribute" />, then the attribute is used. Otherwise, the name of the application is generated based on the name of the assembly, then is returned.</param>
	/// <param name="partition">The name of the partition. This parameter can be null. </param>
	/// <param name="tlb">The name of the output Type Library Exporter (Tlbexp.exe) file, or a string that contains null if the registration helper is expected to generate the name. The actual name used is placed in the parameter on call completion. </param>
	/// <param name="installFlags">A bitwise combination of the <see cref="T:System.EnterpriseServices.InstallationFlags" /> values. </param>
	/// <exception cref="T:System.EnterpriseServices.RegistrationException">The input assembly does not have a strong name. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void InstallAssembly(string assembly, ref string application, string partition, ref string tlb, InstallationFlags installFlags)
	{
		throw new NotImplementedException();
	}

	/// <summary>Installs the named assembly in a COM+ application.</summary>
	/// <param name="regConfig">A <see cref="T:System.EnterpriseServices.RegistrationConfig" /> identifying the assembly to install. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void InstallAssemblyFromConfig([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig)
	{
		throw new NotImplementedException();
	}

	/// <summary>Uninstalls the assembly from the given application.</summary>
	/// <param name="assembly">The file name of the assembly to uninstall. </param>
	/// <param name="application">If this name is not null, it is the name of the application that contains the components in the assembly. </param>
	/// <exception cref="T:System.EnterpriseServices.RegistrationException">The input assembly does not have a strong name. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void UninstallAssembly(string assembly, string application)
	{
		UninstallAssembly(assembly, application, null);
	}

	/// <summary>Uninstalls the assembly from the given application.</summary>
	/// <param name="assembly">The file name of the assembly to uninstall. </param>
	/// <param name="application">If this name is not null, it is the name of the application that contains the components in the assembly. </param>
	/// <param name="partition">The name of the partition. This parameter can be null. </param>
	/// <exception cref="T:System.EnterpriseServices.RegistrationException">The input assembly does not have a strong name. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void UninstallAssembly(string assembly, string application, string partition)
	{
		throw new NotImplementedException();
	}

	/// <summary>Uninstalls the assembly from the given application.</summary>
	/// <param name="regConfig">A <see cref="T:System.EnterpriseServices.RegistrationConfig" /> identifying the assembly to uninstall. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void UninstallAssemblyFromConfig([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig)
	{
		throw new NotImplementedException();
	}
}
