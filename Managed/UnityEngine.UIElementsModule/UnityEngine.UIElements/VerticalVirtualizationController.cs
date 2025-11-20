using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

internal abstract class VerticalVirtualizationController<T> : CollectionVirtualizationController where T : ReusableCollectionItem, new()
{
	protected BaseVerticalCollectionView m_ListView;

	protected const int k_ExtraVisibleItems = 2;

	protected readonly UnityEngine.Pool.ObjectPool<T> m_Pool = new UnityEngine.Pool.ObjectPool<T>(() => new T(), null, delegate(T i)
	{
		i.DetachElement();
	});

	protected List<T> m_ActiveItems;

	protected int m_FirstVisibleIndex;

	private Func<T, bool> m_VisibleItemPredicateDelegate;

	protected List<T> m_ScrollInsertionList = new List<T>();

	private readonly VisualElement k_EmptyRows;

	public override IEnumerable<ReusableCollectionItem> activeItems => m_ActiveItems as IEnumerable<ReusableCollectionItem>;

	internal T firstVisibleItem => m_ActiveItems.FirstOrDefault(m_VisibleItemPredicateDelegate);

	internal T lastVisibleItem => m_ActiveItems.LastOrDefault(m_VisibleItemPredicateDelegate);

	public override int visibleItemCount => m_ActiveItems.Count(m_VisibleItemPredicateDelegate);

	public override int firstVisibleIndex => m_FirstVisibleIndex;

	public override int lastVisibleIndex => lastVisibleItem?.index ?? (-1);

	protected float lastHeight => m_ListView.lastHeight;

	protected virtual bool VisibleItemPredicate(T i)
	{
		bool flag = false;
		if (m_ListView.dragger is ListViewDraggerAnimated listViewDraggerAnimated)
		{
			flag = listViewDraggerAnimated.isDragging && i.index == listViewDraggerAnimated.draggedItem.index;
		}
		return i.rootElement.style.display == DisplayStyle.Flex && !flag;
	}

	protected VerticalVirtualizationController(BaseVerticalCollectionView collectionView)
		: base(collectionView.scrollView)
	{
		m_ListView = collectionView;
		m_ActiveItems = new List<T>();
		m_VisibleItemPredicateDelegate = VisibleItemPredicate;
		k_EmptyRows = new VisualElement();
		k_EmptyRows.AddToClassList(BaseVerticalCollectionView.backgroundFillUssClassName);
	}

	public override void Refresh(bool rebuild)
	{
		bool flag = m_ListView.HasValidDataAndBindings();
		for (int i = 0; i < m_ActiveItems.Count; i++)
		{
			int num = m_FirstVisibleIndex + i;
			T val = m_ActiveItems[i];
			bool flag2 = val.rootElement.style.display == DisplayStyle.Flex;
			if (rebuild)
			{
				if (flag && flag2)
				{
					m_ListView.viewController.InvokeUnbindItem(val, val.index);
					m_ListView.viewController.InvokeDestroyItem(val);
				}
				m_Pool.Release(val);
			}
			else if (num >= 0 && num < m_ListView.itemsSource.Count)
			{
				if (flag && flag2)
				{
					m_ListView.viewController.InvokeUnbindItem(val, val.index);
					val.index = -1;
					Setup(val, num);
				}
			}
			else if (flag2)
			{
				m_Pool.Release(val);
				m_ActiveItems.RemoveAt(i--);
			}
		}
		if (rebuild)
		{
			m_Pool.Clear();
			m_ActiveItems.Clear();
			m_ScrollView.Clear();
		}
	}

	protected void Setup(T recycledItem, int newIndex, bool forceHide = false)
	{
		if (m_ListView.dragger is ListViewDraggerAnimated { isDragging: not false } listViewDraggerAnimated && (listViewDraggerAnimated.draggedItem.index == newIndex || listViewDraggerAnimated.draggedItem == recycledItem))
		{
			return;
		}
		if (newIndex >= m_ListView.itemsSource.Count || forceHide)
		{
			recycledItem.rootElement.style.display = DisplayStyle.None;
			if (recycledItem.index >= 0 && recycledItem.index < m_ListView.itemsSource.Count)
			{
				m_ListView.viewController.InvokeUnbindItem(recycledItem, recycledItem.index);
				recycledItem.index = -1;
			}
			return;
		}
		int idForIndex = m_ListView.viewController.GetIdForIndex(newIndex);
		recycledItem.rootElement.style.display = DisplayStyle.Flex;
		if (recycledItem.index != newIndex)
		{
			bool enable = m_ListView.showAlternatingRowBackgrounds != AlternatingRowBackground.None && newIndex % 2 == 1;
			recycledItem.rootElement.EnableInClassList(BaseVerticalCollectionView.itemAlternativeBackgroundUssClassName, enable);
			if (recycledItem.index != -1)
			{
				m_ListView.viewController.InvokeUnbindItem(recycledItem, recycledItem.index);
			}
			recycledItem.index = newIndex;
			recycledItem.id = idForIndex;
			int num = newIndex - m_FirstVisibleIndex;
			if (num >= m_ScrollView.contentContainer.childCount)
			{
				recycledItem.rootElement.BringToFront();
			}
			else if (num >= 0)
			{
				recycledItem.rootElement.PlaceBehind(m_ScrollView.contentContainer[num]);
			}
			else
			{
				recycledItem.rootElement.SendToBack();
			}
			m_ListView.viewController.InvokeBindItem(recycledItem, newIndex);
			m_ListView.HandleFocus(recycledItem);
		}
	}

	public override void UpdateBackground()
	{
		float num = (float.IsNaN(k_EmptyRows.layout.size.y) ? 0f : k_EmptyRows.layout.size.y);
		float num2 = m_ScrollView.contentViewport.layout.size.y - m_ScrollView.contentContainer.layout.size.y - num;
		if (m_ListView.showAlternatingRowBackgrounds != AlternatingRowBackground.All || num2 <= 0f)
		{
			k_EmptyRows.RemoveFromHierarchy();
		}
		else
		{
			if (lastVisibleItem == null)
			{
				return;
			}
			if (k_EmptyRows.parent == null)
			{
				m_ScrollView.contentViewport.Add(k_EmptyRows);
			}
			float itemHeight = GetItemHeight(-1);
			int num3 = Mathf.FloorToInt(num2 / itemHeight) + 1;
			if (num3 > k_EmptyRows.childCount)
			{
				int num4 = num3 - k_EmptyRows.childCount;
				for (int i = 0; i < num4; i++)
				{
					VisualElement visualElement = new VisualElement();
					visualElement.style.flexShrink = 0f;
					k_EmptyRows.Add(visualElement);
				}
			}
			int num5 = lastVisibleIndex;
			int childCount = k_EmptyRows.hierarchy.childCount;
			for (int j = 0; j < childCount; j++)
			{
				VisualElement visualElement2 = k_EmptyRows.hierarchy[j];
				num5++;
				visualElement2.style.height = itemHeight;
				visualElement2.EnableInClassList(BaseVerticalCollectionView.itemAlternativeBackgroundUssClassName, num5 % 2 == 1);
			}
		}
	}

	public override void ReplaceActiveItem(int index)
	{
		int num = 0;
		foreach (T activeItem in m_ActiveItems)
		{
			if (activeItem.index == index)
			{
				T orMakeItem = GetOrMakeItem();
				activeItem.DetachElement();
				m_ActiveItems.Remove(activeItem);
				m_ListView.viewController.InvokeUnbindItem(activeItem, index);
				m_ListView.viewController.InvokeDestroyItem(activeItem);
				m_ActiveItems.Insert(num, orMakeItem);
				m_ScrollView.Add(orMakeItem.rootElement);
				Setup(orMakeItem, index);
				break;
			}
			num++;
		}
	}

	internal virtual T GetOrMakeItem()
	{
		T val = m_Pool.Get();
		if (val.rootElement == null)
		{
			m_ListView.viewController.InvokeMakeItem(val);
		}
		val.PreAttachElement();
		return val;
	}
}
