using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CAttackEffect : ISerializable
{
	[Serializable]
	public enum EAttackEffect
	{
		None,
		StandardBuff,
		BuffForEachNegativeConditionOnTarget,
		BuffForEachPositiveConditionOnTarget,
		BuffForEachConditionOnTarget,
		BuffIfAnyConditionsOnTarget,
		BuffIfCanPerformSubAbility,
		BuffIfAnItemUsedInAction
	}

	public static EAttackEffect[] AttackEffects = (EAttackEffect[])Enum.GetValues(typeof(EAttackEffect));

	private bool stackedSubAbilityForBuff;

	private CActor actorTargetedForStackedSubAbilityBuff;

	public EAttackEffect Effect { get; private set; }

	public int Strength { get; private set; }

	public int XP { get; private set; }

	public int Pierce { get; private set; }

	public CAbilityFilter Filter { get; private set; }

	public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

	public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

	public CAbility.EAttackType AttackType { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Effect", Effect);
		info.AddValue("Strength", Strength);
		info.AddValue("XP", XP);
		info.AddValue("Pierce", Pierce);
		info.AddValue("Filter", Filter);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("AttackType", AttackType);
	}

	public CAttackEffect(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Effect":
					Effect = (EAttackEffect)info.GetValue("Effect", typeof(EAttackEffect));
					break;
				case "Strength":
					Strength = info.GetInt32("Strength");
					break;
				case "XP":
					XP = info.GetInt32("XP");
					break;
				case "Pierce":
					Pierce = info.GetInt32("Pierce");
					break;
				case "Filter":
					Filter = (CAbilityFilter)info.GetValue("Filter", typeof(CAbilityFilter));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "PositiveConditions":
					PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "AttackType":
					AttackType = (CAbility.EAttackType)info.GetValue("AttackType", typeof(CAbility.EAttackType));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CAttackEffect entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CAttackEffect(EAttackEffect effect, int strength, int xp, int pierce, CAbilityFilter filter, List<CCondition.ENegativeCondition> negCons, List<CCondition.EPositiveCondition> posCons, CAbility.EAttackType attackType)
	{
		Effect = effect;
		Strength = strength;
		XP = xp;
		Pierce = pierce;
		Filter = filter;
		NegativeConditions = negCons ?? new List<CCondition.ENegativeCondition>();
		PositiveConditions = posCons ?? new List<CCondition.EPositiveCondition>();
		AttackType = attackType;
	}

	public void GetBonus(CActor target, CAbilityAttack attackAbility, out int attackBuff, out int xpBuff)
	{
		attackBuff = 0;
		xpBuff = 0;
		if ((AttackType == CAbility.EAttackType.Melee && !attackAbility.IsMeleeAttack) || (AttackType == CAbility.EAttackType.Ranged && attackAbility.IsMeleeAttack) || !Filter.IsValidTarget(target, attackAbility.TargetingActor, attackAbility.IsTargetedAbility, useTargetOriginalType: false, attackAbility.MiscAbilityData?.CanTargetInvisible))
		{
			return;
		}
		switch (Effect)
		{
		case EAttackEffect.StandardBuff:
			attackBuff += Strength;
			xpBuff += XP;
			break;
		case EAttackEffect.BuffForEachNegativeConditionOnTarget:
			attackBuff += target.Tokens.GetAllNegativeConditions().Count * Strength;
			xpBuff += target.Tokens.GetAllNegativeConditions().Count * XP;
			break;
		case EAttackEffect.BuffForEachPositiveConditionOnTarget:
			attackBuff += target.Tokens.GetAllPositiveConditions().Count * Strength;
			xpBuff += target.Tokens.GetAllPositiveConditions().Count * XP;
			break;
		case EAttackEffect.BuffForEachConditionOnTarget:
		{
			int num = 0;
			if (NegativeConditions.Count > 0)
			{
				num = target.Tokens.CheckNegativeTokens.Where((NegativeConditionPair w) => NegativeConditions.Contains(w.NegativeCondition)).Count();
			}
			if (PositiveConditions.Count > 0)
			{
				num += target.Tokens.CheckPositiveTokens.Where((PositiveConditionPair w) => PositiveConditions.Contains(w.PositiveCondition)).Count();
			}
			attackBuff += num * Strength;
			xpBuff += num * XP;
			break;
		}
		case EAttackEffect.BuffIfAnyConditionsOnTarget:
			if (NegativeConditions.Count > 0 && target.Tokens.CheckNegativeTokens.Any((NegativeConditionPair a) => NegativeConditions.Contains(a.NegativeCondition)))
			{
				attackBuff += Strength;
				xpBuff += XP;
			}
			else if (PositiveConditions.Count > 0 && target.Tokens.CheckPositiveTokens.Any((PositiveConditionPair a) => PositiveConditions.Contains(a.PositiveCondition)))
			{
				attackBuff += Strength;
				xpBuff += XP;
			}
			break;
		case EAttackEffect.BuffIfCanPerformSubAbility:
			if (stackedSubAbilityForBuff && target.Equals(actorTargetedForStackedSubAbilityBuff))
			{
				attackBuff += Strength;
				xpBuff += XP;
			}
			break;
		case EAttackEffect.BuffIfAnItemUsedInAction:
		{
			CPhaseAction cPhaseAction = (CPhaseAction)PhaseManager.Phase;
			if (attackAbility.ActiveOverrideItems.Count > 0 || attackAbility.ActiveSingleTargetItems.Count > 0 || cPhaseAction.ItemsUsedThisPhase.Count > 0 || GameState.InternalCurrentActor.Inventory.SelectedItems.Count > 0 || attackAbility.AttackSummary?.AttackActiveBonuses?.FirstOrDefault((CActiveBonus x) => x.BaseCard is CItem) != null || attackAbility.AttackSummary?.AddTargetActiveBonuses?.FirstOrDefault((CActiveBonus x) => x.BaseCard is CItem) != null || attackAbility.AttackSummary?.AddRangeActiveBonuses?.FirstOrDefault((CActiveBonus x) => x.BaseCard is CItem) != null)
			{
				attackBuff += Strength;
				xpBuff += XP;
			}
			break;
		}
		default:
			DLLDebug.LogError("Unrecognised Attack Effect Type " + Effect);
			break;
		}
	}

	public bool StackAbilityForBonus(CActor target, CAbilityAttack attackAbility, List<CAbility> stackAbilities)
	{
		if (AttackType == CAbility.EAttackType.Melee && !attackAbility.IsMeleeAttack)
		{
			return false;
		}
		if (AttackType == CAbility.EAttackType.Ranged && attackAbility.IsMeleeAttack)
		{
			return false;
		}
		stackedSubAbilityForBuff = false;
		if (Effect.Equals(EAttackEffect.BuffIfCanPerformSubAbility))
		{
			bool flag = true;
			List<CTile> list = new List<CTile>();
			foreach (CAbility stackAbility in stackAbilities)
			{
				bool flag2 = false;
				if (stackAbility.AbilityType == CAbility.EAbilityType.DestroyObstacle)
				{
					foreach (CTile item in GameState.GetTilesInRange(attackAbility.TargetingActor, stackAbility.Range, stackAbility.Targeting, emptyTilesOnly: false, ignoreBlocked: true))
					{
						CTile propTile = null;
						CObjectObstacle cObjectObstacle = item.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
						if (cObjectObstacle == null)
						{
							cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile);
						}
						if (cObjectObstacle != null && !cObjectObstacle.OverrideDisallowDestroyAndMove && !cObjectObstacle.Activated && (cObjectObstacle.PropHealthDetails == null || !cObjectObstacle.PropHealthDetails.HasHealth))
						{
							flag2 = true;
							list.Add(item);
						}
					}
				}
				else
				{
					DLLDebug.LogError("Unrecognised Ability Type to perform for buff " + Effect);
				}
				flag = flag2;
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(stackAbilities, null, performNow: true, stopPlayerSkipping: true, false, stopPlayerUndo: true);
				stackedSubAbilityForBuff = true;
				actorTargetedForStackedSubAbilityBuff = target;
				if (list.Count == 1)
				{
					PhaseManager.TileSelected(list[0], new List<CTile>());
				}
			}
		}
		return stackedSubAbilityForBuff;
	}

	public void UpdateStackedAbilityForBonusTarget(CActor target)
	{
		actorTargetedForStackedSubAbilityBuff = target;
	}

	public void ResetStackedSubAbilityForBuffOnAttack()
	{
		stackedSubAbilityForBuff = false;
	}

	public CAttackEffect()
	{
	}

	public CAttackEffect(CAttackEffect state, ReferenceDictionary references)
	{
		Effect = state.Effect;
		Strength = state.Strength;
		XP = state.XP;
		Pierce = state.Pierce;
		Filter = references.Get(state.Filter);
		if (Filter == null && state.Filter != null)
		{
			Filter = new CAbilityFilter(state.Filter, references);
			references.Add(state.Filter, Filter);
		}
		NegativeConditions = references.Get(state.NegativeConditions);
		if (NegativeConditions == null && state.NegativeConditions != null)
		{
			NegativeConditions = new List<CCondition.ENegativeCondition>();
			for (int i = 0; i < state.NegativeConditions.Count; i++)
			{
				CCondition.ENegativeCondition item = state.NegativeConditions[i];
				NegativeConditions.Add(item);
			}
			references.Add(state.NegativeConditions, NegativeConditions);
		}
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<CCondition.EPositiveCondition>();
			for (int j = 0; j < state.PositiveConditions.Count; j++)
			{
				CCondition.EPositiveCondition item2 = state.PositiveConditions[j];
				PositiveConditions.Add(item2);
			}
			references.Add(state.PositiveConditions, PositiveConditions);
		}
		AttackType = state.AttackType;
		stackedSubAbilityForBuff = state.stackedSubAbilityForBuff;
	}
}
