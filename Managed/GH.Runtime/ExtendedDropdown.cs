using System;
using System.Collections.Generic;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExtendedDropdown : TMP_Dropdown, IEscapable, IShowActivity
{
	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	public UnityEvent OnCreatedOptions;

	public UnityEvent OnSelected;

	public UnityEvent OnDeselected;

	public UnityEvent OnClosed;

	public Action<GameObject, Transform> OnDropdownCreated;

	private ControllerInputAreaCustom controllerArea;

	private List<DropdownItem> items = new List<DropdownItem>();

	private ScrollRect scroll;

	private Button _button;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public Action OnShow { get; set; }

	public Action OnHide { get; set; }

	public Action<bool> OnActivityChanged { get; set; }

	public new bool IsActive => base.gameObject.activeSelf;

	public Selectable CurrentSelectable { get; protected set; }

	protected override GameObject CreateBlocker(Canvas rootCanvas)
	{
		GameObject gameObject = base.CreateBlocker(rootCanvas);
		if (_button != null)
		{
			_button.onClick.RemoveListener(Deselect);
		}
		_button = gameObject.GetComponent<Button>();
		_button.onClick.AddListener(Deselect);
		UIWindowManager.RegisterEscapable(this);
		controllerArea = new ControllerInputAreaCustom("Dropdown", OnControllerFocused, OnControllerUnfocused, stackArea: true);
		controllerArea.BlockOthersFocusWhileIsFocused = true;
		controllerArea.Focus();
		OnCreatedOptions?.Invoke();
		return gameObject;
	}

	protected override void OnDestroy()
	{
		if (_button != null)
		{
			_button.onClick.RemoveListener(Deselect);
		}
		OnCreatedOptions.RemoveAllListeners();
		OnSelected.RemoveAllListeners();
		OnDeselected.RemoveAllListeners();
		OnClosed.RemoveAllListeners();
		OnDropdownCreated = null;
		foreach (DropdownItem item in items)
		{
			((ExtendedToggle)item.toggle).onSelected.RemoveAllListeners();
		}
		_button = null;
		items.Clear();
		controllerArea = null;
		CurrentSelectable = null;
		OnShow = null;
		OnHide = null;
		OnActivityChanged = null;
		base.OnDestroy();
	}

	protected override void DestroyDropdownList(GameObject dropdownList)
	{
		scroll = null;
		UIWindowManager.UnregisterEscapable(this);
		base.DestroyDropdownList(dropdownList);
		controllerArea.Destroy();
		OnClosed?.Invoke();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		OnSelected.Invoke();
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		OnDeselected.Invoke();
	}

	protected override GameObject CreateDropdownList(GameObject template)
	{
		GameObject gameObject = base.CreateDropdownList(template);
		scroll = gameObject.GetComponent<ScrollRect>();
		OnDropdownCreated?.Invoke(gameObject, gameObject.GetComponentInChildren<DropdownItem>().transform.parent);
		return gameObject;
	}

	protected override DropdownItem CreateItem(DropdownItem itemTemplate)
	{
		DropdownItem item = base.CreateItem(itemTemplate);
		items.Add(item);
		((ExtendedToggle)item.toggle).onSelected.AddListener(delegate
		{
			scroll.ScrollVerticallyToTop(item.rectTransform);
		});
		return item;
	}

	protected override void DestroyItem(DropdownItem item)
	{
		items.Remove(item);
		base.DestroyItem(item);
	}

	protected override void OnDisable()
	{
		UIWindowManager.UnregisterEscapable(this);
		base.OnDisable();
	}

	private void Deselect()
	{
		if (EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	private void OnControllerFocused()
	{
		Toggle toggle = null;
		foreach (DropdownItem item in items)
		{
			if (item.toggle.isOn)
			{
				toggle = item.toggle;
			}
		}
		if (toggle == null)
		{
			items[0].toggle.Select();
		}
		else
		{
			toggle.Select();
			scroll.ScrollVerticallyToTop(toggle.transform as RectTransform);
		}
		CurrentSelectable = toggle;
		OnShow?.Invoke();
		OnActivityChanged?.Invoke(obj: true);
	}

	private void OnControllerUnfocused()
	{
		for (int i = 0; i < items.Count; i++)
		{
			items[i].toggle.DisableNavigation();
		}
		OnHide?.Invoke();
		OnActivityChanged?.Invoke(obj: false);
	}

	public bool Escape()
	{
		Hide();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
