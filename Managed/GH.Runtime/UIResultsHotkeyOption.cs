using System;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

public class UIResultsHotkeyOption : UIResultsOption
{
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private KeyAction _keyAction;

	[SerializeField]
	private Hotkey _hotkey;

	private SimpleKeyActionHandlerBlocker _activeBlocker = new SimpleKeyActionHandlerBlocker();

	private SimpleKeyActionHandlerBlocker _statsPanelBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);

	private bool _interactable = true;

	public override void Register(Action action)
	{
		base.Register(action);
		_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(_keyAction, OnHotkeyPressed, Show, Hide).AddBlocker(_activeBlocker).AddBlocker(_statsPanelBlocker));
	}

	public override void Unregister()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(_keyAction, OnHotkeyPressed);
		}
		_hotkey.Deinitialize();
	}

	public override void DisableInteractability()
	{
		_interactable = false;
		_canvasGroup.alpha = 0.5f;
	}

	public override void SetActive(bool active)
	{
		_activeBlocker.SetBlock(!active);
	}

	public override void HandleStatsPanelStateChanged(bool active)
	{
		_statsPanelBlocker.SetBlock(active);
	}

	private void OnHotkeyPressed()
	{
		if (_interactable && base.gameObject.activeInHierarchy)
		{
			HotkeyAction();
		}
	}

	protected virtual void HotkeyAction()
	{
		InvokeAction();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
