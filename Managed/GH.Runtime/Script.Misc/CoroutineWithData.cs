using System.Collections;
using UnityEngine;

namespace Script.Misc;

public class CoroutineWithData<T> where T : class
{
	public T Result;

	private readonly IEnumerator _target;

	public Coroutine Coroutine { get; }

	public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
	{
		_target = target;
		Coroutine = owner.StartCoroutine(Run());
	}

	private IEnumerator Run()
	{
		while (_target.MoveNext())
		{
			Result = _target.Current as T;
			yield return _target.Current as IEnumerator;
		}
	}
}
