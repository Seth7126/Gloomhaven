using System;

namespace Manatee.Trello;

public class TrelloInteractionException : Exception
{
	public TrelloInteractionException(Exception innerException)
		: base("Trello has reported an error with the request.", innerException)
	{
	}

	public TrelloInteractionException(string message, Exception innerException = null)
		: base("Trello has reported an error with the request: " + message, innerException)
	{
	}
}
