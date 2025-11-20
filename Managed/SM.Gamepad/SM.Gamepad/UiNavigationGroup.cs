using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace SM.Gamepad;

public class UiNavigationGroup : UiNavigationBase, IUiNavigationNode, IUiNavigationElement
{
	[Header("Group Properties")]
	[SerializeField]
	private CustomizableNavigationHandlerBehaviour _customizableNavigationHandler;

	[Header("Works only with UiNavigationGroup")]
	[SerializeField]
	private bool _autoSelectFirstElement = true;

	[SerializeField]
	private UINavigationSelectable _defaultElementToSelect;

	[SerializeField]
	private List<NavigationSourceHandler> _additionalNavigationSourceHandlers;

	public UiNavigationHandler.NavigationHandleType OuterNavigationHandleType;

	public UiNavigationHandler.NavigationHandleType InnerNavigationHandleType;

	[Tooltip("How navigation outside of the group should be handled")]
	protected OuterUiNavigationHandler OuterUINavigation;

	[Tooltip("How navigation inside of the group should be handled")]
	protected UiNavigationHandler InnerUINavigationHandler;

	protected IUiNavigationManager NavigationManager;

	private IUiNavigationSelectable _lastSelectedElement;

	private List<INavigationSourceHandler> _navigationSourceHandlers;

	private bool _resetCurrentSelectedElement;

	public List<IUiNavigationElement> Elements { get; private set; } = new List<IUiNavigationElement>();

	public List<IUiNavigationElement> ElementsByPriorityDescending { get; private set; } = new List<IUiNavigationElement>();

	public List<IUiNavigationElement> ElementsByPriorityAscending { get; private set; } = new List<IUiNavigationElement>();

	public bool AutoSelectFirstElement => _autoSelectFirstElement;

	public IUiNavigationSelectable DefaultElementToSelect => _defaultElementToSelect;

	public override bool IsInteractable
	{
		get
		{
			if (Elements != null)
			{
				return Elements.Count > 0;
			}
			return false;
		}
	}

	protected virtual IUiNavigationRoot RootOrSelf => base.Root;

	public event Action<UINavigationDirection> BeforeNavigationMoveEvent;

	public event Action<UINavigationDirection> AfterNavigationMoveEvent;

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearElements();
	}

	public virtual void InitializeNodeElement(IUiNavigationManager uiNavigationManager, IUiNavigationRoot root, HashSet<IUiNavigationElement> knownElements, Dictionary<string, IUiNavigationElement> elementsByNameMap, Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> knownSelectablesMap)
	{
		NavigationManager = uiNavigationManager;
		InitializeBaseElement(uiNavigationManager, root, elementsByNameMap, knownSelectablesMap);
		InitializeOldFashionNavigationHandlers();
		if (!knownElements.Contains(this))
		{
			knownElements.Add(this);
		}
		UiNavigationElementsUtils.InitElements(Elements, base.UiNavigationManager, root, knownElements, elementsByNameMap, knownSelectablesMap);
	}

	[UsedImplicitly]
	protected override void OnBeforeTransformParentChanged()
	{
		if (base.Parent != null)
		{
			base.Parent.RemoveElement(this);
		}
		ClearElements();
	}

	private void ClearElements()
	{
		ClearParent();
		ClearRoot();
		Elements.Clear();
		ElementsByPriorityDescending.Clear();
		ElementsByPriorityAscending.Clear();
	}

	public void SetDefaultElementToSelect(UINavigationSelectable defaultElement, bool navigateToElementNextTime = true)
	{
		if (!AutoSelectFirstElement)
		{
			_defaultElementToSelect = defaultElement;
			_resetCurrentSelectedElement = navigateToElementNextTime;
		}
	}

	private void InitNavigationSourceHandlers()
	{
		_navigationSourceHandlers = new List<INavigationSourceHandler>
		{
			new DefaultNavigationSourceHandler()
		};
		if (_additionalNavigationSourceHandlers != null)
		{
			_navigationSourceHandlers.AddRange(_additionalNavigationSourceHandlers);
		}
	}

	public bool IsNavigationSourceAllowed(UIActionBaseEventData uiActionBaseEventData)
	{
		if (_navigationSourceHandlers == null)
		{
			InitNavigationSourceHandlers();
		}
		return _navigationSourceHandlers.Any((INavigationSourceHandler navigationSourceHandler) => navigationSourceHandler.IsAllowedToNavigate(uiActionBaseEventData));
	}

	public bool TryToNavigate(HashSet<IUiNavigationNode> handledNodes, UINavigationDirection navigationDirection)
	{
		this.BeforeNavigationMoveEvent?.Invoke(navigationDirection);
		if (_customizableNavigationHandler != null)
		{
			bool num = _customizableNavigationHandler.TryHandleNavigation(handledNodes, this, navigationDirection);
			if (num)
			{
				Action<UINavigationDirection> action = this.AfterNavigationMoveEvent;
				if (action == null)
				{
					return num;
				}
				action(navigationDirection);
			}
			return num;
		}
		bool num2 = OldFashionNavigation(handledNodes, navigationDirection);
		if (num2)
		{
			Action<UINavigationDirection> action2 = this.AfterNavigationMoveEvent;
			if (action2 == null)
			{
				return num2;
			}
			action2(navigationDirection);
		}
		return num2;
	}

	protected virtual void InitializeOldFashionNavigationHandlers()
	{
		if (RootOrSelf != null)
		{
			OuterUINavigation = new OuterUiNavigationHandler(base.Root as UiNavigationRoot, this);
			InnerUINavigationHandler = new UiNavigationHandler(base.Root as UiNavigationRoot, this);
		}
		else
		{
			OuterUINavigation = null;
			InnerUINavigationHandler = null;
		}
	}

	public bool OldFashionNavigation(HashSet<IUiNavigationNode> handledNodes, UINavigationDirection navigationDirection)
	{
		bool flag = false;
		if (InnerUINavigationHandler != null)
		{
			if (AutoSelectFirstElement && NavigationManager.CurrentlySelectedElement is UINavigationSelectable)
			{
				flag = InnerUINavigationHandler.TryPerformNavigation(handledNodes, navigationDirection);
			}
			else
			{
				if (_resetCurrentSelectedElement)
				{
					_lastSelectedElement = null;
					_resetCurrentSelectedElement = false;
				}
				if (_lastSelectedElement == null && NavigationManager.CurrentlySelectedElement == DefaultElementToSelect)
				{
					_lastSelectedElement = DefaultElementToSelect;
				}
				flag = InnerUINavigationHandler.TryPerformNavigation(handledNodes, navigationDirection, (_lastSelectedElement == null) ? DefaultElementToSelect : null);
				if (flag)
				{
					_lastSelectedElement = NavigationManager.CurrentlySelectedElement;
				}
			}
		}
		if (!flag && OuterUINavigation != null)
		{
			flag = OuterUINavigation.TryPerformNavigation(handledNodes, navigationDirection);
		}
		return flag;
	}

	internal void AddElementInternal(UiNavigationBase navigationElement)
	{
		UiNavigationElementsUtils.InsertElementsBySiblingIndex(Elements, navigationElement);
		UiNavigationElementsUtils.InsertElementsByPriorityDescending(ElementsByPriorityDescending, navigationElement);
		UiNavigationElementsUtils.InsertElementsByPriorityAscending(ElementsByPriorityAscending, navigationElement);
		SM.Gamepad.UiNavigationManager.Instance.AddElementInternal(navigationElement, RootOrSelf);
	}

	internal void RemoveElement(UiNavigationBase uiNavigationBase)
	{
		RemoveElementFromLists(uiNavigationBase);
		SM.Gamepad.UiNavigationManager.Instance.RemoveElementInternal(uiNavigationBase, RootOrSelf);
	}

	internal void RemoveElementFromLists(UiNavigationBase uiNavigationBase)
	{
		Elements.Remove(uiNavigationBase);
		ElementsByPriorityDescending.Remove(uiNavigationBase);
		ElementsByPriorityAscending.Remove(uiNavigationBase);
	}

	public override void RefreshParent()
	{
		foreach (IUiNavigationElement item in new List<IUiNavigationElement>(Elements))
		{
			item.RefreshParent();
		}
		base.RefreshParent();
		InitializeOldFashionNavigationHandlers();
		_lastSelectedElement = null;
	}

	public void UpdateElementsSortingOrder()
	{
		UiNavigationElementsUtils.SortElementsBySiblingIndex(Elements);
		UiNavigationElementsUtils.SortElementsByPriorityDescending(ElementsByPriorityDescending);
		UiNavigationElementsUtils.SortElementsByPriorityAscending(ElementsByPriorityAscending);
	}

	public override void ClearRoot()
	{
		base.ClearRoot();
		foreach (IUiNavigationElement element in Elements)
		{
			element.ClearRoot();
		}
	}

	public override void ClearParent()
	{
		base.ClearParent();
		foreach (IUiNavigationElement element in Elements)
		{
			element.ClearParent();
		}
	}
}
