using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class StarredBoardCollection : ReadOnlyStarredBoardCollection, IStarredBoardCollection, IReadOnlyCollection<IStarredBoard>, IEnumerable<IStarredBoard>, IEnumerable, IRefreshable
{
	internal StarredBoardCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<IStarredBoard> Add(IBoard board, Position position = null, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<IBoard>.Instance.Validate(null, board);
		if (text != null)
		{
			throw new ValidationException<IBoard>(board, new string[1] { text });
		}
		position = position ?? Position.Bottom;
		IJsonStarredBoard jsonStarredBoard = TrelloConfiguration.JsonFactory.Create<IJsonStarredBoard>();
		jsonStarredBoard.Board = TrelloConfiguration.JsonFactory.Create<IJsonBoard>();
		jsonStarredBoard.Board.Id = board.Id;
		jsonStarredBoard.Pos = Position.GetJson(position);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Write_AddStarredBoard, new Dictionary<string, object> { { "_id", base.OwnerId } });
		IJsonStarredBoard json = await JsonRepository.Execute(base.Auth, endpoint, jsonStarredBoard, ct);
		return new StarredBoard(base.OwnerId, json, base.Auth);
	}
}
