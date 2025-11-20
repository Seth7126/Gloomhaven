using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SRDebugger.UI.Other;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class SafeAreaSizer : UIBehaviour, ILayoutElement
{
	[SerializeField]
	[FormerlySerializedAs("Edge")]
	private RectTransform.Edge _edge;

	public float Scale = 1f;

	private float _height;

	private float _width;

	public RectTransform.Edge Edge
	{
		get
		{
			return _edge;
		}
		set
		{
			if (_edge != value)
			{
				_edge = value;
				LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
			}
		}
	}

	public float preferredWidth => _width;

	public float preferredHeight => _height;

	public float minWidth => _width;

	public float minHeight => _height;

	public int layoutPriority => 2;

	public float flexibleHeight => -1f;

	public float flexibleWidth => -1f;

	private void Refresh()
	{
		Rect safeArea = Screen.safeArea;
		Canvas componentInParent = GetComponentInParent<Canvas>();
		if (!(componentInParent == null))
		{
			RectTransform component = componentInParent.GetComponent<RectTransform>();
			_width = (_height = 0f);
			switch (_edge)
			{
			case RectTransform.Edge.Left:
				_width = safeArea.x / componentInParent.pixelRect.width * component.rect.width;
				break;
			case RectTransform.Edge.Right:
				_width = ((float)Screen.width - safeArea.width - safeArea.x) / componentInParent.pixelRect.width * component.rect.width;
				break;
			case RectTransform.Edge.Top:
				_height = ((float)Screen.height - safeArea.height - safeArea.y) / componentInParent.pixelRect.height * component.rect.height;
				break;
			case RectTransform.Edge.Bottom:
				_height = safeArea.y / componentInParent.pixelRect.height * component.rect.height;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			_width *= Scale;
			_height *= Scale;
		}
	}

	public void CalculateLayoutInputHorizontal()
	{
		if (Application.isPlaying)
		{
			Refresh();
		}
	}

	public void CalculateLayoutInputVertical()
	{
		if (Application.isPlaying)
		{
			Refresh();
		}
	}
}
