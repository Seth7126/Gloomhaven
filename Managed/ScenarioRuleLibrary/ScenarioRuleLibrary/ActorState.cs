using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.CustomLevels;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("Name:{ClassID}")]
public class ActorState : ISerializable
{
	public string ClassID { get; protected set; }

	public string ActorGuid { get; set; }

	public TileIndex Location { get; set; }

	public int Health { get; set; }

	public int MaxHealth { get; set; }

	public int Level { get; set; }

	public List<PositiveConditionPair> PositiveConditions { get; protected set; }

	public List<NegativeConditionPair> NegativeConditions { get; protected set; }

	public bool PlayedThisRound { get; protected set; }

	public CActor.ECauseOfDeath CauseOfDeath { get; set; }

	public string KilledByActorGuid { get; protected set; }

	public bool IsRevealed { get; set; }

	public string StartingMapGuid { get; set; }

	public int AugmentSlots { get; protected set; }

	public CAIFocusOverrideDetails AIFocusOverride { get; protected set; }

	public bool PhasedOut { get; protected set; }

	public bool Deactivated { get; protected set; }

	public int ChosenModelIndex { get; protected set; }

	public List<CObjectProp> CarriedQuestProps { get; protected set; }

	public Dictionary<string, int> CharacterResources { get; set; }

	public bool IsDead => CauseOfDeath != CActor.ECauseOfDeath.StillAlive;

	public bool IsDeadForObjectives
	{
		get
		{
			if (CauseOfDeath != CActor.ECauseOfDeath.StillAlive)
			{
				return CauseOfDeath != CActor.ECauseOfDeath.ActorRemovedFromMap;
			}
			return false;
		}
	}

	public CActor Actor
	{
		get
		{
			if (ActorGuid != null && ActorGuid != string.Empty)
			{
				return ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor s) => s.ActorGuid == ActorGuid);
			}
			return null;
		}
	}

	public int ActorID
	{
		get
		{
			if (this is PlayerState)
			{
				return 0;
			}
			if (this is EnemyState enemyState)
			{
				return enemyState.ID;
			}
			if (this is HeroSummonState heroSummonState)
			{
				return heroSummonState.ID;
			}
			throw new Exception("Invalid ActorState type. Unable to get Actor ID.");
		}
	}

	public ActorState()
	{
	}

	public ActorState(ActorState state, ReferenceDictionary references)
	{
		ClassID = state.ClassID;
		ActorGuid = state.ActorGuid;
		Location = references.Get(state.Location);
		if (Location == null && state.Location != null)
		{
			Location = new TileIndex(state.Location, references);
			references.Add(state.Location, Location);
		}
		Health = state.Health;
		MaxHealth = state.MaxHealth;
		Level = state.Level;
		PositiveConditions = references.Get(state.PositiveConditions);
		if (PositiveConditions == null && state.PositiveConditions != null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
			for (int i = 0; i < state.PositiveConditions.Count; i++)
			{
				PositiveConditionPair positiveConditionPair = state.PositiveConditions[i];
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
			for (int j = 0; j < state.NegativeConditions.Count; j++)
			{
				NegativeConditionPair negativeConditionPair = state.NegativeConditions[j];
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
		PlayedThisRound = state.PlayedThisRound;
		CauseOfDeath = state.CauseOfDeath;
		KilledByActorGuid = state.KilledByActorGuid;
		IsRevealed = state.IsRevealed;
		StartingMapGuid = state.StartingMapGuid;
		AugmentSlots = state.AugmentSlots;
		AIFocusOverride = references.Get(state.AIFocusOverride);
		if (AIFocusOverride == null && state.AIFocusOverride != null)
		{
			AIFocusOverride = new CAIFocusOverrideDetails(state.AIFocusOverride, references);
			references.Add(state.AIFocusOverride, AIFocusOverride);
		}
		PhasedOut = state.PhasedOut;
		Deactivated = state.Deactivated;
		ChosenModelIndex = state.ChosenModelIndex;
		CarriedQuestProps = references.Get(state.CarriedQuestProps);
		if (CarriedQuestProps == null && state.CarriedQuestProps != null)
		{
			CarriedQuestProps = new List<CObjectProp>();
			for (int k = 0; k < state.CarriedQuestProps.Count; k++)
			{
				CObjectProp cObjectProp = state.CarriedQuestProps[k];
				CObjectProp cObjectProp2 = references.Get(cObjectProp);
				if (cObjectProp2 == null && cObjectProp != null)
				{
					CObjectProp cObjectProp3 = ((cObjectProp is CObjectChest state2) ? new CObjectChest(state2, references) : ((cObjectProp is CObjectDifficultTerrain state3) ? new CObjectDifficultTerrain(state3, references) : ((cObjectProp is CObjectDoor state4) ? new CObjectDoor(state4, references) : ((cObjectProp is CObjectGoldPile state5) ? new CObjectGoldPile(state5, references) : ((cObjectProp is CObjectHazardousTerrain state6) ? new CObjectHazardousTerrain(state6, references) : ((cObjectProp is CObjectMonsterGrave state7) ? new CObjectMonsterGrave(state7, references) : ((cObjectProp is CObjectObstacle state8) ? new CObjectObstacle(state8, references) : ((cObjectProp is CObjectPortal state9) ? new CObjectPortal(state9, references) : ((cObjectProp is CObjectPressurePlate state10) ? new CObjectPressurePlate(state10, references) : ((cObjectProp is CObjectQuestItem state11) ? new CObjectQuestItem(state11, references) : ((cObjectProp is CObjectResource state12) ? new CObjectResource(state12, references) : ((cObjectProp is CObjectTerrainVisual state13) ? new CObjectTerrainVisual(state13, references) : ((!(cObjectProp is CObjectTrap state14)) ? new CObjectProp(cObjectProp, references) : new CObjectTrap(state14, references))))))))))))));
					cObjectProp2 = cObjectProp3;
					references.Add(cObjectProp, cObjectProp2);
				}
				CarriedQuestProps.Add(cObjectProp2);
			}
			references.Add(state.CarriedQuestProps, CarriedQuestProps);
		}
		CharacterResources = references.Get(state.CharacterResources);
		if (CharacterResources != null || state.CharacterResources == null)
		{
			return;
		}
		CharacterResources = new Dictionary<string, int>(state.CharacterResources.Comparer);
		foreach (KeyValuePair<string, int> characterResource in state.CharacterResources)
		{
			string key = characterResource.Key;
			int value = characterResource.Value;
			CharacterResources.Add(key, value);
		}
		references.Add(state.CharacterResources, CharacterResources);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ClassID", ClassID);
		info.AddValue("ActorGuid", ActorGuid);
		info.AddValue("Location", Location);
		info.AddValue("Health", Health);
		info.AddValue("MaxHealth", MaxHealth);
		info.AddValue("Level", Level);
		info.AddValue("PositiveConditions", PositiveConditions);
		info.AddValue("NegativeConditions", NegativeConditions);
		info.AddValue("PlayedThisRound", PlayedThisRound);
		info.AddValue("CauseOfDeath", CauseOfDeath);
		info.AddValue("KilledByActorGuid", KilledByActorGuid);
		info.AddValue("IsRevealed", IsRevealed);
		info.AddValue("StartingMapGuid", StartingMapGuid);
		info.AddValue("AugmentSlots", AugmentSlots);
		info.AddValue("AIFocusOverride", AIFocusOverride);
		info.AddValue("PhasedOut", PhasedOut);
		info.AddValue("Deactivated", Deactivated);
		info.AddValue("ChosenModelIndex", ChosenModelIndex);
		info.AddValue("CarriedQuestProps", CarriedQuestProps);
		info.AddValue("CharacterResources", CharacterResources);
	}

	public ActorState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ClassID":
					ClassID = info.GetString("ClassID");
					if (ClassID == "Elementalist3DemonSummonAltarID")
					{
						ClassID = "DemonSummonAltarID";
					}
					break;
				case "ActorGuid":
					ActorGuid = info.GetString("ActorGuid");
					break;
				case "Location":
					Location = (TileIndex)info.GetValue("Location", typeof(TileIndex));
					break;
				case "Health":
					Health = info.GetInt32("Health");
					break;
				case "MaxHealth":
					MaxHealth = info.GetInt32("MaxHealth");
					break;
				case "Level":
					Level = info.GetInt32("Level");
					break;
				case "PositiveConditions":
					PositiveConditions = (List<PositiveConditionPair>)info.GetValue("PositiveConditions", typeof(List<PositiveConditionPair>));
					break;
				case "NegativeConditions":
					NegativeConditions = (List<NegativeConditionPair>)info.GetValue("NegativeConditions", typeof(List<NegativeConditionPair>));
					break;
				case "PlayedThisRound":
					PlayedThisRound = info.GetBoolean("PlayedThisRound");
					break;
				case "CauseOfDeath":
					CauseOfDeath = (CActor.ECauseOfDeath)info.GetValue("CauseOfDeath", typeof(CActor.ECauseOfDeath));
					break;
				case "KilledByActorGuid":
					KilledByActorGuid = info.GetString("KilledByActorGuid");
					break;
				case "IsRevealed":
					IsRevealed = info.GetBoolean("IsRevealed");
					break;
				case "StartingMapGuid":
					StartingMapGuid = info.GetString("StartingMapGuid");
					break;
				case "AugmentSlots":
					AugmentSlots = info.GetInt32("AugmentSlots");
					break;
				case "ClassName":
				{
					string text = info.GetString("ClassName").Replace(" ", string.Empty);
					ClassID = text + "ID";
					if (ClassID == "Elementalist3DemonSummonAltarID")
					{
						ClassID = "DemonSummonAltarID";
					}
					break;
				}
				case "AIFocusOverride":
					AIFocusOverride = (CAIFocusOverrideDetails)info.GetValue("AIFocusOverride", typeof(CAIFocusOverrideDetails));
					break;
				case "PhasedOut":
					PhasedOut = info.GetBoolean("PhasedOut");
					break;
				case "Deactivated":
					Deactivated = info.GetBoolean("Deactivated");
					break;
				case "ChosenModelIndex":
					ChosenModelIndex = info.GetInt32("ChosenModelIndex");
					break;
				case "CarriedQuestProps":
					CarriedQuestProps = (List<CObjectProp>)info.GetValue("CarriedQuestProps", typeof(List<CObjectProp>));
					break;
				case "CharacterResources":
					CharacterResources = (Dictionary<string, int>)info.GetValue("CharacterResources", typeof(Dictionary<string, int>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize ActorState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		bool flag = false;
		enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current2 = enumerator.Current;
			try
			{
				if (current2.Name == "IsDead")
				{
					flag = info.GetBoolean("IsDead");
				}
			}
			catch (Exception ex2)
			{
				DLLDebug.LogError("Exception while trying to deserialize custom level entry " + current2.Name + "\n" + ex2.Message + "\n" + ex2.StackTrace);
				throw ex2;
			}
		}
		if (AugmentSlots == 0)
		{
			AugmentSlots = 1;
		}
		if (flag)
		{
			CauseOfDeath = CActor.ECauseOfDeath.Undetermined;
		}
		else if (CauseOfDeath == CActor.ECauseOfDeath.None)
		{
			CauseOfDeath = CActor.ECauseOfDeath.StillAlive;
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		onDeserialized(context);
	}

	protected virtual void onDeserialized(StreamingContext context)
	{
		if (CarriedQuestProps == null)
		{
			CarriedQuestProps = new List<CObjectProp>();
		}
		if (PositiveConditions == null)
		{
			PositiveConditions = new List<PositiveConditionPair>();
		}
		if (NegativeConditions == null)
		{
			NegativeConditions = new List<NegativeConditionPair>();
		}
		if (CharacterResources == null)
		{
			CharacterResources = new Dictionary<string, int>();
		}
	}

	public ActorState(string classID, int chosenModelIndex, string actorGuid, string mapGuid, TileIndex tileIndex, int health, int maxHealth, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, bool playedThisRound, CActor.ECauseOfDeath causeOfDeath, int augmentSlots)
	{
		ClassID = classID;
		ChosenModelIndex = chosenModelIndex;
		ActorGuid = actorGuid;
		StartingMapGuid = mapGuid;
		Location = tileIndex;
		Health = health;
		MaxHealth = maxHealth;
		Level = level;
		PositiveConditions = posCons;
		NegativeConditions = negCons;
		PlayedThisRound = playedThisRound;
		CauseOfDeath = causeOfDeath;
		IsRevealed = false;
		AugmentSlots = augmentSlots;
		PhasedOut = false;
		Deactivated = false;
		CarriedQuestProps = new List<CObjectProp>();
		CharacterResources = new Dictionary<string, int>();
	}

	public void ResetCauseOfDeath()
	{
		CauseOfDeath = CActor.ECauseOfDeath.StillAlive;
	}

	public void SetAIFocusOverride(CAIFocusOverrideDetails aiFocusOverride)
	{
		AIFocusOverride = aiFocusOverride;
	}

	public void ResetAIFocusOverride()
	{
		if (AIFocusOverride != null)
		{
			AIFocusOverride = new CAIFocusOverrideDetails();
		}
	}

	public static List<Tuple<int, string>> Compare(ActorState state1, ActorState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		string text = "Actor State";
		string text2 = null;
		string text3 = null;
		if (state1 is PlayerState)
		{
			text = "Player State";
		}
		else if (state1 is EnemyState enemyState && state2 is EnemyState enemyState2)
		{
			text = "Enemy State";
			text2 = enemyState.ID.ToString();
			text3 = enemyState2.ID.ToString();
		}
		else if (state1 is HeroSummonState heroSummonState && state2 is HeroSummonState heroSummonState2)
		{
			text = "Hero Summon State";
			text2 = heroSummonState.ID.ToString();
			text3 = heroSummonState2.ID.ToString();
		}
		try
		{
			if (state1.ActorGuid != state2.ActorGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 400, "Enemy State ActorGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					}
				});
			}
			if (state1.ClassID != state2.ClassID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 401, text + " ClassID does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					}
				});
			}
			if (!TileIndex.Compare(state1.Location, state2.Location))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 402, text + " Location does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					}
				});
			}
			if (state1.Health != state2.Health)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 403, text + " Health does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"Health",
						state1.Health.ToString(),
						state2.Health.ToString()
					}
				});
			}
			if (state1.MaxHealth != state2.MaxHealth)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 404, text + " MaxHealth does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"MaxHealth",
						state1.MaxHealth.ToString(),
						state2.MaxHealth.ToString()
					}
				});
			}
			if (state1.Level != state2.Level)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 405, text + " Level does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"Level",
						state1.Level.ToString(),
						state2.Level.ToString()
					}
				});
			}
			if (state1.PlayedThisRound != state2.PlayedThisRound)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 406, text + " PlayedThisRound does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"PlayedThisRound",
						state1.PlayedThisRound.ToString(),
						state2.PlayedThisRound.ToString()
					}
				});
			}
			if (state1.CauseOfDeath != state2.CauseOfDeath)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 407, text + " CauseOfDeath does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"CauseOfDeath",
						state1.CauseOfDeath.ToString(),
						state2.CauseOfDeath.ToString()
					}
				});
			}
			if (state1.IsRevealed != state2.IsRevealed)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 408, text + " IsRevealed does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"IsRevealed",
						state1.IsRevealed.ToString(),
						state2.IsRevealed.ToString()
					}
				});
			}
			if (state1.StartingMapGuid != state2.StartingMapGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 409, text + " StartingMapGuid does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3] { "StartingMapGuid", state1.StartingMapGuid, state2.StartingMapGuid }
				});
			}
			if (state1.AugmentSlots != state2.AugmentSlots)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 410, text + " AugmentSlots does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"AugmentSlots",
						state1.AugmentSlots.ToString(),
						state2.AugmentSlots.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(state1.PositiveConditions, state2.PositiveConditions))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 411, text + " PositiveConditions null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"PositiveConditions",
						(state1.PositiveConditions == null) ? "is null" : "is not null",
						(state2.PositiveConditions == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.PositiveConditions.Count != state2.PositiveConditions.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 412, text + " total PositiveConditions Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Enemy ID",
							(text2 != null) ? text2 : "NA",
							(text3 != null) ? text3 : "NA"
						},
						new string[3]
						{
							"Location",
							state1.Location.ToString(),
							state2.Location.ToString()
						},
						new string[3]
						{
							"PositiveConditions Count",
							state1.PositiveConditions.Count.ToString(),
							state2.PositiveConditions.Count.ToString()
						}
					});
					break;
				}
				foreach (PositiveConditionPair posCon1 in state1.PositiveConditions)
				{
					List<PositiveConditionPair> list2 = state1.PositiveConditions.Where((PositiveConditionPair w) => w.Equals(posCon1)).ToList();
					List<PositiveConditionPair> list3 = state2.PositiveConditions.Where((PositiveConditionPair w) => w.Equals(posCon1)).ToList();
					if (list2.Count != list3.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 413, text + " PositiveCondition Count does not match.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Enemy ID",
								(text2 != null) ? text2 : "NA",
								(text3 != null) ? text3 : "NA"
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"PositiveCondition",
								posCon1.ToString(),
								(list3.Count > 0) ? list3.First().ToString() : "None"
							},
							new string[3]
							{
								"PositiveCondition Count",
								list2.Count.ToString(),
								list3.Count.ToString()
							}
						});
					}
				}
				foreach (PositiveConditionPair posCon2 in state2.PositiveConditions)
				{
					List<PositiveConditionPair> list4 = state1.PositiveConditions.Where((PositiveConditionPair w) => w.Equals(posCon2)).ToList();
					List<PositiveConditionPair> list5 = state2.PositiveConditions.Where((PositiveConditionPair w) => w.Equals(posCon2)).ToList();
					if (list4.Count != list5.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 413, text + " PositiveCondition Count does not match.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Enemy ID",
								(text2 != null) ? text2 : "NA",
								(text3 != null) ? text3 : "NA"
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"PositiveCondition",
								(list4.Count > 0) ? list4.First().ToString() : "None",
								posCon2.ToString()
							},
							new string[3]
							{
								"PositiveCondition Count",
								list4.Count.ToString(),
								list5.Count.ToString()
							}
						});
					}
				}
				break;
			}
			switch (StateShared.CheckNullsMatch(state1.NegativeConditions, state2.NegativeConditions))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 414, text + " NegativeConditions null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"NegativeConditions",
						(state1.NegativeConditions == null) ? "is null" : "is not null",
						(state2.NegativeConditions == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (state1.NegativeConditions.Count != state2.NegativeConditions.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 415, text + " total NegativeConditions Count does not match.", new List<string[]>
					{
						new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"Enemy ID",
							(text2 != null) ? text2 : "NA",
							(text3 != null) ? text3 : "NA"
						},
						new string[3]
						{
							"Location",
							state1.Location.ToString(),
							state2.Location.ToString()
						},
						new string[3]
						{
							"NegativeConditions Count",
							state1.NegativeConditions.Count.ToString(),
							state2.NegativeConditions.Count.ToString()
						}
					});
					break;
				}
				foreach (NegativeConditionPair negCon1 in state1.NegativeConditions)
				{
					List<NegativeConditionPair> list6 = state1.NegativeConditions.Where((NegativeConditionPair w) => w.Equals(negCon1)).ToList();
					List<NegativeConditionPair> list7 = state2.NegativeConditions.Where((NegativeConditionPair w) => w.Equals(negCon1)).ToList();
					if (list6.Count != list7.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 416, text + " NegativeCondition Count does not match.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Enemy ID",
								(text2 != null) ? text2 : "NA",
								(text3 != null) ? text3 : "NA"
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"NegativeCondition",
								negCon1.ToString(),
								(list7.Count > 0) ? list7.First().ToString() : "None"
							},
							new string[3]
							{
								"NegativeCondition Count",
								list6.Count.ToString(),
								list7.Count.ToString()
							}
						});
					}
				}
				foreach (NegativeConditionPair negCon2 in state2.NegativeConditions)
				{
					List<NegativeConditionPair> list8 = state1.NegativeConditions.Where((NegativeConditionPair w) => w.Equals(negCon2)).ToList();
					List<NegativeConditionPair> list9 = state2.NegativeConditions.Where((NegativeConditionPair w) => w.Equals(negCon2)).ToList();
					if (list8.Count != list9.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 416, text + " NegativeCondition Count does not match.", new List<string[]>
						{
							new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Enemy ID",
								(text2 != null) ? text2 : "NA",
								(text3 != null) ? text3 : "NA"
							},
							new string[3]
							{
								"Location",
								state1.Location.ToString(),
								state2.Location.ToString()
							},
							new string[3]
							{
								"NegativeCondition",
								(list8.Count > 0) ? list8.First().ToString() : "None",
								negCon2.ToString()
							},
							new string[3]
							{
								"NegativeCondition Count",
								list8.Count.ToString(),
								list9.Count.ToString()
							}
						});
					}
				}
				break;
			}
			StateShared.ENullStatus eNullStatus = StateShared.CheckNullsMatch(state1.AIFocusOverride, state2.AIFocusOverride);
			if (eNullStatus == StateShared.ENullStatus.Mismatch)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 417, text + " AIFocusOverride null state does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Enemy ID",
						(text2 != null) ? text2 : "NA",
						(text3 != null) ? text3 : "NA"
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"AIFocusOverride",
						(state1.AIFocusOverride == null) ? "is null" : "is not null",
						(state2.AIFocusOverride == null) ? "is null" : "is not null"
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(499, "Exception during " + text + " compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
