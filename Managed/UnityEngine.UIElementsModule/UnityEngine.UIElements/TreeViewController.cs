using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.UIElements;

internal abstract class TreeViewController : CollectionViewController
{
	private Dictionary<int, TreeItem> m_TreeItems = new Dictionary<int, TreeItem>();

	private List<int> m_RootIndices = new List<int>();

	private List<TreeViewItemWrapper> m_ItemWrappers = new List<TreeViewItemWrapper>();

	private List<TreeViewItemWrapper> m_WrapperInsertionList = new List<TreeViewItemWrapper>();

	protected UnityEngine.UIElements.Experimental.TreeView treeView => base.view as UnityEngine.UIElements.Experimental.TreeView;

	public void RebuildTree()
	{
		m_TreeItems.Clear();
		m_RootIndices.Clear();
		foreach (int allItemId in GetAllItemIds())
		{
			int parentId = GetParentId(allItemId);
			if (parentId == -1)
			{
				m_RootIndices.Add(allItemId);
			}
			m_TreeItems.Add(allItemId, new TreeItem(allItemId, parentId, GetChildrenIds(allItemId)));
		}
		RegenerateWrappers();
	}

	public IEnumerable<int> GetRootItemIds()
	{
		return m_RootIndices;
	}

	public abstract IEnumerable<int> GetAllItemIds(IEnumerable<int> rootIds = null);

	public abstract int GetParentId(int id);

	public abstract IEnumerable<int> GetChildrenIds(int id);

	public abstract void Move(int id, int newParentId, int childIndex = -1);

	public abstract bool TryRemoveItem(int id);

	internal override void InvokeMakeItem(ReusableCollectionItem reusableItem)
	{
		ReusableTreeViewItem treeItem = reusableItem as ReusableTreeViewItem;
		if (treeItem == null)
		{
			return;
		}
		treeItem.Init(MakeItem());
		treeItem.onPointerUp += OnItemPointerUp;
		treeItem.onToggleValueChanged += ToggleExpandedState;
		if (treeView.autoExpand)
		{
			treeView.expandedItemIds.Remove(treeItem.id);
			treeView.schedule.Execute((Action)delegate
			{
				ExpandItem(treeItem.id, expandAllChildren: true);
			});
		}
	}

	internal override void InvokeBindItem(ReusableCollectionItem reusableItem, int index)
	{
		if (reusableItem is ReusableTreeViewItem reusableTreeViewItem)
		{
			reusableTreeViewItem.Indent(GetIndentationDepth(index));
			reusableTreeViewItem.SetExpandedWithoutNotify(IsExpandedByIndex(index));
			reusableTreeViewItem.SetToggleVisibility(HasChildrenByIndex(index));
		}
		base.InvokeBindItem(reusableItem, index);
	}

	internal override void InvokeDestroyItem(ReusableCollectionItem reusableItem)
	{
		if (reusableItem is ReusableTreeViewItem reusableTreeViewItem)
		{
			reusableTreeViewItem.onPointerUp -= OnItemPointerUp;
			reusableTreeViewItem.onToggleValueChanged -= ToggleExpandedState;
		}
		base.InvokeDestroyItem(reusableItem);
	}

	private void OnItemPointerUp(PointerUpEvent evt)
	{
		if ((evt.modifiers & EventModifiers.Alt) == 0)
		{
			return;
		}
		VisualElement e = evt.currentTarget as VisualElement;
		Toggle toggle = e.Q<Toggle>(UnityEngine.UIElements.Experimental.TreeView.itemToggleUssClassName);
		int index = ((ReusableTreeViewItem)toggle.userData).index;
		int idForIndex = GetIdForIndex(index);
		bool flag = IsExpandedByIndex(index);
		if (!HasChildrenByIndex(index))
		{
			return;
		}
		HashSet<int> hashSet = new HashSet<int>(treeView.expandedItemIds);
		if (flag)
		{
			hashSet.Remove(idForIndex);
		}
		else
		{
			hashSet.Add(idForIndex);
		}
		IEnumerable<int> childrenIdsByIndex = GetChildrenIdsByIndex(index);
		foreach (int allItemId in GetAllItemIds(childrenIdsByIndex))
		{
			if (HasChildren(allItemId))
			{
				if (flag)
				{
					hashSet.Remove(allItemId);
				}
				else
				{
					hashSet.Add(allItemId);
				}
			}
		}
		treeView.expandedItemIds = hashSet.ToList();
		RegenerateWrappers();
		treeView.RefreshItems();
		evt.StopPropagation();
	}

	private void ToggleExpandedState(ChangeEvent<bool> evt)
	{
		Toggle toggle = evt.target as Toggle;
		int index = ((ReusableTreeViewItem)toggle.userData).index;
		if (IsExpandedByIndex(index))
		{
			CollapseItemByIndex(index, collapseAllChildren: false);
		}
		else
		{
			ExpandItemByIndex(index, expandAllChildren: false);
		}
		treeView.scrollView.contentContainer.Focus();
	}

	public override int GetItemCount()
	{
		return m_ItemWrappers?.Count ?? 0;
	}

	public virtual int GetTreeCount()
	{
		return m_TreeItems.Count;
	}

	public override int GetIndexForId(int id)
	{
		for (int i = 0; i < m_ItemWrappers.Count; i++)
		{
			if (m_ItemWrappers[i].id == id)
			{
				return i;
			}
		}
		return -1;
	}

	public override int GetIdForIndex(int index)
	{
		return IsIndexValid(index) ? m_ItemWrappers[index].id : (-1);
	}

	public virtual bool HasChildren(int id)
	{
		if (m_TreeItems.TryGetValue(id, out var value))
		{
			return value.hasChildren;
		}
		return false;
	}

	public bool HasChildrenByIndex(int index)
	{
		return IsIndexValid(index) && m_ItemWrappers[index].hasChildren;
	}

	public IEnumerable<int> GetChildrenIdsByIndex(int index)
	{
		return IsIndexValid(index) ? m_ItemWrappers[index].childrenIds : null;
	}

	public int GetChildIndexForId(int id)
	{
		if (!m_TreeItems.TryGetValue(id, out var value))
		{
			return -1;
		}
		int num = 0;
		IEnumerable<int> enumerable;
		if (!m_TreeItems.TryGetValue(value.parentId, out var value2))
		{
			IEnumerable<int> rootIndices = m_RootIndices;
			enumerable = rootIndices;
		}
		else
		{
			enumerable = value2.childrenIds;
		}
		IEnumerable<int> enumerable2 = enumerable;
		foreach (int item in enumerable2)
		{
			if (item == id)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private int GetIndentationDepth(int index)
	{
		return IsIndexValid(index) ? m_ItemWrappers[index].depth : 0;
	}

	public bool IsExpanded(int id)
	{
		return treeView.expandedItemIds.Contains(id);
	}

	public bool IsExpandedByIndex(int index)
	{
		if (!IsIndexValid(index))
		{
			return false;
		}
		return IsExpanded(m_ItemWrappers[index].id);
	}

	public void ExpandItemByIndex(int index, bool expandAllChildren, bool refresh = true)
	{
		if (!HasChildrenByIndex(index))
		{
			return;
		}
		if (!treeView.expandedItemIds.Contains(GetIdForIndex(index)) || expandAllChildren)
		{
			IEnumerable<int> childrenIdsByIndex = GetChildrenIdsByIndex(index);
			List<int> list = new List<int>();
			foreach (int childId in childrenIdsByIndex)
			{
				if (m_ItemWrappers.All((TreeViewItemWrapper x) => x.id != childId))
				{
					list.Add(childId);
				}
			}
			CreateWrappers(list, GetIndentationDepth(index) + 1, ref m_WrapperInsertionList);
			m_ItemWrappers.InsertRange(index + 1, m_WrapperInsertionList);
			if (!treeView.expandedItemIds.Contains(m_ItemWrappers[index].id))
			{
				treeView.expandedItemIds.Add(m_ItemWrappers[index].id);
			}
			m_WrapperInsertionList.Clear();
		}
		if (expandAllChildren)
		{
			int idForIndex = GetIdForIndex(index);
			IEnumerable<int> childrenIds = GetChildrenIds(idForIndex);
			foreach (int allItemId in GetAllItemIds(childrenIds))
			{
				if (!treeView.expandedItemIds.Contains(allItemId))
				{
					ExpandItemByIndex(GetIndexForId(allItemId), expandAllChildren: true, refresh: false);
				}
			}
		}
		if (refresh)
		{
			treeView.RefreshItems();
		}
	}

	public void ExpandItem(int id, bool expandAllChildren)
	{
		if (!HasChildren(id))
		{
			return;
		}
		for (int i = 0; i < m_ItemWrappers.Count; i++)
		{
			if (m_ItemWrappers[i].id == id && (expandAllChildren || !IsExpandedByIndex(i)))
			{
				ExpandItemByIndex(i, expandAllChildren);
				return;
			}
		}
		if (!treeView.expandedItemIds.Contains(id))
		{
			treeView.expandedItemIds.Add(id);
		}
	}

	public void CollapseItemByIndex(int index, bool collapseAllChildren)
	{
		if (!HasChildrenByIndex(index))
		{
			return;
		}
		if (collapseAllChildren)
		{
			int idForIndex = GetIdForIndex(index);
			IEnumerable<int> childrenIds = GetChildrenIds(idForIndex);
			foreach (int allItemId in GetAllItemIds(childrenIds))
			{
				treeView.expandedItemIds.Remove(allItemId);
			}
		}
		treeView.expandedItemIds.Remove(GetIdForIndex(index));
		int num = 0;
		int i = index + 1;
		for (int indentationDepth = GetIndentationDepth(index); i < m_ItemWrappers.Count && GetIndentationDepth(i) > indentationDepth; i++)
		{
			num++;
		}
		m_ItemWrappers.RemoveRange(index + 1, num);
		treeView.RefreshItems();
	}

	public void CollapseItem(int id, bool collapseAllChildren)
	{
		for (int i = 0; i < m_ItemWrappers.Count; i++)
		{
			if (m_ItemWrappers[i].id == id && IsExpandedByIndex(i))
			{
				CollapseItemByIndex(i, collapseAllChildren);
				return;
			}
		}
		if (treeView.expandedItemIds.Contains(id))
		{
			treeView.expandedItemIds.Remove(id);
		}
	}

	public void ExpandAll()
	{
		foreach (int allItemId in GetAllItemIds())
		{
			if (!treeView.expandedItemIds.Contains(allItemId))
			{
				treeView.expandedItemIds.Add(allItemId);
			}
		}
		RegenerateWrappers();
		treeView.RefreshItems();
	}

	public void CollapseAll()
	{
		if (treeView.expandedItemIds.Count != 0)
		{
			treeView.expandedItemIds.Clear();
			RegenerateWrappers();
			treeView.RefreshItems();
		}
	}

	internal void RegenerateWrappers()
	{
		m_ItemWrappers.Clear();
		IEnumerable<int> rootItemIds = GetRootItemIds();
		if (rootItemIds != null)
		{
			CreateWrappers(rootItemIds, 0, ref m_ItemWrappers);
			SetItemsSourceWithoutNotify(m_ItemWrappers);
		}
	}

	private void CreateWrappers(IEnumerable<int> treeViewItemIds, int depth, ref List<TreeViewItemWrapper> wrappers)
	{
		foreach (int treeViewItemId in treeViewItemIds)
		{
			TreeViewItemWrapper item = new TreeViewItemWrapper(m_TreeItems[treeViewItemId], depth);
			wrappers.Add(item);
			if (treeView?.expandedItemIds != null && treeView.expandedItemIds.Contains(item.id) && item.hasChildren)
			{
				CreateWrappers(GetChildrenIds(item.id), depth + 1, ref wrappers);
			}
		}
	}

	private bool IsIndexValid(int index)
	{
		return index >= 0 && index < m_ItemWrappers.Count;
	}
}
