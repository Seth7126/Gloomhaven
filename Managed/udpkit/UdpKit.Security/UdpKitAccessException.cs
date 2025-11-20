using System;
using System.Collections;
using System.Collections.Generic;

namespace UdpKit.Security;

public class UdpKitAccessException : Exception
{
	public override IDictionary Data => new Dictionary<string, string>();

	public override string Source { get; set; }

	public override string StackTrace => "";

	public override string HelpLink { get; set; }

	public override string Message => "You are not allowed to call this method.";

	public override Exception GetBaseException()
	{
		return this;
	}

	public override string ToString()
	{
		return Message;
	}
}
