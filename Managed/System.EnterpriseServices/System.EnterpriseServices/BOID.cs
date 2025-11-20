using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Represents the unit of work associated with a transaction. This structure is used in <see cref="T:System.EnterpriseServices.XACTTRANSINFO" />.</summary>
[ComVisible(false)]
public struct BOID
{
	/// <summary>Represents an array that contains the data.</summary>
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] rgb;
}
