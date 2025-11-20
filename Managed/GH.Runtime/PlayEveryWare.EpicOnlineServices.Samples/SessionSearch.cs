using System;
using System.Collections.Generic;
using Epic.OnlineServices.Sessions;

namespace PlayEveryWare.EpicOnlineServices.Samples;

public class SessionSearch
{
	public Epic.OnlineServices.Sessions.SessionSearch SearchHandle;

	private Dictionary<Session, SessionDetails> SearchResults = new Dictionary<Session, SessionDetails>();

	public SessionSearch()
	{
		Release();
	}

	public void Release()
	{
		SearchResults.Clear();
		if (SearchHandle != null)
		{
			SearchHandle.Release();
			SearchHandle = null;
		}
	}

	public void SetNewSearch(Epic.OnlineServices.Sessions.SessionSearch handle)
	{
		Release();
		SearchHandle = handle;
	}

	public Epic.OnlineServices.Sessions.SessionSearch GetSearchHandle()
	{
		return SearchHandle;
	}

	public Dictionary<Session, SessionDetails> GetResults()
	{
		return SearchResults;
	}

	public SessionDetails GetSessionHandleById(string sessionId)
	{
		foreach (KeyValuePair<Session, SessionDetails> searchResult in SearchResults)
		{
			if (searchResult.Key.Id.Equals(sessionId, StringComparison.OrdinalIgnoreCase))
			{
				return searchResult.Value;
			}
		}
		return null;
	}

	public void OnSearchResultReceived(Dictionary<Session, SessionDetails> results)
	{
		SearchResults = results;
	}
}
