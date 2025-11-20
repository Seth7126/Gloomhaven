using UnityEngine.UIElements.Experimental;

namespace UnityEngine.UIElements;

internal class ListViewDraggerAnimated : ListViewDragger
{
	private int m_DragStartIndex;

	private int m_CurrentIndex;

	private float m_SelectionHeight;

	private float m_LocalOffsetOnStart;

	private Vector3 m_CurrentPointerPosition;

	private ReusableCollectionItem m_Item;

	private ReusableCollectionItem m_OffsetItem;

	public bool isDragging => m_Item != null;

	public ReusableCollectionItem draggedItem => m_Item;

	internal override bool supportsDragEvents => false;

	public ListViewDraggerAnimated(BaseVerticalCollectionView listView)
		: base(listView)
	{
	}

	protected override StartDragArgs StartDrag(Vector3 pointerPosition)
	{
		base.targetListView.ClearSelection();
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		if (recycledItem == null)
		{
			return null;
		}
		base.targetListView.SetSelection(recycledItem.index);
		m_Item = recycledItem;
		float y = m_Item.rootElement.layout.y;
		m_SelectionHeight = m_Item.rootElement.layout.height;
		m_Item.rootElement.style.position = Position.Absolute;
		m_Item.rootElement.style.height = m_Item.rootElement.layout.height;
		m_Item.rootElement.style.width = m_Item.rootElement.layout.width;
		m_Item.rootElement.style.top = y;
		m_DragStartIndex = m_Item.index;
		m_CurrentIndex = m_DragStartIndex;
		m_CurrentPointerPosition = pointerPosition;
		m_LocalOffsetOnStart = base.targetScrollView.contentContainer.WorldToLocal(pointerPosition).y - y;
		ReusableCollectionItem recycledItemFromIndex = base.targetListView.GetRecycledItemFromIndex(m_CurrentIndex + 1);
		if (recycledItemFromIndex != null)
		{
			m_OffsetItem = recycledItemFromIndex;
			Animate(m_OffsetItem, m_SelectionHeight);
			m_OffsetItem.rootElement.style.paddingTop = m_SelectionHeight;
			if (base.targetListView.virtualizationMethod == CollectionVirtualizationMethod.FixedHeight)
			{
				m_OffsetItem.rootElement.style.height = base.targetListView.fixedItemHeight + m_SelectionHeight;
			}
		}
		return base.dragAndDropController.SetupDragAndDrop(new int[1] { m_Item.index }, skipText: true);
	}

	protected override DragVisualMode UpdateDrag(Vector3 pointerPosition)
	{
		if (m_Item == null)
		{
			return DragVisualMode.Rejected;
		}
		HandleDragAndScroll(pointerPosition);
		m_CurrentPointerPosition = pointerPosition;
		Vector2 vector = base.targetScrollView.contentContainer.WorldToLocal(m_CurrentPointerPosition);
		Rect layout = m_Item.rootElement.layout;
		float height = base.targetScrollView.contentContainer.layout.height;
		layout.y = Mathf.Clamp(vector.y - m_LocalOffsetOnStart, 0f, height - m_SelectionHeight);
		float num = base.targetScrollView.contentContainer.resolvedStyle.paddingTop;
		m_CurrentIndex = -1;
		foreach (ReusableCollectionItem activeItem in base.targetListView.activeItems)
		{
			if (activeItem.rootElement.style.display == DisplayStyle.None)
			{
				continue;
			}
			ReusableCollectionItem reusableCollectionItem = activeItem;
			if (reusableCollectionItem == m_Item)
			{
				continue;
			}
			float num2 = reusableCollectionItem.rootElement.layout.height - reusableCollectionItem.rootElement.resolvedStyle.paddingTop;
			if ((!base.targetListView.sourceIncludesArraySize || reusableCollectionItem.index != 0) && m_CurrentIndex == -1 && layout.y <= num + num2 * 0.5f)
			{
				m_CurrentIndex = reusableCollectionItem.index;
				if (m_OffsetItem != reusableCollectionItem)
				{
					Animate(m_OffsetItem, 0f);
					Animate(reusableCollectionItem, m_SelectionHeight);
					m_OffsetItem = reusableCollectionItem;
				}
				break;
			}
			num += num2;
		}
		if (m_CurrentIndex == -1)
		{
			m_CurrentIndex = base.targetListView.itemsSource.Count;
			Animate(m_OffsetItem, 0f);
			m_OffsetItem = null;
		}
		m_Item.rootElement.layout = layout;
		m_Item.rootElement.BringToFront();
		return DragVisualMode.Move;
	}

	private void Animate(ReusableCollectionItem element, float paddingTop)
	{
		if (element != null && (element.animator == null || ((!element.animator.isRunning || element.animator.to.paddingTop != paddingTop) && (element.animator.isRunning || !(element.rootElement.style.paddingTop == paddingTop)))))
		{
			element.animator?.Stop();
			element.animator?.Recycle();
			StyleValues to = ((base.targetListView.virtualizationMethod == CollectionVirtualizationMethod.FixedHeight) ? new StyleValues
			{
				paddingTop = paddingTop,
				height = base.targetListView.ResolveItemHeight() + paddingTop
			} : new StyleValues
			{
				paddingTop = paddingTop
			});
			element.animator = element.rootElement.experimental.animation.Start(to, 500);
			element.animator.KeepAlive();
		}
	}

	protected override void OnDrop(Vector3 pointerPosition)
	{
		if (m_Item != null && base.targetListView.binding == null)
		{
			base.targetListView.virtualizationController.ReplaceActiveItem(m_Item.index);
		}
		if (m_OffsetItem != null)
		{
			m_OffsetItem.animator?.Stop();
			m_OffsetItem.animator?.Recycle();
			m_OffsetItem.animator = null;
			m_OffsetItem.rootElement.style.paddingTop = 0f;
			if (base.targetListView.virtualizationMethod == CollectionVirtualizationMethod.FixedHeight)
			{
				m_OffsetItem.rootElement.style.height = base.targetListView.ResolveItemHeight();
			}
		}
		base.OnDrop(pointerPosition);
		if (m_Item != null && base.targetListView.binding != null)
		{
			base.targetListView.virtualizationController.ReplaceActiveItem(m_Item.index);
		}
		m_OffsetItem = null;
		m_Item = null;
	}

	protected override void ClearDragAndDropUI()
	{
	}

	protected override bool TryGetDragPosition(Vector2 pointerPosition, ref DragPosition dragPosition)
	{
		dragPosition.recycledItem = m_Item;
		dragPosition.insertAtIndex = m_CurrentIndex;
		dragPosition.dragAndDropPosition = DragAndDropPosition.BetweenItems;
		return true;
	}
}
