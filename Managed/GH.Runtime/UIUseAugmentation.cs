using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIUseAugmentation : UIUseSlot<IAugmentation>
{
	[SerializeField]
	private UIUseAugmentationElement augmentationPrefab;

	[SerializeField]
	private List<UIUseAugmentationElement> augmentations;

	[SerializeField]
	private UIUsePreview previewEffect;

	[SerializeField]
	private UIAugmentTooltip tooltip;

	[SerializeField]
	private UIElementPicker elementPicker;

	[SerializeField]
	private UIOptionPicker optionPicker;

	[SerializeField]
	private UIElementHighlight highlight;

	[SerializeField]
	private Color defaultHighlightColor;

	private PickController pickerController;

	private Action<IAugmentation, bool> onHovered;

	private bool hovered;

	private List<UseAugmentationElementController> augmentationElementControllers = new List<UseAugmentationElementController>();

	private UseAugmentationOptionController augmentationOptionController;

	protected virtual void Awake()
	{
		tooltip.gameObject.SetActive(value: false);
		button.onClick.AddListener(OnPointerDown);
		button.onMouseEnter.AddListener(OnPointerEnter);
		button.onMouseExit.AddListener(OnPointerExit);
	}

	protected new void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			pickerController?.ClosePicker();
			button.onClick.RemoveAllListeners();
		}
	}

	public void Init(CActor actor, IAugmentation augmentation, Action<IAugmentation> onSelect = null, Action<IAugmentation> onUnselect = null, Action<IAugmentation, bool> onHovered = null)
	{
		hovered = false;
		this.onHovered = onHovered;
		base.Init(actor, augmentation, onSelect, onUnselect);
		previewEffect.gameObject.SetActive(value: true);
		selectAudioItem = augmentation.GetSelectAudioItem();
	}

	public void Init(CActor actor, ConsumeButton augmentation, Action<IAugmentation> onSelect = null, Action<IAugmentation> onUnselect = null)
	{
		Init(actor, new ConsumeButtonAugmentation(augmentation), onSelect, onUnselect, delegate(IAugmentation _, bool hovered)
		{
			augmentation.SetHovered(hovered);
		});
		tooltip.Initialize(augmentation);
		Decorate(augmentation.abilityConsume.ConsumeData);
	}

	public void Init(CActor actor, string groupId, List<CActionAugmentation> augmentations, Action<CActionAugmentation, IAugmentation> onSelect = null, Action<CActionAugmentation, IAugmentation> onUnselect = null)
	{
		List<IOption> list = new List<IOption>();
		for (int i = 0; i < augmentations.Count; i++)
		{
			list.Add(new AugmentationOption(augmentations[i]));
		}
		Decorate(actor, null, list);
		ConsumeButtonGroupAugmentation augmentGroup = new ConsumeButtonGroupAugmentation(groupId, augmentations, augmentationOptionController, previewEffect);
		CActionAugmentation selectedConsume = null;
		Init(actor, augmentGroup, delegate(IAugmentation group)
		{
			selectedConsume = augmentGroup.SelectedAugment.Augmentation;
			onSelect(selectedConsume, group);
		}, delegate(IAugmentation group)
		{
			onUnselect(selectedConsume, group);
			selectedConsume = null;
		});
		tooltip.Initialize($"UI_AUGMENT_GROUP_TOOLTIP_TITLE_{groupId}", $"UI_AUGMENT_GROUP_TOOLTIP_{groupId}");
	}

	public List<ElementInfusionBoardManager.EElement> GetSelectedElements()
	{
		return (from it in augmentationElementControllers
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList();
	}

	private void Decorate(CActionAugmentation augmentation)
	{
		previewEffect.SetDescription(augmentation);
		Decorate(actor, augmentation.Elements);
	}

	private void Decorate(CActor actor, List<ElementInfusionBoardManager.EElement> consumeElements = null, List<IOption> options = null)
	{
		augmentationOptionController = null;
		augmentationElementControllers.Clear();
		if (consumeElements != null && consumeElements.Count > 0)
		{
			HelperTools.NormalizePool(ref augmentations, augmentationPrefab.gameObject, augmentations[0].transform.parent, consumeElements.Count, null, delegate(UIUseAugmentationElement slot)
			{
				slot.Hide();
			});
			for (int num = 0; num < consumeElements.Count; num++)
			{
				augmentationElementControllers.Add(new UseAugmentationElementController(augmentations[num], consumeElements[num]));
			}
		}
		else
		{
			HelperTools.NormalizePool(ref augmentations, augmentationPrefab.gameObject, augmentations[0].transform.parent, 1, null, delegate(UIUseAugmentationElement slot)
			{
				slot.Hide();
			});
			if (actor is CPlayerActor)
			{
				augmentationOptionController = new UseAugmentationOptionController(augmentations[augmentationElementControllers.Count], UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.Class.ID), options != null && options.Count > 1);
			}
			else
			{
				augmentationOptionController = new UseAugmentationOptionController(augmentations[augmentationElementControllers.Count], UIInfoTools.Instance.GetCharacterActiveAbilityIcon(actor.Class.ID), options != null && options.Count > 1);
			}
		}
		Highlight();
		CreatePickers(consumeElements, options);
	}

	private void CreatePickers(List<ElementInfusionBoardManager.EElement> consumeElements, List<IOption> options)
	{
		pickerController = null;
		if (consumeElements != null && consumeElements.Count == 1)
		{
			pickerController = new SingleElementPickController(elementPicker).SetOnSelectedElement(delegate
			{
				Highlight();
				Select();
			}).SetOnClosePicker(OnClosePicker).SetOnOpenPicker(OnOpenPicker);
			((SingleElementPickController)pickerController).Setup(augmentationElementControllers[0]);
		}
		else if (consumeElements != null && consumeElements.Count > 1)
		{
			pickerController = new MultiElementPickController(elementPicker).SetOnSelectedAllElements(delegate
			{
				Select();
			}).SetOnSelectedElement(OnSelectedElement).SetOnCancelled(delegate
			{
				Highlight();
			})
				.SetExcludedElements(consumeElements)
				.SetOnClosePicker(OnClosePicker)
				.SetOnOpenPicker(OnOpenPicker);
			((MultiElementPickController)pickerController).Setup(augmentationElementControllers.Cast<IElementHolder>().ToList());
		}
		else if (options != null && options.Count > 1)
		{
			pickerController = new SingleOptionPickController(optionPicker).SetOnSelected(delegate
			{
				Select();
			}).SetOnClosePicker(OnClosePicker).SetOnOpenPicker(OnOpenPicker);
			((SingleOptionPickController)pickerController).Setup(augmentationOptionController, options);
		}
	}

	private void OnSelectedElement(int index, ElementInfusionBoardManager.EElement element)
	{
		Highlight(index);
	}

	private void Highlight(int index = 0)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (augmentationElementControllers.Count == 0)
		{
			highlight.Highlight(defaultHighlightColor);
			return;
		}
		highlight.Highlight(augmentationElementControllers.Select((UseAugmentationElementController it) => it.SelectedElement.GetValueOrDefault(it.RequiredElement)).ToList(), index);
	}

	private void OnClosePicker()
	{
		if (!(base.gameObject == null))
		{
			previewEffect.gameObject.SetActive(value: true);
			CheckShowTooltip();
			for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
			{
				augmentations[i].Focus(focus: true);
			}
		}
	}

	private void OnOpenPicker()
	{
		previewEffect.gameObject.SetActive(value: false);
		CheckShowTooltip();
		for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
		{
			augmentations[i].Focus(focus: false);
		}
	}

	public override void Show()
	{
		base.Show();
		for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
		{
			augmentations[i].Show();
		}
	}

	public override void Unselect()
	{
		if (element.CanBeDisactivated())
		{
			base.Unselect();
		}
	}

	public override void Select()
	{
		if (pickerController != null && pickerController.IsSelecting())
		{
			pickerController.ClosePicker();
		}
		else
		{
			if (pickerController != null && !pickerController.Pick())
			{
				return;
			}
			for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
			{
				augmentations[i].OnConsumed();
			}
			selected = true;
			foreach (UseAugmentationElementController item in augmentationElementControllers.Where((UseAugmentationElementController it) => it.RequiredElement != ElementInfusionBoardManager.EElement.Any))
			{
				AudioControllerUtils.PlaySound(UIInfoTools.Instance.GetSelectElementAudioItemIcon(item.RequiredElement));
			}
			element.ActiveAugment(GetSelectedElements());
			if (InputManager.GamePadInUse && ControllerInputAreaManager.IsFocusedArea(EControllerInputAreaType.Items))
			{
				EventSystem.current.SetSelectedGameObject(base.gameObject);
			}
			base.Select();
		}
	}

	public void OnPointerEnter()
	{
		hovered = true;
		for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
		{
			augmentations[i].ShowHovered(hovered: true);
		}
		CheckShowTooltip();
		onHovered?.Invoke(element, arg2: true);
	}

	public void OnPointerExit()
	{
		hovered = false;
		if (tooltip.gameObject.activeSelf)
		{
			UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
			tooltip.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < augmentations.Count && augmentations[i].gameObject.activeSelf; i++)
		{
			augmentations[i].ShowHovered(hovered: false);
		}
		onHovered?.Invoke(element, arg2: false);
	}

	private void CheckShowTooltip()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (pickerController != null && pickerController.IsSelecting())
		{
			if (tooltip.gameObject.activeSelf)
			{
				UIManager.Instance.UnhighlightElement(tooltip.gameObject, unlockUI: false);
				tooltip.gameObject.SetActive(value: false);
			}
		}
		else if (hovered)
		{
			UIManager.Instance.HighlightElement(tooltip.gameObject, fadeEverythingElse: false, lockUI: false);
			tooltip.gameObject.SetActive(value: true);
		}
	}

	protected override void OnDisable()
	{
		hovered = false;
		OnPointerExit();
	}

	public override void Hide()
	{
		pickerController?.ClosePicker();
		OnPointerExit();
		base.Hide();
	}

	public override void ClearSelection(bool fromClick = false)
	{
		if (selected)
		{
			base.ClearSelection(fromClick);
			for (int i = 0; i < augmentationElementControllers.Count; i++)
			{
				augmentationElementControllers[i].ClearSelection();
			}
			element.DisactiveAugment();
			augmentationOptionController?.ClearSelection();
			Highlight();
		}
	}

	public void SetConsumes(List<ElementInfusionBoardManager.EElement> elements)
	{
		for (int i = 0; i < augmentationElementControllers.Count; i++)
		{
			if (augmentationElementControllers[i].RequiredElement != ElementInfusionBoardManager.EElement.Any && elements.Contains(augmentationElementControllers[i].RequiredElement))
			{
				augmentationElementControllers[i].SetSelectedElement(augmentationElementControllers[i].RequiredElement);
				elements.Remove(augmentationElementControllers[i].RequiredElement);
			}
		}
		int num = 0;
		for (int j = 0; j < augmentationElementControllers.Count; j++)
		{
			if (num >= elements.Count)
			{
				break;
			}
			if (augmentationElementControllers[j].RequiredElement == ElementInfusionBoardManager.EElement.Any)
			{
				augmentationElementControllers[j].SetSelectedElement(elements[num]);
				num++;
			}
		}
	}

	public void SetOption(string optionId)
	{
		if (pickerController != null && pickerController is SingleOptionPickController)
		{
			((SingleOptionPickController)pickerController).SelectOption(optionId);
		}
	}
}
