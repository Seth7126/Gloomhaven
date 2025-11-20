using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Specifies the mode for accessing shared properties in the shared property group manager.</summary>
[Serializable]
[ComVisible(false)]
public enum PropertyLockMode
{
	/// <summary>Locks all the properties in the shared property group for exclusive use by the caller, as long as the caller's current method is executing.</summary>
	Method = 1,
	/// <summary>Locks a property during a get or set, assuring that every get or set operation on a shared property is atomic.</summary>
	SetGet = 0
}
