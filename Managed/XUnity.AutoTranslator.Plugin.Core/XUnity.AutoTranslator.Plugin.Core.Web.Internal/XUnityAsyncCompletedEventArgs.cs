using System;
using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core.Web.Internal;

public class XUnityAsyncCompletedEventArgs : EventArgs
{
	private bool _cancelled;

	private Exception _error;

	private object _userState;

	public bool Cancelled => _cancelled;

	public Exception Error => _error;

	public object UserState => _userState;

	public XUnityAsyncCompletedEventArgs(Exception error, bool cancelled, object userState)
	{
		_error = error;
		_cancelled = cancelled;
		_userState = userState;
	}

	protected void RaiseExceptionIfNecessary()
	{
		if (_error != null)
		{
			throw new TargetInvocationException(_error);
		}
		if (_cancelled)
		{
			throw new InvalidOperationException("The operation was cancelled");
		}
	}
}
