using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Specifies the release mode for the properties in the new shared property group.</summary>
[Serializable]
[ComVisible(false)]
public enum PropertyReleaseMode
{
	/// <summary>The property group is not destroyed until the process in which it was created has terminated.</summary>
	Process = 1,
	/// <summary>When all clients have released their references on the property group, the property group is automatically destroyed. This is the default COM mode.</summary>
	Standard = 0
}
