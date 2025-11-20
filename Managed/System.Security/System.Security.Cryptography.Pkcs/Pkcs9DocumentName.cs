using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentName" /> class defines the name of a CMS/PKCS #7 message.</summary>
public sealed class Pkcs9DocumentName : Pkcs9AttributeObject
{
	private volatile string _lazyDocumentName;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9DocumentName.DocumentName" /> property retrieves the document name.</summary>
	/// <returns>A <see cref="T:System.String" /> object that contains the document name.</returns>
	public string DocumentName => _lazyDocumentName ?? (_lazyDocumentName = Decode(base.RawData));

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentName.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentName" /> class.</summary>
	public Pkcs9DocumentName()
		: base(new Oid("1.3.6.1.4.1.311.88.2.1"))
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentName.#ctor(System.String)" /> constructor creates an instance of the  <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentName" /> class by using the specified name for the CMS/PKCS #7 message.</summary>
	/// <param name="documentName">A  <see cref="T:System.String" />   object that specifies the name for the CMS/PKCS #7 message.</param>
	public Pkcs9DocumentName(string documentName)
		: base("1.3.6.1.4.1.311.88.2.1", Encode(documentName))
	{
		_lazyDocumentName = documentName;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9DocumentName.#ctor(System.Byte[])" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9DocumentName" /> class by using the specified array of byte values as the encoded name of the content of a CMS/PKCS #7 message.</summary>
	/// <param name="encodedDocumentName">An array of byte values that specifies the encoded name of the CMS/PKCS #7 message.</param>
	public Pkcs9DocumentName(byte[] encodedDocumentName)
		: base("1.3.6.1.4.1.311.88.2.1", encodedDocumentName)
	{
	}

	/// <summary>Copies information from an <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object from which to copy information.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		base.CopyFrom(asnEncodedData);
		_lazyDocumentName = null;
	}

	private static string Decode(byte[] rawData)
	{
		if (rawData == null)
		{
			return null;
		}
		return PkcsPal.Instance.DecodeOctetString(rawData).OctetStringToUnicode();
	}

	private static byte[] Encode(string documentName)
	{
		if (documentName == null)
		{
			throw new ArgumentNullException("documentName");
		}
		byte[] octets = documentName.UnicodeToOctetString();
		return PkcsPal.Instance.EncodeOctetString(octets);
	}
}
