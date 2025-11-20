using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;
using Manatee.Trello.Rest;

namespace Manatee.Trello;

public class BoardBackgroundCollection : ReadOnlyBoardBackgroundCollection, IBoardBackgroundCollection, IReadOnlyCollection<IBoardBackground>, IEnumerable<IBoardBackground>, IEnumerable, IRefreshable
{
	internal BoardBackgroundCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<IBoardBackground> Add(byte[] data, CancellationToken ct = default(CancellationToken))
	{
		Dictionary<string, object> parameters = new Dictionary<string, object> { 
		{
			"file",
			new RestFile
			{
				ContentBytes = data
			}
		} };
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Write_AddBoardBackground, new Dictionary<string, object> { { "_id", base.OwnerId } });
		IJsonBoardBackground json = await JsonRepository.Execute<IJsonBoardBackground>(base.Auth, endpoint, ct, parameters);
		return new BoardBackground(base.OwnerId, json, base.Auth);
	}
}
