using System;

namespace UdpKit;

public class UdpException : Exception
{
	internal UdpException()
	{
	}

	internal UdpException(string fmt, params object[] args)
		: this(string.Format(fmt, args))
	{
	}

	internal UdpException(string msg)
		: base(msg)
	{
		UdpLog.Error(msg);
	}
}
