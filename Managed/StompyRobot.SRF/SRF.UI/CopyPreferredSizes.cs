using System;
using UnityEngine;
using UnityEngine.UI;

namespace SRF.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
[AddComponentMenu("SRF/UI/Copy Preferred Size (Multiple)")]
public class CopyPreferredSizes : LayoutElement
{
	public enum Operations
	{
		Max,
		Min
	}

	[Serializable]
	public class CopySource
	{
		public RectTransform Rect;

		public float PaddingHeight;

		public float PaddingWidth;
	}

	public CopySource[] CopySources;

	public Operations Operation;

	public override float preferredWidth
	{
		get
		{
			if (CopySources == null || CopySources.Length == 0 || !IsActive())
			{
				return -1f;
			}
			float num = ((Operation == Operations.Max) ? float.MinValue : float.MaxValue);
			for (int i = 0; i < CopySources.Length; i++)
			{
				if (!(CopySources[i].Rect == null))
				{
					float num2 = LayoutUtility.GetPreferredWidth(CopySources[i].Rect) + CopySources[i].PaddingWidth;
					if (Operation == Operations.Max && num2 > num)
					{
						num = num2;
					}
					else if (Operation == Operations.Min && num2 < num)
					{
						num = num2;
					}
				}
			}
			if (Operation == Operations.Max && num == float.MinValue)
			{
				return -1f;
			}
			if (Operation == Operations.Min && num == float.MaxValue)
			{
				return -1f;
			}
			return num;
		}
	}

	public override float preferredHeight
	{
		get
		{
			if (CopySources == null || CopySources.Length == 0 || !IsActive())
			{
				return -1f;
			}
			float num = ((Operation == Operations.Max) ? float.MinValue : float.MaxValue);
			for (int i = 0; i < CopySources.Length; i++)
			{
				if (!(CopySources[i].Rect == null))
				{
					float num2 = LayoutUtility.GetPreferredHeight(CopySources[i].Rect) + CopySources[i].PaddingHeight;
					if (Operation == Operations.Max && num2 > num)
					{
						num = num2;
					}
					else if (Operation == Operations.Min && num2 < num)
					{
						num = num2;
					}
				}
			}
			if (Operation == Operations.Max && num == float.MinValue)
			{
				return -1f;
			}
			if (Operation == Operations.Min && num == float.MaxValue)
			{
				return -1f;
			}
			return num;
		}
	}

	public override int layoutPriority => 2;
}
