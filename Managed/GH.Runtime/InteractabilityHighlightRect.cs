using System.Collections;
using UnityEngine;

public class InteractabilityHighlightRect : MonoBehaviour
{
	public RectTransform RectToStretch;

	public CanvasGroup DisplayGroup;

	public UIFX_Image_Get_RectFormat FxRectAdjuster;

	private RectTransform m_ParentCanvasRect;

	public RectTransform m_RectTransformToFollow;

	private IEnumerator m_RectFollowRoutine;

	private IEnumerator m_CanvasFadeRoutine;

	private Vector3[] m_ScreenSpaceCorners = new Vector3[4];

	private void OnEnable()
	{
		if (m_RectFollowRoutine != null)
		{
			StartCoroutine(m_RectFollowRoutine);
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void SetRectToFollow(RectTransform rectToFollow, RectTransform parentCanvasRect)
	{
		m_ParentCanvasRect = parentCanvasRect;
		m_RectTransformToFollow = rectToFollow;
		if (m_RectFollowRoutine != null)
		{
			StopCoroutine(m_RectFollowRoutine);
			m_RectFollowRoutine = null;
		}
		m_RectFollowRoutine = ContinuouslyRefitToFollowRect();
		StartCoroutine(m_RectFollowRoutine);
		SetShowing(shouldShow: true);
	}

	public void SetShowing(bool shouldShow)
	{
		if (m_CanvasFadeRoutine != null)
		{
			StopCoroutine(m_CanvasFadeRoutine);
			m_CanvasFadeRoutine = null;
		}
		m_CanvasFadeRoutine = GloomUtility.FadeCanvasGroup(DisplayGroup, 0f, 0.3f, shouldShow ? 1f : 0f, AnimationCurve.Linear(0f, 0f, 1f, 1f), null, delegate
		{
			if (!shouldShow)
			{
				Object.Destroy(base.gameObject);
			}
		});
		StartCoroutine(m_CanvasFadeRoutine);
	}

	private IEnumerator ContinuouslyRefitToFollowRect()
	{
		while (true)
		{
			Rect rect = RectTransformToScreenSpace(m_RectTransformToFollow);
			m_ScreenSpaceCorners[0] = m_ParentCanvasRect.InverseTransformPoint(rect.center + new Vector2((0f - rect.width) / 2f, (0f - rect.height) / 2f));
			m_ScreenSpaceCorners[1] = m_ParentCanvasRect.InverseTransformPoint(rect.center + new Vector2((0f - rect.width) / 2f, rect.height / 2f));
			m_ScreenSpaceCorners[2] = m_ParentCanvasRect.InverseTransformPoint(rect.center + new Vector2(rect.width / 2f, rect.height / 2f));
			m_ScreenSpaceCorners[3] = m_ParentCanvasRect.InverseTransformPoint(rect.center + new Vector2(rect.width / 2f, (0f - rect.height) / 2f));
			RectToStretch.anchoredPosition = m_ScreenSpaceCorners[1];
			float x = m_ScreenSpaceCorners[2].x - m_ScreenSpaceCorners[1].x;
			float y = m_ScreenSpaceCorners[1].y - m_ScreenSpaceCorners[0].y;
			RectToStretch.sizeDelta = new Vector2(x, y);
			FxRectAdjuster.doAdjust();
			yield return null;
		}
	}

	public Rect RectTransformToScreenSpace(RectTransform rectTransform)
	{
		Vector2 vector = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
		return new Rect((Vector2)rectTransform.position - vector * rectTransform.pivot, vector);
	}
}
