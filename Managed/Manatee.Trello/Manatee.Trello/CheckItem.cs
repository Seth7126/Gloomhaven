using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CheckItem : ICheckItem, ICacheable, IRefreshable, IMergeJson<IJsonCheckItem>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "state")]
		State = 1,
		[Display(Description = "name")]
		Name = 2,
		[Display(Description = "pos")]
		Position = 4
	}

	private readonly Field<CheckList> _checkList;

	private readonly Field<string> _name;

	private readonly Field<Position> _position;

	private readonly Field<CheckItemState?> _state;

	private readonly CheckItemContext _context;

	private DateTime? _creation;

	private static Fields _downloadedFields;

	public static Fields DownloadedFields
	{
		get
		{
			return _downloadedFields;
		}
		set
		{
			_downloadedFields = value;
			CheckItemContext.UpdateParameters();
		}
	}

	public ICheckList CheckList
	{
		get
		{
			return _checkList.Value;
		}
		set
		{
			_checkList.Value = (CheckList)value;
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

	public CheckItemState? State
	{
		get
		{
			return _state.Value;
		}
		set
		{
			_state.Value = value;
		}
	}

	internal IJsonCheckItem Json
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

	public event Action<ICheckItem, IEnumerable<string>> Updated;

	static CheckItem()
	{
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
	}

	internal CheckItem(IJsonCheckItem json, string checkListId, TrelloAuthorization auth = null)
	{
		Id = json.Id;
		_context = new CheckItemContext(Id, checkListId, auth);
		_checkList = new Field<CheckList>(_context, "CheckList");
		_checkList.AddRule(NotNullRule<Manatee.Trello.CheckList>.Instance);
		_name = new Field<string>(_context, "Name");
		_name.AddRule(NotNullOrWhiteSpaceRule.Instance);
		_position = new Field<Position>(_context, "Position");
		_position.AddRule(NotNullRule<Position>.Instance);
		_position.AddRule(PositionRule.Instance);
		_state = new Field<CheckItemState?>(_context, "State");
		_state.AddRule(NullableHasValueRule<CheckItemState>.Instance);
		_state.AddRule(EnumerationRule<CheckItemState?>.Instance);
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

	void IMergeJson<IJsonCheckItem>.Merge(IJsonCheckItem json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	public override string ToString()
	{
		return Name;
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonCheckItem json = TrelloConfiguration.Deserializer.Deserialize<IJsonCheckItem>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
