using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("X:{X} Y:{Y} Z:{Z}")]
public class CVector3 : ISerializable
{
	public float X { get; private set; }

	public float Y { get; private set; }

	public float Z { get; private set; }

	public CVector3()
	{
	}

	public CVector3(CVector3 state, ReferenceDictionary references)
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

	public CVector3(SerializationInfo info, StreamingContext context)
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
					X = (float)info.GetDouble("X");
					break;
				case "Y":
					Y = (float)info.GetDouble("Y");
					break;
				case "Z":
					Z = (float)info.GetDouble("Z");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CVector3 entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CVector3(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public override string ToString()
	{
		return "X:" + X + " Y:" + Y + " Z:" + Z;
	}

	public static bool Compare(CVector3 v1, CVector3 v2)
	{
		switch (StateShared.CheckNullsMatch(v1, v2))
		{
		case StateShared.ENullStatus.Mismatch:
			return false;
		case StateShared.ENullStatus.BothNull:
			return true;
		default:
			if (CompareFloats(v1.X, v2.X) && CompareFloats(v1.Y, v2.Y))
			{
				return CompareFloats(v1.Z, v2.Z);
			}
			return false;
		}
	}

	public static bool CompareFloats(float a, float b, float allowedDifference = 0.001f)
	{
		if (a == b)
		{
			return true;
		}
		if (a > b)
		{
			return a - b < allowedDifference;
		}
		return b - a < allowedDifference;
	}
}
