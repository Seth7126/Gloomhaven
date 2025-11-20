using System;

namespace XUnity.Common.Logging;

public abstract class XuaLogger
{
	private static XuaLogger _default;

	private static XuaLogger _common;

	private static XuaLogger _resourceRedirector;

	public static XuaLogger AutoTranslator
	{
		get
		{
			if (_default == null)
			{
				_default = CreateLogger("XUnity.AutoTranslator");
			}
			return _default;
		}
		set
		{
			_default = value ?? throw new ArgumentNullException("value");
		}
	}

	public static XuaLogger Common
	{
		get
		{
			if (_common == null)
			{
				_common = CreateLogger("XUnity.Common");
			}
			return _common;
		}
		set
		{
			_common = value ?? throw new ArgumentNullException("value");
		}
	}

	public static XuaLogger ResourceRedirector
	{
		get
		{
			if (_resourceRedirector == null)
			{
				_resourceRedirector = CreateLogger("XUnity.ResourceRedirector");
			}
			return _resourceRedirector;
		}
		set
		{
			_resourceRedirector = value ?? throw new ArgumentNullException("value");
		}
	}

	public string Source { get; set; }

	internal static XuaLogger CreateLogger(string source)
	{
		try
		{
			return new ModLoaderSpecificLogger(source);
		}
		catch (Exception)
		{
			return new ConsoleLogger(source);
		}
	}

	public XuaLogger(string source)
	{
		Source = source;
	}

	public void Error(Exception e, string message)
	{
		Log(LogLevel.Error, message + Environment.NewLine + e);
	}

	public void Error(string message)
	{
		Log(LogLevel.Error, message);
	}

	public void Warn(Exception e, string message)
	{
		Log(LogLevel.Warn, message + Environment.NewLine + e);
	}

	public void Warn(string message)
	{
		Log(LogLevel.Warn, message);
	}

	public void Info(Exception e, string message)
	{
		Log(LogLevel.Info, message + Environment.NewLine + e);
	}

	public void Info(string message)
	{
		Log(LogLevel.Info, message);
	}

	public void Debug(Exception e, string message)
	{
		Log(LogLevel.Debug, message + Environment.NewLine + e);
	}

	public void Debug(string message)
	{
		Log(LogLevel.Debug, message);
	}

	protected abstract void Log(LogLevel level, string message);

	protected string GetDefaultPrefix(LogLevel level)
	{
		return level switch
		{
			LogLevel.Debug => "[DEBUG][" + Source + "]: ", 
			LogLevel.Info => "[INFO][" + Source + "]: ", 
			LogLevel.Warn => "[WARN][" + Source + "]: ", 
			LogLevel.Error => "[ERROR][" + Source + "]: ", 
			_ => "[UNKNOW][" + Source + "]: ", 
		};
	}
}
