using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class CustomFieldContext : SynchronizationContext<IJsonCustomField>
{
	private readonly string _ownerId;

	static CustomFieldContext()
	{
		SynchronizationContext<IJsonCustomField>.Properties = new Dictionary<string, Property<IJsonCustomField>>
		{
			{
				"Definition",
				new Property<IJsonCustomField, CustomFieldDefinition>((IJsonCustomField d, TrelloAuthorization a) => d.Definition.GetFromCache<CustomFieldDefinition, IJsonCustomFieldDefinition>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCustomField d, CustomFieldDefinition o)
				{
					if (o != null)
					{
						d.Definition = o.Json;
					}
				})
			},
			{
				"Checked",
				new Property<IJsonCustomField, bool?>((IJsonCustomField d, TrelloAuthorization a) => d.Checked, delegate(IJsonCustomField d, bool? o)
				{
					d.Checked = o;
				})
			},
			{
				"Date",
				new Property<IJsonCustomField, DateTime?>((IJsonCustomField d, TrelloAuthorization a) => d.Date, delegate(IJsonCustomField d, DateTime? o)
				{
					d.Date = o;
				})
			},
			{
				"Id",
				new Property<IJsonCustomField, string>((IJsonCustomField d, TrelloAuthorization a) => d.Id, delegate(IJsonCustomField d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Number",
				new Property<IJsonCustomField, double?>((IJsonCustomField d, TrelloAuthorization a) => d.Number, delegate(IJsonCustomField d, double? o)
				{
					d.Number = o;
				})
			},
			{
				"Text",
				new Property<IJsonCustomField, string>((IJsonCustomField d, TrelloAuthorization a) => d.Text, delegate(IJsonCustomField d, string o)
				{
					d.Text = o;
				})
			},
			{
				"Selected",
				new Property<IJsonCustomField, DropDownOption>((IJsonCustomField d, TrelloAuthorization a) => d.Selected.GetFromCache<DropDownOption>(a), delegate(IJsonCustomField d, DropDownOption o)
				{
					d.Selected = o?.Json;
				})
			},
			{
				"Type",
				new Property<IJsonCustomField, CustomFieldType>((IJsonCustomField d, TrelloAuthorization a) => d.Type, delegate(IJsonCustomField d, CustomFieldType o)
				{
					d.Type = o;
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonCustomField, bool>((IJsonCustomField d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonCustomField d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public CustomFieldContext(string id, string ownerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_ownerId = ownerId;
		base.Data.Id = id;
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CustomField_Read_Refresh, new Dictionary<string, object>
		{
			{ "_cardId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override async Task SubmitData(IJsonCustomField json, CancellationToken ct)
	{
		IJsonParameter jsonParameter = TrelloConfiguration.JsonFactory.Create<IJsonParameter>();
		jsonParameter.Object = json;
		json.Type = base.Data.Type;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CustomField_Write_Update, new Dictionary<string, object>
		{
			{ "_cardId", _ownerId },
			{
				"_id",
				base.Data.Definition.Id
			}
		});
		Merge(await JsonRepository.Execute<IJsonParameter, IJsonCustomField>(base.Auth, endpoint, jsonParameter, ct));
	}
}
