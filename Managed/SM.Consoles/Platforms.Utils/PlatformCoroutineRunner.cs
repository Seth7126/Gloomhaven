using System.Collections;
using UnityEngine;

namespace Platforms.Utils;

public class PlatformCoroutineRunner : MonoBehaviour, IPlatformCoroutineRunner
{
	public Coroutine StartPlatformCoroutine(IEnumerator routine)
	{
		return StartCoroutine(routine);
	}

	public void StopPlatformCoroutine(IEnumerator routine)
	{
		StopCoroutine(routine);
	}

	public void StopPlatformCoroutine(Coroutine routine)
	{
		StopCoroutine(routine);
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
