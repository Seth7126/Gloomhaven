using System.Text;

namespace Photon.Bolt;

[Documentation]
public struct InstantiateFlags
{
	public static readonly InstantiateFlags ZERO = new InstantiateFlags(0);

	public static readonly InstantiateFlags TAKE_CONTROL = new InstantiateFlags(1);

	public static readonly InstantiateFlags ASSIGN_CONTROL = new InstantiateFlags(2);

	private readonly int bits;

	public bool IsZero => bits == 0;

	private InstantiateFlags(int val)
	{
		bits = val;
	}

	public override int GetHashCode()
	{
		return bits;
	}

	public override bool Equals(object obj)
	{
		if (obj is InstantiateFlags)
		{
			return bits == ((InstantiateFlags)obj).bits;
		}
		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append("InstantiateFlags");
		if ((bits & 1) == 1)
		{
			stringBuilder.Append(" TAKE_CONTROL");
		}
		if ((bits & 2) == 2)
		{
			stringBuilder.Append(" ASSIGN_CONTROL");
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public static implicit operator bool(InstantiateFlags a)
	{
		return a.bits != 0;
	}

	public static explicit operator int(InstantiateFlags a)
	{
		return a.bits;
	}

	public static explicit operator InstantiateFlags(int a)
	{
		return new InstantiateFlags(a);
	}

	public static InstantiateFlags operator &(InstantiateFlags a, InstantiateFlags b)
	{
		return new InstantiateFlags(a.bits & b.bits);
	}

	public static InstantiateFlags operator |(InstantiateFlags a, InstantiateFlags b)
	{
		return new InstantiateFlags(a.bits | b.bits);
	}

	public static InstantiateFlags operator ^(InstantiateFlags a, InstantiateFlags b)
	{
		return new InstantiateFlags(a.bits ^ b.bits);
	}

	public static InstantiateFlags operator ~(InstantiateFlags a)
	{
		return new InstantiateFlags(~a.bits);
	}

	public static bool operator ==(InstantiateFlags a, InstantiateFlags b)
	{
		return a.bits == b.bits;
	}

	public static bool operator !=(InstantiateFlags a, InstantiateFlags b)
	{
		return a.bits != b.bits;
	}
}
