using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class Notification : INotification, ICacheable, IRefreshable, IMergeJson<IJsonNotification>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "idMemberCreator")]
		Creator = 1,
		[Display(Description = "data")]
		Data = 2,
		[Display(Description = "unread")]
		IsUnread = 4,
		[Display(Description = "type")]
		Type = 8,
		[Display(Description = "date")]
		Date = 0x10
	}

	private static readonly Dictionary<NotificationType, Func<Notification, string>> _stringDefinitions;

	private readonly Field<Member> _creator;

	private readonly Field<DateTime?> _date;

	private readonly Field<bool?> _isUnread;

	private readonly Field<NotificationType?> _type;

	private readonly NotificationContext _context;

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
			NotificationContext.UpdateParameters();
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

	public IMember Creator => _creator.Value;

	public INotificationData Data { get; }

	public DateTime? Date => _date.Value;

	public string Id { get; private set; }

	public bool? IsUnread
	{
		get
		{
			return _isUnread.Value;
		}
		set
		{
			_isUnread.Value = value;
		}
	}

	public NotificationType? Type => _type.Value;

	internal IJsonNotification Json
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

	public event Action<INotification, IEnumerable<string>> Updated;

	static Notification()
	{
		_stringDefinitions = new Dictionary<NotificationType, Func<Notification, string>>
		{
			{
				NotificationType.AddedAttachmentToCard,
				(Notification n) => $"{n.Creator} attached {n.Data.Attachment} to card {n.Data.Card}."
			},
			{
				NotificationType.AddedToBoard,
				(Notification n) => $"{n.Creator} added you to board {n.Data.Board}."
			},
			{
				NotificationType.AddedToCard,
				(Notification n) => $"{n.Creator} assigned you to card {n.Data.Card}."
			},
			{
				NotificationType.AddedToOrganization,
				(Notification n) => $"{n.Creator} added member {n.Data.Member} to organization {n.Data.Organization}."
			},
			{
				NotificationType.AddedMemberToCard,
				(Notification n) => $"{n.Creator} assigned member {n.Data.Member} to card {n.Data.Card}."
			},
			{
				NotificationType.AddAdminToBoard,
				(Notification n) => $"{n.Creator} added member {n.Data.Member} to board {n.Data.Board} as an admin."
			},
			{
				NotificationType.AddAdminToOrganization,
				(Notification n) => $"{n.Creator} added member {n.Data.Member} to organization {n.Data.Organization} as an admin."
			},
			{
				NotificationType.ChangeCard,
				(Notification n) => $"{n.Creator} changed card {n.Data.Card}."
			},
			{
				NotificationType.CloseBoard,
				(Notification n) => $"{n.Creator} closed board {n.Data.Board}."
			},
			{
				NotificationType.CommentCard,
				(Notification n) => $"{n.Creator} commented on card #{n.Data.Card}: '{n.Data.Text}'."
			},
			{
				NotificationType.CreatedCard,
				(Notification n) => $"{n.Creator} created card {n.Data.Card} on board {n.Data.Board}."
			},
			{
				NotificationType.RemovedFromBoard,
				(Notification n) => $"{n.Creator} removed member {n.Data.Member} from board {n.Data.Board}."
			},
			{
				NotificationType.RemovedFromCard,
				(Notification n) => $"{n.Creator} removed you from card {n.Data.Card}."
			},
			{
				NotificationType.RemovedMemberFromCard,
				(Notification n) => $"{n.Creator} removed member {n.Data.Member} from card {n.Data.Card}."
			},
			{
				NotificationType.RemovedFromOrganization,
				(Notification n) => $"{n.Creator} removed member {n.Data.Member} from organization {n.Data.Organization}."
			},
			{
				NotificationType.MentionedOnCard,
				(Notification n) => $"{n.Creator} mentioned you on card {n.Data.Card}: '{n.Data.Text}'."
			},
			{
				NotificationType.UpdateCheckItemStateOnCard,
				(Notification n) => $"{n.Creator} updated checkItem {n.Data.CheckItem} on card {n.Data.Card}."
			},
			{
				NotificationType.MakeAdminOfBoard,
				(Notification n) => $"{n.Creator} made member {n.Data.Member} an admin of board {n.Data.Board}."
			},
			{
				NotificationType.MakeAdminOfOrganization,
				(Notification n) => $"{n.Creator} made member {n.Data.Member} an admin of organization {n.Data.Organization}."
			},
			{
				NotificationType.CardDueSoon,
				(Notification n) => $"Card {n.Data.Card} is due soon."
			},
			{
				NotificationType.AddAttachmentToCard,
				(Notification n) => $"{n.Creator} added an attachment to {n.Data.Card}."
			},
			{
				NotificationType.MemberJoinedTrello,
				(Notification n) => $"{n.Data.Member} joined Trello!"
			},
			{
				NotificationType.ReactionAdded,
				(Notification n) => $"{n.Data.Member} added a reaction to a comment in card {n.Data.Card}."
			},
			{
				NotificationType.ReactionRemoved,
				(Notification n) => $"{n.Data.Member} removed a reaction from a comment in card {n.Data.Card}"
			},
			{
				NotificationType.ReopenBoard,
				(Notification n) => $"{n.Data.Member} reopened board {n.Data.Board}"
			}
		};
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
	}

	public Notification(string id, TrelloAuthorization auth = null)
	{
		Id = id;
		_context = new NotificationContext(id, auth);
		_context.Synchronized.Add(this);
		_creator = new Field<Member>(_context, "Creator");
		_date = new Field<DateTime?>(_context, "Date");
		Data = new NotificationData(_context.NotificationDataContext);
		_isUnread = new Field<bool?>(_context, "IsUnread");
		_type = new Field<NotificationType?>(_context, "Type");
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	internal Notification(IJsonNotification json, TrelloAuthorization auth)
		: this(json.Id, auth)
	{
		_context.Merge(json);
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonNotification>.Merge(IJsonNotification json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonNotification json = TrelloConfiguration.Deserializer.Deserialize<IJsonNotification>(content);
		_context.Merge(json);
	}

	public override string ToString()
	{
		if (!Type.HasValue)
		{
			return "Notification type could not be determined.";
		}
		return _stringDefinitions[Type.Value](this);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
