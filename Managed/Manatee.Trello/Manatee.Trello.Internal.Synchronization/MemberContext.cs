using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class MemberContext : SynchronizationContext<IJsonMember>
{
	private readonly bool _isMe;

	private static readonly Dictionary<string, object> Parameters;

	private static readonly Member.Fields MemberFields;

	public static Dictionary<string, object> CurrentParameters
	{
		get
		{
			lock (Parameters)
			{
				if (!Parameters.Any())
				{
					GenerateParameters();
				}
				return new Dictionary<string, object>(Parameters);
			}
		}
	}

	public ReadOnlyActionCollection Actions { get; }

	public ReadOnlyBoardCollection Boards { get; }

	public ReadOnlyBoardBackgroundCollection BoardBackgrounds { get; }

	public ReadOnlyCardCollection Cards { get; }

	public ReadOnlyNotificationCollection Notifications { get; }

	public ReadOnlyOrganizationCollection Organizations { get; }

	public ReadOnlyStarredBoardCollection StarredBoards { get; }

	public MemberPreferencesContext MemberPreferencesContext { get; }

	public virtual bool HasValidId => IdRule.Instance.Validate(base.Data.Id, null) == null;

	static MemberContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Member.Fields.Bio | Member.Fields.IsConfirmed | Member.Fields.Email | Member.Fields.FullName | Member.Fields.Initials | Member.Fields.LoginTypes | Member.Fields.MemberType | Member.Fields.OneTimeMessagesDismissed | Member.Fields.Preferencess | Member.Fields.Status | Member.Fields.Trophies | Member.Fields.Url | Member.Fields.Username | Member.Fields.AvatarUrl;
		SynchronizationContext<IJsonMember>.Properties = new Dictionary<string, Property<IJsonMember>>
		{
			{
				"AvatarUrl",
				new Property<IJsonMember, string>((IJsonMember d, TrelloAuthorization a) => d.AvatarUrl, delegate(IJsonMember d, string o)
				{
					d.AvatarUrl = o;
				})
			},
			{
				"Bio",
				new Property<IJsonMember, string>((IJsonMember d, TrelloAuthorization a) => d.Bio, delegate(IJsonMember d, string o)
				{
					d.Bio = o;
				})
			},
			{
				"Email",
				new Property<IJsonMember, string>((IJsonMember d, TrelloAuthorization a) => d.Email, delegate(IJsonMember d, string o)
				{
					d.Email = o;
				})
			},
			{
				"FullName",
				new Property<IJsonMember, string>((IJsonMember d, TrelloAuthorization a) => d.FullName, delegate(IJsonMember d, string o)
				{
					d.FullName = o;
				})
			},
			{
				"Id",
				new Property<IJsonMember, string>((IJsonMember d, TrelloAuthorization a) => d.Id, delegate(IJsonMember d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Initials",
				new Property<IJsonMember, string>((IJsonMember d, TrelloAuthorization a) => d.Initials, delegate(IJsonMember d, string o)
				{
					d.Initials = o;
				})
			},
			{
				"IsConfirmed",
				new Property<IJsonMember, bool?>((IJsonMember d, TrelloAuthorization a) => d.Confirmed, delegate(IJsonMember d, bool? o)
				{
					d.Confirmed = o;
				})
			},
			{
				"Preferences",
				new Property<IJsonMember, IJsonMemberPreferences>((IJsonMember d, TrelloAuthorization a) => d.Prefs, delegate(IJsonMember d, IJsonMemberPreferences o)
				{
					d.Prefs = o;
				})
			},
			{
				"Status",
				new Property<IJsonMember, MemberStatus?>((IJsonMember d, TrelloAuthorization a) => d.Status, delegate(IJsonMember d, MemberStatus? o)
				{
					d.Status = o;
				})
			},
			{
				"Trophies",
				new Property<IJsonMember, List<string>>((IJsonMember d, TrelloAuthorization a) => d.Trophies, delegate(IJsonMember d, List<string> o)
				{
					d.Trophies = o?.ToList();
				})
			},
			{
				"Url",
				new Property<IJsonMember, string>((IJsonMember d, TrelloAuthorization a) => d.Url, delegate(IJsonMember d, string o)
				{
					d.Url = o;
				})
			},
			{
				"UserName",
				new Property<IJsonMember, string>((IJsonMember d, TrelloAuthorization a) => d.Username, delegate(IJsonMember d, string o)
				{
					d.Username = o;
				})
			}
		};
	}

	public MemberContext(string id, bool isMe, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_isMe = isMe;
		base.Data.Id = id;
		Actions = new ReadOnlyActionCollection(typeof(Member), () => base.Data.Id, auth);
		if (isMe)
		{
			Boards = new BoardCollection(typeof(Member), () => base.Data.Id, auth);
			BoardBackgrounds = new BoardBackgroundCollection(() => base.Data.Id, auth);
			Organizations = new OrganizationCollection(() => base.Data.Id, auth);
			StarredBoards = new StarredBoardCollection(() => base.Data.Id, auth);
		}
		else
		{
			Boards = new ReadOnlyBoardCollection(typeof(Member), () => base.Data.Id, auth);
			BoardBackgrounds = new ReadOnlyBoardBackgroundCollection(() => base.Data.Id, auth);
			Organizations = new ReadOnlyOrganizationCollection(() => base.Data.Id, auth);
			StarredBoards = new ReadOnlyStarredBoardCollection(() => base.Data.Id, auth);
		}
		Boards.Refreshed += delegate
		{
			OnMerged(new List<string> { "Boards" });
		};
		BoardBackgrounds.Refreshed += delegate
		{
			OnMerged(new List<string> { "BoardBackgrounds" });
		};
		Organizations.Refreshed += delegate
		{
			OnMerged(new List<string> { "Organizations" });
		};
		StarredBoards.Refreshed += delegate
		{
			OnMerged(new List<string> { "StarredBoards" });
		};
		Cards = new ReadOnlyCardCollection(EntityRequestType.Member_Read_Cards, () => base.Data.Id, auth);
		Cards.Refreshed += delegate
		{
			OnMerged(new List<string> { "Cards" });
		};
		Notifications = new ReadOnlyNotificationCollection(() => base.Data.Id, auth);
		Notifications.Refreshed += delegate
		{
			OnMerged(new List<string> { "Notifications" });
		};
		MemberPreferencesContext = new MemberPreferencesContext(base.Auth);
		MemberPreferencesContext memberPreferencesContext = MemberPreferencesContext;
		memberPreferencesContext.SubmitRequested = (Func<CancellationToken, Task>)Delegate.Combine(memberPreferencesContext.SubmitRequested, (Func<CancellationToken, Task>)((CancellationToken ct) => HandleSubmitRequested("Preferences", ct)));
		base.Data.Prefs = MemberPreferencesContext.Data;
	}

	public static void UpdateParameters()
	{
		lock (Parameters)
		{
			Parameters.Clear();
		}
	}

	private static void GenerateParameters()
	{
		lock (Parameters)
		{
			Parameters.Clear();
			int num = Enum.GetValues(typeof(Member.Fields)).Cast<Member.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Member.Fields enumerationValue = (Member.Fields)((int)((uint)num & (uint)MemberFields) & (int)Member.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			Member.Fields fields = (Member.Fields)((int)((uint)num & (uint)Member.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(Member.Fields.Actions))
			{
				Parameters["actions"] = "all";
				Parameters["actions_format"] = "list";
			}
			if (fields.HasFlag(Member.Fields.Boards))
			{
				Parameters["boards"] = "all";
				Parameters["board_fields"] = BoardContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Member.Fields.Cards))
			{
				Parameters["cards"] = "all";
				Parameters["card_fields"] = CardContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Member.Fields.Organizations))
			{
				Parameters["organizations"] = "all";
				Parameters["organization_fields"] = OrganizationContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Member_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override Dictionary<string, object> GetParameters()
	{
		if (_isMe)
		{
			Dictionary<string, object> dictionary;
			lock (Parameters)
			{
				dictionary = Parameters.ToDictionary((KeyValuePair<string, object> x) => x.Key, (KeyValuePair<string, object> x) => x.Value);
			}
			if (Member.DownloadedFields.HasFlag(Member.Fields.Notifications))
			{
				dictionary["notifications"] = "all";
				dictionary["notification_fields"] = NotificationContext.CurrentParameters["fields"];
			}
			if (Member.DownloadedFields.HasFlag(Member.Fields.StarredBoards))
			{
				dictionary["boardStars"] = "true";
			}
			if (Member.DownloadedFields.HasFlag(Member.Fields.BoardBackgrounds))
			{
				dictionary["boardBackgrounds"] = "custom";
			}
			return dictionary;
		}
		return CurrentParameters;
	}

	protected override async Task SubmitData(IJsonMember json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}

	protected override void ApplyDependentChanges(IJsonMember json)
	{
		if (MemberPreferencesContext.HasChanges)
		{
			json.Prefs = MemberPreferencesContext.GetChanges();
			MemberPreferencesContext.ClearChanges();
		}
	}

	protected override IEnumerable<string> MergeDependencies(IJsonMember json, bool overwrite)
	{
		List<string> list = MemberPreferencesContext.Merge(json.Prefs, overwrite).ToList();
		if (json.Actions != null)
		{
			Actions.Update(json.Actions.Select((IJsonAction a) => a.GetFromCache<Action, IJsonAction>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Actions");
		}
		if (json.Boards != null)
		{
			Boards.Update(json.Boards.Select((IJsonBoard a) => a.GetFromCache<Board, IJsonBoard>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Boards");
		}
		if (json.Cards != null)
		{
			Cards.Update(json.Cards.Select((IJsonCard a) => a.GetFromCache<Card, IJsonCard>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Cards");
		}
		if (json.Notifications != null)
		{
			Notifications.Update(json.Notifications.Select((IJsonNotification a) => a.GetFromCache<Notification, IJsonNotification>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Notifications");
		}
		if (json.Organizations != null)
		{
			Organizations.Update(json.Organizations.Select((IJsonOrganization a) => a.GetFromCache<Organization, IJsonOrganization>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Organizations");
		}
		if (json.StarredBoards != null)
		{
			StarredBoards.Update(json.StarredBoards.Select((IJsonStarredBoard a) => a.GetFromCache<StarredBoard, IJsonStarredBoard>(base.Auth, overwrite, new object[1] { base.Data.Id })));
			list.Add("StarredBoards");
		}
		return list;
	}
}
