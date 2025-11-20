using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{ClassID}")]
public class HeroSummonState : ActorState, ISerializable
{
	public int ID { get; set; }

	public string Summoner { get; private set; }

	public List<ActiveBonusState> OnSummonActiveBonuses { get; private set; }

	public int SummonedOrderIndex { get; private set; }

	public CHeroSummonActor HeroSummon => ScenarioManager.Scenario.AllHeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == base.ActorGuid);

	public HeroSummonYMLData HeroSummonData => ScenarioRuleClient.SRLYML.HeroSummons.SingleOrDefault((HeroSummonYMLData s) => s.ID == base.ClassID);

	public bool IsCompanionSummon
	{
		get
		{
			if (HeroSummon?.Summoner.CharacterClass.CompanionSummonData == null)
			{
				return false;
			}
			return HeroSummonData == HeroSummon.Summoner.CharacterClass.CompanionSummonData;
		}
	}

	public HeroSummonState()
	{
	}

	public HeroSummonState(HeroSummonState state, ReferenceDictionary references)
		: base(state, references)
	{
		ID = state.ID;
		Summoner = state.Summoner;
		OnSummonActiveBonuses = references.Get(state.OnSummonActiveBonuses);
		if (OnSummonActiveBonuses == null && state.OnSummonActiveBonuses != null)
		{
			OnSummonActiveBonuses = new List<ActiveBonusState>();
			for (int i = 0; i < state.OnSummonActiveBonuses.Count; i++)
			{
				ActiveBonusState activeBonusState = state.OnSummonActiveBonuses[i];
				ActiveBonusState activeBonusState2 = references.Get(activeBonusState);
				if (activeBonusState2 == null && activeBonusState != null)
				{
					activeBonusState2 = new ActiveBonusState(activeBonusState, references);
					references.Add(activeBonusState, activeBonusState2);
				}
				OnSummonActiveBonuses.Add(activeBonusState2);
			}
			references.Add(state.OnSummonActiveBonuses, OnSummonActiveBonuses);
		}
		SummonedOrderIndex = state.SummonedOrderIndex;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ID", ID);
		info.AddValue("Summoner", Summoner);
		info.AddValue("OnSummonActiveBonuses", OnSummonActiveBonuses);
		info.AddValue("SummonedOrderIndex", SummonedOrderIndex);
	}

	public HeroSummonState(SerializationInfo info, StreamingContext context)
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
				case "ID":
					ID = info.GetInt32("ID");
					break;
				case "Summoner":
					Summoner = info.GetString("Summoner");
					break;
				case "OnSummonActiveBonuses":
					OnSummonActiveBonuses = (List<ActiveBonusState>)info.GetValue("OnSummonActiveBonuses", typeof(List<ActiveBonusState>));
					break;
				case "SummonedOrderIndex":
					SummonedOrderIndex = info.GetInt32("SummonedOrderIndex");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize HeroSummonState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public HeroSummonState(string classID, int chosenModelIndex, string actorGuid, string mapGuid, TileIndex location, int health, int maxHealth, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, bool playedThisRound, CActor.ECauseOfDeath causeOfDeath, int augmentSlots, string summoner)
		: base(classID, chosenModelIndex, actorGuid, mapGuid, location, health, maxHealth, level, posCons, negCons, playedThisRound, causeOfDeath, augmentSlots)
	{
		Summoner = summoner;
		OnSummonActiveBonuses = new List<ActiveBonusState>();
	}

	public HeroSummonState(CHeroSummonActor heroSummonActor, string mapGuid)
		: base(heroSummonActor.Class.ID, heroSummonActor.ChosenModelIndex, heroSummonActor.ActorGuid, mapGuid, new TileIndex(heroSummonActor.ArrayIndex), heroSummonActor.Health, heroSummonActor.MaxHealth, heroSummonActor.Level, heroSummonActor.Tokens.CheckPositiveTokens.ToList(), heroSummonActor.Tokens.CheckNegativeTokens.ToList(), heroSummonActor.PlayedThisRound, heroSummonActor.CauseOfDeath, heroSummonActor.AugmentSlots)
	{
		Summoner = heroSummonActor.Summoner.ActorGuid;
		OnSummonActiveBonuses = new List<ActiveBonusState>();
		base.IsRevealed = true;
	}

	public void Save(bool initial)
	{
		if (!base.IsRevealed)
		{
			return;
		}
		CHeroSummonActor cHeroSummonActor = (initial ? ScenarioManager.Scenario.InitialHeroSummons.Single((CHeroSummonActor s) => s.ActorGuid == base.ActorGuid) : HeroSummon);
		if (cHeroSummonActor == null)
		{
			return;
		}
		ID = cHeroSummonActor.ID;
		base.Location = new TileIndex(cHeroSummonActor.ArrayIndex.X, cHeroSummonActor.ArrayIndex.Y);
		base.Health = cHeroSummonActor.Health;
		base.MaxHealth = cHeroSummonActor.MaxHealth;
		base.Level = cHeroSummonActor.Level;
		base.PositiveConditions = cHeroSummonActor.Tokens.CheckPositiveTokens.ToList();
		base.NegativeConditions = cHeroSummonActor.Tokens.CheckNegativeTokens.ToList();
		base.PlayedThisRound = cHeroSummonActor.PlayedThisRound;
		base.CauseOfDeath = cHeroSummonActor.CauseOfDeath;
		base.KilledByActorGuid = cHeroSummonActor.KilledByActorGuid;
		base.PhasedOut = cHeroSummonActor.PhasedOut;
		base.Deactivated = cHeroSummonActor.Deactivated;
		Summoner = cHeroSummonActor.Summoner.ActorGuid;
		SummonedOrderIndex = cHeroSummonActor.SummonedOrderIndex;
		OnSummonActiveBonuses = new List<ActiveBonusState>();
		foreach (CActiveBonus activeBonus in cHeroSummonActor.BaseCard.ActiveBonuses)
		{
			if (HeroSummonData.OnSummonAbilities != null && HeroSummonData.OnSummonAbilities.SingleOrDefault((CAbility x) => x.Name == activeBonus.Ability.Name) != null)
			{
				OnSummonActiveBonuses.Add(new ActiveBonusState(activeBonus.ID, cHeroSummonActor.BaseCard.ID, cHeroSummonActor.BaseCard.Name, activeBonus.Ability.Name, activeBonus.Actor.ActorGuid, activeBonus.Caster.ActorGuid, isTopAction: false, activeBonus.Remaining, activeBonus.ActiveBonusStartRound, activeBonus.IsDoom, activeBonus.BespokeBehaviourStrength()));
			}
		}
		base.CharacterResources.Clear();
		foreach (CCharacterResource characterResource in cHeroSummonActor.CharacterResources)
		{
			base.CharacterResources.Add(characterResource.ID, characterResource.Amount);
		}
	}

	public void Load()
	{
		HeroSummon.LoadHeroSummon(this);
	}

	public static List<Tuple<int, string>> Compare(HeroSummonState state1, HeroSummonState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(ActorState.Compare(state1, state2, isMPCompare));
			if (state1.Summoner != state2.Summoner)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 701, "HeroSummonState Summoner does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3] { "Summoner", state1.Summoner, state2.Summoner }
				});
			}
			switch (StateShared.CheckNullsMatch(state1.OnSummonActiveBonuses, state2.OnSummonActiveBonuses))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 706, "HeroSummonState OnSummonActiveBonuses Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"OnSummonActiveBonuses",
						(state1.OnSummonActiveBonuses == null) ? "is null" : "is not null",
						(state2.OnSummonActiveBonuses == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.OnSummonActiveBonuses.Count != state2.OnSummonActiveBonuses.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 707, "HeroSummonState OnSummonActiveBonuses Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Standee ID",
							state1.ID.ToString(),
							state2.ID.ToString()
						},
						new string[3]
						{
							"OnSummonActiveBonuses Count",
							state1.OnSummonActiveBonuses.Count.ToString(),
							state2.OnSummonActiveBonuses.Count.ToString()
						}
					});
					break;
				}
				List<ActiveBonusState> list2 = state1.OnSummonActiveBonuses.ToList();
				List<ActiveBonusState> list3 = state2.OnSummonActiveBonuses.ToList();
				for (int num = list2.Count - 1; num >= 0; num--)
				{
					ActiveBonusState abState1 = list2[num];
					ActiveBonusState activeBonusState = list3.Find((ActiveBonusState s) => s.CardID == abState1.CardID && abState1.CardName.Contains(s.CardName) && s.AbilityName == abState1.AbilityName && abState1.ActorGuid == s.ActorGuid);
					if (activeBonusState == null)
					{
						activeBonusState = list3.Find((ActiveBonusState s) => s.CardID == abState1.CardID && abState1.CardName.Contains(s.CardName.Replace(" ", string.Empty).Replace("'", string.Empty)) && s.AbilityName == abState1.AbilityName && abState1.ActorGuid == s.ActorGuid);
					}
					if (activeBonusState == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 708, "HeroSummonState OnSummonActiveBonuses in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Standee ID",
								state1.ID.ToString(),
								state2.ID.ToString()
							},
							new string[3]
							{
								"ActiveBonus CardID",
								abState1.CardID.ToString(),
								"Missing"
							},
							new string[3] { "ActiveBonus CardName", abState1.CardName, "Missing" },
							new string[3] { "ActiveBonus AbilityName", abState1.AbilityName, "Missing" }
						});
					}
					else
					{
						list.AddRange(ActiveBonusState.Compare(abState1, activeBonusState, state1.ActorGuid, state1.ClassID, isMPCompare));
						list3.Remove(activeBonusState);
					}
					list2.Remove(abState1);
				}
				if (list3.Count <= 0)
				{
					break;
				}
				foreach (ActiveBonusState item in list3)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 708, "HeroSummonState OnSummonActiveBonuses in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Standee ID",
							state1.ID.ToString(),
							state2.ID.ToString()
						},
						new string[3]
						{
							"ActiveBonus CardID",
							"Missing",
							item.CardID.ToString()
						},
						new string[3] { "ActiveBonus CardName", "Missing", item.CardName },
						new string[3] { "ActiveBonus AbilityName", "Missing", item.AbilityName }
					});
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(799, "Exception during HeroSummonState compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
