using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Hydra.Api.Errors;
using Hydra.Api.Push;
using Hydra.Sdk.Collections;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;
using Hydra.Sdk.Logs;

namespace Hydra.Sdk.Communication.WebSockets;

public class WebSocketClient
{
	public delegate void MessageReceivedDelegate(WebSocketMessage msg);

	public delegate void OnErrorDelegate(HydraSdkException ex);

	private static PushMessageType[] _validatedVersions = new PushMessageType[4]
	{
		PushMessageType.EconomyUserTransactionsUpdate,
		PushMessageType.PresencePartyUpdate,
		PushMessageType.PresenceSessionUpdate,
		PushMessageType.PresenceUserUpdate
	};

	private Uri _uri;

	private IHydraSdkLogger _logger;

	private ClientWebSocket _ws;

	private CancellationTokenSource _connectToken;

	private CancellationTokenSource _receiveToken;

	private Task _wsTaskConnect;

	private Task _wsTaskReceive;

	private Task _wsTaskDequeue;

	public MessageReceivedDelegate OnMessageReceived;

	public OnErrorDelegate OnError;

	private const int WsStatusCodeAppUsage = 4000;

	private AsyncItemQueue<WebSocketMessage> _updates;

	private readonly Dictionary<PushMessageType, int> _versions;

	public bool IsConnected { get; internal set; }

	public PushToken Token { get; private set; }

	public WebSocketState State => _ws.State;

	public WebSocketClient(Uri uri, IHydraSdkLogger logger)
	{
		_uri = uri;
		_logger = logger;
		_updates = new AsyncItemQueue<WebSocketMessage>();
		_versions = new Dictionary<PushMessageType, int>();
	}

	public IEnumerable<PushVersion> GetVersions()
	{
		return _versions.Select((KeyValuePair<PushMessageType, int> v) => new PushVersion
		{
			VersionType = v.Key,
			Version = v.Value
		});
	}

	public async Task Start(PushAuthorizationData data)
	{
		_ws = new ClientWebSocket();
		byte[] buf = new byte[data.CalculateSize()];
		CodedOutputStream st = new CodedOutputStream(buf);
		data.WriteTo(st);
		string hdr = Convert.ToBase64String(buf);
		_ws.Options.SetRequestHeader("WSDATA2", hdr);
		await _ws.ConnectAsync(_uri, CancellationToken.None);
		_wsTaskConnect = Task.Run((Func<Task>)CreateConnection);
		await _wsTaskConnect;
		IsConnected = true;
	}

	private async Task CreateConnection()
	{
		_connectToken = new CancellationTokenSource();
		byte[] data = await ReceiveMessage(_connectToken.Token);
		CheckCloseStatus();
		if (data != null && data.Length != 0)
		{
			Token = PushToken.Parser.ParseFrom(data);
			_connectToken = null;
			_wsTaskConnect = null;
			_receiveToken = new CancellationTokenSource();
			_wsTaskReceive = Task.Run((Func<Task>)Receive, _receiveToken.Token);
			_wsTaskDequeue = Task.Run((Func<Task>)ProcessingUpdates, _receiveToken.Token);
		}
		_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "Connected");
	}

	private async Task ProcessingUpdates()
	{
		try
		{
			while (_receiveToken != null && !_receiveToken.Token.IsCancellationRequested)
			{
				WebSocketMessage message = await _updates.DequeueAsync();
				OnMessageReceived?.Invoke(message);
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger.Log(HydraLogType.Error, this.GetLogCatErr(), ex2.GetErrorMessage());
		}
	}

	private async Task Receive()
	{
		byte[] buffer = new byte[255];
		new ArraySegment<byte>(buffer);
		new List<byte>();
		while (!_receiveToken.IsCancellationRequested)
		{
			try
			{
				byte[] data = await ReceiveMessage(_receiveToken.Token);
				CheckCloseStatus();
				if (data != null && data.Length != 0)
				{
					using MemoryStream stream = new MemoryStream(data);
					BinaryReader reader = new BinaryReader(stream);
					int headerSize = reader.ReadInt32();
					PushMessageHeader header = PushMessageHeader.Parser.ParseFrom(data, 4, headerSize);
					_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "Enqueued {0}, version: {1}", header.MessageType, header.Version);
					CheckVersion(header.MessageType, header.Version);
					await _updates.Enqueue(new WebSocketMessage
					{
						Type = header.MessageType,
						Data = data,
						Offset = 4 + headerSize,
						Lenght = data.Length - (4 + headerSize)
					});
				}
				if (_receiveToken.Token.IsCancellationRequested)
				{
					break;
				}
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception ex2)
			{
				_logger.Log(HydraLogType.Error, this.GetLogCatErr(), ex2.GetErrorMessage());
				OnError?.Invoke(new HydraSdkException(ErrorCode.SdkParseError, "Push error received", ex2));
				break;
			}
		}
	}

	private void CheckCloseStatus()
	{
		if (_ws.CloseStatus.HasValue && _ws.CloseStatus >= (WebSocketCloseStatus)4000)
		{
			OnError?.Invoke(new HydraSdkException((ErrorCode)(_ws.CloseStatus - 4000).Value, _ws.CloseStatusDescription));
		}
	}

	public async Task Stop()
	{
		if (!IsConnected)
		{
			return;
		}
		try
		{
			if (_connectToken != null)
			{
				_connectToken.Cancel(throwOnFirstException: false);
				await _wsTaskConnect;
			}
			if (_receiveToken != null)
			{
				_receiveToken.Cancel(throwOnFirstException: false);
				_ws.Abort();
			}
		}
		finally
		{
			IsConnected = false;
		}
		_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "Disconnected");
	}

	public Task SendMessage(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
	{
		return _ws.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
	}

	private async Task<byte[]> ReceiveMessage(CancellationToken token)
	{
		byte[] buffer = new byte[255];
		ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
		List<byte> msgbuffer = new List<byte>();
		while (!token.IsCancellationRequested && _ws.State == WebSocketState.Open)
		{
			WebSocketReceiveResult result;
			try
			{
				result = await _ws.ReceiveAsync(segment, token);
			}
			catch (IOException err)
			{
				_logger.Log(HydraLogType.Error, this.GetLogCatErr(), err.GetErrorMessage());
				break;
			}
			catch (TaskCanceledException)
			{
				_logger.Log(HydraLogType.Information, this.GetLogCatMsg(), "Cancelling WebSocket task...");
				break;
			}
			if (result.Count > 0)
			{
				msgbuffer.AddRange(segment.Take(result.Count));
			}
			if (result.EndOfMessage)
			{
				return msgbuffer.ToArray();
			}
			if (_connectToken != null && _connectToken.Token.IsCancellationRequested)
			{
				break;
			}
		}
		_logger.Log(HydraLogType.Message, this.GetLogCatMsg(), "receive ended, ws state = " + _ws.State);
		return null;
	}

	private void CheckVersion(PushMessageType type, int version)
	{
		if (_validatedVersions.Contains(type))
		{
			if (!_versions.ContainsKey(type))
			{
				_versions.Add(type, 0);
			}
			if (version != _versions[type] + 1)
			{
				_logger.Log(HydraLogType.Error, this.GetLogCatErr(), "Push version mismatch error! Type: {0}, version: {1}", type, version);
				throw new HydraSdkException(ErrorCode.SdkInternalError, "Invalid push version");
			}
			_versions[type] = version;
		}
	}
}
