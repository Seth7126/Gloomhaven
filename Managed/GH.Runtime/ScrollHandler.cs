using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollHandler
{
	private readonly MonoBehaviour _operatingMonoBehaviour;

	private Coroutine _currentRectCor;

	public ScrollHandler(MonoBehaviour operatingMonoBehaviour)
	{
		_operatingMonoBehaviour = operatingMonoBehaviour;
	}

	public void ScrollVerticallyToTop(ScrollRect scrollRect, RectTransform target, VerticalLayoutGroup verticalLayoutGroup, float time)
	{
		if (_currentRectCor != null)
		{
			_operatingMonoBehaviour.StopCoroutine(_currentRectCor);
			_currentRectCor = null;
		}
		_currentRectCor = _operatingMonoBehaviour.StartCoroutine(ScrollVerticallyToTopInternal(scrollRect, target.transform as RectTransform, verticalLayoutGroup, time));
	}

	private IEnumerator ScrollVerticallyToTopInternal(ScrollRect scrollRect, RectTransform target, VerticalLayoutGroup verticalLayoutGroup, float time)
	{
		float yPos = (scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - scrollRect.transform.InverseTransformPoint(target.position)).y - (1f - target.pivot.y) * target.sizeDelta.y - (float)verticalLayoutGroup.padding.top;
		float yPosStart = scrollRect.content.anchoredPosition.y;
		float currentTime = 0f;
		while (currentTime < time)
		{
			float y = Mathf.Lerp(yPosStart, yPos, currentTime / time);
			scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, y);
			scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
			currentTime += Time.deltaTime;
			yield return null;
		}
		scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, yPos);
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}
}
