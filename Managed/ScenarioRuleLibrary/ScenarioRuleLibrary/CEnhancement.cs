using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CEnhancement : ISerializable
{
	public static EEnhancement[] Enhancements = (EEnhancement[])Enum.GetValues(typeof(EEnhancement));

	public static EEnhancementLine[] EnhancementLines = (EEnhancementLine[])Enum.GetValues(typeof(EEnhancementLine));

	public EEnhancement Enhancement { get; set; }

	public EEnhancementLine EnhancementLine { get; private set; }

	public int AbilityCardID { get; private set; }

	public string AbilityName { get; private set; }

	public int EnhancementSlot { get; private set; }

	public int PaidPrice { get; set; }

	public CAbilityCard AbilityCard
	{
		get
		{
			foreach (CCharacterClass @class in CharacterClassManager.Classes)
			{
				CAbilityCard cAbilityCard = @class.FindCardWithID(AbilityCardID);
				if (cAbilityCard != null)
				{
					return cAbilityCard;
				}
			}
			return null;
		}
	}

	public CAbility Ability => AbilityCard.FindAbilityOnCard(AbilityName);

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Enhancement", Enhancement);
		info.AddValue("EnhancementLine", EnhancementLine);
		info.AddValue("AbilityCardID", AbilityCardID);
		info.AddValue("AbilityName", AbilityName);
		info.AddValue("EnhancementSlot", EnhancementSlot);
		info.AddValue("PaidPrice", PaidPrice);
	}

	private CEnhancement(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Enhancement":
					Enhancement = (EEnhancement)info.GetValue("Enhancement", typeof(EEnhancement));
					break;
				case "EnhancementLine":
					EnhancementLine = (EEnhancementLine)info.GetValue("EnhancementLine", typeof(EEnhancementLine));
					break;
				case "AbilityCardID":
					AbilityCardID = info.GetInt32("AbilityCardID");
					break;
				case "AbilityName":
					AbilityName = info.GetString("AbilityName");
					break;
				case "EnhancementSlot":
					EnhancementSlot = info.GetInt32("EnhancementSlot");
					break;
				case "PaidPrice":
					PaidPrice = info.GetInt32("PaidPrice");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CEnhancement entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (Enhancement == EEnhancement.NoEnhancement && PaidPrice != 0)
		{
			DLLDebug.LogWarning("Empty enhancement slot has Paid Price of " + PaidPrice + ".  Enhancement: " + ToString() + "\n" + Environment.StackTrace);
			PaidPrice = 0;
		}
		else if (Enhancement != EEnhancement.NoEnhancement && PaidPrice == 0)
		{
			DLLDebug.LogWarning("Enhancement " + Enhancement.ToString() + " has Paid Price of zero.  Enhancement: " + ToString());
			PaidPrice = TotalCost(Enhancement, AbilityCard, Ability, classPersistent: false);
		}
		try
		{
			if (AbilityCardID == 235 && (AbilityName == "TSummon1" || AbilityName == "TSummon2"))
			{
				AbilityName = "TSummon";
			}
			if (AbilityCardID == 253 && (AbilityName == "TSummon1" || AbilityName == "TSummon2" || AbilityName == "TSummon3"))
			{
				AbilityName = "TSummon";
			}
		}
		catch (Exception ex2)
		{
			DLLDebug.LogError("Exception while trying to deserialize CEnhancement and update multi summon card enhancement IDs\n" + ex2.Message + "\n" + ex2.StackTrace);
			throw ex2;
		}
	}

	public CEnhancement(EEnhancement enhancement, EEnhancementLine enhancementLine, int abilityCardID, string abilityName, int enhancementSlot, int paidPrice = 0)
	{
		Enhancement = enhancement;
		EnhancementLine = enhancementLine;
		AbilityCardID = abilityCardID;
		AbilityName = abilityName;
		EnhancementSlot = enhancementSlot;
		PaidPrice = paidPrice;
		if (Enhancement != EEnhancement.NoEnhancement && PaidPrice == 0)
		{
			DLLDebug.LogWarning("Enhancement " + Enhancement.ToString() + " has Paid Price of zero.  Enhancement: " + ToString() + "\n" + Environment.StackTrace);
		}
		else if (Enhancement == EEnhancement.NoEnhancement && PaidPrice != 0)
		{
			DLLDebug.LogWarning("Empty enhancement slot has Paid Price of " + PaidPrice + ".  Enhancement: " + ToString() + "\n" + Environment.StackTrace);
		}
	}

	public CEnhancement Copy()
	{
		return new CEnhancement(Enhancement, EnhancementLine, AbilityCardID, AbilityName, EnhancementSlot, PaidPrice);
	}

	public void BuyEnhancement(EEnhancement enhancement, int paidPrice)
	{
		Enhancement = enhancement;
		PaidPrice = paidPrice;
		if (Enhancement != EEnhancement.NoEnhancement && PaidPrice == 0)
		{
			DLLDebug.LogWarning("Enhancement " + Enhancement.ToString() + " has Paid Price of zero.  Enhancement: " + ToString() + "\n" + Environment.StackTrace);
		}
		else if (Enhancement == EEnhancement.NoEnhancement && PaidPrice != 0)
		{
			DLLDebug.LogWarning("Empty enhancement slot has Paid Price of " + PaidPrice + ".  Enhancement: " + ToString() + "\n" + Environment.StackTrace);
		}
	}

	public static int BaseCost(EEnhancement enhancement, CAbility ability, bool classPersistent)
	{
		if (enhancement != EEnhancement.Area)
		{
			return ScenarioRuleClient.SRLYML.Enhancements.EnhancementBaseCosts(classPersistent)[enhancement];
		}
		return ScenarioRuleClient.SRLYML.Enhancements.EnhancementBaseCosts(classPersistent)[enhancement] / (ability.AreaEffect.Hexes.Count + ability.AbilityEnhancements.Where((CEnhancement w) => w.Enhancement == EEnhancement.Area).Count());
	}

	public static int PreviousEnhancementCost(CAbilityCard card, CAbility ability, bool classPersistent)
	{
		return ScenarioRuleClient.SRLYML.Enhancements.NumberOfPreviousEnhancementsCost(classPersistent)[Math.Max(Math.Min(card.GetActionAbilities(ability).Select(GetAbilityEnhancedCount).Sum(), ScenarioRuleClient.SRLYML.Enhancements.NumberOfPreviousEnhancementsCost(classPersistent).Count - 1), 0)];
		static int GetAbilityEnhancedCount(CAbility s)
		{
			int num = s.AbilityEnhancements.Count((CEnhancement w) => w.Enhancement != EEnhancement.NoEnhancement);
			if (num == s.AbilityEnhancements.Count)
			{
				return num - 1;
			}
			return num;
		}
	}

	public static int AbilityCardLevelCost(CAbilityCard card, bool classPersistent)
	{
		return ScenarioRuleClient.SRLYML.Enhancements.LevelOfAbilityCardCost(classPersistent)[Math.Max(Math.Min(card.Level - 1, ScenarioRuleClient.SRLYML.Enhancements.LevelOfAbilityCardCost(classPersistent).Count - 1), 0)];
	}

	public static int MultiTargetCost(EEnhancement enhancement, CAbility ability, bool classPersistent)
	{
		bool flag = false;
		if ((ability.NumberTargets > 1 || ability.NumberTargets == -1) && ability.AbilityType != CAbility.EAbilityType.Summon)
		{
			flag = true;
		}
		else if (ability is CAbilityAttack cAbilityAttack && (cAbilityAttack.AllTargetsOnAttackPath || cAbilityAttack.AllTargetsOnMovePath || cAbilityAttack.AllTargetsOnMovePathSameStartAndEnd))
		{
			flag = true;
		}
		else if (ability is CAbilityMove cAbilityMove && (cAbilityMove.AllTargetsOnAttackPath || cAbilityMove.AllTargetsOnMovePath || cAbilityMove.AllTargetsOnMovePathSameStartAndEnd))
		{
			flag = true;
		}
		else if (ability.AreaEffect != null && enhancement != EEnhancement.Area)
		{
			flag = true;
		}
		else if (ability.MiscAbilityData != null && ability.MiscAbilityData.ConsiderMultiTargetForEnhancements.HasValue && ability.MiscAbilityData.ConsiderMultiTargetForEnhancements.Value)
		{
			flag = true;
		}
		if (!flag)
		{
			return 0;
		}
		return BaseCost(enhancement, ability, classPersistent) * (ScenarioRuleClient.SRLYML.Enhancements.MultiTargetMultiplier(classPersistent) - 1);
	}

	public static int TotalCost(EEnhancement enhancement, CAbilityCard card, CAbility ability, bool classPersistent)
	{
		if (card == null || ability == null)
		{
			DLLDebug.LogError("Null Ability Card or Ability sent to TotalCost.\n" + Environment.StackTrace);
			return 0;
		}
		return BaseCost(enhancement, ability, classPersistent) + MultiTargetCost(enhancement, ability, classPersistent) + PreviousEnhancementCost(card, ability, classPersistent) + AbilityCardLevelCost(card, classPersistent);
	}

	public bool Compare(object obj)
	{
		if (obj is CEnhancement cEnhancement)
		{
			if (Enhancement == cEnhancement.Enhancement && EnhancementLine == cEnhancement.EnhancementLine && AbilityCardID == cEnhancement.AbilityCardID && AbilityName == cEnhancement.AbilityName && EnhancementSlot == cEnhancement.EnhancementSlot)
			{
				return PaidPrice == cEnhancement.PaidPrice;
			}
			return false;
		}
		return false;
	}

	public override string ToString()
	{
		return "Enhancement: " + Enhancement.ToString() + ", EnhancementLine: " + EnhancementLine.ToString() + ", AbilityCardID: " + AbilityCardID + ", AbilityName: " + AbilityName.ToString() + ", EnhancementSlot: " + EnhancementSlot + ", PaidPrice: " + PaidPrice;
	}

	public CEnhancement()
	{
	}

	public CEnhancement(CEnhancement state, ReferenceDictionary references)
	{
		Enhancement = state.Enhancement;
		EnhancementLine = state.EnhancementLine;
		AbilityCardID = state.AbilityCardID;
		AbilityName = state.AbilityName;
		EnhancementSlot = state.EnhancementSlot;
		PaidPrice = state.PaidPrice;
	}
}
