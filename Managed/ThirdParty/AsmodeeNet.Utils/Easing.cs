using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AsmodeeNet.Utils;

public static class Easing
{
	public static IEnumerator EaseFromTo(float from, float to, float duration, Easer easer, Action<float> easeMethod, Action actionAfterEasing = null)
	{
		float elapsed = 0f;
		float range = to - from;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			float obj = from + range * easer(elapsed / duration);
			easeMethod(obj);
			yield return 0;
		}
		easeMethod(to);
		actionAfterEasing?.Invoke();
	}

	public static IEnumerator EaseFromTo(Vector2 from, Vector2 to, float duration, Easer easer, Action<Vector2> easeMethod, Action actionAfterEasing = null)
	{
		float elapsed = 0f;
		Vector2 range = to - from;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			Vector2 obj = from + range * easer(elapsed / duration);
			easeMethod(obj);
			yield return 0;
		}
		easeMethod(to);
		actionAfterEasing?.Invoke();
	}

	public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration, Easer easer, Action actionAfterEasing)
	{
		float elapsed = 0f;
		Vector3 start = transform.localPosition;
		Vector3 range = target - start;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			if (transform != null)
			{
				transform.localPosition = start + range * easer(elapsed / duration);
			}
			yield return 0;
		}
		if (transform != null)
		{
			transform.localPosition = target;
		}
		actionAfterEasing?.Invoke();
	}

	public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration, Easer ease, Action<object[]> actionAfterEasing = null, params object[] parameters)
	{
		float elapsed = 0f;
		Vector3 start = transform.localPosition;
		Vector3 range = target - start;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			if (transform != null)
			{
				transform.localPosition = start + range * ease(elapsed / duration);
			}
			yield return 0;
		}
		if (transform != null)
		{
			transform.localPosition = target;
		}
		actionAfterEasing?.Invoke(parameters);
	}

	public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration)
	{
		return transform.MoveTo(target, duration, Ease.Linear, null, Array.Empty<object>());
	}

	public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration, EaseType ease, Action actionAfterEasing)
	{
		return transform.MoveTo(target, duration, Ease.FromType(ease), actionAfterEasing);
	}

	public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration, EaseType ease, Action<object[]> actionAfterEasing = null, params object[] parameters)
	{
		return transform.MoveTo(target, duration, Ease.FromType(ease), actionAfterEasing, parameters);
	}

	public static IEnumerator MoveFrom(this Transform transform, Vector3 target, float duration, Easer ease)
	{
		Vector3 localPosition = transform.localPosition;
		transform.localPosition = target;
		return transform.MoveTo(localPosition, duration, ease, null, Array.Empty<object>());
	}

	public static IEnumerator MoveFrom(this Transform transform, Vector3 target, float duration)
	{
		return transform.MoveFrom(target, duration, Ease.Linear);
	}

	public static IEnumerator MoveFrom(this Transform transform, Vector3 target, float duration, EaseType ease)
	{
		return transform.MoveFrom(target, duration, Ease.FromType(ease));
	}

	public static IEnumerator ScaleLayoutTo(this LayoutElement layoutElement, float minWidth, float minHeight, float duration, Easer ease, Action actionAfterEasing = null)
	{
		float elapsed = 0f;
		Vector2 start = new Vector2(layoutElement.minWidth, layoutElement.minHeight);
		Vector2 range = new Vector2(minWidth, minHeight) - start;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			layoutElement.minWidth = start.x + range.x * ease(elapsed / duration);
			layoutElement.minHeight = start.y + range.y * ease(elapsed / duration);
			yield return 0;
		}
		layoutElement.minWidth = minWidth;
		layoutElement.minHeight = minHeight;
		actionAfterEasing?.Invoke();
	}

	public static IEnumerator ScaleRectransformTo(this RectTransform rectTransform, float width, float height, float duration, Easer ease, Action actionAfterEasing = null)
	{
		float elapsed = 0f;
		Vector2 start = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
		Vector2 range = new Vector2(width, height) - start;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, start.x + range.x * ease(elapsed / duration));
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, start.y + range.y * ease(elapsed / duration));
			yield return 0;
		}
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		actionAfterEasing?.Invoke();
	}

	public static IEnumerator ScaleTo(this Transform transform, Vector3 target, float duration, Easer ease, Action actionAfterEasing = null)
	{
		float elapsed = 0f;
		Vector3 start = transform.localScale;
		Vector3 range = target - start;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			transform.localScale = start + range * ease(elapsed / duration);
			yield return 0;
		}
		transform.localScale = target;
		actionAfterEasing?.Invoke();
	}

	public static IEnumerator ScaleTo(this Transform transform, Vector3 target, float duration)
	{
		return transform.ScaleTo(target, duration, Ease.Linear);
	}

	public static IEnumerator ScaleTo(this Transform transform, Vector3 target, float duration, EaseType ease, Action actionAfterEasing = null)
	{
		return transform.ScaleTo(target, duration, Ease.FromType(ease), actionAfterEasing);
	}

	public static IEnumerator ScaleFrom(this Transform transform, Vector3 target, float duration, Easer ease)
	{
		Vector3 localScale = transform.localScale;
		transform.localScale = target;
		return transform.ScaleTo(localScale, duration, ease);
	}

	public static IEnumerator ScaleFrom(this Transform transform, Vector3 target, float duration)
	{
		return transform.ScaleFrom(target, duration, Ease.Linear);
	}

	public static IEnumerator ScaleFrom(this Transform transform, Vector3 target, float duration, EaseType ease)
	{
		return transform.ScaleFrom(target, duration, Ease.FromType(ease));
	}

	public static IEnumerator RotateTo(this Transform transform, Quaternion target, float duration, Easer ease)
	{
		float elapsed = 0f;
		Quaternion start = transform.localRotation;
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			transform.localRotation = Quaternion.Lerp(start, target, ease(elapsed / duration));
			yield return 0;
		}
		transform.localRotation = target;
	}

	public static IEnumerator RotateTo(this Transform transform, Quaternion target, float duration)
	{
		return transform.RotateTo(target, duration, Ease.Linear);
	}

	public static IEnumerator RotateTo(this Transform transform, Quaternion target, float duration, EaseType ease)
	{
		return transform.RotateTo(target, duration, Ease.FromType(ease));
	}

	public static IEnumerator RotateFrom(this Transform transform, Quaternion target, float duration, Easer ease)
	{
		Quaternion localRotation = transform.localRotation;
		transform.localRotation = target;
		return transform.RotateTo(localRotation, duration, ease);
	}

	public static IEnumerator RotateFrom(this Transform transform, Quaternion target, float duration)
	{
		return transform.RotateFrom(target, duration, Ease.Linear);
	}

	public static IEnumerator RotateFrom(this Transform transform, Quaternion target, float duration, EaseType ease)
	{
		return transform.RotateFrom(target, duration, Ease.FromType(ease));
	}

	public static IEnumerator CurveTo(this Transform transform, Vector3 control, Vector3 target, float duration, Easer ease)
	{
		float elapsed = 0f;
		Vector3 start = transform.localPosition;
		Vector3 localPosition = default(Vector3);
		while (elapsed < duration)
		{
			elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
			float num = ease(elapsed / duration);
			localPosition.x = start.x * (1f - num) * (1f - num) + control.x * 2f * (1f - num) * num + target.x * num * num;
			localPosition.y = start.y * (1f - num) * (1f - num) + control.y * 2f * (1f - num) * num + target.y * num * num;
			localPosition.z = start.z * (1f - num) * (1f - num) + control.z * 2f * (1f - num) * num + target.z * num * num;
			transform.localPosition = localPosition;
			yield return 0;
		}
		transform.localPosition = target;
	}

	public static IEnumerator CurveTo(this Transform transform, Vector3 control, Vector3 target, float duration)
	{
		return transform.CurveTo(control, target, duration, Ease.Linear);
	}

	public static IEnumerator CurveTo(this Transform transform, Vector3 control, Vector3 target, float duration, EaseType ease)
	{
		return transform.CurveTo(control, target, duration, Ease.FromType(ease));
	}

	public static IEnumerator CurveFrom(this Transform transform, Vector3 control, Vector3 start, float duration, Easer ease)
	{
		Vector3 localPosition = transform.localPosition;
		transform.localPosition = start;
		return transform.CurveTo(control, localPosition, duration, ease);
	}

	public static IEnumerator CurveFrom(this Transform transform, Vector3 control, Vector3 start, float duration)
	{
		return transform.CurveFrom(control, start, duration, Ease.Linear);
	}

	public static IEnumerator CurveFrom(this Transform transform, Vector3 control, Vector3 start, float duration, EaseType ease)
	{
		return transform.CurveFrom(control, start, duration, Ease.FromType(ease));
	}

	public static IEnumerator Shake(this Transform transform, Vector3 amount, float duration)
	{
		Vector3 start = transform.localPosition;
		Vector3 shake = Vector3.zero;
		while (duration > 0f)
		{
			duration -= Time.deltaTime;
			shake.Set(UnityEngine.Random.Range(0f - amount.x, amount.x), UnityEngine.Random.Range(0f - amount.y, amount.y), UnityEngine.Random.Range(0f - amount.z, amount.z));
			transform.localPosition = start + shake;
			yield return 0;
		}
		transform.localPosition = start;
	}

	public static IEnumerator Shake(this Transform transform, float amount, float duration)
	{
		return transform.Shake(new Vector3(amount, amount, amount), duration);
	}

	public static IEnumerator Wait(float duration)
	{
		while (duration > 0f)
		{
			duration -= Time.deltaTime;
			yield return 0;
		}
	}

	public static IEnumerator WaitUntil(Predicate predicate)
	{
		while (!predicate())
		{
			yield return 0;
		}
	}

	public static float Loop(float duration, float from, float to, float offsetPercent)
	{
		float num = to - from;
		float num2 = (Time.time + duration * offsetPercent) * (Mathf.Abs(num) / duration);
		if (num > 0f)
		{
			return from + Time.time - num * (float)Mathf.FloorToInt(Time.time / num);
		}
		return from - (Time.time - Mathf.Abs(num) * (float)Mathf.FloorToInt(num2 / Mathf.Abs(num)));
	}

	public static float Loop(float duration, float from, float to)
	{
		return Loop(duration, from, to, 0f);
	}

	public static Vector3 Loop(float duration, Vector3 from, Vector3 to, float offsetPercent)
	{
		return Vector3.Lerp(from, to, Loop(duration, 0f, 1f, offsetPercent));
	}

	public static Vector3 Loop(float duration, Vector3 from, Vector3 to)
	{
		return Vector3.Lerp(from, to, Loop(duration, 0f, 1f));
	}

	public static Quaternion Loop(float duration, Quaternion from, Quaternion to, float offsetPercent)
	{
		return Quaternion.Lerp(from, to, Loop(duration, 0f, 1f, offsetPercent));
	}

	public static Quaternion Loop(float duration, Quaternion from, Quaternion to)
	{
		return Quaternion.Lerp(from, to, Loop(duration, 0f, 1f));
	}

	public static float Wave(float duration, float from, float to, float offsetPercent)
	{
		float num = (to - from) / 2f;
		return from + num + Mathf.Sin((Time.time + duration * offsetPercent) / duration * (MathF.PI * 2f)) * num;
	}

	public static float Wave(float duration, float from, float to)
	{
		return Wave(duration, from, to, 0f);
	}

	public static Vector3 Wave(float duration, Vector3 from, Vector3 to, float offsetPercent)
	{
		return Vector3.Lerp(from, to, Wave(duration, 0f, 1f, offsetPercent));
	}

	public static Vector3 Wave(float duration, Vector3 from, Vector3 to)
	{
		return Vector3.Lerp(from, to, Wave(duration, 0f, 1f));
	}

	public static Quaternion Wave(float duration, Quaternion from, Quaternion to, float offsetPercent)
	{
		return Quaternion.Lerp(from, to, Wave(duration, 0f, 1f, offsetPercent));
	}

	public static Quaternion Wave(float duration, Quaternion from, Quaternion to)
	{
		return Quaternion.Lerp(from, to, Wave(duration, 0f, 1f));
	}
}
