using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

public abstract class BaseVerticalCollectionView : BindableElement, ISerializationCallbackReceiver
{
	private Func<int, int> m_GetItemId;

	private Func<VisualElement> m_MakeItem;

	private Action<VisualElement, int> m_BindItem;

	private SelectionType m_SelectionType;

	private static readonly List<ReusableCollectionItem> k_EmptyItems = new List<ReusableCollectionItem>();

	private bool m_HorizontalScrollingEnabled;

	[SerializeField]
	private AlternatingRowBackground m_ShowAlternatingRowBackgrounds = AlternatingRowBackground.None;

	internal static readonly int s_DefaultItemHeight = 30;

	internal float m_FixedItemHeight = s_DefaultItemHeight;

	internal bool m_ItemHeightIsInline;

	private CollectionVirtualizationMethod m_VirtualizationMethod;

	private readonly ScrollView m_ScrollView;

	private CollectionViewController m_ViewController;

	private CollectionVirtualizationController m_VirtualizationController;

	private KeyboardNavigationManipulator m_NavigationManipulator;

	[SerializeField]
	internal Vector2 m_ScrollOffset;

	[SerializeField]
	private readonly List<int> m_SelectedIds = new List<int>();

	private readonly List<int> m_SelectedIndices = new List<int>();

	private readonly List<object> m_SelectedItems = new List<object>();

	private float m_LastHeight;

	private bool m_IsRangeSelectionDirectionUp;

	private ListViewDragger m_Dragger;

	internal const float ItemHeightUnset = -1f;

	internal static CustomStyleProperty<int> s_ItemHeightProperty = new CustomStyleProperty<int>("--unity-item-height");

	private Action<int, int> m_ItemIndexChangedCallback;

	private Action m_ItemsSourceChangedCallback;

	public static readonly string ussClassName = "unity-collection-view";

	public static readonly string borderUssClassName = ussClassName + "--with-border";

	public static readonly string itemUssClassName = ussClassName + "__item";

	public static readonly string dragHoverBarUssClassName = ussClassName + "__drag-hover-bar";

	public static readonly string itemDragHoverUssClassName = itemUssClassName + "--drag-hover";

	public static readonly string itemSelectedVariantUssClassName = itemUssClassName + "--selected";

	public static readonly string itemAlternativeBackgroundUssClassName = itemUssClassName + "--alternative-background";

	public static readonly string listScrollViewUssClassName = ussClassName + "__scroll-view";

	internal static readonly string backgroundFillUssClassName = ussClassName + "__background";

	private Vector3 m_TouchDownPosition;

	private int m_LastFocusedElementIndex = -1;

	private List<int> m_LastFocusedElementTreeChildIndexes = new List<int>();

	internal Func<int, int> getItemId
	{
		get
		{
			return m_GetItemId;
		}
		set
		{
			m_GetItemId = value;
			RefreshItems();
		}
	}

	public IList itemsSource
	{
		get
		{
			return viewController?.itemsSource;
		}
		set
		{
			GetOrCreateViewController().itemsSource = value;
		}
	}

	internal virtual bool sourceIncludesArraySize => false;

	public Func<VisualElement> makeItem
	{
		get
		{
			return m_MakeItem;
		}
		set
		{
			m_MakeItem = value;
			Rebuild();
		}
	}

	public Action<VisualElement, int> bindItem
	{
		get
		{
			return m_BindItem;
		}
		set
		{
			m_BindItem = value;
			RefreshItems();
		}
	}

	public Action<VisualElement, int> unbindItem { get; set; }

	public Action<VisualElement> destroyItem { get; set; }

	public override VisualElement contentContainer => null;

	public SelectionType selectionType
	{
		get
		{
			return m_SelectionType;
		}
		set
		{
			m_SelectionType = value;
			if (m_SelectionType == SelectionType.None)
			{
				ClearSelection();
			}
			else if (m_SelectionType == SelectionType.Single && m_SelectedIndices.Count > 1)
			{
				SetSelection(m_SelectedIndices.First());
			}
		}
	}

	public object selectedItem => (m_SelectedItems.Count == 0) ? null : m_SelectedItems.First();

	public IEnumerable<object> selectedItems => m_SelectedItems;

	public int selectedIndex
	{
		get
		{
			return (m_SelectedIndices.Count == 0) ? (-1) : m_SelectedIndices.First();
		}
		set
		{
			SetSelection(value);
		}
	}

	public IEnumerable<int> selectedIndices => m_SelectedIndices;

	internal List<int> currentSelectionIds => m_SelectedIds;

	internal IEnumerable<ReusableCollectionItem> activeItems => m_VirtualizationController?.activeItems ?? k_EmptyItems;

	internal ScrollView scrollView => m_ScrollView;

	internal ListViewDragger dragger => m_Dragger;

	internal CollectionViewController viewController => m_ViewController;

	internal CollectionVirtualizationController virtualizationController => GetOrCreateVirtualizationController();

	[Obsolete("resolvedItemHeight is deprecated and will be removed from the API.", false)]
	public float resolvedItemHeight => ResolveItemHeight();

	public bool showBorder
	{
		get
		{
			return m_ScrollView.ClassListContains(borderUssClassName);
		}
		set
		{
			m_ScrollView.EnableInClassList(borderUssClassName, value);
		}
	}

	public bool reorderable
	{
		get
		{
			return m_Dragger?.dragAndDropController?.enableReordering == true;
		}
		set
		{
			if (m_Dragger?.dragAndDropController == null)
			{
				if (value)
				{
					InitializeDragAndDropController();
				}
				return;
			}
			ICollectionDragAndDropController dragAndDropController = m_Dragger.dragAndDropController;
			if (dragAndDropController != null && dragAndDropController.enableReordering != value)
			{
				dragAndDropController.enableReordering = value;
				Rebuild();
			}
		}
	}

	public bool horizontalScrollingEnabled
	{
		get
		{
			return m_HorizontalScrollingEnabled;
		}
		set
		{
			if (m_HorizontalScrollingEnabled != value)
			{
				m_HorizontalScrollingEnabled = value;
				m_ScrollView.mode = (value ? ScrollViewMode.VerticalAndHorizontal : ScrollViewMode.Vertical);
			}
		}
	}

	public AlternatingRowBackground showAlternatingRowBackgrounds
	{
		get
		{
			return m_ShowAlternatingRowBackgrounds;
		}
		set
		{
			if (m_ShowAlternatingRowBackgrounds != value)
			{
				m_ShowAlternatingRowBackgrounds = value;
				RefreshItems();
			}
		}
	}

	public CollectionVirtualizationMethod virtualizationMethod
	{
		get
		{
			return m_VirtualizationMethod;
		}
		set
		{
			CollectionVirtualizationMethod collectionVirtualizationMethod = m_VirtualizationMethod;
			m_VirtualizationMethod = value;
			if (collectionVirtualizationMethod != value)
			{
				CreateVirtualizationController();
				Rebuild();
			}
		}
	}

	[Obsolete("itemHeight is deprecated, use fixedItemHeight instead.", false)]
	public int itemHeight
	{
		get
		{
			return (int)fixedItemHeight;
		}
		set
		{
			fixedItemHeight = value;
		}
	}

	public float fixedItemHeight
	{
		get
		{
			return m_FixedItemHeight;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("fixedItemHeight", "Value needs to be positive for virtualization.");
			}
			m_ItemHeightIsInline = true;
			if (Math.Abs(m_FixedItemHeight - value) > float.Epsilon)
			{
				m_FixedItemHeight = value;
				RefreshItems();
			}
		}
	}

	internal float lastHeight => m_LastHeight;

	[Obsolete("onItemChosen is deprecated, use onItemsChosen instead", true)]
	public event Action<object> onItemChosen;

	public event Action<IEnumerable<object>> onItemsChosen;

	[Obsolete("onSelectionChanged is deprecated, use onSelectionChange instead", true)]
	public event Action<List<object>> onSelectionChanged;

	public event Action<IEnumerable<object>> onSelectionChange;

	public event Action<IEnumerable<int>> onSelectedIndicesChange;

	public event Action<int, int> itemIndexChanged;

	public event Action itemsSourceChanged;

	internal float ResolveItemHeight(float height = -1f)
	{
		float num = base.scaledPixelsPerPoint;
		height = ((height < 0f) ? fixedItemHeight : height);
		return Mathf.Round(height * num) / num;
	}

	private protected virtual void CreateVirtualizationController()
	{
		CreateVirtualizationController<ReusableCollectionItem>();
	}

	internal CollectionVirtualizationController GetOrCreateVirtualizationController()
	{
		if (m_VirtualizationController == null)
		{
			CreateVirtualizationController();
		}
		return m_VirtualizationController;
	}

	internal void CreateVirtualizationController<T>() where T : ReusableCollectionItem, new()
	{
		switch (virtualizationMethod)
		{
		case CollectionVirtualizationMethod.FixedHeight:
			m_VirtualizationController = new FixedHeightVirtualizationController<T>(this);
			break;
		case CollectionVirtualizationMethod.DynamicHeight:
			m_VirtualizationController = new DynamicHeightVirtualizationController<T>(this);
			break;
		default:
			throw new ArgumentOutOfRangeException("virtualizationMethod", virtualizationMethod, "Unsupported virtualizationMethod virtualization");
		}
	}

	internal CollectionViewController GetOrCreateViewController()
	{
		if (m_ViewController == null)
		{
			CreateViewController();
		}
		return m_ViewController;
	}

	private protected virtual void CreateViewController()
	{
		SetViewController(new CollectionViewController());
	}

	internal void SetViewController(CollectionViewController controller)
	{
		if (m_ViewController != null)
		{
			m_ViewController.itemIndexChanged -= m_ItemIndexChangedCallback;
			m_ViewController.itemsSourceChanged -= m_ItemsSourceChangedCallback;
		}
		m_ViewController = controller;
		if (m_ViewController != null)
		{
			m_ViewController.SetView(this);
			m_ViewController.itemIndexChanged += m_ItemIndexChangedCallback;
			m_ViewController.itemsSourceChanged += m_ItemsSourceChangedCallback;
		}
	}

	internal virtual ListViewDragger CreateDragger()
	{
		return new ListViewDragger(this);
	}

	internal void InitializeDragAndDropController()
	{
		if (m_Dragger != null)
		{
			m_Dragger.UnregisterCallbacksFromTarget(unregisterPanelEvents: true);
			m_Dragger.dragAndDropController = null;
			m_Dragger = null;
		}
		m_Dragger = CreateDragger();
		m_Dragger.dragAndDropController = CreateDragAndDropController();
	}

	internal abstract ICollectionDragAndDropController CreateDragAndDropController();

	internal void SetDragAndDropController(ICollectionDragAndDropController dragAndDropController)
	{
		if (m_Dragger == null)
		{
			m_Dragger = CreateDragger();
		}
		m_Dragger.dragAndDropController = dragAndDropController;
	}

	internal ICollectionDragAndDropController GetDragAndDropController()
	{
		return m_Dragger?.dragAndDropController;
	}

	public BaseVerticalCollectionView()
	{
		AddToClassList(ussClassName);
		selectionType = SelectionType.Single;
		m_ScrollOffset = Vector2.zero;
		m_ScrollView = new ScrollView();
		m_ScrollView.viewDataKey = "list-view__scroll-view";
		m_ScrollView.AddToClassList(listScrollViewUssClassName);
		m_ScrollView.verticalScroller.valueChanged += delegate(float v)
		{
			OnScroll(new Vector2(0f, v));
		};
		RegisterCallback<GeometryChangedEvent>(OnSizeChanged);
		RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
		m_ScrollView.contentContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		m_ScrollView.contentContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
		base.hierarchy.Add(m_ScrollView);
		m_ScrollView.contentContainer.focusable = true;
		m_ScrollView.contentContainer.usageHints &= ~UsageHints.GroupTransform;
		base.focusable = true;
		base.isCompositeRoot = true;
		base.delegatesFocus = true;
		m_ItemIndexChangedCallback = OnItemIndexChanged;
		m_ItemsSourceChangedCallback = OnItemsSourceChanged;
	}

	public BaseVerticalCollectionView(IList itemsSource, float itemHeight = -1f, Func<VisualElement> makeItem = null, Action<VisualElement, int> bindItem = null)
		: this()
	{
		if (Math.Abs(itemHeight - -1f) > float.Epsilon)
		{
			m_FixedItemHeight = itemHeight;
			m_ItemHeightIsInline = true;
		}
		this.itemsSource = itemsSource;
		this.makeItem = makeItem;
		this.bindItem = bindItem;
	}

	public VisualElement GetRootElementForId(int id)
	{
		return activeItems.FirstOrDefault((ReusableCollectionItem t) => t.id == id)?.GetRootElement();
	}

	public VisualElement GetRootElementForIndex(int index)
	{
		return GetRootElementForId(viewController.GetIdForIndex(index));
	}

	internal bool HasValidDataAndBindings()
	{
		return m_ViewController != null && itemsSource != null && makeItem != null == (bindItem != null);
	}

	private void OnItemIndexChanged(int srcIndex, int dstIndex)
	{
		this.itemIndexChanged?.Invoke(srcIndex, dstIndex);
		if (base.binding == null)
		{
			RefreshItems();
		}
		else
		{
			base.schedule.Execute(RefreshItems).ExecuteLater(100L);
		}
	}

	private void OnItemsSourceChanged()
	{
		this.itemsSourceChanged?.Invoke();
	}

	public void RefreshItem(int index)
	{
		foreach (ReusableCollectionItem activeItem in activeItems)
		{
			if (activeItem.index == index)
			{
				viewController.InvokeBindItem(activeItem, activeItem.index);
				break;
			}
		}
	}

	public void RefreshItems()
	{
		if (m_ViewController != null)
		{
			RefreshSelection();
			virtualizationController.Refresh(rebuild: false);
			PostRefresh();
		}
	}

	[Obsolete("Refresh() has been deprecated. Use Rebuild() instead. (UnityUpgradable) -> Rebuild()", false)]
	public void Refresh()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		if (m_ViewController != null)
		{
			RefreshSelection();
			virtualizationController.Refresh(rebuild: true);
			PostRefresh();
		}
	}

	private void RefreshSelection()
	{
		m_SelectedIndices.Clear();
		m_SelectedItems.Clear();
		if (viewController?.itemsSource == null || m_SelectedIds.Count <= 0)
		{
			return;
		}
		int count = viewController.itemsSource.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_SelectedIds.Contains(viewController.GetIdForIndex(i)))
			{
				m_SelectedIndices.Add(i);
				m_SelectedItems.Add(viewController.GetItemForIndex(i));
			}
		}
	}

	private protected virtual void PostRefresh()
	{
		if (HasValidDataAndBindings())
		{
			m_LastHeight = m_ScrollView.layout.height;
			if (!float.IsNaN(m_ScrollView.layout.height))
			{
				Resize(m_ScrollView.layout.size);
			}
		}
	}

	public void ScrollTo(VisualElement visualElement)
	{
		m_ScrollView.ScrollTo(visualElement);
	}

	public void ScrollToItem(int index)
	{
		if (HasValidDataAndBindings())
		{
			virtualizationController.ScrollToItem(index);
		}
	}

	public void ScrollToId(int id)
	{
		int indexForId = viewController.GetIndexForId(id);
		if (HasValidDataAndBindings())
		{
			virtualizationController.ScrollToItem(indexForId);
		}
	}

	private void OnScroll(Vector2 offset)
	{
		if (HasValidDataAndBindings())
		{
			virtualizationController.OnScroll(offset);
		}
	}

	private void Resize(Vector2 size, int layoutPass = -1)
	{
		virtualizationController.Resize(size, layoutPass);
		m_LastHeight = size.y;
		virtualizationController.UpdateBackground();
	}

	private void OnAttachToPanel(AttachToPanelEvent evt)
	{
		if (evt.destinationPanel != null)
		{
			m_ScrollView.contentContainer.AddManipulator(m_NavigationManipulator = new KeyboardNavigationManipulator(Apply));
			m_ScrollView.contentContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
			m_ScrollView.contentContainer.RegisterCallback<PointerDownEvent>(OnPointerDown);
			m_ScrollView.contentContainer.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
			m_ScrollView.contentContainer.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}
	}

	private void OnDetachFromPanel(DetachFromPanelEvent evt)
	{
		if (evt.originPanel != null)
		{
			m_ScrollView.contentContainer.RemoveManipulator(m_NavigationManipulator);
			m_ScrollView.contentContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
			m_ScrollView.contentContainer.UnregisterCallback<PointerDownEvent>(OnPointerDown);
			m_ScrollView.contentContainer.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
			m_ScrollView.contentContainer.UnregisterCallback<PointerUpEvent>(OnPointerUp);
		}
	}

	[Obsolete("OnKeyDown is obsolete and will be removed from ListView. Use the event system instead, i.e. SendEvent(EventBase e).", false)]
	public void OnKeyDown(KeyDownEvent evt)
	{
		m_NavigationManipulator.OnKeyDown(evt);
	}

	private bool Apply(KeyboardNavigationOperation op, bool shiftKey)
	{
		if (!HasValidDataAndBindings())
		{
			return false;
		}
		switch (op)
		{
		case KeyboardNavigationOperation.SelectAll:
			SelectAll();
			return true;
		case KeyboardNavigationOperation.Cancel:
			ClearSelection();
			return true;
		case KeyboardNavigationOperation.Submit:
			this.onItemsChosen?.Invoke(m_SelectedItems);
			ScrollToItem(selectedIndex);
			return true;
		case KeyboardNavigationOperation.Previous:
			if (selectedIndex > 0)
			{
				HandleSelectionAndScroll(selectedIndex - 1);
				return true;
			}
			break;
		case KeyboardNavigationOperation.Next:
			if (selectedIndex + 1 < m_ViewController.itemsSource.Count)
			{
				HandleSelectionAndScroll(selectedIndex + 1);
				return true;
			}
			break;
		case KeyboardNavigationOperation.Begin:
			HandleSelectionAndScroll(0);
			return true;
		case KeyboardNavigationOperation.End:
			HandleSelectionAndScroll(m_ViewController.itemsSource.Count - 1);
			return true;
		case KeyboardNavigationOperation.PageDown:
			if (m_SelectedIndices.Count > 0)
			{
				int num2 = (m_IsRangeSelectionDirectionUp ? m_SelectedIndices.Min() : m_SelectedIndices.Max());
				HandleSelectionAndScroll(Mathf.Min(viewController.itemsSource.Count - 1, num2 + (virtualizationController.visibleItemCount - 1)));
			}
			return true;
		case KeyboardNavigationOperation.PageUp:
			if (m_SelectedIndices.Count > 0)
			{
				int num = (m_IsRangeSelectionDirectionUp ? m_SelectedIndices.Min() : m_SelectedIndices.Max());
				HandleSelectionAndScroll(Mathf.Max(0, num - (virtualizationController.visibleItemCount - 1)));
			}
			return true;
		}
		return false;
		void HandleSelectionAndScroll(int index)
		{
			if (index >= 0 && index < m_ViewController.itemsSource.Count)
			{
				if (selectionType == SelectionType.Multiple && shiftKey && m_SelectedIndices.Count != 0)
				{
					DoRangeSelection(index);
				}
				else
				{
					selectedIndex = index;
				}
				ScrollToItem(index);
			}
		}
	}

	private void Apply(KeyboardNavigationOperation op, EventBase sourceEvent)
	{
		bool shiftKey = (sourceEvent as KeyDownEvent)?.shiftKey ?? false;
		if (Apply(op, shiftKey))
		{
			sourceEvent.StopPropagation();
			sourceEvent.PreventDefault();
		}
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		if (evt.button == 0)
		{
			if ((evt.pressedButtons & 1) == 0)
			{
				ProcessPointerUp(evt);
			}
			else
			{
				ProcessPointerDown(evt);
			}
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		if (evt.pointerType != PointerType.mouse)
		{
			ProcessPointerDown(evt);
			base.panel.PreventCompatibilityMouseEvents(evt.pointerId);
		}
		else
		{
			ProcessPointerDown(evt);
		}
	}

	private void OnPointerCancel(PointerCancelEvent evt)
	{
		if (HasValidDataAndBindings() && evt.isPrimary)
		{
			ClearSelection();
		}
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		if (evt.pointerType != PointerType.mouse)
		{
			ProcessPointerUp(evt);
			base.panel.PreventCompatibilityMouseEvents(evt.pointerId);
		}
		else
		{
			ProcessPointerUp(evt);
		}
	}

	private void ProcessPointerDown(IPointerEvent evt)
	{
		if (HasValidDataAndBindings() && evt.isPrimary && evt.button == 0)
		{
			if (evt.pointerType != PointerType.mouse)
			{
				m_TouchDownPosition = evt.position;
			}
			else
			{
				DoSelect(evt.localPosition, evt.clickCount, evt.actionKey, evt.shiftKey);
			}
		}
	}

	private void ProcessPointerUp(IPointerEvent evt)
	{
		if (!HasValidDataAndBindings() || !evt.isPrimary || evt.button != 0)
		{
			return;
		}
		if (evt.pointerType != PointerType.mouse)
		{
			if ((evt.position - m_TouchDownPosition).sqrMagnitude <= 25f)
			{
				DoSelect(evt.localPosition, evt.clickCount, evt.actionKey, evt.shiftKey);
			}
			return;
		}
		int indexFromPosition = virtualizationController.GetIndexFromPosition(evt.localPosition);
		if (selectionType == SelectionType.Multiple && !evt.shiftKey && !evt.actionKey && m_SelectedIndices.Count > 1 && m_SelectedIndices.Contains(indexFromPosition))
		{
			ProcessSingleClick(indexFromPosition);
		}
	}

	private void DoSelect(Vector2 localPosition, int clickCount, bool actionKey, bool shiftKey)
	{
		int indexFromPosition = virtualizationController.GetIndexFromPosition(localPosition);
		if (indexFromPosition > viewController.itemsSource.Count - 1)
		{
			return;
		}
		int idForIndex = viewController.GetIdForIndex(indexFromPosition);
		switch (clickCount)
		{
		case 1:
			if (selectionType == SelectionType.None)
			{
				break;
			}
			if (selectionType == SelectionType.Multiple && actionKey)
			{
				if (m_SelectedIds.Contains(idForIndex))
				{
					RemoveFromSelection(indexFromPosition);
				}
				else
				{
					AddToSelection(indexFromPosition);
				}
			}
			else if (selectionType == SelectionType.Multiple && shiftKey)
			{
				if (m_SelectedIndices.Count == 0)
				{
					SetSelection(indexFromPosition);
				}
				else
				{
					DoRangeSelection(indexFromPosition);
				}
			}
			else if (selectionType != SelectionType.Multiple || !m_SelectedIndices.Contains(indexFromPosition))
			{
				SetSelection(indexFromPosition);
			}
			break;
		case 2:
			if (this.onItemsChosen != null)
			{
				ProcessSingleClick(indexFromPosition);
			}
			this.onItemsChosen?.Invoke(m_SelectedItems);
			break;
		}
	}

	private void DoRangeSelection(int rangeSelectionFinalIndex)
	{
		int num = (m_IsRangeSelectionDirectionUp ? m_SelectedIndices.Max() : m_SelectedIndices.Min());
		ClearSelectionWithoutValidation();
		List<int> list = new List<int>();
		m_IsRangeSelectionDirectionUp = rangeSelectionFinalIndex < num;
		if (m_IsRangeSelectionDirectionUp)
		{
			for (int i = rangeSelectionFinalIndex; i <= num; i++)
			{
				list.Add(i);
			}
		}
		else
		{
			for (int num2 = rangeSelectionFinalIndex; num2 >= num; num2--)
			{
				list.Add(num2);
			}
		}
		AddToSelection(list);
	}

	private void ProcessSingleClick(int clickedIndex)
	{
		SetSelection(clickedIndex);
	}

	internal void SelectAll()
	{
		if (!HasValidDataAndBindings() || selectionType != SelectionType.Multiple)
		{
			return;
		}
		for (int i = 0; i < m_ViewController.itemsSource.Count; i++)
		{
			int idForIndex = viewController.GetIdForIndex(i);
			object itemForIndex = viewController.GetItemForIndex(i);
			foreach (ReusableCollectionItem activeItem in activeItems)
			{
				if (activeItem.id == idForIndex)
				{
					activeItem.SetSelected(selected: true);
				}
			}
			if (!m_SelectedIds.Contains(idForIndex))
			{
				m_SelectedIds.Add(idForIndex);
				m_SelectedIndices.Add(i);
				m_SelectedItems.Add(itemForIndex);
			}
		}
		NotifyOfSelectionChange();
		SaveViewData();
	}

	public void AddToSelection(int index)
	{
		AddToSelection(new int[1] { index });
	}

	internal void AddToSelection(IList<int> indexes)
	{
		if (!HasValidDataAndBindings() || indexes == null || indexes.Count == 0)
		{
			return;
		}
		foreach (int index in indexes)
		{
			AddToSelectionWithoutValidation(index);
		}
		NotifyOfSelectionChange();
		SaveViewData();
	}

	private void AddToSelectionWithoutValidation(int index)
	{
		if (m_SelectedIndices.Contains(index))
		{
			return;
		}
		int idForIndex = viewController.GetIdForIndex(index);
		object itemForIndex = viewController.GetItemForIndex(index);
		foreach (ReusableCollectionItem activeItem in activeItems)
		{
			if (activeItem.id == idForIndex)
			{
				activeItem.SetSelected(selected: true);
			}
		}
		m_SelectedIds.Add(idForIndex);
		m_SelectedIndices.Add(index);
		m_SelectedItems.Add(itemForIndex);
	}

	public void RemoveFromSelection(int index)
	{
		if (HasValidDataAndBindings())
		{
			RemoveFromSelectionWithoutValidation(index);
			NotifyOfSelectionChange();
			SaveViewData();
		}
	}

	private void RemoveFromSelectionWithoutValidation(int index)
	{
		if (!m_SelectedIndices.Contains(index))
		{
			return;
		}
		int idForIndex = viewController.GetIdForIndex(index);
		object itemForIndex = viewController.GetItemForIndex(index);
		foreach (ReusableCollectionItem activeItem in activeItems)
		{
			if (activeItem.id == idForIndex)
			{
				activeItem.SetSelected(selected: false);
			}
		}
		m_SelectedIds.Remove(idForIndex);
		m_SelectedIndices.Remove(index);
		m_SelectedItems.Remove(itemForIndex);
	}

	public void SetSelection(int index)
	{
		if (index < 0)
		{
			ClearSelection();
			return;
		}
		SetSelection(new int[1] { index });
	}

	public void SetSelection(IEnumerable<int> indices)
	{
		SetSelectionInternal(indices, sendNotification: true);
	}

	public void SetSelectionWithoutNotify(IEnumerable<int> indices)
	{
		SetSelectionInternal(indices, sendNotification: false);
	}

	internal void SetSelectionInternal(IEnumerable<int> indices, bool sendNotification)
	{
		if (!HasValidDataAndBindings() || indices == null)
		{
			return;
		}
		ClearSelectionWithoutValidation();
		foreach (int index in indices)
		{
			AddToSelectionWithoutValidation(index);
		}
		if (sendNotification)
		{
			NotifyOfSelectionChange();
		}
		SaveViewData();
	}

	private void NotifyOfSelectionChange()
	{
		if (HasValidDataAndBindings())
		{
			this.onSelectionChange?.Invoke(m_SelectedItems);
			this.onSelectedIndicesChange?.Invoke(m_SelectedIndices);
		}
	}

	public void ClearSelection()
	{
		if (HasValidDataAndBindings() && m_SelectedIds.Count != 0)
		{
			ClearSelectionWithoutValidation();
			NotifyOfSelectionChange();
		}
	}

	private void ClearSelectionWithoutValidation()
	{
		foreach (ReusableCollectionItem activeItem in activeItems)
		{
			activeItem.SetSelected(selected: false);
		}
		m_SelectedIds.Clear();
		m_SelectedIndices.Clear();
		m_SelectedItems.Clear();
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
		OverwriteFromViewData(this, fullHierarchicalViewDataKey);
	}

	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		if (evt.eventTypeId == EventBase<PointerUpEvent>.TypeId())
		{
			m_Dragger?.OnPointerUpEvent((PointerUpEvent)evt);
		}
		else if (evt.eventTypeId == EventBase<FocusEvent>.TypeId())
		{
			m_LastFocusedElementTreeChildIndexes.Clear();
			VisualElement element = evt.leafTarget as VisualElement;
			if (m_ScrollView.contentContainer.FindElementInTree(element, m_LastFocusedElementTreeChildIndexes))
			{
				VisualElement visualElement = m_ScrollView.contentContainer[m_LastFocusedElementTreeChildIndexes[0]];
				foreach (ReusableCollectionItem activeItem in activeItems)
				{
					if (activeItem.rootElement == visualElement)
					{
						m_LastFocusedElementIndex = activeItem.index;
						break;
					}
				}
				m_LastFocusedElementTreeChildIndexes.RemoveAt(0);
			}
			else
			{
				m_LastFocusedElementIndex = -1;
			}
		}
		else if (evt.eventTypeId == EventBase<NavigationSubmitEvent>.TypeId() && evt.target == this)
		{
			m_ScrollView.contentContainer.Focus();
		}
	}

	internal void HandleFocus(ReusableCollectionItem recycledItem)
	{
		if (m_LastFocusedElementIndex != -1)
		{
			if (m_LastFocusedElementIndex == recycledItem.index)
			{
				recycledItem.rootElement.ElementAtTreePath(m_LastFocusedElementTreeChildIndexes)?.Focus();
			}
			else
			{
				recycledItem.rootElement.ElementAtTreePath(m_LastFocusedElementTreeChildIndexes)?.Blur();
			}
		}
	}

	private void OnSizeChanged(GeometryChangedEvent evt)
	{
		if (HasValidDataAndBindings() && (!Mathf.Approximately(evt.newRect.width, evt.oldRect.width) || !Mathf.Approximately(evt.newRect.height, evt.oldRect.height)))
		{
			Resize(evt.newRect.size, evt.layoutPass);
		}
	}

	private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
	{
		if (!m_ItemHeightIsInline && e.customStyle.TryGetValue(s_ItemHeightProperty, out var value) && Math.Abs(m_FixedItemHeight - (float)value) > float.Epsilon)
		{
			m_FixedItemHeight = value;
			RefreshItems();
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		RefreshItems();
	}
}
