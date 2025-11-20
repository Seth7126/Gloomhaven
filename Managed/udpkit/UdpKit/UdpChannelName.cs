using System.Collections.Generic;

namespace UdpKit;

public struct UdpChannelName
{
	public class Comparer : IComparer<UdpChannelName>
	{
		public static readonly Comparer Instance = new Comparer();

		private Comparer()
		{
		}

		int IComparer<UdpChannelName>.Compare(UdpChannelName x, UdpChannelName y)
		{
			return x.Id.CompareTo(y.Id);
		}
	}

	public class EqualityComparer : IEqualityComparer<UdpChannelName>
	{
		public static readonly EqualityComparer Instance = new EqualityComparer();

		private EqualityComparer()
		{
		}

		bool IEqualityComparer<UdpChannelName>.Equals(UdpChannelName x, UdpChannelName y)
		{
			return x.Id == y.Id;
		}

		int IEqualityComparer<UdpChannelName>.GetHashCode(UdpChannelName obj)
		{
			return obj.Id;
		}
	}

	internal int Id;

	internal string Name;

	internal UdpChannelName(int id)
	{
		Id = id;
		Name = null;
	}

	internal UdpChannelName(int id, string name)
	{
		Id = id;
		Name = name;
	}

	public override string ToString()
	{
		return string.Format("[UdpChannelName {0}:{1}]", Name ?? "", Id);
	}
}
