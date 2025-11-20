using System;
using System.Collections.Generic;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class DropDownOptionContext : DeletableSynchronizationContext<IJsonCustomDropDownOption>
{
	static DropDownOptionContext()
	{
		SynchronizationContext<IJsonCustomDropDownOption>.Properties = new Dictionary<string, Property<IJsonCustomDropDownOption>>
		{
			{
				"Field",
				new Property<IJsonCustomDropDownOption, CustomFieldDefinition>((IJsonCustomDropDownOption d, TrelloAuthorization a) => d.Field.GetFromCache<CustomFieldDefinition, IJsonCustomFieldDefinition>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCustomDropDownOption d, CustomFieldDefinition o)
				{
					if (o != null)
					{
						d.Field = o.Json;
					}
				})
			},
			{
				"Id",
				new Property<IJsonCustomDropDownOption, string>((IJsonCustomDropDownOption d, TrelloAuthorization a) => d.Id, delegate(IJsonCustomDropDownOption d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Text",
				new Property<IJsonCustomDropDownOption, string>((IJsonCustomDropDownOption d, TrelloAuthorization a) => d.Text, delegate(IJsonCustomDropDownOption d, string o)
				{
					if (o != null)
					{
						d.Text = o;
					}
				})
			},
			{
				"Color",
				new Property<IJsonCustomDropDownOption, LabelColor?>((IJsonCustomDropDownOption d, TrelloAuthorization a) => d.Color, delegate(IJsonCustomDropDownOption d, LabelColor? o)
				{
					if (o.HasValue)
					{
						d.Color = o;
					}
				})
			},
			{
				"Position",
				new Property<IJsonCustomDropDownOption, Position>((IJsonCustomDropDownOption d, TrelloAuthorization a) => Position.GetPosition(d.Pos), delegate(IJsonCustomDropDownOption d, Position o)
				{
					d.Pos = Position.GetJson(o);
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonCustomDropDownOption, bool>((IJsonCustomDropDownOption d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonCustomDropDownOption d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public DropDownOptionContext(IJsonCustomDropDownOption data, TrelloAuthorization auth, bool created)
		: base(auth, useTimer: true)
	{
		base.Data.Id = data.Id;
		Merge(data);
		base.Deleted = created;
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CustomFieldDropDownOption_Read_Refresh, new Dictionary<string, object>
		{
			{
				"_idField",
				base.Data.Field.Id
			},
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CustomFieldDropDownOption_Write_Delete, new Dictionary<string, object>
		{
			{
				"_idField",
				base.Data.Field.Id
			},
			{
				"_id",
				base.Data.Id
			}
		});
	}
}
