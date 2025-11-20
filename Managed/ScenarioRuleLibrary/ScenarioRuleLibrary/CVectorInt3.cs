using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("X:{X} Y:{Y} Z:{Z}")]
public class CVectorInt3 : ISerializable
{
	public int X { get; private set; }

	public int Y { get; private set; }

	public int Z { get; private set; }

	public CVectorInt3()
	{
	}

	public CVectorInt3(CVectorInt3 state, ReferenceDictionary references)
	{
		X = state.X;
		Y = state.Y;
		Z = state.Z;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("X", X);
		info.AddValue("Y", Y);
		info.AddValue("Z", Z);
	}

	public CVectorInt3(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "X":
					X = info.GetInt32("X");
					break;
				case "Y":
					Y = info.GetInt32("Y");
					break;
				case "Z":
					Z = info.GetInt32("Z");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CVectorInt3 entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CVectorInt3(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public override string ToString()
	{
		return "X: " + X + " Y: " + Y + " Z: " + Z;
	}

	public static bool Compare(CVectorInt3 v1, CVectorInt3 v2)
	{
		if (v1.X == v2.X && v1.Y == v2.Y)
		{
			return v1.Z == v2.Z;
		}
		return false;
	}
}
