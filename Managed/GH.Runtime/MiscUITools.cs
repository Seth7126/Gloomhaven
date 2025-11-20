using System.Collections;
using Chronos;
using UnityEngine;

public static class MiscUITools
{
	public static IEnumerator Fade(Transform t, float duration, float targetAlpha, float startAlpha = -1f)
	{
		CanvasGroup cg = t.GetComponent<CanvasGroup>();
		if (startAlpha != -1f)
		{
			cg.alpha = startAlpha;
		}
		float diffAlpha = targetAlpha - cg.alpha;
		float counter = 0f;
		while (counter < duration)
		{
			cg.alpha += Timekeeper.instance.m_GlobalClock.deltaTime * diffAlpha / duration;
			counter += Timekeeper.instance.m_GlobalClock.deltaTime;
			yield return null;
		}
		cg.alpha = targetAlpha;
	}
}
