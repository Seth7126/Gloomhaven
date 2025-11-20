using System;
using System.Collections.Generic;

namespace Photon.Bolt;

[Serializable]
[Documentation]
public struct TypeId
{
	public class Comparer : IComparer<TypeId>
	{
		public static readonly Comparer Instance = new Comparer();

		private Comparer()
		{
		}

		int IComparer<TypeId>.Compare(TypeId x, TypeId y)
		{
			return x.Value - y.Value;
		}
	}

	public class EqualityComparer : IEqualityComparer<TypeId>
	{
		public static readonly EqualityComparer Instance = new EqualityComparer();

		private EqualityComparer()
		{
		}

		bool IEqualityComparer<TypeId>.Equals(TypeId x, TypeId y)
		{
			return x.Value == y.Value;
		}

		int IEqualityComparer<TypeId>.GetHashCode(TypeId x)
		{
			return x.Value;
		}
	}

	public int Value;

	internal TypeId(int value)
	{
		Value = value;
	}

	public override bool Equals(object obj)
	{
		if (obj is TypeId)
		{
			return Value == ((TypeId)obj).Value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value;
	}

	public override string ToString()
	{
		return $"[TypeId:{Value}]";
	}

	public static bool operator ==(TypeId a, TypeId b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(TypeId a, TypeId b)
	{
		return a.Value != b.Value;
	}
}
