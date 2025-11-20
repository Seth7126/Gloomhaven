using System.Diagnostics;

namespace UdpKit;

internal static class UdpLog
{
	public delegate void Writer(uint level, string message);

	public const uint ERROR = 0u;

	public const uint INFO = 1u;

	public const uint DEBUG = 4u;

	public const uint TRACE = 8u;

	public const uint WARN = 16u;

	private static uint enabled = 29u;

	private static Writer writer = null;

	private static readonly object sync = new object();

	private static void Write(uint level, string message)
	{
		lock (sync)
		{
			writer?.Invoke(level, message);
		}
	}

	public static void Info(string format, params object[] args)
	{
		if (UdpMath.IsSet(enabled, 1u))
		{
			Write(1u, string.Format(format, args));
		}
	}

	[Conditional("TRACE")]
	public static void Trace(string format, params object[] args)
	{
		if (UdpMath.IsSet(enabled, 8u))
		{
			Write(8u, string.Format(format, args));
		}
	}

	[Conditional("DEBUG")]
	public static void Debug(string format, params object[] args)
	{
		if (UdpMath.IsSet(enabled, 4u))
		{
			Write(4u, string.Format(format, args));
		}
	}

	public static void Warn(string format, params object[] args)
	{
		if (UdpMath.IsSet(enabled, 16u))
		{
			Write(16u, string.Format(format, args));
		}
	}

	public static void Error(string format, params object[] args)
	{
		Write(0u, string.Format(format, args));
	}

	public static void SetWriter(Writer callback)
	{
		writer = callback;
	}

	public static void Disable(uint flag)
	{
		enabled &= ~flag;
	}

	public static void Enable(uint flag)
	{
		enabled |= flag;
	}
}
