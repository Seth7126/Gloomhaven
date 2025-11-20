using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class DynamicHeightVirtualizationController<T> : VerticalVirtualizationController<T> where T : ReusableCollectionItem, new()
{
	internal static readonly int InitialAverageHeight = 20;

	private Dictionary<int, float> m_ItemHeightCache = new Dictionary<int, float>();

	private HashSet<int> m_WaitingCache = new HashSet<int>();

	private int m_ForcedFirstVisibleItem = -1;

	private int m_ForcedLastVisibleItem = -1;

	private bool m_StickToBottom;

	private float m_AverageHeight = InitialAverageHeight;

	private float m_AccumulatedHeight;

	private float m_StoredPadding;

	private Action m_FillCallback;

	private Action<ReusableCollectionItem> m_GeometryChangedCallback;

	private IVisualElementScheduledItem m_ScheduledItem;

	private Predicate<int> m_IndexOutOfBoundsPredicate;

	public DynamicHeightVirtualizationController(BaseVerticalCollectionView collectionView)
		: base(collectionView)
	{
		m_FillCallback = Fill;
		m_GeometryChangedCallback = OnRecycledItemGeometryChanged;
		m_IndexOutOfBoundsPredicate = IsIndexOutOfBounds;
		collectionView.destroyItem = (Action<VisualElement>)Delegate.Combine(collectionView.destroyItem, (Action<VisualElement>)delegate(VisualElement element)
		{
			foreach (ReusableCollectionItem activeItem in m_ListView.activeItems)
			{
				if (activeItem.rootElement == element)
				{
					UnregisterItemHeight(activeItem.index, element.layout.height);
					break;
				}
			}
		});
	}

	public override void Refresh(bool rebuild)
	{
		base.Refresh(rebuild);
		m_WaitingCache.RemoveWhere(m_IndexOutOfBoundsPredicate);
		if (m_ListView.HasValidDataAndBindings())
		{
			if (m_WaitingCache.Count == 0)
			{
				ApplyScrollViewUpdate();
			}
			if (m_ScheduledItem == null)
			{
				m_ScheduledItem = m_ListView.schedule.Execute(m_FillCallback);
			}
		}
	}

	public override void ScrollToItem(int index)
	{
		if (visibleItemCount == 0 || index < -1)
		{
			return;
		}
		float height = m_ScrollView.contentContainer.layout.height;
		float height2 = m_ScrollView.contentViewport.layout.height;
		if (index == -1)
		{
			m_ForcedFirstVisibleItem = m_ListView.viewController.itemsSource.Count - 1;
			m_ScrollView.scrollOffset = new Vector2(0f, (height2 >= height) ? 0f : height);
			return;
		}
		if (m_FirstVisibleIndex >= index)
		{
			m_ForcedFirstVisibleItem = index;
			m_ScrollView.scrollOffset = new Vector2(0f, GetContentHeightForIndex(index - 1));
			return;
		}
		float contentHeightForIndex = GetContentHeightForIndex(index);
		if (!(contentHeightForIndex < m_StoredPadding + height2))
		{
			m_ForcedLastVisibleItem = index;
			float y = contentHeightForIndex - height2 + (float)InitialAverageHeight;
			m_ScrollView.scrollOffset = new Vector2(0f, y);
		}
	}

	public override void Resize(Vector2 size, int layoutPass)
	{
		float contentHeight = GetContentHeight();
		m_ScrollView.contentContainer.style.height = contentHeight;
		float contentHeightForIndex = GetContentHeightForIndex(m_FirstVisibleIndex - 1);
		float y = m_ListView.m_ScrollOffset.y;
		float storedPadding = m_StoredPadding;
		float num = y - storedPadding;
		float a = contentHeightForIndex + num;
		float num2 = Mathf.Max(0f, contentHeight - m_ScrollView.contentViewport.layout.height);
		float num3 = Mathf.Min(a, num2);
		m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(num2);
		m_ScrollView.verticalScroller.value = num3;
		m_ListView.m_ScrollOffset.y = m_ScrollView.verticalScroller.value;
		m_ScrollView.contentContainer.style.paddingTop = contentHeightForIndex;
		m_StoredPadding = contentHeightForIndex;
		if (layoutPass == 0)
		{
			Fill();
			OnScroll(new Vector2(0f, num3));
		}
		else if (m_ScheduledItem == null)
		{
			m_ScheduledItem = m_ListView.schedule.Execute(m_FillCallback);
		}
	}

	public override void OnScroll(Vector2 scrollOffset)
	{
		if (float.IsNaN(base.lastHeight))
		{
			return;
		}
		float y = scrollOffset.y;
		float contentHeight = GetContentHeight();
		float b = Mathf.Max(0f, contentHeight - m_ScrollView.contentViewport.layout.height);
		m_ListView.m_ScrollOffset.y = Mathf.Min(y, b);
		int num = ((m_ForcedFirstVisibleItem != -1) ? m_ForcedFirstVisibleItem : GetFirstVisibleItem(m_ListView.m_ScrollOffset.y));
		float contentHeightForIndex = GetContentHeightForIndex(num - 1);
		m_ForcedFirstVisibleItem = -1;
		if (num != m_FirstVisibleIndex)
		{
			m_FirstVisibleIndex = num;
			if (m_ActiveItems.Count > 0)
			{
				T val = base.firstVisibleItem;
				if (!m_StickToBottom)
				{
					if (m_FirstVisibleIndex < val.index)
					{
						int num2 = val.index - m_FirstVisibleIndex;
						List<T> scrollInsertionList = m_ScrollInsertionList;
						for (int i = 0; i < num2; i++)
						{
							if (m_ActiveItems.Count <= 0)
							{
								break;
							}
							T val2 = m_ActiveItems[m_ActiveItems.Count - 1];
							if (val2.rootElement.layout.y < m_ListView.m_ScrollOffset.y + m_ScrollView.contentViewport.layout.height)
							{
								break;
							}
							scrollInsertionList.Add(val2);
							m_ActiveItems.RemoveAt(m_ActiveItems.Count - 1);
							val2.rootElement.SendToBack();
						}
						m_ActiveItems.InsertRange(0, scrollInsertionList);
						m_ScrollInsertionList.Clear();
					}
					else
					{
						T val3 = base.lastVisibleItem;
						if (m_FirstVisibleIndex < val3.index && val3.index < m_ListView.itemsSource.Count - 1)
						{
							List<T> scrollInsertionList2 = m_ScrollInsertionList;
							int num3 = 0;
							while (m_FirstVisibleIndex > m_ActiveItems[num3].index)
							{
								T val4 = m_ActiveItems[num3];
								scrollInsertionList2.Add(val4);
								num3++;
								val4.rootElement.BringToFront();
							}
							m_ActiveItems.RemoveRange(0, num3);
							m_ActiveItems.AddRange(scrollInsertionList2);
							m_ScrollInsertionList.Clear();
						}
					}
				}
				float num4 = contentHeightForIndex;
				for (int j = 0; j < m_ActiveItems.Count; j++)
				{
					int num5 = m_FirstVisibleIndex + j;
					if (num5 >= m_ListView.itemsSource.Count)
					{
						m_StickToBottom = true;
						m_ForcedLastVisibleItem = -1;
						ReleaseItem(j--);
						continue;
					}
					bool flag = num4 - m_ListView.m_ScrollOffset.y < m_ScrollView.contentViewport.layout.height;
					int index = m_ActiveItems[j].index;
					m_WaitingCache.Remove(index);
					Setup(m_ActiveItems[j], num5, !flag);
					if (flag)
					{
						if (num5 != index)
						{
							m_WaitingCache.Add(num5);
						}
					}
					else
					{
						ReleaseItem(j--);
					}
					num4 += GetItemHeight(num5);
				}
			}
		}
		m_StoredPadding = contentHeightForIndex;
		m_ScrollView.contentContainer.style.paddingTop = contentHeightForIndex;
		if (m_ScheduledItem == null)
		{
			m_ScheduledItem = m_ListView.schedule.Execute(m_FillCallback);
		}
	}

	private bool NeedsFill()
	{
		int num = base.lastVisibleItem?.index ?? (-1);
		float num2 = m_StoredPadding;
		if (num2 > m_ListView.m_ScrollOffset.y)
		{
			return true;
		}
		for (int i = m_FirstVisibleIndex; i < m_ListView.itemsSource.Count; i++)
		{
			if (num2 - m_ListView.m_ScrollOffset.y > m_ScrollView.contentViewport.layout.height)
			{
				break;
			}
			num2 += GetItemHeight(i);
			if (i > num)
			{
				return true;
			}
		}
		return false;
	}

	private void Fill()
	{
		if (!m_ListView.HasValidDataAndBindings())
		{
			return;
		}
		int num = 0;
		int num2 = base.lastVisibleItem?.index ?? (-1);
		float num3 = m_StoredPadding;
		float num4 = num3;
		if (m_ListView.dragger is ListViewDraggerAnimated { draggedItem: not null } listViewDraggerAnimated)
		{
			num4 -= listViewDraggerAnimated.draggedItem.rootElement.style.height.value.value;
		}
		for (int i = m_FirstVisibleIndex; i < m_ListView.itemsSource.Count; i++)
		{
			if (num4 - m_ListView.m_ScrollOffset.y > m_ScrollView.contentViewport.layout.height)
			{
				break;
			}
			num4 += GetItemHeight(i);
			if (i > num2)
			{
				num++;
			}
		}
		int num5 = visibleItemCount;
		for (int j = 0; j < num; j++)
		{
			int num6 = j + m_FirstVisibleIndex + num5;
			T orMakeItem = GetOrMakeItem();
			m_ActiveItems.Add(orMakeItem);
			m_ScrollView.Add(orMakeItem.rootElement);
			m_WaitingCache.Add(num6);
			Setup(orMakeItem, num6);
		}
		while (num3 > m_ListView.m_ScrollOffset.y)
		{
			int num7 = m_FirstVisibleIndex - 1;
			if (num7 < 0)
			{
				break;
			}
			T orMakeItem2 = GetOrMakeItem();
			m_ActiveItems.Insert(0, orMakeItem2);
			m_ScrollView.Insert(0, orMakeItem2.rootElement);
			m_WaitingCache.Add(num7);
			Setup(orMakeItem2, num7);
			num3 -= GetItemHeight(num7);
			m_FirstVisibleIndex = num7;
		}
		m_ScrollView.contentContainer.style.paddingTop = num3;
		m_StoredPadding = num3;
		m_ScheduledItem = null;
	}

	public override int GetIndexFromPosition(Vector2 position)
	{
		int num = 0;
		for (float num2 = 0f; num2 < position.y; num2 += GetItemHeight(num++))
		{
		}
		return num - 1;
	}

	public override float GetItemHeight(int index)
	{
		float value;
		return m_ItemHeightCache.TryGetValue(index, out value) ? value : m_AverageHeight;
	}

	private int GetFirstVisibleItem(float offset)
	{
		if (offset <= 0f)
		{
			return 0;
		}
		int num = -1;
		while (offset > 0f)
		{
			num++;
			float itemHeight = GetItemHeight(num);
			offset -= itemHeight;
		}
		return num;
	}

	private void UpdateScrollViewContainer(int index, float previousHeight, float newHeight)
	{
		float y = m_ListView.m_ScrollOffset.y;
		float storedPadding = m_StoredPadding;
		m_StoredPadding = GetContentHeightForIndex(m_FirstVisibleIndex - 1);
		if (m_StickToBottom)
		{
			return;
		}
		if (m_ForcedLastVisibleItem >= 0)
		{
			float contentHeightForIndex = GetContentHeightForIndex(m_ForcedLastVisibleItem);
			m_ListView.m_ScrollOffset.y = contentHeightForIndex + (float)InitialAverageHeight - m_ScrollView.contentViewport.layout.height;
			return;
		}
		float num = y - storedPadding;
		if (index == m_FirstVisibleIndex && num != 0f)
		{
			num += newHeight - previousHeight;
		}
		m_ListView.m_ScrollOffset.y = m_StoredPadding + num;
	}

	private void ApplyScrollViewUpdate()
	{
		float contentHeight = GetContentHeight();
		m_StoredPadding = GetContentHeightForIndex(m_FirstVisibleIndex - 1);
		m_ScrollView.contentContainer.style.paddingTop = m_StoredPadding;
		m_ScrollView.contentContainer.style.height = contentHeight;
		float num = Mathf.Max(0f, contentHeight - m_ScrollView.contentViewport.layout.height);
		if (m_StickToBottom)
		{
			m_ListView.m_ScrollOffset.y = num;
		}
		m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(num);
		m_ScrollView.verticalScroller.slider.SetValueWithoutNotify(m_ListView.m_ScrollOffset.y);
		m_ListView.m_ScrollOffset.y = m_ScrollView.verticalScroller.slider.value;
		if (!NeedsFill())
		{
			m_StickToBottom = false;
			m_ForcedLastVisibleItem = -1;
			m_ScheduledItem?.Pause();
			m_ScheduledItem = null;
			float num2 = m_StoredPadding;
			for (int i = 0; i < m_ActiveItems.Count; i++)
			{
				int index = m_FirstVisibleIndex + i;
				if (!(num2 - m_ListView.m_ScrollOffset.y < m_ScrollView.contentViewport.layout.height))
				{
					m_ListView.viewController.InvokeUnbindItem(m_ActiveItems[i], index);
					ReleaseItem(i--);
				}
				num2 += GetItemHeight(index);
			}
		}
		else if (m_ScheduledItem == null)
		{
			m_ScheduledItem = m_ListView.schedule.Execute(m_FillCallback);
		}
	}

	private float GetContentHeight()
	{
		int count = m_ListView.viewController.itemsSource.Count;
		return GetContentHeightForIndex(count - 1);
	}

	private float GetContentHeightForIndex(int lastIndex)
	{
		if (lastIndex < 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i <= lastIndex; i++)
		{
			num += GetItemHeight(i);
		}
		return num;
	}

	private void RegisterItemHeight(int index, float height)
	{
		if (!(height <= 0f))
		{
			float num = m_ListView.ResolveItemHeight(height);
			if (m_ItemHeightCache.TryGetValue(index, out var value))
			{
				m_AccumulatedHeight -= value;
			}
			m_AccumulatedHeight += num;
			int count = m_ItemHeightCache.Count;
			m_AverageHeight = m_ListView.ResolveItemHeight((count > 0) ? (m_AccumulatedHeight / (float)count) : m_AccumulatedHeight);
			m_ItemHeightCache[index] = num;
		}
	}

	private void UnregisterItemHeight(int index, float height)
	{
		if (!(height <= 0f) && m_ItemHeightCache.TryGetValue(index, out var value))
		{
			m_AccumulatedHeight -= value;
			m_ItemHeightCache.Remove(index);
			int count = m_ItemHeightCache.Count;
			m_AverageHeight = m_ListView.ResolveItemHeight((count > 0) ? (m_AccumulatedHeight / (float)count) : m_AccumulatedHeight);
		}
	}

	private void OnRecycledItemGeometryChanged(ReusableCollectionItem item)
	{
		if (item.index == -1 || item.rootElement.layout.height == 0f || (item.animator != null && item.animator.isRunning))
		{
			return;
		}
		float itemHeight = GetItemHeight(item.index);
		if (!m_ItemHeightCache.TryGetValue(item.index, out var value) || !item.rootElement.layout.height.Equals(value))
		{
			RegisterItemHeight(item.index, item.rootElement.layout.height);
			UpdateScrollViewContainer(item.index, itemHeight, item.rootElement.layout.height);
			if (m_WaitingCache.Count == 0)
			{
				ApplyScrollViewUpdate();
			}
		}
		else
		{
			UpdateScrollViewContainer(item.index, itemHeight, itemHeight);
		}
		if (m_WaitingCache.Remove(item.index) && m_WaitingCache.Count == 0)
		{
			ApplyScrollViewUpdate();
		}
	}

	internal override T GetOrMakeItem()
	{
		T orMakeItem = base.GetOrMakeItem();
		orMakeItem.onGeometryChanged += m_GeometryChangedCallback;
		return orMakeItem;
	}

	public override void ReplaceActiveItem(int index)
	{
		base.ReplaceActiveItem(index);
		m_WaitingCache.Remove(index);
	}

	private void ReleaseItem(int index)
	{
		T val = m_ActiveItems[index];
		val.onGeometryChanged -= m_GeometryChangedCallback;
		m_Pool.Release(val);
		m_ActiveItems.RemoveAt(index);
		m_WaitingCache.Remove(index);
	}

	private bool IsIndexOutOfBounds(int i)
	{
		return i >= m_ListView.itemsSource.Count;
	}
}
