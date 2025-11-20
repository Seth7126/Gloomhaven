using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectMonsterGrave : CObjectProp, ISerializable
{
	public bool IsEliteGrave;

	public CObjectMonsterGrave()
	{
	}

	public CObjectMonsterGrave(CObjectMonsterGrave state, ReferenceDictionary references)
		: base(state, references)
	{
		IsEliteGrave = state.IsEliteGrave;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("IsEliteGrave", IsEliteGrave);
	}

	public CObjectMonsterGrave(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "IsEliteGrave")
				{
					IsEliteGrave = info.GetBoolean("IsEliteGrave");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectMonsterGrave entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectMonsterGrave(bool isEliteGrave, string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		IsEliteGrave = isEliteGrave;
	}

	public override bool AutomaticActivate(CActor actor)
	{
		return false;
	}

	public static List<Tuple<int, string>> Compare(CObjectMonsterGrave resource1, CObjectMonsterGrave resource2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(resource1, resource2, isMPCompare));
		return list;
	}
}
