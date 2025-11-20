namespace Steamworks.Data;

public struct Socket
{
	internal uint Id;

	public SocketManager Manager
	{
		get
		{
			return SteamNetworkingSockets.GetSocketManager(Id);
		}
		set
		{
			SteamNetworkingSockets.SetSocketManager(Id, value);
		}
	}

	public override string ToString()
	{
		return Id.ToString();
	}

	public static implicit operator Socket(uint value)
	{
		return new Socket
		{
			Id = value
		};
	}

	public static implicit operator uint(Socket value)
	{
		return value.Id;
	}

	public bool Close()
	{
		return SteamNetworkingSockets.Internal.CloseListenSocket(Id);
	}
}
