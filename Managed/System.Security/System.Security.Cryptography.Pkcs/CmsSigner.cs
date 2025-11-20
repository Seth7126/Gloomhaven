using System.Collections.Generic;
using System.Security.Cryptography.Asn1;
using System.Security.Cryptography.Pkcs.Asn1;
using System.Security.Cryptography.X509Certificates;
using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class provides signing functionality.</summary>
public sealed class CmsSigner
{
	private static readonly Oid s_defaultAlgorithm = Oid.FromOidValue("1.3.14.3.2.26", OidGroup.HashAlgorithm);

	private SubjectIdentifierType _signerIdentifierType;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.Certificate" /> property sets or retrieves the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</summary>
	/// <returns>An  <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</returns>
	public X509Certificate2 Certificate { get; set; }

	public AsymmetricAlgorithm PrivateKey { get; set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.Certificates" /> property retrieves the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that contains certificates associated with the message to be signed.  </summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents the collection of  certificates associated with the message to be signed.</returns>
	public X509Certificate2Collection Certificates { get; private set; } = new X509Certificate2Collection();

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.DigestAlgorithm" /> property sets or retrieves the <see cref="T:System.Security.Cryptography.Oid" /> that represents the hash algorithm used with the signature.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Oid" /> object that represents the hash algorithm used with the signature.</returns>
	public Oid DigestAlgorithm { get; set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.IncludeOption" /> property sets or retrieves the option that controls whether the root and entire chain associated with the signing certificate are included with the created CMS/PKCS #7 message.</summary>
	/// <returns>A member of the <see cref="T:System.Security.Cryptography.X509Certificates.X509IncludeOption" /> enumeration that specifies how much of the X509 certificate chain should be included in the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object. The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.IncludeOption" /> property can be one of the following <see cref="T:System.Security.Cryptography.X509Certificates.X509IncludeOption" /> members.NameValueMeaning<see cref="F:System.Security.Cryptography.X509Certificates.X509IncludeOption.None" />0The certificate chain is not included.<see cref="F:System.Security.Cryptography.X509Certificates.X509IncludeOption.ExcludeRoot" />1The certificate chain, except for the root certificate, is included.<see cref="F:System.Security.Cryptography.X509Certificates.X509IncludeOption.EndCertOnly" />2Only the end certificate is included.<see cref="F:System.Security.Cryptography.X509Certificates.X509IncludeOption.WholeChain" />3The certificate chain, including the root certificate, is included.</returns>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	public X509IncludeOption IncludeOption { get; set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.SignedAttributes" /> property retrieves the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection of signed attributes to be associated with the resulting <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> content. Signed attributes are signed along with the specified content.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection that represents the signed attributes. If there are no signed attributes, the property is an empty collection.</returns>
	public CryptographicAttributeObjectCollection SignedAttributes { get; private set; } = new CryptographicAttributeObjectCollection();

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.UnsignedAttributes" /> property retrieves the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection of unsigned PKCS #9 attributes to be associated with the resulting <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> content. Unsigned attributes can be modified without invalidating the signature.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection that represents the unsigned attributes. If there are no unsigned attributes, the property is an empty collection.</returns>
	public CryptographicAttributeObjectCollection UnsignedAttributes { get; private set; } = new CryptographicAttributeObjectCollection();

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.SignerIdentifierType" /> property sets or retrieves the type of the identifier of the signer.</summary>
	/// <returns>A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the type of the identifier of the signer.</returns>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	public SubjectIdentifierType SignerIdentifierType
	{
		get
		{
			return _signerIdentifierType;
		}
		set
		{
			if (value < SubjectIdentifierType.IssuerAndSerialNumber || value > SubjectIdentifierType.NoSignature)
			{
				throw new ArgumentException(global::SR.Format("The subject identifier type {0} is not valid.", value));
			}
			_signerIdentifierType = value;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class by using a default subject identifier type.</summary>
	public CmsSigner()
		: this(SubjectIdentifierType.IssuerAndSerialNumber, null)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class with the specified subject identifier type.</summary>
	/// <param name="signerIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the signer identifier type.</param>
	public CmsSigner(SubjectIdentifierType signerIdentifierType)
		: this(signerIdentifierType, null)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.X509Certificates.X509Certificate2)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class with the specified signing certificate.</summary>
	/// <param name="certificate">An    <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</param>
	public CmsSigner(X509Certificate2 certificate)
		: this(SubjectIdentifierType.IssuerAndSerialNumber, certificate)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.CspParameters)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class with the specified cryptographic service provider (CSP) parameters. <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.CspParameters)" /> is useful when you know the specific CSP and private key to use for signing.</summary>
	/// <param name="parameters">A <see cref="T:System.Security.Cryptography.CspParameters" />  object that represents the set of CSP parameters to use.</param>
	public CmsSigner(CspParameters parameters)
	{
		throw new PlatformNotSupportedException();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.CmsSigner.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.X509Certificates.X509Certificate2)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> class with the specified signer identifier type and signing certificate.</summary>
	/// <param name="signerIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the signer identifier type.</param>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object that represents the signing certificate.</param>
	public CmsSigner(SubjectIdentifierType signerIdentifierType, X509Certificate2 certificate)
	{
		switch (signerIdentifierType)
		{
		case SubjectIdentifierType.Unknown:
			_signerIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			IncludeOption = X509IncludeOption.ExcludeRoot;
			break;
		case SubjectIdentifierType.IssuerAndSerialNumber:
			_signerIdentifierType = signerIdentifierType;
			IncludeOption = X509IncludeOption.ExcludeRoot;
			break;
		case SubjectIdentifierType.SubjectKeyIdentifier:
			_signerIdentifierType = signerIdentifierType;
			IncludeOption = X509IncludeOption.ExcludeRoot;
			break;
		case SubjectIdentifierType.NoSignature:
			_signerIdentifierType = signerIdentifierType;
			IncludeOption = X509IncludeOption.None;
			break;
		default:
			_signerIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
			IncludeOption = X509IncludeOption.ExcludeRoot;
			break;
		}
		Certificate = certificate;
		DigestAlgorithm = new Oid(s_defaultAlgorithm);
	}

	internal void CheckCertificateValue()
	{
		if (SignerIdentifierType != SubjectIdentifierType.NoSignature)
		{
			if (Certificate == null)
			{
				throw new PlatformNotSupportedException("No signer certificate was provided. This platform does not implement the certificate picker UI.");
			}
			if (!Certificate.HasPrivateKey)
			{
				throw new CryptographicException("A certificate with a private key is required.");
			}
		}
	}

	internal SignerInfoAsn Sign(ReadOnlyMemory<byte> data, string contentTypeOid, bool silent, out X509Certificate2Collection chainCerts)
	{
		HashAlgorithmName digestAlgorithm = Helpers.GetDigestAlgorithm(DigestAlgorithm);
		IncrementalHash hasher = IncrementalHash.CreateHash(digestAlgorithm);
		Helpers.AppendData(hasher, data.Span);
		byte[] hashAndReset = hasher.GetHashAndReset();
		SignerInfoAsn result = new SignerInfoAsn
		{
			DigestAlgorithm = 
			{
				Algorithm = DigestAlgorithm
			}
		};
		CryptographicAttributeObjectCollection signedAttributes = SignedAttributes;
		if ((signedAttributes != null && signedAttributes.Count > 0) || contentTypeOid != "1.2.840.113549.1.7.1")
		{
			List<AttributeAsn> list = BuildAttributes(SignedAttributes);
			using (AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.DER))
			{
				asnWriter.PushSetOf();
				asnWriter.WriteOctetString(hashAndReset);
				asnWriter.PopSetOf();
				list.Add(new AttributeAsn
				{
					AttrType = new Oid("1.2.840.113549.1.9.4", "1.2.840.113549.1.9.4"),
					AttrValues = asnWriter.Encode()
				});
			}
			if (contentTypeOid != null)
			{
				using AsnWriter asnWriter2 = new AsnWriter(AsnEncodingRules.DER);
				asnWriter2.PushSetOf();
				asnWriter2.WriteObjectIdentifier(contentTypeOid);
				asnWriter2.PopSetOf();
				list.Add(new AttributeAsn
				{
					AttrType = new Oid("1.2.840.113549.1.9.3", "1.2.840.113549.1.9.3"),
					AttrValues = asnWriter2.Encode()
				});
			}
			using (AsnWriter asnWriter3 = AsnSerializer.Serialize(new SignedAttributesSet
			{
				SignedAttributes = Helpers.NormalizeSet(list.ToArray(), delegate(byte[] normalized)
				{
					AsnReader asnReader = new AsnReader(normalized, AsnEncodingRules.DER);
					Helpers.AppendData(hasher, asnReader.PeekContentBytes().Span);
				})
			}, AsnEncodingRules.BER))
			{
				result.SignedAttributes = asnWriter3.Encode();
			}
			hashAndReset = hasher.GetHashAndReset();
		}
		switch (SignerIdentifierType)
		{
		case SubjectIdentifierType.IssuerAndSerialNumber:
		{
			byte[] serialNumber = Certificate.GetSerialNumber();
			Array.Reverse(serialNumber);
			result.Sid.IssuerAndSerialNumber = new IssuerAndSerialNumberAsn
			{
				Issuer = Certificate.IssuerName.RawData,
				SerialNumber = serialNumber
			};
			result.Version = 1;
			break;
		}
		case SubjectIdentifierType.SubjectKeyIdentifier:
			result.Sid.SubjectKeyIdentifier = Certificate.GetSubjectKeyIdentifier();
			result.Version = 3;
			break;
		case SubjectIdentifierType.NoSignature:
			result.Sid.IssuerAndSerialNumber = new IssuerAndSerialNumberAsn
			{
				Issuer = SubjectIdentifier.DummySignerEncodedValue,
				SerialNumber = new byte[1]
			};
			result.Version = 1;
			break;
		default:
			throw new CryptographicException();
		}
		if (UnsignedAttributes != null && UnsignedAttributes.Count > 0)
		{
			List<AttributeAsn> list2 = BuildAttributes(UnsignedAttributes);
			result.UnsignedAttributes = Helpers.NormalizeSet(list2.ToArray());
		}
		if (!CmsSignature.Sign(hashAndReset, digestAlgorithm, Certificate, silent, out var oid, out var signatureValue))
		{
			throw new CryptographicException("Could not determine signature algorithm for the signer certificate.");
		}
		result.SignatureValue = signatureValue;
		result.SignatureAlgorithm.Algorithm = oid;
		X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
		x509Certificate2Collection.AddRange(Certificates);
		if (SignerIdentifierType != SubjectIdentifierType.NoSignature)
		{
			if (IncludeOption == X509IncludeOption.EndCertOnly)
			{
				x509Certificate2Collection.Add(Certificate);
			}
			else if (IncludeOption != X509IncludeOption.None)
			{
				X509Chain x509Chain = new X509Chain();
				x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
				x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
				if (!x509Chain.Build(Certificate))
				{
					X509ChainStatus[] chainStatus = x509Chain.ChainStatus;
					foreach (X509ChainStatus x509ChainStatus in chainStatus)
					{
						if (x509ChainStatus.Status == X509ChainStatusFlags.PartialChain)
						{
							throw new CryptographicException("The certificate chain is incomplete, the self-signed root authority could not be determined.");
						}
					}
				}
				X509ChainElementCollection chainElements = x509Chain.ChainElements;
				int count = chainElements.Count;
				int num2 = count - 1;
				if (num2 == 0)
				{
					num2 = -1;
				}
				for (int num3 = 0; num3 < count; num3++)
				{
					X509Certificate2 certificate = chainElements[num3].Certificate;
					if (num3 == num2 && IncludeOption == X509IncludeOption.ExcludeRoot && certificate.SubjectName.RawData.AsSpan().SequenceEqual(certificate.IssuerName.RawData))
					{
						break;
					}
					x509Certificate2Collection.Add(certificate);
				}
			}
		}
		chainCerts = x509Certificate2Collection;
		return result;
	}

	internal static List<AttributeAsn> BuildAttributes(CryptographicAttributeObjectCollection attributes)
	{
		List<AttributeAsn> list = new List<AttributeAsn>();
		if (attributes == null || attributes.Count == 0)
		{
			return list;
		}
		CryptographicAttributeObjectEnumerator enumerator = attributes.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CryptographicAttributeObject current = enumerator.Current;
			using AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.DER);
			asnWriter.PushSetOf();
			AsnEncodedDataEnumerator enumerator2 = current.Values.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				AsnEncodedData current2 = enumerator2.Current;
				asnWriter.WriteEncodedValue(current2.RawData);
			}
			asnWriter.PopSetOf();
			AttributeAsn item = new AttributeAsn
			{
				AttrType = current.Oid,
				AttrValues = asnWriter.Encode()
			};
			list.Add(item);
		}
		return list;
	}
}
