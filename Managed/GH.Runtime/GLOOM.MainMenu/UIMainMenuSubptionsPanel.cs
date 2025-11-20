using System;
using System.Collections.Generic;
using SM.Gamepad;
using Script.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UIMainMenuSubptionsPanel : MonoBehaviour
{
	[SerializeField]
	private UIMainMenuSuboption optionButtonPrefab;

	[SerializeField]
	private ExtendedScrollRect optionsScroll;

	[SerializeField]
	private ToggleGroup toggleGroup;

	[SerializeField]
	private LocalHotkeys hotkeys;

	[SerializeField]
	private List<UIMainMenuSuboption> optionsPool;

	[SerializeField]
	private CanvasGroup canvasGroupInteraction;

	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private Action onHidden;

	private UIWindow window;

	private UIMainMenuSuboption hoveredSlot;

	private IHotkeySession hotkeySession;

	private SessionHotkey backHotkey;

	private SessionHotkey selectHotkey;

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden && window.HasGoneToStartingState)
			{
				controllerArea.Destroy();
				onHidden();
			}
		});
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		hotkeySession = hotkeys.GetSessionOrEmpty().GetHotkey(out selectHotkey, "Select").GetHotkey(out backHotkey, "Back");
	}

	public void SetInteractable(bool interactable)
	{
		if (window.IsOpen)
		{
			canvasGroupInteraction.blocksRaycasts = interactable;
		}
	}

	public void Show(List<MenuSuboption> options, RectTransform targetPointer, Action onHidden)
	{
		this.onHidden = onHidden;
		verticalPointer.PointAt(targetPointer);
		HelperTools.NormalizePool(ref optionsPool, optionButtonPrefab.gameObject, optionsScroll.content, options.Count);
		optionsScroll.ScrollToTop();
		for (int i = 0; i < options.Count; i++)
		{
			MenuSuboption option = options[i];
			UIMainMenuSuboption slot = optionsPool[i];
			slot.Init(option, toggleGroup, delegate
			{
				OnSelectSlot(option);
			}, delegate
			{
				OnDeselectSlot(option);
			}, delegate(bool hovered)
			{
				OnHoveredSlot(slot, hovered);
			});
			UpdateSlotSelectability(slot);
			option.RegisterOnUpdatedInteraction(delegate(bool interact)
			{
				slot.IsInteractable = interact;
				UpdateSlotSelectability(slot);
			});
			option.RegisterOnToggledSelect(slot.ToggleSelect);
			option.RegisterOnClearFrame(slot.ClearFrame);
			if (controllerArea.IsFocused)
			{
				slot.EnableNavigation();
				if (slot.IsSelected || i == 0)
				{
					_ = slot.gameObject;
				}
			}
		}
		window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		window.ShowOrUpdateStartingState();
		controllerArea.Enable();
	}

	private void UpdateSlotSelectability(UIMainMenuSuboption slot)
	{
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		UINavigationSelectable component = slot.GetComponent<UINavigationSelectable>();
		if (!(component == null))
		{
			if (slot.IsInteractable && !component.Root.Elements.Contains(component))
			{
				component.Root.Elements.Add(component);
			}
			else if (!slot.IsInteractable)
			{
				component.Root.Elements.Remove(component);
			}
		}
	}

	private void OnDeselectSlot(MenuSuboption option)
	{
		window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		SetFocused(isFocused: true);
		controllerArea.Focus();
		option.OnDeselected();
	}

	private void OnSelectSlot(MenuSuboption option)
	{
		window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
		option.OnSelected();
		SetFocused(isFocused: false);
	}

	private void OnHoveredSlot(UIMainMenuSuboption option, bool hovered)
	{
		if (hovered)
		{
			hoveredSlot = option;
		}
		else
		{
			hoveredSlot = null;
		}
		UpdateHotkeys();
	}

	private void UpdateHotkeys()
	{
		if (hoveredSlot == null)
		{
			selectHotkey.SetShown(shown: false);
		}
		else
		{
			selectHotkey.SetShown(hoveredSlot.IsInteractable);
		}
	}

	public void DeselectCurrentSlot()
	{
		toggleGroup.SetAllTogglesOff();
	}

	private void SetFocused(bool isFocused)
	{
		for (int i = 0; i < optionsPool.Count && optionsPool[i].gameObject.activeSelf; i++)
		{
			optionsPool[i].SetFocused(isFocused);
		}
	}

	public void Hide()
	{
		if (window.IsOpen)
		{
			DeselectCurrentSlot();
			window.Hide();
		}
	}

	private void EnableNavigation()
	{
		for (int i = 0; i < optionsPool.Count; i++)
		{
			UIMainMenuSuboption uIMainMenuSuboption = optionsPool[i];
			if (uIMainMenuSuboption.IsSelected || i == 0)
			{
				_ = uIMainMenuSuboption.gameObject;
			}
			if (!uIMainMenuSuboption.gameObject.activeSelf)
			{
				break;
			}
			uIMainMenuSuboption.EnableNavigation();
		}
		backHotkey.Show();
	}

	private void DisableNavigation()
	{
		for (int i = 0; i < optionsPool.Count; i++)
		{
			UIMainMenuSuboption uIMainMenuSuboption = optionsPool[i];
			if (!uIMainMenuSuboption.gameObject.activeSelf)
			{
				break;
			}
			uIMainMenuSuboption.DisableNavigation();
		}
		hoveredSlot = null;
		selectHotkey.Hide();
		backHotkey.Hide();
	}
}
