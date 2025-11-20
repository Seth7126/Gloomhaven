using System;
using InControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIControllerKeyTip : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI controlName;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Color interactableBackgroundColor;

	[SerializeField]
	private Color nonInteractableBackgroundColor;

	public void ShowInteractable(bool interactable)
	{
		background.color = (interactable ? interactableBackgroundColor : nonInteractableBackgroundColor);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Show(KeyAction keyAction)
	{
		SetText(keyAction);
		Show();
	}

	public void SetText(KeyAction keyAction)
	{
		Tuple<string, Sprite> gamepadActionInfo = Singleton<InputManager>.Instance.GetGamepadActionInfo(keyAction);
		background.sprite = gamepadActionInfo.Item2;
	}

	public void SetText(PlayerAction playerAction)
	{
		Tuple<string, Sprite> gamepadActionInfo = Singleton<InputManager>.Instance.GetGamepadActionInfo(playerAction);
		background.sprite = gamepadActionInfo.Item2;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
