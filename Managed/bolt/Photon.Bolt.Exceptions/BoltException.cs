using System;

namespace Photon.Bolt.Exceptions;

public class BoltException : Exception
{
	public object ExtraInfo { get; set; }

	public override string Message => (ExtraInfo == null) ? base.Message : string.Format(base.Message, ExtraInfo.ToString());

	public BoltException(string message, params object[] args)
		: base(string.Format(message, args))
	{
	}
}
