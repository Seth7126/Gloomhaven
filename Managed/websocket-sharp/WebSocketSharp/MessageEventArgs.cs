using System;

namespace WebSocketSharp;

public class MessageEventArgs : EventArgs
{
	private string _data;

	private bool _dataSet;

	private Opcode _opcode;

	private byte[] _rawData;

	public string Data
	{
		get
		{
			if (!_dataSet)
			{
				_data = ((_opcode != Opcode.Binary) ? _rawData.UTF8Decode() : BitConverter.ToString(_rawData));
				_dataSet = true;
			}
			return _data;
		}
	}

	public bool IsBinary => _opcode == Opcode.Binary;

	public bool IsPing => _opcode == Opcode.Ping;

	public bool IsText => _opcode == Opcode.Text;

	public byte[] RawData => _rawData;

	[Obsolete("This property will be removed. Use any of the Is properties instead.")]
	public Opcode Type => _opcode;

	internal MessageEventArgs(WebSocketFrame frame)
	{
		_opcode = frame.Opcode;
		_rawData = frame.PayloadData.ApplicationData;
	}

	internal MessageEventArgs(Opcode opcode, byte[] rawData)
	{
		if ((ulong)rawData.LongLength > PayloadData.MaxLength)
		{
			throw new WebSocketException(CloseStatusCode.TooBig);
		}
		_opcode = opcode;
		_rawData = rawData;
	}
}
