namespace System.Security.Permissions;

/// <summary>Specifies whether a permission should have all or no access to resources at creation.</summary>
public enum PermissionState
{
	/// <summary>No access to the resource protected by the permission.</summary>
	None,
	/// <summary>Full access to the resource protected by the permission.</summary>
	Unrestricted
}
