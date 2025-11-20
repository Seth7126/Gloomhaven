using System;

namespace Steamworks.Data;

internal struct ControllerAnalogActionHandle_t : IEquatable<ControllerAnalogActionHandle_t>, IComparable<ControllerAnalogActionHandle_t>
{
	public ulong Value;

	public static implicit operator ControllerAnalogActionHandle_t(ulong value)
	{
		return new ControllerAnalogActionHandle_t
		{
			Value = value
		};
	}

	public static implicit operator ulong(ControllerAnalogActionHandle_t value)
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
		return Equals((ControllerAnalogActionHandle_t)p);
	}

	public bool Equals(ControllerAnalogActionHandle_t p)
	{
		return p.Value == Value;
	}

	public static bool operator ==(ControllerAnalogActionHandle_t a, ControllerAnalogActionHandle_t b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(ControllerAnalogActionHandle_t a, ControllerAnalogActionHandle_t b)
	{
		return !a.Equals(b);
	}

	public int CompareTo(ControllerAnalogActionHandle_t other)
	{
		return Value.CompareTo(other.Value);
	}
}
