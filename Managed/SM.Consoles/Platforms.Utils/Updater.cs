using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Platforms.Utils;

public class Updater : MonoBehaviour, IUpdater, IDisposable
{
	private class CooldownInfo
	{
		private float _previousAction;

		private float _cooldown;

		public float PreviousAction
		{
			get
			{
				return _previousAction;
			}
			set
			{
				_previousAction = value;
			}
		}

		public float Cooldown => _cooldown;

		public bool IsReady => _previousAction + _cooldown <= Time.unscaledTime;

		public CooldownInfo(float cooldown)
		{
			_previousAction = Time.unscaledTime;
			_cooldown = cooldown;
		}
	}

	private DebugFlags _sendDebug = DebugFlags.Error;

	private List<Action> _updatables = new List<Action>();

	private List<Action> _fixedUpdatables = new List<Action>();

	private Dictionary<Action, CooldownInfo> _withCooldown = new Dictionary<Action, CooldownInfo>();

	private ConcurrentQueue<Action> _executeInMainThreadQueue = new ConcurrentQueue<Action>();

	private ConcurrentQueue<Action> _updatablesChangesQueue = new ConcurrentQueue<Action>();

	private ConcurrentQueue<Action> _fixedUpdatablesChangesQueue = new ConcurrentQueue<Action>();

	private ConcurrentQueue<Action> _withCooldownChangesQueue = new ConcurrentQueue<Action>();

	private bool _disposed;

	public DebugFlags SendDebug
	{
		get
		{
			return _sendDebug;
		}
		set
		{
			_sendDebug = value;
		}
	}

	public void SubscribeForUpdate(Action onUpdate)
	{
		_updatablesChangesQueue.Enqueue(delegate
		{
			if (!_updatables.Contains(onUpdate))
			{
				_updatables.Add(onUpdate);
			}
		});
	}

	public void UnsubscribeFromUpdate(Action onUpdate)
	{
		if (_disposed)
		{
			return;
		}
		_updatablesChangesQueue.Enqueue(delegate
		{
			if (_updatables.Contains(onUpdate))
			{
				_updatables.Remove(onUpdate);
			}
		});
	}

	public void SubscribeForFixedUpdate(Action onUpdate)
	{
		_fixedUpdatablesChangesQueue.Enqueue(delegate
		{
			if (!_fixedUpdatables.Contains(onUpdate))
			{
				_fixedUpdatables.Add(onUpdate);
			}
		});
	}

	public void UnsubscribeFromFixedUpdate(Action onUpdate)
	{
		if (_disposed)
		{
			return;
		}
		_fixedUpdatablesChangesQueue.Enqueue(delegate
		{
			if (_fixedUpdatables.Contains(onUpdate))
			{
				_fixedUpdatables.Remove(onUpdate);
			}
		});
	}

	public void SubscribeForCooldown(Action onUpdate, float сooldownInSeconds)
	{
		_withCooldownChangesQueue.Enqueue(delegate
		{
			if (!_withCooldown.ContainsKey(onUpdate))
			{
				_withCooldown.Add(onUpdate, new CooldownInfo(сooldownInSeconds));
			}
		});
	}

	public void UnsubscribeFromCooldown(Action onUpdate)
	{
		if (_disposed)
		{
			return;
		}
		_withCooldownChangesQueue.Enqueue(delegate
		{
			if (_withCooldown.ContainsKey(onUpdate))
			{
				_withCooldown.Remove(onUpdate);
			}
		});
	}

	public void ExecuteInMainThread(Action onUpdate)
	{
		_executeInMainThreadQueue.Enqueue(onUpdate);
	}

	public void Dispose()
	{
		_updatables.Clear();
		_updatablesChangesQueue = null;
		_fixedUpdatables.Clear();
		_fixedUpdatablesChangesQueue = null;
		_withCooldown.Clear();
		_withCooldownChangesQueue = null;
		_executeInMainThreadQueue = null;
		_disposed = true;
	}

	private void OnDestroy()
	{
		Dispose();
	}

	private void Update()
	{
		while (!_updatablesChangesQueue.IsEmpty)
		{
			_updatablesChangesQueue.TryDequeue(out var result);
			result?.Invoke();
		}
		foreach (Action updatable in _updatables)
		{
			updatable();
		}
		if (_executeInMainThreadQueue != null && !_executeInMainThreadQueue.IsEmpty)
		{
			Action result2;
			while (_executeInMainThreadQueue.TryDequeue(out result2))
			{
				result2?.Invoke();
			}
		}
		while (!_withCooldownChangesQueue.IsEmpty)
		{
			_withCooldownChangesQueue.TryDequeue(out var result3);
			result3?.Invoke();
		}
		foreach (KeyValuePair<Action, CooldownInfo> item in _withCooldown)
		{
			Action key = item.Key;
			CooldownInfo value = item.Value;
			if (value.IsReady)
			{
				value.PreviousAction = Time.unscaledTime;
				key();
			}
		}
	}

	private void FixedUpdate()
	{
		while (!_fixedUpdatablesChangesQueue.IsEmpty)
		{
			_fixedUpdatablesChangesQueue.TryDequeue(out var result);
			result?.Invoke();
		}
		foreach (Action fixedUpdatable in _fixedUpdatables)
		{
			fixedUpdatable();
		}
	}
}
