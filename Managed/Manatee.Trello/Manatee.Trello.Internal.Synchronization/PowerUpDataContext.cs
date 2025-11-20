using System.Collections.Generic;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class PowerUpDataContext : SynchronizationContext<IJsonPowerUpData>
{
	static PowerUpDataContext()
	{
		SynchronizationContext<IJsonPowerUpData>.Properties = new Dictionary<string, Property<IJsonPowerUpData>>
		{
			{
				"Id",
				new Property<IJsonPowerUpData, string>((IJsonPowerUpData d, TrelloAuthorization a) => d.Id, delegate(IJsonPowerUpData d, string o)
				{
					d.Id = o;
				})
			},
			{
				"PluginId",
				new Property<IJsonPowerUpData, string>((IJsonPowerUpData d, TrelloAuthorization a) => d.PluginId, delegate(IJsonPowerUpData d, string o)
				{
					d.PluginId = o;
				})
			},
			{
				"Value",
				new Property<IJsonPowerUpData, string>((IJsonPowerUpData d, TrelloAuthorization a) => d.Value, delegate(IJsonPowerUpData d, string o)
				{
					d.Value = o;
				})
			}
		};
	}

	public PowerUpDataContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
	}
}
