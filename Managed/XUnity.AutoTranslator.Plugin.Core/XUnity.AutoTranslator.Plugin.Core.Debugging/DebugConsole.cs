using System;
using System.IO;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Debugging;

internal static class DebugConsole
{
	private static IntPtr _consoleOut;

	public static void Enable()
	{
		if (Settings.EnableConsole)
		{
			TryEnableConsole();
		}
	}

	private static bool TryEnableConsole()
	{
		try
		{
			Kernel32.GetStdHandle(-11);
			if (!Kernel32.AllocConsole())
			{
				return false;
			}
			_consoleOut = Kernel32.CreateFile("CONOUT$", 1073741824, 2, IntPtr.Zero, 3, 0, IntPtr.Zero);
			if (!Kernel32.SetStdHandle(-11, _consoleOut))
			{
				return false;
			}
			StreamWriter obj = new StreamWriter(Console.OpenStandardOutput(), Encoding.Default)
			{
				AutoFlush = true
			};
			Console.SetOut(obj);
			Console.SetError(obj);
			Kernel32.SetConsoleOutputCP(932u);
			Console.OutputEncoding = ConsoleEncoding.GetEncoding(932u);
			return true;
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred during while enabling console.");
		}
		return false;
	}
}
