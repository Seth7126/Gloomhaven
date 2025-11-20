using UnityEngine;

public class UIFollowMapLocationInsideArea : UIFollowMapLocation
{
	[SerializeField]
	private RectTransform area;

	[SerializeField]
	private float xDisplacementToReposition = 20f;

	[SerializeField]
	private UIQuestPointer pointer;

	protected override void RefreshPosition()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		rectTransform.pivot = new Vector2(0.5f, 0f);
		CalculatePosition(Vector3.zero);
		if (area == null)
		{
			return;
		}
		Vector3 vector = rectTransform.DeltaWorldPositionToFitRectTransform(UIManager.Instance.UICamera, area, checkBothAxies: true);
		if (vector.x != 0f && vector.y == 0f)
		{
			if (Mathf.Abs(vector.x) < xDisplacementToReposition)
			{
				base.transform.position += vector;
			}
			else
			{
				_ = base.transform.position;
				bool flag = vector.x > 0f;
				rectTransform.pivot = new Vector2((!flag) ? 1 : 0, 0.5f);
				CalculatePosition(Vector3.zero);
				base.transform.position += rectTransform.DeltaWorldPositionToFitRectTransform(UIManager.Instance.UICamera, area, checkBothAxies: true);
			}
		}
		else if (vector.y < 0f)
		{
			rectTransform.pivot = new Vector2(rectTransform.pivot.x, 1f);
			CalculatePosition(Vector3.zero);
			base.transform.position += rectTransform.DeltaWorldPositionToFitRectTransform(UIManager.Instance.UICamera, area, checkBothAxies: true);
		}
		else if (vector.y > 0f)
		{
			base.transform.position += vector;
		}
		RefrehsPointer();
	}

	private void RefrehsPointer()
	{
		Vector3 screenPoint = target.GetScreenPoint(Vector3.zero);
		pointer.PointToScreenPoint(screenPoint, area);
	}
}
