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

internal class CardContext : DeletableSynchronizationContext<IJsonCard>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Card.Fields MemberFields;

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

	public AttachmentCollection Attachments { get; }

	public BadgesContext BadgesContext { get; }

	public CheckListCollection CheckLists { get; }

	public CommentCollection Comments { get; }

	public ReadOnlyCustomFieldCollection CustomFields { get; }

	public CardLabelCollection Labels { get; }

	public MemberCollection Members { get; }

	public ReadOnlyPowerUpDataCollection PowerUpData { get; }

	public CardStickerCollection Stickers { get; }

	public ReadOnlyMemberCollection VotingMembers { get; }

	public virtual bool HasValidId => IdRule.Instance.Validate(base.Data.Id, null) == null;

	static CardContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Card.Fields.Badges | Card.Fields.DateLastActivity | Card.Fields.Description | Card.Fields.Due | Card.Fields.IsArchived | Card.Fields.IsComplete | Card.Fields.IsSubscribed | Card.Fields.Labels | Card.Fields.List | Card.Fields.ManualCoverAttachment | Card.Fields.Name | Card.Fields.Position | Card.Fields.ShortId | Card.Fields.ShortUrl | Card.Fields.Url;
		SynchronizationContext<IJsonCard>.Properties = new Dictionary<string, Property<IJsonCard>>
		{
			{
				"Board",
				new Property<IJsonCard, Board>((IJsonCard d, TrelloAuthorization a) => d.Board?.GetFromCache<Board, IJsonBoard>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCard d, Board o)
				{
					d.Board = o?.Json;
				})
			},
			{
				"Description",
				new Property<IJsonCard, string>((IJsonCard d, TrelloAuthorization a) => d.Desc, delegate(IJsonCard d, string o)
				{
					d.Desc = o;
				})
			},
			{
				"DueDate",
				new Property<IJsonCard, DateTime?>((IJsonCard d, TrelloAuthorization a) => d.Due?.Decode(), delegate(IJsonCard d, DateTime? o)
				{
					d.Due = o?.Encode();
					d.ForceDueDate = !o.HasValue;
				})
			},
			{
				"Id",
				new Property<IJsonCard, string>((IJsonCard d, TrelloAuthorization a) => d.Id, delegate(IJsonCard d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsArchived",
				new Property<IJsonCard, bool?>((IJsonCard d, TrelloAuthorization a) => d.Closed, delegate(IJsonCard d, bool? o)
				{
					d.Closed = o;
				})
			},
			{
				"IsComplete",
				new Property<IJsonCard, bool?>((IJsonCard d, TrelloAuthorization a) => d.DueComplete, delegate(IJsonCard d, bool? o)
				{
					d.DueComplete = o;
				})
			},
			{
				"IsSubscribed",
				new Property<IJsonCard, bool?>((IJsonCard d, TrelloAuthorization a) => d.Subscribed, delegate(IJsonCard d, bool? o)
				{
					d.Subscribed = o;
				})
			},
			{
				"LastActivity",
				new Property<IJsonCard, DateTime?>((IJsonCard d, TrelloAuthorization a) => d.DateLastActivity, delegate(IJsonCard d, DateTime? o)
				{
					d.DateLastActivity = o;
				})
			},
			{
				"List",
				new Property<IJsonCard, List>((IJsonCard d, TrelloAuthorization a) => d.List?.GetFromCache<List, IJsonList>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCard d, List o)
				{
					d.List = o?.Json;
				})
			},
			{
				"Name",
				new Property<IJsonCard, string>((IJsonCard d, TrelloAuthorization a) => d.Name, delegate(IJsonCard d, string o)
				{
					d.Name = o;
				})
			},
			{
				"Position",
				new Property<IJsonCard, Position>((IJsonCard d, TrelloAuthorization a) => Position.GetPosition(d.Pos), delegate(IJsonCard d, Position o)
				{
					d.Pos = Position.GetJson(o);
				})
			},
			{
				"ShortId",
				new Property<IJsonCard, int?>((IJsonCard d, TrelloAuthorization a) => d.IdShort, delegate(IJsonCard d, int? o)
				{
					d.IdShort = o;
				})
			},
			{
				"ShortUrl",
				new Property<IJsonCard, string>((IJsonCard d, TrelloAuthorization a) => d.ShortUrl, delegate(IJsonCard d, string o)
				{
					d.ShortUrl = o;
				})
			},
			{
				"Url",
				new Property<IJsonCard, string>((IJsonCard d, TrelloAuthorization a) => d.Url, delegate(IJsonCard d, string o)
				{
					d.Url = o;
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonCard, bool>((IJsonCard d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonCard d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public CardContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
		Actions = new ReadOnlyActionCollection(typeof(Card), () => base.Data.Id, auth);
		Actions.Refreshed += delegate
		{
			OnMerged(new List<string> { "Actions" });
		};
		Attachments = new AttachmentCollection(() => base.Data.Id, auth);
		Attachments.Refreshed += delegate
		{
			OnMerged(new List<string> { "Attachments" });
		};
		CheckLists = new CheckListCollection(() => base.Data.Id, auth);
		CheckLists.Refreshed += delegate
		{
			OnMerged(new List<string> { "CheckLists" });
		};
		Comments = new CommentCollection(() => base.Data.Id, auth);
		Comments.Refreshed += delegate
		{
			OnMerged(new List<string> { "Comments" });
		};
		CustomFields = new ReadOnlyCustomFieldCollection(() => base.Data.Id, auth);
		CustomFields.Refreshed += delegate
		{
			OnMerged(new List<string> { "CustomFields" });
		};
		Labels = new CardLabelCollection(this, auth);
		Labels.Refreshed += delegate
		{
			OnMerged(new List<string> { "Labels" });
		};
		Members = new MemberCollection(() => base.Data.Id, auth);
		Members.Refreshed += delegate
		{
			OnMerged(new List<string> { "Members" });
		};
		PowerUpData = new ReadOnlyPowerUpDataCollection(EntityRequestType.Card_Read_PowerUpData, () => base.Data.Id, auth);
		PowerUpData.Refreshed += delegate
		{
			OnMerged(new List<string> { "PowerUpData" });
		};
		Stickers = new CardStickerCollection(() => base.Data.Id, auth);
		Stickers.Refreshed += delegate
		{
			OnMerged(new List<string> { "Stickers" });
		};
		VotingMembers = new ReadOnlyMemberCollection(EntityRequestType.Card_Read_MembersVoted, () => base.Data.Id, auth);
		VotingMembers.Refreshed += delegate
		{
			OnMerged(new List<string> { "VotingMembers" });
		};
		BadgesContext = new BadgesContext(base.Auth);
		base.Data.Badges = BadgesContext.Data;
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
			int num = Enum.GetValues(typeof(Card.Fields)).Cast<Card.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Card.Fields enumerationValue = (Card.Fields)((int)((uint)num & (uint)MemberFields) & (int)Card.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			Card.Fields fields = (Card.Fields)((int)((uint)num & (uint)Card.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(Card.Fields.Actions))
			{
				Parameters["actions"] = "all";
				Parameters["actions_format"] = "list";
			}
			if (fields.HasFlag(Card.Fields.Attachments))
			{
				Parameters["attachments"] = "true";
				Parameters["attachment_fields"] = AttachmentContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Card.Fields.CustomFields))
			{
				Parameters["customFieldItems"] = "true";
			}
			if (fields.HasFlag(Card.Fields.Checklists))
			{
				Parameters["checklists"] = "all";
				Parameters["checklist_fields"] = CheckListContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Card.Fields.Comments))
			{
				Parameters["actions"] = "commentCard";
				Parameters["actions_format"] = "list";
			}
			if (fields.HasFlag(Card.Fields.Members))
			{
				Parameters["members"] = "true";
				Parameters["member_fields"] = MemberContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Card.Fields.Stickers))
			{
				Parameters["stickers"] = "true";
			}
			if (fields.HasFlag(Card.Fields.VotingMembers))
			{
				Parameters["membersVoted"] = "true";
				Parameters["membersVoted_fields"] = MemberContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Card.Fields.Board))
			{
				Parameters["board"] = "true";
				Parameters["board_fields"] = BoardContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(Card.Fields.List))
			{
				Parameters["list"] = "true";
				Parameters["list_fields"] = ListContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Card_Read_Refresh, new Dictionary<string, object> { 
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
		return EndpointFactory.Build(EntityRequestType.Card_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task SubmitData(IJsonCard json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}

	protected override IEnumerable<string> MergeDependencies(IJsonCard json, bool overwrite)
	{
		List<string> list = (from p in BadgesContext.Merge(json.Badges, overwrite)
			select "Badges." + p).ToList();
		if (json.Actions != null)
		{
			Actions.Update(json.Actions.Select((IJsonAction a) => a.GetFromCache<Action, IJsonAction>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Actions");
		}
		if (json.Attachments != null)
		{
			Attachments.Update(json.Attachments.Select((IJsonAttachment a) => a.TryGetFromCache<Attachment, IJsonAttachment>(overwrite) ?? new Attachment(a, base.Data.Id, base.Auth)));
			list.Add("Attachments");
		}
		if (json.CheckLists != null)
		{
			CheckLists.Update(json.CheckLists.Select((IJsonCheckList a) => a.GetFromCache<CheckList, IJsonCheckList>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("CheckLists");
		}
		if (json.Comments != null)
		{
			Comments.Update(json.Comments.Select((IJsonAction a) => a.GetFromCache<Action, IJsonAction>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Comments");
		}
		if (json.CustomFields != null)
		{
			CustomFields.Update(json.CustomFields.Select((IJsonCustomField a) => a.GetFromCache<CustomField, IJsonCustomField>(base.Auth, overwrite, new object[1] { base.Data.Id })));
			list.Add("CustomFields");
		}
		if (json.Labels != null)
		{
			Labels.Update(json.Labels.Select((IJsonLabel a) => a.GetFromCache<Label, IJsonLabel>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Labels");
		}
		if (json.Members != null)
		{
			Members.Update(json.Members.Select((IJsonMember a) => a.GetFromCache<Member, IJsonMember>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Members");
		}
		if (json.PowerUpData != null)
		{
			PowerUpData.Update(json.PowerUpData.Select((IJsonPowerUpData a) => a.GetFromCache<PowerUpData, IJsonPowerUpData>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("PowerUpData");
		}
		if (json.Stickers != null)
		{
			Stickers.Update(json.Stickers.Select((IJsonSticker a) => a.TryGetFromCache<Sticker, IJsonSticker>(overwrite) ?? new Sticker(a, base.Data.Id, base.Auth)));
			list.Add("Stickers");
		}
		if (json.MembersVoted != null)
		{
			VotingMembers.Update(json.MembersVoted.Select((IJsonMember a) => a.GetFromCache<Member, IJsonMember>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("VotingMembers");
		}
		return list;
	}
}
