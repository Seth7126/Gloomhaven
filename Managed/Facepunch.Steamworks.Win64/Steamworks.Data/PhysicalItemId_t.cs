using System;

namespace Steamworks.Data;

internal struct PhysicalItemId_t : IEquatable<PhysicalItemId_t>, IComparable<PhysicalItemId_t>
{
	public uint Value;

	public static implicit operator PhysicalItemId_t(uint value)
	{
		return new PhysicalItemId_t
		{
			Value = value
		};
	}

	public static implicit operator uint(PhysicalItemId_t value)
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
		return Equals((PhysicalItemId_t)p);
	}

	public bool Equals(PhysicalItemId_t p)
	{
		return p.Value == Value;
	}

	public static bool operator ==(PhysicalItemId_t a, PhysicalItemId_t b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PhysicalItemId_t a, PhysicalItemId_t b)
	{
		return !a.Equals(b);
	}

	public int CompareTo(PhysicalItemId_t other)
	{
		return Value.CompareTo(other.Value);
	}
}
