using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Indicates whether to create a new context based on the current context or on the information in <see cref="T:System.EnterpriseServices.ServiceConfig" />.</summary>
[Serializable]
[ComVisible(false)]
public enum InheritanceOption
{
	/// <summary>The new context is created from the existing context. <see cref="F:System.EnterpriseServices.InheritanceOption.Inherit" /> is the default value for <see cref="P:System.EnterpriseServices.ServiceConfig.Inheritance" />.</summary>
	Inherit,
	/// <summary>The new context is created from the default context.</summary>
	Ignore
}
