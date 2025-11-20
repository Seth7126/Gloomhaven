using System.Security.Cryptography.X509Certificates;
using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class represents a CMS/PKCS #7 structure for enveloped data.</summary>
public sealed class EnvelopedCms
{
	private enum LastCall
	{
		Ctor = 1,
		Encrypt,
		Decode,
		Decrypt
	}

	private DecryptorPal _decryptorPal;

	private byte[] _encodedMessage;

	private LastCall _lastCall;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.Version" /> property retrieves the version of the enveloped CMS/PKCS #7 message.  </summary>
	/// <returns>An int value that represents the version of the enveloped CMS/PKCS #7 message.</returns>
	public int Version { get; private set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.ContentInfo" /> property retrieves the inner content information for the enveloped CMS/PKCS #7 message.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the inner content information from the enveloped CMS/PKCS #7 message.</returns>
	public ContentInfo ContentInfo { get; private set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.ContentEncryptionAlgorithm" /> property retrieves the identifier of the algorithm used to encrypt the content.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> object that represents the algorithm identifier.</returns>
	public AlgorithmIdentifier ContentEncryptionAlgorithm { get; private set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.Certificates" /> property retrieves the set of certificates associated with the enveloped CMS/PKCS #7 message.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents the X.509 certificates used with the enveloped CMS/PKCS #7 message. If no certificates exist, the property value is an empty collection.</returns>
	public X509Certificate2Collection Certificates { get; private set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.UnprotectedAttributes" /> property retrieves the unprotected (unencrypted) attributes associated with the enveloped CMS/PKCS #7 message. Unprotected attributes are not encrypted, and so do not have data confidentiality within an <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> object.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection that represents the unprotected attributes. If no unprotected attributes exist, the property value is an empty collection.</returns>
	public CryptographicAttributeObjectCollection UnprotectedAttributes { get; private set; }

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.RecipientInfos" /> property retrieves the recipient information associated with the enveloped CMS/PKCS #7 message.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfoCollection" /> collection that represents the recipient information. If no recipients exist, the property value is an empty collection.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RecipientInfoCollection RecipientInfos
	{
		get
		{
			switch (_lastCall)
			{
			case LastCall.Ctor:
				return new RecipientInfoCollection();
			case LastCall.Encrypt:
				throw PkcsPal.Instance.CreateRecipientInfosAfterEncryptException();
			case LastCall.Decode:
			case LastCall.Decrypt:
				return _decryptorPal.RecipientInfos;
			default:
				throw new InvalidOperationException();
			}
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class.</summary>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public EnvelopedCms()
		: this(new ContentInfo(Array.Empty<byte>()))
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor(System.Security.Cryptography.Pkcs.ContentInfo)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class by using the specified content information as the inner content type.</summary>
	/// <param name="contentInfo">An instance of the <see cref="P:System.Security.Cryptography.Pkcs.EnvelopedCms.ContentInfo" /> class that represents the content and its type.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public EnvelopedCms(ContentInfo contentInfo)
		: this(contentInfo, new AlgorithmIdentifier(Oid.FromOidValue("1.2.840.113549.3.7", OidGroup.EncryptionAlgorithm)))
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor(System.Security.Cryptography.Pkcs.ContentInfo,System.Security.Cryptography.Pkcs.AlgorithmIdentifier)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class by using the specified content information and encryption algorithm. The specified content information is to be used as the inner content type.</summary>
	/// <param name="contentInfo">A  <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the content and its type.</param>
	/// <param name="encryptionAlgorithm">An <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> object that specifies the encryption algorithm.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public EnvelopedCms(ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm)
	{
		if (contentInfo == null)
		{
			throw new ArgumentNullException("contentInfo");
		}
		if (encryptionAlgorithm == null)
		{
			throw new ArgumentNullException("encryptionAlgorithm");
		}
		Version = 0;
		ContentInfo = contentInfo;
		ContentEncryptionAlgorithm = encryptionAlgorithm;
		Certificates = new X509Certificate2Collection();
		UnprotectedAttributes = new CryptographicAttributeObjectCollection();
		_decryptorPal = null;
		_lastCall = LastCall.Ctor;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Encrypt(System.Security.Cryptography.Pkcs.CmsRecipient)" /> method encrypts the contents of the CMS/PKCS #7 message by using the specified recipient information.</summary>
	/// <param name="recipient">A <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipient" /> object that represents the recipient information.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public void Encrypt(CmsRecipient recipient)
	{
		if (recipient == null)
		{
			throw new ArgumentNullException("recipient");
		}
		Encrypt(new CmsRecipientCollection(recipient));
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Encrypt(System.Security.Cryptography.Pkcs.CmsRecipientCollection)" /> method encrypts the contents of the CMS/PKCS #7 message by using the information for the specified list of recipients. The message is encrypted by using a message encryption key with a symmetric encryption algorithm such as triple DES. The message encryption key is then encrypted with the public key of each recipient.</summary>
	/// <param name="recipients">A <see cref="T:System.Security.Cryptography.Pkcs.CmsRecipientCollection" /> collection that represents the information for the list of recipients.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	public void Encrypt(CmsRecipientCollection recipients)
	{
		if (recipients == null)
		{
			throw new ArgumentNullException("recipients");
		}
		if (recipients.Count == 0)
		{
			throw new PlatformNotSupportedException("The recipients collection is empty. You must specify at least one recipient. This platform does not implement the certificate picker UI.");
		}
		if (_decryptorPal != null)
		{
			_decryptorPal.Dispose();
			_decryptorPal = null;
		}
		_encodedMessage = PkcsPal.Instance.Encrypt(recipients, ContentInfo, ContentEncryptionAlgorithm, Certificates, UnprotectedAttributes);
		_lastCall = LastCall.Encrypt;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Encode" /> method encodes the contents of the enveloped CMS/PKCS #7 message and returns it as an array of byte values. Encryption must be done before encoding.</summary>
	/// <returns>If the method succeeds, the method returns an array of byte values that represent the encoded information.If the method fails, it throws an exception.</returns>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public byte[] Encode()
	{
		if (_encodedMessage == null)
		{
			throw new InvalidOperationException("The CMS message is not encrypted.");
		}
		return _encodedMessage.CloneByteArray();
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decode(System.Byte[])" /> method decodes the specified enveloped CMS/PKCS #7 message and resets all member variables in the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> object.</summary>
	/// <param name="encodedMessage">An array of byte values that represent the information to be decoded.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	public void Decode(byte[] encodedMessage)
	{
		if (encodedMessage == null)
		{
			throw new ArgumentNullException("encodedMessage");
		}
		if (_decryptorPal != null)
		{
			_decryptorPal.Dispose();
			_decryptorPal = null;
		}
		_decryptorPal = PkcsPal.Instance.Decode(encodedMessage, out var version, out var contentInfo, out var contentEncryptionAlgorithm, out var originatorCerts, out var unprotectedAttributes);
		Version = version;
		ContentInfo = contentInfo;
		ContentEncryptionAlgorithm = contentEncryptionAlgorithm;
		Certificates = originatorCerts;
		UnprotectedAttributes = unprotectedAttributes;
		_encodedMessage = contentInfo.Content.CloneByteArray();
		_lastCall = LastCall.Decode;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt" /> method decrypts the contents of the decoded enveloped CMS/PKCS #7 message. The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt" /> method searches the current user and computer My stores for the appropriate certificate and private key.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.StorePermission, System.Security, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Flags="CreateStore, DeleteStore, OpenStore, EnumerateCertificates" />
	/// </PermissionSet>
	public void Decrypt()
	{
		DecryptContent(RecipientInfos, null);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.Pkcs.RecipientInfo)" /> method decrypts the contents of the decoded enveloped CMS/PKCS #7 message by using the private key associated with the certificate identified by the specified recipient information.</summary>
	/// <param name="recipientInfo">A <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object that represents the recipient information that identifies the certificate associated with the private key to use for the decryption.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	public void Decrypt(RecipientInfo recipientInfo)
	{
		if (recipientInfo == null)
		{
			throw new ArgumentNullException("recipientInfo");
		}
		DecryptContent(new RecipientInfoCollection(recipientInfo), null);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.Pkcs.RecipientInfo,System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method decrypts the contents of the decoded enveloped CMS/PKCS #7 message by using the private key associated with the certificate identified by the specified recipient information and by using the specified certificate collection.  The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.Pkcs.RecipientInfo,System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method searches the specified certificate collection and the My certificate store for the proper certificate to use for the decryption.</summary>
	/// <param name="recipientInfo">A <see cref="T:System.Security.Cryptography.Pkcs.RecipientInfo" /> object that represents the recipient information to use for the decryption.</param>
	/// <param name="extraStore">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents additional certificates to use for the decryption. The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.Pkcs.RecipientInfo,System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method searches this certificate collection and the My certificate store for the proper certificate to use for the decryption.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	public void Decrypt(RecipientInfo recipientInfo, X509Certificate2Collection extraStore)
	{
		if (recipientInfo == null)
		{
			throw new ArgumentNullException("recipientInfo");
		}
		if (extraStore == null)
		{
			throw new ArgumentNullException("extraStore");
		}
		DecryptContent(new RecipientInfoCollection(recipientInfo), extraStore);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method decrypts the contents of the decoded enveloped CMS/PKCS #7 message by using the specified certificate collection. The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method searches the specified certificate collection and the My certificate store for the proper certificate to use for the decryption.</summary>
	/// <param name="extraStore">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2Collection" /> collection that represents additional certificates to use for the decryption. The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Decrypt(System.Security.Cryptography.X509Certificates.X509Certificate2Collection)" /> method searches this certificate collection and the My certificate store for the proper certificate to use for the decryption.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <exception cref="T:System.InvalidOperationException">A method call was invalid for the object's current state.</exception>
	public void Decrypt(X509Certificate2Collection extraStore)
	{
		if (extraStore == null)
		{
			throw new ArgumentNullException("extraStore");
		}
		DecryptContent(RecipientInfos, extraStore);
	}

	private void DecryptContent(RecipientInfoCollection recipientInfos, X509Certificate2Collection extraStore)
	{
		switch (_lastCall)
		{
		case LastCall.Ctor:
			throw new InvalidOperationException("The CMS message is not encrypted.");
		case LastCall.Encrypt:
			throw PkcsPal.Instance.CreateDecryptAfterEncryptException();
		case LastCall.Decrypt:
			throw PkcsPal.Instance.CreateDecryptTwiceException();
		default:
			throw new InvalidOperationException();
		case LastCall.Decode:
		{
			extraStore = extraStore ?? new X509Certificate2Collection();
			X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
			PkcsPal.Instance.AddCertsFromStoreForDecryption(x509Certificate2Collection);
			x509Certificate2Collection.AddRange(extraStore);
			X509Certificate2Collection certificates = Certificates;
			ContentInfo contentInfo = null;
			Exception exception = PkcsPal.Instance.CreateRecipientsNotFoundException();
			RecipientInfoEnumerator enumerator = recipientInfos.GetEnumerator();
			while (enumerator.MoveNext())
			{
				RecipientInfo current = enumerator.Current;
				X509Certificate2 x509Certificate = x509Certificate2Collection.TryFindMatchingCertificate(current.RecipientIdentifier);
				if (x509Certificate == null)
				{
					exception = PkcsPal.Instance.CreateRecipientsNotFoundException();
					continue;
				}
				contentInfo = _decryptorPal.TryDecrypt(current, x509Certificate, certificates, extraStore, out exception);
				if (exception == null)
				{
					break;
				}
			}
			if (exception != null)
			{
				throw exception;
			}
			ContentInfo = contentInfo;
			_encodedMessage = contentInfo.Content.CloneByteArray();
			_lastCall = LastCall.Decrypt;
			break;
		}
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.Pkcs.ContentInfo)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class by using the specified subject identifier type and content information. The specified content information is to be used as the inner content type.</summary>
	/// <param name="recipientIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the means of identifying the recipient.</param>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the content and its type.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument.</exception>
	public EnvelopedCms(SubjectIdentifierType recipientIdentifierType, ContentInfo contentInfo)
		: this(contentInfo)
	{
		if (recipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier)
		{
			Version = 2;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.#ctor(System.Security.Cryptography.Pkcs.SubjectIdentifierType,System.Security.Cryptography.Pkcs.ContentInfo,System.Security.Cryptography.Pkcs.AlgorithmIdentifier)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.EnvelopedCms" /> class by using the specified subject identifier type, content information, and encryption algorithm. The specified content information is to be used as the inner content type.</summary>
	/// <param name="recipientIdentifierType">A member of the <see cref="T:System.Security.Cryptography.Pkcs.SubjectIdentifierType" /> enumeration that specifies the means of identifying the recipient.</param>
	/// <param name="contentInfo">A <see cref="T:System.Security.Cryptography.Pkcs.ContentInfo" /> object that represents the content and its type.</param>
	/// <param name="encryptionAlgorithm">An <see cref="T:System.Security.Cryptography.Pkcs.AlgorithmIdentifier" /> object that specifies the encryption algorithm.</param>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	public EnvelopedCms(SubjectIdentifierType recipientIdentifierType, ContentInfo contentInfo, AlgorithmIdentifier encryptionAlgorithm)
		: this(contentInfo, encryptionAlgorithm)
	{
		if (recipientIdentifierType == SubjectIdentifierType.SubjectKeyIdentifier)
		{
			Version = 2;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.EnvelopedCms.Encrypt" /> method encrypts the contents of the CMS/PKCS #7 message.</summary>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A cryptographic operation could not be completed.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeTopLevelWindows" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.StorePermission, System.Security, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Flags="CreateStore, DeleteStore, OpenStore, EnumerateCertificates" />
	/// </PermissionSet>
	public void Encrypt()
	{
		Encrypt(new CmsRecipientCollection());
	}
}
