using System;

namespace UdpKit;

public class UdpSessionImpl : UdpSession
{
	internal uint _lastSeen;

	internal Guid _id;

	internal UdpEndPoint _wanEndPoint;

	internal UdpEndPoint _lanEndPoint;

	internal UdpSessionSource _source;

	internal int _connectionsMax;

	internal int _connectionsCurrent;

	internal string _hostName;

	internal byte[] _hostData;

	internal object _hostObject;

	internal bool _hostIsDedicated;

	public override Guid Id
	{
		get
		{
			return _id;
		}
		internal set
		{
			_id = value;
		}
	}

	public override UdpSessionSource Source
	{
		get
		{
			return _source;
		}
		internal set
		{
			_source = value;
		}
	}

	public override UdpEndPoint WanEndPoint
	{
		get
		{
			return _wanEndPoint;
		}
		internal set
		{
			_wanEndPoint = value;
		}
	}

	public override UdpEndPoint LanEndPoint
	{
		get
		{
			return _lanEndPoint;
		}
		internal set
		{
			_lanEndPoint = value;
		}
	}

	public override int ConnectionsMax
	{
		get
		{
			return _connectionsMax;
		}
		internal set
		{
			_connectionsMax = value;
		}
	}

	public override int ConnectionsCurrent
	{
		get
		{
			return _connectionsCurrent;
		}
		internal set
		{
			_connectionsCurrent = value;
		}
	}

	public override string HostName
	{
		get
		{
			return _hostName;
		}
		internal set
		{
			_hostName = value;
		}
	}

	public override bool IsDedicatedServer
	{
		get
		{
			return _hostIsDedicated;
		}
		internal set
		{
			_hostIsDedicated = value;
		}
	}

	public override bool HasWan => _wanEndPoint.IsWan;

	public override bool HasLan => _lanEndPoint.IsLan;

	public override byte[] HostData
	{
		get
		{
			return _hostData;
		}
		internal set
		{
			_hostData = value;
		}
	}

	public override object HostObject
	{
		get
		{
			return _hostObject;
		}
		set
		{
			_hostObject = value;
		}
	}

	public static UdpSession Build(string hostID)
	{
		return new UdpSessionImpl
		{
			_hostName = hostID
		};
	}

	public override UdpSession Clone()
	{
		return (UdpSession)MemberwiseClone();
	}
}
