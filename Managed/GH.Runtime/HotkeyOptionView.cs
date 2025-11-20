using System;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

public class HotkeyOptionView : MonoBehaviour, IOptionView
{
	[SerializeField]
	private KeyAction _keyAction;

	[SerializeField]
	private Hotkey _hotkey;

	[SerializeField]
	private bool _hideWhenNotInteractable = true;

	private SimpleKeyActionHandlerBlocker _interactableBlocker = new SimpleKeyActionHandlerBlocker();

	private bool _show;

	private bool Interactable => !_interactableBlocker.IsBlock;

	private bool NeedShow
	{
		get
		{
			if (_show)
			{
				if (_hideWhenNotInteractable)
				{
					return Interactable;
				}
				return true;
			}
			return false;
		}
	}

	public event Action OnPressed;

	private void OnEnable()
	{
		_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(_keyAction, OnHotkeyPressed).AddBlocker(_interactableBlocker));
	}

	private void OnDisable()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(_keyAction, OnHotkeyPressed);
		}
		_hotkey.Deinitialize();
	}

	public void SetInteractable(bool interactable)
	{
		_interactableBlocker.SetBlock(!interactable);
		if (_hideWhenNotInteractable)
		{
			UpdateShown();
		}
	}

	public void SetShown(bool shown)
	{
		_show = shown;
		UpdateShown();
	}

	private void UpdateShown()
	{
		base.gameObject.SetActive(NeedShow);
	}

	protected virtual void OnHotkeyPressed()
	{
		InvokeOnPressed();
	}

	protected void InvokeOnPressed()
	{
		this.OnPressed?.Invoke();
	}
}
