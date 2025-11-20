using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class FixedHeightVirtualizationController<T> : VerticalVirtualizationController<T> where T : ReusableCollectionItem, new()
{
	private float resolvedItemHeight => m_ListView.ResolveItemHeight();

	protected override bool VisibleItemPredicate(T i)
	{
		return true;
	}

	public FixedHeightVirtualizationController(BaseVerticalCollectionView collectionView)
		: base(collectionView)
	{
	}

	public override int GetIndexFromPosition(Vector2 position)
	{
		return (int)(position.y / resolvedItemHeight);
	}

	public override float GetItemHeight(int index)
	{
		return resolvedItemHeight;
	}

	public override void ScrollToItem(int index)
	{
		if (visibleItemCount == 0 || index < -1)
		{
			return;
		}
		float num = resolvedItemHeight;
		if (index == -1)
		{
			int num2 = (int)(base.lastHeight / num);
			if (m_ListView.itemsSource.Count < num2)
			{
				m_ScrollView.scrollOffset = new Vector2(0f, 0f);
			}
			else
			{
				m_ScrollView.scrollOffset = new Vector2(0f, (float)(m_ListView.itemsSource.Count + 1) * num);
			}
			return;
		}
		if (m_FirstVisibleIndex >= index)
		{
			m_ScrollView.scrollOffset = Vector2.up * (num * (float)index);
			return;
		}
		int num3 = (int)(base.lastHeight / num);
		if (index >= m_FirstVisibleIndex + num3)
		{
			int num4 = index - num3 + 1;
			float num5 = num - (base.lastHeight - (float)num3 * num);
			float y = num * (float)num4 + num5;
			m_ScrollView.scrollOffset = new Vector2(m_ScrollView.scrollOffset.x, y);
		}
	}

	public override void Resize(Vector2 size, int layoutPass)
	{
		float num = resolvedItemHeight;
		float num2 = (float)m_ListView.itemsSource.Count * num;
		m_ScrollView.contentContainer.style.height = num2;
		float num3 = Mathf.Max(0f, num2 - m_ScrollView.contentViewport.layout.height);
		float num4 = Mathf.Min(m_ListView.m_ScrollOffset.y, num3);
		m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(num3);
		m_ScrollView.verticalScroller.slider.SetValueWithoutNotify(num4);
		int num5 = (int)(size.y / num);
		if (num5 > 0)
		{
			num5 += 2;
		}
		int num6 = Mathf.Min(num5, m_ListView.itemsSource.Count);
		if (visibleItemCount != num6)
		{
			int num7 = visibleItemCount;
			if (visibleItemCount > num6)
			{
				int num8 = num7 - num6;
				for (int i = 0; i < num8; i++)
				{
					int index = m_ActiveItems.Count - 1;
					T val = m_ActiveItems[index];
					m_ListView.viewController.InvokeUnbindItem(val, val.index);
					m_Pool.Release(val);
					m_ActiveItems.RemoveAt(index);
				}
			}
			else
			{
				int num9 = num6 - visibleItemCount;
				for (int j = 0; j < num9; j++)
				{
					int newIndex = j + m_FirstVisibleIndex + num7;
					T orMakeItem = GetOrMakeItem();
					m_ActiveItems.Add(orMakeItem);
					m_ScrollView.Add(orMakeItem.rootElement);
					Setup(orMakeItem, newIndex);
				}
			}
		}
		OnScroll(new Vector2(0f, num4));
	}

	public override void OnScroll(Vector2 scrollOffset)
	{
		float y = scrollOffset.y;
		float num = resolvedItemHeight;
		int num2 = (int)(y / num);
		m_ScrollView.contentContainer.style.paddingTop = (float)num2 * num;
		m_ScrollView.contentContainer.style.height = (float)m_ListView.itemsSource.Count * num;
		m_ListView.m_ScrollOffset.y = scrollOffset.y;
		if (num2 == m_FirstVisibleIndex)
		{
			return;
		}
		m_FirstVisibleIndex = num2;
		if (m_ActiveItems.Count <= 0)
		{
			return;
		}
		if (m_FirstVisibleIndex < m_ActiveItems[0].index)
		{
			int num3 = m_ActiveItems[0].index - m_FirstVisibleIndex;
			List<T> scrollInsertionList = m_ScrollInsertionList;
			for (int i = 0; i < num3; i++)
			{
				if (m_ActiveItems.Count <= 0)
				{
					break;
				}
				T val = m_ActiveItems[m_ActiveItems.Count - 1];
				scrollInsertionList.Add(val);
				m_ActiveItems.RemoveAt(m_ActiveItems.Count - 1);
				val.rootElement.SendToBack();
			}
			m_ActiveItems.InsertRange(0, scrollInsertionList);
			m_ScrollInsertionList.Clear();
		}
		else if (m_FirstVisibleIndex < m_ActiveItems[m_ActiveItems.Count - 1].index)
		{
			List<T> scrollInsertionList2 = m_ScrollInsertionList;
			int num4 = 0;
			while (m_FirstVisibleIndex > m_ActiveItems[num4].index)
			{
				T val2 = m_ActiveItems[num4];
				scrollInsertionList2.Add(val2);
				num4++;
				val2.rootElement.BringToFront();
			}
			m_ActiveItems.RemoveRange(0, num4);
			m_ActiveItems.AddRange(scrollInsertionList2);
			scrollInsertionList2.Clear();
		}
		for (int j = 0; j < m_ActiveItems.Count; j++)
		{
			int newIndex = j + m_FirstVisibleIndex;
			Setup(m_ActiveItems[j], newIndex);
		}
	}

	internal override T GetOrMakeItem()
	{
		T orMakeItem = base.GetOrMakeItem();
		orMakeItem.rootElement.style.height = resolvedItemHeight;
		return orMakeItem;
	}
}
