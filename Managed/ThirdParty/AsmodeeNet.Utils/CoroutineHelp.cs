using System;
using System.Collections;
using UnityEngine;

namespace AsmodeeNet.Utils;

public class CoroutineHelp : MonoBehaviour
{
	private Action _actionAfterCoroutine;

	public void Init(IEnumerator coroutine, Action actionAfterCoroutine)
	{
		_actionAfterCoroutine = actionAfterCoroutine;
		StartCoroutine(PlayCoroutine(coroutine));
	}

	private IEnumerator PlayCoroutine(IEnumerator coroutine)
	{
		yield return coroutine;
		_actionAfterCoroutine();
	}
}
