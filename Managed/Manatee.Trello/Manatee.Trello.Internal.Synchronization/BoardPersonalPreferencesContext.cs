using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class BoardPersonalPreferencesContext : SynchronizationContext<IJsonBoardPersonalPreferences>
{
	private readonly Func<string> _getOwnerId;

	private string _ownerId;

	private string OwnerId => _ownerId ?? (_ownerId = _getOwnerId());

	static BoardPersonalPreferencesContext()
	{
		SynchronizationContext<IJsonBoardPersonalPreferences>.Properties = new Dictionary<string, Property<IJsonBoardPersonalPreferences>>
		{
			{
				"ShowSidebar",
				new Property<IJsonBoardPersonalPreferences, bool?>((IJsonBoardPersonalPreferences d, TrelloAuthorization a) => d.ShowSidebar, delegate(IJsonBoardPersonalPreferences d, bool? o)
				{
					d.ShowSidebar = o;
				})
			},
			{
				"ShowSidebarMembers",
				new Property<IJsonBoardPersonalPreferences, bool?>((IJsonBoardPersonalPreferences d, TrelloAuthorization a) => d.ShowSidebarMembers, delegate(IJsonBoardPersonalPreferences d, bool? o)
				{
					d.ShowSidebarMembers = o;
				})
			},
			{
				"ShowSidebarBoardActions",
				new Property<IJsonBoardPersonalPreferences, bool?>((IJsonBoardPersonalPreferences d, TrelloAuthorization a) => d.ShowSidebarBoardActions, delegate(IJsonBoardPersonalPreferences d, bool? o)
				{
					d.ShowSidebarBoardActions = o;
				})
			},
			{
				"ShowSidebarActivity",
				new Property<IJsonBoardPersonalPreferences, bool?>((IJsonBoardPersonalPreferences d, TrelloAuthorization a) => d.ShowSidebarActivity, delegate(IJsonBoardPersonalPreferences d, bool? o)
				{
					d.ShowSidebarActivity = o;
				})
			},
			{
				"ShowListGuide",
				new Property<IJsonBoardPersonalPreferences, bool?>((IJsonBoardPersonalPreferences d, TrelloAuthorization a) => d.ShowListGuide, delegate(IJsonBoardPersonalPreferences d, bool? o)
				{
					d.ShowListGuide = o;
				})
			},
			{
				"EmailPosition",
				new Property<IJsonBoardPersonalPreferences, Position>((IJsonBoardPersonalPreferences d, TrelloAuthorization a) => Position.GetPosition(d.EmailPosition), delegate(IJsonBoardPersonalPreferences d, Position o)
				{
					d.EmailPosition = Position.GetJson(o);
				})
			},
			{
				"EmailList",
				new Property<IJsonBoardPersonalPreferences, List>((IJsonBoardPersonalPreferences d, TrelloAuthorization a) => d.EmailList?.GetFromCache<List, IJsonList>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonBoardPersonalPreferences d, List o)
				{
					d.EmailList = o?.Json;
				})
			}
		};
	}

	public BoardPersonalPreferencesContext(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_getOwnerId = getOwnerId;
	}

	protected override async Task<IJsonBoardPersonalPreferences> GetData(CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Read_PersonalPrefs, new Dictionary<string, object> { { "_id", OwnerId } });
		return await JsonRepository.Execute<IJsonBoardPersonalPreferences>(TrelloAuthorization.Default, endpoint, ct);
	}

	protected override Task SubmitData(IJsonBoardPersonalPreferences json, CancellationToken ct)
	{
		return Task.WhenAll((json.EmailList == null) ? Task.CompletedTask : SubmitDataPoint(json.EmailList.Id, "emailList", ct), SubmitDataPoint(json.EmailPosition, "emailPosition", ct), SubmitDataPoint(json.ShowListGuide, "showListGuide", ct), SubmitDataPoint(json.ShowSidebar, "showSidebar", ct), SubmitDataPoint(json.ShowSidebarActivity, "showSidebarActivity", ct), SubmitDataPoint(json.ShowSidebarBoardActions, "showSidebarBoardActions", ct), SubmitDataPoint(json.ShowSidebarMembers, "showSidebarMembers", ct));
	}

	private async Task SubmitDataPoint<T>(T value, string segment, CancellationToken ct)
	{
		if (!object.Equals(value, default(T)))
		{
			IJsonParameter jsonParameter = TrelloConfiguration.JsonFactory.Create<IJsonParameter>();
			if (typeof(T) == typeof(bool?))
			{
				jsonParameter.Boolean = (bool?)(object)value;
			}
			else
			{
				jsonParameter.String = value.ToString();
			}
			Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_PersonalPrefs, new Dictionary<string, object> { { "_id", _ownerId } });
			endpoint.AddSegment(segment);
			await JsonRepository.Execute(base.Auth, endpoint, jsonParameter, ct);
		}
	}
}
