using System;
using UnityEngine;

public class UIResultsButtonOption : UIResultsOption
{
	[SerializeField]
	private ExtendedButton _button;

	public override void Register(Action action)
	{
		base.Register(action);
		_button.onClick.RemoveAllListeners();
		_button.onClick.AddListener(base.InvokeAction);
	}

	public override void Unregister()
	{
		_button.onClick.RemoveAllListeners();
	}

	public override void DisableInteractability()
	{
		_button.interactable = false;
		_button.ToggleFade(active: true);
	}

	public override void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public override void HandleStatsPanelStateChanged(bool active)
	{
	}
}
