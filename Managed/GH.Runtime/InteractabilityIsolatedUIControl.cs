using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

public class InteractabilityIsolatedUIControl : MonoBehaviour
{
	[SerializeField]
	private bool _shouldHighlightUI = true;

	public CLevelUIInteractionProfile.EIsolatedControlType ControlType;

	public string ControlIdentifier;

	public int ControlIndex;

	public string ControlSecondIdentifier;

	public bool shouldInitialiseOnAwake;

	[Header("UI Button Type:")]
	public List<ExtendedButton> ExtendedButtonsToAllow = new List<ExtendedButton>();

	public List<ExtendedToggle> ExtendedTogglesToAllow = new List<ExtendedToggle>();

	public List<TrackedButton> TrackedButtonsToAllow = new List<TrackedButton>();

	public List<TrackedToggle> TrackedTogglesToAllow = new List<TrackedToggle>();

	public List<UITab> TabsToAllow = new List<UITab>();

	public RectTransform SpecifiedRectTransformToFollow;

	public bool PoolableObject;

	[UsedImplicitly]
	private void Awake()
	{
		if (shouldInitialiseOnAwake)
		{
			Initialise();
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		InteractabilityManager.DeregisterControlForInteractionLimiting(this);
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		if (PoolableObject)
		{
			InteractabilityManager.RegisterControlForInteractionLimiting(this);
		}
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if (PoolableObject)
		{
			InteractabilityManager.DeregisterControlForInteractionLimiting(this);
		}
	}

	public void Initialise()
	{
		if (!PoolableObject)
		{
			InteractabilityManager.RegisterControlForInteractionLimiting(this);
		}
		if (ControlType == CLevelUIInteractionProfile.EIsolatedControlType.EscapeMenu || ControlType == CLevelUIInteractionProfile.EIsolatedControlType.PersistentUI || ControlType == CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow || ControlType == CLevelUIInteractionProfile.EIsolatedControlType.ResultsScreen)
		{
			FillAllowedButtonsListWithChildrenOnly();
		}
	}

	public RectTransform GetHighlightRectTransform()
	{
		if (!_shouldHighlightUI)
		{
			return null;
		}
		if (SpecifiedRectTransformToFollow != null)
		{
			return SpecifiedRectTransformToFollow;
		}
		if (ExtendedButtonsToAllow.Count > 0)
		{
			return ExtendedButtonsToAllow[0].GetComponent<RectTransform>();
		}
		if (ExtendedTogglesToAllow.Count > 0)
		{
			return ExtendedTogglesToAllow[0].GetComponent<RectTransform>();
		}
		if (TrackedButtonsToAllow.Count > 0)
		{
			return TrackedButtonsToAllow[0].GetComponent<RectTransform>();
		}
		if (TrackedTogglesToAllow.Count > 0)
		{
			return TrackedTogglesToAllow[0].GetComponent<RectTransform>();
		}
		if (TabsToAllow.Count > 0)
		{
			return TabsToAllow[0].GetComponent<RectTransform>();
		}
		return GetComponent<RectTransform>();
	}

	public void FillAllowedButtonsListWithChildrenOnly()
	{
		ExtendedButtonsToAllow.Clear();
		ExtendedTogglesToAllow.Clear();
		TrackedButtonsToAllow.Clear();
		TrackedTogglesToAllow.Clear();
		TabsToAllow.Clear();
		ExtendedButtonsToAllow = base.gameObject.GetComponentsInChildren<ExtendedButton>(includeInactive: true).ToList();
		ExtendedTogglesToAllow = base.gameObject.GetComponentsInChildren<ExtendedToggle>(includeInactive: true).ToList();
		TrackedButtonsToAllow = base.gameObject.GetComponentsInChildren<TrackedButton>(includeInactive: true).ToList();
		TrackedTogglesToAllow = base.gameObject.GetComponentsInChildren<TrackedToggle>(includeInactive: true).ToList();
		TabsToAllow = base.gameObject.GetComponentsInChildren<UITab>(includeInactive: true).ToList();
	}
}
