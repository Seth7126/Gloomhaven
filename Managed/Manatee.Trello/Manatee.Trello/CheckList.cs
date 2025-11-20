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

public class CheckList : ICheckList, ICacheable, IRefreshable, IMergeJson<IJsonCheckList>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "name")]
		Name = 1,
		[Display(Description = "idBoard")]
		Board = 2,
		[Display(Description = "idCard")]
		Card = 4,
		[Display(Description = "checkItems")]
		CheckItems = 8,
		[Display(Description = "pos")]
		Position = 0x10
	}

	private readonly Field<Board> _board;

	private readonly Field<Card> _card;

	private readonly Field<string> _name;

	private readonly Field<Position> _position;

	private readonly CheckListContext _context;

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
			CheckListContext.UpdateParameters();
		}
	}

	public IBoard Board => _board.Value;

	public ICard Card
	{
		get
		{
			return _card.Value;
		}
		private set
		{
			_card.Value = (Card)value;
		}
	}

	public ICheckItemCollection CheckItems => _context.CheckItems;

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

	public ICheckItem this[string key] => CheckItems[key];

	public ICheckItem this[int index] => CheckItems[index];

	internal IJsonCheckList Json
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

	public event Action<ICheckList, IEnumerable<string>> Updated;

	static CheckList()
	{
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
	}

	public CheckList(string id, TrelloAuthorization auth = null)
	{
		Id = id;
		_context = new CheckListContext(id, auth);
		_context.Synchronized.Add(this);
		_board = new Field<Board>(_context, "Board");
		_card = new Field<Card>(_context, "Card");
		_card.AddRule(NotNullRule<Manatee.Trello.Card>.Instance);
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

	internal CheckList(IJsonCheckList json, TrelloAuthorization auth)
		: this(json.Id, auth)
	{
		_context.Merge(json);
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

	void IMergeJson<IJsonCheckList>.Merge(IJsonCheckList json, bool overwrite)
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
		IJsonCheckList json = TrelloConfiguration.Deserializer.Deserialize<IJsonCheckList>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
