using System;

namespace Manatee.Trello;

public interface ILog
{
	void Debug(string message, params object[] parameters);

	void Info(string message, params object[] parameters);

	void Error(Exception e);
}
