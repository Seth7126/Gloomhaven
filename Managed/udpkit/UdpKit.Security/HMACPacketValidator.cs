using System;
using System.Security.Cryptography;

namespace UdpKit.Security;

public class HMACPacketValidator : IPacketHashValidator
{
	private HMACSHA256 _hasher;

	private readonly byte[] _tempArray = new byte[8192];

	private readonly byte[] _secret;

	public int Sha256ByteSize { get; } = 32;

	private HMACSHA256 Hasher
	{
		get
		{
			if (_hasher == null || !_hasher.CanReuseTransform)
			{
				_hasher = new HMACSHA256(_secret);
			}
			return _hasher;
		}
	}

	public HMACPacketValidator(string secret)
	{
		_secret = Convert.FromBase64String(secret);
	}

	public int AppendHashToData(byte[] buffer, int length)
	{
		byte[] src = Hasher.ComputeHash(buffer, 0, length);
		Blit.Clear(_tempArray);
		Buffer.BlockCopy(buffer, 0, _tempArray, 0, length);
		Buffer.BlockCopy(src, 0, _tempArray, length, Sha256ByteSize);
		Blit.Clear(buffer);
		Buffer.BlockCopy(_tempArray, 0, buffer, 0, length + Sha256ByteSize);
		return length + Sha256ByteSize;
	}

	public bool ValidatePacket(byte[] buffer, int length)
	{
		byte[] hash = Hasher.ComputeHash(buffer, 0, length - Sha256ByteSize);
		Blit.Clear(_tempArray);
		Buffer.BlockCopy(buffer, length - Sha256ByteSize, _tempArray, 0, Sha256ByteSize);
		return TestHashes(_tempArray, hash);
	}

	public int GetLengthWithoutHash(byte[] buffer, int length)
	{
		return length - Sha256ByteSize;
	}

	private bool TestHashes(byte[] buffer, byte[] hash)
	{
		for (int i = 0; i < Sha256ByteSize; i++)
		{
			if (buffer[i] != hash[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static byte[] GenerateSecret()
	{
		byte[] array = new byte[32];
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		rNGCryptoServiceProvider.GetBytes(array);
		return array;
	}
}
