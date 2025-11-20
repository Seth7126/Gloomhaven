using System;
using UnityEngine;

public abstract class UIResultsOption : MonoBehaviour
{
	private Action _action;

	public virtual void Register(Action action)
	{
		_action = action;
	}

	public abstract void Unregister();

	public abstract void DisableInteractability();

	public abstract void SetActive(bool active);

	public abstract void HandleStatsPanelStateChanged(bool active);

	protected void InvokeAction()
	{
		_action?.Invoke();
	}
}
