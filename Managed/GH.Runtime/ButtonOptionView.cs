using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonOptionView : MonoBehaviour, IOptionView
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private TextLocalizedListener text;

	private Color defaultTextColor;

	public UnityEvent<bool> OnInteractionChanged;

	public event Action OnPressed;

	private void Awake()
	{
		defaultTextColor = text.Text.color;
		button.onClick.AddListener(OnButtonPressed);
	}

	private void OnDestroy()
	{
		button.onClick.RemoveListener(OnButtonPressed);
	}

	public void SetTextKey(string key)
	{
		text.SetTextKey(key);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
		text.Text.color = (interactable ? defaultTextColor : UIInfoTools.Instance.greyedOutTextColor);
		OnInteractionChanged?.Invoke(interactable);
	}

	public void SetShown(bool shown)
	{
		base.gameObject.SetActive(shown);
	}

	private void OnButtonPressed()
	{
		this.OnPressed?.Invoke();
	}
}
