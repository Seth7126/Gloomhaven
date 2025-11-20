using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class PowerUpCollection : ReadOnlyPowerUpCollection, IPowerUpCollection, IReadOnlyCollection<IPowerUp>, IEnumerable<IPowerUp>, IEnumerable, IRefreshable
{
	internal PowerUpCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task EnablePowerUp(IPowerUp powerUp, CancellationToken ct = default(CancellationToken))
	{
		IJsonPowerUp jsonPowerUp = TrelloConfiguration.JsonFactory.Create<IJsonPowerUp>();
		jsonPowerUp.Id = powerUp.Id;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_EnablePowerUp, new Dictionary<string, object> { { "_id", base.OwnerId } });
		await JsonRepository.Execute(base.Auth, endpoint, jsonPowerUp, ct);
	}

	public async Task DisablePowerUp(IPowerUp powerUp, CancellationToken ct = default(CancellationToken))
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_DisablePowerUp, new Dictionary<string, object>
		{
			{ "_boardId", base.OwnerId },
			{ "_id", base.OwnerId }
		});
		await JsonRepository.Execute(base.Auth, endpoint, ct, base.AdditionalParameters);
	}
}
