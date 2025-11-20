using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectPressurePlate : CObjectProp, ISerializable
{
	public enum EPressurePlateType
	{
		None,
		ActivateOnce,
		ActivateWhileOccupied
	}

	public static EPressurePlateType[] PressurePlateTypes = (EPressurePlateType[])Enum.GetValues(typeof(EPressurePlateType));

	private List<string> m_LegacyDoorGuids;

	public EPressurePlateType PressurePlateType { get; private set; }

	public List<string> LinkedDoorGuids { get; private set; }

	public List<string> LinkedSpawnerGuids { get; private set; }

	public bool PressurePlateLocked { get; private set; }

	public CObjectPressurePlate()
	{
	}

	public CObjectPressurePlate(CObjectPressurePlate state, ReferenceDictionary references)
		: base(state, references)
	{
		PressurePlateType = state.PressurePlateType;
		LinkedDoorGuids = references.Get(state.LinkedDoorGuids);
		if (LinkedDoorGuids == null && state.LinkedDoorGuids != null)
		{
			LinkedDoorGuids = new List<string>();
			for (int i = 0; i < state.LinkedDoorGuids.Count; i++)
			{
				string item = state.LinkedDoorGuids[i];
				LinkedDoorGuids.Add(item);
			}
			references.Add(state.LinkedDoorGuids, LinkedDoorGuids);
		}
		LinkedSpawnerGuids = references.Get(state.LinkedSpawnerGuids);
		if (LinkedSpawnerGuids == null && state.LinkedSpawnerGuids != null)
		{
			LinkedSpawnerGuids = new List<string>();
			for (int j = 0; j < state.LinkedSpawnerGuids.Count; j++)
			{
				string item2 = state.LinkedSpawnerGuids[j];
				LinkedSpawnerGuids.Add(item2);
			}
			references.Add(state.LinkedSpawnerGuids, LinkedSpawnerGuids);
		}
		PressurePlateLocked = state.PressurePlateLocked;
		m_LegacyDoorGuids = references.Get(state.m_LegacyDoorGuids);
		if (m_LegacyDoorGuids == null && state.m_LegacyDoorGuids != null)
		{
			m_LegacyDoorGuids = new List<string>();
			for (int k = 0; k < state.m_LegacyDoorGuids.Count; k++)
			{
				string item3 = state.m_LegacyDoorGuids[k];
				m_LegacyDoorGuids.Add(item3);
			}
			references.Add(state.m_LegacyDoorGuids, m_LegacyDoorGuids);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PressurePlateType", PressurePlateType);
		info.AddValue("LinkedDoorGuids", LinkedDoorGuids);
		info.AddValue("LinkedSpawnerGuids", LinkedSpawnerGuids);
		info.AddValue("PressurePlateLocked", PressurePlateLocked);
	}

	public CObjectPressurePlate(SerializationInfo info, StreamingContext context)
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
				case "PressurePlateType":
					PressurePlateType = (EPressurePlateType)info.GetValue("PressurePlateType", typeof(EPressurePlateType));
					break;
				case "LinkedDoorGuids":
					LinkedDoorGuids = (List<string>)info.GetValue("LinkedDoorGuids", typeof(List<string>));
					break;
				case "LinkedSpawnerGuids":
					LinkedSpawnerGuids = (List<string>)info.GetValue("LinkedSpawnerGuids", typeof(List<string>));
					break;
				case "PressurePlateLocked":
					PressurePlateLocked = info.GetBoolean("PressurePlateLocked");
					break;
				case "LinkedDoor":
				{
					CObjectDoor cObjectDoor = (CObjectDoor)info.GetValue("LinkedDoor", typeof(CObjectDoor));
					if (cObjectDoor != null)
					{
						if (m_LegacyDoorGuids == null)
						{
							m_LegacyDoorGuids = new List<string>();
						}
						if (!m_LegacyDoorGuids.Contains(cObjectDoor.PropGuid))
						{
							m_LegacyDoorGuids.Add(cObjectDoor.PropGuid);
						}
					}
					break;
				}
				case "LinkedDoorGuid":
				{
					string item = info.GetString("LinkedDoorGuid");
					if (m_LegacyDoorGuids == null)
					{
						m_LegacyDoorGuids = new List<string>();
					}
					if (!m_LegacyDoorGuids.Contains(item))
					{
						m_LegacyDoorGuids.Add(item);
					}
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectPressurePlate entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	protected override void InternalOnDeserialized()
	{
		base.InternalOnDeserialized();
		if (LinkedDoorGuids == null)
		{
			LinkedDoorGuids = new List<string>();
		}
		if (LinkedSpawnerGuids == null)
		{
			LinkedSpawnerGuids = new List<string>();
		}
		if (m_LegacyDoorGuids != null)
		{
			for (int i = 0; i < m_LegacyDoorGuids.Count; i++)
			{
				if (!LinkedDoorGuids.Contains(m_LegacyDoorGuids[i]))
				{
					LinkedDoorGuids.Add(m_LegacyDoorGuids[i]);
				}
			}
		}
		LinkedDoorGuids.RemoveAll((string d) => d == null);
	}

	public CObjectPressurePlate(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
		LinkedDoorGuids = new List<string>();
		LinkedSpawnerGuids = new List<string>();
		PressurePlateType = EPressurePlateType.ActivateOnce;
	}

	public CObjectPressurePlate(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, string mapGuid)
		: base(name, type, arrayIndex, null, null, null, mapGuid)
	{
		LinkedDoorGuids = new List<string>();
		LinkedSpawnerGuids = new List<string>();
		PressurePlateType = EPressurePlateType.ActivateOnce;
	}

	public void LinkDoor(CObjectDoor doorProp, bool updateDoorLockApparance = true)
	{
		if (LinkedDoorGuids == null)
		{
			LinkedDoorGuids = new List<string>();
		}
		if (!LinkedDoorGuids.Contains(doorProp.PropGuid))
		{
			LinkedDoorGuids.Add(doorProp.PropGuid);
			((CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp y) => y.PropGuid == doorProp.PropGuid)).LinkPressurePlate(this, updateDoorLockApparance);
		}
	}

	public void UnlinkDoor(CObjectDoor doorProp, bool updateDoorLockApparance = true)
	{
		if (LinkedDoorGuids == null)
		{
			LinkedDoorGuids = new List<string>();
		}
		if (LinkedDoorGuids.Contains(doorProp.PropGuid))
		{
			LinkedDoorGuids.Remove(doorProp.PropGuid);
			((CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp y) => y.PropGuid == doorProp.PropGuid)).UnlinkPressurePlate(this, updateDoorLockApparance);
		}
	}

	public void LinkSpawner(CSpawner spawner)
	{
		if (!LinkedSpawnerGuids.Contains(spawner.SpawnerGuid))
		{
			LinkedSpawnerGuids.Add(spawner.SpawnerGuid);
		}
	}

	public void UnlinkSpawner(CSpawner spawner)
	{
		if (LinkedSpawnerGuids.Contains(spawner.SpawnerGuid))
		{
			LinkedSpawnerGuids.Remove(spawner.SpawnerGuid);
		}
	}

	public override bool Activate(CActor actor, CActor creditActor = null)
	{
		if (!base.Activated)
		{
			base.Activated = true;
			base.ActorActivated = ((actor == null) ? string.Empty : actor.ActorGuid);
			CActivateProp_MessageData message = new CActivateProp_MessageData(actor)
			{
				m_Prop = this,
				m_InitialLoad = false
			};
			ScenarioRuleClient.MessageHandler(message);
			int i;
			for (i = 0; i < LinkedDoorGuids.Count; i++)
			{
				CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp y) => y.PropGuid == LinkedDoorGuids[i]);
				if (cObjectDoor == null || cObjectDoor.DoorIsLocked)
				{
					continue;
				}
				cObjectDoor.Activate(actor);
				if (PressurePlateType != EPressurePlateType.ActivateOnce)
				{
					continue;
				}
				foreach (CObjectPressurePlate linkedPressurePlate in cObjectDoor.LinkedPressurePlates)
				{
					linkedPressurePlate.PressurePlateLocked = true;
				}
			}
			int i2;
			for (i2 = 0; i2 < LinkedSpawnerGuids.Count; i2++)
			{
				CSpawner cSpawner = ScenarioManager.CurrentScenarioState.Spawners.SingleOrDefault((CSpawner y) => y.SpawnerGuid == LinkedSpawnerGuids[i2]);
				if (!cSpawner.IsActive && cSpawner.StartingMap.Revealed && cSpawner.SpawnerData.SpawnerActivationType == SpawnerData.ESpawnerActivationType.PressurePlateTriggered)
				{
					cSpawner.SetActive(active: true);
					cSpawner.OnPressurePlateTriggered(ScenarioManager.CurrentScenarioState.Players.Count);
				}
			}
		}
		return false;
	}

	public override bool Deactivate()
	{
		if (base.Activated && !PressurePlateLocked)
		{
			base.Activated = false;
			base.ActorActivated = string.Empty;
			CDeactivatePropAnim_MessageData cDeactivatePropAnim_MessageData = new CDeactivatePropAnim_MessageData(null);
			cDeactivatePropAnim_MessageData.m_Prop = this;
			ScenarioRuleClient.MessageHandler(cDeactivatePropAnim_MessageData);
			if (PressurePlateType == EPressurePlateType.ActivateWhileOccupied)
			{
				int i;
				for (i = 0; i < LinkedDoorGuids.Count; i++)
				{
					CObjectDoor cObjectDoor = (CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps.SingleOrDefault((CObjectProp y) => y.PropGuid == LinkedDoorGuids[i]);
					if (cObjectDoor == null)
					{
						continue;
					}
					bool flag = true;
					foreach (CObjectPressurePlate item in cObjectDoor.LinkedPressurePlates.Where((CObjectPressurePlate x) => x.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 1) != ScenarioManager.EPerPartySizeConfig.Hidden))
					{
						if (item.Activated)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						cObjectDoor.CloseOpenedDoor();
					}
				}
			}
			return true;
		}
		return false;
	}

	public void SetPressurePlateType(EPressurePlateType typeToSet)
	{
		PressurePlateType = typeToSet;
	}

	public static List<Tuple<int, string>> Compare(CObjectPressurePlate plate1, CObjectPressurePlate plate2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(plate1, plate2, isMPCompare));
			if (plate1.PressurePlateType != plate2.PressurePlateType)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 901, "CObjectPressurePlate PressurePlateType does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", plate1.PropGuid, plate2.PropGuid },
					new string[3]
					{
						"ObjectType",
						plate1.ObjectType.ToString(),
						plate2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", plate1.PrefabName, plate2.PrefabName },
					new string[3]
					{
						"PressurePlateType",
						plate1.PressurePlateType.ToString(),
						plate2.PressurePlateType.ToString()
					}
				});
			}
			if (plate1.PressurePlateLocked != plate2.PressurePlateLocked)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 902, "CObjectPressurePlate PressurePlateLocked does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", plate1.PropGuid, plate2.PropGuid },
					new string[3]
					{
						"ObjectType",
						plate1.ObjectType.ToString(),
						plate2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", plate1.PrefabName, plate2.PrefabName },
					new string[3]
					{
						"PressurePlateLocked",
						plate1.PressurePlateLocked.ToString(),
						plate2.PressurePlateLocked.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(plate1.LinkedDoorGuids, plate2.LinkedDoorGuids))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 910, "CObjectPressurePlate LinkedDoorGuids null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", plate1.PropGuid, plate2.PropGuid },
					new string[3]
					{
						"ObjectType",
						plate1.ObjectType.ToString(),
						plate2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", plate1.PrefabName, plate2.PrefabName },
					new string[3]
					{
						"LinkedDoorGuids",
						(plate1.LinkedDoorGuids == null) ? "Is Null" : "Is Not Null",
						(plate2.LinkedDoorGuids == null) ? "Is Null" : "Is Not Null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (plate1.LinkedDoorGuids.Count != plate2.LinkedDoorGuids.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 911, "CObjectPressurePlate total LinkedDoorGuids Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", plate1.PropGuid, plate2.PropGuid },
						new string[3]
						{
							"ObjectType",
							plate1.ObjectType.ToString(),
							plate2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", plate1.PrefabName, plate2.PrefabName },
						new string[3]
						{
							"LinkedDoorGuids Count",
							plate1.LinkedDoorGuids.Count.ToString(),
							plate2.LinkedDoorGuids.Count.ToString()
						}
					});
					break;
				}
				for (int i = 0; i < plate1.LinkedDoorGuids.Count; i++)
				{
					if (plate1.LinkedDoorGuids[i] != plate2.LinkedDoorGuids[i])
					{
						ScenarioState.LogMismatch(list, isMPCompare, 912, $"CObjectPressurePlate LinkedDoorGuids entry number {i} does not match.", new List<string[]>
						{
							new string[3] { "Prop GUID", plate1.PropGuid, plate2.PropGuid },
							new string[3]
							{
								"ObjectType",
								plate1.ObjectType.ToString(),
								plate2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", plate1.PrefabName, plate2.PrefabName },
							new string[3]
							{
								"LinkedDoorGuids Entry Value",
								plate1.LinkedDoorGuids[i],
								plate2.LinkedDoorGuids[i]
							}
						});
					}
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(999, "Exception during CObjectPressurePlate compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
