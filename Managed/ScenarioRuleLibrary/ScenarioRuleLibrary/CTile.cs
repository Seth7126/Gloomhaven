using System.Collections.Generic;
using System.Linq;
using AStar;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CTile
{
	public CMap m_HexMap;

	public CMapTile m_Hex;

	public CMap m_Hex2Map;

	public CMapTile m_Hex2;

	public Point m_ArrayIndex;

	public List<CObjectProp> m_Props = new List<CObjectProp>();

	public List<CSpawner> m_Spawners = new List<CSpawner>();

	public CTile()
	{
	}

	public CTile(CTile state, ReferenceDictionary references)
	{
		m_HexMap = references.Get(state.m_HexMap);
		if (m_HexMap == null && state.m_HexMap != null)
		{
			m_HexMap = new CMap(state.m_HexMap, references);
			references.Add(state.m_HexMap, m_HexMap);
		}
		m_Hex = references.Get(state.m_Hex);
		if (m_Hex == null && state.m_Hex != null)
		{
			m_Hex = new CMapTile(state.m_Hex, references);
			references.Add(state.m_Hex, m_Hex);
		}
		m_Hex2Map = references.Get(state.m_Hex2Map);
		if (m_Hex2Map == null && state.m_Hex2Map != null)
		{
			m_Hex2Map = new CMap(state.m_Hex2Map, references);
			references.Add(state.m_Hex2Map, m_Hex2Map);
		}
		m_Hex2 = references.Get(state.m_Hex2);
		if (m_Hex2 == null && state.m_Hex2 != null)
		{
			m_Hex2 = new CMapTile(state.m_Hex2, references);
			references.Add(state.m_Hex2, m_Hex2);
		}
		m_Props = references.Get(state.m_Props);
		if (m_Props == null && state.m_Props != null)
		{
			m_Props = new List<CObjectProp>();
			for (int i = 0; i < state.m_Props.Count; i++)
			{
				CObjectProp cObjectProp = state.m_Props[i];
				CObjectProp cObjectProp2 = references.Get(cObjectProp);
				if (cObjectProp2 == null && cObjectProp != null)
				{
					CObjectProp cObjectProp3 = ((cObjectProp is CObjectChest state2) ? new CObjectChest(state2, references) : ((cObjectProp is CObjectDifficultTerrain state3) ? new CObjectDifficultTerrain(state3, references) : ((cObjectProp is CObjectDoor state4) ? new CObjectDoor(state4, references) : ((cObjectProp is CObjectGoldPile state5) ? new CObjectGoldPile(state5, references) : ((cObjectProp is CObjectHazardousTerrain state6) ? new CObjectHazardousTerrain(state6, references) : ((cObjectProp is CObjectMonsterGrave state7) ? new CObjectMonsterGrave(state7, references) : ((cObjectProp is CObjectObstacle state8) ? new CObjectObstacle(state8, references) : ((cObjectProp is CObjectPortal state9) ? new CObjectPortal(state9, references) : ((cObjectProp is CObjectPressurePlate state10) ? new CObjectPressurePlate(state10, references) : ((cObjectProp is CObjectQuestItem state11) ? new CObjectQuestItem(state11, references) : ((cObjectProp is CObjectResource state12) ? new CObjectResource(state12, references) : ((cObjectProp is CObjectTerrainVisual state13) ? new CObjectTerrainVisual(state13, references) : ((!(cObjectProp is CObjectTrap state14)) ? new CObjectProp(cObjectProp, references) : new CObjectTrap(state14, references))))))))))))));
					cObjectProp2 = cObjectProp3;
					references.Add(cObjectProp, cObjectProp2);
				}
				m_Props.Add(cObjectProp2);
			}
			references.Add(state.m_Props, m_Props);
		}
		m_Spawners = references.Get(state.m_Spawners);
		if (m_Spawners != null || state.m_Spawners == null)
		{
			return;
		}
		m_Spawners = new List<CSpawner>();
		for (int j = 0; j < state.m_Spawners.Count; j++)
		{
			CSpawner cSpawner = state.m_Spawners[j];
			CSpawner cSpawner2 = references.Get(cSpawner);
			if (cSpawner2 == null && cSpawner != null)
			{
				CSpawner cSpawner3 = ((!(cSpawner is CInteractableSpawner state15)) ? new CSpawner(cSpawner, references) : new CInteractableSpawner(state15, references));
				cSpawner2 = cSpawner3;
				references.Add(cSpawner, cSpawner2);
			}
			m_Spawners.Add(cSpawner2);
		}
		references.Add(state.m_Spawners, m_Spawners);
	}

	public CObjectProp FindProp(ScenarioManager.ObjectImportType importObjectType)
	{
		foreach (CObjectProp prop in m_Props)
		{
			if (prop.ObjectType == importObjectType)
			{
				return prop;
			}
		}
		return null;
	}

	public List<CObjectProp> FindProps(ScenarioManager.ObjectImportType importObjectType)
	{
		return m_Props.FindAll((CObjectProp x) => x.ObjectType == importObjectType);
	}

	public bool CheckForPropTypes(List<ScenarioManager.ObjectImportType> typesToCheckFor)
	{
		if (typesToCheckFor == null || typesToCheckFor.Count == 0)
		{
			return false;
		}
		return m_Props.Any((CObjectProp p) => typesToCheckFor.Contains(p.ObjectType));
	}

	public CSpawner FindSpawner(string guid)
	{
		return m_Spawners.Find((CSpawner x) => x.SpawnerGuid == guid);
	}

	public void SpawnProp(CObjectProp prop, bool notifyClient = true, float spawnDelay = 0f)
	{
		if (prop == null)
		{
			return;
		}
		m_Props.Add(prop);
		CMap cMap = ScenarioManager.Scenario.Maps.Single((CMap s) => s.MapGuid == m_HexMap.MapGuid);
		CMap cMap2 = ((m_Hex2Map != null) ? ScenarioManager.Scenario.Maps.Single((CMap s) => s.MapGuid == m_Hex2Map.MapGuid) : null);
		if (!cMap.Props.Any((CObjectProp a) => a.PropGuid == prop.PropGuid) && (cMap2 == null || !cMap2.Props.Any((CObjectProp a2) => a2.PropGuid == prop.PropGuid)))
		{
			lock (ScenarioManager.CurrentScenarioState.Props)
			{
				ScenarioManager.CurrentScenarioState.Props.Add(prop);
			}
		}
		if (notifyClient)
		{
			CSpawn_MessageData message = new CSpawn_MessageData(null)
			{
				m_SpawnDelay = spawnDelay,
				m_Prop = prop
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public void SpawnSpawner(CSpawner spawner)
	{
		if (spawner != null)
		{
			m_Spawners.Add(spawner);
			CMap cMap = ScenarioManager.Scenario.Maps.Single((CMap s) => s.MapGuid == m_HexMap.MapGuid);
			CMap cMap2 = ((m_Hex2Map != null) ? ScenarioManager.Scenario.Maps.Single((CMap s) => s.MapGuid == m_Hex2Map.MapGuid) : null);
			if (!cMap.Spawners.Any((CSpawner a) => a.SpawnerGuid == spawner.SpawnerGuid) && (cMap2 == null || !cMap2.Spawners.Any((CSpawner a2) => a2.SpawnerGuid == spawner.SpawnerGuid)))
			{
				ScenarioManager.CurrentScenarioState.Spawners.Add(spawner);
			}
		}
	}

	public bool IsMapShared(CTile tile, bool hexMapOnly = false)
	{
		if (tile.m_HexMap != null)
		{
			if (m_HexMap != null && tile.m_HexMap == m_HexMap)
			{
				return true;
			}
			if (m_Hex2Map != null && tile.m_HexMap == m_Hex2Map)
			{
				return true;
			}
		}
		if (!hexMapOnly && tile.m_Hex2Map != null)
		{
			if (m_HexMap != null && tile.m_Hex2Map == m_HexMap)
			{
				return true;
			}
			if (m_Hex2Map != null && tile.m_Hex2Map == m_Hex2Map)
			{
				return true;
			}
		}
		return false;
	}
}
