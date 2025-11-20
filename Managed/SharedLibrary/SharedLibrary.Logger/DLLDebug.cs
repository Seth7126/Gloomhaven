using System;
using SharedLibrary.Client;

namespace SharedLibrary.Logger;

public static class DLLDebug
{
	public class CDLLDebugLog_MessageData
	{
		public string m_Message;

		public string m_Stack;

		public EDLLDebug m_LogType;

		public bool m_ShowError;
	}

	public static void Log(string messageText)
	{
		SendMessage(messageText, EDLLDebug.Info);
	}

	public static void LogInfo(string messageText)
	{
		SendMessage(messageText, EDLLDebug.Info);
	}

	public static void LogWarning(string messageText)
	{
		SendMessage(messageText, EDLLDebug.Warning);
	}

	public static void LogError(string messageText, bool showError = true)
	{
		SendMessage(messageText, EDLLDebug.Error, showError);
	}

	public static void LogFromSimpleLog(string messageText)
	{
		SendMessage(messageText, EDLLDebug.SimpleLog);
	}

	private static void SendMessage(string messageText, EDLLDebug logType, bool showError = true)
	{
		string text = "[DLL Log] ";
		switch (logType)
		{
		case EDLLDebug.Warning:
			text += "Warning: ";
			break;
		case EDLLDebug.Error:
			text += "Error: ";
			break;
		case EDLLDebug.SimpleLog:
			text += "[SIMPLE]: ";
			break;
		}
		if (SharedClient.DLLDebugHandler != null)
		{
			CDLLDebugLog_MessageData message = new CDLLDebugLog_MessageData
			{
				m_Message = text + messageText,
				m_Stack = ((logType != EDLLDebug.SimpleLog) ? Environment.StackTrace : ""),
				m_LogType = logType,
				m_ShowError = showError
			};
			SharedClient.DLLDebugHandler(message);
		}
		else
		{
			Console.WriteLine(text + messageText + "\n" + ((logType != EDLLDebug.Info) ? Environment.StackTrace : ""));
		}
	}
}
