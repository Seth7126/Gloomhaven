#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.ScenarioStates;
using UnityEngine;

public class UIElementPicker : MonoBehaviour
{
	[SerializeField]
	private UIElementPickerSlot[] elementButtons;

	[SerializeField]
	private GameObject content;

	[SerializeField]
	private UiNavigationRoot root;

	private Action<List<ElementInfusionBoardManager.EElement>> onClickCallback;

	private Action<ElementInfusionBoardManager.EElement, bool> onClickElement;

	private List<ElementInfusionBoardManager.EElement> excludeElements;

	private bool isInfusion;

	private int elementsToPick;

	private List<ElementInfusionBoardManager.EElement> pickedElements;

	private IList<ElementInfusionBoardManager.EElement> selectedElementsOnOpen;

	private Action onOpenPicker;

	private Action onClosePicker;

	private ControllerInputAreaCustom controllerArea;

	public bool IsOpen => content.gameObject.activeSelf;

	public void Init(Action<ElementInfusionBoardManager.EElement> onClick, List<ElementInfusionBoardManager.EElement> excludeElements = null, bool isInfusion = false)
	{
		Init(delegate(List<ElementInfusionBoardManager.EElement> elements)
		{
			onClick(elements[0]);
		}, excludeElements, isInfusion);
	}

	public void Init(Action<List<ElementInfusionBoardManager.EElement>> onClick, List<ElementInfusionBoardManager.EElement> excludeElements = null, bool isInfusion = false, int elementsToPick = 1, Action<ElementInfusionBoardManager.EElement, bool> onClickElement = null, Action onShow = null, Action onHide = null, IList<ElementInfusionBoardManager.EElement> selectedElements = null)
	{
		onClosePicker = onHide;
		onOpenPicker = onShow;
		this.isInfusion = isInfusion;
		onClickCallback = onClick;
		this.onClickElement = onClickElement;
		this.excludeElements = excludeElements;
		this.elementsToPick = elementsToPick;
		pickedElements = new List<ElementInfusionBoardManager.EElement>();
		selectedElementsOnOpen = selectedElements;
		RefreshAvailable();
	}

	public void SetInteractable(ElementInfusionBoardManager.EElement element, bool interactable)
	{
		elementButtons[(int)element].SetInteractable(interactable && (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl));
	}

	public void Show(GameObject holder = null)
	{
		controllerArea = new ControllerInputAreaCustom("Element Picker", OnFocused, OnUnfocused, stackArea: true);
		pickedElements.Clear();
		RefreshAvailable();
		if (content.activeSelf)
		{
			return;
		}
		Debug.Log("show picker");
		content.SetActive(value: true);
		onOpenPicker?.Invoke();
		if (holder != null)
		{
			UIManager.Instance.HighlightElement(holder, fadeEverythingElse: false, lockUI: false);
		}
		controllerArea.Focus();
		if (selectedElementsOnOpen == null)
		{
			return;
		}
		foreach (ElementInfusionBoardManager.EElement item in selectedElementsOnOpen)
		{
			Select(item, notifyWhenAllPicked: false);
		}
	}

	public void RefreshAvailable()
	{
		for (int i = 0; i < elementButtons.Length; i++)
		{
			bool flag = (excludeElements == null || !excludeElements.Contains((ElementInfusionBoardManager.EElement)i)) && (isInfusion || (ElementInfusionBoardManager.ElementColumn((ElementInfusionBoardManager.EElement)i) != ElementInfusionBoardManager.EColumn.Inert && !InfusionBoardUI.Instance.IsElementReserved((ElementInfusionBoardManager.EElement)i)));
			elementButtons[i].SetInteractable(!flag || Choreographer.s_Choreographer.ThisPlayerHasTurnControl);
			if (flag)
			{
				elementButtons[i].Show(delegate(ElementInfusionBoardManager.EElement items)
				{
					Select(items);
				});
			}
			else
			{
				elementButtons[i].Hide();
			}
		}
		if (controllerArea != null && controllerArea.IsFocused)
		{
			OnFocused();
		}
	}

	private void OnFocused()
	{
		if (root != null)
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(ScenarioStateTag.SelectElementItem);
			Singleton<UINavigation>.Instance.NavigationManager.SetCurrentRoot(root);
		}
	}

	private void OnUnfocused()
	{
		bool flag = false;
		for (int i = 0; i < elementButtons.Length; i++)
		{
			if (Singleton<UINavigation>.Instance.NavigationManager.CurrentlySelectedElement == elementButtons[i].Selectable)
			{
				flag = true;
			}
		}
		if (flag)
		{
			Hide();
		}
	}

	public void Hide(GameObject holder)
	{
		if (content.activeSelf)
		{
			Hide();
			if (holder != null)
			{
				UIManager.Instance.UnhighlightElement(holder, unlockUI: false);
			}
		}
	}

	public void Hide()
	{
		if (content.activeSelf)
		{
			content.SetActive(value: false);
			controllerArea?.Destroy();
			controllerArea = null;
			onClosePicker?.Invoke();
			if (InputManager.GamePadInUse)
			{
				Singleton<UIUseItemsBar>.Instance.ControllerInputItemsArea.Focus();
			}
			else if (Singleton<UIUseItemsBar>.Instance.ItemSlots.Count != 0)
			{
				Singleton<UIUseItemsBar>.Instance.SetActiveItemButtons(Singleton<UIUseItemsBar>.Instance.IsCurrentItemReadyToUse());
			}
		}
	}

	protected void OnDisable()
	{
		OnUnfocused();
		pickedElements.Clear();
		controllerArea?.Destroy();
		controllerArea = null;
	}

	public void Select(ElementInfusionBoardManager.EElement element, bool notifyWhenAllPicked = true)
	{
		if (pickedElements.Contains(element))
		{
			pickedElements.Remove(element);
			elementButtons[(int)element].SetSelected(selected: false);
			onClickElement?.Invoke(element, arg2: false);
		}
		else if (pickedElements.Count != elementsToPick)
		{
			elementButtons[(int)element].SetSelected(selected: true);
			pickedElements.Add(element);
			onClickElement?.Invoke(element, arg2: true);
			if (pickedElements.Count == elementsToPick && notifyWhenAllPicked)
			{
				onClickCallback?.Invoke(pickedElements.ToList());
			}
		}
	}
}
