using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Represents a structure used in the <see cref="T:System.EnterpriseServices.ITransaction" /> interface.</summary>
[ComVisible(false)]
public struct XACTTRANSINFO
{
	/// <summary>Specifies zero. This field is reserved.</summary>
	public int grfRMSupported;

	/// <summary>Specifies zero. This field is reserved.</summary>
	public int grfRMSupportedRetaining;

	/// <summary>Represents a bitmask that indicates which grfTC flags this transaction implementation supports.</summary>
	public int grfTCSupported;

	/// <summary>Specifies zero. This field is reserved.</summary>
	public int grfTCSupportedRetaining;

	/// <summary>Specifies zero. This field is reserved.</summary>
	public int isoFlags;

	/// <summary>Represents the isolation level associated with this transaction object. ISOLATIONLEVEL_UNSPECIFIED indicates that no isolation level was specified.</summary>
	public int isoLevel;

	/// <summary>Represents the unit of work associated with this transaction.</summary>
	public BOID uow;
}
