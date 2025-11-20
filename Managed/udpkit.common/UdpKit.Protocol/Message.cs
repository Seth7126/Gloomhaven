#define DEBUG
using System;

namespace UdpKit.Protocol;

internal abstract class Message
{
	public const byte MESSAGE_HEADER = byte.MaxValue;

	private int _ptr;

	private byte[] _buffer;

	private byte _type;

	private bool _pack;

	public Guid GameId;

	public Guid PeerId;

	public Guid MessageId;

	public UdpEndPoint Sender;

	public UdpEndPoint Target;

	public uint SendTime;

	public uint Timeout;

	public uint Attempts;

	public Context Context;

	public bool Read => !Pack;

	public bool Pack => _pack;

	public Message()
	{
		MessageId = Guid.NewGuid();
	}

	public void Init(byte type)
	{
		_type = type;
	}

	public void InitBuffer(int ptr, byte[] buffer, bool pack)
	{
		_ptr = ptr;
		_pack = pack;
		_buffer = buffer;
	}

	public int Serialize(int ptr, byte[] buffer, bool pack)
	{
		InitBuffer(ptr, buffer, pack);
		return Serialize();
	}

	public int Serialize()
	{
		Serialize(ref _type);
		Serialize(ref GameId);
		Serialize(ref PeerId);
		Serialize(ref MessageId);
		OnSerialize();
		_buffer = null;
		return _ptr;
	}

	public override string ToString()
	{
		return GetType().Name;
	}

	protected void Create<T>(ref T value) where T : class, new()
	{
		if (_pack)
		{
			UdpAssert.Assert(value != null);
		}
		else
		{
			value = new T();
		}
	}

	protected void Serialize<T>(ref T value) where T : struct
	{
		UdpAssert.Assert(typeof(T).IsEnum);
		if (_pack)
		{
			Blit.PackI32(_buffer, ref _ptr, EnumToInt(value));
		}
		else
		{
			value = IntToEnum<T>(Blit.ReadI32(_buffer, ref _ptr));
		}
	}

	protected void Serialize(ref UdpSession session)
	{
		UdpSessionImpl value = ((session != null) ? ((UdpSessionImpl)session) : new UdpSessionImpl());
		Create(ref value);
		Serialize(ref value._id);
		Serialize(ref value._hostName);
		Serialize(ref value._hostData);
		Serialize(ref value._lanEndPoint);
		Serialize(ref value._wanEndPoint);
		Serialize(ref value._connectionsMax);
		Serialize(ref value._connectionsCurrent);
		Serialize(ref value._hostIsDedicated);
		Serialize(ref value._source);
		session = value;
	}

	protected void Serialize(ref NatFeatures features)
	{
		Create(ref features);
		Serialize(ref features.WanEndPoint);
		Serialize(ref features.AllowsUnsolicitedTraffic);
		Serialize(ref features.SupportsHairpinTranslation);
		Serialize(ref features.SupportsEndPointPreservation);
		Serialize(ref features.LanEndPoint);
	}

	protected void Serialize(ref bool value)
	{
		if (_pack)
		{
			if (HasSpace(1))
			{
				Blit.PackBool(_buffer, ref _ptr, value);
			}
		}
		else
		{
			value = Blit.ReadBool(_buffer, ref _ptr);
		}
	}

	protected void Serialize(ref byte value)
	{
		if (_pack)
		{
			if (HasSpace(1))
			{
				Blit.PackByte(_buffer, ref _ptr, value);
			}
		}
		else
		{
			value = Blit.ReadByte(_buffer, ref _ptr);
		}
	}

	protected void Serialize(ref int value)
	{
		if (_pack)
		{
			if (HasSpace(4))
			{
				Blit.PackI32(_buffer, ref _ptr, value);
			}
		}
		else
		{
			value = Blit.ReadI32(_buffer, ref _ptr);
		}
	}

	protected void Serialize(ref uint value)
	{
		if (_pack)
		{
			if (HasSpace(4))
			{
				Blit.PackU32(_buffer, ref _ptr, value);
			}
		}
		else
		{
			value = Blit.ReadU32(_buffer, ref _ptr);
		}
	}

	protected void Serialize(ref string value)
	{
		if (_pack)
		{
			if (HasSpace(Blit.GetStringSize(value)))
			{
				Blit.PackString(_buffer, ref _ptr, value);
			}
		}
		else
		{
			value = Blit.ReadString(_buffer, ref _ptr);
		}
	}

	protected void Serialize(ref byte[] value)
	{
		if (_pack)
		{
			if (HasSpace(Blit.GetBytesPrefixSize(value)))
			{
				Blit.PackBytesPrefix(_buffer, ref _ptr, value);
			}
		}
		else
		{
			value = Blit.ReadBytesPrefix(_buffer, ref _ptr);
		}
	}

	protected void Serialize(ref Guid value)
	{
		if (_pack)
		{
			if (HasSpace(16))
			{
				Blit.PackGuid(_buffer, ref _ptr, value);
			}
		}
		else
		{
			value = Blit.ReadGuid(_buffer, ref _ptr);
		}
	}

	protected void Serialize(ref UdpEndPoint value)
	{
		if (_pack)
		{
			if (HasSpace(6))
			{
				Blit.PackEndPoint(_buffer, ref _ptr, value);
			}
		}
		else
		{
			value = Blit.ReadEndPoint(_buffer, ref _ptr);
		}
	}

	protected virtual void OnSerialize()
	{
	}

	private bool HasSpace(int bytes)
	{
		if (_ptr >= 0 && _ptr + bytes <= _buffer.Length)
		{
			return true;
		}
		_ptr = -1;
		return false;
	}

	private static int EnumToInt<T>(T value) where T : struct
	{
		return (int)(object)value;
	}

	private static T IntToEnum<T>(int value) where T : struct
	{
		return (T)(object)value;
	}
}
