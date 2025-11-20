using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Security.Cryptography;

/// <summary>Provides the base class for data protectors.</summary>
public abstract class DataProtector
{
	private string m_applicationName;

	private string m_primaryPurpose;

	private IEnumerable<string> m_specificPurposes;

	private volatile byte[] m_hashedPurpose;

	/// <summary>Gets the name of the application.</summary>
	/// <returns>The name of the application.</returns>
	protected string ApplicationName => m_applicationName;

	/// <summary>Specifies whether the hash is prepended to the text array before encryption.</summary>
	/// <returns>Always true.</returns>
	protected virtual bool PrependHashedPurposeToPlaintext => true;

	/// <summary>Gets the primary purpose for the protected data.</summary>
	/// <returns>The primary purpose for the protected data.</returns>
	protected string PrimaryPurpose => m_primaryPurpose;

	/// <summary>Gets the specific purposes for the protected data.</summary>
	/// <returns>A collection of the specific purposes for the protected data.</returns>
	protected IEnumerable<string> SpecificPurposes => m_specificPurposes;

	/// <summary>Creates a new instance of the <see cref="T:System.Security.Cryptography.DataProtector" /> class by using the provided application name, primary purpose, and specific purposes.</summary>
	/// <param name="applicationName">The name of the application.</param>
	/// <param name="primaryPurpose">The primary purpose for the protected data. See Remarks for additional important information.</param>
	/// <param name="specificPurposes">The specific purposes for the protected data. See Remarks for additional important information.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="applicationName" /> is an empty string or null.-or-<paramref name="primaryPurpose" /> is an empty string or null.-or-<paramref name="specificPurposes" /> contains an empty string or null.</exception>
	protected DataProtector(string applicationName, string primaryPurpose, string[] specificPurposes)
	{
		if (string.IsNullOrWhiteSpace(applicationName))
		{
			throw new ArgumentException("Invalid application name and/or purpose", "applicationName");
		}
		if (string.IsNullOrWhiteSpace(primaryPurpose))
		{
			throw new ArgumentException("Invalid application name and/or purpose", "primaryPurpose");
		}
		if (specificPurposes != null)
		{
			for (int i = 0; i < specificPurposes.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(specificPurposes[i]))
				{
					throw new ArgumentException("Invalid application name and/or purpose", "specificPurposes");
				}
			}
		}
		m_applicationName = applicationName;
		m_primaryPurpose = primaryPurpose;
		List<string> list = new List<string>();
		if (specificPurposes != null)
		{
			list.AddRange(specificPurposes);
		}
		m_specificPurposes = list;
	}

	/// <summary>Creates a hash of the property values specified by the constructor.</summary>
	/// <returns>An array of bytes that contain the hash of the <see cref="P:System.Security.Cryptography.DataProtector.ApplicationName" />, <see cref="P:System.Security.Cryptography.DataProtector.PrimaryPurpose" />, and <see cref="P:System.Security.Cryptography.DataProtector.SpecificPurposes" /> properties. </returns>
	protected virtual byte[] GetHashedPurpose()
	{
		if (m_hashedPurpose == null)
		{
			using HashAlgorithm hashAlgorithm = HashAlgorithm.Create("System.Security.Cryptography.Sha256Cng");
			using (BinaryWriter binaryWriter = new BinaryWriter(new CryptoStream(new MemoryStream(), hashAlgorithm, CryptoStreamMode.Write), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)))
			{
				binaryWriter.Write(ApplicationName);
				binaryWriter.Write(PrimaryPurpose);
				foreach (string specificPurpose in SpecificPurposes)
				{
					binaryWriter.Write(specificPurpose);
				}
			}
			m_hashedPurpose = hashAlgorithm.Hash;
		}
		return m_hashedPurpose;
	}

	/// <summary>Determines if re-encryption is required for the specified encrypted data.</summary>
	/// <returns>true if the data must be re-encrypted; otherwise, false.</returns>
	/// <param name="encryptedData">The encrypted data to be evaluated.</param>
	public abstract bool IsReprotectRequired(byte[] encryptedData);

	/// <summary>Creates an instance of a data protector implementation by using the specified class name of the data protector, the application name, the primary purpose, and the specific purposes.</summary>
	/// <returns>A data protector implementation object.</returns>
	/// <param name="providerClass">The class name for the data protector.</param>
	/// <param name="applicationName">The name of the application.</param>
	/// <param name="primaryPurpose">The primary purpose for the protected data.</param>
	/// <param name="specificPurposes">The specific purposes for the protected data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="providerClass" /> is null.</exception>
	public static DataProtector Create(string providerClass, string applicationName, string primaryPurpose, params string[] specificPurposes)
	{
		if (providerClass == null)
		{
			throw new ArgumentNullException("providerClass");
		}
		return (DataProtector)CryptoConfig.CreateFromName(providerClass, applicationName, primaryPurpose, specificPurposes);
	}

	/// <summary>Protects the specified user data.</summary>
	/// <returns>A byte array that contains the encrypted data.</returns>
	/// <param name="userData">The data to be protected.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="userData" /> is null.</exception>
	public byte[] Protect(byte[] userData)
	{
		if (userData == null)
		{
			throw new ArgumentNullException("userData");
		}
		if (PrependHashedPurposeToPlaintext)
		{
			byte[] hashedPurpose = GetHashedPurpose();
			byte[] array = new byte[userData.Length + hashedPurpose.Length];
			Array.Copy(hashedPurpose, 0, array, 0, hashedPurpose.Length);
			Array.Copy(userData, 0, array, hashedPurpose.Length, userData.Length);
			userData = array;
		}
		return ProviderProtect(userData);
	}

	/// <summary>Specifies the delegate method in the derived class that the <see cref="M:System.Security.Cryptography.DataProtector.Protect(System.Byte[])" /> method in the base class calls back into.</summary>
	/// <returns>A byte array that contains the encrypted data.</returns>
	/// <param name="userData">The data to be encrypted.</param>
	protected abstract byte[] ProviderProtect(byte[] userData);

	/// <summary>Specifies the delegate method in the derived class that the <see cref="M:System.Security.Cryptography.DataProtector.Unprotect(System.Byte[])" /> method in the base class calls back into.</summary>
	/// <returns>The unencrypted data..</returns>
	/// <param name="encryptedData">The data to be unencrypted.</param>
	protected abstract byte[] ProviderUnprotect(byte[] encryptedData);

	/// <summary>Unprotects the specified protected data.</summary>
	/// <returns>A byte array that contains the plain-text data.</returns>
	/// <param name="encryptedData">The encrypted data to be unprotected.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="encryptedData" /> is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">
	///   <paramref name="encryptedData" /> contained an invalid purpose.</exception>
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	public byte[] Unprotect(byte[] encryptedData)
	{
		if (encryptedData == null)
		{
			throw new ArgumentNullException("encryptedData");
		}
		if (PrependHashedPurposeToPlaintext)
		{
			byte[] array = ProviderUnprotect(encryptedData);
			byte[] hashedPurpose = GetHashedPurpose();
			bool flag = array.Length >= hashedPurpose.Length;
			for (int i = 0; i < hashedPurpose.Length; i++)
			{
				if (hashedPurpose[i] != array[i % array.Length])
				{
					flag = false;
				}
			}
			if (!flag)
			{
				throw new CryptographicException("Invalid data protection purpose");
			}
			byte[] array2 = new byte[array.Length - hashedPurpose.Length];
			Array.Copy(array, hashedPurpose.Length, array2, 0, array2.Length);
			return array2;
		}
		return ProviderUnprotect(encryptedData);
	}
}
