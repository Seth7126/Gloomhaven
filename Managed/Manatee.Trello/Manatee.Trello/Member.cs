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

public class Member : IMember, ICanWebhook, ICacheable, IRefreshable, IMergeJson<IJsonMember>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "avatarHash")]
		[Obsolete("Trello has deprecated this property.")]
		AvatarHash = 1,
		[Display(Description = "avatarSource")]
		[Obsolete("Trello has deprecated this property.")]
		AvatarSource = 2,
		[Display(Description = "bio")]
		Bio = 4,
		[Display(Description = "confirmed")]
		IsConfirmed = 8,
		[Display(Description = "email")]
		Email = 0x10,
		[Display(Description = "fullName")]
		FullName = 0x20,
		[Display(Description = "gravatarHash")]
		[Obsolete("Trello has deprecated this property.")]
		GravatarHash = 0x40,
		[Display(Description = "initials")]
		Initials = 0x80,
		[Display(Description = "loginTypes")]
		LoginTypes = 0x100,
		[Display(Description = "memberType")]
		MemberType = 0x200,
		[Display(Description = "oneTimeMessagesReceived")]
		OneTimeMessagesDismissed = 0x400,
		[Display(Description = "prefs")]
		Preferencess = 0x800,
		[Display(Description = "status")]
		Status = 0x2000,
		[Display(Description = "trophies")]
		Trophies = 0x4000,
		[Display(Description = "uploadedAvatarHash")]
		[Obsolete("Trello has deprecated this property.")]
		UploadedAvatarHash = 0x8000,
		[Display(Description = "url")]
		Url = 0x10000,
		[Display(Description = "username")]
		Username = 0x20000,
		Actions = 0x40000,
		Boards = 0x80000,
		Organizations = 0x100000,
		Cards = 0x200000,
		Notifications = 0x400000,
		[Display(Description = "avatarUrl")]
		AvatarUrl = 0x800000,
		StarredBoards = 0x1000000,
		BoardBackgrounds = 0x2000000
	}

	private readonly Field<string> _avatarUrl;

	private readonly Field<string> _bio;

	private readonly Field<string> _fullName;

	private readonly Field<string> _initials;

	private readonly Field<bool?> _isConfirmed;

	private readonly Field<MemberStatus?> _status;

	private readonly Field<IEnumerable<string>> _trophies;

	private readonly Field<string> _url;

	private readonly Field<string> _userName;

	internal readonly MemberContext _context;

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
			MemberContext.UpdateParameters();
		}
	}

	public static AvatarSize AvatarSize { get; set; }

	public IReadOnlyActionCollection Actions => _context.Actions;

	[Obsolete("Trello has deprecated this property.")]
	public AvatarSource? AvatarSource => null;

	public string AvatarUrl => GetAvatar();

	public string Bio
	{
		get
		{
			return _bio.Value;
		}
		internal set
		{
			_bio.Value = value;
		}
	}

	public IReadOnlyBoardCollection Boards => _context.Boards;

	public IReadOnlyCollection<IBoardBackground> BoardBackgrounds => _context.BoardBackgrounds;

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

	public string FullName
	{
		get
		{
			return _fullName.Value;
		}
		internal set
		{
			_fullName.Value = value;
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

	public string Initials
	{
		get
		{
			return _initials.Value;
		}
		internal set
		{
			_initials.Value = value;
		}
	}

	public bool? IsConfirmed => _isConfirmed.Value;

	public string Mention => "@" + UserName;

	public IReadOnlyOrganizationCollection Organizations => _context.Organizations;

	public IReadOnlyCollection<IStarredBoard> StarredBoards => _context.StarredBoards;

	public MemberStatus? Status => _status.Value;

	public IEnumerable<string> Trophies => _trophies.Value;

	public string Url => _url.Value;

	public string UserName
	{
		get
		{
			return _userName.Value;
		}
		internal set
		{
			_userName.Value = value;
		}
	}

	internal IJsonMember Json
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

	public event Action<IMember, IEnumerable<string>> Updated;

	static Member()
	{
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
		AvatarSize = AvatarSize.Large;
	}

	public Member(string id, TrelloAuthorization auth = null)
		: this(id, isMe: false, auth)
	{
	}

	internal Member(string id, bool isMe, TrelloAuthorization auth)
	{
		Id = id;
		_context = new MemberContext(id, isMe, auth);
		_context.Synchronized.Add(this);
		_avatarUrl = new Field<string>(_context, "AvatarUrl");
		_bio = new Field<string>(_context, "Bio");
		_fullName = new Field<string>(_context, "FullName");
		_fullName.AddRule(MemberFullNameRule.Instance);
		_initials = new Field<string>(_context, "Initials");
		_initials.AddRule(MemberInitialsRule.Instance);
		_isConfirmed = new Field<bool?>(_context, "IsConfirmed");
		_status = new Field<MemberStatus?>(_context, "Status");
		_trophies = new Field<IEnumerable<string>>(_context, "Trophies");
		_url = new Field<string>(_context, "Url");
		_userName = new Field<string>(_context, "UserName");
		_userName.AddRule(UsernameRule.Instance);
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	internal Member(IJsonMember json, TrelloAuthorization auth)
		: this(json.Id, isMe: false, auth)
	{
		_context.Merge(json);
	}

	public void ApplyAction(IAction action)
	{
		Action action2 = action as Action;
		ActionType? type = action.Type;
		ActionType updateMember = ActionType.UpdateMember;
		if (type.HasValue && (!type.HasValue || !(type.GetValueOrDefault() != updateMember)) && action2?.Json?.Data?.Member != null && !(action2.Data.Member.Id != Id))
		{
			_context.Merge(action2.Json.Data.Member);
		}
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	public override string ToString()
	{
		return FullName;
	}

	void IMergeJson<IJsonMember>.Merge(IJsonMember json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonMember json = TrelloConfiguration.Deserializer.Deserialize<IJsonMember>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}

	private string GetAvatar()
	{
		string value = _avatarUrl.Value;
		if (!value.IsNullOrWhiteSpace())
		{
			return $"{value}/{(int)AvatarSize}.png";
		}
		return null;
	}
}
