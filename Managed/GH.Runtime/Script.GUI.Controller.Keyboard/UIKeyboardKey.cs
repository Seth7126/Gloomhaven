using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Script.GUI.Controller.Keyboard;

public class UIKeyboardKey : MonoBehaviour
{
	[Serializable]
	public class KeyCodeEvent : UnityEvent<KeyCode>
	{
	}

	[Serializable]
	private class HighlightConfig
	{
		[SerializeField]
		private Graphic graphic;

		[SerializeField]
		private Color defaultColor;

		[SerializeField]
		private Color highlightColor;

		public void Reset()
		{
			graphic.color = defaultColor;
		}

		public void Highlight()
		{
			graphic.color = highlightColor;
		}
	}

	[SerializeField]
	private KeyCode keyCode;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private HighlightConfig[] highlights;

	[SerializeField]
	private TextMeshProUGUI text;

	public KeyCodeEvent OnClicked;

	private bool isHighlighted;

	private bool _highlightOnEnable;

	private void Awake()
	{
		button.onMouseEnter.AddListener(Highlight);
		button.onMouseExit.AddListener(Unhighlight);
		if (!InputManager.GamePadInUse)
		{
			button.onClick.AddListener(OnClick);
		}
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnClick).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)));
		if (keyCode != KeyCode.None)
		{
			SetKeyCode(keyCode);
		}
	}

	private void OnDestroy()
	{
		Singleton<KeyActionHandlerController>.Instance?.RemoveHandler(KeyAction.UI_SUBMIT, OnClick);
		OnClicked.RemoveAllListeners();
		button.onMouseEnter.RemoveListener(Highlight);
		button.onMouseExit.RemoveListener(Unhighlight);
		text = null;
		button = null;
	}

	private void OnClick()
	{
		OnClicked.Invoke(keyCode);
	}

	private void Highlight()
	{
		isHighlighted = true;
		if (text != null)
		{
			text.color = Color.black;
		}
		for (int i = 0; i < highlights.Length; i++)
		{
			highlights[i].Highlight();
		}
	}

	private void Unhighlight()
	{
		isHighlighted = false;
		if (text != null)
		{
			text.color = UIInfoTools.Instance.basicTextColor;
		}
		for (int i = 0; i < highlights.Length; i++)
		{
			highlights[i].Reset();
		}
	}

	public void SetKeyCode(KeyCode keyCode)
	{
		this.keyCode = keyCode;
		if (text != null)
		{
			text.text = KeyCodeConverter.ConvertToValue(keyCode);
		}
		if (EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			Unhighlight();
		}
	}

	private void OnDisable()
	{
		if (isHighlighted)
		{
			Unhighlight();
		}
	}
}
