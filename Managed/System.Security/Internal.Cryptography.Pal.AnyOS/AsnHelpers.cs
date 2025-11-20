using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Asn1;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.Pkcs.Asn1;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace Internal.Cryptography.Pal.AnyOS;

internal static class AsnHelpers
{
	internal static SubjectIdentifierOrKey ToSubjectIdentifierOrKey(this OriginatorIdentifierOrKeyAsn originator)
	{
		if (originator.IssuerAndSerialNumber.HasValue)
		{
			X500DistinguishedName x500DistinguishedName = new X500DistinguishedName(originator.IssuerAndSerialNumber.Value.Issuer.ToArray());
			return new SubjectIdentifierOrKey(SubjectIdentifierOrKeyType.IssuerAndSerialNumber, new X509IssuerSerial(x500DistinguishedName.Name, originator.IssuerAndSerialNumber.Value.SerialNumber.Span.ToBigEndianHex()));
		}
		if (originator.SubjectKeyIdentifier.HasValue)
		{
			return new SubjectIdentifierOrKey(SubjectIdentifierOrKeyType.SubjectKeyIdentifier, originator.SubjectKeyIdentifier.Value.Span.ToBigEndianHex());
		}
		if (originator.OriginatorKey != null)
		{
			OriginatorPublicKeyAsn originatorKey = originator.OriginatorKey;
			return new SubjectIdentifierOrKey(SubjectIdentifierOrKeyType.PublicKeyInfo, new PublicKeyInfo(originatorKey.Algorithm.ToPresentationObject(), originatorKey.PublicKey.ToArray()));
		}
		return new SubjectIdentifierOrKey(SubjectIdentifierOrKeyType.Unknown, string.Empty);
	}

	internal static AlgorithmIdentifier ToPresentationObject(this AlgorithmIdentifierAsn asn)
	{
		int keyLength;
		switch (asn.Algorithm.Value)
		{
		case "1.2.840.113549.3.2":
		{
			if (!asn.Parameters.HasValue)
			{
				keyLength = 0;
				break;
			}
			int effectiveKeyBits = AsnSerializer.Deserialize<Rc2CbcParameters>(asn.Parameters.Value, AsnEncodingRules.BER).GetEffectiveKeyBits();
			switch (effectiveKeyBits)
			{
			case 40:
			case 56:
			case 64:
			case 128:
				keyLength = effectiveKeyBits;
				break;
			default:
				keyLength = 0;
				break;
			}
			break;
		}
		case "1.2.840.113549.3.4":
		{
			if (!asn.Parameters.HasValue)
			{
				keyLength = 0;
				break;
			}
			int bytesWritten = 0;
			AsnReader asnReader = new AsnReader(asn.Parameters.Value, AsnEncodingRules.BER);
			if (asnReader.PeekTag() != Asn1Tag.Null)
			{
				if (asnReader.TryGetPrimitiveOctetStringBytes(out var contents))
				{
					bytesWritten = contents.Length;
				}
				else
				{
					Span<byte> destination = stackalloc byte[16];
					if (!asnReader.TryCopyOctetStringBytes(destination, out bytesWritten))
					{
						throw new CryptographicException();
					}
				}
			}
			keyLength = 128 - 8 * bytesWritten;
			break;
		}
		case "1.3.14.3.2.7":
			keyLength = 64;
			break;
		case "1.2.840.113549.3.7":
			keyLength = 192;
			break;
		default:
			keyLength = 0;
			break;
		}
		return new AlgorithmIdentifier(new Oid(asn.Algorithm), keyLength);
	}
}
