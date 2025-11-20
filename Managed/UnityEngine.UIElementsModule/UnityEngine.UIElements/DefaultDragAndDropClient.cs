using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class DefaultDragAndDropClient : IDragAndDrop, IDragAndDropData
{
	private StartDragArgs m_StartDragArgs;

	public object userData => m_StartDragArgs?.userData;

	public IEnumerable<Object> unityObjectReferences => m_StartDragArgs?.unityObjectReferences;

	public IDragAndDropData data => this;

	public void StartDrag(StartDragArgs args)
	{
		m_StartDragArgs = args;
	}

	public void AcceptDrag()
	{
		m_StartDragArgs = null;
	}

	public void SetVisualMode(DragVisualMode visualMode)
	{
	}

	public object GetGenericData(string key)
	{
		if (m_StartDragArgs == null)
		{
			return null;
		}
		return m_StartDragArgs.genericData.ContainsKey(key) ? m_StartDragArgs.genericData[key] : null;
	}
}
