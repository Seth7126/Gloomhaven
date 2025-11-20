#define ENABLE_LOGS
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InteractabilityHighlightCanvas : MonoBehaviour
{
	public static InteractabilityHighlightCanvas s_Instance;

	public RectTransform HighlightCanvasRect;

	public GameObject UIHighlightPrefab;

	public GameObject HexHighlightPrefab;

	private List<InteractabilityHighlightRect> m_ShowingHighlightRects = new List<InteractabilityHighlightRect>();

	private List<GameObject> m_ShowingHighlightHexes = new List<GameObject>();

	[UsedImplicitly]
	private void Awake()
	{
		s_Instance = this;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		if (CoreApplication.IsQuitting)
		{
			return;
		}
		s_Instance = null;
		try
		{
			ClearHighlights();
		}
		catch
		{
			Debug.Log("Exception caught trying to clear Interactability Highlights OnDestroy");
		}
	}

	public void AddHighlightToTile(TileIndex indexToPlaceTileHighlight)
	{
		if (indexToPlaceTileHighlight != null)
		{
			CClientTile cClientTile = CAutoTileClick.TileIndexToClientTile(indexToPlaceTileHighlight);
			if (cClientTile != null)
			{
				GameObject gameObject = Object.Instantiate(HexHighlightPrefab, cClientTile.m_GameObject.transform.parent);
				gameObject.transform.position = cClientTile.m_GameObject.transform.position;
				gameObject.name = "InteractabilityHexHighlight";
				m_ShowingHighlightHexes.Add(gameObject);
			}
		}
	}

	public void AddHighlightForRectTransform(RectTransform rectToHighlight)
	{
		if (!(rectToHighlight == null) && rectToHighlight.gameObject.activeInHierarchy)
		{
			InteractabilityHighlightRect component = Object.Instantiate(UIHighlightPrefab, base.transform).GetComponent<InteractabilityHighlightRect>();
			component.SetRectToFollow(rectToHighlight, HighlightCanvasRect);
			m_ShowingHighlightRects.Add(component);
		}
	}

	public void ClearHighlights()
	{
		foreach (GameObject showingHighlightHex in m_ShowingHighlightHexes)
		{
			Object.Destroy(showingHighlightHex);
		}
		m_ShowingHighlightHexes.Clear();
		foreach (InteractabilityHighlightRect showingHighlightRect in m_ShowingHighlightRects)
		{
			showingHighlightRect.SetShowing(shouldShow: false);
		}
		m_ShowingHighlightRects.Clear();
	}

	public void ToggleHighlights(bool isActive)
	{
		m_ShowingHighlightRects.ForEach(delegate(InteractabilityHighlightRect rect)
		{
			rect.gameObject.SetActive(isActive);
		});
		m_ShowingHighlightHexes.ForEach(delegate(GameObject hex)
		{
			hex.gameObject.SetActive(isActive);
		});
	}
}
