using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SM.Gamepad;

public class LongConfirm : MonoBehaviour
{
	[Tooltip("Image that will be filled")]
	[SerializeField]
	private Image _progressCircle;

	[SerializeField]
	private float _holdTime = 0.65f;

	[Tooltip("Delay before HoldTime will start count")]
	[SerializeField]
	private float _holdDelayTime = 0.11f;

	public Action OnKeyPressed;

	public Action OnKeyReleased;

	private float _currentTime;

	private Coroutine _waitCoroutine;

	public bool WasPressed { get; private set; }

	private event Action OnConfirmed;

	private void OnEnable()
	{
		OnKeyPressed = (Action)Delegate.Combine(OnKeyPressed, new Action(OnPressed));
		OnKeyReleased = (Action)Delegate.Combine(OnKeyReleased, new Action(OnReleased));
		SetProgressCircleActive(value: false);
	}

	private void OnDisable()
	{
		OnKeyPressed = (Action)Delegate.Remove(OnKeyPressed, new Action(OnPressed));
		OnKeyReleased = (Action)Delegate.Remove(OnKeyReleased, new Action(OnReleased));
		SetProgressCircleActive(value: false);
		OnReleased();
	}

	private void OnPressed()
	{
		if (!WasPressed)
		{
			SetProgressCircleActive(value: true);
			_waitCoroutine = StartCoroutine(WaitLongConfirm());
		}
	}

	private void OnReleased()
	{
		if (_waitCoroutine != null)
		{
			StopCoroutine(_waitCoroutine);
			SetProgressCircleActive(value: false);
		}
	}

	private IEnumerator WaitLongConfirm()
	{
		yield return new WaitForSeconds(_holdDelayTime);
		while (_currentTime < _holdTime)
		{
			_currentTime += Time.deltaTime;
			UpdateFillAmount(_currentTime / _holdTime);
			yield return null;
		}
		this.OnConfirmed?.Invoke();
		SetProgressCircleActive(value: false);
	}

	private void SetProgressCircleActive(bool value)
	{
		WasPressed = value;
		_currentTime = 0f;
		_progressCircle.gameObject.SetActive(value);
		UpdateFillAmount(0f);
	}

	private void UpdateFillAmount(float amount)
	{
		_progressCircle.fillAmount = amount;
	}

	public void SetOnConfirmed(Action callback, bool doClear = true)
	{
		if (doClear)
		{
			ClearOnConfirmed();
		}
		this.OnConfirmed = callback;
	}

	public void ClearOnConfirmed()
	{
		this.OnConfirmed = null;
	}
}
