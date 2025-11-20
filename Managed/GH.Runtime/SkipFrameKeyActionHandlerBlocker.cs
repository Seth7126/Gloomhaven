using System;
using System.Collections;
using UnityEngine;

public class SkipFrameKeyActionHandlerBlocker : IKeyActionHandlerBlocker
{
	private readonly MonoBehaviour _coroutineRunner;

	private IEnumerator _enumerator;

	public bool IsBlock { get; private set; }

	public event Action BlockStateChanged;

	public SkipFrameKeyActionHandlerBlocker(MonoBehaviour coroutineRunner, bool defaultIsBlock = true)
	{
		_coroutineRunner = coroutineRunner;
		IsBlock = defaultIsBlock;
	}

	public void Run()
	{
		IsBlock = true;
		this.BlockStateChanged?.Invoke();
		_enumerator = SkipFrame();
		_coroutineRunner.StartCoroutine(_enumerator);
	}

	private IEnumerator SkipFrame()
	{
		yield return null;
		IsBlock = false;
		_enumerator = null;
		this.BlockStateChanged?.Invoke();
	}

	public void Clear()
	{
		if (_enumerator != null)
		{
			_coroutineRunner.StopCoroutine(_enumerator);
			_enumerator = null;
		}
		this.BlockStateChanged = null;
	}
}
