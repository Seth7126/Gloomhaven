using System;

namespace Steamworks.Data;

internal struct PartyBeaconID_t : IEquatable<PartyBeaconID_t>, IComparable<PartyBeaconID_t>
{
	public ulong Value;

	public static implicit operator PartyBeaconID_t(ulong value)
	{
		return new PartyBeaconID_t
		{
			Value = value
		};
	}

	public static implicit operator ulong(PartyBeaconID_t value)
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
		return Equals((PartyBeaconID_t)p);
	}

	public bool Equals(PartyBeaconID_t p)
	{
		return p.Value == Value;
	}

	public static bool operator ==(PartyBeaconID_t a, PartyBeaconID_t b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(PartyBeaconID_t a, PartyBeaconID_t b)
	{
		return !a.Equals(b);
	}

	public int CompareTo(PartyBeaconID_t other)
	{
		return Value.CompareTo(other.Value);
	}
}
