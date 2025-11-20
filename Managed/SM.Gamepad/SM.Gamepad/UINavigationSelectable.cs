using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SM.Gamepad;

public class UINavigationSelectable : UiNavigationBase, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IUiNavigationSelectable, IUiNavigationElement
{
	[SerializeField]
	private Selectable _selectable;

	[Space(20f)]
	[SerializeField]
	private UnityEvent _onNavigationSelectedUnityEvent;

	[SerializeField]
	private UnityEvent _onNavigationDeselectedUnityEvent;

	public Selectable ControlledSelectable
	{
		get
		{
			if (_selectable == null)
			{
				_selectable = GetComponent<Selectable>();
			}
			return _selectable;
		}
	}

	public event Action<IUiNavigationSelectable> OnPointerEnterEvent;

	public event Action<IUiNavigationSelectable> OnPointerExitEvent;

	public event Action<IUiNavigationSelectable> OnNavigationSelectedEvent;

	public event Action<IUiNavigationSelectable> OnNavigationDeselectedEvent;

	public void InitializeSelectableElement(IUiNavigationManager uiNavigationManager, IUiNavigationRoot root, HashSet<IUiNavigationElement> knownElements, Dictionary<string, IUiNavigationElement> elementsByNameMap, Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> knownSelectablesMap)
	{
		InitializeBaseElement(uiNavigationManager, root, elementsByNameMap, knownSelectablesMap);
		if (!knownElements.Contains(this))
		{
			knownElements.Add(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this.OnPointerEnterEvent?.Invoke(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.OnPointerExitEvent?.Invoke(this);
	}

	public void OnNavigationSelected()
	{
		this.OnNavigationSelectedEvent?.Invoke(this);
		_onNavigationSelectedUnityEvent?.Invoke();
		InvokeOnEnterNavigationActions();
	}

	public void OnNavigationDeselected()
	{
		this.OnNavigationDeselectedEvent?.Invoke(this);
		_onNavigationDeselectedUnityEvent?.Invoke();
		InvokeOnExitNavigationActions();
	}

	private void InvokeOnEnterNavigationActions()
	{
		InvokeNavigationActions(OnNavigationEnterExitArgActions);
		InvokeNavigationActions(OnNavigationEnterArgActions);
	}

	private void InvokeOnExitNavigationActions()
	{
		InvokeNavigationActions(OnNavigationEnterExitArgActions);
		InvokeNavigationActions(OnNavigationExitArgActions);
	}

	private void InvokeNavigationActions(ArgNavigationAction[] argActions)
	{
		if (argActions == null)
		{
			return;
		}
		for (int i = 0; i < argActions.Length; i++)
		{
			ArgNavigationAction argNavigationAction = argActions[i];
			if (!(argNavigationAction.NavigationAction == null))
			{
				argNavigationAction.NavigationAction.HandleAction(new NavigationAction.NavigationActionArgs
				{
					NavigationElement = this,
					Components = argNavigationAction.Components
				});
			}
		}
	}
}
