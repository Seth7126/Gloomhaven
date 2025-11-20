using System.Linq;
using DynamicScroll;
using SM.Gamepad;
using UnityEngine;
using UnityEngine.UI;

public static class ScrollRectExtensions
{
	private enum VisibleState
	{
		FullVisible = 1,
		PartiallyVisibleAtTop,
		PartiallyVisibleAtBottom,
		AboveViewport,
		UnderViewport,
		BiggerThanViewport
	}

	private static VisibleState GetVisibleState(ScrollRect scrollRect, RectTransform rectTransform)
	{
		Camera uICamera = UIManager.Instance.UICamera;
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		Vector3[] array3 = new Vector3[4];
		rectTransform.GetWorldCorners(array2);
		scrollRect.viewport.GetWorldCorners(array);
		Vector2 vector = uICamera.WorldToScreenPoint(array[2]);
		Vector2 vector2 = uICamera.WorldToScreenPoint(array[0]);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		for (int i = 0; i < 4; i++)
		{
			array3[i] = uICamera.WorldToScreenPoint(array2[i]);
		}
		if (array3[0].y < vector2.y)
		{
			flag4 = true;
		}
		if (array3[0].y > vector.y)
		{
			flag2 = true;
		}
		if (array3[2].y > vector.y)
		{
			flag = true;
		}
		if (array3[2].y < vector2.y)
		{
			flag3 = true;
		}
		if (flag2)
		{
			return VisibleState.AboveViewport;
		}
		if (flag3)
		{
			return VisibleState.UnderViewport;
		}
		if (flag)
		{
			if (flag4)
			{
				return VisibleState.BiggerThanViewport;
			}
			return VisibleState.PartiallyVisibleAtTop;
		}
		if (flag4)
		{
			return VisibleState.PartiallyVisibleAtBottom;
		}
		return VisibleState.FullVisible;
	}

	public static void ScrollVerticallyTo(this ScrollRect scrollRect, RectTransform target)
	{
		Vector3 vector = scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - scrollRect.transform.InverseTransformPoint(target.position);
		scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, vector.y);
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}

	public static void ScrollVerticallyToTop(this ScrollRect scrollRect, RectTransform target, float offset = 0f)
	{
		Vector3 vector = scrollRect.transform.InverseTransformPoint(scrollRect.content.position) - scrollRect.transform.InverseTransformPoint(target.position);
		scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, vector.y - (1f - target.pivot.y) * target.sizeDelta.y - offset);
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}

	public static void ScrollLayoutGroupVerticallyToTop(this ScrollRect scrollRect, RectTransform target)
	{
		LayoutGroup component = scrollRect.content.GetComponent<LayoutGroup>();
		scrollRect.ScrollVerticallyToTop(target, (!(component == null)) ? component.padding.top : 0);
	}

	public static void ScrollToFit(this ScrollRect scrollRect, RectTransform target)
	{
		Vector2 vector = scrollRect.viewport.InverseTransformPoint(target.position);
		float num = vector.y - target.pivot.y * target.rect.height;
		float num2 = vector.y + (1f - target.pivot.y) * target.rect.height;
		if (num < 0f - scrollRect.viewport.rect.height)
		{
			scrollRect.content.anchoredPosition += new Vector2(0f, 0f - (scrollRect.viewport.rect.height + num));
		}
		else if (num2 > 0f)
		{
			scrollRect.content.anchoredPosition -= new Vector2(0f, num2);
		}
		scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
	}

	public static void ResizeVerticallyToFitContent(this ScrollRect scrollRect, float maxSize)
	{
		scrollRect.ResizeToFitContent(horizontally: false, vertically: true, new Vector2(0f, maxSize));
	}

	public static void ResizeHorizontallyToFitContent(this ScrollRect scrollRect, float maxSize)
	{
		scrollRect.ResizeToFitContent(horizontally: true, vertically: false, new Vector2(maxSize, 0f));
	}

	public static void ResizeToFitContent(this ScrollRect scrollRect, bool horizontally, bool vertically, Vector2 maxSize)
	{
		RectTransform component = scrollRect.GetComponent<RectTransform>();
		Vector2 size = component.GetSize();
		if (horizontally)
		{
			size.x = Mathf.Min(scrollRect.content.sizeDelta.x, maxSize.x);
		}
		if (vertically)
		{
			size.y = Mathf.Min(scrollRect.content.sizeDelta.y, maxSize.y);
		}
		component.SetSize(size);
	}

	private static RectTransform GetParentTo(RectTransform element, params RectTransform[] targets)
	{
		if (targets.Contains(element.parent))
		{
			return element;
		}
		return GetParentTo(element.parent as RectTransform, targets);
	}

	public static bool IsFullyVisibleInViewport(this ScrollRect scroll, IUiNavigationElement element)
	{
		return scroll.IsFullyVisibleInViewport(element.RectTransform);
	}

	public static bool IsFullyVisibleInViewport(this ScrollRect scroll, RectTransform target)
	{
		return target.FitsInRectTransform(UIManager.Instance.UICamera, scroll.viewport);
	}

	public static bool IsPartiallyVisibleInViewport(this ScrollRect scroll, IUiNavigationElement element)
	{
		return scroll.IsPartiallyVisibleInViewport(element.RectTransform);
	}

	public static bool IsPartiallyVisibleInViewport(this ScrollRect scroll, RectTransform target)
	{
		VisibleState visibleState = GetVisibleState(scroll, target);
		if (visibleState != VisibleState.FullVisible && visibleState != VisibleState.PartiallyVisibleAtBottom)
		{
			return visibleState == VisibleState.PartiallyVisibleAtTop;
		}
		return true;
	}
}
