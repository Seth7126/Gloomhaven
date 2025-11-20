using System;
using UnityEngine;
using UnityEngine.Events;

namespace Script.GUI.SMNavigation.Input;

public abstract class InputListener : MonoBehaviour
{
	[SerializeField]
	private bool _autoRegister = true;

	[SerializeField]
	private UnityEvent _onNextUnityEvent;

	[SerializeField]
	private UnityEvent _onPreviousUnityEvent;

	public event Action OnNext;

	public event Action OnPrevious;

	protected virtual void Awake()
	{
		if (_autoRegister)
		{
			Register();
		}
	}

	protected virtual void Next()
	{
		this.OnNext?.Invoke();
		_onNextUnityEvent?.Invoke();
	}

	protected virtual void Previous()
	{
		this.OnPrevious?.Invoke();
		_onPreviousUnityEvent?.Invoke();
	}

	public abstract void Register();

	public abstract void UnRegister();

	private void OnDestroy()
	{
		UnRegister();
	}
}
