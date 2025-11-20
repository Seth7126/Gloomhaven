using System;
using Assets.Script.Misc;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UICharacterCreatorStep : MonoBehaviour
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	protected ExtendedButton confirmButton;

	[SerializeField]
	protected Button cancelButton;

	[SerializeField]
	private GameObject confirmHighlight;

	[SerializeField]
	private UIControllerKeyTip confirmControllerTip;

	[SerializeField]
	private Hotkey confirmHotkey;

	[SerializeField]
	protected LongConfirmHandler longConfirmHandler;

	[SerializeField]
	protected ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private ControllerInputElement[] controllerClickeables;

	[SerializeField]
	private UIControllerKeyTip[] controllerTips;

	protected UIWindow window;

	protected virtual Action ShortPressCallback { get; }

	protected virtual void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHidden);
		if (!InputManager.GamePadInUse)
		{
			confirmButton.onClick.AddListener(Confirm);
			if (cancelButton != null)
			{
				cancelButton.onClick.AddListener(Cancel);
			}
		}
		ShowConfirmHotkey();
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, Confirm);
		KeyActionHandler keyActionHandler = new KeyActionHandler(KeyAction.UI_SUBMIT, Confirm, ShowConfirmHotkey, HideConfirmHotkey);
		AddBlockers(keyActionHandler);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(keyActionHandler);
	}

	protected virtual void AddBlockers(KeyActionHandler keyActionHandler)
	{
		keyActionHandler.AddBlocker(new UIWindowOpenKeyActionBlocker(window));
		keyActionHandler.AddBlocker(new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea));
	}

	[UsedImplicitly]
	protected virtual void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.IsInitialized)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, Confirm);
		}
		if (!InputManager.GamePadInUse)
		{
			confirmButton.onClick.RemoveAllListeners();
			if (cancelButton != null)
			{
				cancelButton.onClick.RemoveAllListeners();
			}
		}
		HideConfirmHotkey();
	}

	protected void RemoveSelectBlockerForConfirm()
	{
		Singleton<KeyActionHandlerController>.Instance.RemoveBlockerForHandler(KeyAction.UI_SUBMIT, Confirm, typeof(ExtendedButtonSelectKeyActionHandlerBlocker));
	}

	protected void ShowConfirmHotkey()
	{
		ShowLongConfirm();
		if (InputManager.GamePadInUse)
		{
			confirmHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		else
		{
			confirmHotkey.gameObject.SetActive(value: false);
		}
	}

	protected void HideConfirmHotkey()
	{
		HideLongConfirm();
		if (InputManager.GamePadInUse)
		{
			confirmHotkey.Deinitialize();
			confirmHotkey.DisplayHotkey(active: false);
		}
	}

	protected virtual void ShowLongConfirm()
	{
		if (longConfirmHandler != null)
		{
			longConfirmHandler.gameObject.SetActive(value: true);
		}
	}

	protected virtual void HideLongConfirm()
	{
		if (longConfirmHandler != null)
		{
			longConfirmHandler.gameObject.SetActive(value: false);
		}
	}

	public virtual void Show(bool instant = false)
	{
		if (InputManager.GamePadInUse)
		{
			EnableConfirmationButton(enable: false);
		}
		toggle.isOn = true;
		window.Show(instant);
		controllerArea.OnFocusedArea.RemoveListener(OnControllerAreaFocused);
		controllerArea.OnFocusedArea.AddListener(OnControllerAreaFocused);
		controllerArea.OnUnfocusedArea.RemoveListener(OnControllerAreaUnfocused);
		controllerArea.OnUnfocusedArea.AddListener(OnControllerAreaUnfocused);
		if (controllerArea.IsFocused)
		{
			OnControllerAreaFocused();
		}
		else
		{
			OnControllerAreaUnfocused();
		}
	}

	protected virtual void OnControllerAreaFocused()
	{
		for (int i = 0; i < controllerClickeables.Length; i++)
		{
			controllerClickeables[i].enabled = true;
		}
		for (int j = 0; j < controllerTips.Length; j++)
		{
			controllerTips[j].Show();
		}
	}

	protected virtual void OnControllerAreaUnfocused()
	{
		for (int i = 0; i < controllerClickeables.Length; i++)
		{
			controllerClickeables[i].enabled = false;
		}
		for (int j = 0; j < controllerTips.Length; j++)
		{
			controllerTips[j].Hide();
		}
	}

	public virtual void Hide(bool instant = false)
	{
		toggle.isOn = false;
		window.Hide(instant);
	}

	protected virtual void OnHidden()
	{
		controllerArea.OnFocusedArea.RemoveListener(OnControllerAreaFocused);
		controllerArea.OnUnfocusedArea.RemoveListener(OnControllerAreaUnfocused);
		OnControllerAreaUnfocused();
		toggle.isOn = false;
	}

	protected virtual void Confirm()
	{
		if (window.IsOpen)
		{
			Confirm(ShortPressCallback);
		}
	}

	protected virtual void Confirm(Action shortPressedCallback)
	{
		if (!window.IsOpen)
		{
			return;
		}
		if (InputManager.GamePadInUse && window.IsOpen)
		{
			if (longConfirmHandler != null && longConfirmHandler.gameObject.activeInHierarchy)
			{
				longConfirmHandler.Pressed(Clicked, shortPressedCallback);
			}
		}
		else
		{
			Clicked();
		}
	}

	protected void Clicked()
	{
		Validate(delegate(bool isValid)
		{
			if (isValid)
			{
				OnConfirmedStep();
			}
		});
	}

	protected virtual void OnConfirmedStep()
	{
	}

	protected virtual void Validate(Action<bool> callback)
	{
		callback(obj: true);
	}

	protected virtual void Cancel()
	{
		Hide();
	}

	protected virtual void EnableConfirmationButton(bool enable)
	{
		if (!InputManager.GamePadInUse)
		{
			confirmButton.interactable = enable;
			confirmButton.targetGraphic.material = (enable ? null : UIInfoTools.Instance.disabledGrayscaleMaterial);
			confirmButton.buttonText.CrossFadeColor(enable ? Color.white : UIInfoTools.Instance.greyedOutTextColor, 0f, ignoreTimeScale: true, useAlpha: true);
		}
		confirmHighlight.SetActive(enable);
		confirmControllerTip.ShowInteractable(enable);
	}
}
public abstract class UICharacterCreatorStep<T> : UICharacterCreatorStep
{
	private CallbackPromise<T> promise;

	protected virtual CallbackPromise<T> ProcessStep(bool showImmediatly = false)
	{
		promise = new CallbackPromise<T>();
		Show(showImmediatly);
		return promise;
	}

	protected override void OnConfirmedStep()
	{
		base.OnConfirmedStep();
		promise.Resolve(GetSelectedValue());
	}

	protected override void OnHidden()
	{
		base.OnHidden();
		if (promise.IsPending)
		{
			promise.Cancel();
		}
	}

	protected override void Validate(Action<bool> callback)
	{
		if (GetSelectedValue() == null)
		{
			callback(obj: false);
		}
		else
		{
			base.Validate(callback);
		}
	}

	protected abstract T GetSelectedValue();
}
