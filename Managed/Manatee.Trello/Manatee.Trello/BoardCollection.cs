using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class BoardCollection : ReadOnlyBoardCollection, IBoardCollection, IReadOnlyBoardCollection, IReadOnlyCollection<IBoard>, IEnumerable<IBoard>, IEnumerable, IRefreshable
{
	private readonly EntityRequestType _addRequestType;

	internal BoardCollection(Type type, Func<string> getOwnerId, TrelloAuthorization auth)
		: base(type, getOwnerId, auth)
	{
		_addRequestType = ((type == typeof(Organization)) ? EntityRequestType.Organization_Write_CreateBoard : EntityRequestType.Member_Write_CreateBoard);
	}

	public async Task<IBoard> Add(string name, string description = null, IBoard source = null, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullOrWhiteSpaceRule.Instance.Validate(null, name);
		if (text != null)
		{
			throw new ValidationException<string>(name, new string[1] { text });
		}
		IJsonBoard jsonBoard = TrelloConfiguration.JsonFactory.Create<IJsonBoard>();
		jsonBoard.Name = name;
		jsonBoard.Desc = description;
		if (source != null)
		{
			jsonBoard.BoardSource = ((Board)source).Json;
		}
		if (_addRequestType == EntityRequestType.Organization_Write_CreateBoard)
		{
			jsonBoard.Organization = TrelloConfiguration.JsonFactory.Create<IJsonOrganization>();
			jsonBoard.Organization.Id = base.OwnerId;
		}
		Endpoint endpoint = EndpointFactory.Build(_addRequestType);
		return new Board(await JsonRepository.Execute(base.Auth, endpoint, jsonBoard, ct), base.Auth);
	}
}
