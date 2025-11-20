using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICityEncounterButton : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GUIAnimator highlightAnimator;

	private Dictionary<Component, string> disableInteractionRequest = new Dictionary<Component, string>();

	public bool Interactable => button.interactable;

	public UnityEvent OnHovered => button.onMouseEnter;

	public UnityEvent OnUnhovered => button.onMouseExit;

	private void Awake()
	{
		ToggleGreyOut(!AdventureState.MapState.CanDrawCityEvent);
	}

	private void OnEnable()
	{
		button.onClick.AddListener(OpenCityEvent);
		Highlight(button.interactable);
	}

	private void OnDisable()
	{
		button.onClick.RemoveListener(OpenCityEvent);
	}

	private void OnDestroy()
	{
		highlightAnimator.Stop();
	}

	public void OpenCityEvent()
	{
		Singleton<UIGuildmasterHUD>.Instance.OpenCityEncounter();
	}

	public void ToggleGreyOut(bool greyedOut = true, Component request = null, string tooltipText = null)
	{
		if (greyedOut)
		{
			disableInteractionRequest[(request == null) ? this : request] = tooltipText;
		}
		else
		{
			disableInteractionRequest.Remove((request == null) ? this : request);
		}
		button.interactable = AdventureState.MapState.CanDrawCityEvent && disableInteractionRequest.Count == 0;
		icon.material = ((!button.interactable) ? UIInfoTools.Instance.disabledGrayscaleMaterial : null);
		if (!AdventureState.MapState.CanDrawCityEvent)
		{
			tooltip.SetText(LocalizationManager.GetTranslation("GUI_CITY_ENCOUNTER_TOOLTIP"), refreshTooltip: true);
			tooltip.gameObject.SetActive(value: true);
		}
		else if (disableInteractionRequest.Any())
		{
			IEnumerable<string> enumerable = from it in disableInteractionRequest
				where it.Value.IsNOTNullOrEmpty()
				select it.Value;
			tooltip.gameObject.SetActive(enumerable.Any());
			if (tooltip.gameObject.activeSelf)
			{
				tooltip.SetText(string.Join("\n\n", enumerable), refreshTooltip: true);
			}
		}
		else
		{
			tooltip.gameObject.SetActive(value: false);
		}
		Highlight(button.interactable);
	}

	private void Highlight(bool highlight)
	{
		highlightAnimator.gameObject.SetActive(highlight);
		if (highlight)
		{
			highlightAnimator.Play();
		}
		else
		{
			highlightAnimator.Stop();
		}
	}
}
