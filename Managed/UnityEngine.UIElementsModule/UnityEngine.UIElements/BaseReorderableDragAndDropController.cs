using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal abstract class BaseReorderableDragAndDropController : ICollectionDragAndDropController, IDragAndDropController<IListDragAndDropArgs>, IReorderable
{
	protected readonly BaseVerticalCollectionView m_View;

	protected List<int> m_SelectedIndices;

	public bool enableReordering { get; set; }

	public BaseReorderableDragAndDropController(BaseVerticalCollectionView view)
	{
		m_View = view;
		enableReordering = true;
	}

	public virtual bool CanStartDrag(IEnumerable<int> itemIndices)
	{
		return enableReordering;
	}

	public virtual StartDragArgs SetupDragAndDrop(IEnumerable<int> itemIndices, bool skipText = false)
	{
		if (m_SelectedIndices == null)
		{
			m_SelectedIndices = new List<int>();
		}
		m_SelectedIndices.Clear();
		string text = string.Empty;
		if (itemIndices != null)
		{
			foreach (int itemIndex in itemIndices)
			{
				m_SelectedIndices.Add(itemIndex);
				if (!skipText)
				{
					if (string.IsNullOrEmpty(text))
					{
						Label label = m_View.GetRecycledItemFromIndex(itemIndex)?.rootElement.Q<Label>();
						text = ((label != null) ? label.text : $"Item {itemIndex}");
					}
					else
					{
						text = "<Multiple>";
						skipText = true;
					}
				}
			}
		}
		m_SelectedIndices.Sort();
		return new StartDragArgs(text, m_View);
	}

	public abstract DragVisualMode HandleDragAndDrop(IListDragAndDropArgs args);

	public abstract void OnDrop(IListDragAndDropArgs args);
}
