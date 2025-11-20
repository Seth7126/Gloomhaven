using System;
using System.Linq;
using Photon.Bolt.Collections;

namespace Photon.Bolt.Internal;

[Documentation(Ignore = true)]
public class PacketTypeStats
{
	public double TotalIn;

	public double TotalOut;

	public double In;

	public double Out;

	internal void Update(BoltRingBuffer<PacketStats> statsIn, BoltRingBuffer<PacketStats> statsOut, Func<PacketStats, int> selector)
	{
		In = Math.Round((float)statsIn.Select(selector).Sum() / 8f / (float)statsIn.count, 2);
		Out = Math.Round((float)statsOut.Select(selector).Sum() / 8f / (float)statsOut.count, 2);
		TotalIn += Math.Round((float)statsIn.Select(selector).Sum() / 8f, 2);
		TotalOut += Math.Round((float)statsOut.Select(selector).Sum() / 8f, 2);
	}
}
