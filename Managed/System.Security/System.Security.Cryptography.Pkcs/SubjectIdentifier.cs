using System.Security.Cryptography.Pkcs.Asn1;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using Internal.Cryptography;
using Unity;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifier" /> class defines the type of the identifier of a subject, such as a <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> or a <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" />.  The subject can be identified by the certificate issuer and serial number or the subject key.</summary>
public sealed class SubjectIdentifier
{
	private const string DummySignerSubjectName = "CN=Dummy Signer";

	internal static readonly byte[] DummySignerEncodedValue = new X500DistinguishedName("CN=Dummy Signer").RawData;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Type" /> property retrieves the type of subject identifier. The subject can be identified by the certificate issuer and serial number or the subject key.</summary>
	/// <returns>A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" />  enumeration that identifies the type of subject.</returns>
	public SubjectIdentifierType Type { get; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Value" /> property retrieves the value of the subject identifier. Use the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Type" /> property to determine the type of subject identifier, and use the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Value" /> property to retrieve the corresponding value.</summary>
	/// <returns>An <see cref="T:System.Object" /> object that represents the value of the subject identifier. This <see cref="T:System.Object" /> can be one of the following objects as determined by the <see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Type" /> property.<see cref="P:System.Security.Cryptography.Pkcs.SubjectIdentifier.Type" /> propertyObjectIssuerAndSerialNumber<see cref="T:System.Security.Cryptography.Xml.X509IssuerSerial" />SubjectKeyIdentifier<see cref="T:System.String" /></returns>
	public object Value { get; }

	internal SubjectIdentifier(SubjectIdentifierType type, object value)
	{
		Type = type;
		Value = value;
	}

	internal SubjectIdentifier(SignerIdentifierAsn signerIdentifierAsn)
		: this(signerIdentifierAsn.IssuerAndSerialNumber, signerIdentifierAsn.SubjectKeyIdentifier)
	{
	}

	internal SubjectIdentifier(IssuerAndSerialNumberAsn? issuerAndSerialNumber, ReadOnlyMemory<byte>? subjectKeyIdentifier)
	{
		if (issuerAndSerialNumber.HasValue)
		{
			IssuerAndSerialNumberAsn value = issuerAndSerialNumber.Value;
			ReadOnlySpan<byte> span = value.Issuer.Span;
			value = issuerAndSerialNumber.Value;
			ReadOnlySpan<byte> span2 = value.SerialNumber.Span;
			bool flag = false;
			for (int i = 0; i < span2.Length; i++)
			{
				if (span2[i] != 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag && DummySignerEncodedValue.AsSpan().SequenceEqual(span))
			{
				Type = SubjectIdentifierType.NoSignature;
				Value = null;
			}
			else
			{
				Type = SubjectIdentifierType.IssuerAndSerialNumber;
				X500DistinguishedName x500DistinguishedName = new X500DistinguishedName(span.ToArray());
				Value = new X509IssuerSerial(x500DistinguishedName.Name, span2.ToBigEndianHex());
			}
		}
		else
		{
			if (!subjectKeyIdentifier.HasValue)
			{
				throw new CryptographicException();
			}
			Type = SubjectIdentifierType.SubjectKeyIdentifier;
			Value = subjectKeyIdentifier.Value.Span.ToBigEndianHex();
		}
	}

	internal SubjectIdentifier()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
