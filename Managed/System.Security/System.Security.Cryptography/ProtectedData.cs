using System.Runtime.InteropServices;
using Internal.Cryptography;

namespace System.Security.Cryptography;

/// <summary>Provides methods for encrypting and decrypting data. This class cannot be inherited.</summary>
public static class ProtectedData
{
	private static readonly byte[] s_nonEmpty = new byte[1];

	/// <summary>Encrypts the data in a specified byte array and returns a byte array that contains the encrypted data.</summary>
	/// <returns>A byte array representing the encrypted data.</returns>
	/// <param name="userData">A byte array that contains data to encrypt. </param>
	/// <param name="optionalEntropy">An optional additional byte array used to increase the complexity of the encryption, or null for no additional complexity.</param>
	/// <param name="scope">One of the enumeration values that specifies the scope of encryption. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="userData" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The encryption failed.</exception>
	/// <exception cref="T:System.NotSupportedException">The operating system does not support this method. </exception>
	/// <exception cref="T:System.OutOfMemoryException">The system ran out of memory while encrypting the data.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.DataProtectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ProtectData" />
	/// </PermissionSet>
	public static byte[] Protect(byte[] userData, byte[] optionalEntropy, DataProtectionScope scope)
	{
		if (userData == null)
		{
			throw new ArgumentNullException("userData");
		}
		return ProtectOrUnprotect(userData, optionalEntropy, scope, protect: true);
	}

	/// <summary>Decrypts the data in a specified byte array and returns a byte array that contains the decrypted data.</summary>
	/// <returns>A byte array representing the decrypted data.</returns>
	/// <param name="encryptedData">A byte array containing data encrypted using the <see cref="M:System.Security.Cryptography.ProtectedData.Protect(System.Byte[],System.Byte[],System.Security.Cryptography.DataProtectionScope)" /> method. </param>
	/// <param name="optionalEntropy">An optional additional byte array that was used to encrypt the data, or null if the additional byte array was not used.</param>
	/// <param name="scope">One of the enumeration values that specifies the scope of data protection that was used to encrypt the data. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="encryptedData" /> parameter is null.</exception>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The decryption failed.</exception>
	/// <exception cref="T:System.NotSupportedException">The operating system does not support this method. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.DataProtectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnprotectData" />
	/// </PermissionSet>
	public static byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy, DataProtectionScope scope)
	{
		if (encryptedData == null)
		{
			throw new ArgumentNullException("encryptedData");
		}
		return ProtectOrUnprotect(encryptedData, optionalEntropy, scope, protect: false);
	}

	private unsafe static byte[] ProtectOrUnprotect(byte[] inputData, byte[] optionalEntropy, DataProtectionScope scope, bool protect)
	{
		fixed (byte* ptr = ((inputData.Length == 0) ? s_nonEmpty : inputData))
		{
			fixed (byte* ptr2 = optionalEntropy)
			{
				global::Interop.Crypt32.DATA_BLOB pDataIn = new global::Interop.Crypt32.DATA_BLOB((IntPtr)ptr, (uint)inputData.Length);
				global::Interop.Crypt32.DATA_BLOB pOptionalEntropy = default(global::Interop.Crypt32.DATA_BLOB);
				if (optionalEntropy != null)
				{
					pOptionalEntropy = new global::Interop.Crypt32.DATA_BLOB((IntPtr)ptr2, (uint)optionalEntropy.Length);
				}
				global::Interop.Crypt32.CryptProtectDataFlags cryptProtectDataFlags = global::Interop.Crypt32.CryptProtectDataFlags.CRYPTPROTECT_UI_FORBIDDEN;
				if (scope == DataProtectionScope.LocalMachine)
				{
					cryptProtectDataFlags |= global::Interop.Crypt32.CryptProtectDataFlags.CRYPTPROTECT_LOCAL_MACHINE;
				}
				global::Interop.Crypt32.DATA_BLOB pDataOut = default(global::Interop.Crypt32.DATA_BLOB);
				try
				{
					if (!(protect ? global::Interop.Crypt32.CryptProtectData(ref pDataIn, null, ref pOptionalEntropy, IntPtr.Zero, IntPtr.Zero, cryptProtectDataFlags, out pDataOut) : global::Interop.Crypt32.CryptUnprotectData(ref pDataIn, IntPtr.Zero, ref pOptionalEntropy, IntPtr.Zero, IntPtr.Zero, cryptProtectDataFlags, out pDataOut)))
					{
						int lastWin32Error = Marshal.GetLastWin32Error();
						if (protect && ErrorMayBeCausedByUnloadedProfile(lastWin32Error))
						{
							throw new CryptographicException("The data protection operation was unsuccessful. This may have been caused by not having the user profile loaded for the current thread's user context, which may be the case when the thread is impersonating.");
						}
						throw lastWin32Error.ToCryptographicException();
					}
					if (pDataOut.pbData == IntPtr.Zero)
					{
						throw new OutOfMemoryException();
					}
					int cbData = (int)pDataOut.cbData;
					byte[] array = new byte[cbData];
					Marshal.Copy(pDataOut.pbData, array, 0, cbData);
					return array;
				}
				finally
				{
					if (pDataOut.pbData != IntPtr.Zero)
					{
						int cbData2 = (int)pDataOut.cbData;
						byte* ptr3 = (byte*)(void*)pDataOut.pbData;
						for (int i = 0; i < cbData2; i++)
						{
							ptr3[i] = 0;
						}
						Marshal.FreeHGlobal(pDataOut.pbData);
					}
				}
			}
		}
	}

	private static bool ErrorMayBeCausedByUnloadedProfile(int errorCode)
	{
		if (errorCode != -2147024894)
		{
			return errorCode == 2;
		}
		return true;
	}
}
