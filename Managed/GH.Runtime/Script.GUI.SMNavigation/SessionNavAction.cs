using System.Collections.Generic;
using SM.Gamepad;

namespace Script.GUI.SMNavigation;

public abstract class SessionNavAction : NavigationAction
{
	private readonly Dictionary<object, ISession> _sessions = new Dictionary<object, ISession>();

	public override void HandleAction(NavigationActionArgs args)
	{
		if (!(args.NavigationElement == null))
		{
			ProcessSession(GetSessionKey(args.NavigationElement), args);
		}
	}

	private void ProcessSession(object sessionKey, NavigationActionArgs args)
	{
		ISession session;
		if (_sessions.TryGetValue(sessionKey, out var value))
		{
			EndSession(sessionKey, value);
		}
		else if (TryInitSession(args, out session))
		{
			StartSession(sessionKey, session);
		}
	}

	protected abstract bool TryInitSession(NavigationActionArgs args, out ISession session);

	private void StartSession(object sessionKey, ISession session)
	{
		_sessions.Add(sessionKey, session);
		session.Enter();
	}

	private void EndSession(object sessionKey, ISession session)
	{
		session.Exit();
		_sessions.Remove(sessionKey);
	}

	private object GetSessionKey(IUiNavigationElement element)
	{
		return element;
	}
}
