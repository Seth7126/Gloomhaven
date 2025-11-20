using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyPowerUpCollection : ReadOnlyCollection<IPowerUp>
{
	internal ReadOnlyPowerUpCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Read_PowerUps, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonPowerUp> source = await JsonRepository.Execute<List<IJsonPowerUp>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		base.Items.AddRange(source.Select(delegate(IJsonPowerUp jn)
		{
			IPowerUp fromCache = jn.GetFromCache<IPowerUp>(base.Auth);
			if (fromCache is PowerUpBase powerUpBase)
			{
				powerUpBase.Json = jn;
			}
			return fromCache;
		}));
	}
}
