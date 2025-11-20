#define DEBUG
using System;
using Photon.Bolt.Utils;
using UdpKit;

namespace Photon.Bolt;

internal class NetworkProperty_ProtocolToken : NetworkProperty
{
	public override void SetDynamic(NetworkObj obj, object value)
	{
		IProtocolToken protocolToken = (IProtocolToken)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].ProtocolToken, protocolToken))
		{
			obj.Storage.Values[obj[this]].ProtocolToken.Release();
			obj.Storage.Values[obj[this]].ProtocolToken = protocolToken;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].ProtocolToken;
	}

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		if (storage.Values[obj[this]].ProtocolToken == null)
		{
			return "NULL";
		}
		return storage.Values[obj[this]].ProtocolToken.ToString();
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		try
		{
			packet.WriteToken(storage.Values[obj[this]].ProtocolToken);
			return true;
		}
		catch (Exception exception)
		{
			BoltLog.Error("User code threw exception while serializing protocol token");
			BoltLog.Exception(exception);
			return false;
		}
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].ProtocolToken.Release();
		storage.Values[obj[this]].ProtocolToken = packet.ReadToken();
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].ProtocolToken.Release();
		storage.Values[obj[this]].ProtocolToken = other.Values[obj[this]].ProtocolToken;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].ProtocolToken == storage2.Values[obj[this]].ProtocolToken;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}
}
