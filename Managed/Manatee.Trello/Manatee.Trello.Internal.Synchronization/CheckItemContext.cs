using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class CheckItemContext : DeletableSynchronizationContext<IJsonCheckItem>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly CheckItem.Fields MemberFields;

	private string _ownerId;

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

	static CheckItemContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = CheckItem.Fields.State | CheckItem.Fields.Name | CheckItem.Fields.Position;
		SynchronizationContext<IJsonCheckItem>.Properties = new Dictionary<string, Property<IJsonCheckItem>>
		{
			{
				"CheckList",
				new Property<IJsonCheckItem, CheckList>((IJsonCheckItem d, TrelloAuthorization a) => d.CheckList?.GetFromCache<CheckList, IJsonCheckList>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCheckItem d, CheckList o)
				{
					d.CheckList = o?.Json;
				})
			},
			{
				"Id",
				new Property<IJsonCheckItem, string>((IJsonCheckItem d, TrelloAuthorization a) => d.Id, delegate(IJsonCheckItem d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Name",
				new Property<IJsonCheckItem, string>((IJsonCheckItem d, TrelloAuthorization a) => d.Name, delegate(IJsonCheckItem d, string o)
				{
					d.Name = o;
				})
			},
			{
				"Position",
				new Property<IJsonCheckItem, Position>((IJsonCheckItem d, TrelloAuthorization a) => Position.GetPosition(d.Pos), delegate(IJsonCheckItem d, Position o)
				{
					d.Pos = Position.GetJson(o);
				})
			},
			{
				"State",
				new Property<IJsonCheckItem, CheckItemState?>((IJsonCheckItem d, TrelloAuthorization a) => d.State, delegate(IJsonCheckItem d, CheckItemState? o)
				{
					d.State = o;
				})
			}
		};
	}

	public CheckItemContext(string id, string ownerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_ownerId = ownerId;
		base.Data.Id = id;
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
			CheckItem.Fields enumerationValue = (CheckItem.Fields)((int)((uint)Enum.GetValues(typeof(CheckItem.Fields)).Cast<CheckItem.Fields>().ToList()
				.Cast<int>()
				.Sum() & (uint)MemberFields) & (int)CheckItem.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CheckItem_Read_Refresh, new Dictionary<string, object>
		{
			{ "_checklistId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override Dictionary<string, object> GetParameters()
	{
		return CurrentParameters;
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CheckItem_Write_Delete, new Dictionary<string, object>
		{
			{ "_checklistId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override async Task SubmitData(IJsonCheckItem json, CancellationToken ct)
	{
		IJsonCheckList jsonCheckList = TrelloConfiguration.JsonFactory.Create<IJsonCheckList>();
		jsonCheckList.Id = _ownerId;
		CheckList checkList = jsonCheckList.GetFromCache<CheckList, IJsonCheckList>(base.Auth, overwrite: true, Array.Empty<object>());
		if (string.IsNullOrEmpty(checkList.Json.Card?.Id))
		{
			await checkList.Refresh(force: false, ct);
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CheckItem_Write_Update, new Dictionary<string, object>
		{
			{
				"_cardId",
				checkList.Card.Id
			},
			{ "_checklistId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
		IJsonCheckItem jsonCheckItem = await JsonRepository.Execute(base.Auth, endpoint, json, ct);
		if (!string.IsNullOrEmpty(jsonCheckItem.CheckList?.Id))
		{
			_ownerId = jsonCheckItem.CheckList.Id;
		}
		Merge(jsonCheckItem);
	}
}
