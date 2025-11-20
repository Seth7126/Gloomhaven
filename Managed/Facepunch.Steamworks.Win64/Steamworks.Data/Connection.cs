using System;
using System.Text;

namespace Steamworks.Data;

public struct Connection : IEquatable<Connection>
{
	public uint Id { get; set; }

	public long UserData
	{
		get
		{
			return SteamNetworkingSockets.Internal.GetConnectionUserData(this);
		}
		set
		{
			SteamNetworkingSockets.Internal.SetConnectionUserData(this, value);
		}
	}

	public string ConnectionName
	{
		get
		{
			if (!SteamNetworkingSockets.Internal.GetConnectionName(this, out var pszName))
			{
				return "ERROR";
			}
			return pszName;
		}
		set
		{
			SteamNetworkingSockets.Internal.SetConnectionName(this, value);
		}
	}

	public bool Equals(Connection other)
	{
		return Id == other.Id;
	}

	public override bool Equals(object obj)
	{
		return obj is Connection connection && Id == connection.Id;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public override string ToString()
	{
		return Id.ToString();
	}

	public static implicit operator Connection(uint value)
	{
		return new Connection
		{
			Id = value
		};
	}

	public static implicit operator uint(Connection value)
	{
		return value.Id;
	}

	public static bool operator ==(Connection value1, Connection value2)
	{
		return value1.Equals(value2);
	}

	public static bool operator !=(Connection value1, Connection value2)
	{
		return !value1.Equals(value2);
	}

	public Result Accept()
	{
		return SteamNetworkingSockets.Internal.AcceptConnection(this);
	}

	public bool Close(bool linger = false, int reasonCode = 0, string debugString = "Closing Connection")
	{
		return SteamNetworkingSockets.Internal.CloseConnection(this, reasonCode, debugString, linger);
	}

	public unsafe Result SendMessage(IntPtr ptr, int size, SendType sendType = SendType.Reliable)
	{
		if (ptr == IntPtr.Zero)
		{
			throw new ArgumentNullException("ptr");
		}
		if (size == 0)
		{
			throw new ArgumentException("`size` cannot be zero", "size");
		}
		IntPtr intPtr = BufferManager.Get(size, 1);
		Buffer.MemoryCopy((void*)ptr, (void*)intPtr, size, size);
		NetMsg* ptr2 = SteamNetworkingUtils.AllocateMessage();
		ptr2->Connection = this;
		ptr2->Flags = sendType;
		ptr2->DataPtr = intPtr;
		ptr2->DataSize = size;
		ptr2->FreeDataPtr = BufferManager.FreeFunctionPointer;
		long num = 0L;
		SteamNetworkingSockets.Internal.SendMessages(1, &ptr2, &num);
		return (num >= 0) ? Result.OK : ((Result)(-num));
	}

	public unsafe Result SendMessage(byte[] data, SendType sendType = SendType.Reliable)
	{
		fixed (byte* ptr = data)
		{
			return SendMessage((IntPtr)ptr, data.Length, sendType);
		}
	}

	public unsafe Result SendMessage(byte[] data, int offset, int length, SendType sendType = SendType.Reliable)
	{
		fixed (byte* ptr = data)
		{
			return SendMessage((IntPtr)ptr + offset, length, sendType);
		}
	}

	public Result SendMessage(string str, SendType sendType = SendType.Reliable)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		return SendMessage(bytes, sendType);
	}

	public Result Flush()
	{
		return SteamNetworkingSockets.Internal.FlushMessagesOnConnection(this);
	}

	public string DetailedStatus()
	{
		if (SteamNetworkingSockets.Internal.GetDetailedConnectionStatus(this, out var pszBuf) != 0)
		{
			return null;
		}
		return pszBuf;
	}

	public ConnectionStatus QuickStatus()
	{
		ConnectionStatus pStats = default(ConnectionStatus);
		SteamNetworkingSockets.Internal.GetQuickConnectionStatus(this, ref pStats);
		return pStats;
	}
}
