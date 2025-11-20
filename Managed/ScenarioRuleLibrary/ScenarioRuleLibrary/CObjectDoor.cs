using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using AStar;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectDoor : CObjectProp, ISerializable
{
	[Serializable]
	public enum EDoorType
	{
		ThinDoor,
		ThickDoor,
		HexOpeningDoor,
		ThinNarrowDoor,
		ThickNarrowDoor
	}

	[Serializable]
	public enum ELockType
	{
		None,
		Pressure,
		Gold,
		Kill,
		Other
	}

	public enum EDoorPressurePlateLockType
	{
		AllActivatedToOpen,
		AnyActivatedToOpen
	}

	public static EDoorType[] DoorTypes = (EDoorType[])Enum.GetValues(typeof(EDoorType));

	public static ELockType[] LockTypes = (ELockType[])Enum.GetValues(typeof(ELockType));

	public static EDoorPressurePlateLockType[] DoorPressurePlateLockTypes = (EDoorPressurePlateLockType[])Enum.GetValues(typeof(EDoorPressurePlateLockType));

	private bool m_ForceActivate;

	public EDoorType DoorType { get; private set; }

	public ELockType LockType { get; private set; }

	public EDoorPressurePlateLockType DoorPressurePlateLockType { get; private set; }

	public bool DoorHasExtraLock { get; private set; }

	public bool DoorExtraLockIsLocked { get; private set; }

	public bool IsDungeonEntrance { get; private set; }

	public bool IsDungeonExit { get; private set; }

	public List<CObjectPressurePlate> LinkedPressurePlates { get; private set; }

	public bool DoorOpenedByMovingActor { get; private set; }

	public List<CMap> RoomsRevealedInLastOpening { get; private set; }

	public float Angle => base.Rotation.Y;

	public bool DoorIsLocked
	{
		get
		{
			bool flag = false;
			if (DoorHasPressurePlateLock)
			{
				switch (DoorPressurePlateLockType)
				{
				case EDoorPressurePlateLockType.AllActivatedToOpen:
					flag = LinkedPressurePlates.Where((CObjectPressurePlate p) => p.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 1) != ScenarioManager.EPerPartySizeConfig.Hidden).Any((CObjectPressurePlate p) => !p.Activated);
					break;
				case EDoorPressurePlateLockType.AnyActivatedToOpen:
					flag = !LinkedPressurePlates.Where((CObjectPressurePlate p) => p.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 1) != ScenarioManager.EPerPartySizeConfig.Hidden).Any((CObjectPressurePlate p) => p.Activated);
					break;
				}
			}
			if (!flag && (!DoorHasExtraLock || !DoorExtraLockIsLocked) && !HexMap.Destroyed)
			{
				if (Hex2Map != null)
				{
					return Hex2Map.Destroyed;
				}
				return false;
			}
			return true;
		}
	}

	public bool DoorHasPressurePlateLock
	{
		get
		{
			if (LinkedPressurePlates != null)
			{
				return LinkedPressurePlates.Count > 0;
			}
			return false;
		}
	}

	public bool DoorIsOpen => ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].IsBridgeOpen;

	public CMap HexMap => ScenarioManager.Tiles[base.ArrayIndex.X, base.ArrayIndex.Y].m_HexMap;

	public CMap Hex2Map => ScenarioManager.Tiles[base.ArrayIndex.X, base.ArrayIndex.Y].m_Hex2Map;

	public CObjectDoor()
	{
	}

	public CObjectDoor(CObjectDoor state, ReferenceDictionary references)
		: base(state, references)
	{
		DoorType = state.DoorType;
		LockType = state.LockType;
		DoorPressurePlateLockType = state.DoorPressurePlateLockType;
		DoorHasExtraLock = state.DoorHasExtraLock;
		DoorExtraLockIsLocked = state.DoorExtraLockIsLocked;
		IsDungeonEntrance = state.IsDungeonEntrance;
		IsDungeonExit = state.IsDungeonExit;
		LinkedPressurePlates = references.Get(state.LinkedPressurePlates);
		if (LinkedPressurePlates == null && state.LinkedPressurePlates != null)
		{
			LinkedPressurePlates = new List<CObjectPressurePlate>();
			for (int i = 0; i < state.LinkedPressurePlates.Count; i++)
			{
				CObjectPressurePlate cObjectPressurePlate = state.LinkedPressurePlates[i];
				CObjectPressurePlate cObjectPressurePlate2 = references.Get(cObjectPressurePlate);
				if (cObjectPressurePlate2 == null && cObjectPressurePlate != null)
				{
					cObjectPressurePlate2 = new CObjectPressurePlate(cObjectPressurePlate, references);
					references.Add(cObjectPressurePlate, cObjectPressurePlate2);
				}
				LinkedPressurePlates.Add(cObjectPressurePlate2);
			}
			references.Add(state.LinkedPressurePlates, LinkedPressurePlates);
		}
		DoorOpenedByMovingActor = state.DoorOpenedByMovingActor;
		RoomsRevealedInLastOpening = references.Get(state.RoomsRevealedInLastOpening);
		if (RoomsRevealedInLastOpening == null && state.RoomsRevealedInLastOpening != null)
		{
			RoomsRevealedInLastOpening = new List<CMap>();
			for (int j = 0; j < state.RoomsRevealedInLastOpening.Count; j++)
			{
				CMap cMap = state.RoomsRevealedInLastOpening[j];
				CMap cMap2 = references.Get(cMap);
				if (cMap2 == null && cMap != null)
				{
					cMap2 = new CMap(cMap, references);
					references.Add(cMap, cMap2);
				}
				RoomsRevealedInLastOpening.Add(cMap2);
			}
			references.Add(state.RoomsRevealedInLastOpening, RoomsRevealedInLastOpening);
		}
		m_ForceActivate = state.m_ForceActivate;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("DoorType", DoorType);
		info.AddValue("LockType", LockType);
		info.AddValue("DoorPressurePlateLockType", DoorPressurePlateLockType);
		info.AddValue("DoorHasExtraLock", DoorHasExtraLock);
		info.AddValue("DoorExtraLockIsLocked", DoorExtraLockIsLocked);
		info.AddValue("IsDungeonEntrance", IsDungeonEntrance);
		info.AddValue("IsDungeonExit", IsDungeonExit);
		info.AddValue("LinkedPressurePlates", LinkedPressurePlates);
	}

	public CObjectDoor(SerializationInfo info, StreamingContext context)
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
				case "DoorType":
					DoorType = (EDoorType)info.GetValue("DoorType", typeof(EDoorType));
					break;
				case "LockType":
					LockType = (ELockType)info.GetValue("LockType", typeof(ELockType));
					break;
				case "DoorPressurePlateLockType":
					DoorPressurePlateLockType = (EDoorPressurePlateLockType)info.GetValue("DoorPressurePlateLockType", typeof(EDoorPressurePlateLockType));
					break;
				case "DoorHasExtraLock":
					DoorHasExtraLock = info.GetBoolean("DoorHasExtraLock");
					break;
				case "DoorExtraLockIsLocked":
					DoorExtraLockIsLocked = info.GetBoolean("DoorExtraLockIsLocked");
					break;
				case "IsDungeonEntrance":
					IsDungeonEntrance = info.GetBoolean("IsDungeonEntrance");
					break;
				case "IsDungeonExit":
					IsDungeonExit = info.GetBoolean("IsDungeonExit");
					break;
				case "LinkedPressurePlates":
					LinkedPressurePlates = (List<CObjectPressurePlate>)info.GetValue("LinkedPressurePlates", typeof(List<CObjectPressurePlate>));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectDoor entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal new void OnDeserialized(StreamingContext context)
	{
		if (LinkedPressurePlates == null)
		{
			LinkedPressurePlates = new List<CObjectPressurePlate>();
		}
	}

	public CObjectDoor(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, string mapGuid, EDoorType doorType, bool isDungeonEntrance, bool isDungeonExit)
		: base(name, type, arrayIndex, position, rotation, null, mapGuid)
	{
		DoorType = doorType;
		LockType = ELockType.None;
		DoorPressurePlateLockType = EDoorPressurePlateLockType.AllActivatedToOpen;
		IsDungeonEntrance = isDungeonEntrance;
		IsDungeonExit = isDungeonExit;
		LinkedPressurePlates = new List<CObjectPressurePlate>();
	}

	public void InitDoor()
	{
		ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].Walkable = true;
		ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].IsBridge = true;
		ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].IsBridgeOpen = false;
		ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].HasHealth = base.PropHealthDetails != null && base.PropHealthDetails.HasHealth;
	}

	public void LinkPressurePlate(CObjectPressurePlate pressurePlate, bool updateApparanceLockType = true)
	{
		LinkedPressurePlates.Add(pressurePlate);
		if (updateApparanceLockType)
		{
			SetDoorApparanceLockType(ELockType.Pressure);
		}
	}

	public void UnlinkPressurePlate(CObjectPressurePlate pressurePlate, bool updateApparanceLockType = true)
	{
		LinkedPressurePlates.Remove(pressurePlate);
		if (updateApparanceLockType)
		{
			SetDoorApparanceLockType(DoorHasPressurePlateLock ? ELockType.Pressure : ELockType.None);
		}
	}

	public void SetDoorApparanceLockType(ELockType lockTypeToSet)
	{
		LockType = lockTypeToSet;
	}

	public void SetDoorPressurePlateLockType(EDoorPressurePlateLockType lockTypeToSet)
	{
		DoorPressurePlateLockType = lockTypeToSet;
	}

	public void SetHasExtraLock(bool hasExtraLock)
	{
		DoorHasExtraLock = hasExtraLock;
		SetExtraLockState(hasExtraLock, openDoorIfUnlocked: false);
	}

	public void SetExtraLockState(bool lockedStateToSet, bool openDoorIfUnlocked = true, CActor actorActivating = null)
	{
		DoorExtraLockIsLocked = lockedStateToSet;
		if (openDoorIfUnlocked && !DoorIsLocked)
		{
			ForceActivate(actorActivating);
		}
	}

	public void SetDoorType(EDoorType typeToSet)
	{
		DoorType = typeToSet;
	}

	public bool ForceActivate(CActor actor)
	{
		m_ForceActivate = true;
		return Activate(actor);
	}

	public void SetActivatedFromLevelEditor(bool activate)
	{
		base.Activated = activate;
	}

	public bool SetDoorOpenedByMovingActor(CActor actor)
	{
		DoorOpenedByMovingActor = true;
		return Activate(actor);
	}

	public bool IsConnectedToMap(CMap checkMap)
	{
		if (HexMap != checkMap)
		{
			if (Hex2Map != null)
			{
				return Hex2Map == checkMap;
			}
			return false;
		}
		return true;
	}

	public bool IsConnectedToDestroyedMap()
	{
		if (!HexMap.Destroyed)
		{
			if (Hex2Map != null)
			{
				return Hex2Map.Destroyed;
			}
			return false;
		}
		return true;
	}

	public override bool Activate(CActor actor, CActor creditActor = null)
	{
		if (!base.Activated && (m_ForceActivate || actor != null))
		{
			if (RoomsRevealedInLastOpening == null)
			{
				RoomsRevealedInLastOpening = new List<CMap>();
			}
			else
			{
				RoomsRevealedInLastOpening.Clear();
			}
			base.ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			if (actor != null)
			{
				CPauseLoco_MessageData cPauseLoco_MessageData = new CPauseLoco_MessageData(actor);
				cPauseLoco_MessageData.m_Pause = true;
				ScenarioRuleClient.MessageHandler(cPauseLoco_MessageData);
			}
			string objectActor = "";
			if (actor != null)
			{
				if (actor is CPlayerActor cPlayerActor)
				{
					objectActor = cPlayerActor.CharacterClass.ID;
				}
				if (actor is CHeroSummonActor cHeroSummonActor)
				{
					objectActor = cHeroSummonActor.Summoner?.CharacterClass.ID;
				}
			}
			if (m_ForceActivate)
			{
				m_ForceActivate = false;
			}
			ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].IsBridgeOpen = true;
			CTile cTile = ScenarioManager.Tiles[base.ArrayIndex.X, base.ArrayIndex.Y];
			SimpleLog.AddToSimpleLog(actor?.ActorLocKey() ?? "Scenario opens door");
			if (!cTile.m_HexMap.Revealed)
			{
				RoomsRevealedInLastOpening.Add(cTile.m_HexMap);
			}
			cTile.m_HexMap.Reveal(initial: false, noIDRegen: false, forLevelEditor: false, DoorOpenedByMovingActor);
			if (cTile.m_Hex2Map != null)
			{
				if (!cTile.m_Hex2Map.Revealed)
				{
					RoomsRevealedInLastOpening.Add(cTile.m_Hex2Map);
				}
				cTile.m_Hex2Map.Reveal(initial: false, noIDRegen: false, forLevelEditor: false, DoorOpenedByMovingActor);
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventObjectProp(ESESubTypeObjectProp.Activated, base.ObjectType, base.PrefabName, objectActor, "Activate", m_OwnerGuid, "", RoomsRevealedInLastOpening));
			GameState.SortIntoInitiativeAndIDOrder();
			base.Activate(actor);
		}
		return false;
	}

	public void UnlockLockedDoorWithoutOpening(CActor actorUnlocking)
	{
		if (DoorHasExtraLock && DoorExtraLockIsLocked)
		{
			DoorExtraLockIsLocked = false;
			CUnlockLockedDoor_MessageData message = new CUnlockLockedDoor_MessageData(actorUnlocking)
			{
				m_Prop = this,
				m_InitialLoad = false
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public void CloseOpenedDoor(bool lockDoor = false)
	{
		if (!base.Activated)
		{
			return;
		}
		CActor cActor = ScenarioManager.Scenario.FindActorAt(new Point(base.ArrayIndex));
		if (cActor != null)
		{
			GameState.ActorBeenDamaged(cActor, ScenarioManager.Scenario.SLTE.TrapDamage, checkIfPlayerCanAvoidDamage: true, null, null, CAbility.EAbilityType.Trap, 0, isTrapDamage: true);
			GameState.ActorHealthCheck(cActor, cActor, isTrap: true);
		}
		CTile teleportTile = ScenarioManager.Tiles[base.ArrayIndex.X, base.ArrayIndex.Y];
		CTile refTile = null;
		CAbilityTeleport.EnsureTileIsClearForTeleport(teleportTile, ref refTile, null, "Hit");
		ScenarioManager.PathFinder.Nodes[base.ArrayIndex.X, base.ArrayIndex.Y].IsBridgeOpen = false;
		SimpleLog.AddToSimpleLog("Scenario closes door");
		CDeactivatePropAnim_MessageData cDeactivatePropAnim_MessageData = new CDeactivatePropAnim_MessageData(null);
		cDeactivatePropAnim_MessageData.m_Prop = this;
		ScenarioRuleClient.MessageHandler(cDeactivatePropAnim_MessageData);
		base.Activated = false;
		if (lockDoor)
		{
			if (!DoorHasExtraLock)
			{
				SetHasExtraLock(hasExtraLock: true);
			}
			else
			{
				SetExtraLockState(lockedStateToSet: true);
			}
		}
	}

	public static EDoorType[] SwappableTypes(EDoorType type)
	{
		switch (type)
		{
		case EDoorType.ThinDoor:
		case EDoorType.ThinNarrowDoor:
			return new EDoorType[2]
			{
				EDoorType.ThinDoor,
				EDoorType.ThinNarrowDoor
			};
		case EDoorType.ThickDoor:
		case EDoorType.ThickNarrowDoor:
			return new EDoorType[2]
			{
				EDoorType.ThickDoor,
				EDoorType.ThickNarrowDoor
			};
		default:
			return new EDoorType[0];
		}
	}

	public static List<Tuple<int, string>> Compare(CObjectDoor door1, CObjectDoor door2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(door1, door2, isMPCompare));
			if (door1.DoorType != door2.DoorType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1801, "CObjectDoor DoorType does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"DoorType",
						door1.DoorType.ToString(),
						door2.DoorType.ToString()
					}
				});
			}
			if (door1.LockType != door2.LockType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1802, "CObjectDoor LockType does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"LockType",
						door1.LockType.ToString(),
						door2.LockType.ToString()
					}
				});
			}
			if (door1.DoorHasExtraLock != door2.DoorHasExtraLock)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1803, "CObjectDoor DoorHasExtraLock does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"DoorHasExtraLock",
						door1.DoorHasExtraLock.ToString(),
						door2.DoorHasExtraLock.ToString()
					}
				});
			}
			if (door1.DoorExtraLockIsLocked != door2.DoorExtraLockIsLocked)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1804, "CObjectDoor DoorExtraLockIsLocked does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"DoorExtraLockIsLocked",
						door1.DoorExtraLockIsLocked.ToString(),
						door2.DoorExtraLockIsLocked.ToString()
					}
				});
			}
			if (door1.IsDungeonEntrance != door2.IsDungeonEntrance)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1805, "CObjectDoor IsDungeonEntrance does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"IsDungeonEntrance",
						door1.IsDungeonEntrance.ToString(),
						door2.IsDungeonEntrance.ToString()
					}
				});
			}
			if (door1.IsDungeonExit != door2.IsDungeonExit)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1806, "CObjectDoor IsDungeonExit does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"IsDungeonExit",
						door1.IsDungeonExit.ToString(),
						door2.IsDungeonExit.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(door1.LinkedPressurePlates, door2.LinkedPressurePlates))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1807, "CObjectDoor LinkedPressurePlates Null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
					new string[3]
					{
						"ObjectType",
						door1.ObjectType.ToString(),
						door2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
					new string[3]
					{
						"ChestTreasureTablesID",
						(door1.LinkedPressurePlates == null) ? "is null" : "is not null",
						(door2.LinkedPressurePlates == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (door1.LinkedPressurePlates.Count != door2.LinkedPressurePlates.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1808, "CObjectDoor LinkedPressurePlates Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
						new string[3]
						{
							"ObjectType",
							door1.ObjectType.ToString(),
							door2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
						new string[3]
						{
							"ChestTreasureTablesID Count",
							door1.LinkedPressurePlates.Count.ToString(),
							door2.LinkedPressurePlates.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (CObjectPressurePlate pressurePlate in door1.LinkedPressurePlates)
				{
					if (!door2.LinkedPressurePlates.Exists((CObjectPressurePlate e) => e.PropGuid == pressurePlate.PropGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1809, "CObjectDoor LinkedPressurePlate in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
							new string[3]
							{
								"ObjectType",
								door1.ObjectType.ToString(),
								door2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
							new string[3] { "PressurePlate Prop GUID", pressurePlate.PropGuid, "Missing" },
							new string[3] { "PressurePlate PrefabName", pressurePlate.PrefabName, "Missing" },
							new string[3]
							{
								"PressurePlateType",
								pressurePlate.PressurePlateType.ToString(),
								"Missing"
							}
						});
						flag = true;
					}
				}
				foreach (CObjectPressurePlate pressurePlate2 in door2.LinkedPressurePlates)
				{
					if (!door1.LinkedPressurePlates.Exists((CObjectPressurePlate e) => e.PropGuid == pressurePlate2.PropGuid))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1810, "CObjectDoor LinkedPressurePlate in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", door1.PropGuid, door2.PropGuid },
							new string[3]
							{
								"ObjectType",
								door1.ObjectType.ToString(),
								door2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", door1.PrefabName, door2.PrefabName },
							new string[3] { "PressurePlate Prop GUID", "Missing", pressurePlate2.PropGuid },
							new string[3] { "PressurePlate PrefabName", "Missing", pressurePlate2.PrefabName },
							new string[3]
							{
								"PressurePlateType",
								"Missing",
								pressurePlate2.PressurePlateType.ToString()
							}
						});
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
				foreach (CObjectPressurePlate pressurePlate3 in door1.LinkedPressurePlates)
				{
					try
					{
						CObjectPressurePlate plate = door2.LinkedPressurePlates.Single((CObjectPressurePlate s) => s.PropGuid == pressurePlate3.PropGuid);
						list.AddRange(CObjectPressurePlate.Compare(pressurePlate3, plate, isMPCompare));
					}
					catch (Exception ex)
					{
						list.Add(new Tuple<int, string>(1811, "Exception during LinkedPressurePlate compare.\n" + ex.Message + "\n" + ex.StackTrace));
					}
				}
				break;
			}
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(1899, "Exception during CObjectDoor compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}
}
