using System;

namespace WebSocketSharp;

public class CloseEventArgs : EventArgs
{
	private bool _clean;

	private ushort _code;

	private PayloadData _payloadData;

	private string _reason;

	internal PayloadData PayloadData => _payloadData ?? (_payloadData = new PayloadData(_code.Append(_reason)));

	public ushort Code => _code;

	public string Reason => _reason ?? string.Empty;

	public bool WasClean
	{
		get
		{
			return _clean;
		}
		internal set
		{
			_clean = value;
		}
	}

	internal CloseEventArgs()
	{
		_code = 1005;
		_payloadData = PayloadData.Empty;
	}

	internal CloseEventArgs(ushort code)
	{
		_code = code;
	}

	internal CloseEventArgs(CloseStatusCode code)
		: this((ushort)code)
	{
	}

	internal CloseEventArgs(PayloadData payloadData)
	{
		_payloadData = payloadData;
		byte[] applicationData = payloadData.ApplicationData;
		int num = applicationData.Length;
		_code = (ushort)((num > 1) ? applicationData.SubArray(0, 2).ToUInt16(ByteOrder.Big) : 1005);
		_reason = ((num > 2) ? applicationData.SubArray(2, num - 2).UTF8Decode() : string.Empty);
	}

	internal CloseEventArgs(ushort code, string reason)
	{
		_code = code;
		_reason = reason;
	}

	internal CloseEventArgs(CloseStatusCode code, string reason)
		: this((ushort)code, reason)
	{
	}
}
