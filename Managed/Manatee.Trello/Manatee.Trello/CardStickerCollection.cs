using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CardStickerCollection : ReadOnlyStickerCollection, ICardStickerCollection, IReadOnlyCollection<ISticker>, IEnumerable<ISticker>, IEnumerable, IRefreshable
{
	private static readonly NumericRule<int> RotationRule = new NumericRule<int>
	{
		Min = 0,
		Max = 359
	};

	internal CardStickerCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<ISticker> Add(string name, double left, double top, int zIndex = 0, int rotation = 0, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullOrWhiteSpaceRule.Instance.Validate(null, name);
		if (text != null)
		{
			throw new ValidationException<string>(name, new string[1] { text });
		}
		text = RotationRule.Validate(null, rotation);
		if (text != null)
		{
			throw new ValidationException<int>(rotation, new string[1] { text });
		}
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "image", name },
			{ "top", top },
			{ "left", left },
			{ "zIndex", zIndex },
			{ "rotate", rotation }
		};
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_AddSticker, new Dictionary<string, object> { { "_id", base.OwnerId } });
		return new Sticker(await JsonRepository.Execute<IJsonSticker>(base.Auth, endpoint, ct, parameters), base.OwnerId, base.Auth);
	}

	public async Task Remove(Sticker sticker, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<Sticker>.Instance.Validate(null, sticker);
		if (text != null)
		{
			throw new ValidationException<Sticker>(sticker, new string[1] { text });
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_RemoveSticker, new Dictionary<string, object>
		{
			{ "_id", base.OwnerId },
			{ "_stickerId", sticker.Id }
		});
		await JsonRepository.Execute(base.Auth, endpoint, ct);
	}
}
