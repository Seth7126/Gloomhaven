using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using ScenarioRuleLibrary.YML;
using SharedLibrary;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectChest : CObjectProp, ISerializable
{
	public List<CItem> StartingItems { get; set; }

	public int Gold { get; set; }

	public int XP { get; set; }

	public int DamageValue { get; set; }

	public List<CCondition.ENegativeCondition> Conditions { get; private set; }

	public List<string> ChestTreasureTablesID { get; set; }

	public bool HasChestPrepopulated
	{
		get
		{
			if ((StartingItems == null || StartingItems.Count <= 0) && Gold == 0)
			{
				return XP != 0;
			}
			return true;
		}
	}

	public CObjectChest()
	{
	}

	public CObjectChest(CObjectChest state, ReferenceDictionary references)
		: base(state, references)
	{
		StartingItems = references.Get(state.StartingItems);
		if (StartingItems == null && state.StartingItems != null)
		{
			StartingItems = new List<CItem>();
			for (int i = 0; i < state.StartingItems.Count; i++)
			{
				CItem cItem = state.StartingItems[i];
				CItem cItem2 = references.Get(cItem);
				if (cItem2 == null && cItem != null)
				{
					cItem2 = new CItem(cItem, references);
					references.Add(cItem, cItem2);
				}
				StartingItems.Add(cItem2);
			}
			references.Add(state.StartingItems, StartingItems);
		}
		Gold = state.Gold;
		XP = state.XP;
		DamageValue = state.DamageValue;
		Conditions = references.Get(state.Conditions);
		if (Conditions == null && state.Conditions != null)
		{
			Conditions = new List<CCondition.ENegativeCondition>();
			for (int j = 0; j < state.Conditions.Count; j++)
			{
				CCondition.ENegativeCondition item = state.Conditions[j];
				Conditions.Add(item);
			}
			references.Add(state.Conditions, Conditions);
		}
		ChestTreasureTablesID = references.Get(state.ChestTreasureTablesID);
		if (ChestTreasureTablesID == null && state.ChestTreasureTablesID != null)
		{
			ChestTreasureTablesID = new List<string>();
			for (int k = 0; k < state.ChestTreasureTablesID.Count; k++)
			{
				string item2 = state.ChestTreasureTablesID[k];
				ChestTreasureTablesID.Add(item2);
			}
			references.Add(state.ChestTreasureTablesID, ChestTreasureTablesID);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("StartingItems", StartingItems);
		info.AddValue("Gold", Gold);
		info.AddValue("XP", XP);
		info.AddValue("Conditions", Conditions);
		info.AddValue("DamageValue", DamageValue);
		info.AddValue("ChestTreasureTablesID", ChestTreasureTablesID);
	}

	public CObjectChest(SerializationInfo info, StreamingContext context)
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
				case "StartingItems":
					StartingItems = (List<CItem>)info.GetValue("StartingItems", typeof(List<CItem>));
					break;
				case "Gold":
					Gold = info.GetInt32("Gold");
					break;
				case "XP":
					XP = info.GetInt32("XP");
					break;
				case "Conditions":
					Conditions = (List<CCondition.ENegativeCondition>)info.GetValue("Conditions", typeof(List<CCondition.ENegativeCondition>));
					break;
				case "DamageValue":
					DamageValue = info.GetInt32("DamageValue");
					break;
				case "ChestTreasureTablesID":
					ChestTreasureTablesID = (List<string>)info.GetValue("ChestTreasureTablesID", typeof(List<string>));
					break;
				case "ChestTreasureTables":
				{
					ChestTreasureTablesID = (List<string>)info.GetValue("ChestTreasureTables", typeof(List<string>));
					if (ChestTreasureTablesID == null || ChestTreasureTablesID.Count <= 0)
					{
						break;
					}
					for (int i = 0; i < ChestTreasureTablesID.Count; i++)
					{
						string text = ChestTreasureTablesID[i];
						if (text.StartsWith("£") && text.EndsWith("£") && !text.Contains("ID"))
						{
							text = text.Replace(" ", "");
							text = text.Replace("£", "");
							ChestTreasureTablesID[i] = "£" + text + "ID£";
						}
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectChest entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal new void OnDeserialized(StreamingContext context)
	{
		if (Conditions == null)
		{
			Conditions = new List<CCondition.ENegativeCondition>();
		}
	}

	public CObjectChest(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		Conditions = new List<CCondition.ENegativeCondition>();
	}

	public CObjectChest(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
		Conditions = new List<CCondition.ENegativeCondition>();
	}

	public CObjectChest(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		Conditions = new List<CCondition.ENegativeCondition>();
	}

	public CObjectChest(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid, List<string> treasureTable)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		ChestTreasureTablesID = treasureTable;
		Conditions = new List<CCondition.ENegativeCondition>();
	}

	public CObjectChest(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid, List<string> treasureTable, List<CCondition.ENegativeCondition> conditions, int damageValue)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		ChestTreasureTablesID = treasureTable;
		Conditions = conditions;
		DamageValue = damageValue;
	}

	public override bool Activate(CActor actor, CActor creditActor = null)
	{
		if ((!base.Activated && actor is CPlayerActor) || actor is CHeroSummonActor { IsCompanionSummon: not false })
		{
			ScenarioRuleClient.ToggleMessageProcessing(process: false);
			base.Activate(actor, creditActor);
			base.ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			string characterID = "";
			if (actor != null)
			{
				if (DamageValue <= 0)
				{
					List<CCondition.ENegativeCondition> conditions = Conditions;
					if (conditions == null || conditions.Count <= 0)
					{
						goto IL_02e5;
					}
				}
				if (actor is CPlayerActor cPlayerActor)
				{
					characterID = cPlayerActor.CharacterClass.ID;
				}
				if (actor is CHeroSummonActor cHeroSummonActor2)
				{
					characterID = cHeroSummonActor2.Summoner?.CharacterClass.ID;
				}
				SEventLogMessageHandler.AddEventLogMessage(new SEventObjectPropChest(DamageValue, characterID, "Activate", ESESubTypeObjectProp.Activated, base.ObjectType, base.PrefabName, m_OwnerGuid));
				if ((actor.Type == CActor.EType.Player || actor.Type == CActor.EType.HeroSummon || actor.Type == CActor.EType.Ally) && ScenarioRuleClient.s_WorkThread != Thread.CurrentThread)
				{
					ScenarioRuleClient.AddSRLQueueMessage(new CSRLChestMessage(this, actor), processImmediately: false);
					return false;
				}
				if (ScenarioManager.Scenario.HasActor(actor))
				{
					if (GameState.ActorHealthCheck(actor, actor) && Conditions != null && Conditions.Count > 0)
					{
						CActor cActor = ((base.Owner == null) ? actor : base.Owner);
						foreach (CCondition.ENegativeCondition condition in Conditions)
						{
							CAbility.EAbilityType eAbilityType = CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType s) => s.ToString() == condition.ToString());
							if (condition != CCondition.ENegativeCondition.NA && eAbilityType != CAbility.EAbilityType.None)
							{
								CAbility cAbility = CAbility.CreateAbility(eAbilityType, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: false);
								cAbility.Start(cActor, cActor);
								((CAbilityTargeting)cAbility).ApplyToActor(actor);
								if (eAbilityType == CAbility.EAbilityType.Stun || eAbilityType == CAbility.EAbilityType.Immobilize || eAbilityType == CAbility.EAbilityType.Sleep)
								{
									CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
									ScenarioRuleClient.MessageHandler(message);
								}
							}
							else
							{
								DLLDebug.LogError("Condition " + condition.ToString() + " could not be found in EAbilityType enum.");
							}
						}
					}
					int health = actor.Health;
					bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					if (DamageValue > 0)
					{
						GameState.ActorBeenDamaged(actor, DamageValue, checkIfPlayerCanAvoidDamage: true, actor);
					}
					if ((!(actor.Class is CCharacterClass) || GameState.PlayerSelectedToAvoidDamage == GameState.EAvoidDamageOption.None) && GameState.ActorHealthCheck(actor, actor, isTrap: true, isTerrain: false, actorWasAsleep))
					{
						CActorBeenDamaged_MessageData message2 = new CActorBeenDamaged_MessageData(actor)
						{
							m_ActorBeingDamaged = actor,
							m_DamageAbility = null,
							m_ActorOriginalHealth = health,
							m_ActorWasAsleep = actorWasAsleep
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
				}
			}
		}
		goto IL_02e5;
		IL_02e5:
		return false;
	}

	public override bool AutomaticActivate(CActor actor)
	{
		return false;
	}

	public virtual bool DelayedActivate(CActor actor)
	{
		return Activate(actor);
	}

	public List<Reward> GetRewardsFromStartingItems()
	{
		List<Reward> list = new List<Reward>();
		if (StartingItems != null)
		{
			foreach (CItem startingItem in StartingItems)
			{
				list.Add(new Reward(startingItem.ID, 1, ETreasureType.Item, null, EGiveToCharacterType.Give));
			}
		}
		if (Gold != 0)
		{
			list.Add(new Reward(ETreasureType.Gold, Gold, ETreasureDistributionType.Combined, null));
		}
		if (XP != 0)
		{
			list.Add(new Reward(ETreasureType.XP, XP, ETreasureDistributionType.Combined, null));
		}
		return list;
	}

	public static List<Tuple<int, string>> Compare(CObjectChest chest1, CObjectChest chest2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(chest1, chest2, isMPCompare));
			if (chest1.Gold != chest2.Gold)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1701, "CObjectChest Gold does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
					new string[3]
					{
						"ObjectType",
						chest1.ObjectType.ToString(),
						chest2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
					new string[3]
					{
						"Gold",
						chest1.Gold.ToString(),
						chest2.Gold.ToString()
					}
				});
			}
			if (chest1.XP != chest2.XP)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1702, "CObjectChest XP does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
					new string[3]
					{
						"ObjectType",
						chest1.ObjectType.ToString(),
						chest2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
					new string[3]
					{
						"XP",
						chest1.XP.ToString(),
						chest2.XP.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(chest1.StartingItems, chest2.StartingItems))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1703, "CObjectChest StartingItems null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
					new string[3]
					{
						"ObjectType",
						chest1.ObjectType.ToString(),
						chest2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
					new string[3]
					{
						"StartingItems",
						(chest1.StartingItems == null) ? "is null" : "is not null",
						(chest2.StartingItems == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (chest1.StartingItems.Count != chest2.StartingItems.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1704, "CObjectChest StartingItems Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
						new string[3]
						{
							"ObjectType",
							chest1.ObjectType.ToString(),
							chest2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
						new string[3]
						{
							"StartingItems Count",
							chest1.StartingItems.Count.ToString(),
							chest2.StartingItems.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (CItem item in chest1.StartingItems)
				{
					if (!chest2.StartingItems.Exists((CItem e) => e.ItemGuid == item.ItemGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1705, "CObjectChest StartingItems in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing item from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
							new string[3]
							{
								"ObjectType",
								chest1.ObjectType.ToString(),
								chest2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
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
				foreach (CItem item2 in chest2.StartingItems)
				{
					if (!chest1.StartingItems.Exists((CItem e) => e.ItemGuid == item2.ItemGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1705, "CObjectChest StartingItems in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing item from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
							new string[3]
							{
								"ObjectType",
								chest1.ObjectType.ToString(),
								chest2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
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
				foreach (CItem item3 in chest1.StartingItems)
				{
					try
					{
						CItem item4 = chest2.StartingItems.Single((CItem s) => s.ItemGuid == item3.ItemGuid);
						list.AddRange(CItem.Compare(item3, item4, "Chest", chest1.PropGuid, chest1.ObjectType.ToString(), isMPCompare));
					}
					catch (Exception ex)
					{
						list.Add(new Tuple<int, string>(1706, "Exception during Item State compare.\n" + ex.Message + "\n" + ex.StackTrace));
					}
				}
				break;
			}
			}
			switch (StateShared.CheckNullsMatch(chest1.ChestTreasureTablesID, chest2.ChestTreasureTablesID))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1707, "CObjectChest ChestTreasureTablesID Null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
					new string[3]
					{
						"ObjectType",
						chest1.ObjectType.ToString(),
						chest2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
					new string[3]
					{
						"ChestTreasureTablesID",
						(chest1.ChestTreasureTablesID == null) ? "is null" : "is not null",
						(chest2.ChestTreasureTablesID == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (chest1.ChestTreasureTablesID.Count != chest2.ChestTreasureTablesID.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1708, "CObjectChest ChestTreasureTablesID Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
						new string[3]
						{
							"ObjectType",
							chest1.ObjectType.ToString(),
							chest2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
						new string[3]
						{
							"ChestTreasureTablesID Count",
							chest1.ChestTreasureTablesID.Count.ToString(),
							chest2.ChestTreasureTablesID.Count.ToString()
						}
					});
					break;
				}
				foreach (string item5 in chest1.ChestTreasureTablesID)
				{
					if (!chest2.ChestTreasureTablesID.Contains(item5))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1709, "CObjectChest ChestTreasureTablesID in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
							new string[3]
							{
								"ObjectType",
								chest1.ObjectType.ToString(),
								chest2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
							new string[3] { "ChestTreasureTablesID", item5, "Missing" }
						});
					}
				}
				foreach (string item6 in chest2.ChestTreasureTablesID)
				{
					if (!chest1.ChestTreasureTablesID.Contains(item6))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1709, "CObjectChest ChestTreasureTablesID in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", chest1.PropGuid, chest2.PropGuid },
							new string[3]
							{
								"ObjectType",
								chest1.ObjectType.ToString(),
								chest2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", chest1.PrefabName, chest2.PrefabName },
							new string[3] { "ChestTreasureTablesID", "Missing", item6 }
						});
					}
				}
				break;
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(1799, "Exception during CObjectChest compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}
}
