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

internal class ListContext : SynchronizationContext<IJsonList>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly List.Fields MemberFields;

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

	public CardCollection Cards { get; }

	public virtual bool HasValidId => IdRule.Instance.Validate(base.Data.Id, null) == null;

	static ListContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = List.Fields.Name | List.Fields.IsClosed | List.Fields.Position | List.Fields.IsSubscribed;
		SynchronizationContext<IJsonList>.Properties = new Dictionary<string, Property<IJsonList>>
		{
			{
				"Board",
				new Property<IJsonList, Board>((IJsonList d, TrelloAuthorization a) => d.Board?.GetFromCache<Board, IJsonBoard>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonList d, Board o)
				{
					if (o != null)
					{
						d.Board = o.Json;
					}
				})
			},
			{
				"Id",
				new Property<IJsonList, string>((IJsonList d, TrelloAuthorization a) => d.Id, delegate(IJsonList d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsArchived",
				new Property<IJsonList, bool?>((IJsonList d, TrelloAuthorization a) => d.Closed, delegate(IJsonList d, bool? o)
				{
					d.Closed = o;
				})
			},
			{
				"IsSubscribed",
				new Property<IJsonList, bool?>((IJsonList d, TrelloAuthorization a) => d.Subscribed, delegate(IJsonList d, bool? o)
				{
					d.Subscribed = o;
				})
			},
			{
				"Name",
				new Property<IJsonList, string>((IJsonList d, TrelloAuthorization a) => d.Name, delegate(IJsonList d, string o)
				{
					d.Name = o;
				})
			},
			{
				"Position",
				new Property<IJsonList, Position>((IJsonList d, TrelloAuthorization a) => Position.GetPosition(d.Pos), delegate(IJsonList d, Position o)
				{
					d.Pos = Position.GetJson(o);
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonList, bool>((IJsonList d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonList d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public ListContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
		Actions = new ReadOnlyActionCollection(typeof(List), () => base.Data.Id, auth);
		Actions.Refreshed += delegate
		{
			OnMerged(new List<string> { "Actions" });
		};
		Cards = new CardCollection(() => base.Data.Id, auth);
		Cards.Refreshed += delegate
		{
			OnMerged(new List<string> { "Cards" });
		};
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
			int num = Enum.GetValues(typeof(List.Fields)).Cast<List.Fields>().ToList()
				.Cast<int>()
				.Sum();
			List.Fields enumerationValue = (List.Fields)((int)((uint)num & (uint)MemberFields) & (int)List.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			List.Fields fields = (List.Fields)((int)((uint)num & (uint)List.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(List.Fields.Actions))
			{
				Parameters["actions"] = "all";
				Parameters["actions_format"] = "list";
			}
			if (fields.HasFlag(List.Fields.Cards))
			{
				Parameters["cards"] = "open";
				object obj = CardContext.CurrentParameters["fields"];
				if (Card.DownloadedFields.HasFlag(Card.Fields.List))
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
			}
			if (fields.HasFlag(List.Fields.Board))
			{
				Parameters["board"] = "true";
				Parameters["board_fields"] = BoardContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.List_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override Dictionary<string, object> GetParameters()
	{
		return CurrentParameters;
	}

	protected override async Task SubmitData(IJsonList json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.List_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}

	protected override IEnumerable<string> MergeDependencies(IJsonList json, bool overwrite)
	{
		List<string> list = new List<string>();
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
		return list;
	}
}
