using System.Collections;
using System.Runtime.InteropServices;
using Unity;

namespace System.Security.Policy;

/// <summary>Represents the enumerator for <see cref="T:System.Security.Policy.ApplicationTrust" /> objects in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
[ComVisible(true)]
public sealed class ApplicationTrustEnumerator : IEnumerator
{
	private ApplicationTrustCollection trusts;

	private int current;

	/// <summary>Gets the current <see cref="T:System.Security.Policy.ApplicationTrust" /> object in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
	/// <returns>The current <see cref="T:System.Security.Policy.ApplicationTrust" /> in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" />.</returns>
	public ApplicationTrust Current => trusts[current];

	/// <summary>Gets the current <see cref="T:System.Object" /> in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
	/// <returns>The current <see cref="T:System.Object" /> in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" />.</returns>
	object IEnumerator.Current => trusts[current];

	internal ApplicationTrustEnumerator(ApplicationTrustCollection atc)
	{
		trusts = atc;
		current = -1;
	}

	/// <summary>Resets the enumerator to the beginning of the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
	public void Reset()
	{
		current = -1;
	}

	/// <summary>Moves to the next element in the <see cref="T:System.Security.Policy.ApplicationTrustCollection" /> collection.</summary>
	/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	public bool MoveNext()
	{
		if (current == trusts.Count - 1)
		{
			return false;
		}
		current++;
		return true;
	}

	internal ApplicationTrustEnumerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
