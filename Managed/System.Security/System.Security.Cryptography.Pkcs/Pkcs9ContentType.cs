using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9ContentType" /> class defines the type of the content of a CMS/PKCS #7 message.</summary>
public sealed class Pkcs9ContentType : Pkcs9AttributeObject
{
	private volatile Oid _lazyContentType;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9ContentType.ContentType" /> property gets an <see cref="T:System.Security.Cryptography.Oid" /> object that contains the content type.</summary>
	/// <returns>An  <see cref="T:System.Security.Cryptography.Oid" /> object that contains the content type.</returns>
	public Oid ContentType => _lazyContentType ?? (_lazyContentType = Decode(base.RawData));

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9ContentType.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9ContentType" /> class.</summary>
	public Pkcs9ContentType()
		: base(Oid.FromOidValue("1.2.840.113549.1.9.3", OidGroup.ExtensionOrAttribute))
	{
	}

	/// <summary>Copies information from an <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object from which to copy information.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		base.CopyFrom(asnEncodedData);
		_lazyContentType = null;
	}

	private static Oid Decode(byte[] rawData)
	{
		if (rawData == null)
		{
			return null;
		}
		return new Oid(PkcsPal.Instance.DecodeOid(rawData));
	}
}
