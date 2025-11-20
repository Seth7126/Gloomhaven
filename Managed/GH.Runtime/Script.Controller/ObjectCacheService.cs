#define ENABLE_LOGS
using System.Collections.Generic;
using SM.Utils;
using ScenarioRuleLibrary;
using UnityEngine;

namespace Script.Controller;

public class ObjectCacheService : Singleton<ObjectCacheService>
{
	private readonly Dictionary<CMap, GameObject> _mapCache = new Dictionary<CMap, GameObject>();

	private readonly Dictionary<CObjectProp, GameObject> _propsCache = new Dictionary<CObjectProp, GameObject>();

	private readonly Dictionary<CMapTile, GameObject> _tileCache = new Dictionary<CMapTile, GameObject>();

	private readonly HashSet<ProceduralTileObserver> _tileObservers = new HashSet<ProceduralTileObserver>();

	private readonly HashSet<ProceduralTile> _proceduralTiles = new HashSet<ProceduralTile>();

	private readonly HashSet<TileBehaviour> _tileBehaviours = new HashSet<TileBehaviour>();

	private readonly HashSet<UnityGameEditorObject> _gameEditorObjects = new HashSet<UnityGameEditorObject>();

	public void AddProp(CObjectProp objectProp, GameObject spawnedInstance)
	{
		_propsCache[objectProp] = spawnedInstance;
	}

	public void RemoveProp(CObjectProp removedProp)
	{
		_propsCache.Remove(removedProp);
	}

	public bool ContainsMap(CMap cMap)
	{
		return _mapCache.ContainsKey(cMap);
	}

	public void AddMap(CMap cMap, GameObject spawnedInstance)
	{
		_mapCache.Add(cMap, spawnedInstance);
	}

	public void RemoveMap(CMap cMap)
	{
		_mapCache.Remove(cMap);
	}

	public void AddTile(CMapTile tile, GameObject spawnedInstance)
	{
		_tileCache.Add(tile, spawnedInstance);
	}

	public void RemoveTile(CMapTile tile)
	{
		_tileCache.Remove(tile);
	}

	public void AddTileObserver(ProceduralTileObserver tileObserver)
	{
		_tileObservers.Add(tileObserver);
	}

	public void RemoveTileObserver(ProceduralTileObserver tileObserver)
	{
		_tileObservers.Remove(tileObserver);
	}

	public void AddProceduralTile(ProceduralTile proceduralTile)
	{
		_proceduralTiles.Add(proceduralTile);
	}

	public void RemoveProceduralTile(ProceduralTile proceduralTile)
	{
		_proceduralTiles.Remove(proceduralTile);
	}

	public void AddUnityGameEditorObject(UnityGameEditorObject unityGameEditor)
	{
		_gameEditorObjects.Add(unityGameEditor);
	}

	public void RemoveUnityGameEditorObject(UnityGameEditorObject unityGameEditor)
	{
		_gameEditorObjects.Remove(unityGameEditor);
	}

	public void AddTileBehaviour(TileBehaviour tileBehaviour)
	{
		if (tileBehaviour == null)
		{
			Debug.Log("[ObjectCacheService] <color=magenta>Tile Behaviour is null</color>");
		}
		_tileBehaviours.Add(tileBehaviour);
	}

	public void RemoveTileBehaviour(TileBehaviour tileBehaviour)
	{
		_tileBehaviours.Remove(tileBehaviour);
	}

	public GameObject GetPropObject(CObjectProp prop)
	{
		if (_propsCache.TryGetValue(prop, out var value))
		{
			return value;
		}
		LogUtils.LogWarning("Failed to find prop " + prop.PrefabName + " " + prop.InstanceName);
		return null;
	}

	public GameObject GetPropObject(string propName)
	{
		foreach (CObjectProp key in _propsCache.Keys)
		{
			if (key.InstanceName == propName)
			{
				return _propsCache[key];
			}
		}
		LogUtils.LogWarning("Failed to find prop " + propName + ".");
		return null;
	}

	public GameObject GetMap(CMap cMap)
	{
		if (_mapCache.TryGetValue(cMap, out var value))
		{
			return value;
		}
		LogUtils.LogWarning("Failed to find map " + cMap.MapInstanceName);
		return null;
	}

	public GameObject GetTile(CMapTile cTile)
	{
		if (_tileCache.TryGetValue(cTile, out var value))
		{
			return value;
		}
		LogUtils.LogWarning("Failed to find tile " + cTile.TileGuid);
		return null;
	}

	public HashSet<ProceduralTileObserver> GetTileObservers()
	{
		return _tileObservers;
	}

	public HashSet<ProceduralTile> GetProceduralTiles()
	{
		return _proceduralTiles;
	}

	public HashSet<TileBehaviour> GetTileBehaviors()
	{
		return _tileBehaviours;
	}

	public HashSet<UnityGameEditorObject> GetUnityGameEditorObjects()
	{
		return _gameEditorObjects;
	}
}
