#define DEBUG
using System;
using System.Collections.Generic;
using DamienG.Security.Cryptography;
using UdpKit.Security;

namespace UdpKit;

public class UdpConnection
{
	private bool ignore_next_disconnect = false;

	internal const byte COMMAND_CONNECT = 1;

	internal const byte COMMAND_ACCEPTED = 2;

	internal const byte COMMAND_REFUSED = 3;

	internal const byte COMMAND_DISCONNECTED = 4;

	internal const byte COMMAND_RECONNECT_LATER = 6;

	internal const byte COMMAND_PING = 5;

	private float NetworkRtt = 0.1f;

	private float AliasedRtt = 0.1f;

	private uint ConnectTimeout;

	private uint ConnectAttempts;

	private readonly uint CreateTime;

	private uint StreamSendInterval = 50u;

	private uint StreamLastSend = 0u;

	private readonly Dictionary<UdpChannelName, UdpChannelStreamer> StreamChannels;

	internal UdpPipe PacketPipe;

	internal UdpPipe StreamPipe;

	private readonly UdpEndPoint EndPoint;

	private readonly UdpConnectionMode Mode;

	internal UdpSocket Socket;

	internal UdpConnectionState State;

	internal volatile uint ConnectionId;

	internal byte[] ConnectToken;

	internal byte[] DisconnectToken;

	internal byte[] AcceptToken;

	internal byte[] AcceptTokenWithPrefix;

	private readonly List<UdpChannelStreamer> outgoingStreamChannels = new List<UdpChannelStreamer>(10);

	internal uint SendTime { get; private set; }

	internal uint RecvTime { get; private set; }

	public object UserToken { get; set; }

	public UdpConnectionDisconnectReason DisconnectReason { get; internal set; }

	public UdpConnectionType ConnectionType { get; internal set; }

	public float NetworkPing => NetworkRtt;

	public float AliasedPing => AliasedRtt;

	public bool IsClient => Mode == UdpConnectionMode.Client;

	public bool IsServer => Mode == UdpConnectionMode.Server;

	public bool IsConnected => State == UdpConnectionState.Connected;

	public UdpEndPoint RemoteEndPoint => EndPoint;

	public float WindowFillRatio => PacketPipe.FillRatio;

	internal void OnCommandReceived(byte[] buffer, int size)
	{
		RecvTime = Socket.GetCurrentTime();
		switch (buffer[1])
		{
		case 1:
			OnCommandConnect();
			break;
		case 2:
			OnCommandAccepted(buffer, size);
			break;
		case 3:
			OnCommandRefused(buffer, size);
			break;
		case 4:
			if (UdpSocket.AllowConnectionRecycle && ignore_next_disconnect)
			{
				ignore_next_disconnect = false;
			}
			else
			{
				OnCommandDisconnected(buffer, size);
			}
			break;
		case 6:
			OnCommandReconnectLater();
			break;
		case 5:
			OnCommandPing();
			break;
		default:
			ConnectionError(UdpConnectionError.IncorrectCommand);
			break;
		}
	}

	private void OnCommandReconnectLater()
	{
		if (UdpSocket.AllowConnectionRecycle)
		{
			ignore_next_disconnect = true;
			ConnectTimeout = Socket.GetCurrentTime() - 5000;
		}
	}

	internal void SendCommand(byte cmd)
	{
		SendCommand(cmd, null);
	}

	internal void SendCommand(byte cmd, byte[] data)
	{
		UpdateSendTime();
		Socket.SendCommand(EndPoint, cmd, data);
	}

	private bool SendCommandConnect()
	{
		if (ConnectAttempts < Socket.Config.ConnectRequestAttempts)
		{
			if (ConnectAttempts != 0)
			{
				UdpLog.Info("{0} retrying connect ...", EndPoint.ToString());
			}
			Socket.Raise(new UdpEventConnectAttempt
			{
				EndPoint = RemoteEndPoint,
				Token = ConnectToken
			});
			SendCommand(1, ConnectToken);
			ConnectTimeout = Socket.GetCurrentTime() + Socket.Config.ConnectRequestTimeout;
			ConnectAttempts++;
			return true;
		}
		return false;
	}

	private void OnCommandConnect()
	{
		if (IsServer)
		{
			if (CheckState(UdpConnectionState.Connected))
			{
				if (UdpSocket.AllowConnectionRecycle && CreateTime + 10000 < Socket.GetCurrentTime())
				{
					SendCommand(6);
					DisconnectReason = UdpConnectionDisconnectReason.Disconnected;
					ChangeState(UdpConnectionState.Disconnected);
				}
				else
				{
					SendCommand(2, AcceptTokenWithPrefix);
				}
			}
		}
		else
		{
			ConnectionError(UdpConnectionError.IncorrectCommand);
		}
	}

	private void OnCommandAccepted(byte[] buffer, int size)
	{
		if (IsClient)
		{
			UdpLog.Info("Connect to {0} accepted", RemoteEndPoint);
			if (CheckState(UdpConnectionState.Connecting))
			{
				if (size > 6)
				{
					AcceptToken = new byte[size - 6];
					Buffer.BlockCopy(buffer, 6, AcceptToken, 0, size - 6);
				}
				AcceptTokenWithPrefix = new byte[size - 2];
				Buffer.BlockCopy(buffer, 2, AcceptTokenWithPrefix, 0, size - 2);
				ConnectionId = BitConverter.ToUInt32(AcceptTokenWithPrefix, 0);
				if (ConnectionId < 2)
				{
					UdpLog.Error("Incorrect connection id #{0} received from server", ConnectionId);
				}
				else
				{
					UdpLog.Info("Correct connection id #{0} received from server", ConnectionId);
				}
				UdpAssert.Assert(ConnectionId > 1);
				ChangeState(UdpConnectionState.Connected);
			}
		}
		else
		{
			ConnectionError(UdpConnectionError.IncorrectCommand);
		}
	}

	private void OnCommandRefused(byte[] buffer, int size)
	{
		if (IsClient)
		{
			if (CheckState(UdpConnectionState.Connecting))
			{
				Socket.Raise(new UdpEventConnectRefused
				{
					EndPoint = RemoteEndPoint,
					Token = UdpUtils.ReadToken(buffer, size, 2)
				});
				ChangeState(UdpConnectionState.Destroy);
			}
		}
		else
		{
			ConnectionError(UdpConnectionError.IncorrectCommand);
		}
	}

	private void OnCommandDisconnected(byte[] buffer, int size)
	{
		if (CheckState(UdpConnectionState.Connected))
		{
			DisconnectReason = UdpConnectionDisconnectReason.Disconnected;
			ChangeState(UdpConnectionState.Disconnected, UdpUtils.ReadToken(buffer, size, 2));
		}
	}

	private void OnCommandPing()
	{
	}

	internal void OnPacketSend(UdpPacket packet)
	{
		if (!IsConnected)
		{
			Socket.Raise(14, this, packet);
			return;
		}
		byte[] sendBuffer = Socket.GetSendBuffer();
		Blit.PackBytes(sendBuffer, PacketPipe.Config.HeaderSize, packet.Data, UdpMath.BytesRequired(packet.Position));
		if (!PacketPipe.WriteHeader(sendBuffer, packet) || !Socket.Send(EndPoint, sendBuffer, PacketPipe.Config.HeaderSize + UdpMath.BytesRequired(packet.Position)))
		{
			Socket.Raise(14, this, packet);
		}
	}

	internal void OnPacketReceived(byte[] buffer, int size)
	{
		RecvTime = Socket.GetCurrentTime();
		if (CheckState(UdpConnectionState.Connected) && PacketPipe.ReadHeader(buffer, size))
		{
			UdpPacket udpPacket = Socket.PacketPool.Acquire();
			udpPacket.Size = size - PacketPipe.Config.HeaderSize << 3;
			Blit.ReadBytes(buffer, PacketPipe.Config.HeaderSize, udpPacket.Data, size - PacketPipe.Config.HeaderSize);
			if (udpPacket.Size > 0)
			{
				Socket.Raise(16, this, udpPacket);
			}
		}
	}

	internal UdpConnection(UdpSocket socket, UdpConnectionMode mode, UdpEndPoint endPoint)
	{
		Mode = mode;
		Socket = socket;
		EndPoint = endPoint;
		RecvTime = (CreateTime = socket.GetCurrentTime());
		NetworkRtt = Socket.Config.DefaultNetworkPing;
		AliasedRtt = Socket.Config.DefaultAliasedPing;
		State = UdpConnectionState.Connecting;
		PacketPipe = new UdpPipe(this, Socket.PacketPipeConfig);
		StreamPipe = new UdpPipe(this, Socket.StreamPipeConfig);
		StreamChannels = new Dictionary<UdpChannelName, UdpChannelStreamer>(UdpChannelName.EqualityComparer.Instance);
	}

	public void Send(UdpPacket packet)
	{
		UdpEvent ev = new UdpEvent
		{
			Type = 15,
			Object0 = this,
			Object1 = packet
		};
		Socket.Raise(ev);
	}

	public void Disconnect(byte[] token, UdpConnectionDisconnectReason disconnectReason = UdpConnectionDisconnectReason.Disconnected)
	{
		Socket.Raise(new UdpEventDisconnect
		{
			Connection = this,
			Token = token,
			DisconnectReason = disconnectReason
		});
	}

	internal void Lost(UdpPipe pipe, object obj)
	{
		if (obj != null)
		{
			switch (pipe.Id)
			{
			case 3:
			{
				UdpEvent ev = new UdpEvent
				{
					Type = 14,
					Object0 = this,
					Object1 = obj
				};
				Socket.Raise(ev);
				break;
			}
			case 4:
				OnStreamLost((UdpStreamOpBlock)obj);
				break;
			}
		}
	}

	internal void Delivered(UdpPipe pipe, object obj)
	{
		if (obj != null)
		{
			switch (pipe.Id)
			{
			case 3:
			{
				UdpEvent ev = new UdpEvent
				{
					Type = 18,
					Object0 = this,
					Object1 = obj
				};
				Socket.Raise(ev);
				break;
			}
			case 4:
				OnStreamDelivered((UdpStreamOpBlock)obj);
				break;
			}
		}
	}

	internal void ProcessConnectingTimeouts(uint now)
	{
		UdpConnectionMode mode = Mode;
		UdpConnectionMode udpConnectionMode = mode;
		if (udpConnectionMode == UdpConnectionMode.Client && ConnectTimeout < now && !SendCommandConnect())
		{
			Socket.Raise(new UdpEventConnectFailed
			{
				EndPoint = EndPoint,
				Token = ConnectToken
			});
			ChangeState(UdpConnectionState.Destroy);
		}
	}

	internal void ProcessConnectedTimeouts(uint now)
	{
		if (RecvTime + Socket.Config.ConnectionTimeout < now)
		{
			UdpLog.Debug("{0} timed out", EndPoint);
			DisconnectReason = UdpConnectionDisconnectReason.Timeout;
			ChangeState(UdpConnectionState.Disconnected);
		}
		if (CheckState(UdpConnectionState.Connected))
		{
			if (SendTime + Socket.Config.PingTimeout < now)
			{
				SendCommand(5);
			}
			PacketPipe.CheckTimeouts(now);
			StreamPipe.CheckTimeouts(now);
		}
	}

	internal void ChangeState(UdpConnectionState newState)
	{
		ChangeState(newState, null);
	}

	internal void ChangeState(UdpConnectionState newState, byte[] token)
	{
		if (newState != State)
		{
			UdpConnectionState state = State;
			switch (State = newState)
			{
			case UdpConnectionState.Connected:
				OnStateConnected(state);
				break;
			case UdpConnectionState.Disconnected:
				OnStateDisconnected(state, token);
				break;
			}
		}
	}

	internal bool CheckState(UdpConnectionState stateValue)
	{
		return State == stateValue;
	}

	internal void UpdatePing(uint recvTime, uint sendTime, uint ackTime)
	{
		uint num = recvTime - sendTime;
		AliasedRtt = AliasedRtt * 0.9f + (float)num / 1000f * 0.1f;
		uint num2 = num - UdpMath.Clamp(ackTime, 0u, num);
		NetworkRtt = NetworkRtt * 0.9f + (float)num2 / 1000f * 0.1f;
	}

	internal void ConnectionError(UdpConnectionError error)
	{
		if (error == UdpConnectionError.SequenceOutOfBounds)
		{
			SendCommand(6);
		}
		ConnectionError(error, "");
	}

	internal void ConnectionError(UdpConnectionError error, string message)
	{
		UdpLog.Error("{1} error {0}: '{2}'", error, EndPoint, message);
		DisconnectReason = UdpConnectionDisconnectReason.Error;
		ChangeState(UdpConnectionState.Disconnected);
	}

	internal void UpdateSendTime()
	{
		SendTime = Socket.GetCurrentTime();
	}

	internal void Destroy()
	{
	}

	private void OnStateConnected(UdpConnectionState oldState)
	{
		if (oldState == UdpConnectionState.Connecting)
		{
			UdpLog.Info("{0} connected", EndPoint);
			if (IsServer)
			{
				SendCommand(2, AcceptTokenWithPrefix);
			}
			UdpEvent ev = new UdpEvent
			{
				Type = 8,
				Object0 = this
			};
			Socket.Raise(ev);
		}
	}

	private void OnStateDisconnected(UdpConnectionState oldState, byte[] token)
	{
		if (oldState == UdpConnectionState.Connected)
		{
			UdpLog.Info("{0} disconnected", RemoteEndPoint);
			PacketPipe.Disconnected();
			OnStreamDisconnected();
			Singleton<EncryptionManager>.Instance.RemoveEndPointReference(RemoteEndPoint);
			DisconnectToken = token;
			UdpEvent ev = new UdpEvent
			{
				Type = 12,
				Object0 = this
			};
			Socket.Raise(ev);
		}
	}

	public override string ToString()
	{
		return $"UdpConnection[{EndPoint}]";
	}

	public void StreamSetBandwidth(int bytesPerSecond)
	{
		Socket.Raise(new UdpEventStreamSetBandwidth
		{
			Connection = this,
			BytesPerSecond = bytesPerSecond
		});
	}

	public void StreamBytes(UdpChannelName channel, byte[] data)
	{
		Socket.Raise(new UdpEventStreamQueue
		{
			Connection = this,
			StreamOp = new UdpStreamOp(0uL, channel, data)
		});
	}

	internal void OnStreamSetBandwidth(int byteRate)
	{
		StreamSendInterval = (uint)(1000f / (float)(byteRate / Socket.StreamPipeConfig.DatagramSize));
		UdpLog.Debug("Stream Send Rate updated to {0}kB/s on connection {1}. Sending packages of size {2} every {3}ms", (float)byteRate / 1024f, this, Socket.StreamPipeConfig.DatagramSize, StreamSendInterval);
	}

	internal void OnStreamDisconnected()
	{
		foreach (UdpChannelStreamer value in StreamChannels.Values)
		{
			value.Clear();
		}
	}

	internal void ProcessStream(uint now)
	{
		if (StreamLastSend + StreamSendInterval < now)
		{
			SendStream(now);
		}
		ReceiveStream();
	}

	internal void OnStreamQueue(UdpStreamChannel channel, UdpStreamOp op)
	{
		op.CRC = Crc32.Compute(op.Data);
		if (!StreamChannels.TryGetValue(channel.Name, out var value))
		{
			value = (StreamChannels[channel.Name] = new UdpChannelStreamer(this, channel));
		}
		if (value.Channel.IsUnreliable)
		{
			UdpLog.Info("Sending {0} bytes unreliably to {1}", op.Data.Length, RemoteEndPoint);
			int num = StreamPipe.Config.DatagramSize - StreamPipe.Config.HeaderSize;
			if (op.Data.Length > num)
			{
				UdpLog.Error("Can't queue unreliable data streams larger than {0}", num);
				return;
			}
			int offset = 0;
			byte[] recvBuffer = Socket.GetRecvBuffer();
			Blit.PackByte(recvBuffer, ref offset, 5);
			Blit.PackI32(recvBuffer, ref offset, channel.Name.Id);
			Blit.PackBytesPrefix(recvBuffer, ref offset, op.Data);
			Socket.Send(RemoteEndPoint, recvBuffer, offset);
		}
		else
		{
			value.Queue(op.Data);
		}
	}

	internal void OnStreamReceived_Unreliable(byte[] buffer, int size)
	{
		int offset = 1;
		int id = Blit.ReadI32(buffer, ref offset);
		byte[] data = Blit.ReadBytesPrefix(buffer, ref offset);
		if (Socket.FindChannel(id, out var channel))
		{
			UdpEventStreamDataReceived udpEventStreamDataReceived = new UdpEventStreamDataReceived
			{
				Connection = this,
				StreamData = new UdpStreamData
				{
					Channel = channel.Name,
					Data = data
				}
			};
			Socket.Raise(udpEventStreamDataReceived);
		}
	}

	internal void OnStreamReceived(byte[] buffer, int bytes)
	{
		RecvTime = Socket.GetCurrentTime();
		if (!StreamPipe.ReadHeader(buffer, bytes) || bytes <= StreamPipe.Config.HeaderSize)
		{
			return;
		}
		int offset = StreamPipe.Config.HeaderSize;
		int id = Blit.ReadI32(buffer, ref offset);
		if (Socket.FindChannel(id, out var channel))
		{
			UdpLog.Info("Received {0} bytes from {1}", bytes, RemoteEndPoint);
			if (!StreamChannels.TryGetValue(new UdpChannelName(id), out var value))
			{
				StreamChannels.Add(channel.Name, value = new UdpChannelStreamer(this, channel));
			}
			value.OnBlockReceived(buffer, bytes, offset);
		}
		else
		{
			ConnectionError(UdpConnectionError.UnknownStreamChannel, id.ToString());
		}
	}

	private void OnStreamLost(UdpStreamOpBlock block)
	{
		if (StreamChannels.TryGetValue(block.Op.Channel, out var value))
		{
			value.OnBlockLost(block.Op, block.Number);
			value.streamBlockPool.Return(block);
		}
	}

	private void OnStreamDelivered(UdpStreamOpBlock block)
	{
		if (StreamChannels.TryGetValue(block.Op.Channel, out var value))
		{
			value.OnBlockDelivered(block.Op, block.Number);
			value.streamBlockPool.Return(block);
		}
	}

	private int SortChannelHelper(UdpChannelStreamer x, UdpChannelStreamer y)
	{
		return (x.Priority <= y.Priority) ? 1 : (-1);
	}

	private void SendStream(uint now)
	{
		if (!IsConnected || StreamChannels.Count == 0)
		{
			return;
		}
		bool flag = false;
		outgoingStreamChannels.Clear();
		foreach (UdpChannelStreamer value in StreamChannels.Values)
		{
			if (value.OutgoingData.Count > 0)
			{
				if (value.Priority > 0)
				{
					flag = true;
				}
				outgoingStreamChannels.Add(value);
			}
		}
		if (outgoingStreamChannels.Count == 0)
		{
			return;
		}
		for (int i = 0; i < outgoingStreamChannels.Count; i++)
		{
			if (!flag)
			{
				outgoingStreamChannels[i].Priority = outgoingStreamChannels[i].Channel.Config.Priority;
			}
		}
		outgoingStreamChannels.Sort(SortChannelHelper);
		for (int j = 0; j < outgoingStreamChannels.Count; j++)
		{
			if (outgoingStreamChannels[j].Priority == 0)
			{
				outgoingStreamChannels[j].Priority = outgoingStreamChannels[j].Channel.Config.Priority;
			}
			if (outgoingStreamChannels[j].TrySend())
			{
				outgoingStreamChannels[j].Priority--;
				UdpAssert.Assert(outgoingStreamChannels[j].Priority >= 0);
				StreamLastSend = now;
				break;
			}
		}
	}

	private void ReceiveStream()
	{
		if (!IsConnected || StreamChannels.Count == 0)
		{
			return;
		}
		foreach (UdpChannelStreamer value in StreamChannels.Values)
		{
			if (value.IncommingData.Count > 0 && value.ProcessReceivedBlocks())
			{
				break;
			}
		}
	}
}
