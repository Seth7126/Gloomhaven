#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using UdpKit.Utils;

namespace UdpKit.Platform.Photon.Puncher;

internal class StunManager
{
	private class PingTarget
	{
		public uint ID;

		public UdpEndPoint targetEndPoint;

		public long time;

		public uint attempts;
	}

	private enum StunState
	{
		NOT_READY,
		NOT_SENT,
		WAITING,
		RECEIVED,
		FAIL,
		END
	}

	private const int MAX_TRIES = 10;

	private const int INTERVAL_TIME = 500;

	private UdpPlatformSocket socket;

	private int id;

	private UdpEndPoint externalEndPoint = UdpEndPoint.Any;

	private UdpEndPoint internalEndPoint = UdpEndPoint.Any;

	private StunState state;

	private int requestCount;

	private int currentStunIndex;

	private Stopwatch stopWatch;

	private Dictionary<int, PingTarget> pingTargets;

	private readonly object pingTargetLock = new object();

	private readonly byte[] pingData = new byte[2] { 1, 5 };

	private readonly PhotonPlatformConfig photonConfig;

	private int pingTargetRemove;

	private const int STUN_HEADER_INDEX_DATA = 8;

	private static readonly int REQUEST_ID = 1501;

	private static readonly int RESPONSE_ID = 1501;

	private static readonly byte[] REQUEST_ID_DATA = BuildID(REQUEST_ID);

	private static readonly byte[] RESPONSE_ID_DATA = BuildID(RESPONSE_ID);

	private static readonly StunMsgHeader SIGNALING_REQUEST = BuildSignalingRequest();

	private static readonly StunMsgHeader SIGNALING_RESPONSE = BuildSignalingResponse();

	private static readonly byte[] SIGNALING_REQUEST_DATA = SIGNALING_REQUEST.Encode();

	private static readonly byte[] SIGNALING_RESPONSE_DATA = SIGNALING_RESPONSE.Encode();

	private static readonly byte[] SIGNALING_BINDING = new byte[8] { 1, 1, 0, 12, 33, 18, 164, 66 };

	public UdpEndPoint ExternalEndpoint
	{
		get
		{
			if (externalEndPoint == default(UdpEndPoint))
			{
				return UdpEndPoint.Any;
			}
			return externalEndPoint;
		}
	}

	public UdpEndPoint InternalEndPoint => internalEndPoint;

	public bool IsDone => state == StunState.END;

	public StunManager(PhotonPlatformConfig photonConfig)
	{
		Reset();
		this.photonConfig = photonConfig;
	}

	public void SetupSocket(UdpPlatformSocket socket)
	{
		this.socket = socket;
	}

	public void SetExternalInfo(UdpEndPoint endPoint)
	{
		externalEndPoint = endPoint;
		state = StunState.RECEIVED;
	}

	public void SetInternalEndPoint(UdpEndPoint endPoint)
	{
		internalEndPoint = endPoint;
		if (endPoint.IPv6)
		{
			SetExternalInfo(endPoint);
		}
	}

	public void RegisterPingTarget(int playerID, UdpEndPoint targetEndPoint)
	{
		if (pingTargets != null && !pingTargets.ContainsKey(playerID))
		{
			lock (pingTargetLock)
			{
				UdpLog.Debug("[NAT Punch] Ping Request from Player {0} with Target EndPoint {1}", playerID, targetEndPoint);
				pingTargets[playerID] = new PingTarget
				{
					ID = (uint)playerID,
					targetEndPoint = targetEndPoint,
					time = PrecisionTimer.GetCurrentTime(),
					attempts = 30u
				};
			}
		}
	}

	public void RemovePingTarget(int playerID)
	{
		if (pingTargets != null && pingTargets.ContainsKey(playerID))
		{
			UdpLog.Debug("[NAT Punch] Ping Request Cancel from Player {0}", playerID);
			lock (pingTargetLock)
			{
				pingTargets.Remove(playerID);
			}
		}
	}

	public bool RecvStun(byte[] buffer)
	{
		if (state == StunState.END || externalEndPoint != UdpEndPoint.Any || !IsSignalingMsg(buffer))
		{
			return false;
		}
		StunMsgHeader stunMsgHeader = new StunMsgHeader().Decode(buffer);
		if (ValidStunMsg(stunMsgHeader, StunMsgType.STUN_BINDING_RESPONSE, id))
		{
			SetExternalInfo(GetIpEndPoint(stunMsgHeader).ConvertToUdpEndPoint());
		}
		return true;
	}

	public void Reset()
	{
		id = new Random().Next(0, 1000);
		requestCount = 0;
		currentStunIndex = 0;
		stopWatch = new Stopwatch();
		socket = null;
		externalEndPoint = UdpEndPoint.Any;
		internalEndPoint = UdpEndPoint.Any;
		state = StunState.NOT_READY;
		pingTargets = new Dictionary<int, PingTarget>();
		UdpLog.Debug("[STUN] Reset configs");
	}

	public void Service()
	{
		switch (state)
		{
		case StunState.NOT_READY:
			if (socket != null && socket.IsBound && externalEndPoint == UdpEndPoint.Any)
			{
				state = StunState.NOT_SENT;
			}
			break;
		case StunState.NOT_SENT:
			RequestStunInfo();
			break;
		case StunState.WAITING:
			if (stopWatch.ElapsedMilliseconds > 500)
			{
				state = StunState.NOT_SENT;
			}
			break;
		case StunState.RECEIVED:
			UdpLog.Debug("[STUN] Request Success: {0}", externalEndPoint);
			state = StunState.END;
			break;
		case StunState.FAIL:
			UdpLog.Debug("[STUN] Request Fail, disabling NAT Punch");
			photonConfig.UsePunchThrough = false;
			state = StunState.END;
			break;
		}
		if (pingTargets == null)
		{
			return;
		}
		lock (pingTargetLock)
		{
			RemovePingTarget(pingTargetRemove);
			pingTargetRemove = -1;
			foreach (PingTarget value in pingTargets.Values)
			{
				if (value.time < PrecisionTimer.GetCurrentTime())
				{
					UdpLog.Debug("[NAT Punch] Ping {0} [{1}]", value.targetEndPoint, value.attempts);
					Ping(value.targetEndPoint);
					value.time = PrecisionTimer.GetCurrentTime() + 200;
					value.attempts--;
					if (value.attempts == 0)
					{
						pingTargetRemove = (int)value.ID;
					}
					break;
				}
			}
		}
	}

	private void RequestStunInfo()
	{
		if (requestCount > 10)
		{
			state = StunState.FAIL;
		}
		else if (StunServers.RefreshServers())
		{
			StunServers.StunServer stunServer = StunServers.ALL_SERVERS[currentStunIndex];
			if (stunServer != null)
			{
				UdpEndPoint udpEndPoint = (internalEndPoint.IPv6 ? stunServer.IPv6.ConvertToUdpEndPoint() : stunServer.IPv4.ConvertToUdpEndPoint());
				UdpLog.Debug("[STUN] Requesting Info to server {0}", udpEndPoint);
				socket.SendTo(BuildStunInfoRequest(id).Encode(), udpEndPoint);
				state = StunState.WAITING;
				requestCount++;
				currentStunIndex = (currentStunIndex + 1) % StunServers.ALL_SERVERS.Length;
				stopWatch.Reset();
				stopWatch.Start();
			}
			else
			{
				currentStunIndex = (currentStunIndex + 1) % StunServers.ALL_SERVERS.Length;
			}
		}
	}

	private void Ping(UdpEndPoint endPoint)
	{
		socket.SendTo(pingData, endPoint);
	}

	private bool IsSignalingMsg(byte[] msg)
	{
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		for (int i = 0; i < 8; i++)
		{
			flag &= msg[i] == SIGNALING_REQUEST_DATA[i];
			flag2 &= msg[i] == SIGNALING_RESPONSE_DATA[i];
			flag3 &= msg[i] == SIGNALING_BINDING[i];
			if (!flag && !flag2 && !flag3)
			{
				return false;
			}
		}
		return true;
	}

	private byte[] BuildPingRequest(int localId)
	{
		byte[] array = new StunMsgHeader(10u, 0u, 554869826u).Encode();
		array[8] = (byte)localId;
		return array;
	}

	private byte[] BuildPongResponse(int localId)
	{
		byte[] array = new StunMsgHeader(266u, 0u, 554869826u).Encode();
		array[8] = (byte)localId;
		return array;
	}

	private static StunMsgHeader BuildSignalingRequest()
	{
		return new StunMsgHeader(10u, 0u, 554869826u, REQUEST_ID_DATA);
	}

	private static StunMsgHeader BuildSignalingResponse()
	{
		return new StunMsgHeader(266u, 0u, 554869826u, RESPONSE_ID_DATA);
	}

	private static StunMsgHeader BuildStunInfoRequest(int id)
	{
		return new StunMsgHeader(1u, 0u, 554869826u, BuildID(id));
	}

	public IPEndPoint GetIpEndPoint(StunMsgHeader stun)
	{
		GetAddress(stun.buffer, 20, out var outPort, out var convertedAddress);
		string ipString = $"{convertedAddress[0]}.{convertedAddress[1]}.{convertedAddress[2]}.{convertedAddress[3]}";
		return new IPEndPoint(IPAddress.Parse(ipString), (int)outPort);
	}

	private void GetAddress(byte[] payload, int startDiff, out uint outPort, out byte[] convertedAddress)
	{
		byte[] array = new byte[2];
		byte[] array2 = new byte[2];
		byte[] array3 = new byte[8];
		GetBytes(payload, array, startDiff);
		GetBytes(payload, array2, startDiff + 2);
		GetBytes(payload, array3, startDiff + 4);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse((Array)array);
			Array.Reverse((Array)array2);
		}
		byte[] array4 = new byte[2];
		byte[] array5 = new byte[4];
		GetBytes(array3, array4, 2);
		GetBytes(array3, array5, 4);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse((Array)array4);
			Array.Reverse((Array)array5);
		}
		ushort num = (ushort)BitConverter.ToInt16(array4, 0);
		uint num2 = (uint)BitConverter.ToInt32(array5, 0);
		outPort = (uint)(num ^ 0x2112);
		uint value = num2 ^ 0x2112A442;
		convertedAddress = BitConverter.GetBytes(value);
		Array.Reverse((Array)convertedAddress);
		byte[] bytes = BitConverter.GetBytes(outPort);
		Array.Reverse((Array)bytes);
	}

	private bool ValidStunMsg(StunMsgHeader msg, StunMsgType type, int id)
	{
		if (msg.Type == (uint)type)
		{
			byte[] array = new byte[4];
			Array.ConstrainedCopy(msg.TS_ID, 8, array, 0, array.Length);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse((Array)array);
			}
			if (BitConverter.ToInt32(array, 0) == id)
			{
				return true;
			}
		}
		return false;
	}

	private static void GetBytes(byte[] src, byte[] dst, int start)
	{
		Buffer.BlockCopy(src, start, dst, 0, dst.Length);
	}

	private static byte[] BuildID(int id)
	{
		byte[] bytes = BitConverter.GetBytes(id);
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse((Array)bytes);
		}
		byte[] array = new byte[12];
		Array.ConstrainedCopy(bytes, 0, array, array.Length - bytes.Length, bytes.Length);
		return array;
	}
}
