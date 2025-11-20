using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CInteractableSpawner : CSpawner
{
	public bool Deactivated { get; private set; }

	public string SpawnerPropType { get; private set; }

	public CInteractableSpawner()
	{
	}

	public CInteractableSpawner(CInteractableSpawner state, ReferenceDictionary references)
		: base(state, references)
	{
		Deactivated = state.Deactivated;
		SpawnerPropType = state.SpawnerPropType;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Deactivated", Deactivated);
		info.AddValue("SpawnerPropType", SpawnerPropType);
	}

	public CInteractableSpawner(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "Deactivated"))
				{
					if (name == "SpawnerPropType")
					{
						SpawnerPropType = info.GetString("SpawnerPropType");
					}
				}
				else
				{
					Deactivated = info.GetBoolean("Deactivated");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CInteractableSpawner entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CInteractableSpawner(string spawnerPropType, SpawnerData spawnerData, TileIndex arrayIndex, string startingMapGuid, string spawnerGuid = null, SpawnerData.ESpawnerEntryDifficulty spawnerEntryDifficulty = SpawnerData.ESpawnerEntryDifficulty.Default)
		: base(spawnerData, arrayIndex, startingMapGuid, spawnerGuid, spawnerEntryDifficulty)
	{
		SpawnerPropType = spawnerPropType;
		Deactivated = false;
	}

	public void CreateSpawnerProp(List<TileIndex> pathingBlockers = null, CVector3 rotation = null)
	{
		if (base.Prop == null)
		{
			base.Prop = new CObjectObstacle(SpawnerPropType, ScenarioManager.ObjectImportType.Spawner, base.ArrayIndex, null, rotation, pathingBlockers, null, base.StartingMapGuid, ignoresFlyAndJump: false, pathingBlockers != null && pathingBlockers.Count > 1);
			CSpawn_MessageData message = new CSpawn_MessageData(null)
			{
				m_SpawnDelay = 0f,
				m_Prop = base.Prop
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public override void HideSpawnerProp()
	{
	}

	public override void SetActive(bool active, bool forceDontSpawn = false)
	{
		if (!Deactivated)
		{
			base.SetActive(active, forceDontSpawn);
		}
	}

	public void Deactivate()
	{
		if (!Deactivated)
		{
			SetActive(active: false);
			Deactivated = true;
			CDeactivatePropAnim_MessageData cDeactivatePropAnim_MessageData = new CDeactivatePropAnim_MessageData(null);
			cDeactivatePropAnim_MessageData.m_Prop = base.Prop;
			ScenarioRuleClient.MessageHandler(cDeactivatePropAnim_MessageData);
		}
	}
}
