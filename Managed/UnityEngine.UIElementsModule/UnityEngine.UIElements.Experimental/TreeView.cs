using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements.Experimental;

internal class TreeView : BaseVerticalCollectionView
{
	public new class UxmlFactory : UxmlFactory<TreeView, UxmlTraits>
	{
	}

	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		private readonly UxmlIntAttributeDescription m_FixedItemHeight = new UxmlIntAttributeDescription
		{
			name = "fixed-item-height",
			obsoleteNames = new string[1] { "item-height" },
			defaultValue = BaseVerticalCollectionView.s_DefaultItemHeight
		};

		private readonly UxmlEnumAttributeDescription<CollectionVirtualizationMethod> m_VirtualizationMethod = new UxmlEnumAttributeDescription<CollectionVirtualizationMethod>
		{
			name = "virtualization-method",
			defaultValue = CollectionVirtualizationMethod.FixedHeight
		};

		private readonly UxmlBoolAttributeDescription m_ShowBorder = new UxmlBoolAttributeDescription
		{
			name = "show-border",
			defaultValue = false
		};

		private readonly UxmlBoolAttributeDescription m_AutoExpand = new UxmlBoolAttributeDescription
		{
			name = "auto-expand",
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
			if (m_FixedItemHeight.TryGetValueFromBag(bag, cc, ref value))
			{
				((TreeView)ve).fixedItemHeight = value;
			}
			((TreeView)ve).virtualizationMethod = m_VirtualizationMethod.GetValueFromBag(bag, cc);
			((TreeView)ve).autoExpand = m_AutoExpand.GetValueFromBag(bag, cc);
			((TreeView)ve).showBorder = m_ShowBorder.GetValueFromBag(bag, cc);
			((TreeView)ve).selectionType = m_SelectionType.GetValueFromBag(bag, cc);
			((TreeView)ve).showAlternatingRowBackgrounds = m_ShowAlternatingRowBackgrounds.GetValueFromBag(bag, cc);
		}
	}

	public new static readonly string ussClassName = "unity-tree-view";

	public new static readonly string itemUssClassName = ussClassName + "__item";

	public static readonly string itemToggleUssClassName = ussClassName + "__item-toggle";

	public static readonly string itemIndentsContainerUssClassName = ussClassName + "__item-indents";

	public static readonly string itemIndentUssClassName = ussClassName + "__item-indent";

	public static readonly string itemContentContainerUssClassName = ussClassName + "__item-content";

	private bool m_AutoExpand;

	[SerializeField]
	private List<int> m_ExpandedItemIds;

	public new IList itemsSource
	{
		get
		{
			return viewController.itemsSource;
		}
		internal set
		{
			GetOrCreateViewController().itemsSource = value;
		}
	}

	internal new TreeViewController viewController => base.viewController as TreeViewController;

	public bool autoExpand
	{
		get
		{
			return m_AutoExpand;
		}
		set
		{
			m_AutoExpand = value;
			viewController?.RegenerateWrappers();
			RefreshItems();
		}
	}

	internal List<int> expandedItemIds
	{
		get
		{
			return m_ExpandedItemIds;
		}
		set
		{
			m_ExpandedItemIds = value;
		}
	}

	public void SetRootItems<T>(IList<TreeViewItemData<T>> rootItems)
	{
		if (base.viewController is DefaultTreeViewController<T> defaultTreeViewController)
		{
			defaultTreeViewController.SetRootItems(rootItems);
			return;
		}
		DefaultTreeViewController<T> defaultTreeViewController2 = new DefaultTreeViewController<T>();
		SetViewController(defaultTreeViewController2);
		defaultTreeViewController2.SetRootItems(rootItems);
	}

	public IEnumerable<int> GetRootIds()
	{
		return viewController.GetRootItemIds();
	}

	public int GetTreeCount()
	{
		return viewController.GetTreeCount();
	}

	private protected override void CreateVirtualizationController()
	{
		CreateVirtualizationController<ReusableTreeViewItem>();
	}

	private protected override void CreateViewController()
	{
		SetViewController(new DefaultTreeViewController<object>());
	}

	internal void SetViewController(TreeViewController controller)
	{
		if (viewController != null)
		{
			controller.itemIndexChanged -= OnItemIndexChanged;
		}
		SetViewController((CollectionViewController)controller);
		RefreshItems();
		if (controller != null)
		{
			controller.itemIndexChanged += OnItemIndexChanged;
		}
	}

	private void OnItemIndexChanged(int srcIndex, int dstIndex)
	{
		RefreshItems();
	}

	internal override ICollectionDragAndDropController CreateDragAndDropController()
	{
		return new TreeViewReorderableDragAndDropController(this);
	}

	public TreeView()
	{
		m_ExpandedItemIds = new List<int>();
		base.name = ussClassName;
		base.viewDataKey = ussClassName;
		AddToClassList(ussClassName);
		base.scrollView.contentContainer.RegisterCallback<KeyDownEvent>(OnScrollViewKeyDown);
		RegisterCallback<MouseUpEvent>(OnTreeViewMouseUp, TrickleDown.TrickleDown);
	}

	public int GetIdForIndex(int index)
	{
		return viewController.GetIdForIndex(index);
	}

	public int GetParentIdForIndex(int index)
	{
		return viewController.GetParentId(GetIdForIndex(index));
	}

	public IEnumerable<int> GetChildrenIdsForIndex(int index)
	{
		return viewController.GetChildrenIdsByIndex(GetIdForIndex(index));
	}

	public T GetItemDataForIndex<T>(int index)
	{
		if (viewController is DefaultTreeViewController<T> defaultTreeViewController)
		{
			return defaultTreeViewController.GetDataForIndex(index);
		}
		object obj = viewController?.GetItemForIndex(index);
		Type type = obj?.GetType();
		if ((object)type == typeof(T))
		{
			return (T)obj;
		}
		if ((object)type == null && (object)viewController?.GetType().GetGenericTypeDefinition() == typeof(DefaultTreeViewController<>))
		{
			type = viewController.GetType().GetGenericArguments()[0];
		}
		throw new ArgumentException($"Type parameter ({typeof(T)}) differs from data source ({type}) and is not recognized by the controller.");
	}

	public T GetItemDataForId<T>(int id)
	{
		if (viewController is DefaultTreeViewController<T> defaultTreeViewController)
		{
			return defaultTreeViewController.GetDataForId(id);
		}
		object obj = viewController?.GetItemForIndex(viewController.GetIndexForId(id));
		Type type = obj?.GetType();
		if ((object)type == typeof(T))
		{
			return (T)obj;
		}
		if ((object)type == null && (object)viewController?.GetType().GetGenericTypeDefinition() == typeof(DefaultTreeViewController<>))
		{
			type = viewController.GetType().GetGenericArguments()[0];
		}
		throw new ArgumentException($"Type parameter ({typeof(T)}) differs from data source ({type}) and is not recognized by the controller.");
	}

	public void AddItem<T>(TreeViewItemData<T> item, int parentId = -1, int childIndex = -1)
	{
		if (viewController is DefaultTreeViewController<T> defaultTreeViewController)
		{
			defaultTreeViewController.AddItem(in item, parentId, childIndex);
			RefreshItems();
		}
		Type arg = null;
		if ((object)viewController?.GetType().GetGenericTypeDefinition() == typeof(DefaultTreeViewController<>))
		{
			arg = viewController.GetType().GetGenericArguments()[0];
		}
		throw new ArgumentException($"Type parameter ({typeof(T)}) differs from data source ({arg})and is not recognized by the controller.");
	}

	public bool TryRemoveItem(int id)
	{
		if (viewController.TryRemoveItem(id))
		{
			RefreshItems();
			return true;
		}
		return false;
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		if (viewController != null)
		{
			viewController.RebuildTree();
			RefreshItems();
		}
	}

	private void OnScrollViewKeyDown(KeyDownEvent evt)
	{
		int index = base.selectedIndex;
		bool flag = true;
		switch (evt.keyCode)
		{
		case KeyCode.RightArrow:
			if (evt.altKey || !IsExpandedByIndex(index))
			{
				ExpandItemByIndex(index, evt.altKey);
			}
			break;
		case KeyCode.LeftArrow:
			if (evt.altKey || IsExpandedByIndex(index))
			{
				CollapseItemByIndex(index, evt.altKey);
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

	public void SetSelectionById(int id)
	{
		SetSelectionById(new int[1] { id });
	}

	public void SetSelectionById(IEnumerable<int> ids)
	{
		SetSelectionInternalById(ids, sendNotification: true);
	}

	public void SetSelectionByIdWithoutNotify(IEnumerable<int> ids)
	{
		SetSelectionInternalById(ids, sendNotification: false);
	}

	internal void SetSelectionInternalById(IEnumerable<int> ids, bool sendNotification)
	{
		if (ids != null)
		{
			List<int> indices = ids.Select(delegate(int id)
			{
				viewController.ExpandItem(id, expandAllChildren: false);
				return viewController.GetIndexForId(id);
			}).ToList();
			SetSelectionInternal(indices, sendNotification);
		}
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
		return viewController.IsExpanded(id);
	}

	public void CollapseItem(int id, bool collapseAllChildren = false)
	{
		viewController.CollapseItem(id, collapseAllChildren);
		RefreshItems();
	}

	public void ExpandItem(int id, bool expandAllChildren = false)
	{
		viewController.ExpandItem(id, expandAllChildren);
		RefreshItems();
	}

	public void ExpandRootItems()
	{
		foreach (int rootItemId in viewController.GetRootItemIds())
		{
			viewController.ExpandItem(rootItemId, expandAllChildren: false);
		}
		RefreshItems();
	}

	public void ExpandAll()
	{
		viewController.ExpandAll();
	}

	public void CollapseAll()
	{
		viewController.CollapseAll();
	}

	private void OnTreeViewMouseUp(MouseUpEvent evt)
	{
		base.scrollView.contentContainer.Focus();
	}

	private bool IsExpandedByIndex(int index)
	{
		return viewController.IsExpandedByIndex(index);
	}

	private void CollapseItemByIndex(int index, bool collapseAll)
	{
		if (viewController.HasChildrenByIndex(index))
		{
			viewController.CollapseItemByIndex(index, collapseAll);
			RefreshItems();
			SaveViewData();
		}
	}

	private void ExpandItemByIndex(int index, bool expandAll)
	{
		if (viewController.HasChildrenByIndex(index))
		{
			viewController.ExpandItemByIndex(index, expandAll);
			RefreshItems();
			SaveViewData();
		}
	}
}
