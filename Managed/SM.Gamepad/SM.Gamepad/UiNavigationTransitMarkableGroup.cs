using System;
using UnityEngine;
using UnityEngine.Events;

namespace SM.Gamepad;

public class UiNavigationTransitMarkableGroup : UiNavigationGroup, IUiNavigationTransitMarkableNode, IUiNavigationNode, IUiNavigationElement
{
	[Space(20f)]
	[SerializeField]
	private UnityEvent _onTransitMarkedUnityEvent;

	[SerializeField]
	private UnityEvent _onTransitUnmarkedUnityEvent;

	public event Action<IUiNavigationSelectable> OnTransitMarkedEvent;

	public event Action<IUiNavigationSelectable> OnTransitUnmarkedEvent;

	public void OnNavigationTransitMarked(IUiNavigationSelectable selectedChild)
	{
		this.OnTransitMarkedEvent?.Invoke(selectedChild);
		_onTransitMarkedUnityEvent?.Invoke();
	}

	public void OnNavigationTransitUnmarked(IUiNavigationSelectable deselectedChild)
	{
		this.OnTransitUnmarkedEvent?.Invoke(deselectedChild);
		_onTransitUnmarkedUnityEvent?.Invoke();
	}
}
