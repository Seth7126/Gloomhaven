#define ENABLE_LOGS
using System;
using System.Collections;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.Popups;

[RequireComponent(typeof(LongConfirm))]
public abstract class LongPressHandlerBase : MonoBehaviour
{
	[SerializeField]
	private float _waitForStartLongConfirm;

	[SerializeField]
	private bool _shortPressOnAnyTiming;

	private LongConfirm _longConfirm;

	private bool _isButtonPressed;

	private bool _isShortPressed;

	private Action _shortPressCallback;

	private WaitForSecondsRealtime _waitForSeconds;

	public bool IsActive => base.gameObject.activeInHierarchy;

	protected abstract KeyAction KeyActionToWait { get; }

	private void Awake()
	{
		_waitForSeconds = new WaitForSecondsRealtime(_waitForStartLongConfirm);
		if (_longConfirm == null)
		{
			_longConfirm = GetComponent<LongConfirm>();
		}
	}

	private void OnDestroy()
	{
		StopCheckingReleasedButton();
		StopAllCoroutines();
	}

	private void OnDisable()
	{
		StopCheckingReleasedButton();
	}

	public virtual void Pressed(Action longPressedCallback, Action shortPressedCallback = null)
	{
		if (_longConfirm == null)
		{
			_longConfirm = GetComponent<LongConfirm>();
		}
		if (!_isButtonPressed && !_longConfirm.WasPressed && InputManager.GetWasPressed(KeyActionToWait))
		{
			Debug.Log("[LongConfirm] " + base.gameObject.name + " -> Pressed");
			EnableLongConfirmInput();
			_isButtonPressed = true;
			_isShortPressed = false;
			_shortPressCallback = shortPressedCallback;
			if (!_shortPressOnAnyTiming)
			{
				StartCoroutine(CheckShortPressWithTiming());
			}
			_longConfirm.SetOnConfirmed(delegate
			{
				LongPressed(longPressedCallback);
			});
			_longConfirm.OnKeyPressed();
		}
	}

	private void EnableLongConfirmInput()
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All, KeyActionToWait);
		InputManager.RequestDisableInput(this, KeyAction.UI_SUBMIT);
		InputManager.RegisterToOnReleased(KeyActionToWait, OnReleased);
	}

	private void DisableLongConfirmInput()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		InputManager.UnregisterToOnReleased(KeyActionToWait, OnReleased);
	}

	public void SetActiveLongConfirmButton(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}

	public void ResetShortPressedCallback()
	{
		_shortPressCallback = null;
	}

	private void OnReleased()
	{
		if (_longConfirm.WasPressed)
		{
			_longConfirm.ClearOnConfirmed();
			_longConfirm.OnKeyReleased();
			if (_shortPressOnAnyTiming)
			{
				Debug.Log("[LongConfirm] " + base.gameObject.name + " -> ShortPressed from OnKeyReleased");
				ShortPressed();
			}
		}
		StopCheckingReleasedButton();
	}

	private void LongPressed(Action callback)
	{
		Debug.Log("[LongConfirm] " + base.gameObject.name + " -> LongPressed");
		StopCheckingReleasedButton();
		callback?.Invoke();
	}

	protected virtual void ShortPressed()
	{
		if (!_isShortPressed)
		{
			_isShortPressed = true;
			_shortPressCallback?.Invoke();
		}
	}

	private void StopCheckingReleasedButton()
	{
		Debug.Log("[LongConfirm] " + base.gameObject.name + " -> Stop checking released button");
		InputManager.SkipNextSubmitAction();
		DisableLongConfirmInput();
		_isButtonPressed = false;
	}

	private IEnumerator CheckShortPressWithTiming()
	{
		yield return _waitForSeconds;
		if (!_isButtonPressed)
		{
			StopCheckingReleasedButton();
			ShortPressed();
			Debug.Log("[LongConfirm] " + base.gameObject.name + " -> ShortPressed from waiting coroutine");
		}
	}
}
