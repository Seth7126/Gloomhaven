#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace UnityEngine.UIElements;

internal class TreeView : VisualElement
{
	public new class UxmlFactory : UxmlFactory<TreeView, UxmlTraits>
	{
	}

	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		private readonly UxmlIntAttributeDescription m_ItemHeight = new UxmlIntAttributeDescription
		{
			name = "item-height",
			defaultValue = BaseVerticalCollectionView.s_DefaultItemHeight
		};

		private readonly UxmlBoolAttributeDescription m_ShowBorder = new UxmlBoolAttributeDescription
		{
			name = "show-border",
			defaultValue = false
		};

		private readonly UxmlEnumAttributeDescription<SelectionType> m_SelectionType = new UxmlEnumAttributeDescription<SelectionType>
		{
			name = "selection-type",
			defaultValue = SelectionType.Single
		};

		private readonly UxmlEnumAttributeDescription<AlternatingRowBackground> m_ShowAlternatingRowBackgrounds = new UxmlEnumAttributeDescription<AlternatingRowBackground>
		{
			name = "show-alternating-row-backgrounds",
			defaultValue = AlternatingRowBackground.None
		};

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			int value = 0;
			if (m_ItemHeight.TryGetValueFromBag(bag, cc, ref value))
			{
				((TreeView)ve).itemHeight = value;
			}
			((TreeView)ve).showBorder = m_ShowBorder.GetValueFromBag(bag, cc);
			((TreeView)ve).selectionType = m_SelectionType.GetValueFromBag(bag, cc);
			((TreeView)ve).showAlternatingRowBackgrounds = m_ShowAlternatingRowBackgrounds.GetValueFromBag(bag, cc);
		}
	}

	private struct TreeViewItemWrapper
	{
		public int depth;

		public ITreeViewItem item;

		public int id => item.id;

		public bool hasChildren => item.hasChildren;
	}

	private static readonly string s_ListViewName = "unity-tree-view__list-view";

	private static readonly string s_ItemName = "unity-tree-view__item";

	private static readonly string s_ItemToggleName = "unity-tree-view__item-toggle";

	private static readonly string s_ItemIndentsContainerName = "unity-tree-view__item-indents";

	private static readonly string s_ItemIndentName = "unity-tree-view__item-indent";

	private static readonly string s_ItemContentContainerName = "unity-tree-view__item-content";

	private Func<VisualElement> m_MakeItem;

	private List<ITreeViewItem> m_SelectedItems;

	private Action<VisualElement, ITreeViewItem> m_BindItem;

	private IList<ITreeViewItem> m_RootItems;

	[SerializeField]
	private List<int> m_ExpandedItemIds;

	private List<TreeViewItemWrapper> m_ItemWrappers;

	private readonly ListView m_ListView;

	private readonly ScrollView m_ScrollView;

	public Func<VisualElement> makeItem
	{
		get
		{
			return m_MakeItem;
		}
		set
		{
			if (m_MakeItem != value)
			{
				m_MakeItem = value;
				m_ListView.Rebuild();
			}
		}
	}

	public ITreeViewItem selectedItem => (m_SelectedItems.Count == 0) ? null : m_SelectedItems.First();

	public IEnumerable<ITreeViewItem> selectedItems
	{
		get
		{
			if (m_SelectedItems != null)
			{
				return m_SelectedItems;
			}
			m_SelectedItems = new List<ITreeViewItem>();
			foreach (ITreeViewItem item in items)
			{
				foreach (int currentSelectionId in m_ListView.currentSelectionIds)
				{
					if (item.id == currentSelectionId)
					{
						m_SelectedItems.Add(item);
					}
				}
			}
			return m_SelectedItems;
		}
	}

	public Action<VisualElement, ITreeViewItem> bindItem
	{
		get
		{
			return m_BindItem;
		}
		set
		{
			m_BindItem = value;
			ListViewRefresh();
		}
	}

	public Action<VisualElement, ITreeViewItem> unbindItem { get; set; }

	public IList<ITreeViewItem> rootItems
	{
		get
		{
			return m_RootItems;
		}
		set
		{
			m_RootItems = value;
			Rebuild();
		}
	}

	public IEnumerable<ITreeViewItem> items => GetAllItems(m_RootItems);

	public float resolvedItemHeight => m_ListView.ResolveItemHeight();

	public int itemHeight
	{
		get
		{
			return (int)m_ListView.fixedItemHeight;
		}
		set
		{
			m_ListView.fixedItemHeight = value;
		}
	}

	public bool horizontalScrollingEnabled
	{
		get
		{
			return m_ListView.horizontalScrollingEnabled;
		}
		set
		{
			m_ListView.horizontalScrollingEnabled = value;
		}
	}

	public bool showBorder
	{
		get
		{
			return m_ListView.showBorder;
		}
		set
		{
			m_ListView.showBorder = value;
		}
	}

	public SelectionType selectionType
	{
		get
		{
			return m_ListView.selectionType;
		}
		set
		{
			m_ListView.selectionType = value;
		}
	}

	public AlternatingRowBackground showAlternatingRowBackgrounds
	{
		get
		{
			return m_ListView.showAlternatingRowBackgrounds;
		}
		set
		{
			m_ListView.showAlternatingRowBackgrounds = value;
		}
	}

	public event Action<IEnumerable<ITreeViewItem>> onItemsChosen;

	public event Action<IEnumerable<ITreeViewItem>> onSelectionChange;

	public TreeView()
	{
		m_SelectedItems = null;
		m_ExpandedItemIds = new List<int>();
		m_ItemWrappers = new List<TreeViewItemWrapper>();
		m_ListView = new ListView();
		m_ListView.name = s_ListViewName;
		m_ListView.itemsSource = m_ItemWrappers;
		m_ListView.viewDataKey = s_ListViewName;
		m_ListView.AddToClassList(s_ListViewName);
		base.hierarchy.Add(m_ListView);
		m_ListView.makeItem = MakeTreeItem;
		m_ListView.bindItem = BindTreeItem;
		m_ListView.unbindItem = UnbindTreeItem;
		m_ListView.getItemId = GetItemId;
		m_ListView.onItemsChosen += OnItemsChosen;
		m_ListView.onSelectionChange += OnSelectionChange;
		m_ScrollView = m_ListView.scrollView;
		m_ScrollView.contentContainer.RegisterCallback<KeyDownEvent>(OnKeyDown);
		RegisterCallback<MouseUpEvent>(OnTreeViewMouseUp, TrickleDown.TrickleDown);
		RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
	}

	public TreeView(IList<ITreeViewItem> items, int fixedItemHeight, Func<VisualElement> makeItem, Action<VisualElement, ITreeViewItem> bindItem)
		: this()
	{
		m_ListView.fixedItemHeight = fixedItemHeight;
		m_MakeItem = makeItem;
		m_BindItem = bindItem;
		m_RootItems = items;
		Rebuild();
	}

	public void RefreshItems()
	{
		RegenerateWrappers();
		ListViewRefresh();
	}

	public void Rebuild()
	{
		RegenerateWrappers();
		m_ListView.Rebuild();
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
		OverwriteFromViewData(this, fullHierarchicalViewDataKey);
		Rebuild();
	}

	public static IEnumerable<ITreeViewItem> GetAllItems(IEnumerable<ITreeViewItem> rootItems)
	{
		if (rootItems == null)
		{
			yield break;
		}
		Stack<IEnumerator<ITreeViewItem>> iteratorStack = new Stack<IEnumerator<ITreeViewItem>>();
		IEnumerator<ITreeViewItem> currentIterator = rootItems.GetEnumerator();
		while (true)
		{
			if (!currentIterator.MoveNext())
			{
				if (iteratorStack.Count > 0)
				{
					currentIterator = iteratorStack.Pop();
					continue;
				}
				break;
			}
			ITreeViewItem currentItem = currentIterator.Current;
			yield return currentItem;
			if (currentItem.hasChildren)
			{
				iteratorStack.Push(currentIterator);
				currentIterator = currentItem.children.GetEnumerator();
			}
		}
	}

	public void OnKeyDown(KeyDownEvent evt)
	{
		int selectedIndex = m_ListView.selectedIndex;
		bool flag = true;
		switch (evt.keyCode)
		{
		case KeyCode.RightArrow:
			if (!IsExpandedByIndex(selectedIndex))
			{
				ExpandItemByIndex(selectedIndex);
			}
			break;
		case KeyCode.LeftArrow:
			if (IsExpandedByIndex(selectedIndex))
			{
				CollapseItemByIndex(selectedIndex);
			}
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			evt.StopPropagation();
		}
	}

	public void SetSelection(int id)
	{
		SetSelection(new int[1] { id });
	}

	public void SetSelection(IEnumerable<int> ids)
	{
		SetSelectionInternal(ids, sendNotification: true);
	}

	public void SetSelectionWithoutNotify(IEnumerable<int> ids)
	{
		SetSelectionInternal(ids, sendNotification: false);
	}

	internal void SetSelectionInternal(IEnumerable<int> ids, bool sendNotification)
	{
		if (ids != null)
		{
			List<int> indices = ids.Select((int id) => GetItemIndex(id, expand: true)).ToList();
			ListViewRefresh();
			m_ListView.SetSelectionInternal(indices, sendNotification);
		}
	}

	public void AddToSelection(int id)
	{
		int itemIndex = GetItemIndex(id, expand: true);
		ListViewRefresh();
		m_ListView.AddToSelection(itemIndex);
	}

	public void RemoveFromSelection(int id)
	{
		int itemIndex = GetItemIndex(id);
		m_ListView.RemoveFromSelection(itemIndex);
	}

	private int GetItemIndex(int id, bool expand = false)
	{
		ITreeViewItem treeViewItem = FindItem(id);
		if (treeViewItem == null)
		{
			throw new ArgumentOutOfRangeException("id", id, "TreeView: Item id not found.");
		}
		if (expand)
		{
			bool flag = false;
			for (ITreeViewItem treeViewItem2 = treeViewItem.parent; treeViewItem2 != null; treeViewItem2 = treeViewItem2.parent)
			{
				if (!m_ExpandedItemIds.Contains(treeViewItem2.id))
				{
					m_ExpandedItemIds.Add(treeViewItem2.id);
					flag = true;
				}
			}
			if (flag)
			{
				RegenerateWrappers();
			}
		}
		int i;
		for (i = 0; i < m_ItemWrappers.Count && m_ItemWrappers[i].id != id; i++)
		{
		}
		return i;
	}

	public void ClearSelection()
	{
		m_ListView.ClearSelection();
	}

	public void ScrollTo(VisualElement visualElement)
	{
		m_ListView.ScrollTo(visualElement);
	}

	public void ScrollToItem(int id)
	{
		int itemIndex = GetItemIndex(id, expand: true);
		RefreshItems();
		m_ListView.ScrollToItem(itemIndex);
	}

	internal void CopyExpandedStates(ITreeViewItem source, ITreeViewItem target)
	{
		if (IsExpanded(source.id))
		{
			ExpandItem(target.id);
			if (source.children == null || source.children.Count() <= 0)
			{
				return;
			}
			if (target.children == null || source.children.Count() != target.children.Count())
			{
				Debug.LogWarning("Source and target hierarchies are not the same");
				return;
			}
			for (int i = 0; i < source.children.Count(); i++)
			{
				ITreeViewItem source2 = source.children.ElementAt(i);
				ITreeViewItem target2 = target.children.ElementAt(i);
				CopyExpandedStates(source2, target2);
			}
		}
		else
		{
			CollapseItem(target.id);
		}
	}

	public bool IsExpanded(int id)
	{
		return m_ExpandedItemIds.Contains(id);
	}

	public void CollapseItem(int id)
	{
		if (FindItem(id) == null)
		{
			throw new ArgumentOutOfRangeException("id", id, "TreeView: Item id not found.");
		}
		for (int i = 0; i < m_ItemWrappers.Count; i++)
		{
			if (m_ItemWrappers[i].item.id == id && IsExpandedByIndex(i))
			{
				CollapseItemByIndex(i);
				return;
			}
		}
		if (m_ExpandedItemIds.Contains(id))
		{
			m_ExpandedItemIds.Remove(id);
			RefreshItems();
		}
	}

	public void ExpandItem(int id)
	{
		if (FindItem(id) == null)
		{
			throw new ArgumentOutOfRangeException("id", id, "TreeView: Item id not found.");
		}
		for (int i = 0; i < m_ItemWrappers.Count; i++)
		{
			if (m_ItemWrappers[i].item.id == id && !IsExpandedByIndex(i))
			{
				ExpandItemByIndex(i);
				return;
			}
		}
		if (!m_ExpandedItemIds.Contains(id))
		{
			m_ExpandedItemIds.Add(id);
			RefreshItems();
		}
	}

	public ITreeViewItem FindItem(int id)
	{
		foreach (ITreeViewItem item in items)
		{
			if (item.id == id)
			{
				return item;
			}
		}
		return null;
	}

	private void ListViewRefresh()
	{
		m_ListView.RefreshItems();
	}

	private void OnItemsChosen(IEnumerable<object> chosenItems)
	{
		if (this.onItemsChosen == null)
		{
			return;
		}
		List<ITreeViewItem> list = new List<ITreeViewItem>();
		foreach (object chosenItem in chosenItems)
		{
			list.Add(((TreeViewItemWrapper)chosenItem).item);
		}
		this.onItemsChosen(list);
	}

	private void OnSelectionChange(IEnumerable<object> selectedListItems)
	{
		if (m_SelectedItems == null)
		{
			m_SelectedItems = new List<ITreeViewItem>();
		}
		m_SelectedItems.Clear();
		foreach (object selectedListItem in selectedListItems)
		{
			m_SelectedItems.Add(((TreeViewItemWrapper)selectedListItem).item);
		}
		this.onSelectionChange?.Invoke(m_SelectedItems);
	}

	private void OnTreeViewMouseUp(MouseUpEvent evt)
	{
		m_ScrollView.contentContainer.Focus();
	}

	private void OnItemMouseUp(MouseUpEvent evt)
	{
		if ((evt.modifiers & EventModifiers.Alt) == 0)
		{
			return;
		}
		VisualElement e = evt.currentTarget as VisualElement;
		Toggle toggle = e.Q<Toggle>(s_ItemToggleName);
		int index = (int)toggle.userData;
		ITreeViewItem item = m_ItemWrappers[index].item;
		bool flag = IsExpandedByIndex(index);
		if (!item.hasChildren)
		{
			return;
		}
		HashSet<int> hashSet = new HashSet<int>(m_ExpandedItemIds);
		if (flag)
		{
			hashSet.Remove(item.id);
		}
		else
		{
			hashSet.Add(item.id);
		}
		foreach (ITreeViewItem allItem in GetAllItems(item.children))
		{
			if (allItem.hasChildren)
			{
				if (flag)
				{
					hashSet.Remove(allItem.id);
				}
				else
				{
					hashSet.Add(allItem.id);
				}
			}
		}
		m_ExpandedItemIds = hashSet.ToList();
		RefreshItems();
		evt.StopPropagation();
	}

	private VisualElement MakeTreeItem()
	{
		VisualElement visualElement = new VisualElement();
		visualElement.name = s_ItemName;
		visualElement.style.flexDirection = FlexDirection.Row;
		VisualElement visualElement2 = visualElement;
		visualElement2.AddToClassList(s_ItemName);
		visualElement2.RegisterCallback<MouseUpEvent>(OnItemMouseUp);
		VisualElement visualElement3 = new VisualElement();
		visualElement3.name = s_ItemIndentsContainerName;
		visualElement3.style.flexDirection = FlexDirection.Row;
		VisualElement visualElement4 = visualElement3;
		visualElement4.AddToClassList(s_ItemIndentsContainerName);
		visualElement2.hierarchy.Add(visualElement4);
		Toggle toggle = new Toggle
		{
			name = s_ItemToggleName
		};
		toggle.AddToClassList(Foldout.toggleUssClassName);
		toggle.RegisterValueChangedCallback(ToggleExpandedState);
		visualElement2.hierarchy.Add(toggle);
		VisualElement visualElement5 = new VisualElement();
		visualElement5.name = s_ItemContentContainerName;
		visualElement5.style.flexGrow = 1f;
		VisualElement visualElement6 = visualElement5;
		visualElement6.AddToClassList(s_ItemContentContainerName);
		visualElement2.Add(visualElement6);
		if (m_MakeItem != null)
		{
			visualElement6.Add(m_MakeItem());
		}
		return visualElement2;
	}

	private void UnbindTreeItem(VisualElement element, int index)
	{
		if (unbindItem != null)
		{
			ITreeViewItem item = m_ItemWrappers[index].item;
			VisualElement arg = element.Q(s_ItemContentContainerName).ElementAt(0);
			unbindItem(arg, item);
		}
	}

	private void BindTreeItem(VisualElement element, int index)
	{
		ITreeViewItem item = m_ItemWrappers[index].item;
		VisualElement visualElement = element.Q(s_ItemIndentsContainerName);
		visualElement.Clear();
		for (int i = 0; i < m_ItemWrappers[index].depth; i++)
		{
			VisualElement visualElement2 = new VisualElement();
			visualElement2.AddToClassList(s_ItemIndentName);
			visualElement.Add(visualElement2);
		}
		Toggle toggle = element.Q<Toggle>(s_ItemToggleName);
		toggle.SetValueWithoutNotify(IsExpandedByIndex(index));
		toggle.userData = index;
		if (item.hasChildren)
		{
			toggle.visible = true;
		}
		else
		{
			toggle.visible = false;
		}
		if (m_BindItem != null)
		{
			VisualElement arg = element.Q(s_ItemContentContainerName).ElementAt(0);
			m_BindItem(arg, item);
		}
	}

	private int GetItemId(int index)
	{
		return m_ItemWrappers[index].id;
	}

	private bool IsExpandedByIndex(int index)
	{
		return m_ExpandedItemIds.Contains(m_ItemWrappers[index].id);
	}

	private void CollapseItemByIndex(int index)
	{
		if (m_ItemWrappers[index].item.hasChildren)
		{
			m_ExpandedItemIds.Remove(m_ItemWrappers[index].item.id);
			int num = 0;
			int i = index + 1;
			for (int depth = m_ItemWrappers[index].depth; i < m_ItemWrappers.Count && m_ItemWrappers[i].depth > depth; i++)
			{
				num++;
			}
			m_ItemWrappers.RemoveRange(index + 1, num);
			ListViewRefresh();
			SaveViewData();
		}
	}

	private void ExpandItemByIndex(int index)
	{
		if (m_ItemWrappers[index].item.hasChildren)
		{
			List<TreeViewItemWrapper> wrappers = new List<TreeViewItemWrapper>();
			CreateWrappers(m_ItemWrappers[index].item.children, m_ItemWrappers[index].depth + 1, ref wrappers);
			m_ItemWrappers.InsertRange(index + 1, wrappers);
			m_ExpandedItemIds.Add(m_ItemWrappers[index].item.id);
			ListViewRefresh();
			SaveViewData();
		}
	}

	private void ToggleExpandedState(ChangeEvent<bool> evt)
	{
		Toggle toggle = evt.target as Toggle;
		int index = (int)toggle.userData;
		bool flag = IsExpandedByIndex(index);
		Assert.AreNotEqual(flag, evt.newValue);
		if (flag)
		{
			CollapseItemByIndex(index);
		}
		else
		{
			ExpandItemByIndex(index);
		}
		m_ScrollView.contentContainer.Focus();
	}

	private void CreateWrappers(IEnumerable<ITreeViewItem> treeViewItems, int depth, ref List<TreeViewItemWrapper> wrappers)
	{
		foreach (ITreeViewItem treeViewItem in treeViewItems)
		{
			TreeViewItemWrapper item = new TreeViewItemWrapper
			{
				depth = depth,
				item = treeViewItem
			};
			wrappers.Add(item);
			if (m_ExpandedItemIds.Contains(treeViewItem.id) && treeViewItem.hasChildren)
			{
				CreateWrappers(treeViewItem.children, depth + 1, ref wrappers);
			}
		}
	}

	private void RegenerateWrappers()
	{
		m_ItemWrappers.Clear();
		if (m_RootItems != null)
		{
			CreateWrappers(m_RootItems, 0, ref m_ItemWrappers);
		}
	}

	private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
	{
		float fixedItemHeight = m_ListView.fixedItemHeight;
		if (!m_ListView.m_ItemHeightIsInline && e.customStyle.TryGetValue(BaseVerticalCollectionView.s_ItemHeightProperty, out var value))
		{
			m_ListView.m_FixedItemHeight = value;
		}
		if (m_ListView.m_FixedItemHeight != fixedItemHeight)
		{
			m_ListView.RefreshItems();
		}
	}
}
