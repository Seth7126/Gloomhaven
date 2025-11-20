namespace System.EnterpriseServices;

/// <summary>Flags used with the <see cref="T:System.EnterpriseServices.RegistrationHelper" /> class.</summary>
[Serializable]
[Flags]
public enum InstallationFlags
{
	/// <summary>Should not be used.</summary>
	Configure = 0x400,
	/// <summary>Configures components only, do not configure methods or interfaces.</summary>
	ConfigureComponentsOnly = 0x10,
	/// <summary>Creates the target application. An error occurs if the target already exists.</summary>
	CreateTargetApplication = 2,
	/// <summary>Do the default installation, which configures, installs, and registers, and assumes that the application already exists.</summary>
	Default = 0,
	/// <summary>Do not export the type library; one can be found either by the generated or supplied type library name.</summary>
	ExpectExistingTypeLib = 1,
	/// <summary>Creates the application if it does not exist; otherwise use the existing application.</summary>
	FindOrCreateTargetApplication = 4,
	/// <summary>Should not be used.</summary>
	Install = 0x200,
	/// <summary>If using an existing application, ensures that the properties on this application match those in the assembly.</summary>
	ReconfigureExistingApplication = 8,
	/// <summary>Should not be used.</summary>
	Register = 0x100,
	/// <summary>When alert text is encountered, writes it to the Console.</summary>
	ReportWarningsToConsole = 0x20
}
