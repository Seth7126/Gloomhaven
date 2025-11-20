using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class CheckListContext : DeletableSynchronizationContext<IJsonCheckList>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly CheckList.Fields MemberFields;

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

	public CheckItemCollection CheckItems { get; }

	static CheckListContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = CheckList.Fields.Name | CheckList.Fields.Board | CheckList.Fields.Card | CheckList.Fields.Position;
		SynchronizationContext<IJsonCheckList>.Properties = new Dictionary<string, Property<IJsonCheckList>>
		{
			{
				"Board",
				new Property<IJsonCheckList, Board>((IJsonCheckList d, TrelloAuthorization a) => d.Board.GetFromCache<Board, IJsonBoard>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCheckList d, Board o)
				{
					d.Board = o?.Json;
				})
			},
			{
				"Card",
				new Property<IJsonCheckList, Card>((IJsonCheckList d, TrelloAuthorization a) => d.Card.GetFromCache<Card, IJsonCard>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCheckList d, Card o)
				{
					d.Card = o?.Json;
				})
			},
			{
				"Id",
				new Property<IJsonCheckList, string>((IJsonCheckList d, TrelloAuthorization a) => d.Id, delegate(IJsonCheckList d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Name",
				new Property<IJsonCheckList, string>((IJsonCheckList d, TrelloAuthorization a) => d.Name, delegate(IJsonCheckList d, string o)
				{
					d.Name = o;
				})
			},
			{
				"Position",
				new Property<IJsonCheckList, Position>((IJsonCheckList d, TrelloAuthorization a) => Position.GetPosition(d.Pos), delegate(IJsonCheckList d, Position o)
				{
					d.Pos = Position.GetJson(o);
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonCheckList, bool>((IJsonCheckList d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonCheckList d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public CheckListContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
		CheckItems = new CheckItemCollection(this, auth);
		CheckItems.Refreshed += delegate
		{
			OnMerged(new List<string> { "CheckItems" });
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
			int num = Enum.GetValues(typeof(CheckList.Fields)).Cast<CheckList.Fields>().ToList()
				.Cast<int>()
				.Sum();
			CheckList.Fields enumerationValue = (CheckList.Fields)((int)((uint)num & (uint)MemberFields) & (int)CheckList.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			CheckList.Fields fields = (CheckList.Fields)((int)((uint)num & (uint)CheckList.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(CheckList.Fields.CheckItems))
			{
				Parameters["checkItems"] = "all";
				Parameters["checkItem_fields"] = CheckItemContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(CheckList.Fields.Board))
			{
				Parameters["board"] = "true";
				Parameters["board_fields"] = BoardContext.CurrentParameters["fields"];
			}
			if (fields.HasFlag(CheckList.Fields.Card))
			{
				Parameters["card"] = "true";
				Parameters["card_fields"] = CardContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CheckList_Read_Refresh, new Dictionary<string, object> { 
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
		return EndpointFactory.Build(EntityRequestType.CheckList_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task SubmitData(IJsonCheckList json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CheckList_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}

	protected override IEnumerable<string> MergeDependencies(IJsonCheckList json, bool overwrite)
	{
		List<string> list = new List<string>();
		if (json.CheckItems != null)
		{
			CheckItems.Update(json.CheckItems.Select((IJsonCheckItem a) => a.TryGetFromCache<CheckItem>() ?? new CheckItem(a, base.Data.Id, base.Auth)));
			list.Add("CheckItems");
		}
		return list;
	}
}
