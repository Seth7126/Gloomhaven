#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UdpKit.Async;
using UdpKit.Platform;
using UdpKit.Protocol;
using UdpKit.Security;

namespace UdpKit;

[Obfuscation(Exclude = false, Feature = "rename", ApplyToMembers = true)]
public class UdpSocket
{
	internal class BroadcastManager
	{
		private readonly UdpSocket udpSocket;

		private ProtocolService service;

		private UdpEndPoint broadcast;

		public bool IsEnabled => service != null && service.Client != null && service.Client.Socket != null && service.Client.Socket.IsBound;

		public BroadcastManager(UdpSocket s)
		{
			udpSocket = s;
		}

		public void Update(uint now)
		{
			if (!IsEnabled)
			{
				return;
			}
			if (service.Client.Socket.RecvPoll(0))
			{
				UdpEndPoint endpoint = default(UdpEndPoint);
				int num = service.Client.Socket.RecvFrom(service.Client.Buffer, ref endpoint);
				if (num > 0)
				{
					service.Client.Recv(endpoint, service.Client.Buffer, 0);
				}
			}
			if (udpSocket.Mode == UdpSocketMode.Client && service.SendTime + udpSocket.Config.BroadcastInterval < now)
			{
				service.Send<BroadcastSearch>(broadcast);
			}
		}

		public void Enable(UdpEventLanBroadcastEnable args)
		{
			if (!udpSocket.platform.SupportsBroadcast)
			{
				UdpLog.Error("Current platform: {0}, does not support broadcasting", udpSocket.platform.GetType().Name);
				return;
			}
			if (IsEnabled)
			{
				UdpLog.Warn("Enable: Closing early Broadcast Socket");
				service.Client.Socket.Close();
			}
			broadcast = new UdpEndPoint(args.BroadcastAddress, args.Port);
			UdpEndPoint udpEndPoint = new UdpEndPoint(args.LocalAddress, args.Port);
			bool flag = udpSocket.Mode == UdpSocketMode.Client;
			UdpLog.Info("Creating Client with PeerId: {0}", udpSocket.PeerId);
			UdpLog.Info("Broadcast Address: {0}", broadcast);
			UdpLog.Info("Broadcast socket EndPoint: {0}", udpEndPoint);
			UdpPlatformSocket socket = udpSocket.platform.CreateBroadcastSocket(udpEndPoint, !flag);
			ProtocolClient p = new ProtocolClient(socket, udpSocket.GameId, udpSocket.PeerId);
			service = new ProtocolService(p);
			service.Client.SetHandler<BroadcastSearch>(OnBroadcastSearch);
			service.Client.SetHandler<BroadcastSession>(OnBroadcastSession);
			UdpLog.Info("Lan Broadcast: enabled");
		}

		private void OnBroadcastSearch(BroadcastSearch search)
		{
			if (search.PeerId != udpSocket.PeerId)
			{
				UdpLog.Warn("Received BroadcastSearch from {0}...", search.PeerId);
				BroadcastSession broadcastSession = service.Client.CreateMessage<BroadcastSession>();
				broadcastSession.Host = udpSocket.sessionManager.GetLocalSession();
				broadcastSession.Port = udpSocket.platformSocket.EndPoint.Port;
				service.Send(search.Sender, broadcastSession);
			}
		}

		private void OnBroadcastSession(BroadcastSession session)
		{
			if (session.PeerId != udpSocket.PeerId)
			{
				UdpLog.Warn("Received BroadcastSession...");
				UdpIPv4Address address = session.Sender.Address;
				int port = session.Port;
				(session.Host as UdpSessionImpl)._lanEndPoint = new UdpEndPoint(address, (ushort)port);
				UdpSessionSource source = (session.Host as UdpSessionImpl).Source;
				udpSocket.sessionManager.UpdateSession(session.Host, source);
			}
		}

		public void Disable()
		{
			if (IsEnabled)
			{
				if (service.Client.Platform.GetSessionSource() != UdpSessionSource.Photon)
				{
					UdpLog.Warn("Disable: Closing early Broadcast Socket of type {0}", service.Client.Platform.GetSessionSource());
					service.Client.Socket.Close();
				}
				UdpLog.Info("Lan Broadcast: disabled");
			}
			service = null;
			broadcast = default(UdpEndPoint);
		}
	}

	internal class SessionManager
	{
		private const uint UPDATE_DELAY = 500u;

		private uint nextUpdate;

		private Map<Guid, UdpSession> sessions;

		private readonly UdpSession local;

		private readonly Stopwatch timer;

		private UdpSocket UdpSocket { get; set; }

		public bool IsHostWithName => UdpSocket.mode == UdpSocketMode.Host && local.HostName.HasValue();

		public SessionManager(UdpSocket udpSocket)
		{
			UdpSocket = udpSocket;
			sessions = new Map<Guid, UdpSession>();
			timer = new Stopwatch();
			timer.Start();
			local = new UdpSessionImpl
			{
				_id = UdpSocket.PeerId,
				_source = UdpSocket.platform.GetSessionSource()
			};
		}

		public void Update(uint now)
		{
			if (timer.ElapsedMilliseconds > 1000 && UdpSocket.platform.SessionListHasChanged())
			{
				SetSessions(UdpSocket.platform.GetSessionList());
				timer.Reset();
				timer.Start();
			}
			if (nextUpdate != 0 && nextUpdate < now)
			{
				RaiseSessionUpdatedEvent();
			}
		}

		public UdpSession GetLocalSession()
		{
			return UdpSocket.platform.GetCurrentSession() ?? local.Clone();
		}

		public void SetHostInfo(UdpEventSessionSetHostData sessionInfo)
		{
			local.HostName = sessionInfo.Name;
			local.HostData = sessionInfo.Token;
			local.HostObject = sessionInfo.TokenObject;
			local.IsDedicatedServer = sessionInfo.Dedicated;
			if (IsHostWithName && !UdpSocket.platform.HandleSetHostInfo(local, RaiseSessionCreatedEvent))
			{
				RaiseSessionCreatedEvent(success: false, UdpSessionError.Error);
			}
		}

		public void UpdateSession(UdpSession session, UdpSessionSource source = UdpSessionSource.None)
		{
			UdpLog.Info("UpdateSession: {0} [{1}]", session.HostName, source);
			if (source != UdpSessionSource.None)
			{
				(session as UdpSessionImpl)._source = source;
			}
			sessions = sessions.Update(session.Id, session);
			if (nextUpdate == 0)
			{
				nextUpdate = UdpSocket.GetCurrentTime() + 500;
			}
		}

		public void SetSessions(List<UdpSession> sessionsList)
		{
			if (sessionsList == null)
			{
				return;
			}
			Map<Guid, UdpSession> map = new Map<Guid, UdpSession>();
			foreach (UdpSession sessions in sessionsList)
			{
				map = map.Add(sessions.Id, sessions);
			}
			this.sessions = map;
			if (nextUpdate == 0)
			{
				nextUpdate = UdpSocket.GetCurrentTime() + 500;
			}
		}

		public void ForgetSessionsAll()
		{
			sessions = new Map<Guid, UdpSession>();
			RaiseSessionUpdatedEvent();
		}

		public void ForgetSessions(UdpSessionSource source)
		{
			Map<Guid, UdpSession> map = sessions;
			foreach (KeyValuePair<Guid, UdpSession> session in sessions)
			{
				if (session.Value.Source == source)
				{
					map = map.Remove(session.Key);
				}
			}
			sessions = map;
			RaiseSessionUpdatedEvent();
		}

		public void SetWanEndPoint(UdpEndPoint endpoint)
		{
			local.WanEndPoint = endpoint;
		}

		public void SetLanEndPoint(UdpEndPoint endpoint)
		{
			local.LanEndPoint = endpoint;
		}

		public void SetConnections(int current, int max)
		{
			local.ConnectionsMax = max;
			local.ConnectionsCurrent = current;
		}

		private void RaiseSessionCreatedEvent(bool success, UdpSessionError error)
		{
			UdpSocket.Raise(new UdpEventSessionCreated
			{
				Session = GetLocalSession(),
				Success = success,
				Error = error
			});
		}

		private void RaiseSessionUpdatedEvent()
		{
			try
			{
				UdpSocket.Raise(new UdpEventSessionListUpdated
				{
					SessionList = sessions
				});
			}
			finally
			{
				nextUpdate = 0u;
			}
		}
	}

	private struct DelayedPacket
	{
		public UdpEndPoint EndPoint;

		public byte[] Data;

		public int Size;

		public uint Time;
	}

	internal class Broadcaster
	{
		private readonly UdpSocket _udpSocket;

		private bool _isClient;

		private IPEndPoint _broadcastEndPoint;

		private UdpClient _socket;

		private uint _lastSend;

		private Context _context;

		private Thread _backgroundThread;

		private byte[] _bufferBroadcastSearch = null;

		private byte[] _bufferBroadcastSession = null;

		private bool IsEnabled => _socket != null;

		private byte[] BroadcastSearchMsg
		{
			get
			{
				if (_bufferBroadcastSearch == null)
				{
					_bufferBroadcastSearch = new byte[1024];
					_context.WriteMessage(_context.CreateMessage<BroadcastSearch>(), _bufferBroadcastSearch);
				}
				return _bufferBroadcastSearch;
			}
		}

		private byte[] BroadcastSessionMsg
		{
			get
			{
				if (_bufferBroadcastSession == null)
				{
					_bufferBroadcastSession = new byte[1024];
					BroadcastSession broadcastSession = _context.CreateMessage<BroadcastSession>();
					broadcastSession.Host = _udpSocket.sessionManager.GetLocalSession();
					broadcastSession.Port = _udpSocket.platformSocket.EndPoint.Port;
					_context.WriteMessage(broadcastSession, _bufferBroadcastSession);
				}
				return _bufferBroadcastSession;
			}
		}

		public Broadcaster(UdpSocket udpSocket)
		{
			_udpSocket = udpSocket;
		}

		public void Update(uint now)
		{
			if (IsEnabled && _isClient && _lastSend + _udpSocket.Config.BroadcastInterval < now)
			{
				UdpLog.Debug("Sending Broadcast Search");
				Send(BroadcastSearchMsg, _broadcastEndPoint);
				_lastSend = _udpSocket.platform.GetPrecisionTime();
			}
		}

		public void Enable(UdpEventLanBroadcastEnable args)
		{
			if (!IsEnabled)
			{
				_broadcastEndPoint = new IPEndPoint(IPAddress.Parse(args.BroadcastAddress.ToString()), args.Port);
				_isClient = _udpSocket.Mode == UdpSocketMode.Client;
				_socket = (_isClient ? new UdpClient(AddressFamily.InterNetwork) : new UdpClient(args.Port, AddressFamily.InterNetwork));
				_socket.EnableBroadcast = true;
				UdpLog.Info("Creating Client with PeerId: {0}", _udpSocket.PeerId);
				UdpLog.Info("Broadcast Address: {0}", _broadcastEndPoint);
				_context = new Context(_udpSocket.GameId, _udpSocket.PeerId);
				UdpLog.Debug("Enabling Broadcast");
				StartReceiveThread();
			}
		}

		private void StartReceiveThread()
		{
			if (_backgroundThread != null)
			{
				_backgroundThread.Abort();
				_backgroundThread = null;
			}
			_backgroundThread = new Thread((ThreadStart)delegate
			{
				try
				{
					IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
					while (true)
					{
						try
						{
							ProcessMessage(remoteEP, _socket.Receive(ref remoteEP));
						}
						catch (ThreadAbortException)
						{
							break;
						}
						catch (Exception)
						{
						}
						Thread.Sleep(1000);
					}
				}
				catch (ThreadAbortException)
				{
				}
			})
			{
				IsBackground = true,
				Priority = ThreadPriority.BelowNormal
			};
			_backgroundThread.Start();
			UdpLog.Debug("Broadcast enabled");
		}

		public void Disable()
		{
			try
			{
				if (IsEnabled)
				{
					UdpLog.Debug("Disabling Broadcast");
					if (_backgroundThread != null)
					{
						_backgroundThread.Abort();
						_backgroundThread = null;
					}
					_socket.Close();
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				_socket = null;
				_backgroundThread = null;
				UdpLog.Debug("Broadcast disabled");
			}
		}

		private void ProcessMessage(IPEndPoint endpoint, byte[] message)
		{
			int offset = 0;
			Message message2 = _context.ParseMessage(message, ref offset);
			message2.Sender = UdpEndPoint.Parse(endpoint.ToString());
			Message message3 = message2;
			Message message4 = message3;
			if (!(message4 is BroadcastSearch search))
			{
				if (message4 is BroadcastSession session)
				{
					ProcessBroadcastSession(session);
				}
			}
			else
			{
				ProcessBroadcastSearch(search, endpoint);
			}
		}

		private void ProcessBroadcastSession(BroadcastSession session)
		{
			if (session.PeerId != _udpSocket.PeerId)
			{
				UdpLog.Warn("Received BroadcastSession...");
				UdpIPv4Address address = session.Sender.Address;
				int port = session.Port;
				(session.Host as UdpSessionImpl)._lanEndPoint = new UdpEndPoint(address, (ushort)port);
				UdpSessionSource source = (session.Host as UdpSessionImpl).Source;
				_udpSocket.sessionManager.UpdateSession(session.Host, source);
			}
		}

		private void ProcessBroadcastSearch(BroadcastSearch search, IPEndPoint sender)
		{
			if (search.PeerId != _udpSocket.PeerId)
			{
				UdpLog.Warn("Received BroadcastSearch from {0}...", search.PeerId);
				Send(BroadcastSessionMsg, sender);
			}
		}

		private void Send(byte[] data, IPEndPoint destination)
		{
			_socket.Send(data, data.Length, destination);
		}
	}

	private readonly Queue<byte[]> delayedBuffers = new Queue<byte[]>();

	private readonly Queue<DelayedPacket> delayedPackets = new Queue<DelayedPacket>();

	public static bool AllowConnectionRecycle;

	internal readonly UdpConfig Config;

	internal readonly UdpPipeConfig PacketPipeConfig;

	internal readonly UdpPipeConfig StreamPipeConfig;

	internal UdpEndPoint LANEndPoint;

	internal UdpEndPoint WANEndPoint;

	internal readonly Guid PeerId;

	internal readonly Guid GameId;

	private volatile int frame;

	private volatile int channelIdCounter;

	private volatile uint connectionIdCounter;

	private volatile UdpSocketMode mode;

	private volatile UdpSocketState state;

	private readonly byte[] sendBuffer;

	private readonly byte[] recvBuffer;

	private readonly UdpPlatform platform;

	private readonly UdpPlatformSocket platformSocket;

	private readonly UdpPacketPool packetPool;

	internal SessionManager sessionManager;

	internal BroadcastManager broadcastManager;

	internal Broadcaster broadcaster;

	private readonly Queue<UdpEvent> eventQueueIn;

	private readonly Queue<UdpEvent> eventQueueOut;

	private readonly List<UdpConnection> connectionList = new List<UdpConnection>();

	private readonly Dictionary<UdpEndPoint, byte[]> pendingConnections = new Dictionary<UdpEndPoint, byte[]>(new UdpEndPoint.Comparer());

	private readonly Dictionary<UdpEndPoint, UdpConnection> connectionLookup = new Dictionary<UdpEndPoint, UdpConnection>(new UdpEndPoint.Comparer());

	private readonly Dictionary<UdpChannelName, UdpStreamChannel> streamChannels = new Dictionary<UdpChannelName, UdpStreamChannel>(UdpChannelName.EqualityComparer.Instance);

	private bool ShouldDelayPacket => Config.SimulatedPingMin > 0 && Config.SimulatedPingMax > 0 && Config.SimulatedPingMin < Config.SimulatedPingMax;

	private bool ShouldDropPacket
	{
		get
		{
			float num = Config.NoiseFunction();
			if (num > 0f && Config.NoiseFunction() < Config.SimulatedLoss)
			{
				UdpLog.Debug("Dropping packet (Simulated)");
				return true;
			}
			return false;
		}
	}

	public UdpEndPoint SocketEndPoint => platformSocket.EndPoint;

	public UdpEndPoint LanEndPoint => LANEndPoint;

	public UdpEndPoint WanEndPoint => WANEndPoint;

	public UdpSocketState State => state;

	public UdpSocketMode Mode => mode;

	public Guid SocketPeerId => PeerId;

	public uint PrecisionTime => GetCurrentTime();

	public UdpPacketPool PacketPool => packetPool;

	public object UserToken { get; set; }

	public Func<int, byte[]> UnconnectedBufferProvider { get; set; }

	public UdpPlatformSocket PlatformSocket => platformSocket;

	internal void ProcessDataStream(uint now)
	{
		for (int i = 0; i < connectionList.Count; i++)
		{
			UdpConnection udpConnection = connectionList[i];
			UdpConnectionState udpConnectionState = udpConnection.State;
			UdpConnectionState udpConnectionState2 = udpConnectionState;
			if (udpConnectionState2 == UdpConnectionState.Connected)
			{
				udpConnection.ProcessStream(now);
			}
		}
	}

	internal UdpChannelName StreamChannelCreate(string name, UdpChannelMode mode, int priority)
	{
		UdpAssert.Assert(name != null);
		UdpAssert.Assert(priority > 0);
		UdpChannelConfig udpChannelConfig = new UdpChannelConfig
		{
			Mode = mode,
			Priority = priority,
			ChannelName = 
			{
				Name = name,
				Id = ++channelIdCounter
			}
		};
		Raise(new UdpEventStreamCreateChannel
		{
			ChannelConfig = udpChannelConfig
		});
		return udpChannelConfig.ChannelName;
	}

	private bool PollInternal(out UdpEvent ev)
	{
		lock (eventQueueIn)
		{
			if (eventQueueIn.Count > 0)
			{
				ev = eventQueueIn.Dequeue();
				return true;
			}
		}
		ev = default(UdpEvent);
		return false;
	}

	private void ProcessStartEvent()
	{
		UdpEvent ev;
		while (PollInternal(out ev))
		{
			switch (ev.Type)
			{
			case 1:
				OnEventStart(ev);
				return;
			case 25:
				OnEventStreamCreateChannel(ev.As<UdpEventStreamCreateChannel>());
				break;
			default:
				UdpLog.Error("Can not send event of type {0} before socket has started", ev.Type);
				break;
			}
		}
	}

	private void ProcessInternalEvents()
	{
		UdpEvent ev;
		while (PollInternal(out ev))
		{
			platform?.OnInternalEvent(ev);
			switch (ev.Type)
			{
			case 3:
				OnEventConnect(ev);
				break;
			case 5:
				OnEventConnectCancel(ev);
				break;
			case 7:
				OnEventAccept(ev);
				break;
			case 9:
				OnEventRefuse(ev);
				break;
			case 11:
				OnEventDisconnect(ev);
				break;
			case 13:
				OnEventClose(ev);
				return;
			case 15:
				OnEventSend(ev);
				break;
			case 17:
				OnEventLanBroadcastEnable(ev);
				break;
			case 19:
				OnEventLanBroadcastDisable();
				break;
			case 33:
				OnEventSessionHostSetInfo(ev);
				break;
			case 31:
				OnEventSessionConnect(ev);
				break;
			case 45:
				OnEventSessionConnectRandom(ev);
				break;
			case 27:
				OnEventStreamQueue(ev);
				break;
			case 29:
				OnEventStreamSetBandwidth(ev);
				break;
			case 1:
			case 25:
				UdpLog.Error("Can not send event of type {0} after the socket has started", ev.Type);
				break;
			default:
				UdpLog.Error("Unknown event type {0}", ev.Type);
				break;
			}
		}
	}

	private void OnEventStart(UdpEvent ev)
	{
		UdpEventStart start = ev.As<UdpEventStart>();
		if (CreatePhysicalSocket(start.EndPoint, UdpSocketState.Running))
		{
			mode = start.Mode;
			try
			{
				LANEndPoint = default(UdpEndPoint);
				if (!platform.IsNull)
				{
					List<UdpEndPoint> list = platform.ResolveHostAddresses(platformSocket.EndPoint.Port, Config.IPv6);
					bool flag = ((!start.EndPoint.IPv6) ? (start.EndPoint.Address.IsAny || start.EndPoint.Address.IsLocalHost) : (start.EndPoint.AddressIPv6.IsAny || start.EndPoint.AddressIPv6.IsLocalHost));
					foreach (UdpEndPoint item in list)
					{
						if (flag)
						{
							LANEndPoint = item;
							break;
						}
						if (start.EndPoint.IPv6 && item.IPv6 && start.EndPoint.AddressIPv6.Equals(item.AddressIPv6))
						{
							LANEndPoint = item;
							break;
						}
						if (!start.EndPoint.IPv6 && !item.IPv6 && start.EndPoint.Address.Equals(item.Address))
						{
							LANEndPoint = item;
							break;
						}
					}
					if (LANEndPoint == default(UdpEndPoint) && platform.SupportsMasterServer)
					{
						throw new Exception("No valid LAN EndPoint resolved.");
					}
				}
				sessionManager.SetLanEndPoint(LANEndPoint);
				UdpLog.Info("LAN endpoint resolved as {0}", LANEndPoint);
				platform.OnStartDone(LANEndPoint, doneCallback);
				return;
			}
			catch (Exception ex)
			{
				UdpLog.Error(ex.ToString());
				doneCallback(result: false, UdpConnectionDisconnectReason.Error);
				return;
			}
		}
		doneCallback(result: false, UdpConnectionDisconnectReason.Error);
		void doneCallback(bool result, UdpConnectionDisconnectReason disconnectReason)
		{
			if (result)
			{
				Raise(new UdpEventStartDone
				{
					EndPoint = platformSocket.EndPoint,
					ResetEvent = start.ResetEvent
				});
			}
			else
			{
				Raise(new UdpEventStartFailed
				{
					ResetEvent = start.ResetEvent,
					disconnectReason = disconnectReason
				});
			}
		}
	}

	private void OnEventConnect(UdpEvent ev)
	{
		UdpEventConnectEndPoint udpEventConnectEndPoint = ev.As<UdpEventConnectEndPoint>();
		ConnectToEndPoint(udpEventConnectEndPoint.EndPoint, udpEventConnectEndPoint.Token);
	}

	private void OnEventConnectCancel(UdpEvent ev)
	{
		UdpEventConnectEndPointCancel udpEventConnectEndPointCancel = ev.As<UdpEventConnectEndPointCancel>();
		if (!CheckState(UdpSocketState.Running) || !connectionLookup.TryGetValue(udpEventConnectEndPointCancel.EndPoint, out var value))
		{
			return;
		}
		if (value.CheckState(UdpConnectionState.Connecting))
		{
			if (!udpEventConnectEndPointCancel.InternalOnly)
			{
				Raise(new UdpEventConnectFailed
				{
					EndPoint = value.RemoteEndPoint,
					Token = value.ConnectToken
				});
			}
			value.ChangeState(UdpConnectionState.Destroy);
		}
		else if (value.CheckState(UdpConnectionState.Connected))
		{
			value.DisconnectReason = UdpConnectionDisconnectReason.Disconnected;
			value.SendCommand(4);
			value.ChangeState(UdpConnectionState.Disconnected);
		}
		UdpLog.Debug("Connection with EndPoint {0} was canceled", udpEventConnectEndPointCancel.EndPoint);
	}

	private void OnEventAccept(UdpEvent ev)
	{
		UdpEventAcceptConnect udpEventAcceptConnect = ev.As<UdpEventAcceptConnect>();
		if (pendingConnections.TryGetValue(udpEventAcceptConnect.EndPoint, out var value))
		{
			pendingConnections.Remove(udpEventAcceptConnect.EndPoint);
			AcceptConnection(udpEventAcceptConnect.EndPoint, udpEventAcceptConnect.UserObject, udpEventAcceptConnect.Token, value);
		}
	}

	private void OnEventRefuse(UdpEvent ev)
	{
		UdpEventRefuseConnect udpEventRefuseConnect = ev.As<UdpEventRefuseConnect>();
		if (pendingConnections.Remove(udpEventRefuseConnect.EndPoint))
		{
			SendCommand(udpEventRefuseConnect.EndPoint, 3, udpEventRefuseConnect.Token);
		}
	}

	private void OnEventDisconnect(UdpEvent ev)
	{
		UdpEventDisconnect udpEventDisconnect = ev.As<UdpEventDisconnect>();
		if (udpEventDisconnect.Connection.CheckState(UdpConnectionState.Connected))
		{
			udpEventDisconnect.Connection.DisconnectReason = udpEventDisconnect.DisconnectReason;
			udpEventDisconnect.Connection.SendCommand(4, udpEventDisconnect.Token);
			udpEventDisconnect.Connection.ChangeState(UdpConnectionState.Disconnected);
		}
	}

	private async void OnEventClose(UdpEvent ev)
	{
		UdpEventClose close = ev.As<UdpEventClose>();
		if (CheckState(UdpSocketState.Running))
		{
			foreach (UdpConnection c in connectionLookup.Values)
			{
				c.SendCommand(4);
				c.DisconnectReason = UdpConnectionDisconnectReason.Disconnected;
				c.ChangeState(UdpConnectionState.Disconnected);
			}
		}
		if (!ChangeState(UdpSocketState.Running, UdpSocketState.Shutdown))
		{
			return;
		}
		try
		{
			if (close.ResetEvent != null)
			{
				await System.Threading.Tasks.Task.Delay(1000);
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			platformSocket.Close();
			if (!string.IsNullOrEmpty(platformSocket.Error))
			{
				UdpLog.Error("Failed to shutdown socket, platform error: {0}", platformSocket.Error);
			}
			connectionList.Clear();
			connectionLookup.Clear();
			eventQueueIn.Clear();
			eventQueueOut.Clear();
			pendingConnections.Clear();
			if (close.ResetEvent != null)
			{
				close.ResetEvent.Set();
			}
		}
	}

	private void OnEventSend(UdpEvent ev)
	{
		((UdpConnection)ev.Object0).OnPacketSend((UdpPacket)ev.Object1);
	}

	private void OnEventSessionHostSetInfo(UdpEvent ev)
	{
		sessionManager.SetHostInfo(ev.As<UdpEventSessionSetHostData>());
	}

	private void OnEventSessionConnectRandom(UdpEvent ev)
	{
		UdpEventSessionConnectRandom connect = ev.As<UdpEventSessionConnectRandom>();
		if (platform.HandleConnectToRandomSession(connect.SessionFilter, connect.Token, handleResult))
		{
			UdpLog.Info("Connecting to a Random Session");
			return;
		}
		UdpLog.Error("Error while trying to connect to session or this platform ({0}) does not support connecting to a random session", platform);
		handleResult(result: false, UdpSessionError.Error);
		void handleResult(bool result, UdpSessionError error)
		{
			if (result)
			{
				Raise(new UdpEventSessionConnected
				{
					Session = platform.GetCurrentSession(),
					Token = connect.Token
				});
			}
			else
			{
				Raise(new UdpEventSessionConnectFailed
				{
					Session = null,
					Token = connect.Token,
					Error = error
				});
			}
		}
	}

	private void OnEventSessionConnect(UdpEvent ev)
	{
		UdpEventSessionConnect connect = ev.As<UdpEventSessionConnect>();
		if (connect.Session == null)
		{
			Raise(new UdpEventSessionConnectFailed
			{
				Session = null,
				Token = connect.Token,
				Error = UdpSessionError.Error
			});
			return;
		}
		switch (connect.Session.Source)
		{
		case UdpSessionSource.Photon:
			if (platform.GetSessionSource() == UdpSessionSource.Photon)
			{
				if (platform.HandleConnectToSession(connect.Session, connect.Token, handleResult))
				{
					UdpLog.Info("Connecting to a Photon Session");
					break;
				}
				UdpLog.Error("Error while trying to connect to the Photon Session");
				handleResult(result: false, UdpSessionError.Error);
			}
			break;
		case UdpSessionSource.Lan:
			UdpLog.Info("Connecting to a LAN Session");
			ConnectToEndPoint(connect.Session.LanEndPoint, connect.Token);
			break;
		}
		void handleResult(bool result, UdpSessionError error)
		{
			if (result)
			{
				Raise(new UdpEventSessionConnected
				{
					Session = connect.Session,
					Token = connect.Token
				});
			}
			else
			{
				Raise(new UdpEventSessionConnectFailed
				{
					Session = connect.Session,
					Token = connect.Token,
					Error = error
				});
			}
		}
	}

	private void OnEventLanBroadcastEnable(UdpEvent ev)
	{
		UdpEventLanBroadcastEnable udpEventLanBroadcastEnable = ev.As<UdpEventLanBroadcastEnable>();
		udpEventLanBroadcastEnable.LocalAddress = LANEndPoint.Address;
		udpEventLanBroadcastEnable.BroadcastAddress = platform.GetBroadcastAddress();
		broadcaster?.Enable(udpEventLanBroadcastEnable);
	}

	private void OnEventLanBroadcastDisable()
	{
		broadcaster?.Disable();
	}

	private void OnEventStreamSetBandwidth(UdpEvent ev)
	{
		UdpEventStreamSetBandwidth udpEventStreamSetBandwidth = ev.As<UdpEventStreamSetBandwidth>();
		udpEventStreamSetBandwidth.Connection.OnStreamSetBandwidth(udpEventStreamSetBandwidth.BytesPerSecond);
	}

	private void OnEventStreamQueue(UdpEvent ev)
	{
		UdpEventStreamQueue udpEventStreamQueue = ev.As<UdpEventStreamQueue>();
		if (!streamChannels.TryGetValue(udpEventStreamQueue.StreamOp.Channel, out var value))
		{
			UdpLog.Error("Unknown {0}", udpEventStreamQueue.StreamOp.Channel);
		}
		else
		{
			udpEventStreamQueue.Connection.OnStreamQueue(value, udpEventStreamQueue.StreamOp);
		}
	}

	private void OnEventStreamCreateChannel(UdpEventStreamCreateChannel ev)
	{
		UdpStreamChannel udpStreamChannel = new UdpStreamChannel
		{
			Config = ev.ChannelConfig
		};
		if (streamChannels.ContainsKey(udpStreamChannel.Name))
		{
			UdpLog.Error("Duplicate channel id '{0}', not creating channel '{0}'", udpStreamChannel.Name);
		}
		else
		{
			streamChannels.Add(udpStreamChannel.Name, udpStreamChannel);
			UdpLog.Debug("Channel {0} created", udpStreamChannel.Name);
		}
	}

	private void ConnectToEndPoint(UdpEndPoint endpoint, byte[] connectToken)
	{
		if (CheckState(UdpSocketState.Running))
		{
			OnEventLanBroadcastDisable();
			UdpConnection udpConnection = CreateConnection(endpoint, UdpConnectionMode.Client, connectToken);
			if (udpConnection == null)
			{
				UdpLog.Error("Could not create connection for endpoint {0}", endpoint);
			}
			else
			{
				UdpLog.Info("Connecting to {0}", endpoint);
			}
		}
	}

	private void DelayPacket(UdpEndPoint ep, byte[] data, int size)
	{
		uint simulatedPingMin = (uint)Config.SimulatedPingMin;
		uint simulatedPingMax = (uint)Config.SimulatedPingMax;
		uint num = simulatedPingMin + (uint)((float)(simulatedPingMax - simulatedPingMin) * Config.NoiseFunction());
		DelayedPacket item = new DelayedPacket
		{
			Data = ((delayedBuffers.Count > 0) ? delayedBuffers.Dequeue() : new byte[recvBuffer.Length]),
			EndPoint = ep,
			Size = size,
			Time = GetCurrentTime() + num
		};
		Array.Copy(data, 0, item.Data, 0, data.Length);
		delayedPackets.Enqueue(item);
	}

	private void RecvDelayedPackets()
	{
		while (delayedPackets.Count > 0 && GetCurrentTime() >= delayedPackets.Peek().Time)
		{
			DelayedPacket delayedPacket = delayedPackets.Dequeue();
			byte[] array = GetRecvBuffer();
			Array.Copy(delayedPacket.Data, 0, array, 0, delayedPacket.Data.Length);
			Array.Clear(delayedPacket.Data, 0, delayedPacket.Data.Length);
			RecvNetworkPacket(delayedPacket.EndPoint, array, delayedPacket.Size);
			delayedBuffers.Enqueue(delayedPacket.Data);
		}
	}

	public T GetSocket<T>() where T : UdpPlatformSocket
	{
		return (PlatformSocket is T val) ? val : null;
	}

	public UdpSocket(Guid gameId, UdpPlatform platform)
		: this(gameId, platform, new UdpConfig())
	{
	}

	public UdpSocket(Guid gameId, UdpPlatform targetPlatform, UdpConfig udpConfig)
	{
		GameId = gameId;
		PeerId = Guid.NewGuid();
		frame = 0;
		channelIdCounter = 0;
		connectionIdCounter = 1u;
		Config = udpConfig.Duplicate();
		platform = targetPlatform;
		platform.Configure(Config);
		platform.udpSocket = this;
		platformSocket = platform.CreateSocket(udpConfig.IPv6);
		if (Config.NoiseFunction == null)
		{
			Random random = new Random();
			Config.NoiseFunction = () => (float)random.NextDouble();
		}
		sendBuffer = new byte[Math.Max(udpConfig.StreamDatagramSize, udpConfig.PacketDatagramSize) * 2];
		recvBuffer = new byte[Math.Max(udpConfig.StreamDatagramSize, udpConfig.PacketDatagramSize) * 2];
		packetPool = new UdpPacketPool(this);
		eventQueueIn = new Queue<UdpEvent>(4096);
		eventQueueOut = new Queue<UdpEvent>(4096);
		PacketPipeConfig = new UdpPipeConfig
		{
			PipeId = 3,
			Timeout = 0u,
			AckBytes = 8,
			SequenceBytes = 2,
			UpdatePing = true,
			WindowSize = Config.PacketWindow,
			DatagramSize = Config.PacketDatagramSize
		};
		StreamPipeConfig = new UdpPipeConfig
		{
			PipeId = 4,
			Timeout = 500u,
			AckBytes = 32,
			SequenceBytes = 3,
			UpdatePing = false,
			WindowSize = Config.StreamWindow,
			DatagramSize = Config.StreamDatagramSize
		};
		sessionManager = new SessionManager(this);
		sessionManager.SetConnections(0, Config.ConnectionLimit);
		broadcaster = new Broadcaster(this);
		state = UdpSocketState.Created;
		ThreadManager.Start(NetworkLoop);
	}

	internal void Start(UdpEndPoint endpoint, ManualResetEvent resetEvent, UdpSocketMode mode)
	{
		Raise(new UdpEventStart
		{
			EndPoint = endpoint,
			Mode = mode,
			ResetEvent = resetEvent
		});
	}

	internal void Close(ManualResetEvent resetEvent)
	{
		if (state == UdpSocketState.Shutdown)
		{
			resetEvent?.Set();
			return;
		}
		Raise(new UdpEventClose
		{
			ResetEvent = resetEvent
		});
	}

	internal void Quit()
	{
		ThreadManager.Clear();
	}

	internal void Connect(UdpSession session, byte[] token)
	{
		Raise(new UdpEventSessionConnect
		{
			Session = session,
			Token = token
		});
	}

	internal void ConnectRandom(UdpSessionFilter sessionFilter, byte[] token)
	{
		Raise(new UdpEventSessionConnectRandom
		{
			SessionFilter = sessionFilter,
			Token = token
		});
	}

	internal void Connect(UdpEndPoint endpoint, byte[] token)
	{
		Raise(new UdpEventConnectEndPoint
		{
			EndPoint = endpoint,
			Token = token
		});
	}

	internal void CancelConnect(UdpEndPoint endpoint, bool internalOnly = false)
	{
		Raise(new UdpEventConnectEndPointCancel
		{
			EndPoint = endpoint,
			InternalOnly = internalOnly
		});
	}

	internal void Accept(UdpEndPoint endpoint, object userObject, byte[] token)
	{
		Raise(new UdpEventAcceptConnect
		{
			EndPoint = endpoint,
			Token = token,
			UserObject = userObject
		});
	}

	internal void Refuse(UdpEndPoint endpoint, byte[] token)
	{
		Raise(new UdpEventRefuseConnect
		{
			EndPoint = endpoint,
			Token = token
		});
	}

	internal UdpSession[] GetSessions()
	{
		return new UdpSession[0];
	}

	internal bool Poll(out UdpEvent ev)
	{
		lock (eventQueueOut)
		{
			if (eventQueueOut.Count > 0)
			{
				ev = eventQueueOut.Dequeue();
				return true;
			}
		}
		ev = default(UdpEvent);
		return false;
	}

	internal void MasterServerDisconnect()
	{
		UdpEvent ev = new UdpEvent
		{
			Type = 37
		};
		Raise(ev);
	}

	internal void LanBroadcastEnable(ushort port)
	{
		Raise(new UdpEventLanBroadcastEnable
		{
			Port = port
		});
	}

	internal void LanBroadcastDisable()
	{
		Raise(new UdpEventLanBroadcastDisable());
	}

	internal void SetHostInfo(string name, bool dedicated, object tokenObject, byte[] token)
	{
		Raise(new UdpEventSessionSetHostData
		{
			Name = name,
			TokenObject = tokenObject,
			Token = token,
			Dedicated = dedicated
		});
	}

	internal bool FindChannel(int id, out UdpStreamChannel channel)
	{
		return streamChannels.TryGetValue(new UdpChannelName(id), out channel);
	}

	internal byte[] GetSendBuffer()
	{
		Array.Clear(sendBuffer, 0, sendBuffer.Length);
		return sendBuffer;
	}

	internal byte[] GetRecvBuffer()
	{
		Array.Clear(recvBuffer, 0, recvBuffer.Length);
		return recvBuffer;
	}

	internal uint GetCurrentTime()
	{
		return platform.GetPrecisionTime();
	}

	internal void Raise(int type, UdpConnection c, UdpPacket p)
	{
		UdpEvent ev = new UdpEvent
		{
			Type = type,
			Object0 = c,
			Object1 = p
		};
		Raise(ev);
	}

	internal void Raise(UdpEvent ev)
	{
		if (ev.Type == 3)
		{
			UdpEventConnectEndPoint udpEventConnectEndPoint = ev.As<UdpEventConnectEndPoint>();
			bool flag = false;
			flag = (udpEventConnectEndPoint.EndPoint.IPv6 ? udpEventConnectEndPoint.EndPoint.AddressIPv6.IsLocalHost : udpEventConnectEndPoint.EndPoint.Address.IsLocalHost);
			StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
			StackFrame[] frames = stackTrace.GetFrames();
			string[] source = new string[3] { "bolt", "udpkit.platform.dotnet", "udpkit.platform.photon" };
			if (frames != null && !flag)
			{
				for (int i = 1; i < frames.Length; i++)
				{
					StackFrame stackFrame = frames[i];
					if (stackFrame != null)
					{
						Type reflectedType = stackFrame.GetMethod().ReflectedType;
						if (((object)reflectedType == null || reflectedType.Namespace == null || reflectedType.Namespace.Contains(Func())) && !source.Contains(reflectedType?.Assembly.GetName().Name))
						{
							throw new UdpKitAccessException();
						}
					}
				}
			}
		}
		if (ev.IsInternal)
		{
			lock (eventQueueIn)
			{
				eventQueueIn.Enqueue(ev);
				return;
			}
		}
		lock (eventQueueOut)
		{
			eventQueueOut.Enqueue(ev);
		}
		static string Func()
		{
			string text = "꼌껦껬껫껺껲";
			int j = 0;
			int num = 0;
			for (; j < 6; j++)
			{
				num = text[j];
				num = ~num;
				num -= 20640;
				text = text.Substring(0, j) + (char)(num & 0xFFFF) + text.Substring(j + 1);
			}
			string text2 = "ﾮﾚﾘﾑﾗﾘ";
			int k = 0;
			int num2 = 0;
			for (; k < 6; k++)
			{
				num2 = text2[k];
				num2--;
				num2 = ~num2;
				num2 -= k;
				text2 = text2.Substring(0, k) + (char)(num2 & 0xFFFF) + text2.Substring(k + 1);
			}
			string text3 = "奚奏奕奔";
			int l = 0;
			int num3 = 0;
			for (; l < 4; l++)
			{
				num3 = text3[l];
				num3 -= 22758;
				text3 = text3.Substring(0, l) + (char)(num3 & 0xFFFF) + text3.Substring(l + 1);
			}
			return text + "." + text2 + text3;
		}
	}

	internal bool Send(UdpEndPoint endpoint, byte[] buffer, int length)
	{
		if (state == UdpSocketState.Running || state == UdpSocketState.Created)
		{
			if (Singleton<EncryptionManager>.Instance.Ready)
			{
				length = Singleton<EncryptionManager>.Instance.Encrypt(endpoint, buffer, length);
			}
			return platformSocket.SendTo(buffer, length, endpoint) == length;
		}
		return false;
	}

	private void SendCommand(UdpEndPoint endpoint, byte cmd)
	{
		SendCommand(endpoint, cmd, null);
	}

	internal void SendCommand(UdpEndPoint endpoint, byte cmd, byte[] data)
	{
		int num = 2;
		byte[] array = GetSendBuffer();
		array[0] = 1;
		array[1] = cmd;
		if (data != null)
		{
			Array.Copy(data, 0, array, 2, data.Length);
			num += data.Length;
		}
		Send(endpoint, array, num);
	}

	private bool ChangeState(UdpSocketState from, UdpSocketState to)
	{
		if (CheckState(from))
		{
			state = to;
			return true;
		}
		return false;
	}

	private bool CheckState(UdpSocketState s)
	{
		if (state != s)
		{
			return false;
		}
		return true;
	}

	private void NetworkLoop()
	{
		while (state == UdpSocketState.Created || state == UdpSocketState.Running)
		{
			try
			{
				while (state == UdpSocketState.Created)
				{
					ProcessStartEvent();
					Thread.Sleep(1);
				}
				while (state == UdpSocketState.Running)
				{
					uint currentTime = GetCurrentTime();
					RecvDelayedPackets();
					RecvNetworkData();
					ProcessTimeouts();
					ProcessDataStream(currentTime);
					ProcessInternalEvents();
					broadcaster?.Update(currentTime);
					sessionManager.Update(currentTime);
					frame++;
				}
				break;
			}
			catch (Exception ex)
			{
				UdpLog.Error(ex.ToString());
			}
		}
	}

	private bool CreatePhysicalSocket(UdpEndPoint ep, UdpSocketState s)
	{
		UdpLog.Info("Binding physical socket using platform '{0}' with IP {1}", platform.GetType(), ep);
		if (ChangeState(UdpSocketState.Created, s))
		{
			platformSocket.Bind(ep);
			if (platformSocket.IsBound)
			{
				UdpLog.Info("Physical socket bound to {0}:{1}", platformSocket.EndPoint.Address, platformSocket.EndPoint.Port);
				return true;
			}
			ChangeState(s, UdpSocketState.Shutdown);
			UdpLog.Error("Could not bind physical socket, platform error: {0}", platformSocket.Error);
		}
		else
		{
			UdpLog.Error("Socket has incorrect state: {0}", state);
		}
		return false;
	}

	private void AcceptConnection(UdpEndPoint ep, object userToken, byte[] acceptToken, byte[] connectToken)
	{
		UdpConnection udpConnection = CreateConnection(ep, UdpConnectionMode.Server, connectToken);
		udpConnection.UserToken = userToken;
		udpConnection.AcceptToken = acceptToken;
		udpConnection.ConnectionId = ++connectionIdCounter;
		if (udpConnection.ConnectionId < 2)
		{
			UdpLog.Error("Incorrect connection id '{0}' assigned to {1}", udpConnection.ConnectionId, ep);
		}
		if (udpConnection.AcceptToken == null)
		{
			udpConnection.AcceptTokenWithPrefix = BitConverter.GetBytes(udpConnection.ConnectionId);
		}
		else
		{
			udpConnection.AcceptTokenWithPrefix = new byte[udpConnection.AcceptToken.Length + 4];
			Buffer.BlockCopy(BitConverter.GetBytes(udpConnection.ConnectionId), 0, udpConnection.AcceptTokenWithPrefix, 0, 4);
			Buffer.BlockCopy(udpConnection.AcceptToken, 0, udpConnection.AcceptTokenWithPrefix, 4, udpConnection.AcceptToken.Length);
		}
		udpConnection.ChangeState(UdpConnectionState.Connected);
		if (sessionManager != null)
		{
			sessionManager.SetConnections(connectionLookup.Count, Config.ConnectionLimit);
		}
	}

	private void ProcessTimeouts()
	{
		if ((frame & 3) != 3)
		{
			return;
		}
		uint currentTime = GetCurrentTime();
		for (int i = 0; i < connectionList.Count; i++)
		{
			UdpConnection udpConnection = connectionList[i];
			switch (udpConnection.State)
			{
			case UdpConnectionState.Connecting:
				udpConnection.ProcessConnectingTimeouts(currentTime);
				break;
			case UdpConnectionState.Connected:
				udpConnection.ProcessConnectedTimeouts(currentTime);
				break;
			case UdpConnectionState.Disconnected:
				udpConnection.ChangeState(UdpConnectionState.Destroy);
				break;
			case UdpConnectionState.Destroy:
				if (DestroyConnection(udpConnection))
				{
					i--;
				}
				break;
			}
		}
	}

	private void RecvNetworkData()
	{
		if (!platformSocket.RecvPoll(1))
		{
			return;
		}
		UdpEndPoint endpoint = UdpEndPoint.Any;
		byte[] array = GetRecvBuffer();
		int num = platformSocket.RecvFrom(array, ref endpoint);
		if (num > 0 && Singleton<EncryptionManager>.Instance.Ready)
		{
			num = Singleton<EncryptionManager>.Instance.Decrypt(endpoint, array, num);
		}
		if (num > 0 && !ShouldDropPacket)
		{
			if (ShouldDelayPacket)
			{
				DelayPacket(endpoint, array, num);
			}
			else
			{
				RecvNetworkPacket(endpoint, array, num);
			}
		}
	}

	private void RecvNetworkPacket(UdpEndPoint ep, byte[] buffer, int bytes)
	{
		switch (buffer[0])
		{
		case 1:
			RecvCommand(ep, buffer, bytes);
			break;
		case 3:
			RecvPacket(ep, buffer, bytes);
			break;
		case 4:
			RecvStream(ep, buffer, bytes);
			break;
		case 5:
			RecvStreamUnreliable(ep, buffer, bytes);
			break;
		case 2:
			break;
		}
	}

	private void RecvStreamUnreliable(UdpEndPoint ep, byte[] buffer, int bytes)
	{
		if (connectionLookup.TryGetValue(ep, out var value))
		{
			value.OnStreamReceived_Unreliable(buffer, bytes);
		}
	}

	private void RecvCommand(UdpEndPoint ep, byte[] buffer, int size)
	{
		if (connectionLookup.TryGetValue(ep, out var value))
		{
			value.OnCommandReceived(buffer, size);
		}
		else
		{
			RecvConnectionCommand_Unconnected(ep, buffer, size);
		}
	}

	private void RecvStream(UdpEndPoint ep, byte[] buffer, int bytes)
	{
		if (connectionLookup.TryGetValue(ep, out var value))
		{
			value.OnStreamReceived(buffer, bytes);
		}
	}

	private void RecvPacket(UdpEndPoint ep, byte[] buffer, int size)
	{
		if (connectionLookup.TryGetValue(ep, out var value))
		{
			value.OnPacketReceived(buffer, size);
		}
	}

	private void AddPendingConnection(UdpEndPoint endpoint, byte[] token)
	{
		if (!pendingConnections.ContainsKey(endpoint))
		{
			pendingConnections.Add(endpoint, token);
			Raise(new UdpEventConnectRequest
			{
				EndPoint = endpoint,
				Token = token
			});
		}
	}

	private UdpConnection CreateConnection(UdpEndPoint endpoint, UdpConnectionMode connectionMode, byte[] connectToken)
	{
		if (connectionLookup.ContainsKey(endpoint))
		{
			UdpLog.Warn("Connection for {0} already exists", endpoint);
			return null;
		}
		UdpConnection udpConnection = new UdpConnection(this, connectionMode, endpoint)
		{
			ConnectToken = connectToken
		};
		AddConnection(endpoint, udpConnection);
		return udpConnection;
	}

	private bool DestroyConnection(UdpConnection cn)
	{
		for (int i = 0; i < connectionList.Count; i++)
		{
			if (connectionList[i] == cn)
			{
				connectionList.RemoveAt(i);
				connectionLookup.Remove(cn.RemoteEndPoint);
				cn.Destroy();
				if (mode == UdpSocketMode.Host && sessionManager != null)
				{
					sessionManager.SetConnections(connectionList.Count, Config.ConnectionLimit);
				}
				return true;
			}
		}
		return false;
	}

	internal void Disconnect(UdpEndPoint endPoint)
	{
		if (connectionLookup.TryGetValue(endPoint, out var value))
		{
			value.Disconnect(null);
		}
	}

	private void RecvConnectionCommand_Unconnected(UdpEndPoint endpoint, byte[] buffer, int size)
	{
		if (buffer[1] != 1)
		{
			return;
		}
		byte[] array = UdpUtils.ReadToken(buffer, size, 2);
		if (Config.AllowIncommingConnections && (connectionLookup.Count + pendingConnections.Count < Config.ConnectionLimit || Config.ConnectionLimit == -1))
		{
			if (Config.AutoAcceptIncommingConnections)
			{
				AcceptConnection(endpoint, null, null, array);
			}
			else
			{
				AddPendingConnection(endpoint, array);
			}
		}
		else
		{
			SendCommand(endpoint, 3);
		}
	}

	private void AddConnection(UdpEndPoint endpoint, UdpConnection cn)
	{
		connectionList.Add(cn);
		connectionLookup.Add(endpoint, cn);
	}
}
