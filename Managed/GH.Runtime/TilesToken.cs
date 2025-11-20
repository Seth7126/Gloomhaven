using System.Collections.Generic;
using AStar;
using Photon.Bolt;
using ScenarioRuleLibrary;
using UdpKit;

public class TilesToken : IProtocolToken
{
	private int tileCount;

	public int[,] Tiles { get; set; }

	public TilesToken(CTile clickedTile, List<CTile> optionalTiles = null)
	{
		if (optionalTiles != null)
		{
			tileCount = optionalTiles.Count + 1;
			Tiles = new int[tileCount, 2];
			for (int i = 1; i < tileCount; i++)
			{
				Tiles[i, 0] = optionalTiles[i - 1].m_ArrayIndex.X;
				Tiles[i, 1] = optionalTiles[i - 1].m_ArrayIndex.Y;
			}
		}
		else
		{
			tileCount = 1;
			Tiles = new int[tileCount, 2];
		}
		Tiles[0, 0] = clickedTile.m_ArrayIndex.X;
		Tiles[0, 1] = clickedTile.m_ArrayIndex.Y;
	}

	public TilesToken(List<Point> points)
	{
		tileCount = points.Count;
		Tiles = new int[tileCount, 2];
		for (int i = 0; i < tileCount; i++)
		{
			Tiles[i, 0] = points[i].X;
			Tiles[i, 1] = points[i].Y;
		}
	}

	public TilesToken()
	{
		tileCount = 0;
		Tiles = new int[tileCount, 2];
	}

	public virtual void Write(UdpPacket packet)
	{
		packet.WriteInt(tileCount);
		for (int i = 0; i < tileCount; i++)
		{
			packet.WriteInt(Tiles[i, 0]);
			packet.WriteInt(Tiles[i, 1]);
		}
	}

	public virtual void Read(UdpPacket packet)
	{
		tileCount = packet.ReadInt();
		Tiles = new int[tileCount, 2];
		for (int i = 0; i < tileCount; i++)
		{
			Tiles[i, 0] = packet.ReadInt();
			Tiles[i, 1] = packet.ReadInt();
		}
	}
}
