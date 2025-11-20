using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAction : SEvent
{
	[Serializable]
	public class SEventAbilityOverrideData : ISerializable
	{
		public CAbility.EAbilityType? AbilityType { get; private set; }

		public int? Strength { get; private set; }

		public int? Range { get; private set; }

		public int? NumberOfTargets { get; private set; }

		public bool? Jump { get; private set; }

		public bool? Fly { get; private set; }

		public List<CCondition.EPositiveCondition> PositiveConditions { get; private set; }

		public List<CCondition.ENegativeCondition> NegativeConditions { get; private set; }

		public bool? AllTargetsOnMovePath { get; private set; }

		public bool? AllTargetsOnAttackPath { get; private set; }

		public int? Pierce { get; private set; }

		public int? RetaliateRange { get; private set; }

		public CAbilityPush.EAdditionalPushEffect? AdditionalPushEffect { get; private set; }

		public int? AdditionalPushEffectDamage { get; private set; }

		public int? AdditionalPushEffectXP { get; private set; }

		public int? ConditionDuration { get; private set; }

		public EConditionDecTrigger? ConditionDecTrigger { get; private set; }

		public SEventAbilityOverrideData()
		{
		}

		public SEventAbilityOverrideData(SEventAbilityOverrideData state, ReferenceDictionary references)
		{
			AbilityType = state.AbilityType;
			Strength = state.Strength;
			Range = state.Range;
			NumberOfTargets = state.NumberOfTargets;
			Jump = state.Jump;
			Fly = state.Fly;
			PositiveConditions = references.Get(state.PositiveConditions);
			if (PositiveConditions == null && state.PositiveConditions != null)
			{
				PositiveConditions = new List<CCondition.EPositiveCondition>();
				for (int i = 0; i < state.PositiveConditions.Count; i++)
				{
					CCondition.EPositiveCondition item = state.PositiveConditions[i];
					PositiveConditions.Add(item);
				}
				references.Add(state.PositiveConditions, PositiveConditions);
			}
			NegativeConditions = references.Get(state.NegativeConditions);
			if (NegativeConditions == null && state.NegativeConditions != null)
			{
				NegativeConditions = new List<CCondition.ENegativeCondition>();
				for (int j = 0; j < state.NegativeConditions.Count; j++)
				{
					CCondition.ENegativeCondition item2 = state.NegativeConditions[j];
					NegativeConditions.Add(item2);
				}
				references.Add(state.NegativeConditions, NegativeConditions);
			}
			AllTargetsOnMovePath = state.AllTargetsOnMovePath;
			AllTargetsOnAttackPath = state.AllTargetsOnAttackPath;
			Pierce = state.Pierce;
			RetaliateRange = state.RetaliateRange;
			AdditionalPushEffect = state.AdditionalPushEffect;
			AdditionalPushEffectDamage = state.AdditionalPushEffectDamage;
			AdditionalPushEffectXP = state.AdditionalPushEffectXP;
			ConditionDuration = state.ConditionDuration;
			ConditionDecTrigger = state.ConditionDecTrigger;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("AbilityType", AbilityType);
			info.AddValue("Strength", Strength);
			info.AddValue("Range", Range);
			info.AddValue("NumberOfTargets", NumberOfTargets);
			info.AddValue("Jump", Jump);
			info.AddValue("Fly", Fly);
			info.AddValue("PositiveConditions", PositiveConditions);
			info.AddValue("NegativeConditions", NegativeConditions);
			info.AddValue("AllTargetsOnMovePath", AllTargetsOnMovePath);
			info.AddValue("AllTargetsOnAttackPath", AllTargetsOnAttackPath);
			info.AddValue("Pierce", Pierce);
			info.AddValue("RetaliateRange", RetaliateRange);
			info.AddValue("AdditionalPushEffect", AdditionalPushEffect);
			info.AddValue("AdditionalPushEffectDamage", AdditionalPushEffectDamage);
			info.AddValue("AdditionalPushEffectXP", AdditionalPushEffectXP);
			info.AddValue("ConditionDuration", ConditionDuration);
			info.AddValue("ConditionDecTrigger", ConditionDecTrigger);
		}

		public SEventAbilityOverrideData(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					switch (current.Name)
					{
					case "AbilityType":
						AbilityType = (CAbility.EAbilityType?)info.GetValue("AbilityType", typeof(CAbility.EAbilityType?));
						break;
					case "Strength":
						Strength = (int?)info.GetValue("Strength", typeof(int?));
						break;
					case "Range":
						Range = (int?)info.GetValue("Range", typeof(int?));
						break;
					case "NumberOfTargets":
						NumberOfTargets = (int?)info.GetValue("NumberOfTargets", typeof(int?));
						break;
					case "Jump":
						Jump = (bool?)info.GetValue("Jump", typeof(bool?));
						break;
					case "Fly":
						Fly = (bool?)info.GetValue("Fly", typeof(bool?));
						break;
					case "PositiveConditions":
						PositiveConditions = (List<CCondition.EPositiveCondition>)info.GetValue("PositiveConditions", typeof(List<CCondition.EPositiveCondition>));
						break;
					case "NegativeConditions":
						NegativeConditions = (List<CCondition.ENegativeCondition>)info.GetValue("NegativeConditions", typeof(List<CCondition.ENegativeCondition>));
						break;
					case "AllTargetsOnMovePath":
						AllTargetsOnMovePath = (bool?)info.GetValue("AllTargetsOnMovePath", typeof(bool?));
						break;
					case "AllTargetsOnAttackPath":
						AllTargetsOnAttackPath = (bool?)info.GetValue("AllTargetsOnAttackPath", typeof(bool?));
						break;
					case "Pierce":
						Pierce = (int?)info.GetValue("Pierce", typeof(int?));
						break;
					case "RetaliateRange":
						RetaliateRange = (int?)info.GetValue("RetaliateRange", typeof(int?));
						break;
					case "AdditionalPushEffect":
						AdditionalPushEffect = (CAbilityPush.EAdditionalPushEffect?)info.GetValue("AdditionalPushEffect", typeof(CAbilityPush.EAdditionalPushEffect?));
						break;
					case "AdditionalPushEffectDamage":
						AdditionalPushEffectDamage = (int?)info.GetValue("AdditionalPushEffectDamage", typeof(int?));
						break;
					case "AdditionalPushEffectXP":
						AdditionalPushEffectXP = (int?)info.GetValue("AdditionalPushEffectXP", typeof(int?));
						break;
					case "ConditionDuration":
						ConditionDuration = (int?)info.GetValue("ConditionDuration", typeof(int?));
						break;
					case "ConditionDecTrigger":
						ConditionDecTrigger = (EConditionDecTrigger?)info.GetValue("ConditionDecTrigger", typeof(EConditionDecTrigger?));
						break;
					}
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Exception while trying to deserialize SEventAbilityOverrideData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
			if (PositiveConditions == null)
			{
				PositiveConditions = new List<CCondition.EPositiveCondition>();
			}
			if (NegativeConditions == null)
			{
				NegativeConditions = new List<CCondition.ENegativeCondition>();
			}
		}

		public SEventAbilityOverrideData(CAbilityOverride abilityOverride)
		{
			AbilityType = abilityOverride.AbilityType;
			Strength = abilityOverride.Strength;
			Range = abilityOverride.Range;
			NumberOfTargets = abilityOverride.NumberOfTargets;
			Jump = abilityOverride.Jump;
			Fly = abilityOverride.Fly;
			PositiveConditions = abilityOverride.PositiveConditions;
			NegativeConditions = abilityOverride.NegativeConditions;
			AllTargetsOnMovePath = abilityOverride.AllTargetsOnMovePath;
			AllTargetsOnAttackPath = abilityOverride.AllTargetsOnAttackPath;
			Pierce = abilityOverride.Pierce;
			RetaliateRange = abilityOverride.RetaliateRange;
			AdditionalPushEffect = abilityOverride.AdditionalPushEffect;
			AdditionalPushEffectDamage = abilityOverride.AdditionalPushEffectDamage;
			AdditionalPushEffectXP = abilityOverride.AdditionalPushEffectXP;
			ConditionDuration = abilityOverride.ConditionDuration;
			ConditionDecTrigger = abilityOverride.ConditionDecTrigger;
		}

		public SEventAbilityOverrideData(CAbility ability)
		{
			AbilityType = ability.AbilityType;
			Strength = ability.Strength;
			Range = ability.Range;
			AllTargetsOnMovePath = ability.AllTargetsOnMovePath;
			AllTargetsOnAttackPath = ability.AllTargetsOnAttackPath;
			PositiveConditions = ability.PositiveConditions.Keys.ToList();
			NegativeConditions = ability.NegativeConditions.Keys.ToList();
			Jump = ((ability is CAbilityMove cAbilityMove) ? new bool?(cAbilityMove.Jump) : ((bool?)null));
			Fly = ((ability is CAbilityMove cAbilityMove2) ? new bool?(cAbilityMove2.Fly) : ((bool?)null));
			Pierce = ((ability is CAbilityAttack cAbilityAttack) ? new int?(cAbilityAttack.Pierce) : ((int?)null));
			NumberOfTargets = null;
			RetaliateRange = null;
			AdditionalPushEffect = null;
			AdditionalPushEffectDamage = null;
			AdditionalPushEffectXP = null;
			ConditionDuration = null;
			ConditionDecTrigger = null;
		}
	}

	[Serializable]
	public class SEventActionAugmentationData : ISerializable
	{
		public List<ElementInfusionBoardManager.EElement> ElementsConsumed { get; private set; }

		public List<SEventActionAugmentationOpData> AugmentationOpDatas { get; private set; }

		public int XP { get; private set; }

		public SEventActionAugmentationData()
		{
		}

		public SEventActionAugmentationData(SEventActionAugmentationData state, ReferenceDictionary references)
		{
			ElementsConsumed = references.Get(state.ElementsConsumed);
			if (ElementsConsumed == null && state.ElementsConsumed != null)
			{
				ElementsConsumed = new List<ElementInfusionBoardManager.EElement>();
				for (int i = 0; i < state.ElementsConsumed.Count; i++)
				{
					ElementInfusionBoardManager.EElement item = state.ElementsConsumed[i];
					ElementsConsumed.Add(item);
				}
				references.Add(state.ElementsConsumed, ElementsConsumed);
			}
			AugmentationOpDatas = references.Get(state.AugmentationOpDatas);
			if (AugmentationOpDatas == null && state.AugmentationOpDatas != null)
			{
				AugmentationOpDatas = new List<SEventActionAugmentationOpData>();
				for (int j = 0; j < state.AugmentationOpDatas.Count; j++)
				{
					SEventActionAugmentationOpData sEventActionAugmentationOpData = state.AugmentationOpDatas[j];
					SEventActionAugmentationOpData sEventActionAugmentationOpData2 = references.Get(sEventActionAugmentationOpData);
					if (sEventActionAugmentationOpData2 == null && sEventActionAugmentationOpData != null)
					{
						sEventActionAugmentationOpData2 = new SEventActionAugmentationOpData(sEventActionAugmentationOpData, references);
						references.Add(sEventActionAugmentationOpData, sEventActionAugmentationOpData2);
					}
					AugmentationOpDatas.Add(sEventActionAugmentationOpData2);
				}
				references.Add(state.AugmentationOpDatas, AugmentationOpDatas);
			}
			XP = state.XP;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ElementsConsumed", ElementsConsumed);
			info.AddValue("AugmentationOpDatas", AugmentationOpDatas);
			info.AddValue("XP", XP);
		}

		public SEventActionAugmentationData(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					switch (current.Name)
					{
					case "ElementsConsumed":
						ElementsConsumed = (List<ElementInfusionBoardManager.EElement>)info.GetValue("ElementsConsumed", typeof(List<ElementInfusionBoardManager.EElement>));
						break;
					case "AugmentationOpDatas":
						AugmentationOpDatas = (List<SEventActionAugmentationOpData>)info.GetValue("AugmentationOpDatas", typeof(List<SEventActionAugmentationOpData>));
						break;
					case "XP":
						XP = info.GetInt32("XP");
						break;
					}
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Exception while trying to deserialize SEventActionAugmentationData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
			if (ElementsConsumed == null)
			{
				ElementsConsumed = new List<ElementInfusionBoardManager.EElement>();
			}
			if (AugmentationOpDatas == null)
			{
				AugmentationOpDatas = new List<SEventActionAugmentationOpData>();
			}
		}

		public SEventActionAugmentationData(List<ElementInfusionBoardManager.EElement> elementsConsumed, List<CActionAugmentationOp> augmentationsOps, int xp)
		{
			ElementsConsumed = elementsConsumed;
			XP = xp;
			AugmentationOpDatas = new List<SEventActionAugmentationOpData>();
			foreach (CActionAugmentationOp augmentationsOp in augmentationsOps)
			{
				AugmentationOpDatas.Add(new SEventActionAugmentationOpData(augmentationsOp.Type, augmentationsOp.AbilityOverride, augmentationsOp.Ability));
			}
		}
	}

	[Serializable]
	public class SEventActionAugmentationOpData : ISerializable
	{
		public CActionAugmentationOp.EActionAugmentationType Type { get; private set; }

		public SEventAbilityOverrideData AbilityOverrideData { get; private set; }

		public SEventActionAugmentationOpData()
		{
		}

		public SEventActionAugmentationOpData(SEventActionAugmentationOpData state, ReferenceDictionary references)
		{
			Type = state.Type;
			AbilityOverrideData = references.Get(state.AbilityOverrideData);
			if (AbilityOverrideData == null && state.AbilityOverrideData != null)
			{
				AbilityOverrideData = new SEventAbilityOverrideData(state.AbilityOverrideData, references);
				references.Add(state.AbilityOverrideData, AbilityOverrideData);
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Type", Type);
			info.AddValue("AbilityOverrideData", AbilityOverrideData);
		}

		public SEventActionAugmentationOpData(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					string name = current.Name;
					if (!(name == "Type"))
					{
						if (name == "AbilityOverrideData")
						{
							AbilityOverrideData = (SEventAbilityOverrideData)info.GetValue("AbilityOverrideData", typeof(SEventAbilityOverrideData));
						}
					}
					else
					{
						Type = (CActionAugmentationOp.EActionAugmentationType)info.GetValue("Type", typeof(CActionAugmentationOp.EActionAugmentationType));
					}
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Exception while trying to deserialize SEventActionAugmentationOpData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public SEventActionAugmentationOpData(CActionAugmentationOp.EActionAugmentationType type, CAbilityOverride abilityOverride, CAbility ability)
		{
			Type = type;
			if (abilityOverride != null && type == CActionAugmentationOp.EActionAugmentationType.AbilityOverride)
			{
				AbilityOverrideData = new SEventAbilityOverrideData(abilityOverride);
			}
			else if (ability != null && type == CActionAugmentationOp.EActionAugmentationType.Ability)
			{
				AbilityOverrideData = new SEventAbilityOverrideData(ability);
			}
		}
	}

	public ESESubTypeAction SubTypeAction { get; private set; }

	public string CurrentPhaseAbilityName { get; private set; }

	public string DefaultAbility { get; private set; }

	public List<SEventActionAugmentationData> ActionAugmentationDatas { get; private set; }

	public SEventAction()
	{
	}

	public SEventAction(SEventAction state, ReferenceDictionary references)
		: base(state, references)
	{
		SubTypeAction = state.SubTypeAction;
		CurrentPhaseAbilityName = state.CurrentPhaseAbilityName;
		DefaultAbility = state.DefaultAbility;
		ActionAugmentationDatas = references.Get(state.ActionAugmentationDatas);
		if (ActionAugmentationDatas != null || state.ActionAugmentationDatas == null)
		{
			return;
		}
		ActionAugmentationDatas = new List<SEventActionAugmentationData>();
		for (int i = 0; i < state.ActionAugmentationDatas.Count; i++)
		{
			SEventActionAugmentationData sEventActionAugmentationData = state.ActionAugmentationDatas[i];
			SEventActionAugmentationData sEventActionAugmentationData2 = references.Get(sEventActionAugmentationData);
			if (sEventActionAugmentationData2 == null && sEventActionAugmentationData != null)
			{
				sEventActionAugmentationData2 = new SEventActionAugmentationData(sEventActionAugmentationData, references);
				references.Add(sEventActionAugmentationData, sEventActionAugmentationData2);
			}
			ActionAugmentationDatas.Add(sEventActionAugmentationData2);
		}
		references.Add(state.ActionAugmentationDatas, ActionAugmentationDatas);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SubTypeAction", SubTypeAction);
		info.AddValue("CurrentPhaseAbilityName", CurrentPhaseAbilityName);
		info.AddValue("DefaultAbility", DefaultAbility);
		info.AddValue("ActionAugmentationDatas", ActionAugmentationDatas);
	}

	public SEventAction(SerializationInfo info, StreamingContext context)
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
				case "SubTypeAction":
					SubTypeAction = (ESESubTypeAction)info.GetValue("SubTypeAction", typeof(ESESubTypeAction));
					break;
				case "CurrentPhaseAbilityName":
					CurrentPhaseAbilityName = info.GetString("CurrentPhaseAbilityName");
					break;
				case "DefaultAbility":
					DefaultAbility = info.GetString("DefaultAbility");
					break;
				case "ActionAugmentationDatas":
					ActionAugmentationDatas = (List<SEventActionAugmentationData>)info.GetValue("ActionAugmentationDatas", typeof(List<SEventActionAugmentationData>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAction entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (ActionAugmentationDatas == null)
		{
			ActionAugmentationDatas = new List<SEventActionAugmentationData>();
		}
	}

	public SEventAction(ESESubTypeAction subTypeAction, string currentPhaseAbilityName, List<CActionAugmentation> actionAugmentations, string text = "", string defaultAbility = "")
		: base(ESEType.Action, text)
	{
		SubTypeAction = subTypeAction;
		CurrentPhaseAbilityName = currentPhaseAbilityName;
		DefaultAbility = defaultAbility;
		ActionAugmentationDatas = new List<SEventActionAugmentationData>();
		foreach (CActionAugmentation actionAugmentation in actionAugmentations)
		{
			ActionAugmentationDatas.Add(new SEventActionAugmentationData(actionAugmentation.Elements, actionAugmentation.AugmentationOps, actionAugmentation.XP));
		}
	}
}
