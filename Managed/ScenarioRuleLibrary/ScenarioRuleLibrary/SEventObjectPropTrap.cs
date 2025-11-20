using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventObjectPropTrap : SEventObjectProp
{
	public int Damage { get; private set; }

	public string ActionActor { get; private set; }

	public SEventObjectPropTrap()
	{
	}

	public SEventObjectPropTrap(SEventObjectPropTrap state, ReferenceDictionary references)
		: base(state, references)
	{
		Damage = state.Damage;
		ActionActor = state.ActionActor;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Damage", Damage);
		info.AddValue("ActionActor", ActionActor);
	}

	public SEventObjectPropTrap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "Damage"))
				{
					if (name == "ActionActor")
					{
						ActionActor = info.GetString("ActionActor");
					}
				}
				else
				{
					Damage = info.GetInt32("Damage");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventObjectPropTrap entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventObjectPropTrap(int damage, string characterID, string action, ESESubTypeObjectProp objectPropSubType, ScenarioManager.ObjectImportType objectType, string objectName, string ownerGuid = "", string text = "", string overrideInternalActor = "")
		: base(objectPropSubType, objectType, objectName, characterID, action, ownerGuid, text)
	{
		Damage = damage;
		ActionActor = overrideInternalActor;
		if (string.IsNullOrEmpty(overrideInternalActor) && GameState.InternalCurrentActor != null)
		{
			ActionActor = GameState.InternalCurrentActor.Class.ID;
		}
	}
}
