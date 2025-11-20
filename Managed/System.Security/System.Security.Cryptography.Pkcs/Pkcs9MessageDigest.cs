using System.Security.Cryptography.Asn1;
using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9MessageDigest" /> class defines the message digest of a CMS/PKCS #7 message.</summary>
public sealed class Pkcs9MessageDigest : Pkcs9AttributeObject
{
	private volatile byte[] _lazyMessageDigest;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9MessageDigest.MessageDigest" /> property retrieves the message digest.</summary>
	/// <returns>An array of byte values that contains the message digest.</returns>
	public byte[] MessageDigest => _lazyMessageDigest ?? (_lazyMessageDigest = Decode(base.RawData));

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9MessageDigest.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9MessageDigest" /> class.</summary>
	public Pkcs9MessageDigest()
		: base(Oid.FromOidValue("1.2.840.113549.1.9.4", OidGroup.ExtensionOrAttribute))
	{
	}

	internal Pkcs9MessageDigest(ReadOnlySpan<byte> signatureDigest)
	{
		using AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.DER);
		asnWriter.WriteOctetString(signatureDigest);
		base.RawData = asnWriter.Encode();
	}

	/// <summary>Copies information from an <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object from which to copy information.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		base.CopyFrom(asnEncodedData);
		_lazyMessageDigest = null;
	}

	private static byte[] Decode(byte[] rawData)
	{
		if (rawData == null)
		{
			return null;
		}
		return PkcsPal.Instance.DecodeOctetString(rawData);
	}
}
