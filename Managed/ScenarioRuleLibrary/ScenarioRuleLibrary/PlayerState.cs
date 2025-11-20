using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{ClassID}")]
public class PlayerState : ActorState, ISerializable
{
	public string CharacterName { get; private set; }

	public int ScenarioGold { get; private set; }

	public int ScenarioXP { get; private set; }

	public bool IsLongResting { get; private set; }

	public AbilityDeckState AbilityDeck { get; private set; }

	public AttackModifierDeckState AttackModifierDeck { get; private set; }

	public List<CItem> Items { get; private set; }

	public bool HiddenAtStart { get; set; }

	public bool ImprovedShortRestState { get; set; }

	public string CompanionSummonGuid { get; set; }

	public CPlayerActor Player
	{
		get
		{
			try
			{
				return ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == base.ActorGuid);
			}
			catch (Exception)
			{
				DLLDebug.LogError("More than one matching player actor in Scenario.AllPlayers with ActorGuid: " + base.ActorGuid);
				return null;
			}
		}
	}

	public PlayerState()
	{
	}

	public PlayerState(PlayerState state, ReferenceDictionary references)
		: base(state, references)
	{
		CharacterName = state.CharacterName;
		ScenarioGold = state.ScenarioGold;
		ScenarioXP = state.ScenarioXP;
		IsLongResting = state.IsLongResting;
		AbilityDeck = references.Get(state.AbilityDeck);
		if (AbilityDeck == null && state.AbilityDeck != null)
		{
			AbilityDeck = new AbilityDeckState(state.AbilityDeck, references);
			references.Add(state.AbilityDeck, AbilityDeck);
		}
		AttackModifierDeck = references.Get(state.AttackModifierDeck);
		if (AttackModifierDeck == null && state.AttackModifierDeck != null)
		{
			AttackModifierDeck = new AttackModifierDeckState(state.AttackModifierDeck, references);
			references.Add(state.AttackModifierDeck, AttackModifierDeck);
		}
		Items = references.Get(state.Items);
		if (Items == null && state.Items != null)
		{
			Items = new List<CItem>();
			for (int i = 0; i < state.Items.Count; i++)
			{
				CItem cItem = state.Items[i];
				CItem cItem2 = references.Get(cItem);
				if (cItem2 == null && cItem != null)
				{
					cItem2 = new CItem(cItem, references);
					references.Add(cItem, cItem2);
				}
				Items.Add(cItem2);
			}
			references.Add(state.Items, Items);
		}
		HiddenAtStart = state.HiddenAtStart;
		ImprovedShortRestState = state.ImprovedShortRestState;
		CompanionSummonGuid = state.CompanionSummonGuid;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("CharacterName", CharacterName);
		info.AddValue("ScenarioGold", ScenarioGold);
		info.AddValue("ScenarioXP", ScenarioXP);
		info.AddValue("IsLongResting", IsLongResting);
		info.AddValue("AbilityDeck", AbilityDeck);
		info.AddValue("AttackModifierDeck", AttackModifierDeck);
		info.AddValue("Items", Items);
		info.AddValue("HiddenAtStart", HiddenAtStart);
		info.AddValue("ImprovedShortRestState", ImprovedShortRestState);
		info.AddValue("CompanionSummonGuid", CompanionSummonGuid);
	}

	public PlayerState(SerializationInfo info, StreamingContext context)
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
				case "CharacterName":
					CharacterName = info.GetString("CharacterName");
					break;
				case "ScenarioGold":
					ScenarioGold = info.GetInt32("ScenarioGold");
					break;
				case "ScenarioXP":
					ScenarioXP = info.GetInt32("ScenarioXP");
					break;
				case "IsLongResting":
					IsLongResting = info.GetBoolean("IsLongResting");
					break;
				case "AbilityDeck":
					AbilityDeck = (AbilityDeckState)info.GetValue("AbilityDeck", typeof(AbilityDeckState));
					break;
				case "AttackModifierDeck":
					AttackModifierDeck = (AttackModifierDeckState)info.GetValue("AttackModifierDeck", typeof(AttackModifierDeckState));
					break;
				case "Items":
					Items = (List<CItem>)info.GetValue("Items", typeof(List<CItem>));
					break;
				case "HiddenAtStart":
					HiddenAtStart = info.GetBoolean("HiddenAtStart");
					break;
				case "ImprovedShortRestState":
					ImprovedShortRestState = info.GetBoolean("ImprovedShortRestState");
					break;
				case "CompanionSummonGuid":
					CompanionSummonGuid = info.GetString("CompanionSummonGuid");
					break;
				}
				if (CompanionSummonGuid == null)
				{
					CompanionSummonGuid = string.Empty;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize PlayerState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public PlayerState(string classID, int chosenModelIndex, string actorGuid, string mapGuid, TileIndex location, int health, int maxHealth, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, bool playedThisRound, CActor.ECauseOfDeath causeOfDeath, int augmentSlots, string characterName, int gold, int xp, bool isLongResting, AbilityDeckState deck, AttackModifierDeckState attackModDeck, List<CItem> items)
		: base(classID, chosenModelIndex, actorGuid, mapGuid, location, health, maxHealth, level, posCons, negCons, playedThisRound, causeOfDeath, augmentSlots)
	{
		CharacterName = characterName;
		ScenarioGold = gold;
		ScenarioXP = xp;
		IsLongResting = isLongResting;
		AbilityDeck = deck;
		AttackModifierDeck = attackModDeck;
		Items = items;
		CompanionSummonGuid = string.Empty;
	}

	public PlayerState(CPlayerActor playerActor, string mapGuid)
		: base(playerActor.Class.ID, playerActor.ChosenModelIndex, playerActor.ActorGuid, mapGuid, new TileIndex(playerActor.ArrayIndex), playerActor.Health, playerActor.MaxHealth, playerActor.Level, playerActor.Tokens.CheckPositiveTokens.ToList(), playerActor.Tokens.CheckNegativeTokens.ToList(), playerActor.PlayedThisRound, playerActor.CauseOfDeath, playerActor.AugmentSlots)
	{
		ScenarioGold = playerActor.Gold;
		ScenarioXP = playerActor.XP;
		IsLongResting = IsLongResting;
		Items = playerActor.Inventory.AllItems;
		base.IsRevealed = true;
		HiddenAtStart = false;
		ImprovedShortRestState = false;
		CompanionSummonGuid = string.Empty;
	}

	public void InitCharacter(int health, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, AbilityDeckState deck, List<CItem> items)
	{
		base.Health = health;
		base.MaxHealth = health;
		base.Level = level;
		base.PositiveConditions = posCons;
		base.NegativeConditions = negCons;
		ScenarioGold = 0;
		ScenarioXP = 0;
		AbilityDeck = deck;
		Items = items;
		CompanionSummonGuid = string.Empty;
		base.CharacterResources = new Dictionary<string, int>();
	}

	public void Save(bool initial, bool forceSave = false, CPlayerActor currentPlayer = null)
	{
		if (!(base.IsRevealed || forceSave))
		{
			return;
		}
		if (currentPlayer == null)
		{
			currentPlayer = ((initial && !base.IsDead) ? ScenarioManager.Scenario.InitialPlayers.Single((CPlayerActor s) => s.ActorGuid == base.ActorGuid) : Player);
		}
		AbilityDeck.Save(currentPlayer.Class);
		if (AttackModifierDeck == null)
		{
			AttackModifierDeck = new AttackModifierDeckState(currentPlayer.Class as CCharacterClass);
		}
		else
		{
			AttackModifierDeck.SaveCharacter(currentPlayer.Class as CCharacterClass);
		}
		CharacterName = currentPlayer.CharacterName;
		base.Location = new TileIndex(currentPlayer.ArrayIndex.X, currentPlayer.ArrayIndex.Y);
		base.Health = currentPlayer.Health;
		base.MaxHealth = currentPlayer.OriginalMaxHealth;
		base.Level = currentPlayer.Level;
		base.PositiveConditions = currentPlayer.Tokens.CheckPositiveTokens.ToList();
		base.NegativeConditions = currentPlayer.Tokens.CheckNegativeTokens.ToList();
		base.PlayedThisRound = currentPlayer.PlayedThisRound;
		base.PhasedOut = currentPlayer.PhasedOut;
		base.Deactivated = currentPlayer.Deactivated;
		base.CauseOfDeath = currentPlayer.CauseOfDeath;
		base.KilledByActorGuid = currentPlayer.KilledByActorGuid;
		ScenarioGold = currentPlayer.Gold;
		ScenarioXP = currentPlayer.XP;
		IsLongResting = (currentPlayer.Class as CCharacterClass).LongRest;
		ImprovedShortRestState = (currentPlayer.Class as CCharacterClass).ImprovedShortRest;
		base.AugmentSlots = currentPlayer.AugmentSlots;
		CompanionSummonGuid = ((currentPlayer.CompanionSummon != null) ? currentPlayer.CompanionSummon.ActorGuid : string.Empty);
		if (!initial)
		{
			Items = currentPlayer.Inventory.AllItems;
		}
		base.CarriedQuestProps = currentPlayer.CarriedQuestItems.ToList();
		base.CharacterResources.Clear();
		foreach (CCharacterResource characterResource in currentPlayer.CharacterResources)
		{
			base.CharacterResources.Add(characterResource.ID, characterResource.Amount);
		}
	}

	public void Load(bool forceLoad = false)
	{
		if (base.IsRevealed || forceLoad)
		{
			AttackModifierDeck.Load(Player.Class as CCharacterClass);
			Player.LoadPlayer(this);
		}
	}

	public void LoadAbiltyDeck(bool forceLoad = false)
	{
		if (base.IsRevealed || forceLoad)
		{
			AbilityDeck.Load(Player.Class);
		}
	}

	public static List<Tuple<int, string>> Compare(PlayerState state1, PlayerState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(ActorState.Compare(state1, state2, isMPCompare));
			if (state1.ScenarioGold != state2.ScenarioGold)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 501, "Player State ScenarioGold does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"ScenarioGold",
						state1.ScenarioGold.ToString(),
						state2.ScenarioGold.ToString()
					}
				});
			}
			if (state1.ScenarioXP != state2.ScenarioXP)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 502, "Player State ScenarioXP does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"ScenarioXP",
						state1.ScenarioXP.ToString(),
						state2.ScenarioXP.ToString()
					}
				});
			}
			if (state1.IsLongResting != state2.IsLongResting)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 503, "Player State IsLongResting does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"IsLongResting",
						state1.IsLongResting.ToString(),
						state2.IsLongResting.ToString()
					}
				});
			}
			if (state1.HiddenAtStart != state2.HiddenAtStart)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 504, "Player State HiddenAtStart does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"HiddenAtStart",
						state1.HiddenAtStart.ToString(),
						state2.HiddenAtStart.ToString()
					}
				});
			}
			if (state1.ImprovedShortRestState != state2.ImprovedShortRestState)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 505, "Player State ImprovedShortRestState does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"ImprovedShortRestState",
						state1.ImprovedShortRestState.ToString(),
						state2.ImprovedShortRestState.ToString()
					}
				});
			}
			if (state1.CompanionSummonGuid != state2.CompanionSummonGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 506, "Player State CompanionSummonGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3] { "CompanionSummonGuid", state1.CompanionSummonGuid, state2.CompanionSummonGuid }
				});
			}
			list.AddRange(AbilityDeckState.Compare(state1.AbilityDeck, state2.AbilityDeck, state1.ActorGuid, state1.ClassID, isMPCompare, state1.Actor?.ID.ToString()));
			list.AddRange(AttackModifierDeckState.Compare(state1.AttackModifierDeck, state2.AttackModifierDeck, state1.ActorGuid, state1.ClassID, state1.Actor?.ID.ToString(), isMPCompare));
			switch (StateShared.CheckNullsMatch(state1.Items, state2.Items))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 507, "Player State Items null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Actor ID",
						state1.Actor?.ID.ToString(),
						state1.Actor?.ID.ToString()
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Items",
						(state1.Items == null) ? "is null" : "is not null",
						(state2.Items == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.Items.Count != state2.Items.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 508, "Player State Items Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3]
						{
							"Actor ID",
							state1.Actor?.ID.ToString(),
							state1.Actor?.ID.ToString()
						},
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Items Count",
							state1.Items.Count.ToString(),
							state2.Items.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (CItem item in state1.Items)
				{
					if (!state2.Items.Exists((CItem e) => e.ItemGuid == item.ItemGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 509, "Player State Items in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing item from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3]
							{
								"Actor ID",
								state1.Actor?.ID.ToString(),
								state1.Actor?.ID.ToString()
							},
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3] { "Items GUID", item.ItemGuid, "Missing" },
							new string[3]
							{
								"Items Name",
								item.YMLData.Name,
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (CItem item2 in state2.Items)
				{
					if (!state1.Items.Exists((CItem e) => e.ItemGuid == item2.ItemGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 509, "Player State Items in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing item from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3]
							{
								"Actor ID",
								state1.Actor?.ID.ToString(),
								state1.Actor?.ID.ToString()
							},
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3] { "Items GUID", "Missing", item2.ItemGuid },
							new string[3]
							{
								"Items Name",
								"Missing",
								item2.YMLData.Name
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (CItem item3 in state1.Items)
				{
					try
					{
						CItem item4 = state2.Items.Single((CItem s) => s.ItemGuid == item3.ItemGuid);
						list.AddRange(CItem.Compare(item3, item4, "Player", state1.ActorGuid, state1.ClassID, isMPCompare));
					}
					catch (Exception ex)
					{
						list.Add(new Tuple<int, string>(510, "Exception during Item State compare.\n" + ex.Message + "\n" + ex.StackTrace));
					}
				}
				break;
			}
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(599, "Exception during Player State compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}
}
