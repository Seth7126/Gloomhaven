#define DEBUG
using System.Collections.Generic;
using Photon.Bolt.Utils;
using UdpKit;
using UdpKit.Platform.Photon;

namespace Photon.Bolt;

public sealed class PhotonRoomProperties : IProtocolToken, IPhotonRoomPropertiesInternal
{
	public bool IsOpen { get; set; }

	public bool IsVisible { get; set; }

	public Dictionary<object, object> CustomRoomProperties { get; }

	public HashSet<string> CustomRoomPropertiesInLobby { get; }

	public object this[string key]
	{
		get
		{
			if (CustomRoomProperties.ContainsKey(key))
			{
				return CustomRoomProperties[key];
			}
			return null;
		}
		set
		{
			AddRoomProperty(key, value);
		}
	}

	public PhotonRoomProperties()
	{
		CustomRoomProperties = new Dictionary<object, object>();
		CustomRoomPropertiesInLobby = new HashSet<string>();
		IsOpen = true;
		IsVisible = true;
	}

	public bool AddRoomProperty(string key, object value, bool showInLobby = true)
	{
		if (!UdpSessionFilter.IsValid(value))
		{
			BoltLog.Warn("Invalid value type {0}", value.GetType());
			return false;
		}
		CustomRoomProperties[key] = value;
		if (showInLobby)
		{
			CustomRoomPropertiesInLobby.Add(key);
		}
		else
		{
			CustomRoomPropertiesInLobby.Remove(key);
		}
		return true;
	}

	public bool RemoveRoomProperty(string key)
	{
		CustomRoomPropertiesInLobby.Remove(key);
		return CustomRoomProperties.Remove(key);
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteBool(IsOpen);
		packet.WriteBool(IsVisible);
	}

	public void Read(UdpPacket packet)
	{
		IsOpen = packet.ReadBool();
		IsVisible = packet.ReadBool();
	}
}
