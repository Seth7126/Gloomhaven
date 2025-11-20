using System.Text;

namespace Photon.Bolt;

[Documentation]
public struct ProxyFlags
{
	public static readonly ProxyFlags ZERO = new ProxyFlags(0);

	public static readonly ProxyFlags CREATE_REQUESTED = new ProxyFlags(1);

	public static readonly ProxyFlags CREATE_DONE = new ProxyFlags(2);

	public static readonly ProxyFlags DESTROY_REQUESTED = new ProxyFlags(4);

	public static readonly ProxyFlags DESTROY_PENDING = new ProxyFlags(8);

	public static readonly ProxyFlags DESTROY_IGNORE = new ProxyFlags(16);

	public static readonly ProxyFlags IDLE = new ProxyFlags(32);

	public static readonly ProxyFlags FORCE_SYNC = new ProxyFlags(64);

	private readonly int bits;

	public bool IsZero => bits == 0;

	private ProxyFlags(int val)
	{
		bits = val;
	}

	public override int GetHashCode()
	{
		return bits;
	}

	public override bool Equals(object obj)
	{
		if (obj is ProxyFlags)
		{
			return bits == ((ProxyFlags)obj).bits;
		}
		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append("ProxyFlags");
		if ((bits & 1) == 1)
		{
			stringBuilder.Append(" CREATE_REQUESTED");
		}
		if ((bits & 2) == 2)
		{
			stringBuilder.Append(" CREATE_DONE");
		}
		if ((bits & 4) == 4)
		{
			stringBuilder.Append(" DESTROY_REQUESTED");
		}
		if ((bits & 8) == 8)
		{
			stringBuilder.Append(" DESTROY_PENDING");
		}
		if ((bits & 0x10) == 16)
		{
			stringBuilder.Append(" DESTROY_IGNORE");
		}
		if ((bits & 0x20) == 32)
		{
			stringBuilder.Append(" IDLE");
		}
		if ((bits & 0x40) == 64)
		{
			stringBuilder.Append(" FORCE_SYNC");
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public static implicit operator bool(ProxyFlags a)
	{
		return a.bits != 0;
	}

	public static explicit operator int(ProxyFlags a)
	{
		return a.bits;
	}

	public static explicit operator ProxyFlags(int a)
	{
		return new ProxyFlags(a);
	}

	public static ProxyFlags operator &(ProxyFlags a, ProxyFlags b)
	{
		return new ProxyFlags(a.bits & b.bits);
	}

	public static ProxyFlags operator |(ProxyFlags a, ProxyFlags b)
	{
		return new ProxyFlags(a.bits | b.bits);
	}

	public static ProxyFlags operator ^(ProxyFlags a, ProxyFlags b)
	{
		return new ProxyFlags(a.bits ^ b.bits);
	}

	public static ProxyFlags operator ~(ProxyFlags a)
	{
		return new ProxyFlags(~a.bits);
	}

	public static bool operator ==(ProxyFlags a, ProxyFlags b)
	{
		return a.bits == b.bits;
	}

	public static bool operator !=(ProxyFlags a, ProxyFlags b)
	{
		return a.bits != b.bits;
	}
}
