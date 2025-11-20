using UnityEngine;

public class UIQuestPointer : UIPointerTo
{
	[SerializeField]
	private GameObject outsideAreaMask;

	public void PointToScreenPoint(Vector3 targetScreenPoint, RectTransform area)
	{
		RectTransform rectTransform = base.transform as RectTransform;
		RectTransform rectTransform2 = rectTransform.parent as RectTransform;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform2, targetScreenPoint, UIManager.Instance.UICamera, out var worldPoint);
		rectTransform.position = worldPoint;
		rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(rectTransform.anchoredPosition.x, (0f - rectTransform2.sizeDelta.x) / 2f, rectTransform2.sizeDelta.x / 2f), Mathf.Clamp(rectTransform.anchoredPosition.y, 0f, rectTransform2.sizeDelta.y));
		RectTransformUtility.ScreenPointToWorldPointInRectangle(area, targetScreenPoint, UIManager.Instance.UICamera, out var worldPoint2);
		PointToWorldPosition(worldPoint2);
		if (outsideAreaMask != null)
		{
			bool flag = RectTransformUtility.RectangleContainsScreenPoint(area, targetScreenPoint, UIManager.Instance.UICamera);
			outsideAreaMask.SetActive(!flag);
		}
	}
}
