using System.Collections;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using UnityEngine;

namespace Script.GUI.GameScreen;

public class FastForwardButton : Singleton<FastForwardButton>
{
	[SerializeField]
	private Hotkey _hotkey;

	[SerializeField]
	private GameObject _pressedSprite;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private float _appearanceDelay;

	[SerializeField]
	private float _alphaAnimDuration;

	private float _startAlphaAnim;

	private Coroutine _setActiveCoroutine;

	protected override void Awake()
	{
		base.Awake();
		_startAlphaAnim = _appearanceDelay - _alphaAnimDuration;
	}

	private void OnEnable()
	{
		_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		Toggle(active: false);
		InputManager.RegisterToOnPressed(KeyAction.SKIP_ATTACK, OnPressed);
		InputManager.RegisterToOnReleased(KeyAction.SKIP_ATTACK, OnReleased);
	}

	private void OnDisable()
	{
		StopSetActiveProcess();
		OnReleased();
		_hotkey.Deinitialize();
		InputManager.UnregisterToOnPressed(KeyAction.SKIP_ATTACK, OnPressed);
		InputManager.UnregisterToOnReleased(KeyAction.SKIP_ATTACK, OnReleased);
	}

	public void Toggle(bool active)
	{
		StopSetActiveProcess();
		if (active)
		{
			_setActiveCoroutine = StartCoroutine(SetActiveProcess());
		}
	}

	private void OnPressed()
	{
		_pressedSprite.SetActive(value: true);
	}

	private void OnReleased()
	{
		_pressedSprite.SetActive(value: false);
	}

	private void StopSetActiveProcess()
	{
		if (_setActiveCoroutine != null)
		{
			StopCoroutine(_setActiveCoroutine);
			_setActiveCoroutine = null;
		}
		_canvasGroup.alpha = 0f;
	}

	private IEnumerator SetActiveProcess()
	{
		float time = 0f;
		while (time < _appearanceDelay)
		{
			time += Time.deltaTime;
			if (time >= _startAlphaAnim)
			{
				_canvasGroup.alpha = (time - _startAlphaAnim) / _alphaAnimDuration;
			}
			yield return null;
		}
		_canvasGroup.alpha = 1f;
	}
}
