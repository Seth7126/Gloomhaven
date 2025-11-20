using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.Asn1;
using System.Security.Cryptography.Pkcs.Asn1;
using System.Security.Cryptography.X509Certificates;
using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

internal abstract class CmsSignature
{
	private class DSACmsSignature : CmsSignature
	{
		private readonly HashAlgorithmName _expectedDigest;

		private readonly string _signatureAlgorithm;

		internal DSACmsSignature(string signatureAlgorithm, HashAlgorithmName expectedDigest)
		{
			_signatureAlgorithm = signatureAlgorithm;
			_expectedDigest = expectedDigest;
		}

		internal override bool VerifySignature(byte[] valueHash, byte[] signature, string digestAlgorithmOid, HashAlgorithmName digestAlgorithmName, ReadOnlyMemory<byte>? signatureParameters, X509Certificate2 certificate)
		{
			if (_expectedDigest != digestAlgorithmName)
			{
				throw new CryptographicException(global::SR.Format("SignerInfo digest algorithm '{0}' is not valid for signature algorithm '{1}'.", digestAlgorithmOid, _signatureAlgorithm));
			}
			DSA dSAPublicKey = certificate.GetDSAPublicKey();
			if (dSAPublicKey == null)
			{
				return false;
			}
			byte[] array = new byte[2 * dSAPublicKey.ExportParameters(includePrivateParameters: false).Q.Length];
			if (!DsaDerToIeee(signature, array))
			{
				return false;
			}
			return dSAPublicKey.VerifySignature(valueHash, array);
		}

		protected override bool Sign(byte[] dataHash, HashAlgorithmName hashAlgorithmName, X509Certificate2 certificate, bool silent, out Oid signatureAlgorithm, out byte[] signatureValue)
		{
			DSA dSA = PkcsPal.Instance.GetPrivateKeyForSigning<DSA>(certificate, silent) ?? certificate.GetDSAPublicKey();
			if (dSA == null)
			{
				signatureAlgorithm = null;
				signatureValue = null;
				return false;
			}
			string text = ((hashAlgorithmName == HashAlgorithmName.SHA1) ? "1.2.840.10040.4.3" : ((hashAlgorithmName == HashAlgorithmName.SHA256) ? "2.16.840.1.101.3.4.3.2" : ((hashAlgorithmName == HashAlgorithmName.SHA384) ? "2.16.840.1.101.3.4.3.3" : ((hashAlgorithmName == HashAlgorithmName.SHA512) ? "2.16.840.1.101.3.4.3.4" : null))));
			if (text == null)
			{
				signatureAlgorithm = null;
				signatureValue = null;
				return false;
			}
			signatureAlgorithm = new Oid(text, text);
			byte[] array = dSA.CreateSignature(dataHash);
			signatureValue = DsaIeeeToDer(new ReadOnlySpan<byte>(array));
			return true;
		}
	}

	private class ECDsaCmsSignature : CmsSignature
	{
		private readonly HashAlgorithmName _expectedDigest;

		private readonly string _signatureAlgorithm;

		internal ECDsaCmsSignature(string signatureAlgorithm, HashAlgorithmName expectedDigest)
		{
			_signatureAlgorithm = signatureAlgorithm;
			_expectedDigest = expectedDigest;
		}

		internal override bool VerifySignature(byte[] valueHash, byte[] signature, string digestAlgorithmOid, HashAlgorithmName digestAlgorithmName, ReadOnlyMemory<byte>? signatureParameters, X509Certificate2 certificate)
		{
			if (_expectedDigest != digestAlgorithmName)
			{
				throw new CryptographicException(global::SR.Format("SignerInfo digest algorithm '{0}' is not valid for signature algorithm '{1}'.", digestAlgorithmOid, _signatureAlgorithm));
			}
			ECDsa eCDsaPublicKey = certificate.GetECDsaPublicKey();
			if (eCDsaPublicKey == null)
			{
				return false;
			}
			byte[] array = new byte[eCDsaPublicKey.KeySize / 4];
			if (!DsaDerToIeee(signature, array))
			{
				return false;
			}
			return eCDsaPublicKey.VerifyHash(valueHash, array);
		}

		protected override bool Sign(byte[] dataHash, HashAlgorithmName hashAlgorithmName, X509Certificate2 certificate, bool silent, out Oid signatureAlgorithm, out byte[] signatureValue)
		{
			ECDsa eCDsa = PkcsPal.Instance.GetPrivateKeyForSigning<ECDsa>(certificate, silent) ?? certificate.GetECDsaPublicKey();
			if (eCDsa == null)
			{
				signatureAlgorithm = null;
				signatureValue = null;
				return false;
			}
			string text = ((hashAlgorithmName == HashAlgorithmName.SHA1) ? "1.2.840.10045.4.1" : ((hashAlgorithmName == HashAlgorithmName.SHA256) ? "1.2.840.10045.4.3.2" : ((hashAlgorithmName == HashAlgorithmName.SHA384) ? "1.2.840.10045.4.3.3" : ((hashAlgorithmName == HashAlgorithmName.SHA512) ? "1.2.840.10045.4.3.4" : null))));
			if (text == null)
			{
				signatureAlgorithm = null;
				signatureValue = null;
				return false;
			}
			signatureAlgorithm = new Oid(text, text);
			signatureValue = DsaIeeeToDer(eCDsa.SignHash(dataHash));
			return true;
		}
	}

	private abstract class RSACmsSignature : CmsSignature
	{
		private readonly string _signatureAlgorithm;

		private readonly HashAlgorithmName? _expectedDigest;

		protected RSACmsSignature(string signatureAlgorithm, HashAlgorithmName? expectedDigest)
		{
			_signatureAlgorithm = signatureAlgorithm;
			_expectedDigest = expectedDigest;
		}

		internal override bool VerifySignature(byte[] valueHash, byte[] signature, string digestAlgorithmOid, HashAlgorithmName digestAlgorithmName, ReadOnlyMemory<byte>? signatureParameters, X509Certificate2 certificate)
		{
			if (_expectedDigest.HasValue && _expectedDigest.Value != digestAlgorithmName)
			{
				throw new CryptographicException(global::SR.Format("SignerInfo digest algorithm '{0}' is not valid for signature algorithm '{1}'.", digestAlgorithmOid, _signatureAlgorithm));
			}
			RSASignaturePadding signaturePadding = GetSignaturePadding(signatureParameters, digestAlgorithmOid, digestAlgorithmName, valueHash.Length);
			return certificate.GetRSAPublicKey()?.VerifyHash(valueHash, signature, digestAlgorithmName, signaturePadding) ?? false;
		}

		protected abstract RSASignaturePadding GetSignaturePadding(ReadOnlyMemory<byte>? signatureParameters, string digestAlgorithmOid, HashAlgorithmName digestAlgorithmName, int digestValueLength);
	}

	private sealed class RSAPkcs1CmsSignature : RSACmsSignature
	{
		public RSAPkcs1CmsSignature(string signatureAlgorithm, HashAlgorithmName? expectedDigest)
			: base(signatureAlgorithm, expectedDigest)
		{
		}

		protected override RSASignaturePadding GetSignaturePadding(ReadOnlyMemory<byte>? signatureParameters, string digestAlgorithmOid, HashAlgorithmName digestAlgorithmName, int digestValueLength)
		{
			if (!signatureParameters.HasValue)
			{
				return RSASignaturePadding.Pkcs1;
			}
			Span<byte> span = stackalloc byte[2];
			span[0] = 5;
			span[1] = 0;
			if (span.SequenceEqual(signatureParameters.Value.Span))
			{
				return RSASignaturePadding.Pkcs1;
			}
			throw new CryptographicException("Invalid signature paramters.");
		}

		protected override bool Sign(byte[] dataHash, HashAlgorithmName hashAlgorithmName, X509Certificate2 certificate, bool silent, out Oid signatureAlgorithm, out byte[] signatureValue)
		{
			RSA rSA = PkcsPal.Instance.GetPrivateKeyForSigning<RSA>(certificate, silent) ?? certificate.GetRSAPublicKey();
			if (rSA == null)
			{
				signatureAlgorithm = null;
				signatureValue = null;
				return false;
			}
			signatureAlgorithm = new Oid("1.2.840.113549.1.1.1", "1.2.840.113549.1.1.1");
			signatureValue = rSA.SignHash(dataHash, hashAlgorithmName, RSASignaturePadding.Pkcs1);
			return true;
		}
	}

	private class RSAPssCmsSignature : RSACmsSignature
	{
		public RSAPssCmsSignature()
			: base(null, null)
		{
		}

		protected override RSASignaturePadding GetSignaturePadding(ReadOnlyMemory<byte>? signatureParameters, string digestAlgorithmOid, HashAlgorithmName digestAlgorithmName, int digestValueLength)
		{
			if (!signatureParameters.HasValue)
			{
				throw new CryptographicException("PSS parameters were not present.");
			}
			PssParamsAsn pssParamsAsn = AsnSerializer.Deserialize<PssParamsAsn>(signatureParameters.Value, AsnEncodingRules.DER);
			if (pssParamsAsn.HashAlgorithm.Algorithm.Value != digestAlgorithmOid)
			{
				throw new CryptographicException(global::SR.Format("This platform requires that the PSS hash algorithm ({0}) match the data digest algorithm ({1}).", pssParamsAsn.HashAlgorithm.Algorithm.Value, digestAlgorithmOid));
			}
			if (pssParamsAsn.TrailerField != 1)
			{
				throw new CryptographicException("Invalid signature paramters.");
			}
			if (pssParamsAsn.SaltLength != digestValueLength)
			{
				throw new CryptographicException(global::SR.Format("PSS salt size {0} is not supported by this platform with hash algorithm {1}.", pssParamsAsn.SaltLength, digestAlgorithmName.Name));
			}
			if (pssParamsAsn.MaskGenAlgorithm.Algorithm.Value != "1.2.840.113549.1.1.8")
			{
				throw new CryptographicException("Mask generation function '{0}' is not supported by this platform.", pssParamsAsn.MaskGenAlgorithm.Algorithm.Value);
			}
			if (!pssParamsAsn.MaskGenAlgorithm.Parameters.HasValue)
			{
				throw new CryptographicException("Invalid signature paramters.");
			}
			AlgorithmIdentifierAsn algorithmIdentifierAsn = AsnSerializer.Deserialize<AlgorithmIdentifierAsn>(pssParamsAsn.MaskGenAlgorithm.Parameters.Value, AsnEncodingRules.DER);
			if (algorithmIdentifierAsn.Algorithm.Value != digestAlgorithmOid)
			{
				throw new CryptographicException(global::SR.Format("This platform does not support the MGF hash algorithm ({0}) being different from the signature hash algorithm ({1}).", algorithmIdentifierAsn.Algorithm.Value, digestAlgorithmOid));
			}
			return RSASignaturePadding.Pss;
		}

		protected override bool Sign(byte[] dataHash, HashAlgorithmName hashAlgorithmName, X509Certificate2 certificate, bool silent, out Oid signatureAlgorithm, out byte[] signatureValue)
		{
			throw new CryptographicException();
		}
	}

	private static readonly Dictionary<string, CmsSignature> s_lookup;

	static CmsSignature()
	{
		s_lookup = new Dictionary<string, CmsSignature>();
		PrepareRegistrationRsa(s_lookup);
		PrepareRegistrationDsa(s_lookup);
		PrepareRegistrationECDsa(s_lookup);
	}

	private static void PrepareRegistrationRsa(Dictionary<string, CmsSignature> lookup)
	{
		lookup.Add("1.2.840.113549.1.1.1", new RSAPkcs1CmsSignature(null, null));
		lookup.Add("1.2.840.113549.1.1.5", new RSAPkcs1CmsSignature("1.2.840.113549.1.1.5", HashAlgorithmName.SHA1));
		lookup.Add("1.2.840.113549.1.1.11", new RSAPkcs1CmsSignature("1.2.840.113549.1.1.11", HashAlgorithmName.SHA256));
		lookup.Add("1.2.840.113549.1.1.12", new RSAPkcs1CmsSignature("1.2.840.113549.1.1.12", HashAlgorithmName.SHA384));
		lookup.Add("1.2.840.113549.1.1.13", new RSAPkcs1CmsSignature("1.2.840.113549.1.1.13", HashAlgorithmName.SHA512));
		lookup.Add("1.2.840.113549.1.1.10", new RSAPssCmsSignature());
	}

	private static void PrepareRegistrationDsa(Dictionary<string, CmsSignature> lookup)
	{
		lookup.Add("1.2.840.10040.4.3", new DSACmsSignature("1.2.840.10040.4.3", HashAlgorithmName.SHA1));
		lookup.Add("2.16.840.1.101.3.4.3.2", new DSACmsSignature("2.16.840.1.101.3.4.3.2", HashAlgorithmName.SHA256));
		lookup.Add("2.16.840.1.101.3.4.3.3", new DSACmsSignature("2.16.840.1.101.3.4.3.3", HashAlgorithmName.SHA384));
		lookup.Add("2.16.840.1.101.3.4.3.4", new DSACmsSignature("2.16.840.1.101.3.4.3.4", HashAlgorithmName.SHA512));
		lookup.Add("1.2.840.10040.4.1", new DSACmsSignature(null, default(HashAlgorithmName)));
	}

	private static void PrepareRegistrationECDsa(Dictionary<string, CmsSignature> lookup)
	{
		lookup.Add("1.2.840.10045.4.1", new ECDsaCmsSignature("1.2.840.10045.4.1", HashAlgorithmName.SHA1));
		lookup.Add("1.2.840.10045.4.3.2", new ECDsaCmsSignature("1.2.840.10045.4.3.2", HashAlgorithmName.SHA256));
		lookup.Add("1.2.840.10045.4.3.3", new ECDsaCmsSignature("1.2.840.10045.4.3.3", HashAlgorithmName.SHA384));
		lookup.Add("1.2.840.10045.4.3.4", new ECDsaCmsSignature("1.2.840.10045.4.3.4", HashAlgorithmName.SHA512));
		lookup.Add("1.2.840.10045.2.1", new ECDsaCmsSignature(null, default(HashAlgorithmName)));
	}

	internal abstract bool VerifySignature(byte[] valueHash, byte[] signature, string digestAlgorithmOid, HashAlgorithmName digestAlgorithmName, ReadOnlyMemory<byte>? signatureParameters, X509Certificate2 certificate);

	protected abstract bool Sign(byte[] dataHash, HashAlgorithmName hashAlgorithmName, X509Certificate2 certificate, bool silent, out Oid signatureAlgorithm, out byte[] signatureValue);

	internal static CmsSignature Resolve(string signatureAlgorithmOid)
	{
		if (s_lookup.TryGetValue(signatureAlgorithmOid, out var value))
		{
			return value;
		}
		return null;
	}

	internal static bool Sign(byte[] dataHash, HashAlgorithmName hashAlgorithmName, X509Certificate2 certificate, bool silent, out Oid oid, out ReadOnlyMemory<byte> signatureValue)
	{
		CmsSignature cmsSignature = Resolve(certificate.GetKeyAlgorithm());
		if (cmsSignature == null)
		{
			oid = null;
			signatureValue = default(ReadOnlyMemory<byte>);
			return false;
		}
		byte[] signatureValue2;
		bool result = cmsSignature.Sign(dataHash, hashAlgorithmName, certificate, silent, out oid, out signatureValue2);
		signatureValue = signatureValue2;
		return result;
	}

	private static bool DsaDerToIeee(ReadOnlyMemory<byte> derSignature, Span<byte> ieeeSignature)
	{
		int num = ieeeSignature.Length / 2;
		try
		{
			AsnReader asnReader = new AsnReader(derSignature, AsnEncodingRules.DER);
			AsnReader asnReader2 = asnReader.ReadSequence();
			if (asnReader.HasData)
			{
				return false;
			}
			ieeeSignature.Clear();
			ReadOnlyMemory<byte> integerBytes = asnReader2.GetIntegerBytes();
			ReadOnlySpan<byte> readOnlySpan = integerBytes.Span;
			if (readOnlySpan.Length > num && readOnlySpan[0] == 0)
			{
				readOnlySpan = readOnlySpan.Slice(1);
			}
			if (readOnlySpan.Length <= num)
			{
				readOnlySpan.CopyTo(ieeeSignature.Slice(num - readOnlySpan.Length, readOnlySpan.Length));
			}
			integerBytes = asnReader2.GetIntegerBytes();
			readOnlySpan = integerBytes.Span;
			if (readOnlySpan.Length > num && readOnlySpan[0] == 0)
			{
				readOnlySpan = readOnlySpan.Slice(1);
			}
			if (readOnlySpan.Length <= num)
			{
				readOnlySpan.CopyTo(ieeeSignature.Slice(num + num - readOnlySpan.Length, readOnlySpan.Length));
			}
			return !asnReader2.HasData;
		}
		catch (CryptographicException)
		{
			return false;
		}
	}

	private static byte[] DsaIeeeToDer(ReadOnlySpan<byte> ieeeSignature)
	{
		int num = ieeeSignature.Length / 2;
		using AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.DER);
		asnWriter.PushSequence();
		byte[] array = new byte[num + 1];
		Span<byte> destination = new Span<byte>(array, 1, num);
		ieeeSignature.Slice(0, num).CopyTo(destination);
		Array.Reverse(array);
		BigInteger value = new BigInteger(array);
		asnWriter.WriteInteger(value);
		array[0] = 0;
		ieeeSignature.Slice(num, num).CopyTo(destination);
		Array.Reverse(array);
		value = new BigInteger(array);
		asnWriter.WriteInteger(value);
		asnWriter.PopSequence();
		return asnWriter.Encode();
	}
}
