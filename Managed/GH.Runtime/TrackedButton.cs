#define ENABLE_LOGS
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrackedButton : Button
{
	[SerializeField]
	private PointerEventData.InputButton buttonKey;

	[SerializeField]
	private AudioButtonProfile audioProfile;

	[SerializeField]
	private string mouseClickAudioItem;

	[SerializeField]
	private string mouseEnterAudioItem;

	[SerializeField]
	private bool playAudioWhenNoInteractable;

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedButton(this))
		{
			Debug.Log("Button press for button " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		if (base.interactable && AutoTestController.s_ShouldRecordUIActionsForAutoTest)
		{
			AutoTestController.s_Instance.LogButtonClick(base.gameObject);
		}
		if (buttonKey != PointerEventData.InputButton.Left && buttonKey == eventData.button)
		{
			if (!IsActive() || !IsInteractable())
			{
				return;
			}
			base.onClick.Invoke();
		}
		else if (buttonKey == PointerEventData.InputButton.Left && buttonKey == eventData.button)
		{
			base.OnPointerClick(eventData);
		}
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseClickAudioItem);
		}
		else if (audioProfile != null)
		{
			AudioControllerUtils.PlaySound(audioProfile.nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedButton(this))
		{
			Debug.Log("Button press for button " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnSubmit(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseClickAudioItem);
		}
		else if (audioProfile != null)
		{
			AudioControllerUtils.PlaySound(audioProfile.nonInteractableMouseDownAudioItem);
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
		}
		if (AutoTestController.s_ShouldRecordUIActionsForAutoTest)
		{
			AutoTestController.s_Instance.LogButtonHover(base.gameObject);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (InputManager.GamePadInUse && (IsInteractable() || playAudioWhenNoInteractable))
		{
			AudioControllerUtils.PlaySound((mouseEnterAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseEnterAudioItem : mouseEnterAudioItem);
		}
	}

	public void ClearSelectedState()
	{
		InstantClearState();
	}
}
