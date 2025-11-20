using System.Collections.Generic;
using AsmodeeNet.Foundation;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

namespace GLOO.Introduction;

public class UIIntroduceElementHighlight : MonoBehaviour
{
	[SerializeField]
	protected List<GameObject> highlightMasks;

	[SerializeField]
	protected List<GameObject> unhighlightMasks;

	[SerializeField]
	protected InteractabilityIsolatedUIControl interactabilityControl;

	[SerializeField]
	[ConditionalField("interactabilityControl", "null", false)]
	private bool resetInteractabilityProfile = true;

	private bool isHighlighted;

	[ContextMenu("Highlight")]
	public void Highlight()
	{
		if (!isHighlighted)
		{
			SetHighlighted(highlighted: true);
		}
	}

	[ContextMenu("Unhighlight")]
	public void Unhighlight()
	{
		if (isHighlighted)
		{
			SetHighlighted(highlighted: false);
		}
	}

	protected virtual void SetHighlighted(bool highlighted)
	{
		isHighlighted = highlighted;
		foreach (GameObject highlightMask in highlightMasks)
		{
			highlightMask.SetActive(highlighted);
		}
		foreach (GameObject unhighlightMask in unhighlightMasks)
		{
			unhighlightMask.SetActive(!highlighted);
		}
		if (!(interactabilityControl != null))
		{
			return;
		}
		if (highlighted)
		{
			if (resetInteractabilityProfile)
			{
				CLevelUIInteractionProfile cLevelUIInteractionProfile = new CLevelUIInteractionProfile();
				cLevelUIInteractionProfile.ControlsToAllow.Add(new CLevelUIInteractionProfile.CLevelUIInteractionSpecific(CLevelUIInteractionProfile.EIsolatedControlType.LevelMessageWindow, CLevelUIInteractionProfile.CLevelUIInteractionSpecific.EControlBehaviourType.LimitInteractionOnly));
				InteractabilityManager.s_Instance.LoadProfile(cLevelUIInteractionProfile);
			}
			InteractabilityManager.AddIsolatedControl(interactabilityControl);
			Singleton<InteractabilityChecker>.Instance.RequestActive(this, active: true);
		}
		else
		{
			InteractabilityManager.s_Instance.LoadProfile(null);
			Singleton<InteractabilityChecker>.Instance.RequestActive(this, active: false);
		}
	}

	private void OnDestroy()
	{
		if (!CoreApplication.IsQuitting && isHighlighted && interactabilityControl != null)
		{
			InteractabilityManager.s_Instance.LoadProfile(null);
		}
	}
}
