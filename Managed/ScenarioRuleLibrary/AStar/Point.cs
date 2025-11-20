using ScenarioRuleLibrary;

namespace AStar;

public struct Point
{
	public int X;

	public int Y;

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public Point(TileIndex tileIndex)
	{
		X = tileIndex.X;
		Y = tileIndex.Y;
	}

	public Point(CTile tile)
	{
		X = tile.m_ArrayIndex.X;
		Y = tile.m_ArrayIndex.Y;
	}

	public static bool operator ==(Point left, Point right)
	{
		if (left.X == right.X)
		{
			return left.Y == right.Y;
		}
		return false;
	}

	public static bool operator !=(Point left, Point right)
	{
		if (left.X == right.X)
		{
			return left.Y != right.Y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		Point point = (Point)obj;
		if (X == point.X)
		{
			return Y == point.Y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 0;
	}

	public override string ToString()
	{
		return $"{X}:{Y}";
	}
}
