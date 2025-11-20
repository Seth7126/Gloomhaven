using System.Security;
using System.Security.Permissions;

namespace System.Drawing.Printing;

/// <summary>Controls access to printers. This class cannot be inherited.</summary>
[Serializable]
public sealed class PrintingPermission : CodeAccessPermission, IUnrestrictedPermission
{
	/// <summary>Gets or sets the code's level of printing access.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Printing.PrintingPermissionLevel" /> values.</returns>
	public PrintingPermissionLevel Level { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrintingPermission" /> class with the level of printing access specified.</summary>
	/// <param name="printingLevel">One of the <see cref="T:System.Drawing.Printing.PrintingPermissionLevel" /> values. </param>
	public PrintingPermission(PrintingPermissionLevel printingLevel)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrintingPermission" /> class with either fully restricted or unrestricted access, as specified.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not a valid <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public PrintingPermission(PermissionState state)
	{
	}

	/// <summary>Creates and returns an identical copy of the current permission object.</summary>
	/// <returns>A copy of the current permission object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission Copy()
	{
		return null;
	}

	/// <summary>Reconstructs a security object with a specified state from an XML encoding.</summary>
	/// <param name="esd">The XML encoding to use to reconstruct the security object. </param>
	public override void FromXml(SecurityElement element)
	{
	}

	/// <summary>Creates and returns a permission that is the intersection of the current permission object and a target permission object.</summary>
	/// <returns>A new permission object that represents the intersection of the current object and the specified target. This object is null if the intersection is empty.</returns>
	/// <param name="target">A permission object of the same type as the current permission object. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is an object that is not of the same type as the current permission object. </exception>
	public override IPermission Intersect(IPermission target)
	{
		return null;
	}

	/// <summary>Determines whether the current permission object is a subset of the specified permission.</summary>
	/// <returns>true if the current permission object is a subset of <paramref name="target" />; otherwise, false.</returns>
	/// <param name="target">A permission object that is to be tested for the subset relationship. This object must be of the same type as the current permission object. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is an object that is not of the same type as the current permission object. </exception>
	public override bool IsSubsetOf(IPermission target)
	{
		return false;
	}

	/// <summary>Gets a value indicating whether the permission is unrestricted.</summary>
	/// <returns>true if permission is unrestricted; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return false;
	}

	/// <summary>Creates an XML encoding of the security object and its current state.</summary>
	/// <returns>An XML encoding of the security object, including any state information.</returns>
	public override SecurityElement ToXml()
	{
		return null;
	}

	/// <summary>Creates a permission that combines the permission object and the target permission object.</summary>
	/// <returns>A new permission object that represents the union of the current permission object and the specified permission object.</returns>
	/// <param name="target">A permission object of the same type as the current permission object. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is an object that is not of the same type as the current permission object. </exception>
	public override IPermission Union(IPermission target)
	{
		return null;
	}
}
