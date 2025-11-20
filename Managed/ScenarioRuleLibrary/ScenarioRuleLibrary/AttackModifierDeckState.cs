using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class AttackModifierDeckState : ISerializable
{
	public List<string> AttackModifierCardsAvailable { get; private set; }

	public List<string> AttackModifierCardsDiscarded { get; private set; }

	public AttackModifierDeckState(AttackModifierDeckState state, ReferenceDictionary references)
	{
		AttackModifierCardsAvailable = references.Get(state.AttackModifierCardsAvailable);
		if (AttackModifierCardsAvailable == null && state.AttackModifierCardsAvailable != null)
		{
			AttackModifierCardsAvailable = new List<string>();
			for (int i = 0; i < state.AttackModifierCardsAvailable.Count; i++)
			{
				string item = state.AttackModifierCardsAvailable[i];
				AttackModifierCardsAvailable.Add(item);
			}
			references.Add(state.AttackModifierCardsAvailable, AttackModifierCardsAvailable);
		}
		AttackModifierCardsDiscarded = references.Get(state.AttackModifierCardsDiscarded);
		if (AttackModifierCardsDiscarded == null && state.AttackModifierCardsDiscarded != null)
		{
			AttackModifierCardsDiscarded = new List<string>();
			for (int j = 0; j < state.AttackModifierCardsDiscarded.Count; j++)
			{
				string item2 = state.AttackModifierCardsDiscarded[j];
				AttackModifierCardsDiscarded.Add(item2);
			}
			references.Add(state.AttackModifierCardsDiscarded, AttackModifierCardsDiscarded);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("AttackModifierCardsAvailable", AttackModifierCardsAvailable);
		info.AddValue("AttackModifierCardsDiscarded", AttackModifierCardsDiscarded);
	}

	public AttackModifierDeckState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "AttackModifierCardsAvailable"))
				{
					if (name == "AttackModifierCardsDiscarded")
					{
						AttackModifierCardsDiscarded = (List<string>)info.GetValue("AttackModifierCardsDiscarded", typeof(List<string>));
					}
				}
				else
				{
					AttackModifierCardsAvailable = (List<string>)info.GetValue("AttackModifierCardsAvailable", typeof(List<string>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize AttackModifierDeckState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public AttackModifierDeckState()
	{
	}

	public AttackModifierDeckState Copy()
	{
		return new AttackModifierDeckState
		{
			AttackModifierCardsAvailable = ((AttackModifierCardsAvailable != null) ? AttackModifierCardsAvailable.ToList() : new List<string>()),
			AttackModifierCardsDiscarded = ((AttackModifierCardsDiscarded != null) ? AttackModifierCardsDiscarded.ToList() : new List<string>())
		};
	}

	public AttackModifierDeckState(CCharacterClass playerClass)
	{
		AttackModifierCardsAvailable = playerClass.AttackModifierCardsPool.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsDiscarded = new List<string>();
	}

	public void SaveCharacter(CCharacterClass playerClass)
	{
		AttackModifierCardsAvailable = playerClass.AttackModifierCards.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsDiscarded = playerClass.DiscardedAttackModifierCards.Select((AttackModifierYMLData s) => s.Name).ToList();
	}

	public void SaveMonster(CMonsterAttackModifierDeck monsterAttackModifierDeck)
	{
		AttackModifierCardsAvailable = monsterAttackModifierDeck.AttackModifierCards.Select((AttackModifierYMLData s) => s.Name).ToList();
		AttackModifierCardsDiscarded = monsterAttackModifierDeck.DiscardedAttackModifierCards.Select((AttackModifierYMLData s) => s.Name).ToList();
	}

	public void Load(CCharacterClass playerClass)
	{
		playerClass.LoadAttackModifierDeck(this);
	}

	public static List<Tuple<int, string>> Compare(AttackModifierDeckState state1, AttackModifierDeckState state2, string actorGuid, string classID, string id, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			switch (StateShared.CheckNullsMatch(state1.AttackModifierCardsAvailable, state2.AttackModifierCardsAvailable))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1101, "AttackModifierDeck AttackModifierCardsAvailable null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"AttackModifierCardsAvailable",
						(state1.AttackModifierCardsAvailable == null) ? "is null" : "is not null",
						(state2.AttackModifierCardsAvailable == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.AttackModifierCardsAvailable.Count != state2.AttackModifierCardsAvailable.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1102, "AttackModifierDeck AttackModifierCardsAvailable Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"AttackModifierCardsAvailable Count",
							state1.AttackModifierCardsAvailable.Count.ToString(),
							state2.AttackModifierCardsAvailable.Count.ToString()
						}
					});
					break;
				}
				foreach (string item in state1.AttackModifierCardsAvailable)
				{
					if (!state2.AttackModifierCardsAvailable.Contains(item))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1103, "AttackModifierDeck AttackModifierCardsAvailable in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3] { "AttackModifierCardsAvailable", item, "Missing" }
						});
					}
				}
				foreach (string item2 in state2.AttackModifierCardsAvailable)
				{
					if (!state1.AttackModifierCardsAvailable.Contains(item2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1103, "AttackModifierDeck AttackModifierCardsAvailable in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3] { "AttackModifierCardsAvailable", "Missing", item2 }
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(state1.AttackModifierCardsDiscarded, state2.AttackModifierCardsDiscarded))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1104, "AttackModifierDeck AttackModifierCardsDiscarded null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", actorGuid, actorGuid },
					new string[3] { "Actor ID", id, id },
					new string[3] { "Class ID", classID, classID },
					new string[3]
					{
						"AttackModifierCardsDiscarded",
						(state1.AttackModifierCardsDiscarded == null) ? "is null" : "is not null",
						(state2.AttackModifierCardsDiscarded == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.AttackModifierCardsDiscarded.Count != state2.AttackModifierCardsDiscarded.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1105, "AttackModifierDeck AttackModifierCardsDiscarded Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", actorGuid, actorGuid },
						new string[3] { "Actor ID", id, id },
						new string[3] { "Class ID", classID, classID },
						new string[3]
						{
							"AttackModifierCardsDiscarded Count",
							state1.AttackModifierCardsDiscarded.Count.ToString(),
							state2.AttackModifierCardsDiscarded.Count.ToString()
						}
					});
					break;
				}
				foreach (string item3 in state1.AttackModifierCardsDiscarded)
				{
					if (!state2.AttackModifierCardsDiscarded.Contains(item3))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1106, "AttackModifierDeck AttackModifierCardsDiscarded in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3] { "AttackModifierCardsDiscarded", item3, "Missing" }
						});
					}
				}
				foreach (string item4 in state2.AttackModifierCardsDiscarded)
				{
					if (!state1.AttackModifierCardsDiscarded.Contains(item4))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1106, "AttackModifierDeck AttackModifierCardsDiscarded in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", actorGuid, actorGuid },
							new string[3] { "Actor ID", id, id },
							new string[3] { "Class ID", classID, classID },
							new string[3] { "AttackModifierCardsDiscarded", "Missing", item4 }
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(599, "Exception during AttackModifierDeck State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
