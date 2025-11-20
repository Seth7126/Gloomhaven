using System.Diagnostics;

namespace UdpKit;

internal static class UdpAssert
{
	[Conditional("DEBUG")]
	internal static void Assert(bool condition)
	{
		if (!condition)
		{
			throw new UdpException("assert failed");
		}
	}

	[Conditional("DEBUG")]
	internal static void Assert(bool condition, string message)
	{
		if (!condition)
		{
			throw new UdpException("assert failed: " + message);
		}
	}

	[Conditional("DEBUG")]
	internal static void Assert(bool condition, string message, params object[] args)
	{
		if (!condition)
		{
			throw new UdpException("assert failed: " + string.Format(message, args));
		}
	}
}
