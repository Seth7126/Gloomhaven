using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class AbilityDeckState : ISerializable
{
	public List<Tuple<int, int>> SelectedAbilityCardIDsAndInstanceIDs { get; private set; }

	public List<Tuple<int, int>> HandAbilityCardIDsAndInstanceIDs { get; private set; }

	public List<Tuple<int, int>> RoundAbilityCardIDsAndInstanceID { get; private set; }

	public List<Tuple<int, CBaseCard.ECardType>> ActivatedAbilityCardIDs { get; private set; }

	public List<int> DiscardedAbilityCardIDs { get; private set; }

	public List<int> LostAbilityCardIDs { get; private set; }

	public List<int> PermaLostAbilityCardIDs { get; private set; }

	public int? InitiativeAbilityCardID { get; private set; }

	public List<ActiveBonusState> ActiveBonuses { get; private set; }

	public List<CEnhancement> Enhancements { get; private set; }

	public bool InitialState { get; private set; }

	public AbilityDeckState()
	{
	}

	public AbilityDeckState(AbilityDeckState state, ReferenceDictionary references)
	{
		SelectedAbilityCardIDsAndInstanceIDs = references.Get(state.SelectedAbilityCardIDsAndInstanceIDs);
		if (SelectedAbilityCardIDsAndInstanceIDs == null && state.SelectedAbilityCardIDsAndInstanceIDs != null)
		{
			SelectedAbilityCardIDsAndInstanceIDs = new List<Tuple<int, int>>();
			for (int i = 0; i < state.SelectedAbilityCardIDsAndInstanceIDs.Count; i++)
			{
				Tuple<int, int> tuple = state.SelectedAbilityCardIDsAndInstanceIDs[i];
				int item = tuple.Item1;
				int item2 = tuple.Item2;
				Tuple<int, int> item3 = new Tuple<int, int>(item, item2);
				SelectedAbilityCardIDsAndInstanceIDs.Add(item3);
			}
			references.Add(state.SelectedAbilityCardIDsAndInstanceIDs, SelectedAbilityCardIDsAndInstanceIDs);
		}
		HandAbilityCardIDsAndInstanceIDs = references.Get(state.HandAbilityCardIDsAndInstanceIDs);
		if (HandAbilityCardIDsAndInstanceIDs == null && state.HandAbilityCardIDsAndInstanceIDs != null)
		{
			HandAbilityCardIDsAndInstanceIDs = new List<Tuple<int, int>>();
			for (int j = 0; j < state.HandAbilityCardIDsAndInstanceIDs.Count; j++)
			{
				Tuple<int, int> tuple2 = state.HandAbilityCardIDsAndInstanceIDs[j];
				int item4 = tuple2.Item1;
				int item5 = tuple2.Item2;
				Tuple<int, int> item6 = new Tuple<int, int>(item4, item5);
				HandAbilityCardIDsAndInstanceIDs.Add(item6);
			}
			references.Add(state.HandAbilityCardIDsAndInstanceIDs, HandAbilityCardIDsAndInstanceIDs);
		}
		RoundAbilityCardIDsAndInstanceID = references.Get(state.RoundAbilityCardIDsAndInstanceID);
		if (RoundAbilityCardIDsAndInstanceID == null && state.RoundAbilityCardIDsAndInstanceID != null)
		{
			RoundAbilityCardIDsAndInstanceID = new List<Tuple<int, int>>();
			for (int k = 0; k < state.RoundAbilityCardIDsAndInstanceID.Count; k++)
			{
				Tuple<int, int> tuple3 = state.RoundAbilityCardIDsAndInstanceID[k];
				int item7 = tuple3.Item1;
				int item8 = tuple3.Item2;
				Tuple<int, int> item9 = new Tuple<int, int>(item7, item8);
				RoundAbilityCardIDsAndInstanceID.Add(item9);
			}
			references.Add(state.RoundAbilityCardIDsAndInstanceID, RoundAbilityCardIDsAndInstanceID);
		}
		ActivatedAbilityCardIDs = references.Get(state.ActivatedAbilityCardIDs);
		if (ActivatedAbilityCardIDs == null && state.ActivatedAbilityCardIDs != null)
		{
			ActivatedAbilityCardIDs = new List<Tuple<int, CBaseCard.ECardType>>();
			for (int l = 0; l < state.ActivatedAbilityCardIDs.Count; l++)
			{
				Tuple<int, CBaseCard.ECardType> tuple4 = state.ActivatedAbilityCardIDs[l];
				int item10 = tuple4.Item1;
				CBaseCard.ECardType item11 = tuple4.Item2;
				Tuple<int, CBaseCard.ECardType> item12 = new Tuple<int, CBaseCard.ECardType>(item10, item11);
				ActivatedAbilityCardIDs.Add(item12);
			}
			references.Add(state.ActivatedAbilityCardIDs, ActivatedAbilityCardIDs);
		}
		DiscardedAbilityCardIDs = references.Get(state.DiscardedAbilityCardIDs);
		if (DiscardedAbilityCardIDs == null && state.DiscardedAbilityCardIDs != null)
		{
			DiscardedAbilityCardIDs = new List<int>();
			for (int m = 0; m < state.DiscardedAbilityCardIDs.Count; m++)
			{
				int item13 = state.DiscardedAbilityCardIDs[m];
				DiscardedAbilityCardIDs.Add(item13);
			}
			references.Add(state.DiscardedAbilityCardIDs, DiscardedAbilityCardIDs);
		}
		LostAbilityCardIDs = references.Get(state.LostAbilityCardIDs);
		if (LostAbilityCardIDs == null && state.LostAbilityCardIDs != null)
		{
			LostAbilityCardIDs = new List<int>();
			for (int n = 0; n < state.LostAbilityCardIDs.Count; n++)
			{
				int item14 = state.LostAbilityCardIDs[n];
				LostAbilityCardIDs.Add(item14);
			}
			references.Add(state.LostAbilityCardIDs, LostAbilityCardIDs);
		}
		PermaLostAbilityCardIDs = references.Get(state.PermaLostAbilityCardIDs);
		if (PermaLostAbilityCardIDs == null && state.PermaLostAbilityCardIDs != null)
		{
			PermaLostAbilityCardIDs = new List<int>();
			for (int num = 0; num < state.PermaLostAbilityCardIDs.Count; num++)
			{
				int item15 = state.PermaLostAbilityCardIDs[num];
				PermaLostAbilityCardIDs.Add(item15);
			}
			references.Add(state.PermaLostAbilityCardIDs, PermaLostAbilityCardIDs);
		}
		InitiativeAbilityCardID = state.InitiativeAbilityCardID;
		ActiveBonuses = references.Get(state.ActiveBonuses);
		if (ActiveBonuses == null && state.ActiveBonuses != null)
		{
			ActiveBonuses = new List<ActiveBonusState>();
			for (int num2 = 0; num2 < state.ActiveBonuses.Count; num2++)
			{
				ActiveBonusState activeBonusState = state.ActiveBonuses[num2];
				ActiveBonusState activeBonusState2 = references.Get(activeBonusState);
				if (activeBonusState2 == null && activeBonusState != null)
				{
					activeBonusState2 = new ActiveBonusState(activeBonusState, references);
					references.Add(activeBonusState, activeBonusState2);
				}
				ActiveBonuses.Add(activeBonusState2);
			}
			references.Add(state.ActiveBonuses, ActiveBonuses);
		}
		Enhancements = references.Get(state.Enhancements);
		if (Enhancements == null && state.Enhancements != null)
		{
			Enhancements = new List<CEnhancement>();
			for (int num3 = 0; num3 < state.Enhancements.Count; num3++)
			{
				CEnhancement cEnhancement = state.Enhancements[num3];
				CEnhancement cEnhancement2 = references.Get(cEnhancement);
				if (cEnhancement2 == null && cEnhancement != null)
				{
					cEnhancement2 = new CEnhancement(cEnhancement, references);
					references.Add(cEnhancement, cEnhancement2);
				}
				Enhancements.Add(cEnhancement2);
			}
			references.Add(state.Enhancements, Enhancements);
		}
		InitialState = state.InitialState;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("SelectedAbilityCardIDsAndInstanceIDs", SelectedAbilityCardIDsAndInstanceIDs);
		info.AddValue("HandAbilityCardIDsAndInstanceIDs", HandAbilityCardIDsAndInstanceIDs);
		info.AddValue("RoundAbilityCardIDsAndInstanceID", RoundAbilityCardIDsAndInstanceID);
		info.AddValue("ActivatedAbilityCardIDs", ActivatedAbilityCardIDs);
		info.AddValue("DiscardedAbilityCardIDs", DiscardedAbilityCardIDs);
		info.AddValue("LostAbilityCardIDs", LostAbilityCardIDs);
		info.AddValue("PermaLostAbilityCardIDs", PermaLostAbilityCardIDs);
		info.AddValue("InitiativeAbilityCardID", InitiativeAbilityCardID);
		info.AddValue("ActiveBonuses", ActiveBonuses);
		info.AddValue("Enhancements", Enhancements);
		info.AddValue("InitialState", InitialState);
	}

	public AbilityDeckState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "SelectedAbilityCardIDsAndInstanceIDs":
					SelectedAbilityCardIDsAndInstanceIDs = (List<Tuple<int, int>>)info.GetValue("SelectedAbilityCardIDsAndInstanceIDs", typeof(List<Tuple<int, int>>));
					break;
				case "HandAbilityCardIDsAndInstanceIDs":
					HandAbilityCardIDsAndInstanceIDs = (List<Tuple<int, int>>)info.GetValue("HandAbilityCardIDsAndInstanceIDs", typeof(List<Tuple<int, int>>));
					break;
				case "RoundAbilityCardIDsAndInstanceID":
					RoundAbilityCardIDsAndInstanceID = (List<Tuple<int, int>>)info.GetValue("RoundAbilityCardIDsAndInstanceID", typeof(List<Tuple<int, int>>));
					break;
				case "DiscardedAbilityCardIDs":
					DiscardedAbilityCardIDs = (List<int>)info.GetValue("DiscardedAbilityCardIDs", typeof(List<int>));
					break;
				case "LostAbilityCardIDs":
					LostAbilityCardIDs = (List<int>)info.GetValue("LostAbilityCardIDs", typeof(List<int>));
					break;
				case "PermaLostAbilityCardIDs":
					PermaLostAbilityCardIDs = (List<int>)info.GetValue("PermaLostAbilityCardIDs", typeof(List<int>));
					break;
				case "ActivatedAbilityCardIDs":
					ActivatedAbilityCardIDs = (List<Tuple<int, CBaseCard.ECardType>>)info.GetValue("ActivatedAbilityCardIDs", typeof(List<Tuple<int, CBaseCard.ECardType>>));
					break;
				case "InitiativeAbilityCardID":
					InitiativeAbilityCardID = (int?)info.GetValue("InitiativeAbilityCardID", typeof(int?));
					break;
				case "ActiveBonuses":
					ActiveBonuses = (List<ActiveBonusState>)info.GetValue("ActiveBonuses", typeof(List<ActiveBonusState>));
					break;
				case "Enhancements":
					Enhancements = (List<CEnhancement>)info.GetValue("Enhancements", typeof(List<CEnhancement>));
					break;
				case "InitialState":
					InitialState = info.GetBoolean("InitialState");
					break;
				case "SelectedAbilityCardIDs":
				{
					List<int> list3 = (List<int>)info.GetValue("SelectedAbilityCardIDs", typeof(List<int>));
					if (list3 == null)
					{
						break;
					}
					SelectedAbilityCardIDsAndInstanceIDs = new List<Tuple<int, int>>();
					foreach (int oldSelectedAbilityCardId in list3)
					{
						SelectedAbilityCardIDsAndInstanceIDs.Add(new Tuple<int, int>(oldSelectedAbilityCardId, oldSelectedAbilityCardId + SelectedAbilityCardIDsAndInstanceIDs.Count((Tuple<int, int> x) => x.Item1 == oldSelectedAbilityCardId) * 10000));
					}
					break;
				}
				case "HandAbilityCardIDs":
				{
					List<int> list2 = (List<int>)info.GetValue("HandAbilityCardIDs", typeof(List<int>));
					if (list2 == null)
					{
						break;
					}
					HandAbilityCardIDsAndInstanceIDs = new List<Tuple<int, int>>();
					foreach (int oldHandAbilityCardId in list2)
					{
						HandAbilityCardIDsAndInstanceIDs.Add(new Tuple<int, int>(oldHandAbilityCardId, oldHandAbilityCardId + HandAbilityCardIDsAndInstanceIDs.Count((Tuple<int, int> x) => x.Item1 == oldHandAbilityCardId) * 10000));
					}
					break;
				}
				case "RoundAbilityCardIDs":
				{
					List<int> list = (List<int>)info.GetValue("RoundAbilityCardIDs", typeof(List<int>));
					if (list == null)
					{
						break;
					}
					RoundAbilityCardIDsAndInstanceID = new List<Tuple<int, int>>();
					foreach (int oldRoundAbilityCardId in list)
					{
						RoundAbilityCardIDsAndInstanceID.Add(new Tuple<int, int>(oldRoundAbilityCardId, oldRoundAbilityCardId + RoundAbilityCardIDsAndInstanceID.Count((Tuple<int, int> x) => x.Item1 == oldRoundAbilityCardId) * 10000));
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize AbilityDeckState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (Enhancements == null)
		{
			Enhancements = new List<CEnhancement>();
		}
		if (ActiveBonuses == null)
		{
			ActiveBonuses = new List<ActiveBonusState>();
		}
	}

	public AbilityDeckState(List<Tuple<int, int>> selectedCardIDs, List<Tuple<int, int>> handIDs, List<Tuple<int, int>> roundIDs, List<Tuple<int, CBaseCard.ECardType>> activatedIDs, List<int> discardedIDs, List<int> lostIDs, List<int> permaLostIDs, int initCardID, List<CEnhancement> enhancements)
	{
		SelectedAbilityCardIDsAndInstanceIDs = selectedCardIDs;
		HandAbilityCardIDsAndInstanceIDs = handIDs;
		RoundAbilityCardIDsAndInstanceID = roundIDs;
		ActivatedAbilityCardIDs = activatedIDs;
		DiscardedAbilityCardIDs = discardedIDs;
		LostAbilityCardIDs = lostIDs;
		PermaLostAbilityCardIDs = permaLostIDs;
		InitiativeAbilityCardID = initCardID;
		Enhancements = enhancements;
		ActiveBonuses = new List<ActiveBonusState>();
	}

	public AbilityDeckState(List<Tuple<int, int>> selectedCardIDs)
	{
		SelectedAbilityCardIDsAndInstanceIDs = selectedCardIDs;
		Enhancements = new List<CEnhancement>();
		ActiveBonuses = new List<ActiveBonusState>();
	}

	public AbilityDeckState(CClass actorClass)
	{
		Save(actorClass);
	}

	public void Save(CClass actorClass)
	{
		if (actorClass is CCharacterClass cCharacterClass)
		{
			InitialState = cCharacterClass.InitialState;
			SelectedAbilityCardIDsAndInstanceIDs = (from c in cCharacterClass.SelectedAbilityCards.Select((CAbilityCard c) => new { c.ID, c.CardInstanceID }).AsEnumerable()
				select new Tuple<int, int>(c.ID, c.CardInstanceID)).ToList();
			HandAbilityCardIDsAndInstanceIDs = (from c in cCharacterClass.HandAbilityCards.Select((CAbilityCard c) => new { c.ID, c.CardInstanceID }).AsEnumerable()
				select new Tuple<int, int>(c.ID, c.CardInstanceID)).ToList();
			RoundAbilityCardIDsAndInstanceID = (from c in cCharacterClass.RoundAbilityCards.Select((CAbilityCard c) => new { c.ID, c.CardInstanceID }).AsEnumerable()
				select new Tuple<int, int>(c.ID, c.CardInstanceID)).ToList();
			ActivatedAbilityCardIDs = new List<Tuple<int, CBaseCard.ECardType>>();
			foreach (CBaseCard activatedCard in cCharacterClass.ActivatedCards)
			{
				ActivatedAbilityCardIDs.Add(new Tuple<int, CBaseCard.ECardType>(activatedCard.ID, activatedCard.CardType));
			}
			DiscardedAbilityCardIDs = cCharacterClass.DiscardedAbilityCards.Select((CAbilityCard c) => c.ID).ToList();
			LostAbilityCardIDs = cCharacterClass.LostAbilityCards.Select((CAbilityCard c) => c.ID).ToList();
			PermaLostAbilityCardIDs = cCharacterClass.PermanentlyLostAbilityCards.Select((CAbilityCard c) => c.ID).ToList();
			InitiativeAbilityCardID = cCharacterClass.InitiativeAbilityCard?.ID;
			Enhancements = cCharacterClass.AbilityCardsPool.SelectMany((CAbilityCard card) => card.GetAllAbilities().SelectMany((CAbility ability) => ability.AbilityEnhancements)).ToList();
			ActiveBonuses = new List<ActiveBonusState>();
			{
				foreach (CBaseCard activatedCard2 in cCharacterClass.ActivatedCards)
				{
					foreach (CActiveBonus activeBonuse in activatedCard2.ActiveBonuses)
					{
						if (activatedCard2 is CAbilityCard cAbilityCard)
						{
							ActiveBonuses.Add(new ActiveBonusState(activeBonuse.ID, cAbilityCard.ID, cAbilityCard.Name, activeBonuse.Ability.Name, activeBonuse.Actor.ActorGuid, activeBonuse.Caster.ActorGuid, cAbilityCard.SelectedAction.ID == cAbilityCard.TopAction.ID, activeBonuse.Remaining, activeBonuse.ActiveBonusStartRound, activeBonuse.IsDoom, activeBonuse.BespokeBehaviourStrength()));
						}
						else
						{
							ActiveBonuses.Add(new ActiveBonusState(activeBonuse.ID, activatedCard2.ID, activatedCard2.Name, activeBonuse.Ability.Name, activeBonuse.Actor.ActorGuid, activeBonuse.Caster.ActorGuid, isTopAction: false, activeBonuse.Remaining, activeBonuse.ActiveBonusStartRound, activeBonuse.IsDoom, activeBonuse.BespokeBehaviourStrength()));
						}
					}
				}
				return;
			}
		}
		if (!(actorClass is CMonsterClass cMonsterClass))
		{
			return;
		}
		InitialState = cMonsterClass.InitialState;
		HandAbilityCardIDsAndInstanceIDs = cMonsterClass.AbilityCards.Select((CMonsterAbilityCard c) => new Tuple<int, int>(c.ID, c.ID)).ToList();
		RoundAbilityCardIDsAndInstanceID = ((cMonsterClass.RoundAbilityCard != null) ? new List<Tuple<int, int>>
		{
			new Tuple<int, int>(cMonsterClass.RoundAbilityCard.ID, cMonsterClass.RoundAbilityCard.ID)
		} : null);
		DiscardedAbilityCardIDs = cMonsterClass.DiscardedAbilityCards.Select((CMonsterAbilityCard s) => s.ID).ToList();
		ActivatedAbilityCardIDs = new List<Tuple<int, CBaseCard.ECardType>>();
		foreach (CBaseCard activatedCard3 in cMonsterClass.ActivatedCards)
		{
			ActivatedAbilityCardIDs.Add(new Tuple<int, CBaseCard.ECardType>(activatedCard3.ID, activatedCard3.CardType));
		}
		if (cMonsterClass.RoundAbilityCard != null)
		{
			ActiveBonuses = new List<ActiveBonusState>();
			foreach (CActiveBonus activeBonuse2 in cMonsterClass.RoundAbilityCard.ActiveBonuses)
			{
				ActiveBonuses.Add(new ActiveBonusState(activeBonuse2.ID, cMonsterClass.RoundAbilityCard.ID, cMonsterClass.RoundAbilityCard.Name, activeBonuse2.Ability.Name, activeBonuse2.Actor.ActorGuid, activeBonuse2.Caster.ActorGuid, isTopAction: false, activeBonuse2.Remaining, activeBonuse2.ActiveBonusStartRound, activeBonuse2.IsDoom, activeBonuse2.BespokeBehaviourStrength()));
			}
		}
		if (cMonsterClass.ActivatedCards == null || cMonsterClass.ActivatedCards.Count <= 0)
		{
			return;
		}
		foreach (CBaseCard activatedCard4 in cMonsterClass.ActivatedCards)
		{
			foreach (CActiveBonus activeBonuse3 in activatedCard4.ActiveBonuses)
			{
				ActiveBonuses.Add(new ActiveBonusState(activeBonuse3.ID, activatedCard4.ID, activatedCard4.Name, activeBonuse3.Ability.Name, activeBonuse3.Actor.ActorGuid, activeBonuse3.Caster.ActorGuid, isTopAction: false, activeBonuse3.Remaining, activeBonuse3.ActiveBonusStartRound, activeBonuse3.IsDoom, activeBonuse3.BespokeBehaviourStrength()));
			}
		}
	}

	public void Load(CClass actorClass)
	{
		if (actorClass is CCharacterClass cCharacterClass)
		{
			cCharacterClass.LoadAbilityDeck(this);
		}
	}

	public static List<Tuple<int, string>> Compare(AbilityDeckState deck1, AbilityDeckState deck2, string actorGuid, string classID, bool isMPCompare, string id)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		StateShared.ENullStatus eNullStatus = StateShared.ENullStatus.None;
		try
		{
			switch (StateShared.CheckNullsMatch(deck1.SelectedAbilityCardIDsAndInstanceIDs, deck2.SelectedAbilityCardIDsAndInstanceIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1001, "AbilityDeckState SelectedAbilityCardIDs Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"SelectedAbilityCardIDs",
						(deck1.SelectedAbilityCardIDsAndInstanceIDs == null) ? "is null" : "is not null",
						(deck2.SelectedAbilityCardIDsAndInstanceIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.SelectedAbilityCardIDsAndInstanceIDs.Count != deck2.SelectedAbilityCardIDsAndInstanceIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1002, "AbilityDeckState SelectedAbilityCardIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"SelectedAbilityCardIDs Count",
							deck1.SelectedAbilityCardIDsAndInstanceIDs.Count.ToString(),
							deck2.SelectedAbilityCardIDsAndInstanceIDs.Count.ToString()
						}
					});
					break;
				}
				foreach (Tuple<int, int> cardTuple in deck1.SelectedAbilityCardIDsAndInstanceIDs)
				{
					if (deck2.SelectedAbilityCardIDsAndInstanceIDs.SingleOrDefault((Tuple<int, int> c) => c.Item1 == cardTuple.Item1 && c.Item2 == cardTuple.Item2) == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1003, "AbilityDeckState SelectedAbilityCardIDsAndInstanceIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"SelectedAbilityCardID",
								cardTuple.Item1.ToString(),
								"Missing"
							},
							new string[3]
							{
								"SelectedAbilityCardInstanceID",
								cardTuple.Item2.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (Tuple<int, int> cardTuple2 in deck2.SelectedAbilityCardIDsAndInstanceIDs)
				{
					if (deck1.SelectedAbilityCardIDsAndInstanceIDs.SingleOrDefault((Tuple<int, int> c) => c.Item1 == cardTuple2.Item1 && c.Item2 == cardTuple2.Item2) == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1003, "AbilityDeckState SelectedAbilityCardIDsAndInstanceIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"SelectedAbilityCardID",
								"Missing",
								cardTuple2.Item1.ToString()
							},
							new string[3]
							{
								"SelectedAbilityCardInstanceID",
								"Missing",
								cardTuple2.Item2.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(deck1.HandAbilityCardIDsAndInstanceIDs, deck2.HandAbilityCardIDsAndInstanceIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1004, "AbilityDeckState HandAbilityCardIDs Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"HandAbilityCardIDs",
						(deck1.HandAbilityCardIDsAndInstanceIDs == null) ? "is null" : "is not null",
						(deck2.HandAbilityCardIDsAndInstanceIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.HandAbilityCardIDsAndInstanceIDs.Count != deck2.HandAbilityCardIDsAndInstanceIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1005, "AbilityDeckState HandAbilityCardIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"HandAbilityCardIDs Count",
							deck1.HandAbilityCardIDsAndInstanceIDs.Count.ToString(),
							deck2.HandAbilityCardIDsAndInstanceIDs.Count.ToString()
						}
					});
					break;
				}
				foreach (Tuple<int, int> cardTuple3 in deck1.HandAbilityCardIDsAndInstanceIDs)
				{
					if (deck2.HandAbilityCardIDsAndInstanceIDs.SingleOrDefault((Tuple<int, int> c) => c.Item1 == cardTuple3.Item1 && c.Item2 == cardTuple3.Item2) == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1003, "AbilityDeckState HandAbilityCardIDsAndInstanceIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"HandAbilityCardID",
								cardTuple3.Item1.ToString(),
								"Missing"
							},
							new string[3]
							{
								"HandAbilityCardInstanceID",
								cardTuple3.Item2.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (Tuple<int, int> cardTuple4 in deck2.HandAbilityCardIDsAndInstanceIDs)
				{
					if (deck1.HandAbilityCardIDsAndInstanceIDs.SingleOrDefault((Tuple<int, int> c) => c.Item1 == cardTuple4.Item1 && c.Item2 == cardTuple4.Item2) == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1003, "AbilityDeckState HandAbilityCardIDsAndInstanceIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"HandAbilityCardID",
								"Missing",
								cardTuple4.Item1.ToString()
							},
							new string[3]
							{
								"HandAbilityCardInstanceID",
								"Missing",
								cardTuple4.Item2.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(deck1.RoundAbilityCardIDsAndInstanceID, deck2.RoundAbilityCardIDsAndInstanceID))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1007, "AbilityDeckState RoundAbilityCardIDs Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"RoundAbilityCardIDs",
						(deck1.RoundAbilityCardIDsAndInstanceID == null) ? "is null" : "is not null",
						(deck2.RoundAbilityCardIDsAndInstanceID == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.RoundAbilityCardIDsAndInstanceID.Count != deck2.RoundAbilityCardIDsAndInstanceID.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1008, "AbilityDeckState RoundAbilityCardIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"RoundAbilityCardIDs Count",
							deck1.RoundAbilityCardIDsAndInstanceID.Count.ToString(),
							deck2.RoundAbilityCardIDsAndInstanceID.Count.ToString()
						}
					});
					break;
				}
				foreach (Tuple<int, int> cardTuple5 in deck1.RoundAbilityCardIDsAndInstanceID)
				{
					if (deck2.RoundAbilityCardIDsAndInstanceID.SingleOrDefault((Tuple<int, int> c) => c.Item1 == cardTuple5.Item1 && c.Item2 == cardTuple5.Item2) == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1003, "AbilityDeckState RoundAbilityCardIDsAndInstanceID in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"RoundAbilityCardID",
								cardTuple5.Item1.ToString(),
								"Missing"
							},
							new string[3]
							{
								"RoundAbilityCardInstanceID",
								cardTuple5.Item2.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (Tuple<int, int> cardTuple6 in deck2.RoundAbilityCardIDsAndInstanceID)
				{
					if (deck1.RoundAbilityCardIDsAndInstanceID.SingleOrDefault((Tuple<int, int> c) => c.Item1 == cardTuple6.Item1 && c.Item2 == cardTuple6.Item2) == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1003, "AbilityDeckState RoundAbilityCardIDsAndInstanceID in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"RoundAbilityCardID",
								"Missing",
								cardTuple6.Item1.ToString()
							},
							new string[3]
							{
								"RoundAbilityCardInstanceID",
								"Missing",
								cardTuple6.Item2.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(deck1.ActivatedAbilityCardIDs, deck2.ActivatedAbilityCardIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1010, "AbilityDeckState ActivatedAbilityCardIDs Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"ActivatedAbilityCardIDs",
						(deck1.ActivatedAbilityCardIDs == null) ? "is null" : "is not null",
						(deck2.ActivatedAbilityCardIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.ActivatedAbilityCardIDs.Count != deck2.ActivatedAbilityCardIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1011, "AbilityDeckState ActivatedAbilityCardIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"ActivatedAbilityCardIDs Count",
							deck1.ActivatedAbilityCardIDs.Count.ToString(),
							deck2.ActivatedAbilityCardIDs.Count.ToString()
						}
					});
					break;
				}
				foreach (Tuple<int, CBaseCard.ECardType> card in deck1.ActivatedAbilityCardIDs)
				{
					if (!deck2.ActivatedAbilityCardIDs.Exists((Tuple<int, CBaseCard.ECardType> e) => e.Item1 == card.Item1 && e.Item2 == card.Item2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1012, "AbilityDeckState ActivatedAbilityCardIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"ActivatedAbilityCardID",
								card.Item1.ToString(),
								"Missing"
							},
							new string[3]
							{
								"Card Type",
								card.Item2.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (Tuple<int, CBaseCard.ECardType> card2 in deck2.ActivatedAbilityCardIDs)
				{
					if (!deck1.ActivatedAbilityCardIDs.Exists((Tuple<int, CBaseCard.ECardType> e) => e.Item1 == card2.Item1 && e.Item2 == card2.Item2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1012, "AbilityDeckState ActivatedAbilityCardIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"ActivatedAbilityCardID",
								"Missing",
								card2.Item1.ToString()
							},
							new string[3]
							{
								"Card Type",
								"Missing",
								card2.Item2.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(deck1.DiscardedAbilityCardIDs, deck2.DiscardedAbilityCardIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1013, "AbilityDeckState DiscardedAbilityCardIDs Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"DiscardedAbilityCardIDs",
						(deck1.DiscardedAbilityCardIDs == null) ? "is null" : "is not null",
						(deck2.DiscardedAbilityCardIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.DiscardedAbilityCardIDs.Count != deck2.DiscardedAbilityCardIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1014, "AbilityDeckState DiscardedAbilityCardIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"DiscardedAbilityCardIDs Count",
							deck1.DiscardedAbilityCardIDs.Count.ToString(),
							deck2.DiscardedAbilityCardIDs.Count.ToString()
						}
					});
					break;
				}
				foreach (int discardedAbilityCardID in deck1.DiscardedAbilityCardIDs)
				{
					if (!deck2.DiscardedAbilityCardIDs.Contains(discardedAbilityCardID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1015, "AbilityDeckState DiscardedAbilityCardIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"DiscardedAbilityCardID",
								discardedAbilityCardID.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (int discardedAbilityCardID2 in deck2.DiscardedAbilityCardIDs)
				{
					if (!deck1.DiscardedAbilityCardIDs.Contains(discardedAbilityCardID2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1015, "AbilityDeckState DiscardedAbilityCardIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"DiscardedAbilityCardID",
								"Missing",
								discardedAbilityCardID2.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(deck1.LostAbilityCardIDs, deck2.LostAbilityCardIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1016, "AbilityDeckState LostAbilityCardIDs Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"LostAbilityCardIDs",
						(deck1.LostAbilityCardIDs == null) ? "is null" : "is not null",
						(deck2.LostAbilityCardIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.LostAbilityCardIDs.Count != deck2.LostAbilityCardIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1017, "AbilityDeckState LostAbilityCardIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"LostAbilityCardIDs Count",
							deck1.LostAbilityCardIDs.Count.ToString(),
							deck2.LostAbilityCardIDs.Count.ToString()
						}
					});
					break;
				}
				foreach (int lostAbilityCardID in deck1.LostAbilityCardIDs)
				{
					if (!deck2.LostAbilityCardIDs.Contains(lostAbilityCardID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1018, "AbilityDeckState LostAbilityCardIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"LostAbilityCardID",
								lostAbilityCardID.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (int lostAbilityCardID2 in deck2.LostAbilityCardIDs)
				{
					if (!deck1.LostAbilityCardIDs.Contains(lostAbilityCardID2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1018, "AbilityDeckState LostAbilityCardIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"LostAbilityCardID",
								"Missing",
								lostAbilityCardID2.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(deck1.PermaLostAbilityCardIDs, deck2.PermaLostAbilityCardIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1019, "AbilityDeckState PermaLostAbilityCardIDs Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"PermaLostAbilityCardIDs",
						(deck1.PermaLostAbilityCardIDs == null) ? "is null" : "is not null",
						(deck2.PermaLostAbilityCardIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.PermaLostAbilityCardIDs.Count != deck2.PermaLostAbilityCardIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1020, "AbilityDeckState PermaLostAbilityCardIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"PermaLostAbilityCardIDs Count",
							deck1.PermaLostAbilityCardIDs.Count.ToString(),
							deck2.PermaLostAbilityCardIDs.Count.ToString()
						}
					});
					break;
				}
				foreach (int permaLostAbilityCardID in deck1.PermaLostAbilityCardIDs)
				{
					if (!deck2.PermaLostAbilityCardIDs.Contains(permaLostAbilityCardID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1021, "AbilityDeckState PermaLostAbilityCardIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"PermaLostAbilityCardID",
								permaLostAbilityCardID.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (int permaLostAbilityCardID2 in deck2.PermaLostAbilityCardIDs)
				{
					if (!deck1.PermaLostAbilityCardIDs.Contains(permaLostAbilityCardID2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1021, "AbilityDeckState PermaLostAbilityCardIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"PermaLostAbilityCardID",
								"Missing",
								permaLostAbilityCardID2.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(deck1.InitiativeAbilityCardID, deck2.InitiativeAbilityCardID))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1022, "AbilityDeckState InitiativeAbilityCardID Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"InitiativeAbilityCardID",
						(!deck1.InitiativeAbilityCardID.HasValue) ? "is null" : "is not null",
						(!deck2.InitiativeAbilityCardID.HasValue) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.InitiativeAbilityCardID.Value != deck2.InitiativeAbilityCardID.Value)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1023, "AbilityDeckState InitiativeAbilityCardID does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"InitiativeAbilityCardID",
							deck1.InitiativeAbilityCardID.Value.ToString(),
							deck2.InitiativeAbilityCardID.Value.ToString()
						}
					});
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(deck1.ActiveBonuses, deck2.ActiveBonuses))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1024, "AbilityDeckState ActiveBonuses Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"ActiveBonuses",
						(deck1.ActiveBonuses == null) ? "is null" : "is not null",
						(deck2.ActiveBonuses == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (deck1.ActiveBonuses.Count != deck2.ActiveBonuses.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1025, "AbilityDeckState ActiveBonuses Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"ActiveBonuses Count",
							deck1.ActiveBonuses.Count.ToString(),
							deck2.ActiveBonuses.Count.ToString()
						}
					});
					break;
				}
				List<ActiveBonusState> list2 = deck1.ActiveBonuses.ToList();
				List<ActiveBonusState> list3 = deck2.ActiveBonuses.ToList();
				for (int num = list2.Count - 1; num >= 0; num--)
				{
					ActiveBonusState abState1 = list2[num];
					ActiveBonusState activeBonusState = list3.LastOrDefault((ActiveBonusState s) => s.CardID == abState1.CardID && abState1.CardName.Contains(s.CardName) && s.AbilityName == abState1.AbilityName && abState1.ActorGuid == s.ActorGuid);
					if (activeBonusState == null)
					{
						activeBonusState = list3.Find((ActiveBonusState s) => s.CardID == abState1.CardID && abState1.CardName.Contains(s.CardName.Replace(" ", string.Empty).Replace("'", string.Empty)) && s.AbilityName == abState1.AbilityName && abState1.ActorGuid == s.ActorGuid);
					}
					if (activeBonusState == null)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1026, "AbilityDeckState ActiveBonus in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
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
						list.AddRange(ActiveBonusState.Compare(abState1, activeBonusState, actorGuid, classID, isMPCompare));
						list3.Remove(activeBonusState);
					}
					list2.Remove(abState1);
					list3.Remove(activeBonusState);
				}
				if (list3.Count <= 0)
				{
					break;
				}
				foreach (ActiveBonusState item in list3)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1026, "AbilityDeckState ActiveBonus in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
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
			switch (StateShared.CheckNullsMatch(deck1.Enhancements, deck2.Enhancements))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1027, "AbilityDeckState Enhancements Null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"Enhancements",
						(deck1.Enhancements == null) ? "is null" : "is not null",
						(deck2.Enhancements == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (deck1.Enhancements.Count != deck2.Enhancements.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1028, "AbilityDeckState Enhancements Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"Enhancements Count",
							deck1.Enhancements.Count.ToString(),
							deck2.Enhancements.Count.ToString()
						}
					});
					break;
				}
				foreach (CEnhancement enhancement1 in deck1.Enhancements)
				{
					if (!deck2.Enhancements.Exists((CEnhancement e) => enhancement1.Compare(e)))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1029, "AbilityDeckState Enhancements in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing and enhancement from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"Enhancement",
								enhancement1.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (CEnhancement enhancement2 in deck2.Enhancements)
				{
					if (!deck1.Enhancements.Exists((CEnhancement e) => enhancement2.Compare(e)))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1029, "AbilityDeckState Enhancements in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing and enhancement from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3]
							{
								"Enhancement",
								"Missing",
								enhancement2.ToString()
							}
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(1099, "Exception during AbilityDeck State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}

	private bool FindMonsterCardIDs(List<string> cards, out List<int> cardIDs)
	{
		cardIDs = new List<int>();
		foreach (string card in cards)
		{
			if (card.Length - 9 > 0 && int.TryParse(card.Substring(card.Length - 9), out var result))
			{
				cardIDs.Add(result);
				continue;
			}
			if (card.Length - 8 > 0 && int.TryParse(card.Substring(card.Length - 8), out var result2))
			{
				cardIDs.Add(result2);
				continue;
			}
			if (card.Length - 7 > 0 && int.TryParse(card.Substring(card.Length - 7), out var result3))
			{
				cardIDs.Add(result3);
				continue;
			}
			if (card.Length - 6 > 0 && int.TryParse(card.Substring(card.Length - 6), out var result4))
			{
				cardIDs.Add(result4);
				continue;
			}
			if (card.Length - 5 > 0 && int.TryParse(card.Substring(card.Length - 5), out var result5))
			{
				cardIDs.Add(result5);
				continue;
			}
			if (card.Length - 4 > 0 && int.TryParse(card.Substring(card.Length - 4), out var result6))
			{
				cardIDs.Add(result6);
				continue;
			}
			if (card.Length - 3 > 0 && int.TryParse(card.Substring(card.Length - 3), out var result7))
			{
				cardIDs.Add(result7);
				continue;
			}
			if (card.Length - 2 > 0 && int.TryParse(card.Substring(card.Length - 2), out var result8))
			{
				cardIDs.Add(result8);
				continue;
			}
			if (card.Length - 1 > 0 && int.TryParse(card.Substring(card.Length - 1), out var result9))
			{
				cardIDs.Add(result9);
				continue;
			}
			return false;
		}
		return true;
	}
}
