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

public class CustomFieldDefinition : ICustomFieldDefinition, ICacheable, IRefreshable, IMergeJson<IJsonCustomFieldDefinition>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	private readonly Field<IBoard> _board;

	private readonly Field<string> _fieldGroup;

	private readonly Field<string> _name;

	private readonly Field<Position> _position;

	private readonly Field<CustomFieldType?> _type;

	private readonly CustomFieldDefinitionContext _context;

	public IBoard Board => _board.Value;

	public ICustomFieldDisplayInfo DisplayInfo { get; }

	public string FieldGroup => _fieldGroup.Value;

	public string Id { get; private set; }

	public string Name
	{
		get
		{
			return _name.Value;
		}
		set
		{
			_name.Value = value;
		}
	}

	public IDropDownOptionCollection Options => _context.DropDownOptions;

	public Position Position
	{
		get
		{
			return _position.Value;
		}
		set
		{
			_position.Value = value;
		}
	}

	public CustomFieldType? Type => _type.Value;

	internal IJsonCustomFieldDefinition Json
	{
		get
		{
			return _context.Data;
		}
		set
		{
			_context.Merge(value);
		}
	}

	TrelloAuthorization IBatchRefresh.Auth => _context.Auth;

	public event Action<ICustomFieldDefinition, IEnumerable<string>> Updated;

	internal CustomFieldDefinition(IJsonCustomFieldDefinition json, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new CustomFieldDefinitionContext(Id, auth);
		_board = new Field<IBoard>(_context, "Board");
		DisplayInfo = new CustomFieldDisplayInfo(_context.DisplayInfo);
		_fieldGroup = new Field<string>(_context, "FieldGroup");
		_name = new Field<string>(_context, "Name");
		_position = new Field<Position>(_context, "Position");
		_type = new Field<CustomFieldType?>(_context, "Type");
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
		_context.Merge(json);
		_context.Synchronized.Add(this);
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

	void IMergeJson<IJsonCustomFieldDefinition>.Merge(IJsonCustomFieldDefinition json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	public async Task<ICustomField<double?>> SetValueForCard(ICard card, double? value, CancellationToken ct = default(CancellationToken))
	{
		NotNullRule<ICard>.Instance.Validate(null, card);
		NullableHasValueRule<double>.Instance.Validate(null, value);
		return await _context.SetValueOnCard(card, value, ct);
	}

	public async Task<ICustomField<bool?>> SetValueForCard(ICard card, bool? value, CancellationToken ct = default(CancellationToken))
	{
		NotNullRule<ICard>.Instance.Validate(null, card);
		NullableHasValueRule<bool>.Instance.Validate(null, value);
		return await _context.SetValueOnCard(card, value, ct);
	}

	public async Task<ICustomField<string>> SetValueForCard(ICard card, string value, CancellationToken ct = default(CancellationToken))
	{
		NotNullRule<ICard>.Instance.Validate(null, card);
		NotNullRule<string>.Instance.Validate(null, value);
		return await _context.SetValueOnCard(card, value, ct);
	}

	public async Task<ICustomField<IDropDownOption>> SetValueForCard(ICard card, IDropDownOption value, CancellationToken ct = default(CancellationToken))
	{
		NotNullRule<ICard>.Instance.Validate(null, card);
		NotNullRule<IDropDownOption>.Instance.Validate(null, value);
		return await _context.SetValueOnCard(card, value, ct);
	}

	public async Task<ICustomField<DateTime?>> SetValueForCard(ICard card, DateTime? value, CancellationToken ct = default(CancellationToken))
	{
		NotNullRule<ICard>.Instance.Validate(null, card);
		NullableHasValueRule<DateTime>.Instance.Validate(null, value);
		return await _context.SetValueOnCard(card, value, ct);
	}

	public override string ToString()
	{
		return $"{Name} ({Type})";
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonCustomFieldDefinition json = TrelloConfiguration.Deserializer.Deserialize<IJsonCustomFieldDefinition>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
