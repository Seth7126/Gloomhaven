using System;
using System.Text;
using UnityEngine;

namespace UdpKit.Security;

public class EncryptionManager : Singleton<EncryptionManager>
{
	private DataEncryptor _encryptor;

	private IPacketHashValidator _packetValidator;

	private IPacketIdValidator _idValidator;

	private string _aesIV;

	private string _aesKey;

	private string _hashSecret;

	public bool Ready => _encryptor != null && _packetValidator != null && _idValidator != null;

	public string AesIV
	{
		get
		{
			if (string.IsNullOrEmpty(_aesIV))
			{
				UdpLog.Warn("AES IV not initliazed");
			}
			return _aesIV;
		}
	}

	public string AesKey
	{
		get
		{
			if (string.IsNullOrEmpty(_aesKey))
			{
				UdpLog.Warn("AES Key not initliazed");
			}
			return _aesKey;
		}
	}

	public string HashSecret
	{
		get
		{
			if (string.IsNullOrEmpty(_hashSecret))
			{
				UdpLog.Warn("Hash Secret not initliazed");
			}
			return _hashSecret;
		}
	}

	public void InitializeEncryption()
	{
		byte[] inArray = GenerateAesIV();
		byte[] inArray2 = GenerateAesKey();
		byte[] inArray3 = GenerateHashSecret();
		string aesIV = Convert.ToBase64String(inArray);
		string aesKey = Convert.ToBase64String(inArray2);
		string hashSecret = Convert.ToBase64String(inArray3);
		InitializeEncryption(aesIV, aesKey, hashSecret);
	}

	public void InitializeEncryption(string aesIV, string aesKey, string hashSecret)
	{
		_aesIV = aesIV;
		_aesKey = aesKey;
		_hashSecret = hashSecret;
		_encryptor = new DataEncryptor(_aesIV, _aesKey);
		_packetValidator = new HMACPacketValidator(_hashSecret);
		_idValidator = Singleton<PacketIdValidator>.Instance;
		Debug.LogFormat("Encryption System is {0}", Ready ? "Ready" : "not Ready");
	}

	public void DeinitializeEncryption()
	{
		_encryptor = null;
		_packetValidator = null;
		Singleton<PacketIdValidator>.Instance.Clear();
		_idValidator = null;
	}

	public int Encrypt(UdpEndPoint endPoint, byte[] buffer, int length)
	{
		if (!Ready)
		{
			UdpLog.Warn("Trying to encrypt a packet, but the encryption was not setup");
			return -1;
		}
		length = _idValidator.PrefixPacketId(endPoint, buffer, length);
		length = _packetValidator.AppendHashToData(buffer, length);
		length = _encryptor.EncryptData(buffer, length);
		return length;
	}

	public int Decrypt(UdpEndPoint endPoint, byte[] buffer, int length)
	{
		if (!Ready)
		{
			UdpLog.Warn("Trying to decrypt a packet, but the encryption was not setup");
			return -1;
		}
		length = _encryptor.DecryptData(buffer, length);
		if (!_packetValidator.ValidatePacket(buffer, length))
		{
			UdpLog.Warn("Packet invalid hash comparison");
			return -1;
		}
		length = _idValidator.ValidatePacketId(endPoint, buffer, length);
		if (length == -1)
		{
			UdpLog.Warn("Packet ID smaller then expected, discarding.");
			return -1;
		}
		length = _packetValidator.GetLengthWithoutHash(buffer, length);
		return length;
	}

	public void RemoveEndPointReference(UdpEndPoint udpEndPoint)
	{
		if (Ready)
		{
			_idValidator.RemoveEndPointReference(udpEndPoint);
		}
	}

	public static string Base64Encode(string plainText, Encoding encoding)
	{
		byte[] bytes = encoding.GetBytes(plainText);
		return Convert.ToBase64String(bytes);
	}

	public static string Base64Decode(string base64EncodedData, Encoding encoding)
	{
		byte[] bytes = Convert.FromBase64String(base64EncodedData);
		return encoding.GetString(bytes);
	}

	public static byte[] GenerateAesKey()
	{
		return DataEncryptor.GenerateKey();
	}

	public static byte[] GenerateAesIV()
	{
		return DataEncryptor.GenerateIV();
	}

	public static byte[] GenerateHashSecret()
	{
		return HMACPacketValidator.GenerateSecret();
	}
}
