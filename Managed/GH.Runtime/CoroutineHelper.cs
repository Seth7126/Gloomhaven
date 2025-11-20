using System;
using System.Collections;
using Chronos;
using JetBrains.Annotations;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
	public static CoroutineHelper instance;

	[UsedImplicitly]
	private void Awake()
	{
		instance = this;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		instance = null;
	}

	public static void StopCoroutineHelper(Coroutine coroutine)
	{
		instance.StopCoroutine(coroutine);
	}

	public static Coroutine RunCoroutine(IEnumerator coroutine)
	{
		return instance.StartCoroutine(coroutine);
	}

	public static IEnumerator DelayedStartCoroutine(float delay, Action<Animator> function, Animator animator)
	{
		yield return Timekeeper.instance.WaitForSeconds(delay);
		function(animator);
	}

	public static Coroutine RunDelayedAction(float delay, Action function)
	{
		return instance.StartCoroutine(DelayedStartCoroutine(delay, function));
	}

	public static IEnumerator DelayedStartCoroutine(float delay, Action function)
	{
		yield return Timekeeper.instance.WaitForSeconds(delay);
		function();
	}

	public static Coroutine RunNextFrame(Action function)
	{
		return instance.StartCoroutine(SkipFramesCoroutine(function));
	}

	public static IEnumerator SkipFramesCoroutine(Action function, int frames = 1)
	{
		while (frames != 0)
		{
			yield return null;
			frames--;
		}
		function();
	}
}
