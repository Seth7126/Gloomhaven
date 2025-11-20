using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using SharedLibrary;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} | {ArrayIndex} | {PropGuid}")]
public class CObjectProp : ISerializable
{
	public static EPropType[] PropTypes = (EPropType[])Enum.GetValues(typeof(EPropType));

	public static ESpecificPropType[] SpecificPropTypes = (ESpecificPropType[])Enum.GetValues(typeof(ESpecificPropType));

	public static ScenarioManager.EPerPartySizeConfig[] PerPartySizeConfigOptions = ((ScenarioManager.EPerPartySizeConfig[])Enum.GetValues(typeof(ScenarioManager.EPerPartySizeConfig))).Where((ScenarioManager.EPerPartySizeConfig c) => c != ScenarioManager.EPerPartySizeConfig.ToElite).ToArray();

	private Dictionary<int, ScenarioManager.EPerPartySizeConfig> ConfigPerPartySize;

	protected string m_OwnerGuid;

	public string PropGuid { get; private set; }

	public ScenarioManager.ObjectImportType ObjectType { get; private set; }

	public string PrefabName { get; set; }

	public TileIndex ArrayIndex { get; private set; }

	public CVector3 Position { get; private set; }

	public CVector3 Rotation { get; private set; }

	public bool Activated { get; protected set; }

	public string StartingMapGuid { get; set; }

	public string ActorActivated { get; protected set; }

	public string OwnerGUID => m_OwnerGuid;

	public EPropType PropType
	{
		get
		{
			ESpecificPropType eSpecificPropType = SpecificPropTypes.SingleOrDefault((ESpecificPropType x) => x.ToString() == PrefabName);
			if (eSpecificPropType != ESpecificPropType.None)
			{
				return SpecificPropTypeToPropType(eSpecificPropType);
			}
			return PropTypes.SingleOrDefault((EPropType x) => x.ToString() == PrefabName);
		}
	}

	public CActor Owner
	{
		get
		{
			CActor cActor = null;
			if (m_OwnerGuid != null)
			{
				cActor = ScenarioManager.Scenario.AllPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == m_OwnerGuid);
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.HeroSummons.SingleOrDefault((CHeroSummonActor s) => s.ActorGuid == m_OwnerGuid);
					if (cActor == null)
					{
						cActor = ScenarioManager.Scenario.AllMonsters.SingleOrDefault((CEnemyActor s) => s.ActorGuid == m_OwnerGuid);
					}
				}
			}
			return cActor;
		}
	}

	public CMap StartingMap => ScenarioManager.CurrentScenarioState.Maps.SingleOrDefault((CMap map) => map.MapGuid == StartingMapGuid);

	public bool IsLootable
	{
		get
		{
			if (ObjectType != ScenarioManager.ObjectImportType.Chest && ObjectType != ScenarioManager.ObjectImportType.GoalChest && ObjectType != ScenarioManager.ObjectImportType.MoneyToken && ObjectType != ScenarioManager.ObjectImportType.CarryableQuestItem)
			{
				return ObjectType == ScenarioManager.ObjectImportType.Resource;
			}
			return true;
		}
	}

	public CTile PropTile => ScenarioManager.Tiles[ArrayIndex.X, ArrayIndex.Y];

	public bool IsDestroyed => ScenarioManager.CurrentScenarioState.DestroyedProps.SingleOrDefault((CObjectProp x) => x.PropGuid == PropGuid) != null;

	public bool IgnoreEndOfTurnLooting { get; private set; }

	public CObjectiveFilter CanLootFilter { get; set; }

	public string CanLootLocKey { get; set; }

	public PropHealthDetails PropHealthDetails { get; set; }

	public bool PropActorHasBeenAssigned { get; private set; }

	public CObjectActor RuntimeAttachedActor { get; private set; }

	public bool OverrideDisallowDestroyAndMove { get; private set; }

	public string InstanceName => PrefabName + " : (" + PropGuid + ")";

	public CObjectProp()
	{
	}

	public CObjectProp(CObjectProp state, ReferenceDictionary references)
	{
		PropGuid = state.PropGuid;
		ObjectType = state.ObjectType;
		PrefabName = state.PrefabName;
		ArrayIndex = references.Get(state.ArrayIndex);
		if (ArrayIndex == null && state.ArrayIndex != null)
		{
			ArrayIndex = new TileIndex(state.ArrayIndex, references);
			references.Add(state.ArrayIndex, ArrayIndex);
		}
		Position = references.Get(state.Position);
		if (Position == null && state.Position != null)
		{
			Position = new CVector3(state.Position, references);
			references.Add(state.Position, Position);
		}
		Rotation = references.Get(state.Rotation);
		if (Rotation == null && state.Rotation != null)
		{
			Rotation = new CVector3(state.Rotation, references);
			references.Add(state.Rotation, Rotation);
		}
		Activated = state.Activated;
		StartingMapGuid = state.StartingMapGuid;
		ActorActivated = state.ActorActivated;
		ConfigPerPartySize = references.Get(state.ConfigPerPartySize);
		if (ConfigPerPartySize == null && state.ConfigPerPartySize != null)
		{
			ConfigPerPartySize = new Dictionary<int, ScenarioManager.EPerPartySizeConfig>(state.ConfigPerPartySize.Comparer);
			foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item in state.ConfigPerPartySize)
			{
				int key = item.Key;
				ScenarioManager.EPerPartySizeConfig value = item.Value;
				ConfigPerPartySize.Add(key, value);
			}
			references.Add(state.ConfigPerPartySize, ConfigPerPartySize);
		}
		IgnoreEndOfTurnLooting = state.IgnoreEndOfTurnLooting;
		CanLootFilter = references.Get(state.CanLootFilter);
		if (CanLootFilter == null && state.CanLootFilter != null)
		{
			CanLootFilter = new CObjectiveFilter(state.CanLootFilter, references);
			references.Add(state.CanLootFilter, CanLootFilter);
		}
		CanLootLocKey = state.CanLootLocKey;
		PropHealthDetails = references.Get(state.PropHealthDetails);
		if (PropHealthDetails == null && state.PropHealthDetails != null)
		{
			PropHealthDetails = new PropHealthDetails(state.PropHealthDetails, references);
			references.Add(state.PropHealthDetails, PropHealthDetails);
		}
		PropActorHasBeenAssigned = state.PropActorHasBeenAssigned;
		OverrideDisallowDestroyAndMove = state.OverrideDisallowDestroyAndMove;
		m_OwnerGuid = state.m_OwnerGuid;
	}

	public static EPropType SpecificPropTypeToPropType(ESpecificPropType specificPropType)
	{
		switch (specificPropType)
		{
		case ESpecificPropType.RockSingle:
		case ESpecificPropType.PlinthSingle:
			return EPropType.OneHexObstacle;
		case ESpecificPropType.BearTrap:
		case ESpecificPropType.BombTrap:
		case ESpecificPropType.DamageMine:
		case ESpecificPropType.PoisonMine:
		case ESpecificPropType.StunMine:
			return EPropType.Trap;
		case ESpecificPropType.RockTriple:
			return EPropType.ThreeHexObstacle;
		case ESpecificPropType.SewerPipeSpawner:
		case ESpecificPropType.GraveSingleSpawner:
		case ESpecificPropType.GraveDoubleSpawner:
			return EPropType.Spawner;
		case ESpecificPropType.QuestDoll:
		case ESpecificPropType.QuestClaw:
			return EPropType.QuestItem;
		case ESpecificPropType.TheFavorite:
		case ESpecificPropType.TheNewFavorite:
			return EPropType.CharacterResource;
		default:
			return EPropType.None;
		}
	}

	public static bool IsLootableObjectImportType(ScenarioManager.ObjectImportType objectType)
	{
		if (objectType != ScenarioManager.ObjectImportType.Chest && objectType != ScenarioManager.ObjectImportType.GoalChest && objectType != ScenarioManager.ObjectImportType.MoneyToken && objectType != ScenarioManager.ObjectImportType.CarryableQuestItem)
		{
			return objectType == ScenarioManager.ObjectImportType.Resource;
		}
		return true;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PropGuid", PropGuid);
		info.AddValue("ObjectType", ObjectType);
		info.AddValue("PrefabName", PrefabName);
		info.AddValue("ArrayIndex", ArrayIndex);
		info.AddValue("Position", Position);
		info.AddValue("Rotation", Rotation);
		info.AddValue("Activated", Activated);
		info.AddValue("m_OwnerGuid", m_OwnerGuid);
		info.AddValue("StartingMapGuid", StartingMapGuid);
		info.AddValue("ActorActivated", ActorActivated);
		info.AddValue("ConfigPerPartySize", ConfigPerPartySize);
		info.AddValue("IgnoreEndOfTurnLooting", IgnoreEndOfTurnLooting);
		info.AddValue("PropHealthDetails", PropHealthDetails);
		info.AddValue("PropActorHasBeenAssigned", PropActorHasBeenAssigned);
		info.AddValue("OverrideDisallowDestroyAndMove", OverrideDisallowDestroyAndMove);
		info.AddValue("CanLootFilter", CanLootFilter);
		info.AddValue("CanLootLocKey", CanLootLocKey);
	}

	public CObjectProp(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "PropGuid":
					PropGuid = info.GetString("PropGuid");
					break;
				case "ObjectType":
					ObjectType = (ScenarioManager.ObjectImportType)info.GetValue("ObjectType", typeof(ScenarioManager.ObjectImportType));
					break;
				case "PrefabName":
					PrefabName = info.GetString("PrefabName");
					break;
				case "ArrayIndex":
					ArrayIndex = (TileIndex)info.GetValue("ArrayIndex", typeof(TileIndex));
					break;
				case "Position":
					Position = (CVector3)info.GetValue("Position", typeof(CVector3));
					break;
				case "Rotation":
					Rotation = (CVector3)info.GetValue("Rotation", typeof(CVector3));
					break;
				case "Activated":
					Activated = info.GetBoolean("Activated");
					break;
				case "m_OwnerGuid":
					m_OwnerGuid = info.GetString("m_OwnerGuid");
					break;
				case "StartingMapGuid":
					StartingMapGuid = info.GetString("StartingMapGuid");
					break;
				case "ActorActivated":
					ActorActivated = info.GetString("ActorActivated");
					break;
				case "ConfigPerPartySize":
					ConfigPerPartySize = (Dictionary<int, ScenarioManager.EPerPartySizeConfig>)info.GetValue("ConfigPerPartySize", typeof(Dictionary<int, ScenarioManager.EPerPartySizeConfig>));
					break;
				case "IgnoreEndOfTurnLooting":
					IgnoreEndOfTurnLooting = info.GetBoolean("IgnoreEndOfTurnLooting");
					break;
				case "PropHealthDetails":
					PropHealthDetails = (PropHealthDetails)info.GetValue("PropHealthDetails", typeof(PropHealthDetails));
					break;
				case "PropActorHasBeenAssigned":
					PropActorHasBeenAssigned = info.GetBoolean("PropActorHasBeenAssigned");
					break;
				case "OverrideDisallowDestroyAndMove":
					OverrideDisallowDestroyAndMove = info.GetBoolean("OverrideDisallowDestroyAndMove");
					break;
				case "CanLootFilter":
					CanLootFilter = (CObjectiveFilter)info.GetValue("CanLootFilter", typeof(CObjectiveFilter));
					break;
				case "CanLootLocKey":
					CanLootLocKey = info.GetString("CanLootLocKey");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectProp entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		InternalOnDeserialized();
	}

	protected virtual void InternalOnDeserialized()
	{
		if (PrefabName == "GraveSingle")
		{
			PrefabName = ESpecificPropType.GraveSingleSpawner.ToString();
		}
		if (PrefabName == "GraveDouble")
		{
			PrefabName = ESpecificPropType.GraveDoubleSpawner.ToString();
		}
	}

	public CObjectProp(string prefabName, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
	{
		if (ScenarioManager.CurrentScenarioState != null && ScenarioManager.CurrentScenarioState.GuidRNG != null)
		{
			PropGuid = ScenarioManager.CurrentScenarioState.GetGUIDBasedOnGuidRNGState().ToString();
		}
		else
		{
			PropGuid = Guid.NewGuid().ToString();
		}
		StartingMapGuid = mapGuid;
		PrefabName = prefabName;
		ObjectType = type;
		ArrayIndex = arrayIndex;
		Position = position;
		Rotation = rotation;
		m_OwnerGuid = owner?.ActorGuid;
		ConfigPerPartySize = GetDefaultConfigPerPartySize();
	}

	public CObjectProp(SharedLibrary.Random scenarioGenerationRNG, string prefabName, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, CActor owner, string mapGuid)
	{
		byte[] array = new byte[16];
		scenarioGenerationRNG.NextBytes(array);
		PropGuid = new Guid(array).ToString();
		StartingMapGuid = mapGuid;
		PrefabName = prefabName;
		ObjectType = type;
		ArrayIndex = arrayIndex;
		Position = position;
		Rotation = rotation;
		m_OwnerGuid = owner?.ActorGuid;
		ConfigPerPartySize = GetDefaultConfigPerPartySize();
	}

	public void SetLocation(TileIndex arrayIndex, CVector3 position, CVector3 rotation)
	{
		ArrayIndex = arrayIndex;
		Position = position;
		Rotation = rotation;
	}

	public virtual bool AutomaticActivate(CActor actor)
	{
		return Activate(actor);
	}

	public virtual bool Activate(CActor actor, CActor creditActor = null)
	{
		if (!Activated)
		{
			Activated = true;
			ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			if (ObjectType == ScenarioManager.ObjectImportType.Chest || ObjectType == ScenarioManager.ObjectImportType.GoalChest || ObjectType == ScenarioManager.ObjectImportType.MoneyToken || ObjectType == ScenarioManager.ObjectImportType.Trap || ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || ObjectType == ScenarioManager.ObjectImportType.Resource || ObjectType == ScenarioManager.ObjectImportType.MonsterGrave)
			{
				ScenarioManager.CurrentScenarioState.Props.Remove(this);
				ScenarioManager.CurrentScenarioState.ActivatedProps.Add(this);
			}
			CActivateProp_MessageData message = new CActivateProp_MessageData(actor)
			{
				m_Prop = this,
				m_InitialLoad = false,
				m_CreditActor = creditActor
			};
			ScenarioRuleClient.MessageHandler(message);
			return true;
		}
		return false;
	}

	public virtual bool Deactivate()
	{
		return false;
	}

	public virtual void DestroyProp(float spawnDelay = 0f, bool sendMessageToClient = true)
	{
		if (!IsDestroyed)
		{
			ScenarioManager.CurrentScenarioState.Props.Remove(this);
			ScenarioManager.CurrentScenarioState.DestroyedProps.Add(this);
			PropTile.m_Props.Remove(this);
			if (sendMessageToClient)
			{
				CDestroyProp_MessageData message = new CDestroyProp_MessageData
				{
					m_Prop = this,
					m_DestroyDelay = spawnDelay
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
	}

	public virtual bool WillActivationKillActor(CActor actor)
	{
		return false;
	}

	public virtual bool WillActivationDamageActor(CActor actor)
	{
		return false;
	}

	public bool CanActorLoot(CActor actor)
	{
		if (CanLootFilter != null)
		{
			ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.SingleOrDefault((ActorState x) => x.ActorGuid == actor.ActorGuid);
			return CanLootFilter.IsValidTarget(actorState);
		}
		return true;
	}

	public void SetConfigForPartySize(int partySize, ScenarioManager.EPerPartySizeConfig configToSet)
	{
		ConfigPerPartySize[partySize] = configToSet;
	}

	public ScenarioManager.EPerPartySizeConfig GetConfigForPartySize(int partySize)
	{
		if (ConfigPerPartySize == null || !ConfigPerPartySize.ContainsKey(partySize))
		{
			return ScenarioManager.EPerPartySizeConfig.Normal;
		}
		return ConfigPerPartySize[partySize];
	}

	public void SetIgnoreEndOfTurnLooting(bool shouldIgnore)
	{
		IgnoreEndOfTurnLooting = shouldIgnore;
	}

	public void SetCanLootFilter(CObjectiveFilter canLootFilter)
	{
		CanLootFilter = canLootFilter;
	}

	public void SetCanLootLocKey(string canLootLocKey)
	{
		CanLootLocKey = canLootLocKey;
	}

	public void SetActorAttachedAtRuntime(CObjectActor actorToAttach)
	{
		RuntimeAttachedActor = actorToAttach;
		PropActorHasBeenAssigned = true;
	}

	public void ClearAttachedRuntimeActor()
	{
		RuntimeAttachedActor = null;
		PropActorHasBeenAssigned = false;
	}

	public void SetNewStartingMapGuid(string startingMapGuid)
	{
		StartingMapGuid = startingMapGuid;
	}

	public void SetOverrideDisallowMoveOrDestroy(bool shouldDisallow)
	{
		OverrideDisallowDestroyAndMove = shouldDisallow;
	}

	public static CObjectObstacle FindPropWithPathingBlocker(Point pathingBlocker, ref CTile propTile)
	{
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int j = 0; j < ScenarioManager.Width; j++)
			{
				CTile cTile = ScenarioManager.Tiles[j, i];
				if (cTile == null)
				{
					continue;
				}
				foreach (CObjectObstacle item in cTile.FindProps(ScenarioManager.ObjectImportType.Obstacle).OfType<CObjectObstacle>())
				{
					foreach (TileIndex pathingBlocker2 in item.PathingBlockers)
					{
						if (pathingBlocker2.X == pathingBlocker.X && pathingBlocker2.Y == pathingBlocker.Y)
						{
							propTile = cTile;
							return item;
						}
					}
				}
			}
		}
		return null;
	}

	public static CInteractableSpawner FindSpawnerWithPathingBlocker(Point pathingBlocker, ref CTile spawnerTile)
	{
		for (int i = 0; i < ScenarioManager.Height; i++)
		{
			for (int j = 0; j < ScenarioManager.Width; j++)
			{
				CTile cTile = ScenarioManager.Tiles[j, i];
				if (cTile == null)
				{
					continue;
				}
				foreach (CInteractableSpawner item in cTile.m_Spawners.OfType<CInteractableSpawner>())
				{
					if (!(item.Prop is CObjectObstacle { PathingBlockers: not null } cObjectObstacle))
					{
						continue;
					}
					foreach (TileIndex pathingBlocker2 in cObjectObstacle.PathingBlockers)
					{
						if (pathingBlocker2.X == pathingBlocker.X && pathingBlocker2.Y == pathingBlocker.Y)
						{
							spawnerTile = cTile;
							return item;
						}
					}
				}
			}
		}
		return null;
	}

	private static Dictionary<int, ScenarioManager.EPerPartySizeConfig> GetDefaultConfigPerPartySize()
	{
		return new Dictionary<int, ScenarioManager.EPerPartySizeConfig>
		{
			{
				1,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				2,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				3,
				ScenarioManager.EPerPartySizeConfig.Normal
			},
			{
				4,
				ScenarioManager.EPerPartySizeConfig.Normal
			}
		};
	}

	public static CTile FindPropTile(string name)
	{
		CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.Props.SingleOrDefault((CObjectProp s) => s.InstanceName == name) ?? ScenarioManager.CurrentScenarioState.ActivatedProps.SingleOrDefault((CObjectProp s) => s.InstanceName == name);
		if (cObjectProp != null)
		{
			return ScenarioManager.Tiles[cObjectProp.ArrayIndex.X, cObjectProp.ArrayIndex.Y];
		}
		return null;
	}

	public static bool IsProp(ScenarioManager.ObjectImportType type)
	{
		if ((uint)(type - 4) <= 6u || type == ScenarioManager.ObjectImportType.CarryableQuestItem || (uint)(type - 22) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static int PropImportanceValue(EPropType type)
	{
		switch (type)
		{
		case EPropType.GoalChest:
		case EPropType.Spawner:
		case EPropType.PressurePlate:
		case EPropType.Portal:
			return 0;
		case EPropType.Chest:
			return 1;
		case EPropType.OneHexObstacle:
		case EPropType.TwoHexObstacle:
		case EPropType.ThreeHexObstacle:
		case EPropType.DarkPitObstacle:
		case EPropType.ThreeHexCurvedObstacle:
		case EPropType.ThreeHexStraightObstacle:
			return 2;
		case EPropType.Trap:
		case EPropType.TerrainHotCoals:
		case EPropType.TerrainWater:
		case EPropType.TerrainRubble:
		case EPropType.TerrainThorns:
			return 3;
		default:
			return 4;
		}
	}

	public static List<Tuple<int, string>> Compare(CObjectProp prop1, CObjectProp prop2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (prop1.ObjectType != prop2.ObjectType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1601, "CObjectProp ObjectType does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName }
				});
			}
			if (prop1.PrefabName != prop2.PrefabName)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1602, "CObjectProp PrefabName does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName }
				});
			}
			if (!TileIndex.Compare(prop1.ArrayIndex, prop2.ArrayIndex))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1603, "CObjectProp ArrayIndex does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"ArrayIndex",
						prop1.ArrayIndex.ToString(),
						prop2.ArrayIndex.ToString()
					}
				});
			}
			if (!CVector3.Compare(prop1.Position, prop2.Position))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1604, "CObjectProp Position does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"Position",
						prop1.Position.ToString(),
						prop2.Position.ToString()
					}
				});
			}
			if (prop1.Activated != prop2.Activated)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1606, "CObjectProp Activated does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"Activated",
						prop1.Activated.ToString(),
						prop2.Activated.ToString()
					}
				});
			}
			if (prop1.m_OwnerGuid != prop2.m_OwnerGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1607, "CObjectProp m_OwnerGuid does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3] { "m_OwnerGuid", prop1.m_OwnerGuid, prop2.m_OwnerGuid }
				});
			}
			if (prop1.StartingMapGuid != prop2.StartingMapGuid)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1608, "CObjectProp StartingMapGuid does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3] { "StartingMapGuid", prop1.StartingMapGuid, prop2.StartingMapGuid }
				});
			}
			if (prop1.ActorActivated != prop2.ActorActivated)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1617, "CObjectProp ActorActivated does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3] { "ActorActivated", prop1.ActorActivated, prop2.ActorActivated }
				});
			}
			if (prop1.IgnoreEndOfTurnLooting != prop2.IgnoreEndOfTurnLooting)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1609, "CObjectProp IgnoreEndOfTurnLooting does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"IgnoreEndOfTurnLooting",
						prop1.IgnoreEndOfTurnLooting.ToString(),
						prop2.IgnoreEndOfTurnLooting.ToString()
					}
				});
			}
			if (prop1.PropActorHasBeenAssigned != prop2.PropActorHasBeenAssigned)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1610, "CObjectProp PropActorHasBeenAssigned does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"PropActorHasBeenAssigned",
						prop1.PropActorHasBeenAssigned.ToString(),
						prop2.PropActorHasBeenAssigned.ToString()
					}
				});
			}
			if (prop1.OverrideDisallowDestroyAndMove != prop2.OverrideDisallowDestroyAndMove)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1611, "CObjectProp OverrideDisallowDestroyAndMove does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"OverrideDisallowDestroyAndMove",
						prop1.OverrideDisallowDestroyAndMove.ToString(),
						prop2.OverrideDisallowDestroyAndMove.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(prop1.ConfigPerPartySize, prop2.ConfigPerPartySize))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1612, "CObjectProp ConfigPerPartySize null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
					new string[3]
					{
						"ObjectType",
						prop1.ObjectType.ToString(),
						prop2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
					new string[3]
					{
						"ConfigPerPartySize",
						(prop1.ConfigPerPartySize == null) ? "is null" : "is not null",
						(prop2.ConfigPerPartySize == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (prop1.ConfigPerPartySize.Count != prop2.ConfigPerPartySize.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1613, "CObjectProp total ConfigPerPartySize Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
						new string[3]
						{
							"ObjectType",
							prop1.ObjectType.ToString(),
							prop2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
						new string[3]
						{
							"ConfigPerPartySize Count",
							prop1.ConfigPerPartySize.Count.ToString(),
							prop2.ConfigPerPartySize.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item in prop1.ConfigPerPartySize)
				{
					if (!prop2.ConfigPerPartySize.ContainsKey(item.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1614, "CObjectProp ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
							new string[3]
							{
								"ObjectType",
								prop1.ObjectType.ToString(),
								prop2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
							new string[3]
							{
								"ConfigPerPartySize Key",
								item.Key.ToString(),
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item2 in prop2.ConfigPerPartySize)
				{
					if (!prop1.ConfigPerPartySize.ContainsKey(item2.Key))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1615, "CObjectProp ConfigPerPartySize in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a key that is in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
							new string[3]
							{
								"ObjectType",
								prop1.ObjectType.ToString(),
								prop2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
							new string[3]
							{
								"ConfigPerPartySize Key",
								"Missing",
								item2.Key.ToString()
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (KeyValuePair<int, ScenarioManager.EPerPartySizeConfig> item3 in prop1.ConfigPerPartySize)
				{
					if (item3.Value != prop2.ConfigPerPartySize[item3.Key])
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1616, "CObjectProp ConfigPerPartySize has key with differing values.", new List<string[]>
						{
							new string[3] { "Prop GUID", prop1.PropGuid, prop2.PropGuid },
							new string[3]
							{
								"ObjectType",
								prop1.ObjectType.ToString(),
								prop2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", prop1.PrefabName, prop2.PrefabName },
							new string[3]
							{
								"ConfigPerPartySize Key",
								item3.Key.ToString(),
								item3.Key.ToString()
							},
							new string[3]
							{
								"ConfigPerPartySize Value",
								item3.Value.ToString(),
								prop2.ConfigPerPartySize[item3.Key].ToString()
							}
						});
						flag = true;
					}
				}
				break;
			}
			}
			list.AddRange(PropHealthDetails.Compare(prop1.PropHealthDetails, prop2.PropHealthDetails, isMPCompare));
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(1699, "Exception during CObjectProp compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
