using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Internal.Cryptography;
using Microsoft.Win32.SafeHandles;
using Mono;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Represents an X.509 certificate.  </summary>
[Serializable]
public class X509Certificate2 : X509Certificate
{
	private volatile byte[] lazyRawData;

	private volatile Oid lazySignatureAlgorithm;

	private volatile int lazyVersion;

	private volatile X500DistinguishedName lazySubjectName;

	private volatile X500DistinguishedName lazyIssuerName;

	private volatile PublicKey lazyPublicKey;

	private volatile AsymmetricAlgorithm lazyPrivateKey;

	private volatile X509ExtensionCollection lazyExtensions;

	/// <summary>Gets or sets a value indicating that an X.509 certificate is archived.</summary>
	/// <returns>true if the certificate is archived, false if the certificate is not archived.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public bool Archived
	{
		get
		{
			ThrowIfInvalid();
			return Impl.Archived;
		}
		set
		{
			ThrowIfInvalid();
			Impl.Archived = value;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Security.Cryptography.X509Certificates.X509Extension" /> objects.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ExtensionCollection" /> object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public X509ExtensionCollection Extensions
	{
		get
		{
			ThrowIfInvalid();
			X509ExtensionCollection x509ExtensionCollection = lazyExtensions;
			if (x509ExtensionCollection == null)
			{
				x509ExtensionCollection = new X509ExtensionCollection();
				foreach (X509Extension extension in Impl.Extensions)
				{
					X509Extension x509Extension = CreateCustomExtensionIfAny(extension.Oid);
					if (x509Extension == null)
					{
						x509ExtensionCollection.Add(extension);
						continue;
					}
					x509Extension.CopyFrom(extension);
					x509ExtensionCollection.Add(x509Extension);
				}
				lazyExtensions = x509ExtensionCollection;
			}
			return x509ExtensionCollection;
		}
	}

	/// <summary>Gets or sets the associated alias for a certificate.</summary>
	/// <returns>The certificate's friendly name.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public string FriendlyName
	{
		get
		{
			ThrowIfInvalid();
			return Impl.FriendlyName;
		}
		set
		{
			ThrowIfInvalid();
			Impl.FriendlyName = value;
		}
	}

	/// <summary>Gets a value that indicates whether an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object contains a private key. </summary>
	/// <returns>true if the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object contains a private key; otherwise, false. </returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public bool HasPrivateKey
	{
		get
		{
			ThrowIfInvalid();
			return Impl.HasPrivateKey;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object that represents the private key associated with a certificate.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.AsymmetricAlgorithm" /> object, which is either an RSA or DSA cryptographic service provider.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key value is not an RSA or DSA key, or the key is unreadable. </exception>
	/// <exception cref="T:System.ArgumentNullException">The value being set for this property is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The key algorithm for this private key is not supported.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicUnexpectedOperationException">The X.509 keys do not match.</exception>
	/// <exception cref="T:System.ArgumentException">The cryptographic service provider key is null.</exception>
	public AsymmetricAlgorithm PrivateKey
	{
		get
		{
			ThrowIfInvalid();
			if (!HasPrivateKey)
			{
				return null;
			}
			if (lazyPrivateKey == null)
			{
				string keyAlgorithm = GetKeyAlgorithm();
				if (!(keyAlgorithm == "1.2.840.113549.1.1.1"))
				{
					if (!(keyAlgorithm == "1.2.840.10040.4.1"))
					{
						throw new NotSupportedException("The certificate key algorithm is not supported.");
					}
					lazyPrivateKey = Impl.GetDSAPrivateKey();
				}
				else
				{
					lazyPrivateKey = Impl.GetRSAPrivateKey();
				}
			}
			return lazyPrivateKey;
		}
		set
		{
			throw new PlatformNotSupportedException();
		}
	}

	/// <summary>Gets the distinguished name of the certificate issuer.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> object that contains the name of the certificate issuer.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public X500DistinguishedName IssuerName
	{
		get
		{
			ThrowIfInvalid();
			X500DistinguishedName x500DistinguishedName = lazyIssuerName;
			if (x500DistinguishedName == null)
			{
				x500DistinguishedName = (lazyIssuerName = Impl.IssuerName);
			}
			return x500DistinguishedName;
		}
	}

	/// <summary>Gets the date in local time after which a certificate is no longer valid.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object that represents the expiration date for the certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public DateTime NotAfter => GetNotAfter();

	/// <summary>Gets the date in local time on which a certificate becomes valid.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> object that represents the effective date of the certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public DateTime NotBefore => GetNotBefore();

	/// <summary>Gets a <see cref="P:System.Security.Cryptography.X509Certificates.X509Certificate2.PublicKey" /> object associated with a certificate.</summary>
	/// <returns>A <see cref="P:System.Security.Cryptography.X509Certificates.X509Certificate2.PublicKey" /> object.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The key value is not an RSA or DSA key, or the key is unreadable. </exception>
	public PublicKey PublicKey
	{
		get
		{
			ThrowIfInvalid();
			PublicKey publicKey = lazyPublicKey;
			if (publicKey == null)
			{
				string keyAlgorithm = GetKeyAlgorithm();
				byte[] keyAlgorithmParameters = GetKeyAlgorithmParameters();
				byte[] publicKey2 = GetPublicKey();
				Oid oid = new Oid(keyAlgorithm);
				publicKey = (lazyPublicKey = new PublicKey(oid, new AsnEncodedData(oid, keyAlgorithmParameters), new AsnEncodedData(oid, publicKey2)));
			}
			return publicKey;
		}
	}

	/// <summary>Gets the raw data of a certificate.</summary>
	/// <returns>The raw data of the certificate as a byte array.</returns>
	public byte[] RawData
	{
		get
		{
			ThrowIfInvalid();
			byte[] array = lazyRawData;
			if (array == null)
			{
				array = (lazyRawData = Impl.RawData);
			}
			return array.CloneByteArray();
		}
	}

	/// <summary>Gets the serial number of a certificate.</summary>
	/// <returns>The serial number of the certificate.</returns>
	public string SerialNumber => GetSerialNumberString();

	/// <summary>Gets the algorithm used to create the signature of a certificate.</summary>
	/// <returns>Returns the object identifier (<see cref="T:System.Security.Cryptography.Oid" />) of the signature algorithm.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public Oid SignatureAlgorithm
	{
		get
		{
			ThrowIfInvalid();
			Oid oid = lazySignatureAlgorithm;
			if (oid == null)
			{
				string signatureAlgorithm = Impl.SignatureAlgorithm;
				oid = (lazySignatureAlgorithm = Oid.FromOidValue(signatureAlgorithm, OidGroup.SignatureAlgorithm));
			}
			return oid;
		}
	}

	/// <summary>Gets the subject distinguished name from a certificate.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X500DistinguishedName" /> object that represents the name of the certificate subject.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public X500DistinguishedName SubjectName
	{
		get
		{
			ThrowIfInvalid();
			X500DistinguishedName x500DistinguishedName = lazySubjectName;
			if (x500DistinguishedName == null)
			{
				x500DistinguishedName = (lazySubjectName = Impl.SubjectName);
			}
			return x500DistinguishedName;
		}
	}

	/// <summary>Gets the thumbprint of a certificate.</summary>
	/// <returns>The thumbprint of the certificate.</returns>
	public string Thumbprint => GetCertHash().ToHexStringUpper();

	/// <summary>Gets the X.509 format version of a certificate.</summary>
	/// <returns>The certificate format.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public int Version
	{
		get
		{
			ThrowIfInvalid();
			int num = lazyVersion;
			if (num == 0)
			{
				num = (lazyVersion = Impl.Version);
			}
			return num;
		}
	}

	internal new X509Certificate2Impl Impl
	{
		get
		{
			X509Certificate2Impl result = base.Impl as X509Certificate2Impl;
			X509Helper.ThrowIfContextInvalid(result);
			return result;
		}
	}

	/// <summary>Resets the state of an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object.</summary>
	public override void Reset()
	{
		lazyRawData = null;
		lazySignatureAlgorithm = null;
		lazyVersion = 0;
		lazySubjectName = null;
		lazyIssuerName = null;
		lazyPublicKey = null;
		lazyPrivateKey = null;
		lazyExtensions = null;
		base.Reset();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class.</summary>
	public X509Certificate2()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using information from a byte array.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(byte[] rawData)
		: base(rawData)
	{
		if (rawData != null && rawData.Length != 0)
		{
			using (SafePasswordHandle password = new SafePasswordHandle((string)null))
			{
				X509CertificateImpl x509CertificateImpl = X509Helper.Import(rawData, password, X509KeyStorageFlags.DefaultKeySet);
				ImportHandle(x509CertificateImpl);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a byte array and a password.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(byte[] rawData, string password)
		: base(rawData, password)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a byte array and a password.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	[CLSCompliant(false)]
	public X509Certificate2(byte[] rawData, SecureString password)
		: base(rawData, password)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		: base(rawData, password, keyStorageFlags)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	[CLSCompliant(false)]
	public X509Certificate2(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		: base(rawData, password, keyStorageFlags)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using an unmanaged handle.</summary>
	/// <param name="handle">A pointer to a certificate context in unmanaged code. The C structure is called PCCERT_CONTEXT.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(IntPtr handle)
		: base(handle)
	{
	}

	internal X509Certificate2(X509Certificate2Impl impl)
		: base(impl)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName)
		: base(fileName)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name and a password used to access the certificate.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName, string password)
		: base(fileName, password)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name and a password.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName, SecureString password)
		: base(fileName, password)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name, a password used to access the certificate, and a key storage flag.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		: base(fileName, password, keyStorageFlags)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using a certificate file name, a password, and a key storage flag.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		: base(fileName, password, keyStorageFlags)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</summary>
	/// <param name="certificate">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate2(X509Certificate certificate)
		: base(certificate)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class using the specified serialization and stream context information. </summary>
	/// <param name="info">The serialization information required to deserialize the new <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" />.</param>
	/// <param name="context">Contextual information about the source of the stream to be deserialized.</param>
	protected X509Certificate2(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		throw new PlatformNotSupportedException();
	}

	/// <summary>Indicates the type of certificate contained in a byte array.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> object.</returns>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rawData" /> has a zero length or is null. </exception>
	public static X509ContentType GetCertContentType(byte[] rawData)
	{
		if (rawData == null || rawData.Length == 0)
		{
			throw new ArgumentException("Array cannot be empty or null.", "rawData");
		}
		return X509Pal.Instance.GetCertContentType(rawData);
	}

	/// <summary>Indicates the type of certificate contained in a file.</summary>
	/// <returns>An <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> object.</returns>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null.</exception>
	public static X509ContentType GetCertContentType(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		Path.GetFullPath(fileName);
		return X509Pal.Instance.GetCertContentType(fileName);
	}

	/// <summary>Gets the subject and issuer names from a certificate.</summary>
	/// <returns>The name of the certificate.</returns>
	/// <param name="nameType">The <see cref="T:System.Security.Cryptography.X509Certificates.X509NameType" /> value for the subject. </param>
	/// <param name="forIssuer">true to include the issuer name; otherwise, false. </param>
	public string GetNameInfo(X509NameType nameType, bool forIssuer)
	{
		return Impl.GetNameInfo(nameType, forIssuer);
	}

	/// <summary>Displays an X.509 certificate in text format.</summary>
	/// <returns>The certificate information.</returns>
	public override string ToString()
	{
		return base.ToString(fVerbose: true);
	}

	/// <summary>Displays an X.509 certificate in text format.</summary>
	/// <returns>The certificate information.</returns>
	/// <param name="verbose">true to display the public key, private key, extensions, and so forth; false to display information that is similar to the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> class, including thumbprint, serial number, subject and issuer names, and so on. </param>
	public override string ToString(bool verbose)
	{
		if (!verbose || !base.IsValid)
		{
			return ToString();
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[Version]");
		stringBuilder.Append("  V");
		stringBuilder.Append(Version);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Subject]");
		stringBuilder.Append("  ");
		stringBuilder.Append(SubjectName.Name);
		string nameInfo = GetNameInfo(X509NameType.SimpleName, forIssuer: false);
		if (nameInfo.Length > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("Simple Name: ");
			stringBuilder.Append(nameInfo);
		}
		string nameInfo2 = GetNameInfo(X509NameType.EmailName, forIssuer: false);
		if (nameInfo2.Length > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("Email Name: ");
			stringBuilder.Append(nameInfo2);
		}
		string nameInfo3 = GetNameInfo(X509NameType.UpnName, forIssuer: false);
		if (nameInfo3.Length > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("UPN Name: ");
			stringBuilder.Append(nameInfo3);
		}
		string nameInfo4 = GetNameInfo(X509NameType.DnsName, forIssuer: false);
		if (nameInfo4.Length > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("DNS Name: ");
			stringBuilder.Append(nameInfo4);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Issuer]");
		stringBuilder.Append("  ");
		stringBuilder.Append(IssuerName.Name);
		nameInfo = GetNameInfo(X509NameType.SimpleName, forIssuer: true);
		if (nameInfo.Length > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("Simple Name: ");
			stringBuilder.Append(nameInfo);
		}
		nameInfo2 = GetNameInfo(X509NameType.EmailName, forIssuer: true);
		if (nameInfo2.Length > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("Email Name: ");
			stringBuilder.Append(nameInfo2);
		}
		nameInfo3 = GetNameInfo(X509NameType.UpnName, forIssuer: true);
		if (nameInfo3.Length > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("UPN Name: ");
			stringBuilder.Append(nameInfo3);
		}
		nameInfo4 = GetNameInfo(X509NameType.DnsName, forIssuer: true);
		if (nameInfo4.Length > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("DNS Name: ");
			stringBuilder.Append(nameInfo4);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Serial Number]");
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(SerialNumber);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Not Before]");
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(X509Certificate.FormatDate(NotBefore));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Not After]");
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(X509Certificate.FormatDate(NotAfter));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Thumbprint]");
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(Thumbprint);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Signature Algorithm]");
		stringBuilder.Append("  ");
		stringBuilder.Append(SignatureAlgorithm.FriendlyName);
		stringBuilder.Append('(');
		stringBuilder.Append(SignatureAlgorithm.Value);
		stringBuilder.AppendLine(")");
		stringBuilder.AppendLine();
		stringBuilder.Append("[Public Key]");
		try
		{
			PublicKey publicKey = PublicKey;
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("Algorithm: ");
			stringBuilder.Append(publicKey.Oid.FriendlyName);
			try
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("  ");
				stringBuilder.Append("Length: ");
				using RSA rSA = this.GetRSAPublicKey();
				if (rSA != null)
				{
					stringBuilder.Append(rSA.KeySize);
				}
			}
			catch (NotSupportedException)
			{
			}
			stringBuilder.AppendLine();
			stringBuilder.Append("  ");
			stringBuilder.Append("Key Blob: ");
			stringBuilder.AppendLine(publicKey.EncodedKeyValue.Format(multiLine: true));
			stringBuilder.Append("  ");
			stringBuilder.Append("Parameters: ");
			stringBuilder.Append(publicKey.EncodedParameters.Format(multiLine: true));
		}
		catch (CryptographicException)
		{
		}
		Impl.AppendPrivateKeyInfo(stringBuilder);
		X509ExtensionCollection extensions = Extensions;
		if (extensions.Count > 0)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append("[Extensions]");
			X509ExtensionEnumerator enumerator = extensions.GetEnumerator();
			while (enumerator.MoveNext())
			{
				X509Extension current = enumerator.Current;
				try
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("* ");
					stringBuilder.Append(current.Oid.FriendlyName);
					stringBuilder.Append('(');
					stringBuilder.Append(current.Oid.Value);
					stringBuilder.Append("):");
					stringBuilder.AppendLine();
					stringBuilder.Append("  ");
					stringBuilder.Append(current.Format(multiLine: true));
				}
				catch (CryptographicException)
				{
				}
			}
		}
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object with data from a byte array.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	public override void Import(byte[] rawData)
	{
		base.Import(rawData);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object using data from a byte array, a password, and flags for determining how to import the private key.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	public override void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
	{
		base.Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object using data from a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	[CLSCompliant(false)]
	public override void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		base.Import(rawData, password, keyStorageFlags);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object with information from a certificate file.</summary>
	/// <param name="fileName">The name of a certificate. </param>
	public override void Import(string fileName)
	{
		base.Import(fileName);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object with information from a certificate file, a password, and a <see cref="T:System.Security.Cryptography.X509Certificates.X509KeyStorageFlags" /> value.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	public override void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
	{
		base.Import(fileName, password, keyStorageFlags);
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object with information from a certificate file, a password, and a key storage flag.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	[CLSCompliant(false)]
	public override void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		base.Import(fileName, password, keyStorageFlags);
	}

	/// <summary>Performs a X.509 chain validation using basic validation policy.</summary>
	/// <returns>true if the validation succeeds; false if the validation fails.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate is unreadable. </exception>
	public bool Verify()
	{
		return Impl.Verify(this);
	}

	private static X509Extension CreateCustomExtensionIfAny(Oid oid)
	{
		switch (oid.Value)
		{
		case "2.5.29.10":
			if (!X509Pal.Instance.SupportsLegacyBasicConstraintsExtension)
			{
				return null;
			}
			return new X509BasicConstraintsExtension();
		case "2.5.29.19":
			return new X509BasicConstraintsExtension();
		case "2.5.29.15":
			return new X509KeyUsageExtension();
		case "2.5.29.37":
			return new X509EnhancedKeyUsageExtension();
		case "2.5.29.14":
			return new X509SubjectKeyIdentifierExtension();
		default:
			return null;
		}
	}
}
