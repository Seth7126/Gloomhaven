using System.Text;

namespace Photon.Bolt;

[Documentation]
public struct CommandFlags
{
	public static readonly CommandFlags ZERO = new CommandFlags(0);

	public static readonly CommandFlags HAS_EXECUTED = new CommandFlags(1);

	public static readonly CommandFlags SEND_STATE = new CommandFlags(2);

	public static readonly CommandFlags SEND_STATE_PERFORMED = new CommandFlags(4);

	public static readonly CommandFlags CORRECTION_RECEIVED = new CommandFlags(8);

	public static readonly CommandFlags DISPOSE = new CommandFlags(16);

	public static readonly CommandFlags MISSING = new CommandFlags(32);

	private readonly int bits;

	public bool IsZero => bits == 0;

	private CommandFlags(int val)
	{
		bits = val;
	}

	public override int GetHashCode()
	{
		return bits;
	}

	public override bool Equals(object obj)
	{
		if (obj is CommandFlags)
		{
			return bits == ((CommandFlags)obj).bits;
		}
		return false;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append("CommandFlags");
		if ((bits & 1) == 1)
		{
			stringBuilder.Append(" HAS_EXECUTED");
		}
		if ((bits & 2) == 2)
		{
			stringBuilder.Append(" SEND_STATE");
		}
		if ((bits & 4) == 4)
		{
			stringBuilder.Append(" SEND_STATE_PERFORMED");
		}
		if ((bits & 8) == 8)
		{
			stringBuilder.Append(" CORRECTION_RECEIVED");
		}
		if ((bits & 0x10) == 16)
		{
			stringBuilder.Append(" DISPOSE");
		}
		if ((bits & 0x20) == 32)
		{
			stringBuilder.Append(" MISSING");
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public static implicit operator bool(CommandFlags a)
	{
		return a.bits != 0;
	}

	public static explicit operator int(CommandFlags a)
	{
		return a.bits;
	}

	public static explicit operator CommandFlags(int a)
	{
		return new CommandFlags(a);
	}

	public static CommandFlags operator &(CommandFlags a, CommandFlags b)
	{
		return new CommandFlags(a.bits & b.bits);
	}

	public static CommandFlags operator |(CommandFlags a, CommandFlags b)
	{
		return new CommandFlags(a.bits | b.bits);
	}

	public static CommandFlags operator ^(CommandFlags a, CommandFlags b)
	{
		return new CommandFlags(a.bits ^ b.bits);
	}

	public static CommandFlags operator ~(CommandFlags a)
	{
		return new CommandFlags(~a.bits);
	}

	public static bool operator ==(CommandFlags a, CommandFlags b)
	{
		return a.bits == b.bits;
	}

	public static bool operator !=(CommandFlags a, CommandFlags b)
	{
		return a.bits != b.bits;
	}
}
