using System.Security;
using System.Security.Permissions;

namespace System.Drawing.Printing;

/// <summary>Allows declarative printing permission checks.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class PrintingPermissionAttribute : CodeAccessSecurityAttribute
{
	/// <summary>Gets or sets the type of printing allowed.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Printing.PrintingPermissionLevel" /> values.</returns>
	/// <exception cref="T:System.ArgumentException">The value is not one of the <see cref="T:System.Drawing.Printing.PrintingPermissionLevel" /> values. </exception>
	public PrintingPermissionLevel Level { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PrintingPermissionAttribute" /> class.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public PrintingPermissionAttribute(SecurityAction action)
		: base(action)
	{
	}

	/// <summary>Creates the permission based on the requested access levels, which are set through the <see cref="P:System.Drawing.Printing.PrintingPermissionAttribute.Level" /> property on the attribute.</summary>
	/// <returns>An <see cref="T:System.Security.IPermission" /> that represents the created permission.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override IPermission CreatePermission()
	{
		return null;
	}
}
