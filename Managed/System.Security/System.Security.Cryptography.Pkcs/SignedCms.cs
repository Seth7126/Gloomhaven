using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Asn1;
using System.Security.Cryptography.Pkcs.Asn1;
using System.Security.Cryptography.X509Certificates;
using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class enables signing and verifying of CMS/PKCS #7 messages. </summary>
public sealed class SignedCms
{
	private static readonly Oid s_cmsDataOid = Oid.FromOidValue("1.2.840.113549.1.7.1", OidGroup.ExtensionOrAttribute);

	private SignedDataAsn _signedData;

	private bool _hasData;

	private Memory<byte> _heldData;

	private ReadOnlyMemory<byte>? _heldContent;

	private bool _hasPkcs7Content;

	private string _contentType;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.Version" /> property retrieves the version of the CMS/PKCS #7 message.</summary>
	/// <returns>An int value that represents the CMS/PKCS #7 message version.</returns>
	public int Version { get; private set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.ContentInfo" /> property retrieves the inner contents of the encoded CMS/PKCS #7 message.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the contents of the encoded CMS/PKCS #7 message.</returns>
	public ContentInfo ContentInfo { get; private set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.Detached" /> property retrieves whether the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object is for a detached signature.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object is for a detached signature. If this property is true, the signature is detached. If this property is false, the signature is not detached.</returns>
	public bool Detached { get; private set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.Certificates" /> property retrieves the certificates associated with the encoded CMS/PKCS #7 message.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents the set of certificates for the encoded CMS/PKCS #7 message.</returns>
	public X509Certificate2Collection Certificates
	{
		get
		{
			X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
			if (!_hasData)
			{
				return x509Certificate2Collection;
			}
			CertificateChoiceAsn[] certificateSet = _signedData.CertificateSet;
			if (certificateSet == null)
			{
				return x509Certificate2Collection;
			}
			CertificateChoiceAsn[] array = certificateSet;
			for (int i = 0; i < array.Length; i++)
			{
				CertificateChoiceAsn certificateChoiceAsn = array[i];
				x509Certificate2Collection.Add(new X509Certificate2(certificateChoiceAsn.Certificate.Value.ToArray()));
			}
			return x509Certificate2Collection;
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.SignerInfos" /> property retrieves the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection associated with the CMS/PKCS #7 message.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> object that represents the signer information for the CMS/PKCS #7 message.</returns>
	public SignerInfoCollection SignerInfos
	{
		get
		{
			if (!_hasData)
			{
				return new SignerInfoCollection();
			}
			return new SignerInfoCollection(_signedData.SignerInfos, this);
		}
	}

	private static ContentInfo MakeEmptyContentInfo()
	{
		return new ContentInfo(new Oid(s_cmsDataOid), Array.Empty<byte>());
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class.</summary>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms()
		: this(SubjectIdentifierType.IssuerAndSerialNumber, MakeEmptyContentInfo(), detached: false)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified subject identifier type as the default subject identifier type for signers.</summary>
	/// <param name="signerIdentifierType">A <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> member that specifies the default subject identifier type for signers.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms(SubjectIdentifierType signerIdentifierType)
		: this(signerIdentifierType, MakeEmptyContentInfo(), detached: false)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.ContentInfo)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified content information as the inner content.</summary>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that specifies the content information as the inner content of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms(ContentInfo contentInfo)
		: this(SubjectIdentifierType.IssuerAndSerialNumber, contentInfo, detached: false)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.Pkcs.ContentInfo)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified subject identifier type as the default subject identifier type for signers and content information as the inner content.</summary>
	/// <param name="signerIdentifierType">A <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> member that specifies the default subject identifier type for signers.</param>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that specifies the content information as the inner content of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms(SubjectIdentifierType signerIdentifierType, ContentInfo contentInfo)
		: this(signerIdentifierType, contentInfo, detached: false)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.ContentInfo,System.Boolean)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified content information as the inner content and by using the detached state.</summary>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that specifies the content information as the inner content of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message.</param>
	/// <param name="detached">A <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object is for a detached signature. If <paramref name="detached" /> is true, the signature is detached. If <paramref name="detached" /> is false, the signature is not detached.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public SignedCms(ContentInfo contentInfo, bool detached)
		: this(SubjectIdentifierType.IssuerAndSerialNumber, contentInfo, detached)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.Pkcs.ContentInfo,System.Boolean)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> class by using the specified subject identifier type as the default subject identifier type for signers, the content information as the inner content, and by using the detached state.</summary>
	/// <param name="signerIdentifierType">A <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> member that specifies the default subject identifier type for signers.</param>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that specifies the content information as the inner content of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message.</param>
	/// <param name="detached">A <see cref="T:System.Boolean" /> value that specifies whether the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object is for a detached signature. If <paramref name="detached" /> is true, the signature is detached. If detached is false, the signature is not detached.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	public SignedCms(SubjectIdentifierType signerIdentifierType, ContentInfo contentInfo, bool detached)
	{
		if (contentInfo == null)
		{
			throw new ArgumentNullException("contentInfo");
		}
		if (contentInfo.Content == null)
		{
			throw new ArgumentNullException("contentInfo.Content");
		}
		ContentInfo = contentInfo;
		Detached = detached;
		Version = 0;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.Encode" /> method encodes the information in the object into a CMS/PKCS #7 message.</summary>
	/// <returns>An array of byte values that represents the encoded message. The encoded message can be decoded by the <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.Decode(System.Byte[])" /> method.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] Encode()
	{
		if (!_hasData)
		{
			throw new InvalidOperationException("The CMS message is not signed.");
		}
		try
		{
			return Helpers.EncodeContentInfo(_signedData, "1.2.840.113549.1.7.2");
		}
		catch (CryptographicException)
		{
			if (Detached)
			{
				throw;
			}
			SignedDataAsn value = _signedData;
			value.EncapContentInfo.Content = null;
			using (AsnWriter asnWriter = AsnSerializer.Serialize(value, AsnEncodingRules.DER))
			{
				value = AsnSerializer.Deserialize<SignedDataAsn>(asnWriter.Encode(), AsnEncodingRules.BER);
			}
			value.EncapContentInfo.Content = _signedData.EncapContentInfo.Content;
			return Helpers.EncodeContentInfo(value, "1.2.840.113549.1.7.2", AsnEncodingRules.BER);
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.Decode(System.Byte[])" /> method decodes an encoded <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> message. Upon successful decoding, the decoded information can be retrieved from the properties of the <see cref="T:System.Security.Cryptography.Pkcs.SignedCms" /> object.</summary>
	/// <param name="encodedMessage">Array of byte values that represents the encoded CMS/PKCS #7 message to be decoded.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public void Decode(byte[] encodedMessage)
	{
		if (encodedMessage == null)
		{
			throw new ArgumentNullException("encodedMessage");
		}
		Decode(new ReadOnlyMemory<byte>(encodedMessage));
	}

	internal void Decode(ReadOnlyMemory<byte> encodedMessage)
	{
		int bytesRead;
		ContentInfoAsn contentInfoAsn = AsnSerializer.Deserialize<ContentInfoAsn>(encodedMessage, AsnEncodingRules.BER, out bytesRead);
		if (contentInfoAsn.ContentType != "1.2.840.113549.1.7.2")
		{
			throw new CryptographicException("Invalid cryptographic message type.");
		}
		_heldData = contentInfoAsn.Content.ToArray();
		_signedData = AsnSerializer.Deserialize<SignedDataAsn>(_heldData, AsnEncodingRules.BER);
		_contentType = _signedData.EncapContentInfo.ContentType;
		_hasPkcs7Content = false;
		if (!Detached)
		{
			ReadOnlyMemory<byte>? content = _signedData.EncapContentInfo.Content;
			ReadOnlyMemory<byte> value;
			if (content.HasValue)
			{
				value = GetContent(content.Value, _contentType);
				_hasPkcs7Content = content.Value.Length == value.Length;
			}
			else
			{
				value = ReadOnlyMemory<byte>.Empty;
			}
			_heldContent = value;
			ContentInfo = new ContentInfo(new Oid(_contentType), value.ToArray());
		}
		else
		{
			_heldContent = ContentInfo.Content.CloneByteArray();
		}
		Version = _signedData.Version;
		_hasData = true;
	}

	internal static ReadOnlyMemory<byte> GetContent(ReadOnlyMemory<byte> wrappedContent, string contentType)
	{
		byte[] array = null;
		int bytesWritten = 0;
		try
		{
			AsnReader asnReader = new AsnReader(wrappedContent, AsnEncodingRules.BER);
			if (asnReader.TryGetPrimitiveOctetStringBytes(out var contents))
			{
				return contents;
			}
			array = ArrayPool<byte>.Shared.Rent(wrappedContent.Length);
			if (!asnReader.TryCopyOctetStringBytes(array, out bytesWritten))
			{
				throw new CryptographicException();
			}
			return array.AsSpan(0, bytesWritten).ToArray();
		}
		catch (Exception)
		{
			if (contentType == "1.2.840.113549.1.7.1")
			{
				throw;
			}
			return wrappedContent;
		}
		finally
		{
			if (array != null)
			{
				array.AsSpan(0, bytesWritten).Clear();
				ArrayPool<byte>.Shared.Return(array);
			}
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.ComputeSignature" /> method prompts the user to select a signing certificate, creates a signature, and adds the signature to the CMS/PKCS #7 message.</summary>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.StorePermission, System.Security, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Flags="CreateStore, DeleteStore, OpenStore, EnumerateCertificates" />
	/// </PermissionSet>
	public void ComputeSignature()
	{
		throw new PlatformNotSupportedException("No signer certificate was provided. This platform does not implement the certificate picker UI.");
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.ComputeSignature(System.Security.Cryptography.Pkcs.CmsSigner)" /> method creates a signature using the specified signer and adds the signature to the CMS/PKCS #7 message.</summary>
	/// <param name="signer">A <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object that represents the signer.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public void ComputeSignature(CmsSigner signer)
	{
		ComputeSignature(signer, silent: true);
	}

	/// <summary>Creates a signature using the specified signer and adds the signature to the CMS/PKCS #7 message. If the value of the silent parameter is false and the <see cref="P:System.Security.Cryptography.Pkcs.CmsSigner.Certificate" /> property of the <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object specified by the signer parameter is not set to a valid certificate, this method prompts the user to select a signing certificate.</summary>
	/// <param name="signer">A <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" /> object that represents the signer.</param>
	/// <param name="silent">false to prompt the user to select a signing certificate.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">The value of the <paramref name="silent" /> parameter is true and a signing certificate is not specified.</exception>
	public void ComputeSignature(CmsSigner signer, bool silent)
	{
		if (signer == null)
		{
			throw new ArgumentNullException("signer");
		}
		if (ContentInfo.Content.Length == 0)
		{
			throw new CryptographicException("Cannot create CMS signature for empty content.");
		}
		ReadOnlyMemory<byte> data = _heldContent ?? ((ReadOnlyMemory<byte>)ContentInfo.Content);
		string text = _contentType ?? ContentInfo.ContentType.Value ?? "1.2.840.113549.1.7.1";
		X509Certificate2Collection chainCerts;
		SignerInfoAsn signerInfoAsn = signer.Sign(data, text, silent, out chainCerts);
		bool flag = false;
		if (!_hasData)
		{
			flag = true;
			_signedData = new SignedDataAsn
			{
				DigestAlgorithms = Array.Empty<AlgorithmIdentifierAsn>(),
				SignerInfos = Array.Empty<SignerInfoAsn>(),
				EncapContentInfo = new EncapsulatedContentInfoAsn
				{
					ContentType = text
				}
			};
			if (!Detached)
			{
				using AsnWriter asnWriter = new AsnWriter(AsnEncodingRules.DER);
				asnWriter.WriteOctetString(data.Span);
				_signedData.EncapContentInfo.Content = asnWriter.Encode();
			}
			_hasData = true;
		}
		int num = _signedData.SignerInfos.Length;
		Array.Resize(ref _signedData.SignerInfos, num + 1);
		_signedData.SignerInfos[num] = signerInfoAsn;
		UpdateCertificatesFromAddition(chainCerts);
		ConsiderDigestAddition(signerInfoAsn.DigestAlgorithm);
		UpdateMetadata();
		if (flag)
		{
			Reencode();
		}
	}

	/// <summary>Removes the signature at the specified index of the <see cref="P:System.Security.Cryptography.Pkcs.SignedCms.SignerInfos" /> collection. </summary>
	/// <param name="index">The zero-based index of the signature to remove.</param>
	/// <exception cref="T:System.InvalidOperationException">A CMS/PKCS #7 message is not signed, and <paramref name="index" /> is invalid.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than the signature count minus 1.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The signature could not be removed.-or-An internal cryptographic error occurred.</exception>
	public void RemoveSignature(int index)
	{
		if (!_hasData)
		{
			throw new InvalidOperationException("The CMS message is not signed.");
		}
		if (index < 0 || index >= _signedData.SignerInfos.Length)
		{
			throw new ArgumentOutOfRangeException("index", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		AlgorithmIdentifierAsn digestAlgorithm = _signedData.SignerInfos[index].DigestAlgorithm;
		Helpers.RemoveAt(ref _signedData.SignerInfos, index);
		ConsiderDigestRemoval(digestAlgorithm);
		UpdateMetadata();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.RemoveSignature(System.Security.Cryptography.Pkcs.SignerInfo)" /> method removes the signature for the specified <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object.</summary>
	/// <param name="signerInfo">A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object that represents the countersignature being removed.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public void RemoveSignature(SignerInfo signerInfo)
	{
		if (signerInfo == null)
		{
			throw new ArgumentNullException("signerInfo");
		}
		int num = SignerInfos.FindIndexForSigner(signerInfo);
		if (num < 0)
		{
			throw new CryptographicException("Cannot find the original signer.");
		}
		RemoveSignature(num);
	}

	internal ReadOnlySpan<byte> GetHashableContentSpan()
	{
		ReadOnlyMemory<byte> value = _heldContent.Value;
		if (!_hasPkcs7Content)
		{
			return value.Span;
		}
		return new AsnReader(value, AsnEncodingRules.BER).PeekContentBytes().Span;
	}

	internal void Reencode()
	{
		ContentInfo contentInfo = ContentInfo;
		try
		{
			byte[] encodedMessage = Encode();
			if (Detached)
			{
				_heldContent = null;
			}
			Decode(encodedMessage);
		}
		finally
		{
			ContentInfo = contentInfo;
		}
	}

	private void UpdateMetadata()
	{
		int version = 1;
		if ((_contentType ?? ContentInfo.ContentType.Value) != "1.2.840.113549.1.7.1")
		{
			version = 3;
		}
		else if (_signedData.SignerInfos.Any((SignerInfoAsn si) => si.Version == 3))
		{
			version = 3;
		}
		Version = version;
		_signedData.Version = version;
	}

	private void ConsiderDigestAddition(AlgorithmIdentifierAsn candidate)
	{
		int num = _signedData.DigestAlgorithms.Length;
		for (int i = 0; i < num; i++)
		{
			if (candidate.Equals(ref _signedData.DigestAlgorithms[i]))
			{
				return;
			}
		}
		Array.Resize(ref _signedData.DigestAlgorithms, num + 1);
		_signedData.DigestAlgorithms[num] = candidate;
	}

	private void ConsiderDigestRemoval(AlgorithmIdentifierAsn candidate)
	{
		bool flag = true;
		for (int i = 0; i < _signedData.SignerInfos.Length; i++)
		{
			if (candidate.Equals(ref _signedData.SignerInfos[i].DigestAlgorithm))
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < _signedData.DigestAlgorithms.Length; j++)
		{
			if (candidate.Equals(ref _signedData.DigestAlgorithms[j]))
			{
				Helpers.RemoveAt(ref _signedData.DigestAlgorithms, j);
				break;
			}
		}
	}

	internal void UpdateCertificatesFromAddition(X509Certificate2Collection newCerts)
	{
		if (newCerts.Count == 0)
		{
			return;
		}
		CertificateChoiceAsn[] certificateSet = _signedData.CertificateSet;
		int num = ((certificateSet != null) ? certificateSet.Length : 0);
		if (num > 0 || newCerts.Count > 1)
		{
			HashSet<X509Certificate2> hashSet = new HashSet<X509Certificate2>(Certificates.OfType<X509Certificate2>());
			for (int i = 0; i < newCerts.Count; i++)
			{
				X509Certificate2 item = newCerts[i];
				if (!hashSet.Add(item))
				{
					newCerts.RemoveAt(i);
					i--;
				}
			}
		}
		if (newCerts.Count != 0)
		{
			if (_signedData.CertificateSet == null)
			{
				_signedData.CertificateSet = new CertificateChoiceAsn[newCerts.Count];
			}
			else
			{
				Array.Resize(ref _signedData.CertificateSet, num + newCerts.Count);
			}
			for (int j = num; j < _signedData.CertificateSet.Length; j++)
			{
				_signedData.CertificateSet[j] = new CertificateChoiceAsn
				{
					Certificate = newCerts[j - num].RawData
				};
			}
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Boolean)" /> method verifies the digital signatures on the signed CMS/PKCS #7 message and, optionally, validates the signers' certificates.</summary>
	/// <param name="verifySignatureOnly">A <see cref="T:System.Boolean" /> value that specifies whether only the digital signatures are verified without the signers' certificates being validated. If <paramref name="verifySignatureOnly" /> is true, only the digital signatures are verified. If it is false, the digital signatures are verified, the signers' certificates are validated, and the purposes of the certificates are validated. The purposes of a certificate are considered valid if the certificate has no key usage or if the key usage supports digital signatures or nonrepudiation.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException"> A method call was invalid for the object's current state.</exception>
	public void CheckSignature(bool verifySignatureOnly)
	{
		CheckSignature(new X509Certificate2Collection(), verifySignatureOnly);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" /> method verifies the digital signatures on the signed CMS/PKCS #7 message by using the specified collection of certificates and, optionally, validates the signers' certificates.</summary>
	/// <param name="extraStore">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> object that can be used to validate the certificate chain. If no additional certificates are to be used to validate the certificate chain, use <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Boolean)" /> instead of <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" />.</param>
	/// <param name="verifySignatureOnly">A <see cref="T:System.Boolean" /> value that specifies whether only the digital signatures are verified without the signers' certificates being validated. If <paramref name="verifySignatureOnly" /> is true, only the digital signatures are verified. If it is false, the digital signatures are verified, the signers' certificates are validated, and the purposes of the certificates are validated. The purposes of a certificate are considered valid if the certificate has no key usage or if the key usage supports digital signatures or nonrepudiation.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	public void CheckSignature(X509Certificate2Collection extraStore, bool verifySignatureOnly)
	{
		if (!_hasData)
		{
			throw new InvalidOperationException("The CMS message is not signed.");
		}
		if (extraStore == null)
		{
			throw new ArgumentNullException("extraStore");
		}
		CheckSignatures(SignerInfos, extraStore, verifySignatureOnly);
	}

	private static void CheckSignatures(SignerInfoCollection signers, X509Certificate2Collection extraStore, bool verifySignatureOnly)
	{
		if (signers.Count < 1)
		{
			throw new CryptographicException("The signed cryptographic message does not have a signer for the specified signer index.");
		}
		SignerInfoEnumerator enumerator = signers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignerInfo current = enumerator.Current;
			current.CheckSignature(extraStore, verifySignatureOnly);
			SignerInfoCollection counterSignerInfos = current.CounterSignerInfos;
			if (counterSignerInfos.Count > 0)
			{
				CheckSignatures(counterSignerInfos, extraStore, verifySignatureOnly);
			}
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckHash" /> method verifies the data integrity of the CMS/PKCS #7 message. <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckHash" /> is a specialized method used in specific security infrastructure applications that only wish to check the hash of the CMS message, rather than perform a full digital signature verification. <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckHash" /> does not authenticate the author nor sender of the message because this method does not involve verifying a digital signature. For general-purpose checking of the integrity and authenticity of a CMS/PKCS #7 message, use the <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Boolean)" /> or <see cref="M:System.Security.Cryptography.Pkcs.SignedCms.CheckSignature(System.Security.Cryptography.X509Certificates.X509Certificate2Collection,System.Boolean)" /> methods.</summary>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	public void CheckHash()
	{
		if (!_hasData)
		{
			throw new InvalidOperationException("The CMS message is not signed.");
		}
		SignerInfoCollection signerInfos = SignerInfos;
		if (signerInfos.Count < 1)
		{
			throw new CryptographicException("The signed cryptographic message does not have a signer for the specified signer index.");
		}
		SignerInfoEnumerator enumerator = signerInfos.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SignerInfo current = enumerator.Current;
			if (current.SignerIdentifier.Type == SubjectIdentifierType.NoSignature)
			{
				current.CheckHash();
			}
		}
	}

	internal ref SignedDataAsn GetRawData()
	{
		return ref _signedData;
	}
}
