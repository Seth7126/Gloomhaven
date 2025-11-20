using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityTeleport : SEventAbility
{
	public CAbilityTeleport.ETeleportState TeleportState { get; private set; }

	public TileIndex MovedFromPoint { get; private set; }

	public TileIndex MovedToPoint { get; private set; }

	public SEventAbilityTeleport()
	{
	}

	public SEventAbilityTeleport(SEventAbilityTeleport state, ReferenceDictionary references)
		: base(state, references)
	{
		TeleportState = state.TeleportState;
		MovedFromPoint = references.Get(state.MovedFromPoint);
		if (MovedFromPoint == null && state.MovedFromPoint != null)
		{
			MovedFromPoint = new TileIndex(state.MovedFromPoint, references);
			references.Add(state.MovedFromPoint, MovedFromPoint);
		}
		MovedToPoint = references.Get(state.MovedToPoint);
		if (MovedToPoint == null && state.MovedToPoint != null)
		{
			MovedToPoint = new TileIndex(state.MovedToPoint, references);
			references.Add(state.MovedToPoint, MovedToPoint);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TeleportState", TeleportState);
		info.AddValue("MovedFromPoint", MovedFromPoint);
		info.AddValue("MovedToPoint", MovedToPoint);
	}

	public SEventAbilityTeleport(SerializationInfo info, StreamingContext context)
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
				case "TeleportState":
					TeleportState = (CAbilityTeleport.ETeleportState)info.GetValue("TeleportState", typeof(CAbilityTeleport.ETeleportState));
					break;
				case "MovedFromPoint":
					MovedFromPoint = (TileIndex)info.GetValue("MovedFromPoint", typeof(TileIndex));
					break;
				case "MovedToPoint":
					MovedToPoint = (TileIndex)info.GetValue("MovedToPoint", typeof(TileIndex));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityTeleport entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityTeleport(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityTeleport.ETeleportState teleportState, TileIndex movedFromPoint, TileIndex movedToPoint, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "")
		: base(CAbility.EAbilityType.Teleport, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text)
	{
		TeleportState = teleportState;
		MovedFromPoint = movedFromPoint;
		MovedToPoint = movedToPoint;
	}
}
