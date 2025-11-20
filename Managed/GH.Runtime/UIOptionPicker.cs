using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIOptionPicker : MonoBehaviour
{
	[SerializeField]
	private UIPickerSlot optionPrefab;

	[SerializeField]
	private List<UIPickerSlot> optionButtons;

	[SerializeField]
	private GameObject content;

	private Action<List<IOption>, bool> onClickCallback;

	private Action<IOption, bool> onClickElement;

	private int elementsToPick;

	private List<IOption> pickedElements;

	private Dictionary<string, UIPickerSlot> slots = new Dictionary<string, UIPickerSlot>();

	private Action<IOption, bool> onHovered;

	private Action onOpenPicker;

	private Action onClosePicker;

	private ControllerInputAreaCustom controllerArea;

	public bool IsOpen => content.gameObject.activeSelf;

	public void Init(List<IOption> options, Action<List<IOption>, bool> onClick, int elementsToPick = 1, Action<IOption, bool> onClickElement = null, Action onShow = null, Action onHide = null, Action<IOption, bool> onHovered = null)
	{
		onClosePicker = onHide;
		onOpenPicker = onShow;
		onClickCallback = onClick;
		this.onClickElement = onClickElement;
		this.elementsToPick = elementsToPick;
		this.onHovered = onHovered;
		pickedElements = new List<IOption>();
		slots.Clear();
		HelperTools.NormalizePool(ref optionButtons, optionPrefab.gameObject, optionButtons[0].transform.parent, options.Count);
		for (int i = 0; i < options.Count; i++)
		{
			IOption option = options[i];
			slots[option.ID] = optionButtons[i];
			optionButtons[i].Init(option.GetPickerText(), option.GetPickerIcon(), delegate
			{
				Select(option);
			}, delegate(bool hovered)
			{
				onHovered?.Invoke(option, hovered);
			});
		}
	}

	public void Show(GameObject holder = null)
	{
		pickedElements.Clear();
		if (!content.activeSelf)
		{
			content.SetActive(value: true);
			onOpenPicker?.Invoke();
			if (holder != null)
			{
				UIManager.Instance.HighlightElement(holder, fadeEverythingElse: false, lockUI: false);
			}
			controllerArea = new ControllerInputAreaCustom("Option Picker", EnableNavigation, DisableNavigation, stackArea: true);
			controllerArea.Focus();
		}
	}

	private void EnableNavigation()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			optionButtons[i].Selectable.SetNavigation(null, null, optionButtons[(i == 0) ? (slots.Count - 1) : (i - 1)].Selectable, optionButtons[(i + 1) % slots.Count].Selectable);
		}
		EventSystem.current.SetSelectedGameObject(optionButtons[0].gameObject);
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
	}

	private void DisableNavigation()
	{
		bool flag = false;
		foreach (UIPickerSlot value in slots.Values)
		{
			if (EventSystem.current.currentSelectedGameObject == value.gameObject)
			{
				flag = true;
			}
			value.Selectable.DisableNavigation();
		}
		if (flag)
		{
			Hide();
		}
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
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
			controllerArea?.Destroy();
			controllerArea = null;
			content.SetActive(value: false);
			onClosePicker?.Invoke();
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			pickedElements.Clear();
			controllerArea?.Destroy();
			controllerArea = null;
			DisableNavigation();
		}
	}

	public void Select(IOption option)
	{
		if (pickedElements.Contains(option))
		{
			pickedElements.Remove(option);
			slots[option.ID].SetSelected(selected: false);
			onClickElement?.Invoke(option, arg2: false);
			if (elementsToPick < 0)
			{
				onClickCallback(pickedElements.ToList(), arg2: true);
			}
		}
		else
		{
			slots[option.ID].SetSelected(selected: true);
			pickedElements.Add(option);
			onClickElement?.Invoke(option, arg2: true);
			if (elementsToPick < 0 || pickedElements.Count == elementsToPick)
			{
				onClickCallback(pickedElements.ToList(), arg2: false);
			}
		}
	}
}
