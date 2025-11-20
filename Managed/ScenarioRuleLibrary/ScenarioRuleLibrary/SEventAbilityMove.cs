using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityMove : SEventAbility
{
	public CAbilityMove.EMoveState MoveState { get; private set; }

	public int TilesMoved { get; private set; }

	public int ActualMoved { get; private set; }

	public bool Jumped { get; private set; }

	public TileIndex MovedFromPoint { get; private set; }

	public TileIndex MovedToPoint { get; private set; }

	public SEventAbilityMove()
	{
	}

	public SEventAbilityMove(SEventAbilityMove state, ReferenceDictionary references)
		: base(state, references)
	{
		MoveState = state.MoveState;
		TilesMoved = state.TilesMoved;
		ActualMoved = state.ActualMoved;
		Jumped = state.Jumped;
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
		info.AddValue("MoveState", MoveState);
		info.AddValue("TilesMoved", TilesMoved);
		info.AddValue("ActualMoved", TilesMoved);
		info.AddValue("Jumped", Jumped);
		info.AddValue("MovedFromPoint", MovedFromPoint);
		info.AddValue("MovedToPoint", MovedToPoint);
	}

	public SEventAbilityMove(SerializationInfo info, StreamingContext context)
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
				case "MoveState":
					MoveState = (CAbilityMove.EMoveState)info.GetValue("MoveState", typeof(CAbilityMove.EMoveState));
					break;
				case "TilesMoved":
					TilesMoved = info.GetInt32("TilesMoved");
					break;
				case "ActualMoved":
					ActualMoved = info.GetInt32("ActualMoved");
					break;
				case "Jumped":
					Jumped = info.GetBoolean("Jumped");
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
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityMove entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityMove(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityMove.EMoveState moveState, int tilesMoved, int actualMoved, TileIndex movedFromPoint, TileIndex movedToPoint, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool jump, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "", bool defaultAction = false, bool hasHappened = true)
		: base(CAbility.EAbilityType.Move, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text, defaultAction, hasHappened)
	{
		MoveState = moveState;
		TilesMoved = tilesMoved;
		ActualMoved = actualMoved;
		MovedFromPoint = movedFromPoint;
		MovedToPoint = movedToPoint;
		Jumped = jump;
	}
}
