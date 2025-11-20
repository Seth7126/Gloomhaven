using System.Security.Cryptography.X509Certificates;

namespace System.Security.Policy;

/// <summary>Provides the Authenticode X.509v3 digital signature of a code assembly as evidence for policy evaluation. This class cannot be inherited.</summary>
[Serializable]
public sealed class Publisher : EvidenceBase, IIdentityPermissionFactory
{
	/// <summary>Gets the publisher's Authenticode X.509v3 certificate.</summary>
	/// <returns>The publisher's <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" />.</returns>
	public X509Certificate Certificate => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.Publisher" /> class with the Authenticode X.509v3 certificate containing the publisher's public key.</summary>
	/// <param name="cert">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> that contains the software publisher's public key. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="cert" /> parameter is null. </exception>
	public Publisher(X509Certificate cert)
	{
	}

	/// <summary>Creates an equivalent copy of the <see cref="T:System.Security.Policy.Publisher" />.</summary>
	/// <returns>A new, identical copy of the <see cref="T:System.Security.Policy.Publisher" />.</returns>
	public object Copy()
	{
		return null;
	}

	/// <summary>Creates an identity permission that corresponds to the current instance of the <see cref="T:System.Security.Policy.Publisher" /> class.</summary>
	/// <returns>A <see cref="T:System.Security.Permissions.PublisherIdentityPermission" /> for the specified <see cref="T:System.Security.Policy.Publisher" />.</returns>
	/// <param name="evidence">The <see cref="T:System.Security.Policy.Evidence" /> from which to construct the identity permission. </param>
	public IPermission CreateIdentityPermission(Evidence evidence)
	{
		return null;
	}

	/// <summary>Compares the current <see cref="T:System.Security.Policy.Publisher" /> to the specified object for equivalence.</summary>
	/// <returns>true if the two instances of the <see cref="T:System.Security.Policy.Publisher" /> class are equal; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Security.Policy.Publisher" /> to test for equivalence with the current object. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="o" /> parameter is not a <see cref="T:System.Security.Policy.Publisher" /> object. </exception>
	public override bool Equals(object o)
	{
		return base.Equals(o);
	}

	/// <summary>Gets the hash code of the current <see cref="P:System.Security.Policy.Publisher.Certificate" />.</summary>
	/// <returns>The hash code of the current <see cref="P:System.Security.Policy.Publisher.Certificate" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Returns a string representation of the current <see cref="T:System.Security.Policy.Publisher" />.</summary>
	/// <returns>A representation of the current <see cref="T:System.Security.Policy.Publisher" />.</returns>
	public override string ToString()
	{
		return base.ToString();
	}
}
