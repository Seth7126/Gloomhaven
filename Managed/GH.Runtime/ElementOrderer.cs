using UnityEngine;
using UnityEngine.UI;

public class ElementOrderer : MonoBehaviour
{
	private GridLayoutGroup gridLayoutGroup;

	private void Start()
	{
		gridLayoutGroup = GetComponentInParent<GridLayoutGroup>();
	}

	public void SwitchOrder(bool bringToFront)
	{
		int siblingIndex = base.transform.GetSiblingIndex();
		if (bringToFront)
		{
			base.transform.SetAsLastSibling();
		}
		else
		{
			base.transform.SetAsFirstSibling();
		}
		if (gridLayoutGroup != null && base.transform.GetSiblingIndex() != siblingIndex)
		{
			switch (gridLayoutGroup.startCorner)
			{
			case GridLayoutGroup.Corner.UpperLeft:
				gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperRight;
				break;
			case GridLayoutGroup.Corner.UpperRight:
				gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
				break;
			case GridLayoutGroup.Corner.LowerLeft:
				gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerRight;
				break;
			case GridLayoutGroup.Corner.LowerRight:
				gridLayoutGroup.startCorner = GridLayoutGroup.Corner.LowerLeft;
				break;
			}
		}
	}
}
