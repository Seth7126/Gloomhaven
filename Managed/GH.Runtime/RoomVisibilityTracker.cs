#define ENABLE_LOGS
using System;
using System.Collections;
using UnityEngine;

public class RoomVisibilityTracker : MonoBehaviour
{
	[SerializeField]
	private TileBehaviour m_CentralTile;

	public static event Action<ProceduralMapTile, bool> ProceduralMapTileVisibilityStateChanged;

	private IEnumerator Start()
	{
		yield return null;
		bool flag = IsVisible();
		ShowMaptile(flag);
		if (!flag)
		{
			RoomVisibilityManager.s_Instance?.AddRoom(this);
		}
	}

	public void ShowMaptile(bool show, bool forceOverride = false)
	{
		Debug.LogWarning($"Called Show Maptile: {show}", this);
		ProceduralMapTile component = GetComponent<ProceduralMapTile>();
		if (!(component != null))
		{
			return;
		}
		if (show)
		{
			component.visibility = ProceduralMapTile.Visibility.All;
		}
		else
		{
			component.visibility = ProceduralMapTile.Visibility.Preview;
		}
		RoomVisibilityTracker.ProceduralMapTileVisibilityStateChanged?.Invoke(component, show);
		foreach (GameObject connectedDoor in component.ConnectedDoors)
		{
			ProceduralDoorway component2 = connectedDoor.GetComponent<ProceduralDoorway>();
			if (component2 != null)
			{
				if (show)
				{
					component2.OverrideVisibility(ProceduralMapTile.Visibility.All);
				}
				else if (!forceOverride)
				{
					component2.SetVisibility(component.visibility);
				}
				else
				{
					component2.OverrideVisibility(component.visibility);
				}
			}
		}
	}

	public void AddProp(GameObject prop)
	{
	}

	public bool IsVisible()
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.LevelEditor)
		{
			return true;
		}
		if (m_CentralTile == null)
		{
			Debug.LogError("Central tile is NULL, can't define is volume visible or not");
			return false;
		}
		if (m_CentralTile.m_ClientTile.m_Tile.m_HexMap != null)
		{
			if (m_CentralTile.m_ClientTile.m_Tile.m_HexMap.Revealed)
			{
				return !m_CentralTile.m_ClientTile.m_Tile.m_HexMap.Destroyed;
			}
			return false;
		}
		return false;
	}

	public void EnableAssets()
	{
		ShowMaptile(show: true);
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.RoomRevealed, null, (m_CentralTile.m_ClientTile.m_Tile.m_HexMap == null) ? string.Empty : m_CentralTile.m_ClientTile.m_Tile.m_HexMap.MapGuid));
	}
}
