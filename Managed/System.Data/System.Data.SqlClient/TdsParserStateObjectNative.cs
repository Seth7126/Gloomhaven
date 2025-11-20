using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Data.SqlClient;

internal class TdsParserStateObjectNative : TdsParserStateObject
{
	internal sealed class WritePacketCache : IDisposable
	{
		private bool _disposed;

		private Stack<SNIPacket> _packets;

		public WritePacketCache()
		{
			_disposed = false;
			_packets = new Stack<SNIPacket>();
		}

		public SNIPacket Take(SNIHandle sniHandle)
		{
			SNIPacket sNIPacket;
			if (_packets.Count > 0)
			{
				sNIPacket = _packets.Pop();
				SNINativeMethodWrapper.SNIPacketReset(sniHandle, SNINativeMethodWrapper.IOType.WRITE, sNIPacket, SNINativeMethodWrapper.ConsumerNumber.SNI_Consumer_SNI);
			}
			else
			{
				sNIPacket = new SNIPacket(sniHandle);
			}
			return sNIPacket;
		}

		public void Add(SNIPacket packet)
		{
			if (!_disposed)
			{
				_packets.Push(packet);
			}
			else
			{
				packet.Dispose();
			}
		}

		public void Clear()
		{
			while (_packets.Count > 0)
			{
				_packets.Pop().Dispose();
			}
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Clear();
			}
		}
	}

	private SNIHandle _sessionHandle;

	private SNIPacket _sniPacket;

	internal SNIPacket _sniAsyncAttnPacket;

	private readonly WritePacketCache _writePacketCache = new WritePacketCache();

	private GCHandle _gcHandle;

	private Dictionary<IntPtr, SNIPacket> _pendingWritePackets = new Dictionary<IntPtr, SNIPacket>();

	internal SNIHandle Handle => _sessionHandle;

	internal override uint Status
	{
		get
		{
			if (_sessionHandle == null)
			{
				return uint.MaxValue;
			}
			return _sessionHandle.Status;
		}
	}

	internal override object SessionHandle => _sessionHandle;

	protected override object EmptyReadPacket => IntPtr.Zero;

	public TdsParserStateObjectNative(TdsParser parser)
		: base(parser)
	{
	}

	internal TdsParserStateObjectNative(TdsParser parser, TdsParserStateObject physicalConnection, bool async)
		: base(parser, physicalConnection, async)
	{
	}

	protected override void CreateSessionHandle(TdsParserStateObject physicalConnection, bool async)
	{
		TdsParserStateObjectNative tdsParserStateObjectNative = physicalConnection as TdsParserStateObjectNative;
		SNINativeMethodWrapper.ConsumerInfo myInfo = CreateConsumerInfo(async);
		_sessionHandle = new SNIHandle(myInfo, tdsParserStateObjectNative.Handle);
	}

	private SNINativeMethodWrapper.ConsumerInfo CreateConsumerInfo(bool async)
	{
		SNINativeMethodWrapper.ConsumerInfo result = new SNINativeMethodWrapper.ConsumerInfo
		{
			defaultBufferSize = _outBuff.Length
		};
		if (async)
		{
			result.readDelegate = SNILoadHandle.SingletonInstance.ReadAsyncCallbackDispatcher;
			result.writeDelegate = SNILoadHandle.SingletonInstance.WriteAsyncCallbackDispatcher;
			_gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
			result.key = (IntPtr)_gcHandle;
		}
		return result;
	}

	internal override void CreatePhysicalSNIHandle(string serverName, bool ignoreSniOpenTimeout, long timerExpire, out byte[] instanceName, ref byte[] spnBuffer, bool flushCache, bool async, bool fParallel, bool isIntegratedSecurity)
	{
		spnBuffer = null;
		if (isIntegratedSecurity)
		{
			spnBuffer = new byte[SNINativeMethodWrapper.SniMaxComposedSpnLength];
		}
		SNINativeMethodWrapper.ConsumerInfo myInfo = CreateConsumerInfo(async);
		long num;
		if (long.MaxValue == timerExpire)
		{
			num = 2147483647L;
		}
		else
		{
			num = ADP.TimerRemainingMilliseconds(timerExpire);
			if (num > int.MaxValue)
			{
				num = 2147483647L;
			}
			else if (0 > num)
			{
				num = 0L;
			}
		}
		_sessionHandle = new SNIHandle(myInfo, serverName, spnBuffer, ignoreSniOpenTimeout, checked((int)num), out instanceName, flushCache, !async, fParallel);
	}

	protected override uint SNIPacketGetData(object packet, byte[] _inBuff, ref uint dataSize)
	{
		return SNINativeMethodWrapper.SNIPacketGetData((IntPtr)packet, _inBuff, ref dataSize);
	}

	protected override bool CheckPacket(object packet, TaskCompletionSource<object> source)
	{
		IntPtr intPtr = (IntPtr)packet;
		if (!(IntPtr.Zero == intPtr))
		{
			if (IntPtr.Zero != intPtr)
			{
				return source != null;
			}
			return false;
		}
		return true;
	}

	public void ReadAsyncCallback(IntPtr key, IntPtr packet, uint error)
	{
		ReadAsyncCallback(key, packet, error);
	}

	public void WriteAsyncCallback(IntPtr key, IntPtr packet, uint sniError)
	{
		WriteAsyncCallback(key, packet, sniError);
	}

	protected override void RemovePacketFromPendingList(object ptr)
	{
		IntPtr key = (IntPtr)ptr;
		lock (_writePacketLockObject)
		{
			if (_pendingWritePackets.TryGetValue(key, out var value))
			{
				_pendingWritePackets.Remove(key);
				_writePacketCache.Add(value);
			}
		}
	}

	internal override void Dispose()
	{
		SafeHandle sniPacket = _sniPacket;
		SafeHandle sessionHandle = _sessionHandle;
		SafeHandle sniAsyncAttnPacket = _sniAsyncAttnPacket;
		_sniPacket = null;
		_sessionHandle = null;
		_sniAsyncAttnPacket = null;
		DisposeCounters();
		if (sessionHandle != null || sniPacket != null)
		{
			sniPacket?.Dispose();
			sniAsyncAttnPacket?.Dispose();
			if (sessionHandle != null)
			{
				sessionHandle.Dispose();
				DecrementPendingCallbacks(release: true);
			}
		}
		DisposePacketCache();
	}

	protected override void FreeGcHandle(int remaining, bool release)
	{
		if ((remaining == 0 || release) && _gcHandle.IsAllocated)
		{
			_gcHandle.Free();
		}
	}

	internal override bool IsFailedHandle()
	{
		return _sessionHandle.Status != 0;
	}

	internal override object ReadSyncOverAsync(int timeoutRemaining, out uint error)
	{
		SNIHandle handle = Handle;
		if (handle == null)
		{
			throw ADP.ClosedConnectionError();
		}
		IntPtr ppNewPacket = IntPtr.Zero;
		error = SNINativeMethodWrapper.SNIReadSyncOverAsync(handle, ref ppNewPacket, GetTimeoutRemaining());
		return ppNewPacket;
	}

	internal override bool IsPacketEmpty(object readPacket)
	{
		return IntPtr.Zero == (IntPtr)readPacket;
	}

	internal override void ReleasePacket(object syncReadPacket)
	{
		SNINativeMethodWrapper.SNIPacketRelease((IntPtr)syncReadPacket);
	}

	internal override uint CheckConnection()
	{
		SNIHandle handle = Handle;
		if (handle != null)
		{
			return SNINativeMethodWrapper.SNICheckConnection(handle);
		}
		return 0u;
	}

	internal override object ReadAsync(out uint error, ref object handle)
	{
		IntPtr ppNewPacket = IntPtr.Zero;
		error = SNINativeMethodWrapper.SNIReadAsync((SNIHandle)handle, ref ppNewPacket);
		return ppNewPacket;
	}

	internal override object CreateAndSetAttentionPacket()
	{
		SNIPacket sNIPacket = (_sniAsyncAttnPacket = new SNIPacket(Handle));
		SetPacketData(sNIPacket, SQL.AttentionHeader, 8);
		return sNIPacket;
	}

	internal override uint WritePacket(object packet, bool sync)
	{
		return SNINativeMethodWrapper.SNIWritePacket(Handle, (SNIPacket)packet, sync);
	}

	internal override object AddPacketToPendingList(object packetToAdd)
	{
		SNIPacket sNIPacket = (SNIPacket)packetToAdd;
		_sniPacket = null;
		IntPtr intPtr = sNIPacket.DangerousGetHandle();
		lock (_writePacketLockObject)
		{
			_pendingWritePackets.Add(intPtr, sNIPacket);
		}
		return intPtr;
	}

	internal override bool IsValidPacket(object packetPointer)
	{
		return (IntPtr)packetPointer != IntPtr.Zero;
	}

	internal override object GetResetWritePacket()
	{
		if (_sniPacket != null)
		{
			SNINativeMethodWrapper.SNIPacketReset(Handle, SNINativeMethodWrapper.IOType.WRITE, _sniPacket, SNINativeMethodWrapper.ConsumerNumber.SNI_Consumer_SNI);
		}
		else
		{
			lock (_writePacketLockObject)
			{
				_sniPacket = _writePacketCache.Take(Handle);
			}
		}
		return _sniPacket;
	}

	internal override void ClearAllWritePackets()
	{
		if (_sniPacket != null)
		{
			_sniPacket.Dispose();
			_sniPacket = null;
		}
		lock (_writePacketLockObject)
		{
			_writePacketCache.Clear();
		}
	}

	internal override void SetPacketData(object packet, byte[] buffer, int bytesUsed)
	{
		SNINativeMethodWrapper.SNIPacketSetData((SNIPacket)packet, buffer, bytesUsed);
	}

	internal override uint SniGetConnectionId(ref Guid clientConnectionId)
	{
		return SNINativeMethodWrapper.SniGetConnectionId(Handle, ref clientConnectionId);
	}

	internal override uint DisabeSsl()
	{
		return SNINativeMethodWrapper.SNIRemoveProvider(Handle, SNINativeMethodWrapper.ProviderEnum.SSL_PROV);
	}

	internal override uint EnableMars(ref uint info)
	{
		return SNINativeMethodWrapper.SNIAddProvider(Handle, SNINativeMethodWrapper.ProviderEnum.SMUX_PROV, ref info);
	}

	internal override uint EnableSsl(ref uint info)
	{
		return SNINativeMethodWrapper.SNIAddProvider(Handle, SNINativeMethodWrapper.ProviderEnum.SSL_PROV, ref info);
	}

	internal override uint SetConnectionBufferSize(ref uint unsignedPacketSize)
	{
		return SNINativeMethodWrapper.SNISetInfo(Handle, SNINativeMethodWrapper.QTypes.SNI_QUERY_CONN_BUFSIZE, ref unsignedPacketSize);
	}

	internal override uint GenerateSspiClientContext(byte[] receivedBuff, uint receivedLength, ref byte[] sendBuff, ref uint sendLength, byte[] _sniSpnBuffer)
	{
		return SNINativeMethodWrapper.SNISecGenClientContext(Handle, receivedBuff, receivedLength, sendBuff, ref sendLength, _sniSpnBuffer);
	}

	internal override uint WaitForSSLHandShakeToComplete()
	{
		return SNINativeMethodWrapper.SNIWaitForSSLHandshakeToComplete(Handle, GetTimeoutRemaining());
	}

	internal override void DisposePacketCache()
	{
		lock (_writePacketLockObject)
		{
			_writePacketCache.Dispose();
		}
	}
}
