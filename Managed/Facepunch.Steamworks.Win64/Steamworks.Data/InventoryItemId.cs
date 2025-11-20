using System;

namespace Steamworks.Data;

public struct InventoryItemId : IEquatable<InventoryItemId>, IComparable<InventoryItemId>
{
	public ulong Value;

	public static implicit operator InventoryItemId(ulong value)
	{
		return new InventoryItemId
		{
			Value = value
		};
	}

	public static implicit operator ulong(InventoryItemId value)
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
		return Equals((InventoryItemId)p);
	}

	public bool Equals(InventoryItemId p)
	{
		return p.Value == Value;
	}

	public static bool operator ==(InventoryItemId a, InventoryItemId b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(InventoryItemId a, InventoryItemId b)
	{
		return !a.Equals(b);
	}

	public int CompareTo(InventoryItemId other)
	{
		return Value.CompareTo(other.Value);
	}
}
