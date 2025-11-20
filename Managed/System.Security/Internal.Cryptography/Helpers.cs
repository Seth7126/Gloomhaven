using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Asn1;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.Pkcs.Asn1;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Internal.Cryptography;

internal static class Helpers
{
	private struct Certificate
	{
		internal TbsCertificateLite TbsCertificate;

		internal AlgorithmIdentifierAsn AlgorithmIdentifier;

		[BitString]
		internal ReadOnlyMemory<byte> SignatureValue;
	}

	private struct TbsCertificateLite
	{
		[ExpectedTag(0, ExplicitTag = true)]
		[DefaultValue(new byte[] { 160, 3, 2, 1, 0 })]
		internal int Version;

		[Integer]
		internal ReadOnlyMemory<byte> SerialNumber;

		internal AlgorithmIdentifierAsn AlgorithmIdentifier;

		[AnyValue]
		[ExpectedTag(TagClass.Universal, 16)]
		internal ReadOnlyMemory<byte> Issuer;

		[AnyValue]
		[ExpectedTag(TagClass.Universal, 16)]
		internal ReadOnlyMemory<byte> Validity;

		[AnyValue]
		[ExpectedTag(TagClass.Universal, 16)]
		internal ReadOnlyMemory<byte> Subject;

		[AnyValue]
		[ExpectedTag(TagClass.Universal, 16)]
		internal ReadOnlyMemory<byte> SubjectPublicKeyInfo;

		[BitString]
		[ExpectedTag(1)]
		[OptionalValue]
		internal ReadOnlyMemory<byte>? IssuerUniqueId;

		[OptionalValue]
		[BitString]
		[ExpectedTag(2)]
		internal ReadOnlyMemory<byte>? SubjectUniqueId;

		[ExpectedTag(3)]
		[AnyValue]
		[OptionalValue]
		internal ReadOnlyMemory<byte>? Extensions;
	}

	internal struct AsnSet<T>
	{
		[SetOf]
		public T[] SetData;
	}

	internal static void AppendData(this IncrementalHash hasher, ReadOnlySpan<byte> data)
	{
		hasher.AppendData(data.ToArray());
	}

	internal static HashAlgorithmName GetDigestAlgorithm(Oid oid)
	{
		return GetDigestAlgorithm(oid.Value);
	}

	internal static HashAlgorithmName GetDigestAlgorithm(string oidValue)
	{
		return oidValue switch
		{
			"1.2.840.113549.2.5" => HashAlgorithmName.MD5, 
			"1.3.14.3.2.26" => HashAlgorithmName.SHA1, 
			"2.16.840.1.101.3.4.2.1" => HashAlgorithmName.SHA256, 
			"2.16.840.1.101.3.4.2.2" => HashAlgorithmName.SHA384, 
			"2.16.840.1.101.3.4.2.3" => HashAlgorithmName.SHA512, 
			_ => throw new CryptographicException("'{0}' is not a known hash algorithm.", oidValue), 
		};
	}

	internal static string GetOidFromHashAlgorithm(HashAlgorithmName algName)
	{
		if (algName == HashAlgorithmName.MD5)
		{
			return "1.2.840.113549.2.5";
		}
		if (algName == HashAlgorithmName.SHA1)
		{
			return "1.3.14.3.2.26";
		}
		if (algName == HashAlgorithmName.SHA256)
		{
			return "2.16.840.1.101.3.4.2.1";
		}
		if (algName == HashAlgorithmName.SHA384)
		{
			return "2.16.840.1.101.3.4.2.2";
		}
		if (algName == HashAlgorithmName.SHA512)
		{
			return "2.16.840.1.101.3.4.2.3";
		}
		throw new CryptographicException("Unknown algorithm '{0}'.", algName.Name);
	}

	public static byte[] Resize(this byte[] a, int size)
	{
		Array.Resize(ref a, size);
		return a;
	}

	public static void RemoveAt<T>(ref T[] arr, int idx)
	{
		if (arr.Length == 1)
		{
			arr = Array.Empty<T>();
			return;
		}
		T[] array = new T[arr.Length - 1];
		if (idx != 0)
		{
			Array.Copy(arr, 0, array, 0, idx);
		}
		if (idx < array.Length)
		{
			Array.Copy(arr, idx + 1, array, idx, array.Length - idx);
		}
		arr = array;
	}

	public static T[] NormalizeSet<T>(T[] setItems, Action<byte[]> encodedValueProcessor = null)
	{
		byte[] array = AsnSerializer.Serialize(new AsnSet<T>
		{
			SetData = setItems
		}, AsnEncodingRules.DER).Encode();
		AsnSet<T> asnSet = AsnSerializer.Deserialize<AsnSet<T>>(array, AsnEncodingRules.DER);
		encodedValueProcessor?.Invoke(array);
		return asnSet.SetData;
	}

	internal static byte[] EncodeContentInfo<T>(T value, string contentType, AsnEncodingRules ruleSet = AsnEncodingRules.DER)
	{
		using AsnWriter asnWriter = AsnSerializer.Serialize(value, ruleSet);
		using AsnWriter asnWriter2 = AsnSerializer.Serialize(new ContentInfoAsn
		{
			ContentType = contentType,
			Content = asnWriter.Encode()
		}, ruleSet);
		return asnWriter2.Encode();
	}

	public static CmsRecipientCollection DeepCopy(this CmsRecipientCollection recipients)
	{
		CmsRecipientCollection cmsRecipientCollection = new CmsRecipientCollection();
		CmsRecipientEnumerator enumerator = recipients.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CmsRecipient current = enumerator.Current;
			X509Certificate2 certificate = current.Certificate;
			CmsRecipient recipient = new CmsRecipient(certificate: new X509Certificate2(certificate.Handle), recipientIdentifierType: current.RecipientIdentifierType);
			cmsRecipientCollection.Add(recipient);
			GC.KeepAlive(certificate);
		}
		return cmsRecipientCollection;
	}

	public static byte[] UnicodeToOctetString(this string s)
	{
		byte[] array = new byte[2 * (s.Length + 1)];
		Encoding.Unicode.GetBytes(s, 0, s.Length, array, 0);
		return array;
	}

	public static string OctetStringToUnicode(this byte[] octets)
	{
		if (octets.Length < 2)
		{
			return string.Empty;
		}
		return Encoding.Unicode.GetString(octets, 0, octets.Length - 2);
	}

	public static X509Certificate2Collection GetStoreCertificates(StoreName storeName, StoreLocation storeLocation, bool openExistingOnly)
	{
		using X509Store x509Store = new X509Store(storeName, storeLocation);
		OpenFlags openFlags = OpenFlags.IncludeArchived;
		if (openExistingOnly)
		{
			openFlags |= OpenFlags.OpenExistingOnly;
		}
		x509Store.Open(openFlags);
		return x509Store.Certificates;
	}

	public static X509Certificate2 TryFindMatchingCertificate(this X509Certificate2Collection certs, SubjectIdentifier recipientIdentifier)
	{
		switch (recipientIdentifier.Type)
		{
		case SubjectIdentifierType.IssuerAndSerialNumber:
		{
			X509IssuerSerial x509IssuerSerial = (X509IssuerSerial)recipientIdentifier.Value;
			byte[] ba2 = x509IssuerSerial.SerialNumber.ToSerialBytes();
			string issuerName = x509IssuerSerial.IssuerName;
			X509Certificate2Enumerator enumerator = certs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				X509Certificate2 current2 = enumerator.Current;
				if (AreByteArraysEqual(current2.GetSerialNumber(), ba2) && current2.Issuer == issuerName)
				{
					return current2;
				}
			}
			break;
		}
		case SubjectIdentifierType.SubjectKeyIdentifier:
		{
			byte[] ba = ((string)recipientIdentifier.Value).ToSkiBytes();
			X509Certificate2Enumerator enumerator = certs.GetEnumerator();
			while (enumerator.MoveNext())
			{
				X509Certificate2 current = enumerator.Current;
				byte[] subjectKeyIdentifier = PkcsPal.Instance.GetSubjectKeyIdentifier(current);
				if (AreByteArraysEqual(ba, subjectKeyIdentifier))
				{
					return current;
				}
			}
			break;
		}
		default:
			throw new CryptographicException();
		}
		return null;
	}

	private static bool AreByteArraysEqual(byte[] ba1, byte[] ba2)
	{
		if (ba1.Length != ba2.Length)
		{
			return false;
		}
		for (int i = 0; i < ba1.Length; i++)
		{
			if (ba1[i] != ba2[i])
			{
				return false;
			}
		}
		return true;
	}

	private static byte[] ToSkiBytes(this string skiString)
	{
		return skiString.UpperHexStringToByteArray();
	}

	public static string ToSkiString(this byte[] skiBytes)
	{
		return ToUpperHexString(skiBytes);
	}

	public static string ToBigEndianHex(this ReadOnlySpan<byte> bytes)
	{
		return ToUpperHexString(bytes);
	}

	private static byte[] ToSerialBytes(this string serialString)
	{
		byte[] array = serialString.UpperHexStringToByteArray();
		Array.Reverse(array);
		return array;
	}

	public static string ToSerialString(this byte[] serialBytes)
	{
		serialBytes = serialBytes.CloneByteArray();
		Array.Reverse(serialBytes);
		return ToUpperHexString(serialBytes);
	}

	private static string ToUpperHexString(ReadOnlySpan<byte> ba)
	{
		StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
		for (int i = 0; i < ba.Length; i++)
		{
			stringBuilder.Append(ba[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	private static byte[] UpperHexStringToByteArray(this string normalizedString)
	{
		byte[] array = new byte[normalizedString.Length / 2];
		for (int i = 0; i < array.Length; i++)
		{
			char c = normalizedString[i * 2];
			byte b = (byte)(UpperHexCharToNybble(c) << 4);
			c = normalizedString[i * 2 + 1];
			b |= UpperHexCharToNybble(c);
			array[i] = b;
		}
		return array;
	}

	private static byte UpperHexCharToNybble(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return (byte)(c - 48);
		}
		if (c >= 'A' && c <= 'F')
		{
			return (byte)(c - 65 + 10);
		}
		throw new CryptographicException();
	}

	public static Pkcs9AttributeObject CreateBestPkcs9AttributeObjectAvailable(Oid oid, byte[] encodedAttribute)
	{
		Pkcs9AttributeObject pkcs9AttributeObject = new Pkcs9AttributeObject(oid, encodedAttribute);
		switch (oid.Value)
		{
		case "1.3.6.1.4.1.311.88.2.1":
			pkcs9AttributeObject = Upgrade<Pkcs9DocumentName>(pkcs9AttributeObject);
			break;
		case "1.3.6.1.4.1.311.88.2.2":
			pkcs9AttributeObject = Upgrade<Pkcs9DocumentDescription>(pkcs9AttributeObject);
			break;
		case "1.2.840.113549.1.9.5":
			pkcs9AttributeObject = Upgrade<Pkcs9SigningTime>(pkcs9AttributeObject);
			break;
		case "1.2.840.113549.1.9.3":
			pkcs9AttributeObject = Upgrade<Pkcs9ContentType>(pkcs9AttributeObject);
			break;
		case "1.2.840.113549.1.9.4":
			pkcs9AttributeObject = Upgrade<Pkcs9MessageDigest>(pkcs9AttributeObject);
			break;
		}
		return pkcs9AttributeObject;
	}

	private static T Upgrade<T>(Pkcs9AttributeObject basicAttribute) where T : Pkcs9AttributeObject, new()
	{
		T val = new T();
		val.CopyFrom(basicAttribute);
		return val;
	}

	public static byte[] GetSubjectKeyIdentifier(this X509Certificate2 certificate)
	{
		X509Extension x509Extension = certificate.Extensions["2.5.29.14"];
		if (x509Extension != null)
		{
			if (new AsnReader(x509Extension.RawData, AsnEncodingRules.DER).TryGetPrimitiveOctetStringBytes(out var contents))
			{
				return contents.ToArray();
			}
			throw new CryptographicException("ASN1 corrupted data.");
		}
		using HashAlgorithm hashAlgorithm = SHA1.Create();
		return hashAlgorithm.ComputeHash(GetSubjectPublicKeyInfo(certificate).ToArray());
	}

	internal static byte[] OneShot(this ICryptoTransform transform, byte[] data)
	{
		return transform.OneShot(data, 0, data.Length);
	}

	internal static byte[] OneShot(this ICryptoTransform transform, byte[] data, int offset, int length)
	{
		if (transform.CanTransformMultipleBlocks)
		{
			return transform.TransformFinalBlock(data, offset, length);
		}
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
		{
			cryptoStream.Write(data, offset, length);
		}
		return memoryStream.ToArray();
	}

	private static ReadOnlyMemory<byte> GetSubjectPublicKeyInfo(X509Certificate2 certificate)
	{
		return AsnSerializer.Deserialize<Certificate>(certificate.RawData, AsnEncodingRules.DER).TbsCertificate.SubjectPublicKeyInfo;
	}
}
