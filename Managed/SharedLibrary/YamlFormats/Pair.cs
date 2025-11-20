using System;

namespace YamlFormats;

public class Pair<TLeft, TRight> : IEquatable<Pair<TLeft, TRight>>
{
	public TLeft Left;

	public TRight Right;

	public Pair(TLeft left, TRight right)
	{
		Left = left;
		Right = right;
	}

	public override string ToString()
	{
		return $"({Left},{Right})";
	}

	public override int GetHashCode()
	{
		return Left.GetHashCode() + Right.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is Pair<TLeft, TRight>)
		{
			return Equals((Pair<TLeft, TRight>)obj);
		}
		return false;
	}

	public bool Equals(Pair<TLeft, TRight> other)
	{
		ref TLeft left = ref Left;
		object obj = other.Left;
		if (left.Equals(obj))
		{
			ref TRight right = ref Right;
			object obj2 = other.Right;
			return right.Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(Pair<TLeft, TRight> one, Pair<TLeft, TRight> other)
	{
		return one.Equals(other);
	}

	public static bool operator !=(Pair<TLeft, TRight> one, Pair<TLeft, TRight> other)
	{
		return !(one == other);
	}
}
