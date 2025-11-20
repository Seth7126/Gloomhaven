using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public abstract class CustomField : ICustomField, ICacheable, IRefreshable, IMergeJson<IJsonCustomField>, IHandleSynchronization
{
	private readonly Field<ICustomFieldDefinition> _definition;

	internal CustomFieldContext Context { get; }

	public string Id { get; private set; }

	public ICustomFieldDefinition Definition => _definition.Value;

	internal IJsonCustomField Json
	{
		get
		{
			return Context.Data;
		}
		set
		{
			Context.Merge(value);
		}
	}

	public event Action<ICustomField, IEnumerable<string>> Updated;

	internal CustomField(IJsonCustomField json, string cardId, TrelloAuthorization auth)
	{
		Id = json.Id;
		Context = new CustomFieldContext(Id, cardId, auth);
		_definition = new Field<ICustomFieldDefinition>(Context, "Definition");
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
		Context.Merge(json);
		Context.Synchronized.Add(this);
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return Context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonCustomField>.Merge(IJsonCustomField json, bool overwrite)
	{
		Context.Merge(json, overwrite);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = Context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
public abstract class CustomField<T> : CustomField, ICustomField<T>, ICustomField, ICacheable, IRefreshable
{
	public abstract T Value { get; set; }

	internal CustomField(IJsonCustomField json, string cardId, TrelloAuthorization auth)
		: base(json, cardId, auth)
	{
	}

	public override string ToString()
	{
		if (Value == null)
		{
			return base.Definition.ToString();
		}
		return $"{base.Definition.Name} - {Value}";
	}
}
