using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventObjectProp : SEvent
{
	public ESESubTypeObjectProp ObjectPropSubType { get; private set; }

	public ScenarioManager.ObjectImportType ObjectType { get; private set; }

	public string ObjectName { get; private set; }

	public string OwnerGuid { get; private set; }

	public string ObjectCharacterID { get; private set; }

	public string ObjectAction { get; private set; }

	public List<CMap> ObjectMaps { get; private set; }

	public SEventObjectProp()
	{
	}

	public SEventObjectProp(SEventObjectProp state, ReferenceDictionary references)
		: base(state, references)
	{
		ObjectPropSubType = state.ObjectPropSubType;
		ObjectType = state.ObjectType;
		ObjectName = state.ObjectName;
		OwnerGuid = state.OwnerGuid;
		ObjectCharacterID = state.ObjectCharacterID;
		ObjectAction = state.ObjectAction;
		ObjectMaps = references.Get(state.ObjectMaps);
		if (ObjectMaps != null || state.ObjectMaps == null)
		{
			return;
		}
		ObjectMaps = new List<CMap>();
		for (int i = 0; i < state.ObjectMaps.Count; i++)
		{
			CMap cMap = state.ObjectMaps[i];
			CMap cMap2 = references.Get(cMap);
			if (cMap2 == null && cMap != null)
			{
				cMap2 = new CMap(cMap, references);
				references.Add(cMap, cMap2);
			}
			ObjectMaps.Add(cMap2);
		}
		references.Add(state.ObjectMaps, ObjectMaps);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ObjectPropSubType", ObjectPropSubType);
		info.AddValue("ObjectType", ObjectType);
		info.AddValue("ObjectName", ObjectName);
		info.AddValue("OwnerGuid", OwnerGuid);
		info.AddValue("ObjectCharacterID", ObjectCharacterID);
		info.AddValue("ObjectAction", ObjectAction);
		info.AddValue("ObjectMaps", ObjectMaps);
	}

	public SEventObjectProp(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ObjectPropSubType":
					ObjectPropSubType = (ESESubTypeObjectProp)info.GetValue("ObjectPropSubType", typeof(ESESubTypeObjectProp));
					break;
				case "ObjectType":
					ObjectType = (ScenarioManager.ObjectImportType)info.GetValue("ObjectType", typeof(ScenarioManager.ObjectImportType));
					break;
				case "ObjectName":
					ObjectName = info.GetString("ObjectName");
					break;
				case "OwnerGuid":
					OwnerGuid = info.GetString("OwnerGuid");
					break;
				case "ObjectCharacterID":
					ObjectCharacterID = info.GetString("ObjectCharacterID");
					break;
				case "ObjectAction":
					ObjectAction = info.GetString("ObjectAction");
					break;
				case "ObjectMaps":
					ObjectMaps = (List<CMap>)info.GetValue("ObjectMaps", typeof(List<CMap>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventObjectProp entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventObjectProp(ESESubTypeObjectProp objectPropSubType, ScenarioManager.ObjectImportType objectType, string objectName, string objectActor, string objectAction, string ownerGuid = "", string text = "", List<CMap> maps = null)
		: base(ESEType.ObjectProp, text)
	{
		ObjectPropSubType = objectPropSubType;
		ObjectType = objectType;
		ObjectName = objectName;
		OwnerGuid = ownerGuid;
		ObjectCharacterID = objectActor;
		ObjectAction = objectAction;
		ObjectMaps = maps;
	}
}
