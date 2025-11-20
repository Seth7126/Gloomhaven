using System;
using System.IO;
using System.Security.Cryptography;

namespace UdpKit.Security;

public class DataEncryptor : IDataEncryption
{
	private readonly Aes _cryptoProvider;

	private ICryptoTransform _encryptor;

	private ICryptoTransform _decryptor;

	private readonly byte[] _aesKey;

	private readonly byte[] _aesIV;

	private readonly byte[] encryptBuffer;

	private readonly byte[] decryptBuffer;

	private ICryptoTransform Decryptor
	{
		get
		{
			if (_decryptor == null || !_decryptor.CanReuseTransform)
			{
				_decryptor = _cryptoProvider.CreateDecryptor(_aesKey, _aesIV);
			}
			return _decryptor;
		}
	}

	private ICryptoTransform Encryptor
	{
		get
		{
			if (_encryptor == null || !_encryptor.CanReuseTransform)
			{
				_encryptor = _cryptoProvider.CreateEncryptor(_aesKey, _aesIV);
			}
			return _encryptor;
		}
	}

	private byte[] GetEncryptBuffer()
	{
		Array.Clear(encryptBuffer, 0, encryptBuffer.Length);
		return encryptBuffer;
	}

	private byte[] GetDecryptBuffer()
	{
		Array.Clear(decryptBuffer, 0, decryptBuffer.Length);
		return decryptBuffer;
	}

	public DataEncryptor(string aesIV, string aesKey)
	{
		_aesIV = Convert.FromBase64String(aesIV);
		_aesKey = Convert.FromBase64String(aesKey);
		_cryptoProvider = BuildAesProvider();
		encryptBuffer = new byte[8192];
		decryptBuffer = new byte[8192];
	}

	public int EncryptData(byte[] buffer, int length)
	{
		return CypherData(buffer, length, Encryptor, GetEncryptBuffer());
	}

	public byte[] EncryptDataAlloc(byte[] buffer, int length)
	{
		return CypherDataAlloc(buffer, length, Encryptor);
	}

	public int DecryptData(byte[] buffer, int length)
	{
		return CypherData(buffer, length, Decryptor, GetDecryptBuffer());
	}

	public byte[] DecryptDataAlloc(byte[] buffer, int length)
	{
		return CypherDataAlloc(buffer, length, Decryptor);
	}

	private int CypherData(byte[] buffer, int length, ICryptoTransform cryptoTransform, byte[] memoryBuffer)
	{
		using (MemoryStream memoryStream = new MemoryStream(memoryBuffer, 0, memoryBuffer.Length, writable: true, publiclyVisible: true))
		{
			using CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
			cryptoStream.Write(buffer, 0, length);
			cryptoStream.FlushFinalBlock();
			length = (int)memoryStream.Position;
			Buffer.BlockCopy(memoryStream.GetBuffer(), 0, buffer, 0, length);
		}
		return length;
	}

	private byte[] CypherDataAlloc(byte[] buffer, int length, ICryptoTransform cryptoTransform)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
		cryptoStream.Write(buffer, 0, length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	private static Aes BuildAesProvider()
	{
		Aes aes = Aes.Create();
		aes.Mode = CipherMode.CBC;
		aes.KeySize = 256;
		return aes;
	}

	internal static byte[] GenerateKey()
	{
		Aes aes = BuildAesProvider();
		aes.GenerateKey();
		return aes.Key;
	}

	internal static byte[] GenerateIV()
	{
		Aes aes = BuildAesProvider();
		aes.GenerateIV();
		return aes.IV;
	}
}
