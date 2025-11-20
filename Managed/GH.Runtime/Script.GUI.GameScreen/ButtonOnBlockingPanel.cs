using System;
using UnityEngine;

namespace Script.GUI.GameScreen;

public class ButtonOnBlockingPanel : MonoBehaviour
{
	protected Action<bool> onChangeInteraction;

	protected CanvasGroup canvasGroup;

	public bool IsInteractable()
	{
		bool activeSelf = base.gameObject.activeSelf;
		bool flag = canvasGroup.alpha > 0.5f;
		return activeSelf && flag;
	}

	public void TakeInteractableCallback(Action<bool> callback)
	{
		onChangeInteraction = callback;
	}

	public void ToggleVisibility(bool active)
	{
		base.gameObject.SetActive(active);
		onChangeInteraction?.Invoke(active);
	}

	public void SetDisableVisualState(bool value)
	{
		canvasGroup.interactable = !value;
		ChangeCanvasAlpha(!value);
	}

	protected void ChangeCanvasAlpha(bool visibility)
	{
		canvasGroup.alpha = (visibility ? 1f : 0f);
		onChangeInteraction(visibility);
	}
}
