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

public class Board : IBoard, ICanWebhook, ICacheable, IQueryable, IRefreshable, IMergeJson<IJsonBoard>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "name")]
		Name = 1,
		[Display(Description = "desc")]
		Description = 2,
		[Display(Description = "closed")]
		Closed = 4,
		[Display(Description = "organization")]
		Organization = 8,
		[Display(Description = "pinned")]
		Pinned = 0x10,
		[Display(Description = "starred")]
		Starred = 0x20,
		[Display(Description = "prefs")]
		Preferencess = 0x40,
		[Display(Description = "url")]
		Url = 0x80,
		[Display(Description = "subscribed")]
		IsSubscribed = 0x100,
		[Display(Description = "dateLastActivity")]
		LastActivityDate = 0x200,
		[Display(Description = "dateLastView")]
		LastViewDate = 0x400,
		[Display(Description = "shortLink")]
		ShortLink = 0x800,
		[Display(Description = "shortUrl")]
		ShortUrl = 0x1000,
		Lists = 0x2000,
		Members = 0x4000,
		CustomFields = 0x8000,
		Labels = 0x10000,
		Memberships = 0x20000,
		Actions = 0x40000,
		Cards = 0x80000,
		PowerUps = 0x100000,
		PowerUpData = 0x200000
	}

	private readonly Field<string> _description;

	private readonly Field<bool?> _isClosed;

	private readonly Field<bool?> _isPinned;

	private readonly Field<bool?> _isStarred;

	private readonly Field<bool?> _isSubscribed;

	private readonly Field<string> _name;

	private readonly Field<Organization> _organization;

	private readonly Field<string> _url;

	private readonly Field<DateTime?> _lastActivity;

	private readonly Field<DateTime?> _lastViewed;

	private readonly Field<string> _shortLink;

	private readonly Field<string> _shortUrl;

	private readonly BoardContext _context;

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
			BoardContext.UpdateParameters();
		}
	}

	public IReadOnlyActionCollection Actions => _context.Actions;

	public IReadOnlyCardCollection Cards => _context.Cards;

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

	public ICustomFieldDefinitionCollection CustomFields => _context.CustomFields;

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

	public bool? IsClosed
	{
		get
		{
			return _isClosed.Value;
		}
		set
		{
			_isClosed.Value = value;
		}
	}

	public bool? IsPinned => _isPinned.Value;

	public bool? IsStarred => _isStarred.Value;

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

	public IBoardLabelCollection Labels => _context.Labels;

	public DateTime? LastActivity => _lastActivity.Value;

	public DateTime? LastViewed => _lastViewed.Value;

	public IListCollection Lists => _context.Lists;

	public IReadOnlyMemberCollection Members => _context.Members;

	public IBoardMembershipCollection Memberships => _context.Memberships;

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

	public IOrganization Organization
	{
		get
		{
			return _organization.Value;
		}
		set
		{
			_organization.Value = (Organization)value;
		}
	}

	public IPowerUpCollection PowerUps => _context.PowerUps;

	public IReadOnlyCollection<IPowerUpData> PowerUpData => _context.PowerUpData;

	public IBoardPreferences Preferences { get; }

	public IBoardPersonalPreferences PersonalPreferences { get; }

	public string ShortLink => _shortLink.Value;

	public string ShortUrl => _shortUrl.Value;

	public string Url => _url.Value;

	public IList this[string key] => Lists[key];

	public IList this[int index] => Lists[index];

	internal IJsonBoard Json
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

	internal TrelloAuthorization Auth => _context.Auth;

	TrelloAuthorization IBatchRefresh.Auth => _context.Auth;

	public event Action<IBoard, IEnumerable<string>> Updated;

	static Board()
	{
		DownloadedFields = (Fields)(Enum.GetValues(typeof(Fields)).Cast<int>().Sum() & -16385 & -524289);
	}

	public Board(string id, TrelloAuthorization auth = null)
	{
		_context = new BoardContext(id, auth);
		_context.Synchronized.Add(this);
		Id = id;
		_description = new Field<string>(_context, "Description");
		_isClosed = new Field<bool?>(_context, "IsClosed");
		_isClosed.AddRule(NullableHasValueRule<bool>.Instance);
		_isPinned = new Field<bool?>(_context, "IsPinned");
		_isPinned.AddRule(NullableHasValueRule<bool>.Instance);
		_isStarred = new Field<bool?>(_context, "IsStarred");
		_isStarred.AddRule(NullableHasValueRule<bool>.Instance);
		_isSubscribed = new Field<bool?>(_context, "IsSubscribed");
		_isSubscribed.AddRule(NullableHasValueRule<bool>.Instance);
		_name = new Field<string>(_context, "Name");
		_name.AddRule(NotNullOrWhiteSpaceRule.Instance);
		_organization = new Field<Organization>(_context, "Organization");
		Preferences = new BoardPreferences(_context.BoardPreferencesContext);
		PersonalPreferences = new BoardPersonalPreferences(() => Id, auth);
		_url = new Field<string>(_context, "Url");
		_shortUrl = new Field<string>(_context, "ShortUrl");
		_shortLink = new Field<string>(_context, "ShortLink");
		_lastActivity = new Field<DateTime?>(_context, "LastActivity");
		_lastViewed = new Field<DateTime?>(_context, "LastViewed");
		if (_context.HasValidId && auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	internal Board(IJsonBoard json, TrelloAuthorization auth)
		: this(json.Id, auth)
	{
		_context.Merge(json);
	}

	public void ApplyAction(IAction action)
	{
		Action action2 = action as Action;
		ActionType? type = action.Type;
		ActionType updateBoard = ActionType.UpdateBoard;
		if (type.HasValue && (!type.HasValue || !(type.GetValueOrDefault() != updateBoard)) && action2?.Json?.Data?.Board != null && !(action2.Json.Data.Board.Id != Id))
		{
			_context.Merge(action2.Json.Data.Board, overwrite: false);
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

	void IMergeJson<IJsonBoard>.Merge(IJsonBoard json, bool overwrite)
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
		IJsonBoard json = TrelloConfiguration.Deserializer.Deserialize<IJsonBoard>(content);
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
