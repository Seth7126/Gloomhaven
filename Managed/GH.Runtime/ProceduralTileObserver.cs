using System.Collections.Generic;
using Apparance.Net;
using JetBrains.Annotations;
using Script.Controller;
using UnityEngine;

[RequireComponent(typeof(ApparanceEntity))]
[ExecuteInEditMode]
public class ProceduralTileObserver : ProceduralBase, IProceduralContent
{
	private List<GameObject> Tiles = new List<GameObject>();

	private ParameterCollection Parameters;

	private bool m_bRequestRebuild;

	private bool m_bRequestObserverUpdate;

	private UnityEngine.Vector3 previousPosition;

	private float previousOrientation;

	private Bounds previousBounds;

	private uint previousLocalSeed;

	private bool previousActive;

	private Dictionary<GameObject, UnityEngine.Vector3> TilePositions = new Dictionary<GameObject, UnityEngine.Vector3>();

	public List<GameObject> Debug_TileList => Tiles;

	private void Start()
	{
		m_bRequestObserverUpdate = true;
		m_bRequestRebuild = true;
	}

	private void NotifyObserverMove()
	{
		Bounds bounds = default(Bounds);
		ProceduralTileTracker.CollectBounds(base.gameObject, ref bounds);
		ProceduralTileTracker.NotifyObserverMove(base.gameObject, previousBounds, bounds);
		previousPosition = base.transform.position;
		previousOrientation = base.transform.eulerAngles.y;
		previousBounds = bounds;
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		Singleton<ObjectCacheService>.Instance.AddTileObserver(this);
		m_bRequestObserverUpdate = true;
		m_bRequestRebuild = true;
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if (Singleton<ObjectCacheService>.Instance != null)
		{
			Singleton<ObjectCacheService>.Instance.RemoveTileObserver(this);
		}
		Tiles.Clear();
		m_bRequestRebuild = false;
		m_bRequestObserverUpdate = false;
		previousPosition = UnityEngine.Vector3.zero;
		previousOrientation = 0f;
		previousBounds = default(Bounds);
		previousLocalSeed = 0u;
		previousActive = false;
	}

	public void NotifyTile(GameObject tile, bool was_in, bool now_in, bool physical_move)
	{
		bool flag = Tiles.Contains(tile);
		if (now_in)
		{
			if (!flag)
			{
				Tiles.Add(tile);
				TilePositions[tile] = tile.transform.position;
			}
		}
		else if (was_in && flag)
		{
			Tiles.Remove(tile);
			TilePositions.Remove(tile);
		}
		Tiles.RemoveAll((GameObject t) => t == null || !((IProceduralTile)t.GetComponent<ProceduralTile>()).IsActive);
		bool num = Tiles.Contains(tile);
		bool flag2 = !num && flag;
		bool flag3 = num && !flag;
		bool flag4 = was_in || now_in;
		bool flag5 = num && TilePositions.ContainsKey(tile) && tile.transform.position != TilePositions[tile];
		if (flag2 || flag3 || (physical_move && flag4 && flag5))
		{
			m_bRequestRebuild = true;
		}
	}

	private void OnValidate()
	{
		Update();
	}

	protected virtual void Update()
	{
		if (base.gameObject.scene.name != null && base.gameObject.scene.rootCount != 0)
		{
			UnityEngine.Vector3 position = base.transform.position;
			float y = base.transform.eulerAngles.y;
			if (position != previousPosition || y != previousOrientation)
			{
				NotifyObserverMove();
			}
			bool activeInHierarchy = base.gameObject.activeInHierarchy;
			if (activeInHierarchy != previousActive)
			{
				m_bRequestRebuild = true;
				previousActive = activeInHierarchy;
			}
			if (m_bRequestObserverUpdate)
			{
				m_bRequestObserverUpdate = false;
				NotifyObserverMove();
			}
			if (m_bRequestRebuild)
			{
				m_bRequestRebuild = false;
				Rebuild();
				Apply();
			}
		}
	}

	private void Rebuild()
	{
		Parameters = ParameterCollection.CreateEmpty();
		Parameters.BeginWrite();
		ParameterCollection parameterCollection = Parameters.WriteListBegin(3);
		foreach (GameObject tile in Tiles)
		{
			if (tile != null)
			{
				IProceduralTile component = tile.GetComponent<IProceduralTile>();
				if (component != null)
				{
					ParameterCollection parameterCollection2 = parameterCollection.WriteListBegin();
					parameterCollection2.WriteInteger(ProceduralTileTracker.TileAddressFromPosition(component.Position));
					parameterCollection2.WriteInteger(component.Flags);
					parameterCollection2.WriteFloat(component.ContentRadius);
					parameterCollection2.WriteFloat(component.ContentHeight);
					parameterCollection.WriteListEnd();
				}
			}
		}
		Parameters.WriteListEnd();
		WriteExtraParameters(Parameters, 4);
		Parameters.EndWrite();
	}

	public virtual void WriteExtraParameters(ParameterCollection parameters, int next_id)
	{
	}

	private void Apply()
	{
		ApparanceEntity component = GetComponent<ApparanceEntity>();
		component.PartialParameterOverride = Parameters;
		component.IsPopulated = false;
		component.NotifyPropertyChanged();
		component.IsPopulated = true;
		component.NotifyPropertyChanged();
	}

	void IProceduralContent.RebuildContent()
	{
		m_bRequestRebuild = true;
	}
}
