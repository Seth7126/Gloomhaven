using System;
using System.Collections.Generic;

public class RequestCounter
{
	private HashSet<object> _requests = new HashSet<object>();

	private readonly Action _onFirstIn;

	private readonly Action _onEmpty;

	public bool Any => _requests.Count > 0;

	public bool Empty => _requests.Count <= 0;

	public RequestCounter(Action onFirstIn = null, Action onEmpty = null)
	{
		_onFirstIn = onFirstIn;
		_onEmpty = onEmpty;
	}

	public void Add(object requester)
	{
		if (!_requests.Contains(requester))
		{
			_requests.Add(requester);
			if (_requests.Count == 1)
			{
				_onFirstIn?.Invoke();
			}
		}
	}

	public void Remove(object requester)
	{
		if (_requests.Contains(requester))
		{
			_requests.Remove(requester);
			if (_requests.Count <= 0)
			{
				_onEmpty?.Invoke();
			}
		}
	}
}
