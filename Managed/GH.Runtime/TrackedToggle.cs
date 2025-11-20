#define ENABLE_LOGS
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrackedToggle : Toggle
{
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
		if (!InteractabilityManager.ShouldAllowClickForTrackedToggle(this))
		{
			Debug.Log("Toggle press for toggle " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
			return;
		}
		base.OnPointerClick(eventData);
		if (IsInteractable() || playAudioWhenNoInteractable)
		{
			AudioControllerUtils.PlaySound((mouseClickAudioItem.IsNullOrEmpty() && audioProfile != null) ? audioProfile.mouseDownAudioItem : mouseClickAudioItem);
		}
		else if (audioProfile != null)
		{
			AudioControllerUtils.PlaySound(audioProfile.nonInteractableMouseDownAudioItem);
		}
		if (base.interactable && AutoTestController.s_ShouldRecordUIActionsForAutoTest)
		{
			AutoTestController.s_Instance.LogButtonClick(base.gameObject);
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (!InteractabilityManager.ShouldAllowClickForTrackedToggle(this))
		{
			Debug.Log("Toggle press for toggle " + base.gameObject.name + " intercepted and prevented by InteractabilityManager");
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
}
