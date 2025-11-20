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

public class Card : ICard, ICanWebhook, ICacheable, IQueryable, IRefreshable, IMergeJson<IJsonCard>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "badges")]
		Badges = 1,
		[Display(Description = "board")]
		Board = 2,
		[Display(Description = "checkLists")]
		Checklists = 4,
		[Display(Description = "dateLastActivity")]
		DateLastActivity = 8,
		[Display(Description = "desc")]
		Description = 0x10,
		[Display(Description = "due")]
		Due = 0x20,
		[Display(Description = "closed")]
		IsArchived = 0x40,
		[Display(Description = "dueComplete")]
		IsComplete = 0x80,
		[Display(Description = "subscribed")]
		IsSubscribed = 0x100,
		[Display(Description = "labels")]
		Labels = 0x200,
		[Display(Description = "idList")]
		List = 0x400,
		[Display(Description = "manualCoverAttachment")]
		ManualCoverAttachment = 0x800,
		[Display(Description = "name")]
		Name = 0x1000,
		[Display(Description = "pos")]
		Position = 0x2000,
		[Display(Description = "idShort")]
		ShortId = 0x4000,
		[Display(Description = "shortUrl")]
		ShortUrl = 0x8000,
		[Display(Description = "url")]
		Url = 0x10000,
		Actions = 0x20000,
		Attachments = 0x40000,
		CustomFields = 0x80000,
		Comments = 0x100000,
		Members = 0x200000,
		Stickers = 0x400000,
		VotingMembers = 0x800000
	}

	private readonly Field<Board> _board;

	private readonly Field<string> _description;

	private readonly Field<DateTime?> _dueDate;

	private readonly Field<bool?> _isArchived;

	private readonly Field<bool?> _isComplete;

	private readonly Field<bool?> _isSubscribed;

	private readonly Field<DateTime?> _lastActivity;

	private readonly Field<List> _list;

	private readonly Field<string> _name;

	private readonly Field<Position> _position;

	private readonly Field<int?> _shortId;

	private readonly Field<string> _shortUrl;

	private readonly Field<string> _url;

	private readonly CardContext _context;

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
			CardContext.UpdateParameters();
		}
	}

	public IReadOnlyActionCollection Actions => _context.Actions;

	public IAttachmentCollection Attachments => _context.Attachments;

	public IBadges Badges { get; }

	public IBoard Board => _board.Value;

	public ICheckListCollection CheckLists => _context.CheckLists;

	public ICommentCollection Comments => _context.Comments;

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

	public IReadOnlyCollection<ICustomField> CustomFields => _context.CustomFields;

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

	public DateTime? DueDate
	{
		get
		{
			return _dueDate.Value;
		}
		set
		{
			_dueDate.Value = value;
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

	public bool? IsComplete
	{
		get
		{
			return _isComplete.Value;
		}
		set
		{
			_isComplete.Value = value;
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

	public ICardLabelCollection Labels => _context.Labels;

	public DateTime? LastActivity => _lastActivity.Value;

	public IList List
	{
		get
		{
			return _list.Value;
		}
		set
		{
			_list.Value = (List)value;
			_board.Value = (Board)value.Board;
		}
	}

	public IMemberCollection Members => _context.Members;

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

	public IReadOnlyCollection<IPowerUpData> PowerUpData => _context.PowerUpData;

	public int? ShortId => _shortId.Value;

	public string ShortUrl => _shortUrl.Value;

	public ICardStickerCollection Stickers => _context.Stickers;

	public string Url => _url.Value;

	public IReadOnlyMemberCollection VotingMembers => _context.VotingMembers;

	public ICheckList this[string key] => CheckLists[key];

	public ICheckList this[int index] => CheckLists[index];

	internal IJsonCard Json
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

	public event Action<ICard, IEnumerable<string>> Updated;

	static Card()
	{
		DownloadedFields = (Fields)(Enum.GetValues(typeof(Fields)).Cast<int>().Sum() & -1048577);
	}

	public Card(string id, TrelloAuthorization auth = null)
	{
		Id = id;
		_context = new CardContext(id, auth);
		_context.Synchronized.Add(this);
		Badges = new Badges(_context.BadgesContext);
		_board = new Field<Board>(_context, "Board");
		_board.AddRule(NotNullRule<Manatee.Trello.Board>.Instance);
		_description = new Field<string>(_context, "Description");
		_dueDate = new Field<DateTime?>(_context, "DueDate");
		_isComplete = new Field<bool?>(_context, "IsComplete");
		_isArchived = new Field<bool?>(_context, "IsArchived");
		_isArchived.AddRule(NullableHasValueRule<bool>.Instance);
		_isSubscribed = new Field<bool?>(_context, "IsSubscribed");
		_isSubscribed.AddRule(NullableHasValueRule<bool>.Instance);
		_lastActivity = new Field<DateTime?>(_context, "LastActivity");
		_list = new Field<List>(_context, "List");
		_list.AddRule(NotNullRule<IList>.Instance);
		_name = new Field<string>(_context, "Name");
		_name.AddRule(NotNullOrWhiteSpaceRule.Instance);
		_position = new Field<Position>(_context, "Position");
		_position.AddRule(PositionRule.Instance);
		_shortId = new Field<int?>(_context, "ShortId");
		_shortUrl = new Field<string>(_context, "ShortUrl");
		_url = new Field<string>(_context, "Url");
		if (_context.HasValidId && auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	internal Card(IJsonCard json, TrelloAuthorization auth)
		: this(json.Id, auth)
	{
		_context.Merge(json);
	}

	public void ApplyAction(IAction action)
	{
		Action action2 = action as Action;
		ActionType? type = action.Type;
		ActionType updateCard = ActionType.UpdateCard;
		if (type.HasValue && (!type.HasValue || !(type.GetValueOrDefault() != updateCard)) && action2?.Json?.Data?.Card != null && !(action2.Json?.Data?.Card.Id != Id))
		{
			_context.Merge(action2.Json.Data.Card, overwrite: false);
		}
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

	void IMergeJson<IJsonCard>.Merge(IJsonCard json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	public override string ToString()
	{
		return Name ?? $"#{ShortId}";
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonCard json = TrelloConfiguration.Deserializer.Deserialize<IJsonCard>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		if (Id != _context.Data.Id)
		{
			TrelloConfiguration.Cache.Add(this);
			Id = _context.Data.Id;
		}
		this.Updated?.Invoke(this, properties);
	}
}
