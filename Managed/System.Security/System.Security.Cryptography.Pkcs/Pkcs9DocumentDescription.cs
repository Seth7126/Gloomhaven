using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription" /> class defines the description of the content of a CMS/PKCS #7 message.</summary>
public sealed class Pkcs9DocumentDescription : Pkcs9AttributeObject
{
	private volatile string _lazyDocumentDescription;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription.DocumentDescription" /> property retrieves the document description.</summary>
	/// <returns>A <see cref="T:System.String" /> object that contains the document description.</returns>
	public string DocumentDescription => _lazyDocumentDescription ?? (_lazyDocumentDescription = Decode(base.RawData));

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription" /> class.</summary>
	public Pkcs9DocumentDescription()
		: base(new Oid("1.3.6.1.4.1.311.88.2.2"))
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription.#ctor(System.String)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription" /> class by using the specified description of the content of a CMS/PKCS #7 message.</summary>
	/// <param name="documentDescription">An instance of the <see cref="T:System.String" />  class that specifies the description for the CMS/PKCS #7 message.</param>
	public Pkcs9DocumentDescription(string documentDescription)
		: base("1.3.6.1.4.1.311.88.2.2", Encode(documentDescription))
	{
		_lazyDocumentDescription = documentDescription;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription.#ctor(System.Byte[])" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription" /> class by using the specified array of byte values as the encoded description of the content of a CMS/PKCS #7 message.</summary>
	/// <param name="encodedDocumentDescription">An array of byte values that specifies the encoded description of the CMS/PKCS #7 message.</param>
	public Pkcs9DocumentDescription(byte[] encodedDocumentDescription)
		: base("1.3.6.1.4.1.311.88.2.2", encodedDocumentDescription)
	{
	}

	/// <summary>Copies information from an <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object from which to copy information.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		base.CopyFrom(asnEncodedData);
		_lazyDocumentDescription = null;
	}

	private static string Decode(byte[] rawData)
	{
		if (rawData == null)
		{
			return null;
		}
		return PkcsPal.Instance.DecodeOctetString(rawData).OctetStringToUnicode();
	}

	private static byte[] Encode(string documentDescription)
	{
		if (documentDescription == null)
		{
			throw new ArgumentNullException("documentDescription");
		}
		byte[] octets = documentDescription.UnicodeToOctetString();
		return PkcsPal.Instance.EncodeOctetString(octets);
	}
}
