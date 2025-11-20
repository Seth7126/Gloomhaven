using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

internal class ListViewController : CollectionViewController
{
	private ListView listView => base.view as ListView;

	public event Action itemsSourceSizeChanged;

	public event Action<IEnumerable<int>> itemsAdded;

	public event Action<IEnumerable<int>> itemsRemoved;

	internal override void InvokeMakeItem(ReusableCollectionItem reusableItem)
	{
		if (reusableItem is ReusableListViewItem reusableListViewItem)
		{
			reusableListViewItem.Init(MakeItem(), listView.reorderable && listView.reorderMode == ListViewReorderMode.Animated);
			reusableListViewItem.bindableElement.style.position = Position.Relative;
			reusableListViewItem.bindableElement.style.flexBasis = StyleKeyword.Initial;
			reusableListViewItem.bindableElement.style.marginTop = 0f;
			reusableListViewItem.bindableElement.style.marginBottom = 0f;
			reusableListViewItem.bindableElement.style.flexGrow = 0f;
			reusableListViewItem.bindableElement.style.flexShrink = 0f;
		}
	}

	internal override void InvokeBindItem(ReusableCollectionItem reusableItem, int index)
	{
		if (reusableItem is ReusableListViewItem reusableListViewItem)
		{
			bool flag = listView.reorderable && listView.reorderMode == ListViewReorderMode.Animated;
			reusableListViewItem.UpdateDragHandle(flag && NeedsDragHandle(index));
		}
		base.InvokeBindItem(reusableItem, index);
	}

	public virtual bool NeedsDragHandle(int index)
	{
		return !listView.sourceIncludesArraySize || index != 0;
	}

	public virtual void AddItems(int itemCount)
	{
		EnsureItemSourceCanBeResized();
		int count = base.itemsSource.Count;
		List<int> list = CollectionPool<List<int>, int>.Get();
		try
		{
			if (base.itemsSource.IsFixedSize)
			{
				base.itemsSource = AddToArray((Array)base.itemsSource, itemCount);
				for (int i = 0; i < itemCount; i++)
				{
					list.Add(count + i);
				}
			}
			else
			{
				for (int j = 0; j < itemCount; j++)
				{
					list.Add(count + j);
					base.itemsSource.Add(null);
				}
			}
			RaiseItemsAdded(list);
		}
		finally
		{
			CollectionPool<List<int>, int>.Release(list);
		}
		RaiseOnSizeChanged();
		if (base.itemsSource.IsFixedSize)
		{
			listView.Rebuild();
		}
	}

	public virtual void Move(int index, int newIndex)
	{
		int dstIndex = newIndex;
		int num = ((newIndex < index) ? 1 : (-1));
		while (Mathf.Min(index, newIndex) < Mathf.Max(index, newIndex))
		{
			Swap(index, newIndex);
			newIndex += num;
		}
		RaiseItemIndexChanged(index, dstIndex);
	}

	public virtual void RemoveItem(int index)
	{
		List<int> list = CollectionPool<List<int>, int>.Get();
		try
		{
			list.Add(index);
			RemoveItems(list);
		}
		finally
		{
			CollectionPool<List<int>, int>.Release(list);
		}
	}

	public virtual void RemoveItems(List<int> indices)
	{
		EnsureItemSourceCanBeResized();
		indices.Sort();
		if (base.itemsSource.IsFixedSize)
		{
			base.itemsSource = RemoveFromArray((Array)base.itemsSource, indices);
		}
		else
		{
			for (int num = indices.Count - 1; num >= 0; num--)
			{
				base.itemsSource.RemoveAt(indices[num]);
			}
		}
		RaiseItemsRemoved(indices);
		RaiseOnSizeChanged();
	}

	protected void RaiseOnSizeChanged()
	{
		this.itemsSourceSizeChanged?.Invoke();
	}

	protected void RaiseItemsAdded(IEnumerable<int> indices)
	{
		this.itemsAdded?.Invoke(indices);
	}

	protected void RaiseItemsRemoved(IEnumerable<int> indices)
	{
		this.itemsRemoved?.Invoke(indices);
	}

	private static Array AddToArray(Array source, int itemCount)
	{
		Type elementType = source.GetType().GetElementType();
		if ((object)elementType == null)
		{
			throw new InvalidOperationException("Cannot resize source, because its size is fixed.");
		}
		Array array = Array.CreateInstance(elementType, source.Length + itemCount);
		Array.Copy(source, array, source.Length);
		return array;
	}

	private static Array RemoveFromArray(Array source, List<int> indicesToRemove)
	{
		int length = source.Length;
		int num = length - indicesToRemove.Count;
		if (num < 0)
		{
			throw new InvalidOperationException("Cannot remove more items than the current count from source.");
		}
		Type elementType = source.GetType().GetElementType();
		if (num == 0)
		{
			return Array.CreateInstance(elementType, 0);
		}
		Array array = Array.CreateInstance(elementType, num);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < source.Length; i++)
		{
			if (num3 < indicesToRemove.Count && indicesToRemove[num3] == i)
			{
				num3++;
				continue;
			}
			array.SetValue(source.GetValue(i), num2);
			num2++;
		}
		return array;
	}

	private void Swap(int lhs, int rhs)
	{
		object value = base.itemsSource[lhs];
		base.itemsSource[lhs] = base.itemsSource[rhs];
		base.itemsSource[rhs] = value;
	}

	private void EnsureItemSourceCanBeResized()
	{
		if (base.itemsSource.IsFixedSize && !base.itemsSource.GetType().IsArray)
		{
			throw new InvalidOperationException("Cannot add or remove items from source, because its size is fixed.");
		}
	}
}
