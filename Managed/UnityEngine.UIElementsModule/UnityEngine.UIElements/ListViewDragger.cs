using System;
using System.Linq;

namespace UnityEngine.UIElements;

internal class ListViewDragger : DragEventsProcessor
{
	internal struct DragPosition : IEquatable<DragPosition>
	{
		public int insertAtIndex;

		public ReusableCollectionItem recycledItem;

		public DragAndDropPosition dragAndDropPosition;

		public bool Equals(DragPosition other)
		{
			return insertAtIndex == other.insertAtIndex && object.Equals(recycledItem, other.recycledItem) && dragAndDropPosition == other.dragAndDropPosition;
		}

		public override bool Equals(object obj)
		{
			return obj is DragPosition && Equals((DragPosition)obj);
		}

		public override int GetHashCode()
		{
			int num = insertAtIndex;
			num = (num * 397) ^ ((recycledItem != null) ? recycledItem.GetHashCode() : 0);
			return (num * 397) ^ (int)dragAndDropPosition;
		}
	}

	private DragPosition m_LastDragPosition;

	private VisualElement m_DragHoverBar;

	private const int k_AutoScrollAreaSize = 5;

	private const int k_BetweenElementsAreaSize = 5;

	private const int k_PanSpeed = 20;

	private const int k_DragHoverBarHeight = 2;

	protected BaseVerticalCollectionView targetListView => m_Target as BaseVerticalCollectionView;

	protected ScrollView targetScrollView => targetListView.scrollView;

	public ICollectionDragAndDropController dragAndDropController { get; set; }

	public ListViewDragger(BaseVerticalCollectionView listView)
		: base(listView)
	{
	}

	protected override bool CanStartDrag(Vector3 pointerPosition)
	{
		if (dragAndDropController == null)
		{
			return false;
		}
		if (!targetScrollView.contentContainer.worldBound.Contains(pointerPosition))
		{
			return false;
		}
		if (targetListView.selectedIndices.Any())
		{
			return dragAndDropController.CanStartDrag(targetListView.selectedIndices);
		}
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		return recycledItem != null && dragAndDropController.CanStartDrag(new int[1] { recycledItem.index });
	}

	protected override StartDragArgs StartDrag(Vector3 pointerPosition)
	{
		if (targetListView.selectedIndices.Any())
		{
			return dragAndDropController.SetupDragAndDrop(targetListView.selectedIndices);
		}
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		if (recycledItem == null)
		{
			return null;
		}
		return dragAndDropController.SetupDragAndDrop(new int[1] { recycledItem.index });
	}

	protected override DragVisualMode UpdateDrag(Vector3 pointerPosition)
	{
		DragPosition dragPosition = default(DragPosition);
		DragVisualMode visualMode = GetVisualMode(pointerPosition, ref dragPosition);
		if (visualMode == DragVisualMode.Rejected)
		{
			ClearDragAndDropUI();
		}
		else
		{
			ApplyDragAndDropUI(dragPosition);
		}
		return visualMode;
	}

	private DragVisualMode GetVisualMode(Vector3 pointerPosition, ref DragPosition dragPosition)
	{
		if (dragAndDropController == null)
		{
			return DragVisualMode.Rejected;
		}
		HandleDragAndScroll(pointerPosition);
		if (!TryGetDragPosition(pointerPosition, ref dragPosition))
		{
			return DragVisualMode.Rejected;
		}
		ListDragAndDropArgs listDragAndDropArgs = MakeDragAndDropArgs(dragPosition);
		return dragAndDropController.HandleDragAndDrop(listDragAndDropArgs);
	}

	protected override void OnDrop(Vector3 pointerPosition)
	{
		DragPosition dragPosition = default(DragPosition);
		if (TryGetDragPosition(pointerPosition, ref dragPosition))
		{
			ListDragAndDropArgs listDragAndDropArgs = MakeDragAndDropArgs(dragPosition);
			if (dragAndDropController.HandleDragAndDrop(listDragAndDropArgs) != DragVisualMode.Rejected)
			{
				dragAndDropController.OnDrop(listDragAndDropArgs);
			}
		}
	}

	internal void HandleDragAndScroll(Vector2 pointerPosition)
	{
		bool flag = pointerPosition.y < targetScrollView.worldBound.yMin + 5f;
		bool flag2 = pointerPosition.y > targetScrollView.worldBound.yMax - 5f;
		if (flag || flag2)
		{
			Vector2 scrollOffset = targetScrollView.scrollOffset + (flag ? Vector2.down : Vector2.up) * 20f;
			scrollOffset.y = Mathf.Clamp(scrollOffset.y, 0f, Mathf.Max(0f, targetScrollView.contentContainer.worldBound.height - targetScrollView.contentViewport.worldBound.height));
			targetScrollView.scrollOffset = scrollOffset;
		}
	}

	protected void ApplyDragAndDropUI(DragPosition dragPosition)
	{
		if (m_LastDragPosition.Equals(dragPosition))
		{
			return;
		}
		if (m_DragHoverBar == null)
		{
			m_DragHoverBar = new VisualElement();
			m_DragHoverBar.AddToClassList(BaseVerticalCollectionView.dragHoverBarUssClassName);
			m_DragHoverBar.style.width = targetListView.localBound.width;
			m_DragHoverBar.style.visibility = Visibility.Hidden;
			m_DragHoverBar.pickingMode = PickingMode.Ignore;
			targetListView.RegisterCallback<GeometryChangedEvent>(delegate
			{
				m_DragHoverBar.style.width = targetListView.localBound.width;
			});
			targetScrollView.contentViewport.Add(m_DragHoverBar);
		}
		ClearDragAndDropUI();
		m_LastDragPosition = dragPosition;
		switch (dragPosition.dragAndDropPosition)
		{
		case DragAndDropPosition.OverItem:
			dragPosition.recycledItem.rootElement.AddToClassList(BaseVerticalCollectionView.itemDragHoverUssClassName);
			break;
		case DragAndDropPosition.BetweenItems:
		{
			if (dragPosition.insertAtIndex == 0)
			{
				PlaceHoverBarAt(0f);
				break;
			}
			ReusableCollectionItem recycledItemFromIndex2 = targetListView.GetRecycledItemFromIndex(dragPosition.insertAtIndex - 1);
			if (recycledItemFromIndex2 == null)
			{
				recycledItemFromIndex2 = targetListView.GetRecycledItemFromIndex(dragPosition.insertAtIndex);
			}
			PlaceHoverBarAtElement(recycledItemFromIndex2.rootElement);
			break;
		}
		case DragAndDropPosition.OutsideItems:
		{
			ReusableCollectionItem recycledItemFromIndex = targetListView.GetRecycledItemFromIndex(targetListView.itemsSource.Count - 1);
			if (recycledItemFromIndex != null)
			{
				PlaceHoverBarAtElement(recycledItemFromIndex.rootElement);
			}
			else if (targetListView.sourceIncludesArraySize && targetListView.itemsSource.Count > 0)
			{
				PlaceHoverBarAtElement(targetListView.GetRecycledItemFromIndex(0).rootElement);
			}
			else
			{
				PlaceHoverBarAt(0f);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("dragAndDropPosition", dragPosition.dragAndDropPosition, "Unsupported dragAndDropPosition value");
		}
	}

	protected virtual bool TryGetDragPosition(Vector2 pointerPosition, ref DragPosition dragPosition)
	{
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		if (recycledItem != null)
		{
			if (targetListView.sourceIncludesArraySize && recycledItem.index == 0)
			{
				dragPosition.insertAtIndex = recycledItem.index + 1;
				dragPosition.dragAndDropPosition = DragAndDropPosition.BetweenItems;
				return true;
			}
			if (recycledItem.rootElement.worldBound.yMax - pointerPosition.y < 5f)
			{
				dragPosition.insertAtIndex = recycledItem.index + 1;
				dragPosition.dragAndDropPosition = DragAndDropPosition.BetweenItems;
				return true;
			}
			if (pointerPosition.y - recycledItem.rootElement.worldBound.yMin > 5f)
			{
				Vector2 scrollOffset = targetScrollView.scrollOffset;
				targetScrollView.ScrollTo(recycledItem.rootElement);
				if (scrollOffset != targetScrollView.scrollOffset)
				{
					return TryGetDragPosition(pointerPosition, ref dragPosition);
				}
				dragPosition.recycledItem = recycledItem;
				dragPosition.insertAtIndex = recycledItem.index;
				dragPosition.dragAndDropPosition = DragAndDropPosition.OverItem;
				return true;
			}
			dragPosition.insertAtIndex = recycledItem.index;
			dragPosition.dragAndDropPosition = DragAndDropPosition.BetweenItems;
			return true;
		}
		if (!targetListView.worldBound.Contains(pointerPosition))
		{
			return false;
		}
		dragPosition.dragAndDropPosition = DragAndDropPosition.OutsideItems;
		if (pointerPosition.y >= targetScrollView.contentContainer.worldBound.yMax)
		{
			dragPosition.insertAtIndex = targetListView.itemsSource.Count;
		}
		else
		{
			dragPosition.insertAtIndex = 0;
		}
		return true;
	}

	private ListDragAndDropArgs MakeDragAndDropArgs(DragPosition dragPosition)
	{
		object target = null;
		ReusableCollectionItem recycledItem = dragPosition.recycledItem;
		if (recycledItem != null)
		{
			target = targetListView.viewController.GetItemForIndex(recycledItem.index);
		}
		return new ListDragAndDropArgs
		{
			target = target,
			insertAtIndex = dragPosition.insertAtIndex,
			dragAndDropPosition = dragPosition.dragAndDropPosition,
			dragAndDropData = (base.useDragEvents ? DragAndDropUtility.dragAndDrop.data : dragAndDropClient.data)
		};
	}

	private void PlaceHoverBarAtElement(VisualElement element)
	{
		VisualElement contentViewport = targetScrollView.contentViewport;
		PlaceHoverBarAt(Mathf.Min(contentViewport.WorldToLocal(element.worldBound).yMax, contentViewport.localBound.yMax - 2f));
	}

	private void PlaceHoverBarAt(float top)
	{
		m_DragHoverBar.style.top = top;
		m_DragHoverBar.style.visibility = Visibility.Visible;
	}

	protected override void ClearDragAndDropUI()
	{
		m_LastDragPosition = default(DragPosition);
		foreach (ReusableCollectionItem activeItem in targetListView.activeItems)
		{
			activeItem.rootElement.RemoveFromClassList(BaseVerticalCollectionView.itemDragHoverUssClassName);
		}
		if (m_DragHoverBar != null)
		{
			m_DragHoverBar.style.visibility = Visibility.Hidden;
		}
	}

	protected ReusableCollectionItem GetRecycledItem(Vector3 pointerPosition)
	{
		foreach (ReusableCollectionItem activeItem in targetListView.activeItems)
		{
			if (activeItem.rootElement.worldBound.Contains(pointerPosition))
			{
				return activeItem;
			}
		}
		return null;
	}
}
