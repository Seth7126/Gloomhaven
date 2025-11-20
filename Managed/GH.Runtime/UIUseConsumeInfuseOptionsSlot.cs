using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using ScenarioRuleLibrary;
using UnityEngine;

public abstract class UIUseConsumeInfuseOptionsSlot<T> : UIUseConsumeInfuseSlot<T>
{
	[Header("Extra option")]
	[SerializeField]
	private UIOptionPicker optionPicker;

	[SerializeField]
	protected List<UIUseOption> optionsUI;

	protected IOptionHolder optionHolder;

	private PickController optionPickerController;

	public List<IOption> SelectedOptions => optionHolder?.SelectedOptions;

	protected override void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			base.OnDisable();
			optionPickerController?.ClosePicker();
		}
	}

	public override void Select()
	{
		if (optionPickerController != null)
		{
			if (optionPickerController.IsSelecting())
			{
				optionPickerController.ClosePicker();
				return;
			}
			if (!optionPickerController.Pick())
			{
				return;
			}
		}
		base.Select();
	}

	public void SelectAll(List<ElementInfusionBoardManager.EElement> elements)
	{
		infusePickerController.OnSelectedAll(elements);
	}

	protected void Init(CActor actor, T element, Action<T> onSelect = null, Action<T> onUnselect = null, Func<T, bool> isMandatoryChecker = null, bool isSelected = false, List<IInfuseElement> infusions = null, List<ElementInfusionBoardManager.EElement> consumes = null, Tuple<IOptionHolder, List<IOption>> option = null, Action<CItem> onPickedAll = null)
	{
		Init(actor, element, onSelect, onUnselect, isMandatoryChecker, isSelected, infusions, consumes, onPickedAll);
		CreateOptions(option);
	}

	public override void ClearSelection(bool fromClick = false)
	{
		if (selected)
		{
			base.ClearSelection(fromClick: false);
			optionPickerController?.ClosePicker();
			optionHolder?.ClearSelection();
		}
	}

	public override void Unselect()
	{
		base.Unselect();
		optionPickerController?.ClosePicker();
	}

	protected void CreateOptions(Tuple<IOptionHolder, List<IOption>> option)
	{
		optionHolder = null;
		optionPickerController = null;
		if (option == null)
		{
			for (int i = 0; i < optionsUI.Count; i++)
			{
				optionsUI[i].Hide();
			}
			return;
		}
		optionHolder = option.Item1;
		if (optionHolder is ISingleOptionHolder elementUI)
		{
			optionPickerController = new SingleOptionPickController(optionPicker).SetOnSelected(delegate
			{
				Select();
			}).SetOnClosePicker(OnClosePicker).SetOnOpenPicker(OnOpenPicker);
			((SingleOptionPickController)optionPickerController).Setup(elementUI, option.Item2);
		}
		else
		{
			optionPickerController = new OptionalOptionPickController(optionPicker).SetOnSelected(SelectOptions).SetOnHovered(optionHolder.OnHoveredOption).SetOnClosePicker(OnClosePicker)
				.SetOnOpenPicker(OnOpenPicker);
			((OptionalOptionPickController)optionPickerController).Setup(optionHolder, option.Item2);
		}
	}

	private void SelectOptions(List<IOption> options, bool isDeselection)
	{
		if (isDeselection && options.Count == 0)
		{
			optionPickerController.InitializePicker();
			Deselect();
		}
		else if (optionPickerController.Pick())
		{
			base.Select();
		}
	}

	protected override void CheckShowTooltip()
	{
		if (base.gameObject.activeSelf)
		{
			if (optionPicker != null && optionPicker.IsOpen)
			{
				ShowTooltip(show: false);
			}
			else
			{
				base.CheckShowTooltip();
			}
		}
	}

	public void SetSelectedOptions(List<string> optionsID)
	{
		if (optionsID != null && optionsID.Count > 0 && optionPickerController != null)
		{
			if (optionPickerController is SingleOptionPickController singleOptionPickController)
			{
				singleOptionPickController.SelectOption(optionsID[0]);
			}
			else if (optionPickerController is OptionalOptionPickController optionalOptionPickController)
			{
				optionalOptionPickController.SelectOptions(optionsID);
			}
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (optionPickerController != null && optionPickerController.IsSelecting())
		{
			optionPickerController.ClosePicker();
		}
	}
}
