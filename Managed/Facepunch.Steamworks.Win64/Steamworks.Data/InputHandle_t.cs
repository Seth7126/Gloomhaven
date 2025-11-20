using System;

namespace Steamworks.Data;

internal struct InputHandle_t : IEquatable<InputHandle_t>, IComparable<InputHandle_t>
{
	public ulong Value;

	public static implicit operator InputHandle_t(ulong value)
	{
		return new InputHandle_t
		{
			Value = value
		};
	}

	public static implicit operator ulong(InputHandle_t value)
	{
		return value.Value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override bool Equals(object p)
	{
		return Equals((InputHandle_t)p);
	}

	public bool Equals(InputHandle_t p)
	{
		return p.Value == Value;
	}

	public static bool operator ==(InputHandle_t a, InputHandle_t b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(InputHandle_t a, InputHandle_t b)
	{
		return !a.Equals(b);
	}

	public int CompareTo(InputHandle_t other)
	{
		return Value.CompareTo(other.Value);
	}
}
