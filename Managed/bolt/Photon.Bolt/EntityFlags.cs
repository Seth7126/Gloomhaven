using System.Text;

namespace Photon.Bolt;

[Documentation]
public struct EntityFlags
{
	public static readonly EntityFlags ZERO = new EntityFlags(0);

	public static readonly EntityFlags HAS_CONTROL = new EntityFlags(1);

	public static readonly EntityFlags PERSIST_ON_LOAD = new EntityFlags(2);

	public static readonly EntityFlags ATTACHED = new EntityFlags(4);

	public static readonly EntityFlags CONTROLLER_LOCAL_PREDICTION = new EntityFlags(8);

	public static readonly EntityFlags SCENE_OBJECT = new EntityFlags(16);

	private readonly int bits;

	public bool IsZero => bits == 0;

	private EntityFlags(int val)
	{
		bits = val;
	}

	public override int GetHashCode()
	{
		return bits;
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityFlags)
		{
			return bits == ((EntityFlags)obj).bits;
		}
		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append("EntityFlags");
		if ((bits & 1) == 1)
		{
			stringBuilder.Append(" HAS_CONTROL");
		}
		if ((bits & 2) == 2)
		{
			stringBuilder.Append(" PERSIST_ON_LOAD");
		}
		if ((bits & 4) == 4)
		{
			stringBuilder.Append(" ATTACHED");
		}
		if ((bits & 8) == 8)
		{
			stringBuilder.Append(" CONTROLLER_LOCAL_PREDICTION");
		}
		if ((bits & 0x10) == 16)
		{
			stringBuilder.Append(" SCENE_OBJECT");
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public static implicit operator bool(EntityFlags a)
	{
		return a.bits != 0;
	}

	public static explicit operator int(EntityFlags a)
	{
		return a.bits;
	}

	public static explicit operator EntityFlags(int a)
	{
		return new EntityFlags(a);
	}

	public static EntityFlags operator &(EntityFlags a, EntityFlags b)
	{
		return new EntityFlags(a.bits & b.bits);
	}

	public static EntityFlags operator |(EntityFlags a, EntityFlags b)
	{
		return new EntityFlags(a.bits | b.bits);
	}

	public static EntityFlags operator ^(EntityFlags a, EntityFlags b)
	{
		return new EntityFlags(a.bits ^ b.bits);
	}

	public static EntityFlags operator ~(EntityFlags a)
	{
		return new EntityFlags(~a.bits);
	}

	public static bool operator ==(EntityFlags a, EntityFlags b)
	{
		return a.bits == b.bits;
	}

	public static bool operator !=(EntityFlags a, EntityFlags b)
	{
		return a.bits != b.bits;
	}
}
