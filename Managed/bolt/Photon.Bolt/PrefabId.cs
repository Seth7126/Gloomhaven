using System;
using System.Collections.Generic;

namespace Photon.Bolt;

[Serializable]
[Documentation]
public struct PrefabId
{
	public class Comparer : IComparer<PrefabId>
	{
		public static readonly Comparer Instance = new Comparer();

		private Comparer()
		{
		}

		int IComparer<PrefabId>.Compare(PrefabId x, PrefabId y)
		{
			return x.Value - y.Value;
		}
	}

	public class EqualityComparer : IEqualityComparer<PrefabId>
	{
		public static readonly EqualityComparer Instance = new EqualityComparer();

		private EqualityComparer()
		{
		}

		bool IEqualityComparer<PrefabId>.Equals(PrefabId x, PrefabId y)
		{
			return x.Value == y.Value;
		}

		int IEqualityComparer<PrefabId>.GetHashCode(PrefabId x)
		{
			return x.Value;
		}
	}

	public int Value;

	internal PrefabId(int value)
	{
		Value = value;
	}

	public override bool Equals(object obj)
	{
		if (obj is PrefabId)
		{
			return Value == ((PrefabId)obj).Value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Value;
	}

	public override string ToString()
	{
		return $"[PrefabId:{Value}]";
	}

	public static bool operator ==(PrefabId a, PrefabId b)
	{
		return a.Value == b.Value;
	}

	public static bool operator !=(PrefabId a, PrefabId b)
	{
		return a.Value != b.Value;
	}
}
