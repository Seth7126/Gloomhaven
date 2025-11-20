using System;
using WebSocketSharp.Net.WebSockets;

namespace WebSocketSharp.Server;

public abstract class WebSocketServiceHost
{
	internal ServerState State => Sessions.State;

	public abstract bool KeepClean { get; set; }

	public abstract string Path { get; }

	public abstract WebSocketSessionManager Sessions { get; }

	public abstract Type Type { get; }

	public abstract TimeSpan WaitTime { get; set; }

	internal void Start()
	{
		Sessions.Start();
	}

	internal void StartSession(WebSocketContext context)
	{
		CreateSession().Start(context, Sessions);
	}

	internal void Stop(ushort code, string reason)
	{
		CloseEventArgs e = new CloseEventArgs(code, reason);
		bool flag = !code.IsReserved();
		byte[] frameAsBytes = (flag ? WebSocketFrame.CreateCloseFrame(e.PayloadData, mask: false).ToArray() : null);
		Sessions.Stop(e, frameAsBytes, flag);
	}

	protected abstract WebSocketBehavior CreateSession();
}
internal class WebSocketServiceHost<TBehavior> : WebSocketServiceHost where TBehavior : WebSocketBehavior
{
	private Func<TBehavior> _initializer;

	private Logger _logger;

	private string _path;

	private WebSocketSessionManager _sessions;

	public override bool KeepClean
	{
		get
		{
			return _sessions.KeepClean;
		}
		set
		{
			string text = _sessions.State.CheckIfAvailable(ready: true, start: false, shutting: false);
			if (text != null)
			{
				_logger.Error(text);
			}
			else
			{
				_sessions.KeepClean = value;
			}
		}
	}

	public override string Path => _path;

	public override WebSocketSessionManager Sessions => _sessions;

	public override Type Type => typeof(TBehavior);

	public override TimeSpan WaitTime
	{
		get
		{
			return _sessions.WaitTime;
		}
		set
		{
			string text = _sessions.State.CheckIfAvailable(ready: true, start: false, shutting: false) ?? value.CheckIfValidWaitTime();
			if (text != null)
			{
				_logger.Error(text);
			}
			else
			{
				_sessions.WaitTime = value;
			}
		}
	}

	internal WebSocketServiceHost(string path, Func<TBehavior> initializer, Logger logger)
	{
		_path = path;
		_initializer = initializer;
		_logger = logger;
		_sessions = new WebSocketSessionManager(logger);
	}

	protected override WebSocketBehavior CreateSession()
	{
		return _initializer();
	}
}
