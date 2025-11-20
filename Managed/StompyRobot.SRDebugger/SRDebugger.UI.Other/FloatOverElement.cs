using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SRDebugger.UI.Other;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class FloatOverElement : UIBehaviour, ILayoutSelfController, ILayoutController
{
	public RectTransform CopyFrom;

	private DrivenRectTransformTracker _tracker;

	private void Copy()
	{
		if (!(CopyFrom == null))
		{
			_tracker.Clear();
			RectTransform component = GetComponent<RectTransform>();
			component.anchorMin = CopyFrom.anchorMin;
			component.anchorMax = CopyFrom.anchorMax;
			component.anchoredPosition = CopyFrom.anchoredPosition;
			component.offsetMin = CopyFrom.offsetMin;
			component.offsetMax = CopyFrom.offsetMax;
			component.sizeDelta = CopyFrom.sizeDelta;
			component.localScale = CopyFrom.localScale;
			component.pivot = CopyFrom.pivot;
			_tracker.Add(this, component, DrivenTransformProperties.All);
		}
	}

	public void SetLayoutHorizontal()
	{
		Copy();
	}

	public void SetLayoutVertical()
	{
		Copy();
	}
}
