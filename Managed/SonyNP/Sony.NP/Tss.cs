using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public class Tss
{
	[StructLayout(LayoutKind.Sequential)]
	public class GetDataRequest : RequestBase
	{
		internal ulong offset;

		internal ulong length;

		internal ulong lastModifiedTicks;

		internal int tssSlotId;

		[MarshalAs(UnmanagedType.I1)]
		internal bool retrieveStatusOnly;

		public ulong Offset
		{
			get
			{
				return offset;
			}
			set
			{
				offset = value;
			}
		}

		public ulong Length
		{
			get
			{
				return length;
			}
			set
			{
				length = value;
			}
		}

		public DateTime LastModifiedTicks
		{
			get
			{
				return Core.RtcTicksToDateTime(lastModifiedTicks);
			}
			set
			{
				lastModifiedTicks = Core.DateTimeToRtcTicks(value);
			}
		}

		public int TssSlotId
		{
			get
			{
				return tssSlotId;
			}
			set
			{
				tssSlotId = value;
			}
		}

		public bool RetrieveStatusOnly
		{
			get
			{
				return retrieveStatusOnly;
			}
			set
			{
				retrieveStatusOnly = value;
			}
		}

		public GetDataRequest()
			: base(ServiceTypes.Tss, FunctionTypes.TssGetData)
		{
		}
	}

	public enum TssStatusCodes
	{
		Ok,
		Partial,
		NotModified
	}

	public class TssDataResponse : ResponseBase
	{
		internal byte[] data;

		internal DateTime lastModified;

		internal TssStatusCodes statusCode;

		internal long contentLength;

		public byte[] Data => data;

		public DateTime LastModified => lastModified;

		public TssStatusCodes StatusCode => statusCode;

		public long ContentLength => contentLength;

		protected internal override void ReadResult(uint id, FunctionTypes apiCalled, RequestBase request)
		{
			base.ReadResult(id, apiCalled, request);
			APIResult result;
			MemoryBuffer memoryBuffer = BeginReadResponseBuffer(id, apiCalled, out result);
			if (result.RaiseException)
			{
				throw new NpToolkitException(result);
			}
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TssDataBegin);
			memoryBuffer.ReadData(ref data);
			lastModified = Core.ReadRtcTick(memoryBuffer);
			statusCode = (TssStatusCodes)memoryBuffer.ReadInt32();
			contentLength = memoryBuffer.ReadInt64();
			memoryBuffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.TssDataEnd);
			EndReadResponseBuffer(memoryBuffer);
		}
	}

	[DllImport("UnityNpToolkit2")]
	private static extern int PrxTssGetData(GetDataRequest request, out APIResult result);

	public static int GetData(GetDataRequest request, TssDataResponse response)
	{
		if (response.locked)
		{
			throw new NpToolkitException("Response object is already locked");
		}
		APIResult result;
		int num = PrxTssGetData(request, out result);
		if (result.RaiseException)
		{
			throw new NpToolkitException(result);
		}
		RequestBase.FinaliseRequest(request, response, num);
		return num;
	}
}
