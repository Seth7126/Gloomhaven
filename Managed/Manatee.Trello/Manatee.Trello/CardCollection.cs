using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CardCollection : ReadOnlyCardCollection, ICardCollection, IReadOnlyCardCollection, IReadOnlyCollection<ICard>, IEnumerable<ICard>, IEnumerable, IRefreshable
{
	internal CardCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(typeof(List), getOwnerId, auth)
	{
	}

	public async Task<ICard> Add(string name, string description = null, Position position = null, DateTime? dueDate = null, bool? isComplete = null, IEnumerable<IMember> members = null, IEnumerable<ILabel> labels = null, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullOrWhiteSpaceRule.Instance.Validate(null, name);
		if (text != null)
		{
			throw new ValidationException<string>(name, new string[1] { text });
		}
		IJsonCard jsonCard = TrelloConfiguration.JsonFactory.Create<IJsonCard>();
		jsonCard.Name = name;
		jsonCard.Desc = description;
		jsonCard.Pos = ((position == null) ? null : Position.GetJson(position));
		jsonCard.Due = dueDate;
		jsonCard.DueComplete = isComplete;
		jsonCard.IdMembers = members?.Select((IMember m) => m.Id).Join(",");
		jsonCard.IdLabels = labels?.Select((ILabel l) => l.Id).Join(",");
		return await CreateCard(jsonCard, ct);
	}

	public async Task<ICard> Add(ICard source, CardCopyKeepFromSourceOptions keep = CardCopyKeepFromSourceOptions.None, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<ICard>.Instance.Validate(null, source);
		if (text != null)
		{
			throw new ValidationException<ICard>(source, new string[1] { text });
		}
		IJsonCard jsonCard = TrelloConfiguration.JsonFactory.Create<IJsonCard>();
		jsonCard.CardSource = ((Card)source).Json;
		jsonCard.KeepFromSource = keep;
		return await CreateCard(jsonCard, ct);
	}

	public async Task<ICard> Add(string name, string sourceUrl, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullOrWhiteSpaceRule.Instance.Validate(null, name);
		if (text != null)
		{
			throw new ValidationException<string>(name, new string[1] { text });
		}
		text = UriRule.Instance.Validate(null, sourceUrl);
		if (text != null)
		{
			throw new ValidationException<string>(sourceUrl, new string[1] { text });
		}
		IJsonCard jsonCard = TrelloConfiguration.JsonFactory.Create<IJsonCard>();
		jsonCard.Name = name;
		jsonCard.UrlSource = sourceUrl;
		return await CreateCard(jsonCard, ct);
	}

	private async Task<Card> CreateCard(IJsonCard json, CancellationToken ct)
	{
		json.List = TrelloConfiguration.JsonFactory.Create<IJsonList>();
		json.List.Id = base.OwnerId;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.List_Write_AddCard);
		return new Card(await JsonRepository.Execute(base.Auth, endpoint, json, ct), base.Auth);
	}
}
