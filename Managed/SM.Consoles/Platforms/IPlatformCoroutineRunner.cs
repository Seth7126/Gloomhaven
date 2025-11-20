using System.Collections;
using UnityEngine;

namespace Platforms;

public interface IPlatformCoroutineRunner
{
	Coroutine StartPlatformCoroutine(IEnumerator routine);

	void StopPlatformCoroutine(Coroutine routine);

	void StopPlatformCoroutine(IEnumerator routine);
}
