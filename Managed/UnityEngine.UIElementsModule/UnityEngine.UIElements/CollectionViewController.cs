#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Linq;
using UnityEngine.Assertions;

namespace UnityEngine.UIElements;

internal class CollectionViewController
{
	private BaseVerticalCollectionView m_View;

	private IList m_ItemsSource;

	public IList itemsSource
	{
		get
		{
			return m_ItemsSource;
		}
		set
		{
			if (m_ItemsSource != value)
			{
				m_ItemsSource = value;
				RaiseItemsSourceChanged();
			}
		}
	}

	protected BaseVerticalCollectionView view => m_View;

	public event Action itemsSourceChanged;

	public event Action<int, int> itemIndexChanged;

	protected void SetItemsSourceWithoutNotify(IList source)
	{
		m_ItemsSource = source;
	}

	public void SetView(BaseVerticalCollectionView view)
	{
		m_View = view;
		Assert.IsNotNull(m_View, "View must not be null.");
	}

	public virtual int GetItemCount()
	{
		return m_ItemsSource?.Count ?? 0;
	}

	public virtual int GetIndexForId(int id)
	{
		return id;
	}

	public virtual int GetIdForIndex(int index)
	{
		return m_View.getItemId?.Invoke(index) ?? index;
	}

	public virtual object GetItemForIndex(int index)
	{
		if (index < 0 || index >= m_ItemsSource.Count)
		{
			return null;
		}
		return m_ItemsSource[index];
	}

	internal virtual void InvokeMakeItem(ReusableCollectionItem reusableItem)
	{
		reusableItem.Init(MakeItem());
	}

	internal virtual void InvokeBindItem(ReusableCollectionItem reusableItem, int index)
	{
		BindItem(reusableItem.bindableElement, index);
		reusableItem.SetSelected(m_View.selectedIndices.Contains(index));
		reusableItem.rootElement.pseudoStates &= ~PseudoStates.Hover;
	}

	internal virtual void InvokeUnbindItem(ReusableCollectionItem reusableItem, int index)
	{
		UnbindItem(reusableItem.bindableElement, index);
	}

	internal virtual void InvokeDestroyItem(ReusableCollectionItem reusableItem)
	{
		DestroyItem(reusableItem.bindableElement);
	}

	public virtual VisualElement MakeItem()
	{
		if (m_View.makeItem == null)
		{
			if (m_View.bindItem != null)
			{
				throw new NotImplementedException("You must specify makeItem if bindItem is specified.");
			}
			return new Label();
		}
		return m_View.makeItem();
	}

	public virtual void BindItem(VisualElement element, int index)
	{
		if (m_View.bindItem == null)
		{
			if (m_View.makeItem != null)
			{
				throw new NotImplementedException("You must specify bindItem if makeItem is specified.");
			}
			Label label = (Label)element;
			label.text = m_ItemsSource[index]?.ToString() ?? "null";
		}
		else
		{
			m_View.bindItem(element, index);
		}
	}

	public virtual void UnbindItem(VisualElement element, int index)
	{
		m_View.unbindItem?.Invoke(element, index);
	}

	public virtual void DestroyItem(VisualElement element)
	{
		m_View.destroyItem?.Invoke(element);
	}

	protected void RaiseItemsSourceChanged()
	{
		this.itemsSourceChanged?.Invoke();
	}

	protected void RaiseItemIndexChanged(int srcIndex, int dstIndex)
	{
		this.itemIndexChanged?.Invoke(srcIndex, dstIndex);
	}
}
