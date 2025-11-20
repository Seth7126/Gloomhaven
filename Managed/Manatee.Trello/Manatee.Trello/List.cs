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

public class List : IList, ICanWebhook, ICacheable, IRefreshable, IMergeJson<IJsonList>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "name")]
		Name = 1,
		[Display(Description = "closed")]
		IsClosed = 2,
		[Display(Description = "board")]
		Board = 4,
		[Display(Description = "pos")]
		Position = 8,
		[Display(Description = "subscribed")]
		IsSubscribed = 0x10,
		Actions = 0x20,
		Cards = 0x40
	}

	private readonly Field<Board> _board;

	private readonly Field<bool?> _isArchived;

	private readonly Field<bool?> _isSubscribed;

	private readonly Field<string> _name;

	private readonly Field<Position> _position;

	private readonly ListContext _context;

	private string _id;

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
			ListContext.UpdateParameters();
		}
	}

	public IReadOnlyActionCollection Actions => _context.Actions;

	public IBoard Board
	{
		get
		{
			return _board.Value;
		}
		set
		{
			_board.Value = (Board)value;
		}
	}

	public ICardCollection Cards => _context.Cards;

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

	public string Id
	{
		get
		{
			if (!_context.HasValidId)
			{
				_context.Synchronize(force: true, CancellationToken.None).Wait();
			}
			return _id;
		}
		private set
		{
			_id = value;
		}
	}

	public bool? IsArchived
	{
		get
		{
			return _isArchived.Value;
		}
		set
		{
			_isArchived.Value = value;
		}
	}

	public bool? IsSubscribed
	{
		get
		{
			return _isSubscribed.Value;
		}
		set
		{
			_isSubscribed.Value = value;
		}
	}

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

	public ICard this[string key] => Cards[key];

	public ICard this[int index] => Cards[index];

	internal IJsonList Json
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

	public event Action<IList, IEnumerable<string>> Updated;

	static List()
	{
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
	}

	public List(string id, TrelloAuthorization auth = null)
	{
		Id = id;
		_context = new ListContext(id, auth);
		_context.Synchronized.Add(this);
		_board = new Field<Board>(_context, "Board");
		_board.AddRule(NotNullRule<Manatee.Trello.Board>.Instance);
		_isArchived = new Field<bool?>(_context, "IsArchived");
		_isArchived.AddRule(NullableHasValueRule<bool>.Instance);
		_isSubscribed = new Field<bool?>(_context, "IsSubscribed");
		_isSubscribed.AddRule(NullableHasValueRule<bool>.Instance);
		_name = new Field<string>(_context, "Name");
		_name.AddRule(NotNullOrWhiteSpaceRule.Instance);
		_position = new Field<Position>(_context, "Position");
		_position.AddRule(NotNullRule<Position>.Instance);
		_position.AddRule(PositionRule.Instance);
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	internal List(IJsonList json, TrelloAuthorization auth)
		: this(json.Id, auth)
	{
		_context.Merge(json);
	}

	public void ApplyAction(IAction action)
	{
		Action action2 = action as Action;
		ActionType? type = action.Type;
		ActionType updateList = ActionType.UpdateList;
		if (type.HasValue && (!type.HasValue || !(type.GetValueOrDefault() != updateList)) && action2?.Json?.Data?.List != null && !(action2.Json.Data.List.Id != Id))
		{
			_context.Merge(action2.Json.Data.List, overwrite: false);
		}
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonList>.Merge(IJsonList json, bool overwrite)
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
		IJsonList json = TrelloConfiguration.Deserializer.Deserialize<IJsonList>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
