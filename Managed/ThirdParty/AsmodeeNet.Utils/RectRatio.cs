using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AsmodeeNet.Utils;

[ExecuteAlways]
public class RectRatio : UIBehaviour
{
	public enum AxisToFit
	{
		Width,
		Height
	}

	[SerializeField]
	private Graphic _target;

	[SerializeField]
	private AxisToFit _referenceAxis;

	private float _ratio;

	protected override void OnEnable()
	{
		if ((bool)_target)
		{
			_ratio = (float)_target.mainTexture.width / (float)_target.mainTexture.height;
			OnRectTransformDimensionsChange();
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		if ((bool)_target)
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			Vector2 sizeDelta = rectTransform.sizeDelta;
			switch (_referenceAxis)
			{
			case AxisToFit.Width:
				sizeDelta.y = sizeDelta.x / _ratio;
				break;
			case AxisToFit.Height:
				sizeDelta.x = sizeDelta.y * _ratio;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			rectTransform.sizeDelta = sizeDelta;
		}
	}
}
