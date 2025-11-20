using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventObjectPropChest : SEventObjectProp
{
	public int Damage { get; private set; }

	public SEventObjectPropChest()
	{
	}

	public SEventObjectPropChest(SEventObjectPropChest state, ReferenceDictionary references)
		: base(state, references)
	{
		Damage = state.Damage;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Damage", Damage);
	}

	public SEventObjectPropChest(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "Damage")
				{
					Damage = info.GetInt32("Damage");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventObjectPropChest entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventObjectPropChest(int damage, string characterID, string action, ESESubTypeObjectProp objectPropSubType, ScenarioManager.ObjectImportType objectType, string objectName, string ownerGuid = "", string text = "")
		: base(objectPropSubType, objectType, objectName, characterID, action, ownerGuid, text)
	{
		Damage = damage;
	}
}
