#define ENABLE_LOGS
using System;
using System.Runtime.InteropServices;
using AOT;
using SM.Utils;

namespace ExitGames.Client.Photon.Encryption;

public class EncryptorNative : IPhotonEncryptor
{
	public enum ChainingMode : byte
	{
		CBC,
		GCM
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void LogCallbackDelegate(IntPtr userData, int level, [MarshalAs(UnmanagedType.LPStr)] string msg);

	private enum egDebugLevel
	{
		OFF,
		ERRORS,
		WARNINGS,
		INFO,
		ALL
	}

	private const string LibName = "PhotonEncryptorPlugin";

	protected IntPtr encryptor;

	[DllImport("PhotonEncryptorPlugin")]
	public static extern IntPtr egconstructEncryptor2(byte[] pEncryptSecret, byte[] pHMACSecret, ChainingMode chainingMode, int mtu);

	[DllImport("PhotonEncryptorPlugin")]
	public static extern void egdestructEncryptor2(IntPtr pEncryptor);

	[DllImport("PhotonEncryptorPlugin")]
	public static extern void egencrypt2(IntPtr pEncryptor, byte[] pIn, int inSize, byte[] pHeader, byte[] pOut, int outOffset, ref int outSize);

	[DllImport("PhotonEncryptorPlugin")]
	public static extern void egdecrypt2(IntPtr pEncryptor, byte[] pIn, int inSize, int inOffset, byte[] pHeader, byte[] pOut, out int outSize);

	[DllImport("PhotonEncryptorPlugin")]
	public static extern int egcalculateEncryptedSize(IntPtr pEncryptor, int unencryptedSize);

	[DllImport("PhotonEncryptorPlugin")]
	public static extern int eggetFragmentLength(IntPtr pEncryptor);

	[DllImport("PhotonEncryptorPlugin")]
	public static extern void egsetEncryptorLoggingCallback(IntPtr userData, LogCallbackDelegate callback);

	[DllImport("PhotonEncryptorPlugin")]
	public static extern bool egsetEncryptorLoggingLevel(int level);

	[DllImport("PhotonEncryptorPlugin")]
	public static extern int eggetEncryptorPluginVersion();

	~EncryptorNative()
	{
		Dispose(dispose: false);
	}

	[MonoPInvokeCallback(typeof(LogCallbackDelegate))]
	private static void OnNativeLog(IntPtr userData, int debugLevel, string message)
	{
		switch ((egDebugLevel)debugLevel)
		{
		case egDebugLevel.ERRORS:
			LogUtils.LogError("EncryptorNative: " + message);
			break;
		case egDebugLevel.WARNINGS:
			LogUtils.LogWarning("EncryptorNative: " + message);
			break;
		case egDebugLevel.OFF:
		case egDebugLevel.INFO:
		case egDebugLevel.ALL:
			LogUtils.Log("EncryptorNative: " + message);
			break;
		}
	}

	public void Init(byte[] encryptionSecret, byte[] hmacSecret, byte[] ivBytes = null, bool chainingModeGCM = false, int mtu = 1200)
	{
		egsetEncryptorLoggingCallback(IntPtr.Zero, OnNativeLog);
		egsetEncryptorLoggingLevel(1);
		encryptor = egconstructEncryptor2(encryptionSecret, hmacSecret, chainingModeGCM ? ChainingMode.GCM : ChainingMode.CBC, mtu);
	}

	public void Dispose()
	{
		Dispose(dispose: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool dispose)
	{
		if (encryptor != IntPtr.Zero)
		{
			egdestructEncryptor2(encryptor);
			encryptor = IntPtr.Zero;
		}
	}

	public void Encrypt2(byte[] data, int len, byte[] header, byte[] output, int outOffset, ref int outSize)
	{
		egencrypt2(encryptor, data, len, header, output, outOffset, ref outSize);
	}

	public byte[] Decrypt2(byte[] data, int offset, int len, byte[] header, out int outLen)
	{
		outLen = data.Length;
		egdecrypt2(encryptor, data, len, offset, header, data, out outLen);
		return data;
	}

	public int CalculateEncryptedSize(int unencryptedSize)
	{
		return egcalculateEncryptedSize(encryptor, unencryptedSize);
	}

	public int CalculateFragmentLength()
	{
		return eggetFragmentLength(encryptor);
	}
}
