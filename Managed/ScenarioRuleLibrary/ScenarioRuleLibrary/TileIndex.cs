using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("X:{X} Y:{Y}")]
public class TileIndex : ISerializable, IEquatable<TileIndex>
{
	public int X;

	public int Y;

	public TileIndex()
	{
	}

	public TileIndex(TileIndex state, ReferenceDictionary references)
	{
		X = state.X;
		Y = state.Y;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("X", X);
		info.AddValue("Y", Y);
	}

	public TileIndex(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "X"))
				{
					if (name == "Y")
					{
						Y = info.GetInt32("Y");
					}
				}
				else
				{
					X = info.GetInt32("X");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize TileIndex entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public TileIndex(int x, int y)
	{
		X = x;
		Y = y;
	}

	public TileIndex(Point point)
	{
		X = point.X;
		Y = point.Y;
	}

	public TileIndex Copy()
	{
		return new TileIndex(X, Y);
	}

	public override string ToString()
	{
		return "X:" + X + " Y:" + Y;
	}

	public static bool Compare(TileIndex t1, TileIndex t2)
	{
		switch (StateShared.CheckNullsMatch(t1, t2))
		{
		case StateShared.ENullStatus.Mismatch:
			return false;
		case StateShared.ENullStatus.BothNull:
			return true;
		default:
			if (t1.X == t2.X)
			{
				return t1.Y == t2.Y;
			}
			return false;
		}
	}

	public bool Equals(TileIndex other)
	{
		if (X == other.X)
		{
			return Y == other.Y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return X + 31 * Y;
	}
}
