using System.Collections.Generic;
using System.Text;

namespace Photon.Bolt;

[Documentation(Ignore = true)]
public struct Filter
{
	[Documentation(Ignore = true)]
	public class EqualityComparer : IEqualityComparer<Filter>
	{
		public static readonly EqualityComparer Instance = new EqualityComparer();

		private EqualityComparer()
		{
		}

		bool IEqualityComparer<Filter>.Equals(Filter a, Filter b)
		{
			return a.Bits == b.Bits;
		}

		int IEqualityComparer<Filter>.GetHashCode(Filter f)
		{
			return f.Bits;
		}
	}

	internal readonly int Bits;

	internal static string[] Names = new string[32];

	internal Filter(int bits)
	{
		Bits = bits;
	}

	public override int GetHashCode()
	{
		return Bits;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append("Filter");
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if ((Bits & num) == num)
			{
				if (Names[i] == null)
				{
					stringBuilder.Append(" ?" + i);
				}
				else
				{
					stringBuilder.Append(" " + Names[i]);
				}
			}
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public static implicit operator bool(Filter a)
	{
		return a.Bits != 0;
	}

	public static Filter operator &(Filter a, Filter b)
	{
		return new Filter(a.Bits & b.Bits);
	}

	public static Filter operator |(Filter a, Filter b)
	{
		return new Filter(a.Bits | b.Bits);
	}
}
