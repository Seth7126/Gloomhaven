using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using Internal.Cryptography;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography.X509Certificates;

/// <summary>Provides methods that help you use X.509 v.3 certificates.</summary>
[Serializable]
public class X509Certificate : IDisposable, IDeserializationCallback, ISerializable
{
	private X509CertificateImpl impl;

	private volatile byte[] lazyCertHash;

	private volatile byte[] lazySerialNumber;

	private volatile string lazyIssuer;

	private volatile string lazySubject;

	private volatile string lazyKeyAlgorithm;

	private volatile byte[] lazyKeyAlgorithmParameters;

	private volatile byte[] lazyPublicKey;

	private DateTime lazyNotBefore = DateTime.MinValue;

	private DateTime lazyNotAfter = DateTime.MinValue;

	internal const X509KeyStorageFlags KeyStorageFlagsAll = X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.UserProtected | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.EphemeralKeySet;

	/// <summary>Gets a handle to a Microsoft Cryptographic API certificate context described by an unmanaged PCCERT_CONTEXT structure. </summary>
	/// <returns>An <see cref="T:System.IntPtr" /> structure that represents an unmanaged PCCERT_CONTEXT structure.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public IntPtr Handle
	{
		get
		{
			if (X509Helper.IsValid(impl))
			{
				return impl.Handle;
			}
			return IntPtr.Zero;
		}
	}

	/// <summary>Gets the name of the certificate authority that issued the X.509v3 certificate.</summary>
	/// <returns>The name of the certificate authority that issued the X.509v3 certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate handle is invalid.</exception>
	public string Issuer
	{
		get
		{
			ThrowIfInvalid();
			string text = lazyIssuer;
			if (text == null)
			{
				text = (lazyIssuer = Impl.Issuer);
			}
			return text;
		}
	}

	/// <summary>Gets the subject distinguished name from the certificate.</summary>
	/// <returns>The subject distinguished name from the certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate handle is invalid.</exception>
	public string Subject
	{
		get
		{
			ThrowIfInvalid();
			string text = lazySubject;
			if (text == null)
			{
				text = (lazySubject = Impl.Subject);
			}
			return text;
		}
	}

	internal X509CertificateImpl Impl => impl;

	internal bool IsValid => X509Helper.IsValid(impl);

	/// <summary>Resets the state of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2" /> object.</summary>
	public virtual void Reset()
	{
		if (impl != null)
		{
			impl.Dispose();
			impl = null;
		}
		lazyCertHash = null;
		lazyIssuer = null;
		lazySubject = null;
		lazySerialNumber = null;
		lazyKeyAlgorithm = null;
		lazyKeyAlgorithmParameters = null;
		lazyPublicKey = null;
		lazyNotBefore = DateTime.MinValue;
		lazyNotAfter = DateTime.MinValue;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class. </summary>
	public X509Certificate()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class defined from a sequence of bytes representing an X.509v3 certificate.</summary>
	/// <param name="data">A byte array containing data from an X.509 certificate.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	public X509Certificate(byte[] data)
	{
		if (data != null && data.Length != 0)
		{
			impl = X509Helper.Import(data);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a byte array and a password.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate.</param>
	/// <param name="password">The password required to access the X.509 certificate data.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	public X509Certificate(byte[] rawData, string password)
		: this(rawData, password, X509KeyStorageFlags.DefaultKeySet)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a byte array and a password.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate.</param>
	/// <param name="password">The password required to access the X.509 certificate data.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	[CLSCompliant(false)]
	public X509Certificate(byte[] rawData, SecureString password)
		: this(rawData, password, X509KeyStorageFlags.DefaultKeySet)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	public X509Certificate(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
	{
		if (rawData == null || rawData.Length == 0)
		{
			throw new ArgumentException("Array cannot be empty or null.", "rawData");
		}
		ValidateKeyStorageFlags(keyStorageFlags);
		using SafePasswordHandle password2 = new SafePasswordHandle(password);
		impl = X509Helper.Import(rawData, password2, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	[CLSCompliant(false)]
	public X509Certificate(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		if (rawData == null || rawData.Length == 0)
		{
			throw new ArgumentException("Array cannot be empty or null.", "rawData");
		}
		ValidateKeyStorageFlags(keyStorageFlags);
		using SafePasswordHandle password2 = new SafePasswordHandle(password);
		impl = X509Helper.Import(rawData, password2, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a handle to an unmanaged PCCERT_CONTEXT structure.</summary>
	/// <param name="handle">A handle to an unmanaged PCCERT_CONTEXT structure.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The handle parameter does not represent a valid PCCERT_CONTEXT structure.</exception>
	public X509Certificate(IntPtr handle)
	{
		throw new PlatformNotSupportedException("Initializing `X509Certificate` from native handle is not supported.");
	}

	internal X509Certificate(X509CertificateImpl impl)
	{
		this.impl = X509Helper.InitFromCertificate(impl);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using the name of a PKCS7 signed file. </summary>
	/// <param name="fileName">The name of a PKCS7 signed file.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	public X509Certificate(string fileName)
		: this(fileName, (string)null, X509KeyStorageFlags.DefaultKeySet)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using the name of a PKCS7 signed file and a password to access the certificate.</summary>
	/// <param name="fileName">The name of a PKCS7 signed file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	public X509Certificate(string fileName, string password)
		: this(fileName, password, X509KeyStorageFlags.DefaultKeySet)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a certificate file name and a password.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public X509Certificate(string fileName, SecureString password)
		: this(fileName, password, X509KeyStorageFlags.DefaultKeySet)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using the name of a PKCS7 signed file, a password to access the certificate, and a key storage flag. </summary>
	/// <param name="fileName">The name of a PKCS7 signed file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	public X509Certificate(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		ValidateKeyStorageFlags(keyStorageFlags);
		byte[] rawData = File.ReadAllBytes(fileName);
		using SafePasswordHandle password2 = new SafePasswordHandle(password);
		impl = X509Helper.Import(rawData, password2, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a certificate file name, a password, and a key storage flag. </summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public X509Certificate(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		: this()
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		ValidateKeyStorageFlags(keyStorageFlags);
		byte[] rawData = File.ReadAllBytes(fileName);
		using SafePasswordHandle password2 = new SafePasswordHandle(password);
		impl = X509Helper.Import(rawData, password2, keyStorageFlags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using another <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class.</summary>
	/// <param name="cert">A <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class from which to initialize this class. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	/// <exception cref="T:System.ArgumentNullException">The value of the <paramref name="cert" /> parameter is null.</exception>
	public X509Certificate(X509Certificate cert)
	{
		if (cert == null)
		{
			throw new ArgumentNullException("cert");
		}
		impl = X509Helper.InitFromCertificate(cert);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> class using a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object and a <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that describes serialization information.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that describes how serialization should be performed.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	public X509Certificate(SerializationInfo info, StreamingContext context)
		: this()
	{
		throw new PlatformNotSupportedException();
	}

	/// <summary>Creates an X.509v3 certificate from the specified PKCS7 signed file.</summary>
	/// <returns>The newly created X.509 certificate.</returns>
	/// <param name="filename">The path of the PKCS7 signed file from which to create the X.509 certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="filename" /> parameter is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public static X509Certificate CreateFromCertFile(string filename)
	{
		return new X509Certificate(filename);
	}

	/// <summary>Creates an X.509v3 certificate from the specified signed file.</summary>
	/// <returns>The newly created X.509 certificate.</returns>
	/// <param name="filename">The path of the signed file from which to create the X.509 certificate. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	public static X509Certificate CreateFromSignedFile(string filename)
	{
		return new X509Certificate(filename);
	}

	/// <summary>Gets serialization information with all the data needed to recreate an instance of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</summary>
	/// <param name="info">The object to populate with serialization information.</param>
	/// <param name="context">The destination context of the serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new PlatformNotSupportedException();
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and is called back by the deserialization event when deserialization is complete.  </summary>
	/// <param name="sender">The source of the deserialization event.</param>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
		throw new PlatformNotSupportedException();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Reset();
		}
	}

	/// <summary>Compares two <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> objects for equality.</summary>
	/// <returns>true if the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object is equal to the object specified by the <paramref name="other" /> parameter; otherwise, false.</returns>
	/// <param name="obj">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to compare to the current object. </param>
	public override bool Equals(object obj)
	{
		if (!(obj is X509Certificate other))
		{
			return false;
		}
		return Equals(other);
	}

	/// <summary>Compares two <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> objects for equality.</summary>
	/// <returns>true if the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object is equal to the object specified by the <paramref name="other" /> parameter; otherwise, false.</returns>
	/// <param name="other">An <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to compare to the current object.</param>
	public virtual bool Equals(X509Certificate other)
	{
		if (other == null)
		{
			return false;
		}
		if (Impl == null)
		{
			return other.Impl == null;
		}
		if (!Issuer.Equals(other.Issuer))
		{
			return false;
		}
		byte[] rawSerialNumber = GetRawSerialNumber();
		byte[] rawSerialNumber2 = other.GetRawSerialNumber();
		if (rawSerialNumber.Length != rawSerialNumber2.Length)
		{
			return false;
		}
		for (int i = 0; i < rawSerialNumber.Length; i++)
		{
			if (rawSerialNumber[i] != rawSerialNumber2[i])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Exports the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to a byte array in a format described by one of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values. </summary>
	/// <returns>An array of bytes that represents the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	/// <param name="contentType">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values that describes how to format the output data. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A value other than <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Cert" />, <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.SerializedCert" />, or <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Pkcs12" /> was passed to the <paramref name="contentType" /> parameter.-or-The certificate could not be exported.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Open, Export" />
	/// </PermissionSet>
	public virtual byte[] Export(X509ContentType contentType)
	{
		return Export(contentType, (string)null);
	}

	/// <summary>Exports the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to a byte array in a format described by one of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values, and using the specified password.</summary>
	/// <returns>An array of bytes that represents the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	/// <param name="contentType">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values that describes how to format the output data.</param>
	/// <param name="password">The password required to access the X.509 certificate data.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A value other than <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Cert" />, <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.SerializedCert" />, or <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Pkcs12" /> was passed to the <paramref name="contentType" /> parameter.-or-The certificate could not be exported.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Open, Export" />
	/// </PermissionSet>
	public virtual byte[] Export(X509ContentType contentType, string password)
	{
		VerifyContentType(contentType);
		if (Impl == null)
		{
			throw new CryptographicException(-2147467261);
		}
		using SafePasswordHandle password2 = new SafePasswordHandle(password);
		return Impl.Export(contentType, password2);
	}

	/// <summary>Exports the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object to a byte array using the specified format and a password.</summary>
	/// <returns>A byte array that represents the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	/// <param name="contentType">One of the <see cref="T:System.Security.Cryptography.X509Certificates.X509ContentType" /> values that describes how to format the output data.</param>
	/// <param name="password">The password required to access the X.509 certificate data.</param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">A value other than <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Cert" />, <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.SerializedCert" />, or <see cref="F:System.Security.Cryptography.X509Certificates.X509ContentType.Pkcs12" /> was passed to the <paramref name="contentType" /> parameter.-or-The certificate could not be exported.</exception>
	[CLSCompliant(false)]
	public virtual byte[] Export(X509ContentType contentType, SecureString password)
	{
		VerifyContentType(contentType);
		if (Impl == null)
		{
			throw new CryptographicException(-2147467261);
		}
		using SafePasswordHandle password2 = new SafePasswordHandle(password);
		return Impl.Export(contentType, password2);
	}

	/// <summary>Returns the raw data for the entire X.509v3 certificate.</summary>
	/// <returns>The X.509 certificate data as a hexadecimal string.</returns>
	public virtual string GetRawCertDataString()
	{
		ThrowIfInvalid();
		return GetRawCertData().ToHexStringUpper();
	}

	/// <summary>Returns the hash value for the X.509v3 certificate as an array of bytes.</summary>
	/// <returns>The hash value for the X.509 certificate.</returns>
	public virtual byte[] GetCertHash()
	{
		ThrowIfInvalid();
		return GetRawCertHash().CloneByteArray();
	}

	public virtual byte[] GetCertHash(HashAlgorithmName hashAlgorithm)
	{
		throw new PlatformNotSupportedException();
	}

	public virtual bool TryGetCertHash(HashAlgorithmName hashAlgorithm, Span<byte> destination, out int bytesWritten)
	{
		throw new PlatformNotSupportedException();
	}

	/// <summary>Returns the SHA1 hash value for the X.509v3 certificate as a hexadecimal string.</summary>
	/// <returns>The hexadecimal string representation of the X.509 certificate hash value.</returns>
	public virtual string GetCertHashString()
	{
		ThrowIfInvalid();
		return GetRawCertHash().ToHexStringUpper();
	}

	public virtual string GetCertHashString(HashAlgorithmName hashAlgorithm)
	{
		ThrowIfInvalid();
		return GetCertHash(hashAlgorithm).ToHexStringUpper();
	}

	private byte[] GetRawCertHash()
	{
		return lazyCertHash ?? (lazyCertHash = Impl.Thumbprint);
	}

	/// <summary>Returns the effective date of this X.509v3 certificate.</summary>
	/// <returns>The effective date for this X.509 certificate.</returns>
	public virtual string GetEffectiveDateString()
	{
		return GetNotBefore().ToString();
	}

	/// <summary>Returns the expiration date of this X.509v3 certificate.</summary>
	/// <returns>The expiration date for this X.509 certificate.</returns>
	public virtual string GetExpirationDateString()
	{
		return GetNotAfter().ToString();
	}

	/// <summary>Returns the name of the format of this X.509v3 certificate.</summary>
	/// <returns>The format of this X.509 certificate.</returns>
	public virtual string GetFormat()
	{
		return "X509";
	}

	/// <summary>Returns the public key for the X.509v3 certificate.</summary>
	/// <returns>The public key for the X.509 certificate as a hexadecimal string.</returns>
	public virtual string GetPublicKeyString()
	{
		return GetPublicKey().ToHexStringUpper();
	}

	/// <summary>Returns the raw data for the entire X.509v3 certificate.</summary>
	/// <returns>A byte array containing the X.509 certificate data.</returns>
	public virtual byte[] GetRawCertData()
	{
		ThrowIfInvalid();
		return Impl.RawData.CloneByteArray();
	}

	/// <summary>Returns the hash code for the X.509v3 certificate as an integer.</summary>
	/// <returns>The hash code for the X.509 certificate as an integer.</returns>
	public override int GetHashCode()
	{
		if (Impl == null)
		{
			return 0;
		}
		byte[] rawCertHash = GetRawCertHash();
		int num = 0;
		for (int i = 0; i < rawCertHash.Length && i < 4; i++)
		{
			num = (num << 8) | rawCertHash[i];
		}
		return num;
	}

	/// <summary>Returns the key algorithm information for this X.509v3 certificate.</summary>
	/// <returns>The key algorithm information for this X.509 certificate as a string.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual string GetKeyAlgorithm()
	{
		ThrowIfInvalid();
		string text = lazyKeyAlgorithm;
		if (text == null)
		{
			text = (lazyKeyAlgorithm = Impl.KeyAlgorithm);
		}
		return text;
	}

	/// <summary>Returns the key algorithm parameters for the X.509v3 certificate.</summary>
	/// <returns>The key algorithm parameters for the X.509 certificate as an array of bytes.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual byte[] GetKeyAlgorithmParameters()
	{
		ThrowIfInvalid();
		byte[] array = lazyKeyAlgorithmParameters;
		if (array == null)
		{
			array = (lazyKeyAlgorithmParameters = Impl.KeyAlgorithmParameters);
		}
		return array.CloneByteArray();
	}

	/// <summary>Returns the key algorithm parameters for the X.509v3 certificate.</summary>
	/// <returns>The key algorithm parameters for the X.509 certificate as a hexadecimal string.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual string GetKeyAlgorithmParametersString()
	{
		ThrowIfInvalid();
		return GetKeyAlgorithmParameters().ToHexStringUpper();
	}

	/// <summary>Returns the public key for the X.509v3 certificate.</summary>
	/// <returns>The public key for the X.509 certificate as an array of bytes.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual byte[] GetPublicKey()
	{
		ThrowIfInvalid();
		byte[] array = lazyPublicKey;
		if (array == null)
		{
			array = (lazyPublicKey = Impl.PublicKeyValue);
		}
		return array.CloneByteArray();
	}

	/// <summary>Returns the serial number of the X.509v3 certificate.</summary>
	/// <returns>The serial number of the X.509 certificate as an array of bytes.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	public virtual byte[] GetSerialNumber()
	{
		ThrowIfInvalid();
		byte[] array = GetRawSerialNumber().CloneByteArray();
		Array.Reverse(array);
		return array;
	}

	/// <summary>Returns the serial number of the X.509v3 certificate.</summary>
	/// <returns>The serial number of the X.509 certificate as a hexadecimal string.</returns>
	public virtual string GetSerialNumberString()
	{
		ThrowIfInvalid();
		return GetRawSerialNumber().ToHexStringUpper();
	}

	private byte[] GetRawSerialNumber()
	{
		return lazySerialNumber ?? (lazySerialNumber = Impl.SerialNumber);
	}

	/// <summary>Returns the name of the principal to which the certificate was issued.</summary>
	/// <returns>The name of the principal to which the certificate was issued.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The certificate context is invalid.</exception>
	[Obsolete("This method has been deprecated.  Please use the Subject property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
	public virtual string GetName()
	{
		ThrowIfInvalid();
		return Impl.LegacySubject;
	}

	/// <summary>Returns the name of the certification authority that issued the X.509v3 certificate.</summary>
	/// <returns>The name of the certification authority that issued the X.509 certificate.</returns>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">An error with the certificate occurs. For example:The certificate file does not exist.The certificate is invalid.The certificate's password is incorrect.</exception>
	[Obsolete("This method has been deprecated.  Please use the Issuer property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
	public virtual string GetIssuerName()
	{
		ThrowIfInvalid();
		return Impl.LegacyIssuer;
	}

	/// <summary>Returns a string representation of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</summary>
	/// <returns>A string representation of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	public override string ToString()
	{
		return ToString(fVerbose: false);
	}

	/// <summary>Returns a string representation of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object, with extra information, if specified.</summary>
	/// <returns>A string representation of the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object.</returns>
	/// <param name="fVerbose">true to produce the verbose form of the string representation; otherwise, false. </param>
	public virtual string ToString(bool fVerbose)
	{
		if (!fVerbose || !X509Helper.IsValid(impl))
		{
			return base.ToString();
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("[Subject]");
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(Subject);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Issuer]");
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(Issuer);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Serial Number]");
		stringBuilder.Append("  ");
		byte[] serialNumber = GetSerialNumber();
		Array.Reverse(serialNumber);
		stringBuilder.Append(serialNumber.ToHexArrayUpper());
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Not Before]");
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(FormatDate(GetNotBefore()));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Not After]");
		stringBuilder.Append("  ");
		stringBuilder.AppendLine(FormatDate(GetNotAfter()));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("[Thumbprint]");
		stringBuilder.Append("  ");
		stringBuilder.Append(GetRawCertHash().ToHexArrayUpper());
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	/// <summary>Populates the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object with data from a byte array.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void Import(byte[] rawData)
	{
		throw new PlatformNotSupportedException("X509Certificate is immutable on this platform. Use the equivalent constructor instead.");
	}

	/// <summary>Populates the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object using data from a byte array, a password, and flags for determining how the private key is imported.</summary>
	/// <param name="rawData">A byte array containing data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
	{
		throw new PlatformNotSupportedException("X509Certificate is immutable on this platform. Use the equivalent constructor instead.");
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object using data from a byte array, a password, and a key storage flag.</summary>
	/// <param name="rawData">A byte array that contains data from an X.509 certificate. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="rawData" /> parameter is null.-or-The length of the <paramref name="rawData" /> parameter is 0.</exception>
	public virtual void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		throw new PlatformNotSupportedException("X509Certificate is immutable on this platform. Use the equivalent constructor instead.");
	}

	/// <summary>Populates the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object with information from a certificate file.</summary>
	/// <param name="fileName">The name of a certificate file represented as a string. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void Import(string fileName)
	{
		throw new PlatformNotSupportedException("X509Certificate is immutable on this platform. Use the equivalent constructor instead.");
	}

	/// <summary>Populates the <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object with information from a certificate file, a password, and a <see cref="T:System.Security.Cryptography.X509Certificates.X509KeyStorageFlags" /> value.</summary>
	/// <param name="fileName">The name of a certificate file represented as a string. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.KeyContainerPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Create" />
	/// </PermissionSet>
	[ComVisible(false)]
	public virtual void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
	{
		throw new PlatformNotSupportedException("X509Certificate is immutable on this platform. Use the equivalent constructor instead.");
	}

	/// <summary>Populates an <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate" /> object with information from a certificate file, a password, and a key storage flag.</summary>
	/// <param name="fileName">The name of a certificate file. </param>
	/// <param name="password">The password required to access the X.509 certificate data. </param>
	/// <param name="keyStorageFlags">A bitwise combination of the enumeration values that control where and how to import the certificate. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="fileName" /> parameter is null.</exception>
	public virtual void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
	{
		throw new PlatformNotSupportedException("X509Certificate is immutable on this platform. Use the equivalent constructor instead.");
	}

	internal DateTime GetNotAfter()
	{
		ThrowIfInvalid();
		DateTime dateTime = lazyNotAfter;
		if (dateTime == DateTime.MinValue)
		{
			dateTime = (lazyNotAfter = impl.NotAfter);
		}
		return dateTime;
	}

	internal DateTime GetNotBefore()
	{
		ThrowIfInvalid();
		DateTime dateTime = lazyNotBefore;
		if (dateTime == DateTime.MinValue)
		{
			dateTime = (lazyNotBefore = impl.NotBefore);
		}
		return dateTime;
	}

	/// <summary>Converts the specified date and time to a string.</summary>
	/// <returns>A string representation of the value of the <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="date">The date and time to convert.</param>
	protected static string FormatDate(DateTime date)
	{
		CultureInfo cultureInfo = CultureInfo.CurrentCulture;
		if (!cultureInfo.DateTimeFormat.Calendar.IsValidDay(date.Year, date.Month, date.Day, 0))
		{
			if (cultureInfo.DateTimeFormat.Calendar is UmAlQuraCalendar)
			{
				cultureInfo = cultureInfo.Clone() as CultureInfo;
				cultureInfo.DateTimeFormat.Calendar = new HijriCalendar();
			}
			else
			{
				cultureInfo = CultureInfo.InvariantCulture;
			}
		}
		return date.ToString(cultureInfo);
	}

	internal static void ValidateKeyStorageFlags(X509KeyStorageFlags keyStorageFlags)
	{
		if ((keyStorageFlags & ~(X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.UserProtected | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.EphemeralKeySet)) != X509KeyStorageFlags.DefaultKeySet)
		{
			throw new ArgumentException("Value of flags is invalid.", "keyStorageFlags");
		}
		X509KeyStorageFlags x509KeyStorageFlags = keyStorageFlags & (X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.EphemeralKeySet);
		if (x509KeyStorageFlags == (X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.EphemeralKeySet))
		{
			throw new ArgumentException(SR.Format("The flags '{0}' may not be specified together.", x509KeyStorageFlags), "keyStorageFlags");
		}
	}

	private void VerifyContentType(X509ContentType contentType)
	{
		if (contentType != X509ContentType.Cert && contentType != X509ContentType.SerializedCert && contentType != X509ContentType.Pfx)
		{
			throw new CryptographicException("Invalid content type.");
		}
	}

	internal void ImportHandle(X509CertificateImpl impl)
	{
		Reset();
		this.impl = impl;
	}

	internal void ThrowIfInvalid()
	{
		X509Helper.ThrowIfContextInvalid(impl);
	}
}
