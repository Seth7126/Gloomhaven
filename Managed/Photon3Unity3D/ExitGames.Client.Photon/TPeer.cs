using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon;

internal class TPeer : PeerBase
{
	internal const int TCP_HEADER_BYTES = 7;

	internal const int MSG_HEADER_BYTES = 2;

	public const int ALL_HEADER_BYTES = 9;

	private Queue<StreamBuffer> incomingList = new Queue<StreamBuffer>(32);

	internal List<StreamBuffer> outgoingStream;

	private int lastPingResult;

	private byte[] pingRequest = new byte[5] { 240, 0, 0, 0, 0 };

	internal static readonly byte[] tcpFramedMessageHead = new byte[9] { 251, 0, 0, 0, 0, 0, 0, 243, 2 };

	internal static readonly byte[] tcpMsgHead = new byte[2] { 243, 2 };

	protected internal bool DoFraming = true;

	internal override int QueuedIncomingCommandsCount => incomingList.Count;

	internal override int QueuedOutgoingCommandsCount => outgoingCommandsInStream;

	internal TPeer()
	{
		TrafficPackageHeaderSize = 0;
	}

	internal override bool IsTransportEncrypted()
	{
		return usedTransportProtocol == ConnectionProtocol.WebSocketSecure;
	}

	internal override void Reset()
	{
		base.Reset();
		if (photonPeer.PayloadEncryptionSecret != null && usedTransportProtocol != ConnectionProtocol.WebSocketSecure)
		{
			InitEncryption(photonPeer.PayloadEncryptionSecret);
		}
		incomingList = new Queue<StreamBuffer>(32);
		timestampOfLastReceive = base.timeInt;
	}

	internal override bool Connect(string serverAddress, string proxyServerAddress, string appID, object photonToken)
	{
		outgoingStream = new List<StreamBuffer>();
		messageHeader = (DoFraming ? tcpFramedMessageHead : tcpMsgHead);
		if (usedTransportProtocol == ConnectionProtocol.WebSocket || usedTransportProtocol == ConnectionProtocol.WebSocketSecure)
		{
			PhotonSocket.ConnectAddress = PepareWebSocketUrl(serverAddress, appID, photonToken);
		}
		if (PhotonSocket.Connect())
		{
			peerConnectionState = ConnectionStateValue.Connecting;
			return true;
		}
		return false;
	}

	public override void OnConnect()
	{
		lastPingResult = base.timeInt;
		if (DoFraming || PhotonToken != null)
		{
			byte[] data = WriteInitRequest();
			EnqueueInit(data);
		}
		SendOutgoingCommands();
	}

	internal override void Disconnect()
	{
		if (peerConnectionState != ConnectionStateValue.Disconnected && peerConnectionState != ConnectionStateValue.Disconnecting)
		{
			if ((int)base.debugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, "TPeer.Disconnect()");
			}
			StopConnection();
		}
	}

	internal override void StopConnection()
	{
		peerConnectionState = ConnectionStateValue.Disconnecting;
		if (PhotonSocket != null)
		{
			PhotonSocket.Disconnect();
		}
		lock (incomingList)
		{
			incomingList.Clear();
		}
		peerConnectionState = ConnectionStateValue.Disconnected;
		EnqueueStatusCallback(StatusCode.Disconnect);
	}

	internal override void FetchServerTimestamp()
	{
		if (peerConnectionState != ConnectionStateValue.Connected)
		{
			if ((int)base.debugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + peerConnectionState);
			}
			base.Listener.OnStatusChanged(StatusCode.SendError);
		}
		else
		{
			SendPing();
			serverTimeOffsetIsAvailable = false;
		}
	}

	private void EnqueueInit(byte[] data)
	{
		StreamBuffer streamBuffer = new StreamBuffer(data.Length + 32);
		byte[] array = new byte[7] { 251, 0, 0, 0, 0, 0, 1 };
		int targetOffset = 1;
		Protocol.Serialize(data.Length + array.Length, array, ref targetOffset);
		streamBuffer.Write(array, 0, array.Length);
		streamBuffer.Write(data, 0, data.Length);
		if (base.TrafficStatsEnabled)
		{
			base.TrafficStatsOutgoing.CountControlCommand(streamBuffer.Length);
		}
		EnqueueMessageAsPayload(DeliveryMode.Reliable, streamBuffer, 0);
	}

	internal override bool DispatchIncomingCommands()
	{
		if (peerConnectionState == ConnectionStateValue.Connected && base.timeInt - timestampOfLastReceive > base.DisconnectTimeout)
		{
			EnqueueStatusCallback(StatusCode.TimeoutDisconnect);
			EnqueueActionForDispatch(Disconnect);
		}
		while (true)
		{
			MyAction myAction;
			lock (ActionQueue)
			{
				if (ActionQueue.Count <= 0)
				{
					break;
				}
				myAction = ActionQueue.Dequeue();
				goto IL_0097;
			}
			IL_0097:
			myAction();
		}
		StreamBuffer streamBuffer;
		lock (incomingList)
		{
			if (incomingList.Count <= 0)
			{
				return false;
			}
			streamBuffer = incomingList.Dequeue();
		}
		ByteCountCurrentDispatch = streamBuffer.Length + 3;
		bool result = DeserializeMessageAndCallback(streamBuffer);
		PeerBase.MessageBufferPoolPut(streamBuffer);
		return result;
	}

	internal override bool SendOutgoingCommands()
	{
		if (peerConnectionState == ConnectionStateValue.Disconnected)
		{
			return false;
		}
		if (!PhotonSocket.Connected)
		{
			return false;
		}
		timeLastSendOutgoing = base.timeInt;
		if (peerConnectionState == ConnectionStateValue.Connected && base.timeInt - lastPingResult > base.timePingInterval)
		{
			SendPing();
		}
		lock (outgoingStream)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < outgoingStream.Count; i++)
			{
				StreamBuffer streamBuffer = outgoingStream[i];
				SendData(streamBuffer.GetBuffer(), streamBuffer.Length);
				num2 += streamBuffer.Length;
				num++;
				PeerBase.MessageBufferPoolPut(streamBuffer);
				if (num2 >= base.mtu)
				{
					break;
				}
			}
			outgoingStream.RemoveRange(0, num);
			outgoingCommandsInStream -= num;
			return outgoingStream.Count > 0;
		}
	}

	internal override bool SendAcksOnly()
	{
		if (PhotonSocket == null || !PhotonSocket.Connected)
		{
			return false;
		}
		if (peerConnectionState == ConnectionStateValue.Connected && base.timeInt - lastPingResult > base.timePingInterval)
		{
			SendPing();
		}
		return false;
	}

	internal override bool EnqueuePhotonMessage(StreamBuffer opBytes, SendOptions sendParams)
	{
		return EnqueueMessageAsPayload(sendParams.DeliveryMode, opBytes, sendParams.Channel);
	}

	internal bool EnqueueMessageAsPayload(DeliveryMode deliveryMode, StreamBuffer opMessage, byte channelId)
	{
		if (opMessage == null)
		{
			return false;
		}
		if (DoFraming)
		{
			byte[] buffer = opMessage.GetBuffer();
			int targetOffset = 1;
			Protocol.Serialize(opMessage.Length, buffer, ref targetOffset);
			buffer[5] = channelId;
			switch (deliveryMode)
			{
			case DeliveryMode.Unreliable:
				buffer[6] = 0;
				break;
			case DeliveryMode.Reliable:
				buffer[6] = 1;
				break;
			case DeliveryMode.UnreliableUnsequenced:
				buffer[6] = 2;
				break;
			case DeliveryMode.ReliableUnsequenced:
				buffer[6] = 3;
				break;
			default:
				throw new ArgumentOutOfRangeException("DeliveryMode", deliveryMode, null);
			}
		}
		lock (outgoingStream)
		{
			outgoingStream.Add(opMessage);
			outgoingCommandsInStream++;
		}
		int num = (ByteCountLastOperation = opMessage.Length);
		if (base.TrafficStatsEnabled)
		{
			switch (deliveryMode)
			{
			case DeliveryMode.Unreliable:
			case DeliveryMode.UnreliableUnsequenced:
				base.TrafficStatsOutgoing.CountUnreliableOpCommand(num);
				break;
			case DeliveryMode.Reliable:
			case DeliveryMode.ReliableUnsequenced:
				base.TrafficStatsOutgoing.CountReliableOpCommand(num);
				break;
			}
			base.TrafficStatsGameLevel.CountOperation(num);
		}
		return true;
	}

	internal void SendPing()
	{
		int value = (lastPingResult = base.timeInt);
		if (!DoFraming)
		{
			SendOptions sendOptions = new SendOptions
			{
				DeliveryMode = DeliveryMode.Reliable
			};
			ParameterDictionary parameterDictionary = new ParameterDictionary();
			parameterDictionary.Add(1, value);
			StreamBuffer streamBuffer = SerializeOperationToMessage(PhotonCodes.Ping, parameterDictionary, EgMessageType.InternalOperationRequest, sendOptions.Encrypt);
			SendData(streamBuffer.GetBuffer(), streamBuffer.Length);
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsOutgoing.CountControlCommand(streamBuffer.Length);
			}
			PeerBase.MessageBufferPoolPut(streamBuffer);
		}
		else
		{
			int targetOffset = 1;
			Protocol.Serialize(value, pingRequest, ref targetOffset);
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsOutgoing.CountControlCommand(pingRequest.Length);
			}
			SendData(pingRequest, pingRequest.Length);
		}
	}

	internal void SendData(byte[] data, int length)
	{
		try
		{
			bytesOut += length;
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsOutgoing.TotalPacketCount++;
				base.TrafficStatsOutgoing.TotalCommandsInPackets++;
			}
			if (base.NetworkSimulationSettings.IsSimulationEnabled)
			{
				byte[] array = new byte[length];
				Buffer.BlockCopy(data, 0, array, 0, length);
				SendNetworkSimulated(array);
			}
			else
			{
				PhotonSocket.Send(data, length);
			}
		}
		catch (Exception ex)
		{
			if ((int)base.debugOut >= 1)
			{
				base.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
			}
			SupportClass.WriteStackTrace(ex);
		}
	}

	internal override void ReceiveIncomingCommands(byte[] inbuff, int dataLength)
	{
		if (inbuff == null)
		{
			if ((int)base.debugOut >= 1)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "checkAndQueueIncomingCommands() inBuff: null");
			}
			return;
		}
		timestampOfLastReceive = base.timeInt;
		bytesIn += dataLength + 7;
		if (base.TrafficStatsEnabled)
		{
			base.TrafficStatsIncoming.TotalPacketCount++;
			base.TrafficStatsIncoming.TotalCommandsInPackets++;
		}
		if (inbuff[0] == 243)
		{
			byte b = (byte)(inbuff[1] & 0x7F);
			byte b2 = inbuff[2];
			if (b != 7 || b2 != PhotonCodes.Ping)
			{
				StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
				streamBuffer.Write(inbuff, 0, dataLength);
				streamBuffer.Position = 0;
				lock (incomingList)
				{
					incomingList.Enqueue(streamBuffer);
					return;
				}
			}
			DeserializeMessageAndCallback(new StreamBuffer(inbuff));
		}
		else if (inbuff[0] == 240)
		{
			base.TrafficStatsIncoming.CountControlCommand(dataLength);
			ReadPingResult(inbuff);
		}
		else if ((int)base.debugOut >= 1 && dataLength > 0)
		{
			EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() MagicNumber should be 0xF0 or 0xF3. Is: " + inbuff[0] + " dataLength: " + dataLength);
		}
	}

	private void ReadPingResult(byte[] inbuff)
	{
		int value = 0;
		int value2 = 0;
		int offset = 1;
		Protocol.Deserialize(out value, inbuff, ref offset);
		Protocol.Deserialize(out value2, inbuff, ref offset);
		lastRoundTripTime = base.timeInt - value2;
		if (!serverTimeOffsetIsAvailable)
		{
			roundTripTime = lastRoundTripTime;
		}
		UpdateRoundTripTimeAndVariance(lastRoundTripTime);
		if (!serverTimeOffsetIsAvailable)
		{
			serverTimeOffset = value + (lastRoundTripTime >> 1) - base.timeInt;
			serverTimeOffsetIsAvailable = true;
		}
	}

	protected internal void ReadPingResult(OperationResponse operationResponse)
	{
		int num = (int)operationResponse.Parameters[2];
		int num2 = (int)operationResponse.Parameters[1];
		lastRoundTripTime = base.timeInt - num2;
		if (!serverTimeOffsetIsAvailable)
		{
			roundTripTime = lastRoundTripTime;
		}
		UpdateRoundTripTimeAndVariance(lastRoundTripTime);
		if (!serverTimeOffsetIsAvailable)
		{
			serverTimeOffset = num + (lastRoundTripTime >> 1) - base.timeInt;
			serverTimeOffsetIsAvailable = true;
		}
	}
}
