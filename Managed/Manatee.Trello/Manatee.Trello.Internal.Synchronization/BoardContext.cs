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

internal class BoardContext : DeletableSynchronizationContext<IJsonBoard>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Board.Fields MemberFields;

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

	public ReadOnlyCardCollection Cards { get; }

	public CustomFieldDefinitionCollection CustomFields { get; }

	public BoardLabelCollection Labels { get; }

	public ListCollection Lists { get; }

	public ReadOnlyMemberCollection Members { get; }

	public BoardMembershipCollection Memberships { get; }

	public PowerUpCollection PowerUps { get; }

	public ReadOnlyPowerUpDataCollection PowerUpData { get; }

	public BoardPreferencesContext BoardPreferencesContext { get; }

	public virtual bool HasValidId => IdRule.Instance.Validate(base.Data.Id, null) == null;

	static BoardContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Board.Fields.Name | Board.Fields.Description | Board.Fields.Closed | Board.Fields.Pinned | Board.Fields.Starred | Board.Fields.Preferencess | Board.Fields.Url | Board.Fields.IsSubscribed | Board.Fields.LastActivityDate | Board.Fields.LastViewDate | Board.Fields.ShortLink | Board.Fields.ShortUrl;
		SynchronizationContext<IJsonBoard>.Properties = new Dictionary<string, Property<IJsonBoard>>
		{
			{
				"Description",
				new Property<IJsonBoard, string>((IJsonBoard d, TrelloAuthorization a) => d.Desc, delegate(IJsonBoard d, string o)
				{
					d.Desc = o;
				})
			},
			{
				"Id",
				new Property<IJsonBoard, string>((IJsonBoard d, TrelloAuthorization a) => d.Id, delegate(IJsonBoard d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsClosed",
				new Property<IJsonBoard, bool?>((IJsonBoard d, TrelloAuthorization a) => d.Closed, delegate(IJsonBoard d, bool? o)
				{
					d.Closed = o;
				})
			},
			{
				"IsSubscribed",
				new Property<IJsonBoard, bool?>((IJsonBoard d, TrelloAuthorization a) => d.Subscribed, delegate(IJsonBoard d, bool? o)
				{
					d.Subscribed = o;
				})
			},
			{
				"Name",
				new Property<IJsonBoard, string>((IJsonBoard d, TrelloAuthorization a) => d.Name, delegate(IJsonBoard d, string o)
				{
					d.Name = o;
				})
			},
			{
				"Organization",
				new Property<IJsonBoard, Organization>((IJsonBoard d, TrelloAuthorization a) => d.Organization?.GetFromCache<Organization, IJsonOrganization>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonBoard d, Organization o)
				{
					d.Organization = o?.Json;
				})
			},
			{
				"Preferences",
				new Property<IJsonBoard, IJsonBoardPreferences>((IJsonBoard d, TrelloAuthorization a) => d.Prefs, delegate(IJsonBoard d, IJsonBoardPreferences o)
				{
					d.Prefs = o;
				})
			},
			{
				"Url",
				new Property<IJsonBoard, string>((IJsonBoard d, TrelloAuthorization a) => d.Url, delegate(IJsonBoard d, string o)
				{
					d.Url = o;
				})
			},
			{
				"IsPinned",
				new Property<IJsonBoard, bool?>((IJsonBoard d, TrelloAuthorization a) => d.Pinned, delegate(IJsonBoard d, bool? o)
				{
					d.Pinned = o;
				})
			},
			{
				"IsStarred",
				new Property<IJsonBoard, bool?>((IJsonBoard d, TrelloAuthorization a) => d.Starred, delegate(IJsonBoard d, bool? o)
				{
					d.Starred = o;
				})
			},
			{
				"LastViewed",
				new Property<IJsonBoard, DateTime?>((IJsonBoard d, TrelloAuthorization a) => d.DateLastView, delegate(IJsonBoard d, DateTime? o)
				{
					d.DateLastView = o;
				})
			},
			{
				"LastActivity",
				new Property<IJsonBoard, DateTime?>((IJsonBoard d, TrelloAuthorization a) => d.DateLastActivity, delegate(IJsonBoard d, DateTime? o)
				{
					d.DateLastActivity = o;
				})
			},
			{
				"ShortUrl",
				new Property<IJsonBoard, string>((IJsonBoard d, TrelloAuthorization a) => d.ShortUrl, delegate(IJsonBoard d, string o)
				{
					d.ShortUrl = o;
				})
			},
			{
				"ShortLink",
				new Property<IJsonBoard, string>((IJsonBoard d, TrelloAuthorization a) => d.ShortLink, delegate(IJsonBoard d, string o)
				{
					d.ShortLink = o;
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonBoard, bool>((IJsonBoard d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonBoard d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public BoardContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
		Actions = new ReadOnlyActionCollection(typeof(Board), () => base.Data.Id, auth);
		Actions.Refreshed += delegate
		{
			OnMerged(new List<string> { "Actions" });
		};
		Cards = new ReadOnlyCardCollection(typeof(Board), () => base.Data.Id, auth);
		Cards.Refreshed += delegate
		{
			OnMerged(new List<string> { "Cards" });
		};
		CustomFields = new CustomFieldDefinitionCollection(() => base.Data.Id, auth);
		CustomFields.Refreshed += delegate
		{
			OnMerged(new List<string> { "CustomFields" });
		};
		Labels = new BoardLabelCollection(() => base.Data.Id, auth);
		Labels.Refreshed += delegate
		{
			OnMerged(new List<string> { "Labels" });
		};
		Lists = new ListCollection(() => base.Data.Id, auth);
		Lists.Refreshed += delegate
		{
			OnMerged(new List<string> { "Lists" });
		};
		Members = new ReadOnlyMemberCollection(EntityRequestType.Board_Read_Members, () => base.Data.Id, auth);
		Members.Refreshed += delegate
		{
			OnMerged(new List<string> { "Members" });
		};
		Memberships = new BoardMembershipCollection(() => base.Data.Id, auth);
		Memberships.Refreshed += delegate
		{
			OnMerged(new List<string> { "Memberships" });
		};
		PowerUps = new PowerUpCollection(() => base.Data.Id, auth);
		PowerUps.Refreshed += delegate
		{
			OnMerged(new List<string> { "PowerUps" });
		};
		PowerUpData = new ReadOnlyPowerUpDataCollection(EntityRequestType.Board_Read_PowerUpData, () => base.Data.Id, auth);
		PowerUpData.Refreshed += delegate
		{
			OnMerged(new List<string> { "PowerUpData" });
		};
		BoardPreferencesContext = new BoardPreferencesContext(base.Auth);
		BoardPreferencesContext boardPreferencesContext = BoardPreferencesContext;
		boardPreferencesContext.SubmitRequested = (Func<CancellationToken, Task>)Delegate.Combine(boardPreferencesContext.SubmitRequested, (Func<CancellationToken, Task>)((CancellationToken ct) => HandleSubmitRequested("Preferences", ct)));
		base.Data.Prefs = BoardPreferencesContext.Data;
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
			Board.Fields fields = (Board.Fields)Enum.GetValues(typeof(Board.Fields)).Cast<Board.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Board.Fields enumerationValue = fields & MemberFields & Board.DownloadedFields;
			Parameters["fields"] = enumerationValue.GetDescription();
			if (!TrelloConfiguration.EnableDeepDownloads)
			{
				return;
			}
			Board.Fields fields2 = fields & Board.DownloadedFields & ~MemberFields;
			if (fields2.HasFlag(Board.Fields.Actions))
			{
				Parameters["actions"] = "all";
				Parameters["actions_format"] = "list";
			}
			if (fields2.HasFlag(Board.Fields.Cards))
			{
				Parameters["cards"] = "open";
				object obj = CardContext.CurrentParameters["fields"];
				if (Board.DownloadedFields.HasFlag(Board.Fields.Lists) || Card.DownloadedFields.HasFlag(Card.Fields.List))
				{
					obj = obj?.ToString() + ",idList";
				}
				Parameters["card_fields"] = obj;
				if (Card.DownloadedFields.HasFlag(Card.Fields.Members))
				{
					Parameters["card_members"] = "true";
				}
				if (Card.DownloadedFields.HasFlag(Card.Fields.Attachments))
				{
					Parameters["card_attachments"] = "true";
				}
				if (Card.DownloadedFields.HasFlag(Card.Fields.Stickers))
				{
					Parameters["card_stickers"] = "true";
				}
				if (Card.DownloadedFields.HasFlag(Card.Fields.CustomFields))
				{
					Parameters["card_customFieldItems"] = "true";
				}
			}
			if (fields2.HasFlag(Board.Fields.CustomFields))
			{
				Parameters["customFields"] = "true";
			}
			if (fields2.HasFlag(Board.Fields.Labels))
			{
				Parameters["labels"] = "all";
				Parameters["label_fields"] = LabelContext.CurrentParameters["fields"];
			}
			if (fields2.HasFlag(Board.Fields.Lists))
			{
				Parameters["lists"] = "open";
				Parameters["list_fields"] = ListContext.CurrentParameters["fields"];
			}
			if (fields2.HasFlag(Board.Fields.Members))
			{
				Parameters["members"] = "all";
				Parameters["member_fields"] = MemberContext.CurrentParameters["fields"];
			}
			if (fields2.HasFlag(Board.Fields.Memberships))
			{
				Parameters["memberships"] = "all";
				Parameters["memberships_member"] = "true";
				Parameters["membership_member_fields"] = MemberContext.CurrentParameters["fields"];
			}
			if (fields2.HasFlag(Board.Fields.PowerUps))
			{
				Parameters["plugins"] = "enabled";
			}
			if (fields2.HasFlag(Board.Fields.PowerUpData))
			{
				Parameters["pluginData"] = "true";
			}
			if (fields2.HasFlag(Board.Fields.Organization))
			{
				Parameters["organization"] = "true";
				Parameters["organization_fields"] = OrganizationContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Board_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override Dictionary<string, object> GetParameters()
	{
		return CurrentParameters;
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Board_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task SubmitData(IJsonBoard json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
		base.Data.Prefs = BoardPreferencesContext.Data;
	}

	protected override void ApplyDependentChanges(IJsonBoard json)
	{
		base.Data.Prefs = BoardPreferencesContext.Data;
		if (BoardPreferencesContext.HasChanges)
		{
			json.Prefs = BoardPreferencesContext.GetChanges();
			BoardPreferencesContext.ClearChanges();
		}
	}

	protected override IEnumerable<string> MergeDependencies(IJsonBoard json, bool overwrite)
	{
		List<string> list = (from p in BoardPreferencesContext.Merge(json.Prefs, overwrite)
			select "Preferences." + p).ToList();
		if (json.Actions != null)
		{
			Actions.Update(json.Actions.Select((IJsonAction a) => a.GetFromCache<Action, IJsonAction>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Actions");
		}
		if (json.Cards != null)
		{
			Cards.Update(json.Cards.Select((IJsonCard a) => a.GetFromCache<Card, IJsonCard>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Cards");
		}
		if (json.CustomFields != null)
		{
			CustomFields.Update(json.CustomFields.Select((IJsonCustomFieldDefinition a) => a.GetFromCache<CustomFieldDefinition, IJsonCustomFieldDefinition>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("CustomFields");
		}
		if (json.Labels != null)
		{
			Labels.Update(json.Labels.Select((IJsonLabel a) => a.GetFromCache<Label, IJsonLabel>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Labels");
		}
		if (json.Lists != null)
		{
			Lists.Update(json.Lists.Select((IJsonList a) => a.GetFromCache<List, IJsonList>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Lists");
		}
		if (json.Members != null)
		{
			Members.Update(json.Members.Select((IJsonMember a) => a.GetFromCache<Member, IJsonMember>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Members");
		}
		if (json.Memberships != null)
		{
			Memberships.Update(json.Memberships.Select((IJsonBoardMembership a) => a.TryGetFromCache<BoardMembership, IJsonBoardMembership>(overwrite) ?? new BoardMembership(a, base.Data.Id, base.Auth)));
			list.Add("Memberships");
		}
		if (json.PowerUps != null)
		{
			PowerUps.Update(json.PowerUps.Select((IJsonPowerUp a) => a.GetFromCache<IPowerUp>(base.Auth)));
			list.Add("PowerUps");
		}
		if (json.PowerUpData != null)
		{
			PowerUpData.Update(json.PowerUpData.Select((IJsonPowerUpData a) => a.GetFromCache<PowerUpData, IJsonPowerUpData>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("PowerUpData");
		}
		return list;
	}
}
