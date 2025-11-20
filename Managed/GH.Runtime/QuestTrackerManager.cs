using MapRuleLibrary.MapState;
using UnityEngine;

public class QuestTrackerManager : MonoBehaviour
{
	[SerializeField]
	private UIQuestTracker questTracker;

	[SerializeField]
	private RectTransform areaTrack;

	[SerializeField]
	private Vector2 offsetTracker;

	public QuestEvent OnClickedTracker;

	private MapLocation location;

	private Vector2 halfSizeArea;

	private void Awake()
	{
		base.enabled = false;
		NewPartyDisplayUI.PartyDisplay.OnShown.AddListener(delegate
		{
			UpdateArea(isOpen: true);
		});
		NewPartyDisplayUI.PartyDisplay.OnHidden.AddListener(delegate
		{
			UpdateArea(isOpen: false);
		});
		UpdateArea(NewPartyDisplayUI.PartyDisplay.IsOpen);
	}

	private void UpdateArea(bool isOpen)
	{
		if (isOpen)
		{
			areaTrack.sizeDelta = new Vector2(0f - (NewPartyDisplayUI.PartyDisplay.transform as RectTransform).rect.size.x, areaTrack.sizeDelta.y);
		}
		else
		{
			areaTrack.sizeDelta = Vector2.zero;
		}
		areaTrack.anchoredPosition = Vector2.zero;
		halfSizeArea = areaTrack.rect.size / 2f - questTracker.GetComponent<RectTransform>().rect.size / 2f - offsetTracker;
	}

	public void TrackQuest(CQuestState quest, MapLocation location)
	{
		this.location = location;
		questTracker.SetQuest(quest, OnClickedTracker.Invoke);
		base.enabled = true;
	}

	public void RemoveTracker()
	{
		location = null;
		questTracker.Hide();
		base.enabled = false;
	}

	private void Update()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (!(location == null))
		{
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(CameraController.s_CameraController.m_Camera, location.transform.position);
			if (!RectTransformUtility.RectangleContainsScreenPoint(areaTrack, screenPoint, UIManager.Instance.UICamera))
			{
				RectTransformUtility.ScreenPointToWorldPointInRectangle(areaTrack, screenPoint, UIManager.Instance.UICamera, out var worldPoint);
				questTracker.transform.position = worldPoint;
				RectTransform rectTransform = questTracker.transform as RectTransform;
				rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(rectTransform.anchoredPosition.x, 0f - halfSizeArea.x, halfSizeArea.x), Mathf.Clamp(rectTransform.anchoredPosition.y, 0f - halfSizeArea.y, halfSizeArea.y));
				questTracker.RefreshPointTo(worldPoint);
				questTracker.Show();
			}
			else
			{
				questTracker.Hide();
			}
		}
	}
}
