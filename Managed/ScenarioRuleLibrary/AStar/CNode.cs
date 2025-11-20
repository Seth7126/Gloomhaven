using System;
using ScenarioRuleLibrary;

namespace AStar;

public class CNode
{
	private CNode parentNode;

	public int IslandID = -1;

	public Point Location { get; private set; }

	public bool Walkable { get; set; }

	public bool Blocked { get; set; }

	public bool SuperBlocked { get; set; }

	public bool TransientBlocked { get; set; }

	public bool IsBridge { get; set; }

	public bool IsBridgeOpen { get; set; }

	public bool HasHealth { get; set; }

	public bool IsDifficultTerrain { get; set; }

	public float G { get; set; }

	public float H { get; private set; }

	public NodeState State { get; set; }

	public float F => G + H;

	public CNode ParentNode
	{
		get
		{
			return parentNode;
		}
		set
		{
			parentNode = value;
			G = parentNode.G + GetTraversalCost(Location, parentNode.Location);
		}
	}

	public CNode(int x, int y)
	{
		Location = new Point(x, y);
		State = NodeState.Untested;
		Walkable = false;
		Blocked = false;
		SuperBlocked = false;
		IsBridge = false;
		IsBridgeOpen = true;
		H = 0f;
		G = 0f;
	}

	public void Reinitialise(int x, int y, Point endLocation, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false)
	{
		State = NodeState.Untested;
		H = GetTraversalCost(Location, endLocation, fly: false, jump: false, ignoreMoveCost, ignoreDifficultTerrain);
		G = 0f;
		parentNode = null;
	}

	public override string ToString()
	{
		return $"{Location.X}, {Location.Y}: {State}";
	}

	internal static float GetTraversalCost(Point location, Point otherLocation, bool fly = false, bool jump = false, bool ignoreMoveCost = true, bool ignoreDifficultTerrain = false)
	{
		float num = 0f;
		CNode cNode = ScenarioManager.PathFinder.Nodes[location.X, location.Y];
		if (cNode.IsDifficultTerrain && !ignoreMoveCost && !fly && !jump && !ignoreDifficultTerrain)
		{
			num = 0.2f;
		}
		if (cNode.IsBridge)
		{
			num = 0.2f;
		}
		if (CPathFinder.OddEvenCheck)
		{
			float num2 = (float)otherLocation.X + ((CPathFinder.OddEvenCheck & ((otherLocation.Y & 1) == 1)) ? 0.5f : 0f);
			float num3 = otherLocation.Y;
			float num4 = (float)location.X + ((CPathFinder.OddEvenCheck & ((location.Y & 1) == 1)) ? 0.5f : 0f);
			float num5 = location.Y;
			float num6 = num2 - num4;
			float num7 = num3 - num5;
			return (float)Math.Sqrt(num6 * num6 + num7 * num7) + num;
		}
		float num8 = otherLocation.X - location.X;
		float num9 = otherLocation.Y - location.Y;
		return (float)Math.Sqrt(num8 * num8 + num9 * num9) + num;
	}

	public bool NavTo(CNode toNode, bool ignoreBlocked)
	{
		if (toNode.Walkable && !toNode.SuperBlocked && ((!toNode.Blocked && !toNode.TransientBlocked) || ignoreBlocked))
		{
			if (IslandID != toNode.IslandID && !IsBridge)
			{
				return toNode.IsBridge;
			}
			return true;
		}
		return false;
	}
}
