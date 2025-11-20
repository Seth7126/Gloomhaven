using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbility : SEvent
{
	[Serializable]
	public class SEventAbilityAddedConditionData : ISerializable
	{
		public CAbility.EAbilityType AddedConditionType { get; private set; }

		public int Duration { get; private set; }

		public EConditionDecTrigger ConditionDecTrigger { get; private set; }

		public SEventAbilityAddedConditionData()
		{
		}

		public SEventAbilityAddedConditionData(SEventAbilityAddedConditionData state, ReferenceDictionary references)
		{
			AddedConditionType = state.AddedConditionType;
			Duration = state.Duration;
			ConditionDecTrigger = state.ConditionDecTrigger;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("AddedConditionType", AddedConditionType);
			info.AddValue("Duration", Duration);
			info.AddValue("ConditionDecTrigger", ConditionDecTrigger);
		}

		public SEventAbilityAddedConditionData(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					switch (current.Name)
					{
					case "AddedConditionType":
						AddedConditionType = (CAbility.EAbilityType)info.GetValue("AddedConditionType", typeof(CAbility.EAbilityType));
						break;
					case "Duration":
						Duration = info.GetInt32("Duration");
						break;
					case "ConditionDecTrigger":
						ConditionDecTrigger = (EConditionDecTrigger)info.GetValue("ConditionDecTrigger", typeof(EConditionDecTrigger));
						break;
					}
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Exception while trying to deserialize SEventAbilityAddedConditionData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public SEventAbilityAddedConditionData(CAbility.EAbilityType conditionType, int duration, EConditionDecTrigger conditionDecTrigger)
		{
			AddedConditionType = conditionType;
			Duration = duration;
			ConditionDecTrigger = conditionDecTrigger;
		}
	}

	public CActor.EType? ActorType;

	public ESESubTypeAbility AbilitySubType { get; private set; }

	public CAbility.EAbilityType AbilityType { get; private set; }

	public string Name { get; private set; }

	public int CardID { get; private set; }

	public CBaseCard.ECardType CardType { get; private set; }

	public string ActorClassID { get; private set; }

	public int Strength { get; private set; }

	public List<SEventAbilityAddedConditionData> AddedPositiveConditionData { get; private set; }

	public List<SEventAbilityAddedConditionData> AddedNegativeConditionData { get; private set; }

	public bool ActorEnemySummon { get; private set; }

	public List<PositiveConditionPair> PositiveConditions { get; private set; }

	public List<NegativeConditionPair> NegativeConditions { get; private set; }

	public string ActedOnByClassID { get; private set; }

	public CActor.EType? ActedOnType { get; private set; }

	public bool ActedOnEnemySummon { get; private set; }

	public List<PositiveConditionPair> ActedOnPositiveConditions { get; private set; }

	public List<NegativeConditionPair> ActedOnNegativeConditions { get; private set; }

	public bool DefaultAction { get; private set; }

	public bool HasHappened { get; private set; }

	public SEventAbility()
	{
	}

	public SEventAbility(SEventAbility state, ReferenceDictionary references)
		: base(state, references)
	{
		AbilitySubType = state.AbilitySubType;
		AbilityType = state.AbilityType;
		Name = state.Name;
		CardID = state.CardID;
		CardType = state.CardType;
		ActorClassID = state.ActorClassID;
		Strength = state.Strength;
		AddedPositiveConditionData = references.Get(state.AddedPositiveConditionData);
		if (AddedPositiveConditionData == null && state.AddedPositiveConditionData != null)
		{
			AddedPositiveConditionData = new List<SEventAbilityAddedConditionData>();
			for (int i = 0; i < state.AddedPositiveConditionData.Count; i++)
			{
				SEventAbilityAddedConditionData sEventAbilityAddedConditionData = state.AddedPositiveConditionData[i];
				SEventAbilityAddedConditionData sEventAbilityAddedConditionData2 = references.Get(sEventAbilityAddedConditionData);
				if (sEventAbilityAddedConditionData2 == null && sEventAbilityAddedConditionData != null)
				{
					sEventAbilityAddedConditionData2 = new SEventAbilityAddedConditionData(sEventAbilityAddedConditionData, references);
					references.Add(sEventAbilityAddedConditionData, sEventAbilityAddedConditionData2);
				}
				AddedPositiveConditionData.Add(sEventAbilityAddedConditionData2);
			}
			references.Add(state.AddedPositiveConditionData, AddedPositiveConditionData);
		}
		AddedNegativeConditionData = references.Get(state.AddedNegativeConditionData);
		if (AddedNegativeConditionData == null && state.AddedNegativeConditionData != null)
		{
			AddedNegativeConditionData = new List<SEventAbilityAddedConditionData>();
			for (int j = 0; j < state.AddedNegativeConditionData.Count; j++)
			{
				SEventAbilityAddedConditionData sEventAbilityAddedConditionData3 = state.AddedNegativeConditionData[j];
				SEventAbilityAddedConditionData sEventAbilityAddedConditionData4 = references.Get(sEventAbilityAddedConditionData3);
				if (sEventAbilityAddedConditionData4 == null && sEventAbilityAddedConditionData3 != null)
				{
					sEventAbilityAddedConditionData4 = new SEventAbilityAddedConditionData(sEventAbilityAddedConditionData3, references);
					references.Add(sEventAbilityAddedConditionData3, sEventAbilityAddedConditionData4);
				}
				AddedNegativeConditionData.Add(sEventAbilityAddedConditionData4);
			}
			references.Add(state.AddedNegativeConditionData, AddedNegativeConditionData);
		}
		ActorType = state.ActorType;
		ActorEnemySummon = state.ActorEnemySummon;
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
			for (int k = 0; k < state.PositiveConditions.Count; k++)
			{
				PositiveConditionPair positiveConditionPair = state.PositiveConditions[k];
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
			for (int l = 0; l < state.NegativeConditions.Count; l++)
			{
				NegativeConditionPair negativeConditionPair = state.NegativeConditions[l];
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
		ActedOnByClassID = state.ActedOnByClassID;
		ActedOnType = state.ActedOnType;
		ActedOnEnemySummon = state.ActedOnEnemySummon;
		ActedOnPositiveConditions = references.Get(state.ActedOnPositiveConditions);
		if (ActedOnPositiveConditions == null && state.ActedOnPositiveConditions != null)
		{
			ActedOnPositiveConditions = new List<PositiveConditionPair>();
			for (int m = 0; m < state.ActedOnPositiveConditions.Count; m++)
			{
				PositiveConditionPair positiveConditionPair3 = state.ActedOnPositiveConditions[m];
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
			for (int n = 0; n < state.ActedOnNegativeConditions.Count; n++)
			{
				NegativeConditionPair negativeConditionPair3 = state.ActedOnNegativeConditions[n];
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
		DefaultAction = state.DefaultAction;
		HasHappened = state.HasHappened;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("AbilityType", AbilityType);
		info.AddValue("AbilitySubType", AbilitySubType);
		info.AddValue("Name", Name);
		info.AddValue("CardID", CardID);
		info.AddValue("CardType", CardType);
		info.AddValue("ActorClassID", ActorClassID);
		info.AddValue("Strength", Strength);
		info.AddValue("AddedPositiveConditionData", AddedPositiveConditionData);
		info.AddValue("AddedNegativeConditionData", AddedNegativeConditionData);
		info.AddValue("ActorType", ActorType);
		info.AddValue("ActorEnemySummon", ActorEnemySummon);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("ActedOnByClassID", ActedOnByClassID);
		info.AddValue("ActedOnType", ActedOnType);
		info.AddValue("ActedOnEnemySummon", ActedOnEnemySummon);
		info.AddValue("ActedOnPositiveConditions", ActedOnPositiveConditions);
		info.AddValue("ActedOnNegativeConditions", ActedOnNegativeConditions);
		info.AddValue("DefaultAction", DefaultAction);
		info.AddValue("HasHappened", HasHappened);
	}

	public SEventAbility(SerializationInfo info, StreamingContext context)
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
				case "AbilityType":
					AbilityType = (CAbility.EAbilityType)info.GetValue("AbilityType", typeof(CAbility.EAbilityType));
					break;
				case "AbilitySubType":
					AbilitySubType = (ESESubTypeAbility)info.GetValue("AbilitySubType", typeof(ESESubTypeAbility));
					break;
				case "Name":
					Name = info.GetString("Name");
					break;
				case "CardID":
					CardID = info.GetInt32("CardID");
					break;
				case "CardType":
					CardType = (CBaseCard.ECardType)info.GetValue("CardType", typeof(CBaseCard.ECardType));
					break;
				case "ActorClassID":
					ActorClassID = info.GetString("ActorClassID");
					break;
				case "Strength":
					Strength = info.GetInt32("Strength");
					break;
				case "AddedPositiveConditionData":
					AddedPositiveConditionData = (List<SEventAbilityAddedConditionData>)info.GetValue("AddedPositiveConditionData", typeof(List<SEventAbilityAddedConditionData>));
					break;
				case "AddedNegativeConditionData":
					AddedNegativeConditionData = (List<SEventAbilityAddedConditionData>)info.GetValue("AddedNegativeConditionData", typeof(List<SEventAbilityAddedConditionData>));
					break;
				case "ActorType":
					ActorType = (CActor.EType?)info.GetValue("ActorType", typeof(CActor.EType?));
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
				case "ActedOnByClassID":
					ActedOnByClassID = info.GetString("ActedOnByClassID");
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
				case "DefaultAction":
					DefaultAction = info.GetBoolean("DefaultAction");
					break;
				case "HasHappened":
					HasHappened = info.GetBoolean("HasHappened");
					break;
				case "ActorClassName":
				{
					string text2 = info.GetString("ActorClassName").Replace(" ", string.Empty);
					ActorClassID = text2 + "ID";
					break;
				}
				case "ActedOnByClass":
				{
					string text = info.GetString("ActedOnByClass").Replace(" ", string.Empty);
					ActedOnByClassID = text + "ID";
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize SEventAbility entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (AddedPositiveConditionData == null)
		{
			AddedPositiveConditionData = new List<SEventAbilityAddedConditionData>();
		}
		if (AddedNegativeConditionData == null)
		{
			AddedNegativeConditionData = new List<SEventAbilityAddedConditionData>();
		}
	}

	public SEventAbility(CAbility.EAbilityType abilityType, ESESubTypeAbility abilitySubType, string name, int cardID, CBaseCard.ECardType cardType, string actorClassID, int strength, List<CAbility> positiveConditions, List<CAbility> negativeConditions, CActor.EType? actorType, bool IsSummon, List<PositiveConditionPair> actorpositiveConditions, List<NegativeConditionPair> actornegativeConditions, string actedOnClassID, CActor.EType actedOnType, bool ActedOnIsSummon, List<PositiveConditionPair> ActedOnpositiveConditions, List<NegativeConditionPair> ActedOnnegativeConditions, string text = "", bool defaultAction = false, bool hasHappened = true)
		: base(ESEType.Ability, text)
	{
		AbilityType = abilityType;
		AbilitySubType = abilitySubType;
		Name = name;
		CardID = cardID;
		CardType = cardType;
		ActorClassID = actorClassID;
		Strength = strength;
		ActorType = actorType;
		ActorEnemySummon = IsSummon;
		ActedOnByClassID = actedOnClassID;
		ActedOnType = actedOnType;
		ActedOnEnemySummon = ActedOnIsSummon;
		DefaultAction = defaultAction;
		HasHappened = hasHappened;
		AddedPositiveConditionData = new List<SEventAbilityAddedConditionData>();
		if (positiveConditions != null && positiveConditions.Count > 0)
		{
			foreach (CAbility positiveCondition in positiveConditions)
			{
				CAbilityCondition cAbilityCondition = positiveCondition as CAbilityCondition;
				AddedPositiveConditionData.Add(new SEventAbilityAddedConditionData(cAbilityCondition.AbilityType, cAbilityCondition.Duration, cAbilityCondition.DecrementTrigger));
			}
		}
		AddedNegativeConditionData = new List<SEventAbilityAddedConditionData>();
		if (negativeConditions != null && negativeConditions.Count > 0)
		{
			foreach (CAbility negativeCondition in negativeConditions)
			{
				CAbilityCondition cAbilityCondition2 = negativeCondition as CAbilityCondition;
				AddedNegativeConditionData.Add(new SEventAbilityAddedConditionData(cAbilityCondition2.AbilityType, cAbilityCondition2.Duration, cAbilityCondition2.DecrementTrigger));
			}
		}
		NegativeConditions = new List<NegativeConditionPair>();
		if (actornegativeConditions != null && actornegativeConditions.Count > 0)
		{
			foreach (NegativeConditionPair actornegativeCondition in actornegativeConditions)
			{
				NegativeConditions.Add(new NegativeConditionPair(actornegativeCondition.NegativeCondition, actornegativeCondition.MapDuration, actornegativeCondition.RoundDuration, actornegativeCondition.ConditionDecTrigger));
			}
		}
		PositiveConditions = new List<PositiveConditionPair>();
		if (actorpositiveConditions != null && actorpositiveConditions.Count > 0)
		{
			foreach (PositiveConditionPair actorpositiveCondition in actorpositiveConditions)
			{
				PositiveConditions.Add(new PositiveConditionPair(actorpositiveCondition.PositiveCondition, actorpositiveCondition.MapDuration, actorpositiveCondition.RoundDuration, actorpositiveCondition.ConditionDecTrigger));
			}
		}
		ActedOnNegativeConditions = new List<NegativeConditionPair>();
		if (ActedOnnegativeConditions != null && ActedOnnegativeConditions.Count > 0)
		{
			foreach (NegativeConditionPair ActedOnnegativeCondition in ActedOnnegativeConditions)
			{
				ActedOnNegativeConditions.Add(new NegativeConditionPair(ActedOnnegativeCondition.NegativeCondition, ActedOnnegativeCondition.MapDuration, ActedOnnegativeCondition.RoundDuration, ActedOnnegativeCondition.ConditionDecTrigger));
			}
		}
		ActedOnPositiveConditions = new List<PositiveConditionPair>();
		if (ActedOnpositiveConditions == null || ActedOnpositiveConditions.Count <= 0)
		{
			return;
		}
		foreach (PositiveConditionPair ActedOnpositiveCondition in ActedOnpositiveConditions)
		{
			ActedOnPositiveConditions.Add(new PositiveConditionPair(ActedOnpositiveCondition.PositiveCondition, ActedOnpositiveCondition.MapDuration, ActedOnpositiveCondition.RoundDuration, ActedOnpositiveCondition.ConditionDecTrigger));
		}
	}
}
