using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIUseConsumeInfuseSlot<T> : UIUseSlot<T>
{
	internal class InfuseElementController : IElementHolder
	{
		private UIUseOption elementUI;

		private IInfuseElement elementInfuse;

		public ElementInfusionBoardManager.EElement RequiredElement
		{
			get
			{
				if (!elementInfuse.IsAnyElement)
				{
					return elementInfuse.SelectedElement;
				}
				return ElementInfusionBoardManager.EElement.Any;
			}
		}

		public ElementInfusionBoardManager.EElement? SelectedElement
		{
			get
			{
				if (elementInfuse.SelectedElement != ElementInfusionBoardManager.EElement.Any)
				{
					return elementInfuse.SelectedElement;
				}
				return null;
			}
		}

		public InfuseElementController(IInfuseElement elementInfuse, UIUseOption elementUI = null)
		{
			this.elementUI = elementUI;
			this.elementInfuse = elementInfuse;
			elementUI?.Clear();
		}

		public void SetSelectedElement(ElementInfusionBoardManager.EElement? element)
		{
			if (!element.HasValue)
			{
				elementUI?.Clear();
				elementInfuse.ResetElementToInitial();
			}
			else
			{
				elementUI?.SetOption(UIInfoTools.Instance.GetElementPickerSprite(element.Value));
				elementInfuse.PickElement(element.Value);
			}
		}
	}

	internal class ConsumeElementController : IElementHolder
	{
		private UIUseOption elementUI;

		private ElementInfusionBoardManager.EElement? selectedElement;

		public ElementInfusionBoardManager.EElement RequiredElement { get; }

		public ElementInfusionBoardManager.EElement? SelectedElement => selectedElement;

		public ConsumeElementController(UIUseOption elementUI, ElementInfusionBoardManager.EElement element)
		{
			this.elementUI = elementUI;
			RequiredElement = element;
			SetSelectedElement(null);
		}

		public void SetSelectedElement(ElementInfusionBoardManager.EElement? element)
		{
			selectedElement = element;
			if (!element.HasValue)
			{
				elementUI.SetOption((RequiredElement == ElementInfusionBoardManager.EElement.Any) ? null : UIInfoTools.Instance.GetElementPickerSprite(RequiredElement));
			}
			else
			{
				elementUI.SetOption(UIInfoTools.Instance.GetElementPickerSprite(element.Value));
			}
		}
	}

	[SerializeField]
	private Image unfocusImage;

	[SerializeField]
	private Color unfocusedColor = Color.gray;

	[Header("Element")]
	[SerializeField]
	private UIElementPicker elementPicker;

	[SerializeField]
	private List<UIUseOption> infuseElements;

	[SerializeField]
	private List<UIUseOption> consumeElements;

	protected List<IElementHolder> infusions = new List<IElementHolder>();

	protected List<IElementHolder> consumes = new List<IElementHolder>();

	protected InfuseElementPickController infusePickerController;

	protected MultiElementPickController consumePickerController;

	private Action<CItem> onPickedAll;

	private Action onPickerCancel;

	protected bool hovered;

	public int InfusionsNum => infusions.Count;

	public int ConsumesNum => consumes.Count;

	protected virtual void Awake()
	{
		infusePickerController = new InfuseElementPickController(elementPicker).SetOnSelectedAllElements(OnElementsSelected);
		infusePickerController.SetOnClosePicker(OnCloseInfusionPicker).SetOnOpenPicker(OnOpenPicker);
		consumePickerController = new MultiElementPickController(elementPicker).SetOnSelectedAllElements(OnSelectedElements);
		consumePickerController.SetOnClosePicker(OnClosePicker).SetOnOpenPicker(OnOpenPicker);
	}

	protected virtual void OnSelectedElements(List<ElementInfusionBoardManager.EElement> elements)
	{
		Select();
	}

	protected void Init(CActor actor, T element, Action<T> onSelect = null, Action<T> onUnselect = null, Func<T, bool> isMandatoryChecker = null, bool isSelected = false, List<IInfuseElement> infusions = null, List<ElementInfusionBoardManager.EElement> consumes = null, Action<CItem> onPickedAll = null, Action onPickerCancel = null)
	{
		base.Init(actor, element, onSelect, onUnselect, isMandatoryChecker, isSelected);
		CreateInfusions(infusions);
		CreateConsumes(consumes);
		this.onPickedAll = onPickedAll;
		this.onPickerCancel = onPickerCancel;
	}

	protected virtual void OnCloseInfusionPicker()
	{
		if (!infusePickerController.AreAllAnySelected())
		{
			onPickerCancel?.Invoke();
			consumePickerController.Cancel();
		}
		OnClosePicker();
	}

	protected virtual void OnClosePicker()
	{
		if (!(base.gameObject == null) && base.gameObject.activeSelf)
		{
			unfocusImage.color = UIInfoTools.Instance.White;
			CheckShowTooltip();
		}
	}

	protected virtual void OnOpenPicker()
	{
		unfocusImage.color = unfocusedColor;
		CheckShowTooltip();
	}

	public void SetInfusions(List<ElementInfusionBoardManager.EElement> elements)
	{
		int num = 0;
		for (int i = 0; i < infusions.Count; i++)
		{
			if (infusions[i].RequiredElement == ElementInfusionBoardManager.EElement.Any)
			{
				infusions[i].SetSelectedElement((num < elements.Count) ? new ElementInfusionBoardManager.EElement?(elements[num]) : ((ElementInfusionBoardManager.EElement?)null));
				num++;
			}
		}
	}

	private void OnElementsSelected(List<ElementInfusionBoardManager.EElement> elements)
	{
		onPickedAll?.Invoke(element as CItem);
	}

	public override void Select()
	{
		if (consumes.Count > 0)
		{
			if (consumePickerController.IsSelecting())
			{
				consumePickerController.ClosePicker();
				return;
			}
			if (!consumePickerController.Pick())
			{
				return;
			}
		}
		if (infusions.Count > 0)
		{
			if (infusePickerController.IsSelecting())
			{
				infusePickerController.ClosePicker();
				return;
			}
			if (!infusePickerController.Pick())
			{
				return;
			}
		}
		base.Select();
	}

	public void ConsumeOrInfuseIfPossible()
	{
		if (consumes.Count > 0)
		{
			Consume(consumes.Select((IElementHolder it) => it.SelectedElement.Value).ToList(), active: true);
		}
		if (infusions.Count > 0)
		{
			Infuse(infusions.Select((IElementHolder it) => it.SelectedElement.Value).ToList(), active: true);
		}
	}

	public void Deselect()
	{
		base.Unselect();
	}

	public bool IsAllInfusionsPicked()
	{
		return infusions.All((IElementHolder item) => item.SelectedElement.HasValue);
	}

	public bool IsAllConsumesPicked()
	{
		return consumes.All((IElementHolder item) => item.SelectedElement.HasValue);
	}

	public List<ElementInfusionBoardManager.EElement> GetSelectedInfusions()
	{
		return (from it in infusions
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList();
	}

	public List<ElementInfusionBoardManager.EElement> GetSelectedConsumes()
	{
		return (from it in consumes
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList();
	}

	protected virtual void Infuse(List<ElementInfusionBoardManager.EElement> infusions, bool active)
	{
		if (active)
		{
			ElementInfusionBoardManager.Infuse(infusions, actor);
			InfusionBoardUI.Instance.UpdateBoard(infusions);
		}
		else
		{
			ElementInfusionBoardManager.UndoInfuse(infusions, actor);
			InfusionBoardUI.Instance.RemoveElementsInCreation(infusions);
		}
	}

	protected virtual void Consume(List<ElementInfusionBoardManager.EElement> consumes, bool active)
	{
		InfusionBoardUI.Instance.ReserveElements(consumes, active);
	}

	public override void ClearSelection(bool fromClick = false)
	{
		if (!selected)
		{
			return;
		}
		base.ClearSelection();
		Consume((from it in consumes
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList(), active: false);
		foreach (IElementHolder consume in consumes)
		{
			consume.SetSelectedElement(null);
		}
		Infuse((from it in infusions
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList(), active: false);
		foreach (IElementHolder infusion in infusions)
		{
			infusion.SetSelectedElement(null);
		}
	}

	public void ClearSelectionNew()
	{
		foreach (IElementHolder consume in consumes)
		{
			consume.SetSelectedElement(null);
		}
		foreach (IElementHolder infusion in infusions)
		{
			infusion.SetSelectedElement(null);
		}
	}

	protected virtual void CreateInfusions(List<IInfuseElement> elements)
	{
		infusions.Clear();
		if (elements == null)
		{
			HelperTools.NormalizePool(ref infuseElements, infuseElements[0].gameObject, infuseElements[0].transform.parent, 0);
		}
		else
		{
			HelperTools.NormalizePool(ref infuseElements, infuseElements[0].gameObject, infuseElements[0].transform.parent, elements.Count((IInfuseElement it) => it.IsAnyElement));
			int num = 0;
			for (int num2 = 0; num2 < elements.Count; num2++)
			{
				UIUseOption elementUI = null;
				if (elements[num2].IsAnyElement)
				{
					elementUI = infuseElements[num];
					num++;
				}
				infusions.Add(new InfuseElementController(elements[num2], elementUI));
			}
		}
		infusePickerController.Setup(infusions);
	}

	protected virtual void CreateConsumes(List<ElementInfusionBoardManager.EElement> elements)
	{
		consumes.Clear();
		if (elements == null)
		{
			HelperTools.NormalizePool(ref consumeElements, consumeElements[0].gameObject, consumeElements[0].transform.parent, 0);
		}
		else
		{
			HelperTools.NormalizePool(ref consumeElements, consumeElements[0].gameObject, consumeElements[0].transform.parent, elements.Count);
			for (int i = 0; i < elements.Count; i++)
			{
				consumes.Add(new ConsumeElementController(consumeElements[i], elements[i]));
			}
		}
		consumePickerController.Setup(consumes);
	}

	public virtual void OnPointerEnter()
	{
		hovered = true;
		CheckShowTooltip();
	}

	public virtual void OnPointerExit()
	{
		hovered = false;
		ShowTooltip(show: false);
	}

	public override void Hide()
	{
		OnPointerExit();
		base.Hide();
	}

	protected virtual void CheckShowTooltip()
	{
		if (base.gameObject.activeSelf)
		{
			if (elementPicker.IsOpen)
			{
				ShowTooltip(show: false);
			}
			else
			{
				ShowTooltip(hovered);
			}
		}
	}

	protected abstract void ShowTooltip(bool show);

	public void SetAnyConsume(ElementInfusionBoardManager.EElement? element, int consumeIndex = 0)
	{
		if (consumeIndex < consumes.Count)
		{
			List<IElementHolder> list = consumes.FindAll((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any);
			if (consumeIndex < list.Count)
			{
				list[consumeIndex].SetSelectedElement(element);
			}
		}
	}

	public void SetAnyConsume(List<ElementInfusionBoardManager.EElement> elements)
	{
		List<IElementHolder> list = consumes.FindAll((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any);
		if (elements.Count == list.Count)
		{
			for (int num = 0; num < list.Count; num++)
			{
				list[num].SetSelectedElement(elements[num]);
			}
		}
	}

	protected new virtual void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			consumePickerController?.OnDestroy();
			infusePickerController?.OnDestroy();
		}
	}
}
