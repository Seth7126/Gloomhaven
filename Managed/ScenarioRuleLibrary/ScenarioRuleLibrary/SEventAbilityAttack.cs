using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityAttack : SEventAbility
{
	public CAbilityAttack.EAttackState AttackState { get; private set; }

	public List<CAttackEffect> AttackEffects { get; private set; }

	public bool IsTargetedAbility { get; private set; }

	public bool AreaEffect { get; private set; }

	public int Targets { get; private set; }

	public SEventAbilityAttack()
	{
	}

	public SEventAbilityAttack(SEventAbilityAttack state, ReferenceDictionary references)
		: base(state, references)
	{
		AttackState = state.AttackState;
		AttackEffects = references.Get(state.AttackEffects);
		if (AttackEffects == null && state.AttackEffects != null)
		{
			AttackEffects = new List<CAttackEffect>();
			for (int i = 0; i < state.AttackEffects.Count; i++)
			{
				CAttackEffect cAttackEffect = state.AttackEffects[i];
				CAttackEffect cAttackEffect2 = references.Get(cAttackEffect);
				if (cAttackEffect2 == null && cAttackEffect != null)
				{
					cAttackEffect2 = new CAttackEffect(cAttackEffect, references);
					references.Add(cAttackEffect, cAttackEffect2);
				}
				AttackEffects.Add(cAttackEffect2);
			}
			references.Add(state.AttackEffects, AttackEffects);
		}
		IsTargetedAbility = state.IsTargetedAbility;
		AreaEffect = state.AreaEffect;
		Targets = state.Targets;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("AttackState", AttackState);
		info.AddValue("AttackEffects", AttackEffects);
		info.AddValue("IsTargetedAbility", IsTargetedAbility);
		info.AddValue("AreaEffect", AreaEffect);
		info.AddValue("Targets", Targets);
	}

	public SEventAbilityAttack(SerializationInfo info, StreamingContext context)
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
				case "AttackState":
					AttackState = (CAbilityAttack.EAttackState)info.GetValue("AttackState", typeof(CAbilityAttack.EAttackState));
					break;
				case "AttackEffects":
					AttackEffects = (List<CAttackEffect>)info.GetValue("AttackEffects", typeof(List<CAttackEffect>));
					break;
				case "IsTargetedAbility":
					IsTargetedAbility = info.GetBoolean("IsTargetedAbility");
					break;
				case "AreaEffect":
					AreaEffect = info.GetBoolean("AreaEffect");
					break;
				case "Targets":
					Targets = info.GetInt32("Targets");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbilityAttack entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityAttack(ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassName, CAbilityAttack.EAttackState attackState, List<CAttackEffect> attackEffects, bool isTargetedAbility, bool areaEffect, int targets, int strength, List<CAbility> addedPositiveConditions, List<CAbility> addedNegativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "", bool defaultAction = false, bool hasHappened = true)
		: base(CAbility.EAbilityType.Attack, abilitySubType, name, cardID, cardType, actorClassName, strength, addedPositiveConditions, addedNegativeConditions, actorType, IsSummon, actorpositiveConditions, actornegativeConditions, actedOnClass, actedOnType, ActedOnIsSummon, ActedOnpositiveConditions, ActedOnnegativeConditions, text, defaultAction, hasHappened)
	{
		AttackState = attackState;
		AttackEffects = attackEffects;
		IsTargetedAbility = isTargetedAbility;
		AreaEffect = areaEffect;
		Targets = Targets;
	}
}
