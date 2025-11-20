using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;
using Manatee.Trello.Rest;

namespace Manatee.Trello;

public class MemberStickerCollection : ReadOnlyStickerCollection, IMemberStickerCollection
{
	internal MemberStickerCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<ISticker> Add(byte[] data, string name, CancellationToken ct = default(CancellationToken))
	{
		Dictionary<string, object> parameters = new Dictionary<string, object> { 
		{
			"file",
			new RestFile
			{
				ContentBytes = data,
				FileName = name
			}
		} };
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_AddAttachment, new Dictionary<string, object> { { "_id", base.OwnerId } });
		return new Sticker(await JsonRepository.Execute<IJsonSticker>(base.Auth, endpoint, ct, parameters), base.OwnerId, base.Auth);
	}
}
