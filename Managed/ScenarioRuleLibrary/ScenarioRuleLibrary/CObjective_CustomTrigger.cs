using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CObjective_CustomTrigger : CObjective
{
	public CLevelTrigger CustomTrigger { get; private set; }

	public bool ObjectiveTriggeredOnClient { get; private set; }

	public CObjective_CustomTrigger()
	{
	}

	public CObjective_CustomTrigger(CObjective_CustomTrigger state, ReferenceDictionary references)
		: base(state, references)
	{
		CustomTrigger = references.Get(state.CustomTrigger);
		if (CustomTrigger == null && state.CustomTrigger != null)
		{
			CustomTrigger = new CLevelTrigger(state.CustomTrigger, references);
			references.Add(state.CustomTrigger, CustomTrigger);
		}
		ObjectiveTriggeredOnClient = state.ObjectiveTriggeredOnClient;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CustomTrigger", CustomTrigger);
		info.AddValue("ObjectiveTriggeredOnClient", ObjectiveTriggeredOnClient);
	}

	public CObjective_CustomTrigger(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "CustomTrigger"))
				{
					if (name == "ObjectiveTriggeredOnClient")
					{
						ObjectiveTriggeredOnClient = info.GetBoolean("ObjectiveTriggeredOnClient");
					}
				}
				else
				{
					CustomTrigger = (CLevelTrigger)info.GetValue("CustomTrigger", typeof(CLevelTrigger));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjective_CustomTrigger entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjective_CustomTrigger(EObjectiveResult result, CObjectiveFilter objectiveFilter, CLevelTrigger customTrigger, bool activeFromStart = true, int? activateOnRound = null, string customLoc = null, string customTileHoverLoc = null, string eventIdentifier = null, bool isHidden = false, bool isOptional = false, bool winDespiteExhaustion = false, bool enoughToWinAlone = false, Dictionary<string, bool> requiredObjectiveStates = null)
		: base(EObjectiveType.CustomTrigger, result, objectiveFilter, activeFromStart, activateOnRound, customLoc, customTileHoverLoc, eventIdentifier, isHidden, isOptional, winDespiteExhaustion, enoughToWinAlone, requiredObjectiveStates)
	{
		CustomTrigger = customTrigger;
	}

	public override bool CheckObjectiveComplete(int partySize, bool isEndOfRound = false)
	{
		base.IsComplete = ObjectiveTriggeredOnClient && CheckOtherObjectiveStatesRequirement();
		return base.IsComplete;
	}

	public override float GetObjectiveProgress(int partySize, out int total, out int current)
	{
		total = 1;
		current = (ObjectiveTriggeredOnClient ? 1 : 0);
		if (!ObjectiveTriggeredOnClient)
		{
			return 0f;
		}
		return 1f;
	}

	public void SetObjectiveTriggered()
	{
		ObjectiveTriggeredOnClient = true;
	}
}
