using System.Runtime.InteropServices;

namespace System.Data.SqlClient;

internal sealed class SNILoadHandle : SafeHandle
{
	internal static readonly SNILoadHandle SingletonInstance = new SNILoadHandle();

	internal readonly SNINativeMethodWrapper.SqlAsyncCallbackDelegate ReadAsyncCallbackDispatcher = ReadDispatcher;

	internal readonly SNINativeMethodWrapper.SqlAsyncCallbackDelegate WriteAsyncCallbackDispatcher = WriteDispatcher;

	private readonly uint _sniStatus = uint.MaxValue;

	private readonly EncryptionOptions _encryptionOption;

	public override bool IsInvalid => IntPtr.Zero == handle;

	public uint Status => _sniStatus;

	public EncryptionOptions Options => _encryptionOption;

	private SNILoadHandle()
		: base(IntPtr.Zero, ownsHandle: true)
	{
		try
		{
		}
		finally
		{
			_sniStatus = SNINativeMethodWrapper.SNIInitialize();
			uint pbQInfo = 0u;
			if (_sniStatus == 0)
			{
				SNINativeMethodWrapper.SNIQueryInfo(SNINativeMethodWrapper.QTypes.SNI_QUERY_CLIENT_ENCRYPT_POSSIBLE, ref pbQInfo);
			}
			_encryptionOption = ((pbQInfo == 0) ? EncryptionOptions.NOT_SUP : EncryptionOptions.OFF);
			handle = (IntPtr)1;
		}
	}

	protected override bool ReleaseHandle()
	{
		if (handle != IntPtr.Zero)
		{
			if (_sniStatus == 0)
			{
				LocalDBAPI.ReleaseDLLHandles();
				SNINativeMethodWrapper.SNITerminate();
			}
			handle = IntPtr.Zero;
		}
		return true;
	}

	private static void ReadDispatcher(IntPtr key, IntPtr packet, uint error)
	{
		if (IntPtr.Zero != key)
		{
			((TdsParserStateObject)((GCHandle)key).Target)?.ReadAsyncCallback(IntPtr.Zero, packet, error);
		}
	}

	private static void WriteDispatcher(IntPtr key, IntPtr packet, uint error)
	{
		if (IntPtr.Zero != key)
		{
			((TdsParserStateObject)((GCHandle)key).Target)?.WriteAsyncCallback(IntPtr.Zero, packet, error);
		}
	}
}
