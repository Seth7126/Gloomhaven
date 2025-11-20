using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class LabelContext : DeletableSynchronizationContext<IJsonLabel>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Label.Fields MemberFields;

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

	static LabelContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Label.Fields.Color | Label.Fields.Name;
		SynchronizationContext<IJsonLabel>.Properties = new Dictionary<string, Property<IJsonLabel>>
		{
			{
				"Board",
				new Property<IJsonLabel, Board>((IJsonLabel d, TrelloAuthorization a) => d.Board?.GetFromCache<Board, IJsonBoard>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonLabel d, Board o)
				{
					d.Board = o?.Json;
				})
			},
			{
				"Color",
				new Property<IJsonLabel, LabelColor?>((IJsonLabel d, TrelloAuthorization a) => d.Color, delegate(IJsonLabel d, LabelColor? o)
				{
					d.Color = o;
				})
			},
			{
				"Id",
				new Property<IJsonLabel, string>((IJsonLabel d, TrelloAuthorization a) => d.Id, delegate(IJsonLabel d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Name",
				new Property<IJsonLabel, string>((IJsonLabel d, TrelloAuthorization a) => d.Name, delegate(IJsonLabel d, string o)
				{
					d.Name = o;
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonLabel, bool>((IJsonLabel d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonLabel d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public LabelContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
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
			int num = Enum.GetValues(typeof(Label.Fields)).Cast<Label.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Label.Fields enumerationValue = (Label.Fields)((int)((uint)num & (uint)MemberFields) & (int)Label.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			Label.Fields fields = (Label.Fields)((int)((uint)num & (uint)Label.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(Label.Fields.Board))
			{
				Parameters["board"] = "true";
				Parameters["board_fields"] = BoardContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Label_Read_Refresh, new Dictionary<string, object> { 
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
		return EndpointFactory.Build(EntityRequestType.Label_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task SubmitData(IJsonLabel json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Label_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}
}
