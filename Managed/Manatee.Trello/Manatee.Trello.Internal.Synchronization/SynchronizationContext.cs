using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Internal.RequestProcessing;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal abstract class SynchronizationContext
{
	public class WeakMulticastDelegate
	{
		private readonly List<WeakReference<IHandleSynchronization>> _handlers;

		public WeakMulticastDelegate()
		{
			_handlers = new List<WeakReference<IHandleSynchronization>>();
		}

		public void Add(IHandleSynchronization handler)
		{
			lock (_handlers)
			{
				_handlers.Add(new WeakReference<IHandleSynchronization>(handler));
			}
		}

		public void Invoke(List<string> properties)
		{
			List<WeakReference<IHandleSynchronization>> source;
			lock (_handlers)
			{
				source = _handlers.ToList();
			}
			List<WeakReference<IHandleSynchronization>> list = source.Where((WeakReference<IHandleSynchronization> h) => !_Invoke(h, properties)).ToList();
			lock (_handlers)
			{
				foreach (WeakReference<IHandleSynchronization> item in list)
				{
					_handlers.Remove(item);
				}
			}
		}

		private static bool _Invoke(WeakReference<IHandleSynchronization> handler, List<string> properties)
		{
			if (!handler.TryGetTarget(out var target))
			{
				return false;
			}
			target.HandleSynchronized(properties);
			return true;
		}
	}

	private readonly Timer _timer;

	private readonly SemaphoreSlim _semaphore;

	private readonly object _updateLock;

	private readonly object _expireLock;

	private bool _cancelUpdate;

	private DateTime _expires;

	public abstract bool HasChanges { get; }

	public TrelloAuthorization Auth { get; }

	protected bool ManagesSubmissions { get; }

	public WeakMulticastDelegate Synchronized { get; }

	protected SynchronizationContext(TrelloAuthorization auth, bool useTimer)
	{
		ManagesSubmissions = useTimer;
		if (useTimer && TrelloConfiguration.ChangeSubmissionTime.Milliseconds != 0)
		{
			_timer = new Timer(async delegate
			{
				await _TimerElapsed();
			}, null, TrelloConfiguration.ChangeSubmissionTime, TrelloConfiguration.ChangeSubmissionTime);
		}
		_updateLock = new object();
		_expireLock = new object();
		_semaphore = new SemaphoreSlim(1, 1);
		_expires = DateTime.MinValue;
		RestRequestProcessor.LastCall += _TimerElapsed;
		Auth = auth ?? TrelloAuthorization.Default;
		Synchronized = new WeakMulticastDelegate();
	}

	~SynchronizationContext()
	{
		_timer?.Dispose();
	}

	public abstract T GetValue<T>(string property);

	public abstract Task SetValue<T>(string property, T value, CancellationToken ct);

	public async Task Synchronize(bool force, CancellationToken ct)
	{
		if (Auth == TrelloAuthorization.Null)
		{
			return;
		}
		DateTime now = DateTime.Now;
		if (!force && _expires > now)
		{
			return;
		}
		lock (_expireLock)
		{
			if (!force && _expires > now)
			{
				return;
			}
			_expires = now + TrelloConfiguration.RefreshThrottle;
		}
		object newData = await GetBasicData(ct);
		lock (_updateLock)
		{
			Merge(newData);
		}
	}

	protected abstract Task<object> GetBasicData(CancellationToken ct);

	protected abstract void Merge(object newData);

	protected abstract Task Submit(CancellationToken ct);

	protected virtual void OnMerged(List<string> properties)
	{
		Synchronized.Invoke(properties);
	}

	protected void CancelUpdate()
	{
		_cancelUpdate = true;
	}

	protected async Task ResetTimer(CancellationToken ct)
	{
		if (_timer == null)
		{
			await _SubmitChanges(ct);
			return;
		}
		_timer.Stop();
		_timer.Start(TrelloConfiguration.ChangeSubmissionTime);
	}

	protected async Task DoLocked(Func<CancellationToken, Task> action, CancellationToken ct)
	{
		await _semaphore.WaitAsync(ct);
		try
		{
			await action(ct);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	private async Task _TimerElapsed()
	{
		_timer?.Stop();
		if (!_cancelUpdate)
		{
			await DoLocked(async delegate(CancellationToken t)
			{
				await _SubmitChanges(t);
			}, CancellationToken.None);
		}
	}

	private async Task _SubmitChanges(CancellationToken ct)
	{
		if (!(Auth == TrelloAuthorization.Null) && HasChanges)
		{
			await Submit(ct);
		}
	}
}
internal abstract class SynchronizationContext<TJson> : SynchronizationContext where TJson : class
{
	protected static Dictionary<string, Property<TJson>> Properties;

	private readonly List<string> _localChanges;

	private readonly object _mergeLock;

	public override bool HasChanges => _localChanges?.Any() ?? false;

	protected bool IsInitialized { get; private set; }

	internal TJson Data { get; }

	protected SynchronizationContext(TrelloAuthorization auth, bool useTimer = true)
		: base(auth, useTimer)
	{
		Data = TrelloConfiguration.JsonFactory.Create<TJson>();
		_localChanges = new List<string>();
		_mergeLock = new object();
	}

	public sealed override T GetValue<T>(string property)
	{
		return (T)Properties[property].Get(Data, base.Auth);
	}

	public override Task SetValue<T>(string property, T value, CancellationToken ct)
	{
		return DoLocked(async delegate(CancellationToken t)
		{
			if (CanUpdate())
			{
				Properties[property].Set(Data, value);
				RaiseUpdated(new string[1] { property });
				_localChanges.Add(property);
				await ResetTimer(t);
			}
		}, ct);
	}

	public virtual Endpoint GetRefreshEndpoint()
	{
		throw new NotImplementedException();
	}

	protected virtual async Task<TJson> GetData(CancellationToken ct)
	{
		Endpoint refreshEndpoint = GetRefreshEndpoint();
		Dictionary<string, object> parameters = GetParameters();
		TJson result = await JsonRepository.Execute<TJson>(base.Auth, refreshEndpoint, ct, parameters);
		MarkInitialized();
		return result;
	}

	protected virtual Task SubmitData(TJson json, CancellationToken ct)
	{
		return Task.CompletedTask;
	}

	protected virtual void ApplyDependentChanges(TJson json)
	{
	}

	protected virtual Dictionary<string, object> GetParameters()
	{
		return new Dictionary<string, object>();
	}

	protected sealed override async Task<object> GetBasicData(CancellationToken ct)
	{
		return await GetData(ct);
	}

	protected sealed override void Merge(object newData)
	{
		Merge((TJson)newData);
	}

	protected sealed override async Task Submit(CancellationToken ct)
	{
		TJson changes = GetChanges();
		if (base.ManagesSubmissions)
		{
			ApplyDependentChanges(changes);
			await SubmitData(changes, ct);
			ClearChanges();
		}
	}

	protected virtual IEnumerable<string> MergeDependencies(TJson json, bool overwrite)
	{
		return Enumerable.Empty<string>();
	}

	protected virtual bool CanUpdate()
	{
		return true;
	}

	protected Task HandleSubmitRequested(string propertyName, CancellationToken ct)
	{
		_AddLocalChange(propertyName);
		return ResetTimer(ct);
	}

	protected void MarkInitialized()
	{
		IsInitialized = true;
	}

	internal IEnumerable<string> Merge(TJson json, bool overwrite = true)
	{
		if (json is IAcceptId { ValidForMerge: false })
		{
			return Enumerable.Empty<string>();
		}
		IEnumerable<string> source;
		lock (_mergeLock)
		{
			MarkInitialized();
			if (!CanUpdate())
			{
				return Enumerable.Empty<string>();
			}
			if (object.Equals(json, null) || json == Data)
			{
				return Enumerable.Empty<string>();
			}
			List<string> list = new List<string>();
			foreach (string item in Properties.Keys.Except(_localChanges))
			{
				Property<TJson> property = Properties[item];
				object objB = property.Get(Data, base.Auth);
				object obj = property.Get(json, base.Auth);
				if ((obj != null || overwrite) && !object.Equals(obj, objB))
				{
					property.Set(Data, obj);
					if (!property.IsHidden)
					{
						list.Add(item);
					}
				}
			}
			source = list.Concat(MergeDependencies(json, overwrite));
		}
		List<string> list2 = source.ToList();
		if (list2.Any())
		{
			OnMerged(list2);
			RaiseUpdated(list2);
		}
		return list2;
	}

	internal void ClearChanges()
	{
		_localChanges.Clear();
	}

	internal TJson GetChanges()
	{
		TJson val = TrelloConfiguration.JsonFactory.Create<TJson>();
		foreach (string localChange in _localChanges)
		{
			Property<TJson> property = Properties[localChange];
			object arg = property.Get(Data, base.Auth);
			property.Set(val, arg);
		}
		return val;
	}

	internal void RaiseUpdated(IEnumerable<string> properties)
	{
		if (TrelloConfiguration.EnableConsistencyProcessing && Data is IJsonCacheable data)
		{
			EventAggregator.Publish(EntityUpdatedEvent.Create(typeof(TJson), data, properties));
		}
	}

	internal void RaiseDeleted()
	{
		if (TrelloConfiguration.EnableConsistencyProcessing && Data is IJsonCacheable data)
		{
			EventAggregator.Publish(EntityDeletedEvent.Create(typeof(TJson), data));
		}
	}

	private void _AddLocalChange(string property)
	{
		if (!_localChanges.Contains(property))
		{
			_localChanges.Add(property);
		}
	}
}
