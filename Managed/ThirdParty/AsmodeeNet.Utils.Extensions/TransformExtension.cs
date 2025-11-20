using System.Collections;
using UnityEngine;

namespace AsmodeeNet.Utils.Extensions;

public static class TransformExtension
{
	public static void RemoveAllChildren(this Transform transform)
	{
		foreach (Transform item in transform)
		{
			Object.Destroy(item.gameObject);
		}
		transform.DetachChildren();
	}

	public static void Show(this Transform transform, bool show)
	{
		Renderer component = transform.gameObject.GetComponent<Renderer>();
		if (component != null)
		{
			component.enabled = show;
		}
		CanvasRenderer component2 = transform.gameObject.GetComponent<CanvasRenderer>();
		if (component2 != null)
		{
			component2.cull = !show;
		}
		Canvas component3 = transform.gameObject.GetComponent<Canvas>();
		if (component3 != null)
		{
			component3.enabled = show;
			return;
		}
		foreach (Transform item in transform)
		{
			item.Show(show);
		}
	}

	public static IEnumerator Bounce(this Transform transform, float bounceFactor = 1.1f, float duration = 0.3f, Vector3? idleScale = null)
	{
		Vector3 finalScale = idleScale ?? Vector3.one;
		Vector3 bouncedScale = (transform.localScale = finalScale * bounceFactor);
		float currentTime = 0f;
		do
		{
			transform.localScale = Vector3.Lerp(bouncedScale, finalScale, currentTime * 3.3333333f);
			currentTime += Time.deltaTime;
			yield return null;
		}
		while (currentTime <= duration);
	}
}
