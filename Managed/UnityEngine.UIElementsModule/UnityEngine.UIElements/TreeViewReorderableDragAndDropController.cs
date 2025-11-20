using UnityEngine.UIElements.Experimental;

namespace UnityEngine.UIElements;

internal class TreeViewReorderableDragAndDropController : BaseReorderableDragAndDropController
{
	protected readonly UnityEngine.UIElements.Experimental.TreeView m_TreeView;

	public TreeViewReorderableDragAndDropController(UnityEngine.UIElements.Experimental.TreeView view)
		: base(view)
	{
		m_TreeView = view;
		base.enableReordering = true;
	}

	public override DragVisualMode HandleDragAndDrop(IListDragAndDropArgs args)
	{
		if (!base.enableReordering)
		{
			return DragVisualMode.Rejected;
		}
		return (args.dragAndDropData.userData == m_TreeView) ? DragVisualMode.Move : DragVisualMode.Rejected;
	}

	public override void OnDrop(IListDragAndDropArgs args)
	{
		int idForIndex = m_TreeView.GetIdForIndex(args.insertAtIndex);
		int parentIdForIndex = m_TreeView.GetParentIdForIndex(args.insertAtIndex);
		int childIndexForId = m_TreeView.viewController.GetChildIndexForId(idForIndex);
		if (args.dragAndDropPosition == DragAndDropPosition.OverItem || (idForIndex == -1 && parentIdForIndex == -1 && childIndexForId == -1))
		{
			for (int i = 0; i < m_SelectedIndices.Count; i++)
			{
				int index = m_SelectedIndices[i];
				int idForIndex2 = m_TreeView.GetIdForIndex(index);
				int newParentId = idForIndex;
				int childIndex = -1;
				m_TreeView.viewController.Move(idForIndex2, newParentId, childIndex);
			}
		}
		else
		{
			for (int num = m_SelectedIndices.Count - 1; num >= 0; num--)
			{
				int index2 = m_SelectedIndices[num];
				int idForIndex3 = m_TreeView.GetIdForIndex(index2);
				int newParentId2 = parentIdForIndex;
				int childIndex2 = childIndexForId;
				m_TreeView.viewController.Move(idForIndex3, newParentId2, childIndex2);
			}
		}
		m_TreeView.viewController.RebuildTree();
		m_TreeView.RefreshItems();
	}
}
