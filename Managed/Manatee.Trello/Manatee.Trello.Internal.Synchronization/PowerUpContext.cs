using System.Collections.Generic;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class PowerUpContext : SynchronizationContext<IJsonPowerUp>
{
	static PowerUpContext()
	{
		SynchronizationContext<IJsonPowerUp>.Properties = new Dictionary<string, Property<IJsonPowerUp>>
		{
			{
				"Id",
				new Property<IJsonPowerUp, string>((IJsonPowerUp d, TrelloAuthorization a) => d.Id, delegate(IJsonPowerUp d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Name",
				new Property<IJsonPowerUp, string>((IJsonPowerUp d, TrelloAuthorization a) => d.Name, delegate(IJsonPowerUp d, string o)
				{
					d.Name = o;
				})
			},
			{
				"IsPublic",
				new Property<IJsonPowerUp, bool?>((IJsonPowerUp d, TrelloAuthorization a) => d.Public, delegate(IJsonPowerUp d, bool? o)
				{
					d.Public = o;
				})
			},
			{
				"AdditionalInfo",
				new Property<IJsonPowerUp, string>((IJsonPowerUp d, TrelloAuthorization a) => d.Url, delegate(IJsonPowerUp d, string o)
				{
					d.Url = o;
				})
			}
		};
	}

	public PowerUpContext(IJsonPowerUp json, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = json.Id;
		Merge(json);
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.PowerUp_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}
}
