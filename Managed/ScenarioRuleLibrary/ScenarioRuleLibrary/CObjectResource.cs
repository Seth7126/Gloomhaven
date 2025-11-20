using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectResource : CObjectProp, ISerializable
{
	public string ResourceID;

	public CharacterResourceData ResourceData => ScenarioRuleClient.SRLYML.CharacterResources.SingleOrDefault((CharacterResourceData x) => x.ID == ResourceID);

	public CObjectResource()
	{
	}

	public CObjectResource(CObjectResource state, ReferenceDictionary references)
		: base(state, references)
	{
		ResourceID = state.ResourceID;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ResourceID", ResourceID);
	}

	public CObjectResource(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "ResourceID")
				{
					ResourceID = info.GetString("ResourceID");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectResource entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectResource(CharacterResourceData resourceData, string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		if (resourceData != null)
		{
			ResourceID = resourceData.ID;
			if (resourceData.CanLootFilter != null)
			{
				SetCanLootFilter(resourceData.CanLootFilter.Copy());
			}
		}
	}

	public override bool AutomaticActivate(CActor actor)
	{
		return false;
	}

	public static List<Tuple<int, string>> Compare(CObjectResource resource1, CObjectResource resource2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(CObjectProp.Compare(resource1, resource2, isMPCompare));
		return list;
	}
}
