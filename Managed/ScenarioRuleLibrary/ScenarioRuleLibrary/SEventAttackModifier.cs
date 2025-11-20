using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAttackModifier : SEvent
{
	[Serializable]
	public class OverrideBuffData : ISerializable
	{
		public enum EOverrideBuffType
		{
			Conditional,
			Item,
			ActiveBonus
		}

		public EOverrideBuffType OverrideBuffType { get; set; }

		public string OverrideBuffName { get; set; }

		public int OverrideStrength { get; set; }

		public List<CCondition.ENegativeCondition> OverrideNegativeConditions { get; private set; }

		public List<CCondition.EPositiveCondition> OverridePositiveConditions { get; private set; }

		public bool LogFullName { get; set; }

		public string OverrideSourceActorClassID { get; set; }

		public OverrideBuffData()
		{
		}

		public OverrideBuffData(OverrideBuffData state, ReferenceDictionary references)
		{
			OverrideBuffType = state.OverrideBuffType;
			OverrideBuffName = state.OverrideBuffName;
			OverrideStrength = state.OverrideStrength;
			OverrideNegativeConditions = references.Get(state.OverrideNegativeConditions);
			if (OverrideNegativeConditions == null && state.OverrideNegativeConditions != null)
			{
				OverrideNegativeConditions = new List<CCondition.ENegativeCondition>();
				for (int i = 0; i < state.OverrideNegativeConditions.Count; i++)
				{
					CCondition.ENegativeCondition item = state.OverrideNegativeConditions[i];
					OverrideNegativeConditions.Add(item);
				}
				references.Add(state.OverrideNegativeConditions, OverrideNegativeConditions);
			}
			OverridePositiveConditions = references.Get(state.OverridePositiveConditions);
			if (OverridePositiveConditions == null && state.OverridePositiveConditions != null)
			{
				OverridePositiveConditions = new List<CCondition.EPositiveCondition>();
				for (int j = 0; j < state.OverridePositiveConditions.Count; j++)
				{
					CCondition.EPositiveCondition item2 = state.OverridePositiveConditions[j];
					OverridePositiveConditions.Add(item2);
				}
				references.Add(state.OverridePositiveConditions, OverridePositiveConditions);
			}
			LogFullName = state.LogFullName;
			OverrideSourceActorClassID = state.OverrideSourceActorClassID;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("OverrideBuffType", OverrideBuffType);
			info.AddValue("OverrideBuffName", OverrideBuffName);
			info.AddValue("OverrideStrength", OverrideStrength);
			info.AddValue("OverrideNegativeConditions", OverrideNegativeConditions);
			info.AddValue("OverridePositiveConditions", OverridePositiveConditions);
			info.AddValue("OverrideSourceActorClassID", OverrideSourceActorClassID);
		}

		public OverrideBuffData(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					switch (current.Name)
					{
					case "OverrideBuffType":
						OverrideBuffType = (EOverrideBuffType)info.GetValue("OverrideBuffType", typeof(EOverrideBuffType));
						break;
					case "OverrideBuffName":
						OverrideBuffName = info.GetString("OverrideBuffName");
						break;
					case "OverrideStrength":
						OverrideStrength = info.GetInt32("OverrideStrength");
						break;
					case "OverrideNegativeConditions":
						OverrideNegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("OverrideNegativeConditions", typeof(List<CCondition.ENegativeCondition>));
						break;
					case "OverridePositiveConditions":
						OverridePositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("OverridePositiveConditions", typeof(List<CCondition.EPositiveCondition>));
						break;
					case "OverrideSourceActorClassID":
						OverrideSourceActorClassID = info.GetString("OverrideSourceActorClassID");
						break;
					}
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Exception while trying to deserialize OverrideBuffData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public OverrideBuffData(EOverrideBuffType overrideBuffType, string overrideBuffName, int overrideStrength, List<CCondition.ENegativeCondition> overrideNegativeConditions, List<CCondition.EPositiveCondition> overridePositiveConditions, string overrideSourceActorClassID = null, bool logFullName = true)
		{
			OverrideBuffType = overrideBuffType;
			OverrideBuffName = overrideBuffName;
			OverrideStrength = overrideStrength;
			OverrideNegativeConditions = overrideNegativeConditions;
			OverridePositiveConditions = overridePositiveConditions;
			OverrideSourceActorClassID = overrideSourceActorClassID;
			LogFullName = logFullName;
		}
	}

	public int PreModifierAttackStrength { get; private set; }

	public int FinalAttackStrength { get; set; }

	public int AttackAbilityAttackStrength { get; set; }

	public List<OverrideBuffData> OverrideBuffs { get; set; }

	public EAdvantageStatuses AdvantageStatus { get; private set; }

	public List<string> UsedAttackModifierStrings { get; private set; }

	public List<string> UsedAttackModifierStringsModified { get; private set; }

	public List<string> UnusedAttackModifierStrings { get; private set; }

	public int TargetActorID { get; set; }

	public int TargetActorShield { get; set; }

	public int DrawingActorPierce { get; set; }

	public int HealFromModifiers { get; private set; }

	public int ShieldFromModifiers { get; private set; }

	public int PierceFromModifers { get; private set; }

	public int FinalPierce { get; set; }

	public bool TargetActorWasPoisoned { get; set; }

	public bool TargetActorWasInvulnerable { get; set; }

	public List<CCondition.ENegativeCondition> NegativeConditionsFromModifiers { get; private set; }

	public List<CCondition.EPositiveCondition> PositiveConditionsFromModifiers { get; private set; }

	public List<ElementInfusionBoardManager.EElement> ElementsInfusedFromModifiers { get; private set; }

	public string ActorClass { get; private set; }

	public CActor.EType ActorType { get; private set; }

	public bool ActorEnemySummon { get; private set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	public string ActedOnByClass { get; private set; }

	public CActor.EType? ActedOnType { get; private set; }

	public bool ActedOnEnemySummon { get; private set; }

	public List<PositiveConditionPair> ActedOnPositiveConditions { get; private set; }

	public List<NegativeConditionPair> ActedOnNegativeConditions { get; private set; }

	public int AttackIndex { get; set; }

	public SEventAttackModifier(SEventAttackModifier state, ReferenceDictionary references)
		: base(state, references)
	{
		PreModifierAttackStrength = state.PreModifierAttackStrength;
		FinalAttackStrength = state.FinalAttackStrength;
		AttackAbilityAttackStrength = state.AttackAbilityAttackStrength;
		OverrideBuffs = references.Get(state.OverrideBuffs);
		if (OverrideBuffs == null && state.OverrideBuffs != null)
		{
			OverrideBuffs = new List<OverrideBuffData>();
			for (int i = 0; i < state.OverrideBuffs.Count; i++)
			{
				OverrideBuffData overrideBuffData = state.OverrideBuffs[i];
				OverrideBuffData overrideBuffData2 = references.Get(overrideBuffData);
				if (overrideBuffData2 == null && overrideBuffData != null)
				{
					overrideBuffData2 = new OverrideBuffData(overrideBuffData, references);
					references.Add(overrideBuffData, overrideBuffData2);
				}
				OverrideBuffs.Add(overrideBuffData2);
			}
			references.Add(state.OverrideBuffs, OverrideBuffs);
		}
		AdvantageStatus = state.AdvantageStatus;
		UsedAttackModifierStrings = references.Get(state.UsedAttackModifierStrings);
		if (UsedAttackModifierStrings == null && state.UsedAttackModifierStrings != null)
		{
			UsedAttackModifierStrings = new List<string>();
			for (int j = 0; j < state.UsedAttackModifierStrings.Count; j++)
			{
				string item = state.UsedAttackModifierStrings[j];
				UsedAttackModifierStrings.Add(item);
			}
			references.Add(state.UsedAttackModifierStrings, UsedAttackModifierStrings);
		}
		UsedAttackModifierStringsModified = references.Get(state.UsedAttackModifierStringsModified);
		if (UsedAttackModifierStringsModified == null && state.UsedAttackModifierStringsModified != null)
		{
			UsedAttackModifierStringsModified = new List<string>();
			for (int k = 0; k < state.UsedAttackModifierStringsModified.Count; k++)
			{
				string item2 = state.UsedAttackModifierStringsModified[k];
				UsedAttackModifierStringsModified.Add(item2);
			}
			references.Add(state.UsedAttackModifierStringsModified, UsedAttackModifierStringsModified);
		}
		UnusedAttackModifierStrings = references.Get(state.UnusedAttackModifierStrings);
		if (UnusedAttackModifierStrings == null && state.UnusedAttackModifierStrings != null)
		{
			UnusedAttackModifierStrings = new List<string>();
			for (int l = 0; l < state.UnusedAttackModifierStrings.Count; l++)
			{
				string item3 = state.UnusedAttackModifierStrings[l];
				UnusedAttackModifierStrings.Add(item3);
			}
			references.Add(state.UnusedAttackModifierStrings, UnusedAttackModifierStrings);
		}
		TargetActorID = state.TargetActorID;
		TargetActorShield = state.TargetActorShield;
		DrawingActorPierce = state.DrawingActorPierce;
		HealFromModifiers = state.HealFromModifiers;
		ShieldFromModifiers = state.ShieldFromModifiers;
		PierceFromModifers = state.PierceFromModifers;
		FinalPierce = state.FinalPierce;
		TargetActorWasPoisoned = state.TargetActorWasPoisoned;
		TargetActorWasInvulnerable = state.TargetActorWasInvulnerable;
		NegativeConditionsFromModifiers = references.Get(state.NegativeConditionsFromModifiers);
		if (NegativeConditionsFromModifiers == null && state.NegativeConditionsFromModifiers != null)
		{
			NegativeConditionsFromModifiers = new List<CCondition.ENegativeCondition>();
			for (int m = 0; m < state.NegativeConditionsFromModifiers.Count; m++)
			{
				CCondition.ENegativeCondition item4 = state.NegativeConditionsFromModifiers[m];
				NegativeConditionsFromModifiers.Add(item4);
			}
			references.Add(state.NegativeConditionsFromModifiers, NegativeConditionsFromModifiers);
		}
		PositiveConditionsFromModifiers = references.Get(state.PositiveConditionsFromModifiers);
		if (PositiveConditionsFromModifiers == null && state.PositiveConditionsFromModifiers != null)
		{
			PositiveConditionsFromModifiers = new List<CCondition.EPositiveCondition>();
			for (int n = 0; n < state.PositiveConditionsFromModifiers.Count; n++)
			{
				CCondition.EPositiveCondition item5 = state.PositiveConditionsFromModifiers[n];
				PositiveConditionsFromModifiers.Add(item5);
			}
			references.Add(state.PositiveConditionsFromModifiers, PositiveConditionsFromModifiers);
		}
		ElementsInfusedFromModifiers = references.Get(state.ElementsInfusedFromModifiers);
		if (ElementsInfusedFromModifiers == null && state.ElementsInfusedFromModifiers != null)
		{
			ElementsInfusedFromModifiers = new List<ElementInfusionBoardManager.EElement>();
			for (int num = 0; num < state.ElementsInfusedFromModifiers.Count; num++)
			{
				ElementInfusionBoardManager.EElement item6 = state.ElementsInfusedFromModifiers[num];
				ElementsInfusedFromModifiers.Add(item6);
			}
			references.Add(state.ElementsInfusedFromModifiers, ElementsInfusedFromModifiers);
		}
		ActorClass = state.ActorClass;
		ActorType = state.ActorType;
		ActorEnemySummon = state.ActorEnemySummon;
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
			for (int num2 = 0; num2 < state.PositiveConditions.Count; num2++)
			{
				PositiveConditionPair positiveConditionPair = state.PositiveConditions[num2];
				PositiveConditionPair positiveConditionPair2 = references.Get(positiveConditionPair);
				if (positiveConditionPair2 == null && positiveConditionPair != null)
				{
					positiveConditionPair2 = new PositiveConditionPair(positiveConditionPair, references);
					references.Add(positiveConditionPair, positiveConditionPair2);
				}
				PositiveConditions.Add(positiveConditionPair2);
			}
			references.Add(state.PositiveConditions, PositiveConditions);
		}
		NegativeConditions = references.Get(state.NegativeConditions);
		if (NegativeConditions == null && state.NegativeConditions != null)
		{
			NegativeConditions = new List<NegativeConditionPair>();
			for (int num3 = 0; num3 < state.NegativeConditions.Count; num3++)
			{
				NegativeConditionPair negativeConditionPair = state.NegativeConditions[num3];
				NegativeConditionPair negativeConditionPair2 = references.Get(negativeConditionPair);
				if (negativeConditionPair2 == null && negativeConditionPair != null)
				{
					negativeConditionPair2 = new NegativeConditionPair(negativeConditionPair, references);
					references.Add(negativeConditionPair, negativeConditionPair2);
				}
				NegativeConditions.Add(negativeConditionPair2);
			}
			references.Add(state.NegativeConditions, NegativeConditions);
		}
		ActedOnByClass = state.ActedOnByClass;
		ActedOnType = state.ActedOnType;
		ActedOnEnemySummon = state.ActedOnEnemySummon;
		ActedOnPositiveConditions = references.Get(state.ActedOnPositiveConditions);
		if (ActedOnPositiveConditions == null && state.ActedOnPositiveConditions != null)
		{
			ActedOnPositiveConditions = new List<PositiveConditionPair>();
			for (int num4 = 0; num4 < state.ActedOnPositiveConditions.Count; num4++)
			{
				PositiveConditionPair positiveConditionPair3 = state.ActedOnPositiveConditions[num4];
				PositiveConditionPair positiveConditionPair4 = references.Get(positiveConditionPair3);
				if (positiveConditionPair4 == null && positiveConditionPair3 != null)
				{
					positiveConditionPair4 = new PositiveConditionPair(positiveConditionPair3, references);
					references.Add(positiveConditionPair3, positiveConditionPair4);
				}
				ActedOnPositiveConditions.Add(positiveConditionPair4);
			}
			references.Add(state.ActedOnPositiveConditions, ActedOnPositiveConditions);
		}
		ActedOnNegativeConditions = references.Get(state.ActedOnNegativeConditions);
		if (ActedOnNegativeConditions == null && state.ActedOnNegativeConditions != null)
		{
			ActedOnNegativeConditions = new List<NegativeConditionPair>();
			for (int num5 = 0; num5 < state.ActedOnNegativeConditions.Count; num5++)
			{
				NegativeConditionPair negativeConditionPair3 = state.ActedOnNegativeConditions[num5];
				NegativeConditionPair negativeConditionPair4 = references.Get(negativeConditionPair3);
				if (negativeConditionPair4 == null && negativeConditionPair3 != null)
				{
					negativeConditionPair4 = new NegativeConditionPair(negativeConditionPair3, references);
					references.Add(negativeConditionPair3, negativeConditionPair4);
				}
				ActedOnNegativeConditions.Add(negativeConditionPair4);
			}
			references.Add(state.ActedOnNegativeConditions, ActedOnNegativeConditions);
		}
		AttackIndex = state.AttackIndex;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PreModifierAttackStrength", PreModifierAttackStrength);
		info.AddValue("FinalAttackStrength", FinalAttackStrength);
		info.AddValue("AttackAbilityAttackStrength", AttackAbilityAttackStrength);
		info.AddValue("OverrideBuffs", OverrideBuffs);
		info.AddValue("AdvantageStatus", AdvantageStatus);
		info.AddValue("UsedAttackModifierStrings", UsedAttackModifierStrings);
		info.AddValue("UsedAttackModifierStringsModified", UsedAttackModifierStringsModified);
		info.AddValue("UnusedAttackModifierStrings", UnusedAttackModifierStrings);
		info.AddValue("TargetActorID", TargetActorID);
		info.AddValue("TargetActorShield", TargetActorShield);
		info.AddValue("DrawingActorPierce", DrawingActorPierce);
		info.AddValue("HealFromModifiers", HealFromModifiers);
		info.AddValue("ShieldFromModifiers", ShieldFromModifiers);
		info.AddValue("PierceFromModifiers", PierceFromModifers);
		info.AddValue("FinalPierce", FinalPierce);
		info.AddValue("TargetActorWasPoisoned", TargetActorWasPoisoned);
		info.AddValue("TargetActorWasInvulnerable", TargetActorWasInvulnerable);
		info.AddValue("NegativeConditionsFromModifiers", NegativeConditionsFromModifiers);
		info.AddValue("PositiveConditionsFromModifiers", PositiveConditionsFromModifiers);
		info.AddValue("ElementsInfusedFromModifiers", ElementsInfusedFromModifiers);
		info.AddValue("ActorClass", ActorClass);
		info.AddValue("ActorType", ActorType);
		info.AddValue("ActorEnemySummon", ActorEnemySummon);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("ActedOnByClass", ActedOnByClass);
		info.AddValue("ActedOnType", ActedOnType);
		info.AddValue("ActedOnEnemySummon", ActedOnEnemySummon);
		info.AddValue("ActedOnPositiveConditions", ActedOnPositiveConditions);
		info.AddValue("ActedOnNegativeConditions", ActedOnNegativeConditions);
		info.AddValue("AttackIndex", AttackIndex);
	}

	public SEventAttackModifier(SerializationInfo info, StreamingContext context)
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
				case "PreModifierAttackStrength":
					PreModifierAttackStrength = info.GetInt32("PreModifierAttackStrength");
					break;
				case "FinalAttackStrength":
					FinalAttackStrength = info.GetInt32("FinalAttackStrength");
					break;
				case "AttackAbilityAttackStrength":
					AttackAbilityAttackStrength = info.GetInt32("AttackAbilityAttackStrength");
					break;
				case "OverrideBuffs":
					OverrideBuffs = (List<OverrideBuffData>)info.GetValue("OverrideBuffs", typeof(List<OverrideBuffData>));
					break;
				case "AdvantageStatus":
					AdvantageStatus = (EAdvantageStatuses)info.GetValue("AdvantageStatus", typeof(EAdvantageStatuses));
					break;
				case "UsedAttackModifierStrings":
					UsedAttackModifierStrings = (List<string>)info.GetValue("UsedAttackModifierStrings", typeof(List<string>));
					break;
				case "UsedAttackModifierStringsModified":
					UsedAttackModifierStringsModified = (List<string>)info.GetValue("UsedAttackModifierStringsModified", typeof(List<string>));
					break;
				case "UnusedAttackModifierStrings":
					UnusedAttackModifierStrings = (List<string>)info.GetValue("UnusedAttackModifierStrings", typeof(List<string>));
					break;
				case "TargetActorID":
					TargetActorID = info.GetInt32("TargetActorID");
					break;
				case "TargetActorShield":
					TargetActorShield = info.GetInt32("TargetActorShield");
					break;
				case "DrawingActorPierce":
					DrawingActorPierce = info.GetInt32("DrawingActorPierce");
					break;
				case "HealFromModifiers":
					HealFromModifiers = info.GetInt32("HealFromModifiers");
					break;
				case "ShieldFromModifiers":
					ShieldFromModifiers = info.GetInt32("ShieldFromModifiers");
					break;
				case "PierceFromModifers":
					PierceFromModifers = info.GetInt32("PierceFromModifers");
					break;
				case "FinalPierce":
					FinalPierce = info.GetInt32("FinalPierce");
					break;
				case "TargetActorWasPoisoned":
					TargetActorWasPoisoned = info.GetBoolean("TargetActorWasPoisoned");
					break;
				case "TargetActorWasInvulnerable":
					TargetActorWasInvulnerable = info.GetBoolean("TargetActorWasInvulnerable");
					break;
				case "NegativeConditionsFromModifiers":
					NegativeConditionsFromModifiers = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditionsFromModifiers", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "PositiveConditionsFromModifiers":
					PositiveConditionsFromModifiers = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditionsFromModifiers", typeof(List<CCondition.EPositiveCondition>));
					break;
				case "ElementsInfusedFromModifiers":
					ElementsInfusedFromModifiers = (List<ElementInfusionBoardManager.EElement>)info.GetValue("ElementsInfusedFromModifiers", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				case "ActorClass":
					ActorClass = info.GetString("ActorClass");
					break;
				case "ActorType":
					ActorType = (CActor.EType)info.GetValue("ActorType", typeof(CActor.EType));
					break;
				case "ActorEnemySummon":
					ActorEnemySummon = info.GetBoolean("ActorEnemySummon");
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "ActedOnByClass":
					ActedOnByClass = info.GetString("ActedOnByClass");
					break;
				case "ActedOnType":
					ActedOnType = (CActor.EType?)info.GetValue("ActedOnType", typeof(CActor.EType?));
					break;
				case "ActedOnEnemySummon":
					ActedOnEnemySummon = info.GetBoolean("ActedOnEnemySummon");
					break;
				case "ActedOnPositiveConditions":
					ActedOnPositiveConditions = (List<PositiveConditionPair>)info.GetValue("ActedOnPositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "ActedOnNegativeConditions":
					ActedOnNegativeConditions = (List<NegativeConditionPair>)info.GetValue("ActedOnNegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "AttackIndex":
					AttackIndex = info.GetInt32("AttackIndex");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAttackModifier entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAttackModifier(int initialAttackStrength, EAdvantageStatuses advantageStatuses, List<string> attackModifierStrings, List<string> attackModifierStringsModified, List<string> unusedAttackModifiersStrings, int targetActorId, int finalAttackStrength, int healFromModifiers, int shieldFromModifiers, int pierceFromModifiers, List<CCondition.ENegativeCondition> negativeConditionsFromModifiers, List<CCondition.EPositiveCondition> positiveConditionsFromModifiers, List<ElementInfusionBoardManager.EElement> elementsInfusedFromModifiers, string actorClass, CActor.EType actorType, bool IsSummon, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, string actedOnClass, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "", int attackIndex = 0)
		: base(ESEType.AttackModifier, text)
	{
		PreModifierAttackStrength = initialAttackStrength;
		AdvantageStatus = advantageStatuses;
		UsedAttackModifierStrings = attackModifierStrings;
		UsedAttackModifierStringsModified = attackModifierStringsModified;
		UnusedAttackModifierStrings = unusedAttackModifiersStrings;
		TargetActorID = targetActorId;
		FinalAttackStrength = finalAttackStrength;
		HealFromModifiers = healFromModifiers;
		ShieldFromModifiers = shieldFromModifiers;
		PierceFromModifers = pierceFromModifiers;
		NegativeConditionsFromModifiers = negativeConditionsFromModifiers;
		PositiveConditionsFromModifiers = positiveConditionsFromModifiers;
		ElementsInfusedFromModifiers = elementsInfusedFromModifiers;
		ActorClass = actorClass;
		ActorType = actorType;
		ActorEnemySummon = IsSummon;
		PositiveConditions = positiveConditions;
		NegativeConditions = negativeConditions;
		ActedOnByClass = actedOnClass;
		ActedOnType = actedOnType;
		ActedOnEnemySummon = ActedOnIsSummon;
		ActedOnPositiveConditions = ActedOnpositiveConditions;
		ActedOnNegativeConditions = ActedOnnegativeConditions;
		AttackIndex = attackIndex;
	}

	public SEventAttackModifier(string text = "")
		: base(ESEType.AttackModifier, text)
	{
	}
}
