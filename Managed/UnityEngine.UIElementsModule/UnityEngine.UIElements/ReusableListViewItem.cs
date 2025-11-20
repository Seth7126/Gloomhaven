namespace UnityEngine.UIElements;

internal class ReusableListViewItem : ReusableCollectionItem
{
	private VisualElement m_Container;

	private VisualElement m_DragHandle;

	private VisualElement m_ItemContainer;

	public override VisualElement rootElement => m_Container ?? base.bindableElement;

	public void Init(VisualElement item, bool usesAnimatedDragger)
	{
		base.Init(item);
		UpdateHierarchy(usesAnimatedDragger);
	}

	private void UpdateHierarchy(bool usesAnimatedDragger)
	{
		if (usesAnimatedDragger)
		{
			if (m_Container == null)
			{
				m_Container = new VisualElement
				{
					name = ListView.reorderableItemUssClassName
				};
				m_Container.AddToClassList(ListView.reorderableItemUssClassName);
				m_DragHandle = new VisualElement
				{
					name = ListView.reorderableItemHandleUssClassName
				};
				m_DragHandle.AddToClassList(ListView.reorderableItemHandleUssClassName);
				VisualElement visualElement = new VisualElement
				{
					name = ListView.reorderableItemHandleBarUssClassName
				};
				visualElement.AddToClassList(ListView.reorderableItemHandleBarUssClassName);
				m_DragHandle.Add(visualElement);
				VisualElement visualElement2 = new VisualElement
				{
					name = ListView.reorderableItemHandleBarUssClassName
				};
				visualElement2.AddToClassList(ListView.reorderableItemHandleBarUssClassName);
				m_DragHandle.Add(visualElement2);
				m_ItemContainer = new VisualElement
				{
					name = ListView.reorderableItemContainerUssClassName
				};
				m_ItemContainer.AddToClassList(ListView.reorderableItemContainerUssClassName);
				m_ItemContainer.Add(base.bindableElement);
				m_Container.Add(m_DragHandle);
				m_Container.Add(m_ItemContainer);
			}
		}
		else if (m_Container != null)
		{
			m_Container.RemoveFromHierarchy();
			m_Container = null;
		}
	}

	public void UpdateDragHandle(bool needsDragHandle)
	{
		if (needsDragHandle)
		{
			if (m_DragHandle.parent == null)
			{
				rootElement.Insert(0, m_DragHandle);
				rootElement.AddToClassList(ListView.reorderableItemUssClassName);
			}
		}
		else if (m_DragHandle?.parent != null)
		{
			m_DragHandle.RemoveFromHierarchy();
			rootElement.RemoveFromClassList(ListView.reorderableItemUssClassName);
		}
	}

	public override void PreAttachElement()
	{
		base.PreAttachElement();
		rootElement.AddToClassList(ListView.itemUssClassName);
	}

	public override void DetachElement()
	{
		base.DetachElement();
		rootElement.RemoveFromClassList(ListView.itemUssClassName);
	}
}
