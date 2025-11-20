using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal sealed class DefaultTreeViewController<T> : TreeViewController
{
	private TreeData<T> m_TreeData;

	private Stack<IEnumerator<int>> m_IteratorStack = new Stack<IEnumerator<int>>();

	public void SetRootItems(IList<TreeViewItemData<T>> items)
	{
		m_TreeData = new TreeData<T>(items);
		RebuildTree();
	}

	public void AddItem(in TreeViewItemData<T> item, int parentId, int childIndex)
	{
		m_TreeData.AddItem(item, parentId, childIndex);
		RebuildTree();
	}

	public override bool TryRemoveItem(int id)
	{
		if (m_TreeData.TryRemove(id))
		{
			RebuildTree();
			return true;
		}
		return false;
	}

	public T GetDataForId(int id)
	{
		return m_TreeData.GetDataForId(id).data;
	}

	public T GetDataForIndex(int index)
	{
		int idForIndex = GetIdForIndex(index);
		return GetDataForId(idForIndex);
	}

	public override object GetItemForIndex(int index)
	{
		return GetDataForIndex(index);
	}

	public override int GetParentId(int id)
	{
		return m_TreeData.GetParentId(id);
	}

	public override bool HasChildren(int id)
	{
		return m_TreeData.GetDataForId(id).hasChildren;
	}

	private static IEnumerable<int> GetItemIds(IEnumerable<TreeViewItemData<T>> items)
	{
		if (items == null)
		{
			yield break;
		}
		foreach (TreeViewItemData<T> item2 in items)
		{
			yield return item2.id;
		}
	}

	public override IEnumerable<int> GetChildrenIds(int id)
	{
		return GetItemIds(m_TreeData.GetDataForId(id).children);
	}

	public override void Move(int id, int newParentId, int childIndex = -1)
	{
		if (id != newParentId && !IsChildOf(newParentId, id))
		{
			m_TreeData.Move(id, newParentId, childIndex);
			RaiseItemIndexChanged(GetIndexForId(id), GetIndexForId(newParentId) + childIndex + 1);
		}
	}

	private bool IsChildOf(int childId, int id)
	{
		if (m_TreeData.GetDataForId(id).HasChildRecursive(childId))
		{
			return true;
		}
		return false;
	}

	public override IEnumerable<int> GetAllItemIds(IEnumerable<int> rootIds = null)
	{
		if (rootIds == null)
		{
			if (m_TreeData.rootItemIds == null)
			{
				yield break;
			}
			rootIds = m_TreeData.rootItemIds;
		}
		IEnumerator<int> currentIterator = rootIds.GetEnumerator();
		while (true)
		{
			if (!currentIterator.MoveNext())
			{
				if (m_IteratorStack.Count > 0)
				{
					currentIterator = m_IteratorStack.Pop();
					continue;
				}
				break;
			}
			int currentItemId = currentIterator.Current;
			yield return currentItemId;
			if (HasChildren(currentItemId))
			{
				m_IteratorStack.Push(currentIterator);
				currentIterator = GetChildrenIds(currentItemId).GetEnumerator();
			}
		}
	}
}
