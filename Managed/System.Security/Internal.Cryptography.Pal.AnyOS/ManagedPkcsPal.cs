using System;
using System.Buffers;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Asn1;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.Pkcs.Asn1;
using System.Security.Cryptography.X509Certificates;

namespace Internal.Cryptography.Pal.AnyOS;

internal sealed class ManagedPkcsPal : PkcsPal
{
	private sealed class ManagedDecryptorPal : DecryptorPal
	{
		private byte[] _dataCopy;

		private EnvelopedDataAsn _envelopedData;

		public ManagedDecryptorPal(byte[] dataCopy, EnvelopedDataAsn envelopedDataAsn, RecipientInfoCollection recipientInfos)
			: base(recipientInfos)
		{
			_dataCopy = dataCopy;
			_envelopedData = envelopedDataAsn;
		}

		public unsafe override ContentInfo TryDecrypt(RecipientInfo recipientInfo, X509Certificate2 cert, X509Certificate2Collection originatorCerts, X509Certificate2Collection extraStore, out Exception exception)
		{
			if (recipientInfo.Pal is ManagedKeyTransPal managedKeyTransPal)
			{
				byte[] array = managedKeyTransPal.DecryptCek(cert, out exception);
				byte[] array2;
				fixed (byte* ptr = array)
				{
					try
					{
						if (exception != null)
						{
							return null;
						}
						ReadOnlyMemory<byte>? encryptedContent = _envelopedData.EncryptedContentInfo.EncryptedContent;
						if (!encryptedContent.HasValue)
						{
							exception = null;
							return new ContentInfo(new Oid(_envelopedData.EncryptedContentInfo.ContentType), Array.Empty<byte>());
						}
						array2 = DecryptContent(encryptedContent.Value, array, out exception);
					}
					finally
					{
						if (array != null)
						{
							Array.Clear(array, 0, array.Length);
						}
					}
				}
				if (exception != null)
				{
					return null;
				}
				if (_envelopedData.EncryptedContentInfo.ContentType == "1.2.840.113549.1.7.1")
				{
					byte[] array3 = null;
					try
					{
						AsnReader asnReader = new AsnReader(array2, AsnEncodingRules.BER);
						if (asnReader.TryGetPrimitiveOctetStringBytes(out var contents))
						{
							array2 = contents.ToArray();
						}
						else
						{
							array3 = ArrayPool<byte>.Shared.Rent(array2.Length);
							if (asnReader.TryCopyOctetStringBytes(array3, out var bytesWritten))
							{
								Span<byte> span = new Span<byte>(array3, 0, bytesWritten);
								array2 = span.ToArray();
								span.Clear();
							}
						}
					}
					catch (CryptographicException)
					{
					}
					finally
					{
						if (array3 != null)
						{
							ArrayPool<byte>.Shared.Return(array3);
						}
					}
				}
				else
				{
					array2 = GetAsnSequenceWithContentNoValidation(array2);
				}
				exception = null;
				return new ContentInfo(new Oid(_envelopedData.EncryptedContentInfo.ContentType), array2);
			}
			exception = new CryptographicException("The recipient type '{0}' is not supported for encryption or decryption on this platform.", recipientInfo.Type.ToString());
			return null;
		}

		private static byte[] GetAsnSequenceWithContentNoValidation(ReadOnlySpan<byte> content)
		{
			using AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.BER);
			asnWriter.WriteOctetString(content);
			byte[] array = asnWriter.Encode();
			array[0] = 48;
			return array;
		}

		private byte[] DecryptContent(ReadOnlyMemory<byte> encryptedContent, byte[] cek, out Exception exception)
		{
			exception = null;
			int length = encryptedContent.Length;
			byte[] array = ArrayPool<byte>.Shared.Rent(length);
			try
			{
				encryptedContent.CopyTo(array);
				using SymmetricAlgorithm symmetricAlgorithm = OpenAlgorithm(_envelopedData.EncryptedContentInfo.ContentEncryptionAlgorithm);
				using ICryptoTransform transform = symmetricAlgorithm.CreateDecryptor(cek, symmetricAlgorithm.IV);
				return transform.OneShot(array, 0, length);
			}
			catch (CryptographicException ex)
			{
				exception = ex;
				return null;
			}
			finally
			{
				Array.Clear(array, 0, length);
				ArrayPool<byte>.Shared.Return(array);
				array = null;
			}
		}

		public override void Dispose()
		{
		}
	}

	private sealed class ManagedKeyAgreePal : KeyAgreeRecipientInfoPal
	{
		private readonly KeyAgreeRecipientInfoAsn _asn;

		private readonly int _index;

		public override byte[] EncryptedKey => _asn.RecipientEncryptedKeys[_index].EncryptedKey.ToArray();

		public override AlgorithmIdentifier KeyEncryptionAlgorithm => _asn.KeyEncryptionAlgorithm.ToPresentationObject();

		public override SubjectIdentifier RecipientIdentifier => new SubjectIdentifier(_asn.RecipientEncryptedKeys[_index].Rid.IssuerAndSerialNumber, _asn.RecipientEncryptedKeys[_index].Rid.RKeyId?.SubjectKeyIdentifier);

		public override int Version => _asn.Version;

		public override DateTime Date
		{
			get
			{
				KeyAgreeRecipientIdentifierAsn rid = _asn.RecipientEncryptedKeys[_index].Rid;
				if (rid.RKeyId == null)
				{
					throw new InvalidOperationException("The Date property is not available for none KID key agree recipient.");
				}
				if (!rid.RKeyId.Date.HasValue)
				{
					return DateTime.FromFileTimeUtc(0L);
				}
				return rid.RKeyId.Date.Value.LocalDateTime;
			}
		}

		public override SubjectIdentifierOrKey OriginatorIdentifierOrKey => _asn.Originator.ToSubjectIdentifierOrKey();

		public override CryptographicAttributeObject OtherKeyAttribute
		{
			get
			{
				KeyAgreeRecipientIdentifierAsn rid = _asn.RecipientEncryptedKeys[_index].Rid;
				if (rid.RKeyId == null)
				{
					throw new InvalidOperationException("The Date property is not available for none KID key agree recipient.");
				}
				if (!rid.RKeyId.Other.HasValue)
				{
					return null;
				}
				Oid oid = new Oid(rid.RKeyId.Other.Value.KeyAttrId);
				byte[] encodedData = Array.Empty<byte>();
				if (rid.RKeyId.Other.Value.KeyAttr.HasValue)
				{
					encodedData = rid.RKeyId.Other.Value.KeyAttr.Value.ToArray();
				}
				AsnEncodedDataCollection values = new AsnEncodedDataCollection(new Pkcs9AttributeObject(oid, encodedData));
				return new CryptographicAttributeObject(oid, values);
			}
		}

		internal ManagedKeyAgreePal(KeyAgreeRecipientInfoAsn asn, int index)
		{
			_asn = asn;
			_index = index;
		}
	}

	private sealed class ManagedKeyTransPal : KeyTransRecipientInfoPal
	{
		private readonly KeyTransRecipientInfoAsn _asn;

		public override byte[] EncryptedKey => _asn.EncryptedKey.ToArray();

		public override AlgorithmIdentifier KeyEncryptionAlgorithm => _asn.KeyEncryptionAlgorithm.ToPresentationObject();

		public override SubjectIdentifier RecipientIdentifier => new SubjectIdentifier(_asn.Rid.IssuerAndSerialNumber, _asn.Rid.SubjectKeyIdentifier);

		public override int Version => _asn.Version;

		internal ManagedKeyTransPal(KeyTransRecipientInfoAsn asn)
		{
			_asn = asn;
		}

		internal byte[] DecryptCek(X509Certificate2 cert, out Exception exception)
		{
			ReadOnlyMemory<byte>? parameters = _asn.KeyEncryptionAlgorithm.Parameters;
			string value = _asn.KeyEncryptionAlgorithm.Algorithm.Value;
			RSAEncryptionPadding padding;
			if (!(value == "1.2.840.113549.1.1.1"))
			{
				if (!(value == "1.2.840.113549.1.1.7"))
				{
					exception = new CryptographicException("Unknown algorithm '{0}'.", _asn.KeyEncryptionAlgorithm.Algorithm.Value);
					return null;
				}
				if (parameters.HasValue && !parameters.Value.Span.SequenceEqual(s_rsaOaepSha1Parameters))
				{
					exception = new CryptographicException("ASN1 corrupted data.");
					return null;
				}
				padding = RSAEncryptionPadding.OaepSHA1;
			}
			else
			{
				if (parameters.HasValue && !parameters.Value.Span.SequenceEqual(s_rsaPkcsParameters))
				{
					exception = new CryptographicException("ASN1 corrupted data.");
					return null;
				}
				padding = RSAEncryptionPadding.Pkcs1;
			}
			byte[] array = null;
			int length = 0;
			try
			{
				using RSA rSA = cert.GetRSAPrivateKey();
				if (rSA == null)
				{
					exception = new CryptographicException("A certificate with a private key is required.");
					return null;
				}
				exception = null;
				return rSA.Decrypt(_asn.EncryptedKey.Span.ToArray(), padding);
			}
			catch (CryptographicException ex)
			{
				exception = ex;
				return null;
			}
			finally
			{
				if (array != null)
				{
					Array.Clear(array, 0, length);
					ArrayPool<byte>.Shared.Return(array);
				}
			}
		}
	}

	private static readonly byte[] s_invalidEmptyOid = new byte[2] { 6, 0 };

	private static readonly byte[] s_rsaPkcsParameters = new byte[2] { 5, 0 };

	private static readonly byte[] s_rsaOaepSha1Parameters = new byte[2] { 48, 0 };

	public override byte[] EncodeOctetString(byte[] octets)
	{
		using AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.DER);
		asnWriter.WriteOctetString(octets);
		return asnWriter.Encode();
	}

	public override byte[] DecodeOctetString(byte[] encodedOctets)
	{
		AsnReader asnReader = new AsnReader(encodedOctets, AsnEncodingRules.BER);
		Span<byte> destination = stackalloc byte[256];
		ReadOnlySpan<byte> readOnlySpan = default(Span<byte>);
		byte[] array = null;
		try
		{
			if (!asnReader.TryGetPrimitiveOctetStringBytes(out var contents))
			{
				if (asnReader.TryCopyOctetStringBytes(destination, out var bytesWritten))
				{
					readOnlySpan = destination.Slice(0, bytesWritten);
				}
				else
				{
					array = ArrayPool<byte>.Shared.Rent(asnReader.PeekContentBytes().Length);
					if (!asnReader.TryCopyOctetStringBytes(array, out bytesWritten))
					{
						throw new CryptographicException();
					}
					readOnlySpan = new ReadOnlySpan<byte>(array, 0, bytesWritten);
				}
			}
			else
			{
				readOnlySpan = contents.Span;
			}
			if (asnReader.HasData)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			return readOnlySpan.ToArray();
		}
		finally
		{
			if (array != null)
			{
				Array.Clear(array, 0, readOnlySpan.Length);
				ArrayPool<byte>.Shared.Return(array);
			}
		}
	}

	public override byte[] EncodeUtcTime(DateTime utcTime)
	{
		using AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.DER);
		try
		{
			if (utcTime.Kind == DateTimeKind.Unspecified)
			{
				asnWriter.WriteUtcTime(utcTime.ToLocalTime(), 1950);
			}
			else
			{
				asnWriter.WriteUtcTime(utcTime, 1950);
			}
			return asnWriter.Encode();
		}
		catch (ArgumentException ex)
		{
			throw new CryptographicException(ex.Message, ex);
		}
	}

	public override DateTime DecodeUtcTime(byte[] encodedUtcTime)
	{
		AsnReader asnReader = new AsnReader(encodedUtcTime, AsnEncodingRules.BER);
		DateTimeOffset utcTime = asnReader.GetUtcTime();
		if (asnReader.HasData)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		return utcTime.UtcDateTime;
	}

	public override string DecodeOid(byte[] encodedOid)
	{
		if (s_invalidEmptyOid.AsSpan().SequenceEqual(encodedOid))
		{
			return string.Empty;
		}
		AsnReader asnReader = new AsnReader(encodedOid, AsnEncodingRules.BER);
		string result = asnReader.ReadObjectIdentifierAsString();
		if (asnReader.HasData)
		{
			throw new CryptographicException("ASN1 corrupted data.");
		}
		return result;
	}

	public override Oid GetEncodedMessageType(byte[] encodedMessage)
	{
		ContentInfoAsn contentInfoAsn = AsnSerializer.Deserialize<ContentInfoAsn>(new AsnReader(encodedMessage, AsnEncodingRules.BER).GetEncodedValue(), AsnEncodingRules.BER);
		switch (contentInfoAsn.ContentType)
		{
		case "1.2.840.113549.1.7.1":
		case "1.2.840.113549.1.7.2":
		case "1.2.840.113549.1.7.3":
		case "1.2.840.113549.1.7.4":
		case "1.2.840.113549.1.7.5":
		case "1.2.840.113549.1.7.6":
			return new Oid(contentInfoAsn.ContentType);
		default:
			throw new CryptographicException("Invalid cryptographic message type.");
		}
	}

	public override DecryptorPal Decode(byte[] encodedMessage, out int version, out ContentInfo contentInfo, out AlgorithmIdentifier contentEncryptionAlgorithm, out X509Certificate2Collection originatorCerts, out CryptographicAttributeObjectCollection unprotectedAttributes)
	{
		ContentInfoAsn contentInfoAsn = AsnSerializer.Deserialize<ContentInfoAsn>(new AsnReader(encodedMessage, AsnEncodingRules.BER).GetEncodedValue(), AsnEncodingRules.BER);
		if (contentInfoAsn.ContentType != "1.2.840.113549.1.7.3")
		{
			throw new CryptographicException("Invalid cryptographic message type.");
		}
		byte[] array = contentInfoAsn.Content.ToArray();
		EnvelopedDataAsn envelopedDataAsn = AsnSerializer.Deserialize<EnvelopedDataAsn>(array, AsnEncodingRules.BER);
		version = envelopedDataAsn.Version;
		contentInfo = new ContentInfo(new Oid(envelopedDataAsn.EncryptedContentInfo.ContentType), envelopedDataAsn.EncryptedContentInfo.EncryptedContent?.ToArray() ?? Array.Empty<byte>());
		contentEncryptionAlgorithm = envelopedDataAsn.EncryptedContentInfo.ContentEncryptionAlgorithm.ToPresentationObject();
		originatorCerts = new X509Certificate2Collection();
		if (envelopedDataAsn.OriginatorInfo != null && envelopedDataAsn.OriginatorInfo.CertificateSet != null)
		{
			CertificateChoiceAsn[] certificateSet = envelopedDataAsn.OriginatorInfo.CertificateSet;
			for (int i = 0; i < certificateSet.Length; i++)
			{
				CertificateChoiceAsn certificateChoiceAsn = certificateSet[i];
				if (certificateChoiceAsn.Certificate.HasValue)
				{
					originatorCerts.Add(new X509Certificate2(certificateChoiceAsn.Certificate.Value.ToArray()));
				}
			}
		}
		unprotectedAttributes = SignerInfo.MakeAttributeCollection(envelopedDataAsn.UnprotectedAttributes);
		List<RecipientInfo> list = new List<RecipientInfo>();
		RecipientInfoAsn[] recipientInfos = envelopedDataAsn.RecipientInfos;
		for (int i = 0; i < recipientInfos.Length; i++)
		{
			RecipientInfoAsn recipientInfoAsn = recipientInfos[i];
			if (recipientInfoAsn.Ktri != null)
			{
				list.Add(new KeyTransRecipientInfo(new ManagedKeyTransPal(recipientInfoAsn.Ktri)));
				continue;
			}
			if (recipientInfoAsn.Kari != null)
			{
				for (int j = 0; j < recipientInfoAsn.Kari.RecipientEncryptedKeys.Length; j++)
				{
					list.Add(new KeyAgreeRecipientInfo(new ManagedKeyAgreePal(recipientInfoAsn.Kari, j)));
				}
				continue;
			}
			throw new CryptographicException();
		}
		return new ManagedDecryptorPal(array, envelopedDataAsn, new RecipientInfoCollection(list));
	}

	public unsafe override byte[] Encrypt(CmsRecipientCollection recipients, ContentInfo contentInfo, AlgorithmIdentifier contentEncryptionAlgorithm, X509Certificate2Collection originatorCerts, CryptographicAttributeObjectCollection unprotectedAttributes)
	{
		byte[] cek;
		byte[] parameterBytes;
		byte[] encryptedContent = EncryptContent(contentInfo, contentEncryptionAlgorithm, out cek, out parameterBytes);
		fixed (byte* ptr = cek)
		{
			try
			{
				return Encrypt(recipients, contentInfo, contentEncryptionAlgorithm, originatorCerts, unprotectedAttributes, encryptedContent, cek, parameterBytes);
			}
			finally
			{
				Array.Clear(cek, 0, cek.Length);
			}
		}
	}

	private static byte[] Encrypt(CmsRecipientCollection recipients, ContentInfo contentInfo, AlgorithmIdentifier contentEncryptionAlgorithm, X509Certificate2Collection originatorCerts, CryptographicAttributeObjectCollection unprotectedAttributes, byte[] encryptedContent, byte[] cek, byte[] parameterBytes)
	{
		EnvelopedDataAsn value = new EnvelopedDataAsn
		{
			EncryptedContentInfo = 
			{
				ContentType = contentInfo.ContentType.Value,
				ContentEncryptionAlgorithm = 
				{
					Algorithm = contentEncryptionAlgorithm.Oid,
					Parameters = parameterBytes
				},
				EncryptedContent = encryptedContent
			}
		};
		if (unprotectedAttributes != null && unprotectedAttributes.Count > 0)
		{
			List<AttributeAsn> list = CmsSigner.BuildAttributes(unprotectedAttributes);
			value.UnprotectedAttributes = Helpers.NormalizeSet(list.ToArray());
		}
		if (originatorCerts != null && originatorCerts.Count > 0)
		{
			CertificateChoiceAsn[] array = new CertificateChoiceAsn[originatorCerts.Count];
			for (int i = 0; i < originatorCerts.Count; i++)
			{
				array[i].Certificate = originatorCerts[i].RawData;
			}
			value.OriginatorInfo = new OriginatorInfoAsn
			{
				CertificateSet = array
			};
		}
		value.RecipientInfos = new RecipientInfoAsn[recipients.Count];
		bool flag = true;
		for (int j = 0; j < recipients.Count; j++)
		{
			CmsRecipient cmsRecipient = recipients[j];
			if (cmsRecipient.Certificate.GetKeyAlgorithm() == "1.2.840.113549.1.1.1")
			{
				value.RecipientInfos[j].Ktri = MakeKtri(cek, cmsRecipient, out var v0Recipient);
				flag = flag && v0Recipient;
				continue;
			}
			throw new CryptographicException("Unknown algorithm '{0}'.", cmsRecipient.Certificate.GetKeyAlgorithm());
		}
		if (value.OriginatorInfo != null || !flag || value.UnprotectedAttributes != null)
		{
			value.Version = 2;
		}
		return Helpers.EncodeContentInfo(value, "1.2.840.113549.1.7.3");
	}

	private byte[] EncryptContent(ContentInfo contentInfo, AlgorithmIdentifier contentEncryptionAlgorithm, out byte[] cek, out byte[] parameterBytes)
	{
		using SymmetricAlgorithm symmetricAlgorithm = OpenAlgorithm(contentEncryptionAlgorithm);
		using ICryptoTransform transform = symmetricAlgorithm.CreateEncryptor();
		cek = symmetricAlgorithm.Key;
		if (symmetricAlgorithm is RC2)
		{
			using AsnWriter asnWriter = AsnSerializer.Serialize(new Rc2CbcParameters(symmetricAlgorithm.IV, symmetricAlgorithm.KeySize), AsnEncodingRules.DER);
			parameterBytes = asnWriter.Encode();
		}
		else
		{
			parameterBytes = EncodeOctetString(symmetricAlgorithm.IV);
		}
		byte[] content = contentInfo.Content;
		if (contentInfo.ContentType.Value == "1.2.840.113549.1.7.1")
		{
			content = EncodeOctetString(content);
			return transform.OneShot(content);
		}
		if (contentInfo.Content.Length == 0)
		{
			return transform.OneShot(contentInfo.Content);
		}
		AsnReader asnReader = new AsnReader(contentInfo.Content, AsnEncodingRules.BER);
		return transform.OneShot(asnReader.PeekContentBytes().ToArray());
	}

	public override Exception CreateRecipientsNotFoundException()
	{
		return new CryptographicException("The enveloped-data message does not contain the specified recipient.");
	}

	public override Exception CreateRecipientInfosAfterEncryptException()
	{
		return CreateInvalidMessageTypeException();
	}

	public override Exception CreateDecryptAfterEncryptException()
	{
		return CreateInvalidMessageTypeException();
	}

	public override Exception CreateDecryptTwiceException()
	{
		return CreateInvalidMessageTypeException();
	}

	private static Exception CreateInvalidMessageTypeException()
	{
		return new CryptographicException("Invalid cryptographic message type.");
	}

	private static KeyTransRecipientInfoAsn MakeKtri(byte[] cek, CmsRecipient recipient, out bool v0Recipient)
	{
		KeyTransRecipientInfoAsn keyTransRecipientInfoAsn = new KeyTransRecipientInfoAsn();
		if (recipient.RecipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier)
		{
			keyTransRecipientInfoAsn.Version = 2;
			keyTransRecipientInfoAsn.Rid.SubjectKeyIdentifier = recipient.Certificate.GetSubjectKeyIdentifier();
		}
		else
		{
			if (recipient.RecipientIdentifierType != SubjectIdentifierType.IssuerAndSerialNumber)
			{
				throw new CryptographicException("The subject identifier type {0} is not valid.", recipient.RecipientIdentifierType.ToString());
			}
			byte[] serialNumber = recipient.Certificate.GetSerialNumber();
			Array.Reverse(serialNumber);
			IssuerAndSerialNumberAsn value = new IssuerAndSerialNumberAsn
			{
				Issuer = recipient.Certificate.IssuerName.RawData,
				SerialNumber = serialNumber
			};
			keyTransRecipientInfoAsn.Rid.IssuerAndSerialNumber = value;
		}
		RSAEncryptionPadding padding;
		if (recipient.Certificate.GetKeyAlgorithm() == "1.2.840.113549.1.1.7")
		{
			padding = RSAEncryptionPadding.OaepSHA1;
			keyTransRecipientInfoAsn.KeyEncryptionAlgorithm.Algorithm = new Oid("1.2.840.113549.1.1.7", "1.2.840.113549.1.1.7");
			keyTransRecipientInfoAsn.KeyEncryptionAlgorithm.Parameters = s_rsaOaepSha1Parameters;
		}
		else
		{
			padding = RSAEncryptionPadding.Pkcs1;
			keyTransRecipientInfoAsn.KeyEncryptionAlgorithm.Algorithm = new Oid("1.2.840.113549.1.1.1", "1.2.840.113549.1.1.1");
			keyTransRecipientInfoAsn.KeyEncryptionAlgorithm.Parameters = s_rsaPkcsParameters;
		}
		using (RSA rSA = recipient.Certificate.GetRSAPublicKey())
		{
			keyTransRecipientInfoAsn.EncryptedKey = rSA.Encrypt(cek, padding);
		}
		v0Recipient = keyTransRecipientInfoAsn.Version == 0;
		return keyTransRecipientInfoAsn;
	}

	public override void AddCertsFromStoreForDecryption(X509Certificate2Collection certs)
	{
		certs.AddRange(Helpers.GetStoreCertificates(StoreName.My, StoreLocation.CurrentUser, openExistingOnly: false));
		try
		{
			certs.AddRange(Helpers.GetStoreCertificates(StoreName.My, StoreLocation.LocalMachine, openExistingOnly: false));
		}
		catch (CryptographicException)
		{
		}
	}

	public override byte[] GetSubjectKeyIdentifier(X509Certificate2 certificate)
	{
		return certificate.GetSubjectKeyIdentifier();
	}

	public override T GetPrivateKeyForSigning<T>(X509Certificate2 certificate, bool silent)
	{
		return GetPrivateKey<T>(certificate);
	}

	public override T GetPrivateKeyForDecryption<T>(X509Certificate2 certificate, bool silent)
	{
		return GetPrivateKey<T>(certificate);
	}

	private T GetPrivateKey<T>(X509Certificate2 certificate) where T : AsymmetricAlgorithm
	{
		if (typeof(T) == typeof(RSA))
		{
			return (T)(AsymmetricAlgorithm)certificate.GetRSAPrivateKey();
		}
		if (typeof(T) == typeof(ECDsa))
		{
			return (T)(AsymmetricAlgorithm)certificate.GetECDsaPrivateKey();
		}
		return null;
	}

	private static SymmetricAlgorithm OpenAlgorithm(AlgorithmIdentifierAsn contentEncryptionAlgorithm)
	{
		SymmetricAlgorithm symmetricAlgorithm = OpenAlgorithm(contentEncryptionAlgorithm.Algorithm);
		if (symmetricAlgorithm is RC2)
		{
			if (!contentEncryptionAlgorithm.Parameters.HasValue)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			Rc2CbcParameters rc2CbcParameters = AsnSerializer.Deserialize<Rc2CbcParameters>(contentEncryptionAlgorithm.Parameters.Value, AsnEncodingRules.BER);
			symmetricAlgorithm.KeySize = rc2CbcParameters.GetEffectiveKeyBits();
			symmetricAlgorithm.IV = rc2CbcParameters.Iv.ToArray();
		}
		else
		{
			if (!contentEncryptionAlgorithm.Parameters.HasValue)
			{
				throw new CryptographicException("ASN1 corrupted data.");
			}
			AsnReader asnReader = new AsnReader(contentEncryptionAlgorithm.Parameters.Value, AsnEncodingRules.BER);
			if (asnReader.TryGetPrimitiveOctetStringBytes(out var contents))
			{
				symmetricAlgorithm.IV = contents.ToArray();
			}
			else
			{
				byte[] array = new byte[symmetricAlgorithm.BlockSize / 8];
				if (!asnReader.TryCopyOctetStringBytes(array, out var bytesWritten) || bytesWritten != array.Length)
				{
					throw new CryptographicException("ASN1 corrupted data.");
				}
				symmetricAlgorithm.IV = array;
			}
		}
		return symmetricAlgorithm;
	}

	private static SymmetricAlgorithm OpenAlgorithm(AlgorithmIdentifier algorithmIdentifier)
	{
		SymmetricAlgorithm symmetricAlgorithm = OpenAlgorithm(algorithmIdentifier.Oid);
		if (symmetricAlgorithm is RC2)
		{
			if (algorithmIdentifier.KeyLength != 0)
			{
				symmetricAlgorithm.KeySize = algorithmIdentifier.KeyLength;
			}
			else
			{
				symmetricAlgorithm.KeySize = 128;
			}
		}
		return symmetricAlgorithm;
	}

	private static SymmetricAlgorithm OpenAlgorithm(Oid algorithmIdentifier)
	{
		SymmetricAlgorithm symmetricAlgorithm;
		switch (algorithmIdentifier.Value)
		{
		case "1.2.840.113549.3.2":
			symmetricAlgorithm = RC2.Create();
			break;
		case "1.3.14.3.2.7":
			symmetricAlgorithm = DES.Create();
			break;
		case "1.2.840.113549.3.7":
			symmetricAlgorithm = TripleDES.Create();
			break;
		case "2.16.840.1.101.3.4.1.2":
			symmetricAlgorithm = Aes.Create();
			symmetricAlgorithm.KeySize = 128;
			break;
		case "2.16.840.1.101.3.4.1.22":
			symmetricAlgorithm = Aes.Create();
			symmetricAlgorithm.KeySize = 192;
			break;
		case "2.16.840.1.101.3.4.1.42":
			symmetricAlgorithm = Aes.Create();
			symmetricAlgorithm.KeySize = 256;
			break;
		default:
			throw new CryptographicException("Unknown algorithm '{0}'.", algorithmIdentifier.Value);
		}
		symmetricAlgorithm.Padding = PaddingMode.PKCS7;
		symmetricAlgorithm.Mode = CipherMode.CBC;
		return symmetricAlgorithm;
	}
}
