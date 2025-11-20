using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.GUI.Quest;
using MapRuleLibrary.MapState;
using UnityEngine;
using UnityEngine.Events;

public class MapMarkersManager : Singleton<MapMarkersManager>
{
	[Serializable]
	public class QuestLocationEvent : UnityEvent<CQuestState, MapLocation>
	{
	}

	[SerializeField]
	private UIQuestMapMarker questMaker;

	[SerializeField]
	private UILocationMapMarker locationMarker;

	[SerializeField]
	private UIHeadquartersMapMarker headquartersMarker;

	[SerializeField]
	private CanvasGroup mapMarkersContainer;

	[SerializeField]
	private RectTransform highlightContainer;

	[SerializeField]
	private float opacityFade = 0.5f;

	[SerializeField]
	private float maxZoomLocationsVisible = 70f;

	private bool trackZoom = true;

	private float lastZoom = -1f;

	private bool locationMarkersInZoom = true;

	public QuestLocationEvent OnAddedQuestMarker;

	public QuestLocationEvent OnRemovedQuestMarker;

	private List<UIMapMarker> orderableMarkers = new List<UIMapMarker>();

	public UIQuestMapMarker SpawnQuestMarker(MapLocation location, Vector3 offset)
	{
		return SpawnQuestMarker(new Quest(location.LocationQuest), location, offset);
	}

	public UIQuestMapMarker SpawnQuestMarker(IQuest quest, MapLocation location, Vector3 offset)
	{
		UIQuestMapMarker component = ObjectPool.Spawn(questMaker, mapMarkersContainer.transform).GetComponent<UIQuestMapMarker>();
		component.SetLocation(location, offset);
		component.SetQuest(quest);
		orderableMarkers.Add(component);
		RefreshMarkersOrder();
		if (location.LocationQuest != null)
		{
			OnAddedQuestMarker.Invoke(location.LocationQuest, location);
		}
		return component;
	}

	public void RemoveQuestMarker(MapLocation mapLocation, UIQuestMapMarker questMarker)
	{
		orderableMarkers.Remove(questMarker);
		ObjectPool.Recycle(questMarker.gameObject);
		RefreshMarkersOrder();
		if (mapLocation.LocationQuest != null)
		{
			OnRemovedQuestMarker.Invoke(mapLocation.LocationQuest, mapLocation);
		}
	}

	public UILocationMapMarker SpawnLocationMarker(MapLocation location, Vector3 offset, bool keepOrder = false)
	{
		UILocationMapMarker component = ObjectPool.Spawn(locationMarker, mapMarkersContainer.transform).GetComponent<UILocationMapMarker>();
		component.SetLocation(location, offset);
		if (keepOrder)
		{
			orderableMarkers.Add(component);
			RefreshMarkersOrder();
		}
		RefreshVisibilityLocationMarker(component);
		return component;
	}

	public UILocationMapMarker SpawnInformationMarker(string information, Vector3 location, Func<bool> enableChecker)
	{
		UILocationMapMarker component = ObjectPool.Spawn(locationMarker, mapMarkersContainer.transform).GetComponent<UILocationMapMarker>();
		component.SetLocation(location, information, enableChecker);
		orderableMarkers.Add(component);
		RefreshMarkersOrder();
		return component;
	}

	public UILocationMapMarker SpawnHeadquartersMarker(MapLocation location, Vector3 offset)
	{
		headquartersMarker.SetLocation(location, offset);
		orderableMarkers.Add(headquartersMarker);
		RefreshMarkersOrder();
		headquartersMarker.gameObject.SetActive(value: true);
		return headquartersMarker;
	}

	public void RemoveLocationMarker(UILocationMapMarker marker)
	{
		if (orderableMarkers.Remove(marker))
		{
			RefreshMarkersOrder();
		}
		if (marker == headquartersMarker)
		{
			headquartersMarker.gameObject.SetActive(value: false);
		}
		else
		{
			ObjectPool.Recycle(marker.gameObject);
		}
	}

	public void HideMarkers()
	{
		mapMarkersContainer.alpha = 0f;
		TrackCameraZoom(track: false);
	}

	public void ShowMarkers()
	{
		mapMarkersContainer.alpha = 1f;
		TrackCameraZoom(track: true);
	}

	public void FadeMarkers()
	{
		mapMarkersContainer.alpha = opacityFade;
		TrackCameraZoom(track: true);
	}

	public void RefreshMarkersOrder()
	{
		foreach (UIMapMarker item in from it in orderableMarkers
			where it != null && it.gameObject.activeSelf
			orderby base.transform.position.x descending
			select it)
		{
			item.transform.SetAsLastSibling();
		}
	}

	private void TrackCameraZoom(bool track)
	{
		if (trackZoom != track)
		{
			trackZoom = track;
			if (trackZoom)
			{
				UpdateZoomLocationMarkers();
			}
		}
	}

	private void LateUpdate()
	{
		if (trackZoom && Math.Abs(CameraController.s_CameraController.Zoom - lastZoom) > 0.0001f)
		{
			UpdateZoomLocationMarkers();
		}
	}

	private void UpdateZoomLocationMarkers()
	{
		lastZoom = CameraController.s_CameraController.Zoom;
		if (lastZoom <= maxZoomLocationsVisible == locationMarkersInZoom)
		{
			return;
		}
		locationMarkersInZoom = !locationMarkersInZoom;
		foreach (UILocationMapMarker item in orderableMarkers.Where((UIMapMarker it) => it is UILocationMapMarker && it != headquartersMarker))
		{
			RefreshVisibilityLocationMarker(item);
		}
	}

	public void RefreshVisibilityLocationMarker(UILocationMapMarker marker)
	{
		if (!(headquartersMarker == marker) && marker.IsEnabled())
		{
			if (locationMarkersInZoom)
			{
				marker.ShowFadeIn();
			}
			else
			{
				marker.HideFadeOut();
			}
		}
	}

	public void RefreshQuestsState()
	{
		foreach (UIQuestMapMarker item in orderableMarkers.Where((UIMapMarker it) => it is UIQuestMapMarker))
		{
			item.RefreshState();
		}
	}

	public void SetMaxZoomLocationsVisible(float zoom)
	{
		if (maxZoomLocationsVisible != zoom)
		{
			maxZoomLocationsVisible = zoom;
			UpdateZoomLocationMarkers();
		}
	}
}
