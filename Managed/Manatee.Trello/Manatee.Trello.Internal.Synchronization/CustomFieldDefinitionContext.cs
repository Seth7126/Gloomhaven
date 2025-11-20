using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class CustomFieldDefinitionContext : DeletableSynchronizationContext<IJsonCustomFieldDefinition>
{
	public DropDownOptionCollection DropDownOptions { get; }

	public CustomFieldDisplayInfoContext DisplayInfo { get; }

	static CustomFieldDefinitionContext()
	{
		SynchronizationContext<IJsonCustomFieldDefinition>.Properties = new Dictionary<string, Property<IJsonCustomFieldDefinition>>
		{
			{
				"Board",
				new Property<IJsonCustomFieldDefinition, Board>((IJsonCustomFieldDefinition d, TrelloAuthorization a) => d.Board?.GetFromCache<Board, IJsonBoard>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonCustomFieldDefinition d, Board o)
				{
					if (o != null)
					{
						d.Board = o.Json;
					}
				})
			},
			{
				"Display",
				new Property<IJsonCustomFieldDefinition, IJsonCustomFieldDisplayInfo>((IJsonCustomFieldDefinition d, TrelloAuthorization a) => d.Display, delegate(IJsonCustomFieldDefinition d, IJsonCustomFieldDisplayInfo o)
				{
					if (o != null)
					{
						d.Display = o;
					}
				})
			},
			{
				"FieldGroup",
				new Property<IJsonCustomFieldDefinition, string>((IJsonCustomFieldDefinition d, TrelloAuthorization a) => d.FieldGroup, delegate(IJsonCustomFieldDefinition d, string o)
				{
					if (o != null)
					{
						d.FieldGroup = o;
					}
				})
			},
			{
				"Id",
				new Property<IJsonCustomFieldDefinition, string>((IJsonCustomFieldDefinition d, TrelloAuthorization a) => d.Id, delegate(IJsonCustomFieldDefinition d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Name",
				new Property<IJsonCustomFieldDefinition, string>((IJsonCustomFieldDefinition d, TrelloAuthorization a) => d.Name, delegate(IJsonCustomFieldDefinition d, string o)
				{
					if (o != null)
					{
						d.Name = o;
					}
				})
			},
			{
				"Position",
				new Property<IJsonCustomFieldDefinition, Position>((IJsonCustomFieldDefinition d, TrelloAuthorization a) => Position.GetPosition(d.Pos), delegate(IJsonCustomFieldDefinition d, Position o)
				{
					d.Pos = Position.GetJson(o);
				})
			},
			{
				"Type",
				new Property<IJsonCustomFieldDefinition, CustomFieldType?>((IJsonCustomFieldDefinition d, TrelloAuthorization a) => d.Type, delegate(IJsonCustomFieldDefinition d, CustomFieldType? o)
				{
					if (o.HasValue)
					{
						d.Type = o;
					}
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonCustomFieldDefinition, bool>((IJsonCustomFieldDefinition d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonCustomFieldDefinition d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public CustomFieldDefinitionContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
		DropDownOptions = new DropDownOptionCollection(() => base.Data.Id, auth);
		DropDownOptions.Refreshed += delegate
		{
			OnMerged(new List<string> { "DropDownOptions" });
		};
		DisplayInfo = new CustomFieldDisplayInfoContext(auth);
		CustomFieldDisplayInfoContext displayInfo = DisplayInfo;
		displayInfo.SubmitRequested = (Func<CancellationToken, Task>)Delegate.Combine(displayInfo.SubmitRequested, (Func<CancellationToken, Task>)((CancellationToken ct) => HandleSubmitRequested("Display", ct)));
		base.Data.Display = DisplayInfo.Data;
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CustomFieldDefinition_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.CustomFieldDefinition_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task SubmitData(IJsonCustomFieldDefinition json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CustomFieldDefinition_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
		base.Data.Display = DisplayInfo.Data;
	}

	protected override void ApplyDependentChanges(IJsonCustomFieldDefinition json)
	{
		base.Data.Display = DisplayInfo.Data;
		if (DisplayInfo.HasChanges)
		{
			json.Display = DisplayInfo.GetChanges();
			DisplayInfo.ClearChanges();
		}
	}

	protected override IEnumerable<string> MergeDependencies(IJsonCustomFieldDefinition json, bool overwrite)
	{
		List<string> list = DisplayInfo.Merge(json.Display).ToList();
		if (json.Options != null)
		{
			DropDownOptions.Update(json.Options.Select((IJsonCustomDropDownOption a) => a.GetFromCache<DropDownOption, IJsonCustomDropDownOption>(base.Auth, overwrite, Array.Empty<object>())));
			list.Add("Options");
		}
		return list;
	}

	public async Task<ICustomField<T>> SetValueOnCard<T>(ICard card, T value, CancellationToken ct)
	{
		IJsonCustomField jsonCustomField = TrelloConfiguration.JsonFactory.Create<IJsonCustomField>();
		Func<IJsonCustomField, ICustomField<T>> createField = null;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CustomField_Write_Update, new Dictionary<string, object>
		{
			{ "_cardId", card.Id },
			{
				"_id",
				base.Data.Id
			}
		});
		IJsonCustomField arg;
		if (typeof(T) == typeof(IDropDownOption))
		{
			jsonCustomField.Type = CustomFieldType.DropDown;
			jsonCustomField.Selected = ((DropDownOption)(object)value).Json;
			createField = (IJsonCustomField j) => (ICustomField<T>)new DropDownField(j, card.Id, base.Auth);
			arg = await JsonRepository.Execute<IJsonCustomField, IJsonCustomField>(base.Auth, endpoint, jsonCustomField, ct);
		}
		else
		{
			if (typeof(T) == typeof(double?))
			{
				jsonCustomField.Type = CustomFieldType.Number;
				jsonCustomField.Number = (double?)(object)value;
				createField = (IJsonCustomField j) => (ICustomField<T>)new NumberField(j, card.Id, base.Auth);
			}
			else if (typeof(T) == typeof(bool?))
			{
				jsonCustomField.Type = CustomFieldType.CheckBox;
				jsonCustomField.Checked = (bool?)(object)value;
				createField = (IJsonCustomField j) => (ICustomField<T>)new CheckBoxField(j, card.Id, base.Auth);
			}
			else if (typeof(T) == typeof(DateTime?))
			{
				jsonCustomField.Type = CustomFieldType.DateTime;
				jsonCustomField.Date = (DateTime?)(object)value;
				createField = (IJsonCustomField j) => (ICustomField<T>)new DateTimeField(j, card.Id, base.Auth);
			}
			else if (typeof(T) == typeof(string))
			{
				jsonCustomField.Type = CustomFieldType.Text;
				jsonCustomField.Text = (string)(object)value;
				createField = (IJsonCustomField j) => (ICustomField<T>)new TextField(j, card.Id, base.Auth);
			}
			IJsonParameter jsonParameter = TrelloConfiguration.JsonFactory.Create<IJsonParameter>();
			jsonParameter.Object = jsonCustomField;
			arg = await JsonRepository.Execute<IJsonParameter, IJsonCustomField>(base.Auth, endpoint, jsonParameter, ct);
		}
		return createField(arg);
	}
}
