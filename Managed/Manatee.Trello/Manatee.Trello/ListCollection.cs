using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ListCollection : ReadOnlyListCollection, IListCollection, IReadOnlyListCollection, IReadOnlyCollection<IList>, IEnumerable<IList>, IEnumerable, IRefreshable
{
	[Obsolete("Trello does not support limiting lists.")]
	public override int? Limit { get; set; }

	internal ListCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<IList> Add(string name, Position position = null, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullOrWhiteSpaceRule.Instance.Validate(null, name);
		if (text != null)
		{
			throw new ValidationException<string>(name, new string[1] { text });
		}
		IJsonList jsonList = TrelloConfiguration.JsonFactory.Create<IJsonList>();
		jsonList.Name = name;
		jsonList.Board = TrelloConfiguration.JsonFactory.Create<IJsonBoard>();
		jsonList.Board.Id = base.OwnerId;
		jsonList.Pos = Position.GetJson(position);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_AddList);
		return new List(await JsonRepository.Execute(base.Auth, endpoint, jsonList, ct), base.Auth);
	}
}
