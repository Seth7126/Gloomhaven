using System;
using Manatee.Trello.Internal;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class Position : IComparable, IComparable<Position>
{
	private const double TopValue = double.NegativeInfinity;

	private const double BottomValue = double.PositiveInfinity;

	private const double UnknownValue = double.NaN;

	public static Position Top { get; } = new Position(double.NegativeInfinity);

	public static Position Bottom { get; } = new Position(double.PositiveInfinity);

	public static Position Unknown { get; } = new Position(double.NaN);

	public bool IsValid
	{
		get
		{
			if (!object.Equals(Value, double.NegativeInfinity))
			{
				if (!object.Equals(Value, double.NaN) && !object.Equals(Value, double.NegativeInfinity))
				{
					return Value > 0.0;
				}
				return false;
			}
			return true;
		}
	}

	public double Value { get; }

	public Position(double value)
	{
		Value = value;
	}

	public static Position Between(Position a, Position b)
	{
		return new Position((a.Value + b.Value) / 2.0);
	}

	internal static Position GetPosition(IJsonPosition pos)
	{
		if (pos == null)
		{
			return null;
		}
		if (pos.Named.IsNullOrWhiteSpace() && pos.Explicit.HasValue)
		{
			return new Position(pos.Explicit.Value);
		}
		string named = pos.Named;
		if (!(named == "top"))
		{
			if (named == "bottom")
			{
				return Bottom;
			}
			return null;
		}
		return Top;
	}

	internal static IJsonPosition GetJson(Position pos)
	{
		IJsonPosition jsonPosition = TrelloConfiguration.JsonFactory.Create<IJsonPosition>();
		if (pos == null)
		{
			return jsonPosition;
		}
		if (object.Equals(pos, Unknown))
		{
			jsonPosition.Named = "unknown";
		}
		else if (object.Equals(pos, Top))
		{
			jsonPosition.Named = "top";
		}
		else if (object.Equals(pos, Bottom))
		{
			jsonPosition.Named = "bottom";
		}
		else
		{
			jsonPosition.Explicit = pos.Value;
		}
		return jsonPosition;
	}

	public int CompareTo(Position other)
	{
		return Value.CompareTo(other.Value);
	}

	public int CompareTo(object obj)
	{
		return ((IComparable)Value).CompareTo(obj);
	}

	public override string ToString()
	{
		if (Equals(Unknown))
		{
			return "unknown";
		}
		if (Equals(Top))
		{
			return "top";
		}
		if (Equals(Bottom))
		{
			return "bottom";
		}
		return Value.ToLowerString();
	}

	public static implicit operator Position(double value)
	{
		return new Position(value);
	}

	public static implicit operator Position(int value)
	{
		return new Position(value);
	}

	public static explicit operator double(Position position)
	{
		return position.Value;
	}

	public static explicit operator int(Position position)
	{
		return (int)position.Value;
	}

	public static bool operator ==(Position a, Position b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if (object.Equals(a, null) || object.Equals(b, null))
		{
			return false;
		}
		return object.Equals(a.Value, b.Value);
	}

	public static bool operator !=(Position a, Position b)
	{
		return !(a == b);
	}

	public static bool operator <(Position a, Position b)
	{
		return a.Value < b.Value;
	}

	public static bool operator >(Position a, Position b)
	{
		return a.Value > b.Value;
	}

	public static bool operator <=(Position a, Position b)
	{
		return a.Value <= b.Value;
	}

	public static bool operator >=(Position a, Position b)
	{
		return a.Value >= b.Value;
	}

	public bool Equals(Position other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return object.Equals(other.Value, Value);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as Position);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
