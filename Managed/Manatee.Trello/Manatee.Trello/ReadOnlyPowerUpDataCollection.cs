using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyPowerUpDataCollection : ReadOnlyCollection<IPowerUpData>
{
	private readonly EntityRequestType _requestType;

	internal ReadOnlyPowerUpDataCollection(EntityRequestType requestType, Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		_requestType = requestType;
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(_requestType, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonPowerUpData> source = await JsonRepository.Execute<List<IJsonPowerUpData>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		base.Items.AddRange(source.Select(delegate(IJsonPowerUpData jn)
		{
			PowerUpData fromCache = jn.GetFromCache<PowerUpData, IJsonPowerUpData>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jn;
			return fromCache;
		}));
	}
}
