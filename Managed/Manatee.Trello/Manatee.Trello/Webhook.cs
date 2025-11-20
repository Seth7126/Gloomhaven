using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class Webhook<T> : IWebhook<T>, ICacheable, IRefreshable, IMergeJson<IJsonWebhook>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization where T : class, ICanWebhook
{
	private readonly Field<string> _callBackUrl;

	private readonly Field<string> _description;

	private readonly Field<bool?> _isActive;

	private readonly Field<T> _target;

	private readonly WebhookContext<T> _context;

	private DateTime? _creation;

	public string CallBackUrl
	{
		get
		{
			return _callBackUrl.Value;
		}
		set
		{
			_callBackUrl.Value = value;
		}
	}

	public DateTime CreationDate
	{
		get
		{
			if (!_creation.HasValue)
			{
				_creation = Id.ExtractCreationDate();
			}
			return _creation.Value;
		}
	}

	public string Description
	{
		get
		{
			return _description.Value;
		}
		set
		{
			_description.Value = value;
		}
	}

	public string Id { get; private set; }

	public bool? IsActive
	{
		get
		{
			return _isActive.Value;
		}
		set
		{
			_isActive.Value = value;
		}
	}

	public T Target
	{
		get
		{
			return _target.Value;
		}
		set
		{
			_target.Value = value;
		}
	}

	TrelloAuthorization IBatchRefresh.Auth => _context.Auth;

	public event Action<IWebhook<T>, IEnumerable<string>> Updated;

	public Webhook(string id, TrelloAuthorization auth = null)
	{
		Id = id;
		_context = new WebhookContext<T>(Id, auth);
		_context.Synchronized.Add(this);
		_callBackUrl = new Field<string>(_context, "CallBackUrl");
		_callBackUrl.AddRule(UriRule.Instance);
		_description = new Field<string>(_context, "Description");
		_isActive = new Field<bool?>(_context, "IsActive");
		_isActive.AddRule(NullableHasValueRule<bool>.Instance);
		_target = new Field<T>(_context, "Target");
		_target.AddRule(NotNullRule<T>.Instance);
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	private Webhook(string id, WebhookContext<T> context)
	{
		Id = id;
		_context = context;
		_context.Synchronized.Add(this);
		_callBackUrl = new Field<string>(_context, "CallBackUrl");
		_callBackUrl.AddRule(UriRule.Instance);
		_description = new Field<string>(_context, "Description");
		_isActive = new Field<bool?>(_context, "IsActive");
		_isActive.AddRule(NullableHasValueRule<bool>.Instance);
		_target = new Field<T>(_context, "Target");
		_target.AddRule(NotNullRule<T>.Instance);
		TrelloConfiguration.Cache.Add(this);
	}

	public static async Task<Webhook<T>> Create(T target, string callBackUrl, string description = null, TrelloAuthorization auth = null, CancellationToken ct = default(CancellationToken))
	{
		WebhookContext<T> context = new WebhookContext<T>(auth);
		return new Webhook<T>(await context.Create(target, description, callBackUrl, ct), context);
	}

	public async Task Delete(CancellationToken ct = default(CancellationToken))
	{
		await _context.Delete(ct);
		if (TrelloConfiguration.RemoveDeletedItemsFromCache)
		{
			TrelloConfiguration.Cache.Remove(this);
		}
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonWebhook>.Merge(IJsonWebhook json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonWebhook json = TrelloConfiguration.Deserializer.Deserialize<IJsonWebhook>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
