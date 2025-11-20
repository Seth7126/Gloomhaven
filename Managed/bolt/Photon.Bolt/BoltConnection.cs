#define DEBUG
using System;
using System.Collections.Generic;
using Photon.Bolt.Channel;
using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using Photon.Bolt.SceneManagement;
using Photon.Bolt.Tokens;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

[Documentation]
public class BoltConnection : BoltObject, IBoltListNode<BoltConnection>
{
	private readonly UdpConnection _udpConnection;

	private readonly BoltChannel[] _channels;

	private int _framesToStep;

	private int _packetsReceived;

	private int _packetCounter;

	private int _packetLostCounter;

	private int _remoteFrameDiff;

	private int _remoteFrameActual;

	private int _remoteFrameEstimated;

	private bool _remoteFrameAdjust;

	private int _bitsSecondIn;

	private int _bitsSecondInAcc;

	private int _bitsSecondOut;

	private int _bitsSecondOutAcc;

	private float _errorAccumulator;

	internal PacketTypeStats _commandStats;

	internal PacketTypeStats _stateStats;

	internal PacketTypeStats _eventsStats;

	internal BinaryDataChannel _binaryDataChannel;

	internal EventChannel _eventChannel;

	internal SceneLoadChannel _sceneLoadChannel;

	internal EntityChannel _entityChannel;

	internal EntityChannel.CommandChannel _commandChannel;

	internal List<Entity> _controlling;

	internal EntityList _controllingList;

	internal BoltRingBuffer<PacketStats> _packetStatsIn;

	internal BoltRingBuffer<PacketStats> _packetStatsOut;

	internal bool _canReceiveEntities = true;

	internal SceneLoadState _remoteSceneLoading;

	BoltConnection IBoltListNode<BoltConnection>.prev { get; set; }

	BoltConnection IBoltListNode<BoltConnection>.next { get; set; }

	object IBoltListNode<BoltConnection>.list { get; set; }

	public bool IsLoadingMap => _remoteSceneLoading.Scene != BoltCore._localSceneLoading.Scene || _remoteSceneLoading.State != 3;

	public EntityLookup ScopedTo => _entityChannel._outgoingLookup;

	public EntityLookup SourceOf => _entityChannel._incommingLookup;

	public EntityList HasControlOf => _controllingList;

	public int RemoteFrame => _remoteFrameEstimated;

	public IProtocolToken ConnectToken { get; internal set; }

	public IProtocolToken DisconnectToken { get; internal set; }

	public IProtocolToken AcceptToken { get; internal set; }

	public float PingNetwork => _udpConnection.NetworkPing;

	public int DejitterFrames => _remoteFrameActual - _remoteFrameEstimated;

	public float PingAliased => _udpConnection.AliasedPing;

	public UdpConnectionType ConnectionType => _udpConnection.ConnectionType;

	internal UdpConnection udpConnection => _udpConnection;

	internal int remoteFrameLatest => _remoteFrameActual;

	internal int remoteFrameDiff => _remoteFrameDiff;

	public int BitsPerSecondIn => _bitsSecondIn;

	public int BitsPerSecondOut => _bitsSecondOut;

	public int PacketsReceived => _packetsReceived;

	public int PacketsSent => _packetCounter;

	public int PacketsLost => _packetLostCounter;

	public uint ConnectionId => udpConnection.ConnectionId;

	public UdpEndPoint RemoteEndPoint => udpConnection.RemoteEndPoint;

	public object UserData { get; set; }

	public PacketTypeStats CommandsStats => _commandStats;

	public PacketTypeStats EventsStats => _eventsStats;

	public PacketTypeStats StatesStats => _stateStats;

	public UdpConnectionDisconnectReason DisconnectReason => udpConnection.DisconnectReason;

	internal int SendRateMultiplier
	{
		get
		{
			float windowFillRatio = udpConnection.WindowFillRatio;
			if (windowFillRatio < 0.25f)
			{
				return 1;
			}
			windowFillRatio -= 0.25f;
			windowFillRatio /= 0.75f;
			return Mathf.Clamp((int)(windowFillRatio * 60f), 1, 60);
		}
	}

	public void SetCanReceiveEntities(bool v)
	{
		_canReceiveEntities = v;
	}

	internal BoltConnection(UdpConnection udpConnection)
	{
		UserData = udpConnection.UserToken;
		_controlling = new List<Entity>();
		_controllingList = new EntityList(_controlling);
		_udpConnection = udpConnection;
		_udpConnection.UserToken = this;
		_channels = new BoltChannel[5]
		{
			_binaryDataChannel = new BinaryDataChannel(),
			_sceneLoadChannel = new SceneLoadChannel(),
			_commandChannel = new EntityChannel.CommandChannel(),
			_eventChannel = new EventChannel(),
			_entityChannel = new EntityChannel()
		};
		_remoteFrameAdjust = false;
		_remoteSceneLoading = SceneLoadState.DefaultRemote();
		_packetStatsOut = new BoltRingBuffer<PacketStats>(BoltCore._config.framesPerSecond)
		{
			autofree = true
		};
		_packetStatsIn = new BoltRingBuffer<PacketStats>(BoltCore._config.framesPerSecond)
		{
			autofree = true
		};
		_commandStats = new PacketTypeStats();
		_eventsStats = new PacketTypeStats();
		_stateStats = new PacketTypeStats();
		_errorAccumulator = 0f;
		for (int i = 0; i < _channels.Length; i++)
		{
			_channels[i].connection = this;
		}
	}

	public ExistsResult ExistsOnRemote(BoltEntity entity)
	{
		return _entityChannel.ExistsOnRemote(entity.Entity, allowMaybe: false);
	}

	public ExistsResult ExistsOnRemote(BoltEntity entity, bool allowMaybe)
	{
		return _entityChannel.ExistsOnRemote(entity.Entity, allowMaybe);
	}

	public void StreamBytes(UdpChannelName channel, byte[] data)
	{
		_udpConnection.StreamBytes(channel, data);
	}

	public void SetStreamBandwidth(int bytesPerSecond)
	{
		_udpConnection.StreamSetBandwidth(bytesPerSecond);
	}

	public void SendData(byte[] data)
	{
		_binaryDataChannel.Outgoing.Enqueue(data);
	}

	public bool ReceiveData(out byte[] data)
	{
		if (_binaryDataChannel.Incomming.Count > 0)
		{
			data = _binaryDataChannel.Incomming.Dequeue();
			return true;
		}
		data = null;
		return false;
	}

	public void Disconnect()
	{
		Disconnect(null);
	}

	public void Disconnect(IProtocolToken token, UdpConnectionDisconnectReason disconnectReason = UdpConnectionDisconnectReason.Disconnected)
	{
		_udpConnection.Disconnect(token.ToByteArray(), disconnectReason);
	}

	public int GetSkippedUpdates(BoltEntity en)
	{
		return _entityChannel.GetSkippedUpdates(en.Entity);
	}

	public void ForceSceneSync()
	{
		_sceneLoadChannel?.ForceSceneSync();
	}

	public override bool Equals(object obj)
	{
		return this == obj;
	}

	public override int GetHashCode()
	{
		return _udpConnection.GetHashCode();
	}

	public override string ToString()
	{
		return $"[Connection {_udpConnection.RemoteEndPoint}]";
	}

	internal void DisconnectedInternal()
	{
		for (int i = 0; i < _channels.Length; i++)
		{
			_channels[i].Disconnected();
		}
		if (UserData != null)
		{
			if (UserData is IDisposable)
			{
				(UserData as IDisposable).Dispose();
			}
			UserData = null;
		}
	}

	internal bool StepRemoteEntities()
	{
		if (_framesToStep > 0)
		{
			_framesToStep--;
			_remoteFrameEstimated++;
			Dictionary<NetworkId, EntityProxy>.Enumerator enumerator = _entityChannel._incommingDict.GetEnumerator();
			while (enumerator.MoveNext())
			{
				EntityProxy value = enumerator.Current.Value;
				if (!value.Entity.HasPredictedControl && !value.Entity.IsFrozen)
				{
					value.Entity.Simulate();
				}
			}
		}
		return _framesToStep > 0;
	}

	internal void AdjustRemoteFrame()
	{
		if (_packetsReceived == 0)
		{
			return;
		}
		if (BoltCore._config.disableDejitterBuffer)
		{
			if (_remoteFrameAdjust)
			{
				_framesToStep = Mathf.Max(0, _remoteFrameActual - _remoteFrameEstimated);
				_remoteFrameEstimated = _remoteFrameActual;
				_remoteFrameAdjust = false;
			}
			else
			{
				_framesToStep = 1;
			}
			return;
		}
		int remoteSendRate = BoltCore.remoteSendRate;
		int localInterpolationDelay = BoltCore.localInterpolationDelay;
		int localInterpolationDelayMin = BoltCore.localInterpolationDelayMin;
		int localInterpolationDelayMax = BoltCore.localInterpolationDelayMax;
		bool flag = localInterpolationDelay >= 0;
		if (_remoteFrameAdjust)
		{
			_remoteFrameAdjust = false;
			if (flag)
			{
				if (_packetsReceived == 1)
				{
					_remoteFrameEstimated = _remoteFrameActual - localInterpolationDelay;
				}
				_remoteFrameDiff = _remoteFrameActual - _remoteFrameEstimated;
				if (_remoteFrameDiff < localInterpolationDelayMin - remoteSendRate || _remoteFrameDiff > localInterpolationDelayMax + remoteSendRate)
				{
					int remoteFrameEstimated = _remoteFrameActual - localInterpolationDelay;
					BoltLog.Debug("{0} FRAME RESET: {1}", this, _remoteFrameDiff);
					_remoteFrameEstimated = remoteFrameEstimated;
					_remoteFrameDiff = _remoteFrameActual - _remoteFrameEstimated;
				}
			}
		}
		if (flag)
		{
			if (_remoteFrameDiff > localInterpolationDelayMax)
			{
				BoltLog.Debug("{0} FRAME FORWARD: {1}", this, _remoteFrameDiff);
				_framesToStep = 2;
				_remoteFrameDiff -= _framesToStep;
			}
			else if (_remoteFrameDiff < localInterpolationDelayMin)
			{
				BoltLog.Debug("{0} FRAME STALL: {1}", this, _remoteFrameDiff);
				_framesToStep = 0;
				_remoteFrameDiff++;
			}
			else
			{
				_framesToStep = 1;
			}
		}
		else
		{
			_remoteFrameEstimated = _remoteFrameActual - (remoteSendRate - 1);
		}
	}

	internal void SwitchPerfCounters()
	{
		_bitsSecondOut = _bitsSecondOutAcc;
		_bitsSecondOutAcc = 0;
		_bitsSecondIn = _bitsSecondInAcc;
		_bitsSecondInAcc = 0;
		if (BoltRuntimeSettings.instance.enableClientMetrics)
		{
			_commandStats.Update(_packetStatsIn, _packetStatsOut, (PacketStats x) => x.CommandBits);
			_eventsStats.Update(_packetStatsIn, _packetStatsOut, (PacketStats x) => x.EventBits);
			_stateStats.Update(_packetStatsIn, _packetStatsOut, (PacketStats x) => x.StateBits);
		}
	}

	internal void Send()
	{
		try
		{
			Packet packet = PacketPool.Acquire();
			packet.Frame = BoltCore.frame;
			packet.Number = ++_packetCounter;
			packet.UdpPacket = BoltCore.AllocateUdpPacket();
			packet.UdpPacket.UserToken = packet;
			packet.UdpPacket.WriteIntVB(packet.Frame);
			for (int i = 0; i < _channels.Length; i++)
			{
				_channels[i].Pack(packet);
			}
			Assert.False(packet.UdpPacket.Overflowing);
			_udpConnection.Send(packet.UdpPacket);
			GlobalEventListenerBase.OnPacketSendInvoke(this, packet);
			_bitsSecondOutAcc += packet.UdpPacket.Position;
			_packetStatsOut.Enqueue(packet.Stats);
		}
		catch (Exception exception)
		{
			BoltLog.Exception(exception);
			throw;
		}
	}

	internal void PacketReceived(UdpPacket udpPacket)
	{
		try
		{
			using Packet packet = PacketPool.Acquire();
			packet.UdpPacket = udpPacket;
			packet.Frame = packet.UdpPacket.ReadIntVB();
			if (packet.Frame > _remoteFrameActual)
			{
				_remoteFrameAdjust = true;
				_remoteFrameActual = packet.Frame;
			}
			_bitsSecondInAcc += packet.UdpPacket.Size;
			_packetsReceived++;
			for (int i = 0; i < _channels.Length; i++)
			{
				_channels[i].Read(packet);
			}
			_packetStatsIn.Enqueue(packet.Stats);
			Assert.False<BoltPackageOverflowException>(udpPacket.Overflowing, udpConnection.RemoteEndPoint);
			_errorAccumulator = Mathf.Max(0f, _errorAccumulator - (float)BoltCore.frameSlice);
		}
		catch (BoltPackageOverflowException ex)
		{
			BoltLog.Exception(ex);
			Disconnect(new BoltDisconnectToken(ex.Message, UdpConnectionDisconnectReason.Timeout), UdpConnectionDisconnectReason.Timeout);
		}
		catch (Exception ex2)
		{
			_errorAccumulator += BoltCore.frameSlice * 2;
			BoltLog.Warn(ex2);
			BoltLog.Warn("Exception thrown while unpacking data from {0} ({1})", udpConnection.RemoteEndPoint, _errorAccumulator);
			if (_errorAccumulator >= 1f)
			{
				BoltLog.Error("Too many errors accumulated while unpacking data from {0}, disconnecting.", udpConnection.RemoteEndPoint);
				Disconnect(new BoltDisconnectToken(ex2.Message, UdpConnectionDisconnectReason.Error), UdpConnectionDisconnectReason.Error);
			}
		}
	}

	internal void PacketDelivered(Packet packet)
	{
		try
		{
			for (int i = 0; i < _channels.Length; i++)
			{
				_channels[i].Delivered(packet);
			}
		}
		catch (Exception exception)
		{
			BoltLog.Exception(exception);
			BoltLog.Error("exception thrown while handling delivered packet to {0}", udpConnection.RemoteEndPoint);
		}
	}

	internal void PacketLost(Packet packet)
	{
		try
		{
			_packetLostCounter++;
			for (int i = 0; i < _channels.Length; i++)
			{
				_channels[i].Lost(packet);
			}
		}
		catch (Exception exception)
		{
			BoltLog.Exception(exception);
			BoltLog.Error("exception thrown while handling lost packet to {0}", udpConnection.RemoteEndPoint);
		}
	}

	public static implicit operator bool(BoltConnection cn)
	{
		return cn != null;
	}
}
