using System;
using System.Text;

namespace Manatee.Trello.Internal.Logging;

internal class DebugLog : ILog
{
	public void Debug(string message, params object[] parameters)
	{
		Post("Debug: " + message);
	}

	public void Info(string message, params object[] parameters)
	{
		Post(string.Format("Info: " + message, parameters));
	}

	public void Error(Exception e)
	{
		Post(BuildMessage("Error: An exception of type " + e.GetType().Name + " occurred:", e.Message, e.StackTrace));
	}

	private static string BuildMessage(params string[] lines)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string value in lines)
		{
			stringBuilder.AppendLine(value);
		}
		return stringBuilder.ToString();
	}

	private static void Post(string output)
	{
	}
}
