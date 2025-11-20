using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class ListViewReorderableDragAndDropController : BaseReorderableDragAndDropController
{
	protected readonly ListView m_ListView;

	public ListViewReorderableDragAndDropController(ListView view)
		: base(view)
	{
		m_ListView = view;
	}

	public override DragVisualMode HandleDragAndDrop(IListDragAndDropArgs args)
	{
		if (args.dragAndDropPosition == DragAndDropPosition.OverItem || !base.enableReordering)
		{
			return DragVisualMode.Rejected;
		}
		return (args.dragAndDropData.userData == m_ListView) ? DragVisualMode.Move : DragVisualMode.Rejected;
	}

	public override void OnDrop(IListDragAndDropArgs args)
	{
		int insertAtIndex = args.insertAtIndex;
		int num = 0;
		int num2 = 0;
		for (int num3 = m_SelectedIndices.Count - 1; num3 >= 0; num3--)
		{
			int num4 = m_SelectedIndices[num3];
			if (num4 >= 0)
			{
				int num5 = insertAtIndex - num;
				if (num4 > insertAtIndex)
				{
					num4 += num2;
					num2++;
				}
				else if (num4 < num5)
				{
					num++;
					num5--;
				}
				m_ListView.viewController.Move(num4, num5);
			}
		}
		if (m_ListView.selectionType != SelectionType.None)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < m_SelectedIndices.Count; i++)
			{
				list.Add(insertAtIndex - num + i);
			}
			m_ListView.SetSelectionWithoutNotify(list);
		}
		else
		{
			m_ListView.ClearSelection();
		}
	}
}
