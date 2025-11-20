using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AsmodeeNet.Utils;

public class DelayedInputFieldEvent : MonoBehaviour
{
	[Serializable]
	public class OnValueChanged : UnityEvent<string>
	{
	}

	[SerializeField]
	private float _timeOffset = 0.3f;

	[SerializeField]
	private OnValueChanged _onValueChanged;

	private Coroutine _coroutine;

	private float _timeLeft;

	private string _message;

	public void NewInput(string text)
	{
		_message = text;
		_timeLeft = _timeOffset;
		if (_coroutine == null)
		{
			_coroutine = StartCoroutine(DelayEvent());
		}
	}

	private IEnumerator DelayEvent()
	{
		while (_timeLeft > 0f)
		{
			yield return null;
			_timeLeft -= Time.deltaTime;
		}
		_coroutine = null;
		_onValueChanged.Invoke(_message);
	}
}
