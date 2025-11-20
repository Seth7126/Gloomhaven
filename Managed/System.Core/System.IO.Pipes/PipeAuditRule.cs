using System.Security.AccessControl;
using System.Security.Principal;

namespace System.IO.Pipes;

/// <summary>Represents an abstraction of an access control entry (ACE) that defines an audit rule for a pipe.</summary>
public sealed class PipeAuditRule : AuditRule
{
	/// <summary>Gets the <see cref="T:System.IO.Pipes.PipeAccessRights" /> flags that are associated with the current <see cref="T:System.IO.Pipes.PipeAuditRule" /> object.</summary>
	/// <returns>A bitwise combination of the <see cref="T:System.IO.Pipes.PipeAccessRights" /> values. </returns>
	public PipeAccessRights PipeAccessRights => PipeAccessRule.RightsFromAccessMask(base.AccessMask);

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.PipeAuditRule" /> class for a user account specified in a <see cref="T:System.Security.Principal.IdentityReference" /> object.</summary>
	/// <param name="identity">An <see cref="T:System.Security.Principal.IdentityReference" /> object that encapsulates a reference to a user account.</param>
	/// <param name="rights">One of the <see cref="T:System.IO.Pipes.PipeAccessRights" /> values that specifies the type of operation associated with the access rule.</param>
	/// <param name="flags">One of the <see cref="T:System.Security.AccessControl.AuditFlags" /> values that specifies when to perform auditing.</param>
	public PipeAuditRule(IdentityReference identity, PipeAccessRights rights, AuditFlags flags)
		: this(identity, AccessMaskFromRights(rights), isInherited: false, flags)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.IO.Pipes.PipeAuditRule" /> class for a named user account.</summary>
	/// <param name="identity">The name of the user account.</param>
	/// <param name="rights">One of the <see cref="T:System.IO.Pipes.PipeAccessRights" /> values that specifies the type of operation associated with the access rule.</param>
	/// <param name="flags">One of the <see cref="T:System.Security.AccessControl.AuditFlags" /> values that specifies when to perform auditing.</param>
	public PipeAuditRule(string identity, PipeAccessRights rights, AuditFlags flags)
		: this(new NTAccount(identity), AccessMaskFromRights(rights), isInherited: false, flags)
	{
	}

	internal PipeAuditRule(IdentityReference identity, int accessMask, bool isInherited, AuditFlags flags)
		: base(identity, accessMask, isInherited, InheritanceFlags.None, PropagationFlags.None, flags)
	{
	}

	private static int AccessMaskFromRights(PipeAccessRights rights)
	{
		if (rights < (PipeAccessRights)0 || rights > (PipeAccessRights.FullControl | PipeAccessRights.AccessSystemSecurity))
		{
			throw new ArgumentOutOfRangeException("rights", "Invalid PipeAccessRights value.");
		}
		return (int)rights;
	}
}
