using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gloomhaven;

public class KeyActionControlButton : MonoBehaviour, IDeselectHandler, IEventSystemHandler
{
	[SerializeField]
	private KeyAction keyAction;

	[SerializeField]
	private TextMeshProUGUI keyText;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private RectTransform rowRectTransform;

	private bool recording;

	private Action<KeyAction> onPressed;

	private Action<KeyAction> onDeselect;

	private Action<RectTransform> onSelected;

	private void Start()
	{
		button.onClick.AddListener(RecordNewKey);
		button.onSelected.AddListener(delegate
		{
			onSelected?.Invoke(rowRectTransform ?? (base.transform as RectTransform));
		});
		Refresh();
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
	}

	private void RecordNewKey()
	{
		onPressed?.Invoke(keyAction);
		keyText.text = "...";
	}

	private void SetKeyCode(KeyCode keycode)
	{
		keyText.text = ((keycode == KeyCode.None) ? "..." : KeyCodeConverter.Convert(keycode));
	}

	public void Refresh()
	{
		KeyCode keyCode = Singleton<InputManager>.Instance.GetKeyCode(keyAction);
		SetKeyCode(keyCode);
	}

	public void Init(Action<KeyAction> onPressed, Action<KeyAction> onDeselect, Action<RectTransform> onSelected)
	{
		this.onPressed = onPressed;
		this.onDeselect = onDeselect;
		this.onSelected = onSelected;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		onDeselect?.Invoke(keyAction);
	}

	public void EnableNavigation(bool selected = false)
	{
		button.SetNavigation(Navigation.Mode.Vertical);
		if (selected)
		{
			button.Select();
		}
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}
}
