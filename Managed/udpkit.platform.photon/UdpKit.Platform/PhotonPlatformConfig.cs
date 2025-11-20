using System;
using System.Net;
using System.Reflection;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UdpKit.Platform.Photon;
using UnityEngine;

namespace UdpKit.Platform;

public class PhotonPlatformConfig
{
	public PhotonRegion Region;

	public bool UsePunchThrough = false;

	internal int BackgroundConnectionTimeout;

	internal RuntimePlatform? CurrentPlatform = null;

	internal uint ConnectionRequestAttempts = 5u;

	internal uint ConnectionLANRequestAttempts = 5u;

	private string _appID;

	private string _regionMaster;

	private float _roomCreateTimeout;

	private float _roomJoinTimeout;

	private object _boltRuntimeSettingsInstance;

	private Type _boltRuntimeSettings;

	private Type _boltNetwork;

	private const BindingFlags STATIC = BindingFlags.Static | BindingFlags.Public;

	private const BindingFlags INSTANCE = BindingFlags.Instance | BindingFlags.Public;

	public string AppId
	{
		get
		{
			return _appID;
		}
		set
		{
			if (IsAppId(value))
			{
				_appID = value;
				return;
			}
			throw new ArgumentException("Please, inform a valid Photon App ID");
		}
	}

	public AuthenticationValues AuthenticationValues { get; set; }

	public ConnectionProtocol ConnectionProtocol { get; set; } = ConnectionProtocol.Udp;

	public SerializationProtocol SerializationProtocol { get; set; } = SerializationProtocol.GpBinaryV18;

	public IPEndPoint ForceExternalEndPoint { get; set; } = null;

	public float RoomCreateTimeout
	{
		get
		{
			return _roomCreateTimeout;
		}
		set
		{
			_roomCreateTimeout = Math.Max(10f, value);
		}
	}

	public float RoomJoinTimeout
	{
		get
		{
			return _roomJoinTimeout;
		}
		set
		{
			_roomJoinTimeout = Math.Max(10f, value);
		}
	}

	public int MaxConnections => (int)_boltNetwork.GetProperty("maxConnections", BindingFlags.Static | BindingFlags.Public).GetGetMethod().Invoke(null, NO_ARGS);

	public string CustomSTUNServer { get; set; }

	private object[] NO_ARGS => new object[0];

	public PhotonPlatformConfig()
	{
		InitDefaults();
	}

	internal PhotonPlatformConfig InitDefaults()
	{
		_boltNetwork = Type.GetType("Photon.Bolt.BoltNetwork, bolt");
		_boltRuntimeSettings = Type.GetType("Photon.Bolt.BoltRuntimeSettings, bolt");
		_boltRuntimeSettingsInstance = _boltRuntimeSettings.GetProperty("instance", BindingFlags.Static | BindingFlags.Public).GetGetMethod().Invoke(null, NO_ARGS);
		AppId = (string)_boltRuntimeSettings.GetField("photonAppId", BindingFlags.Instance | BindingFlags.Public).GetValue(_boltRuntimeSettingsInstance);
		int num = (int)_boltRuntimeSettings.GetField("photonCloudRegionIndex", BindingFlags.Instance | BindingFlags.Public).GetValue(_boltRuntimeSettingsInstance);
		string[] array = (string[])_boltRuntimeSettings.GetProperty("photonCloudRegionsId", BindingFlags.Static | BindingFlags.Public).GetGetMethod().Invoke(null, NO_ARGS);
		_regionMaster = array[num];
		Region = PhotonRegion.GetRegion(_regionMaster);
		UsePunchThrough = (bool)_boltRuntimeSettings.GetField("photonUsePunch", BindingFlags.Instance | BindingFlags.Public).GetValue(_boltRuntimeSettingsInstance);
		RoomCreateTimeout = (float)_boltRuntimeSettings.GetField("RoomCreateTimeout", BindingFlags.Instance | BindingFlags.Public).GetValue(_boltRuntimeSettingsInstance);
		RoomJoinTimeout = (float)_boltRuntimeSettings.GetField("RoomJoinTimeout", BindingFlags.Instance | BindingFlags.Public).GetValue(_boltRuntimeSettingsInstance);
		BackgroundConnectionTimeout = 60000;
		AuthenticationValues = null;
		return this;
	}

	internal void UpdateBestRegion(PhotonRegion region)
	{
		try
		{
			_boltRuntimeSettings.GetMethod("UpdateBestRegion", BindingFlags.Instance | BindingFlags.Public).Invoke(_boltRuntimeSettingsInstance, new object[1] { region });
		}
		catch (Exception ex)
		{
			UdpLog.Error("Error while try to update Best Region settings: {0}", ex.ToString());
		}
	}

	internal bool IsAppId(string val)
	{
		try
		{
			new Guid(val);
		}
		catch
		{
			return false;
		}
		return true;
	}
}
