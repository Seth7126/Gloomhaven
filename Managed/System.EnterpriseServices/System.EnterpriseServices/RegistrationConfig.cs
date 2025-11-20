using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Provides configuration information for installing assemblies into the COM+ catalog.</summary>
[Serializable]
[Guid("36dcda30-dc3b-4d93-be42-90b2d74c64e7")]
public class RegistrationConfig
{
	/// <summary>Gets or sets the name of the COM+ application in which the assembly is to be installed.</summary>
	/// <returns>The name of the COM+ application in which the assembly is to be installed.</returns>
	[System.MonoTODO]
	public string Application
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the name of the root directory of the application.</summary>
	/// <returns>The name of the root directory of the application.</returns>
	[System.MonoTODO]
	public string ApplicationRootDirectory
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the file name of the assembly to install.</summary>
	/// <returns>The file name of the assembly to install.</returns>
	[System.MonoTODO]
	public string AssemblyFile
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets a flag that indicates how to install the assembly.</summary>
	/// <returns>One of the <see cref="T:System.EnterpriseServices.InstallationFlags" /> values.</returns>
	[System.MonoTODO]
	public InstallationFlags InstallationFlags
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the name of the COM+ partition.</summary>
	/// <returns>The name of the COM+ partition.</returns>
	[System.MonoTODO]
	public string Partition
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets or sets the name of the output Type Library Exporter (Tlbexp.exe) file.</summary>
	/// <returns>The name of the output Type Library Exporter (Tlbexp.exe) file.</returns>
	[System.MonoTODO]
	public string TypeLibrary
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.RegistrationConfig" /> class.</summary>
	[System.MonoTODO]
	public RegistrationConfig()
	{
		throw new NotImplementedException();
	}
}
