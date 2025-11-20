#define ENABLE_LOGS
using System.Text.RegularExpressions;
using SM.Utils;
using SharedLibrary.Logger;
using Utilities;

namespace SharedLibrary.SimpleLog;

public static class SimpleLog
{
	private static string _simpleLogPath;

	private static object _lock = new object();

	public static void Initialize(IFileService fileService)
	{
	}

	public static void OnFFSNetConsoleLinePrinted(string text, bool addToSimpleLog)
	{
		if (addToSimpleLog)
		{
			AddToSimpleLog(text);
		}
	}

	public static void AddToSimpleLog(string addString, bool sendToClientLog = true)
	{
		LogUtils.Log(Regex.Replace(addString, "<.*?>", string.Empty));
		if (sendToClientLog)
		{
			lock (_lock)
			{
				DLLDebug.LogFromSimpleLog(addString);
			}
		}
	}

	public static void WriteSimpleLogToFile()
	{
	}
}
