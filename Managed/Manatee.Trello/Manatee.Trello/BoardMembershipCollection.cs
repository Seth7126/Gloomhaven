using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class BoardMembershipCollection : ReadOnlyBoardMembershipCollection, IBoardMembershipCollection, IReadOnlyBoardMembershipCollection, IReadOnlyCollection<IBoardMembership>, IEnumerable<IBoardMembership>, IEnumerable, IRefreshable
{
	internal BoardMembershipCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<IBoardMembership> Add(IMember member, BoardMembershipType membership, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<IMember>.Instance.Validate(null, member);
		if (text != null)
		{
			throw new ValidationException<IMember>(member, new string[1] { text });
		}
		IJsonBoardMembership jsonBoardMembership = TrelloConfiguration.JsonFactory.Create<IJsonBoardMembership>();
		jsonBoardMembership.Member = ((Member)member).Json;
		jsonBoardMembership.MemberType = membership;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_AddOrUpdateMember, new Dictionary<string, object>
		{
			{ "_id", base.OwnerId },
			{ "_memberId", member.Id }
		});
		IJsonBoardMembership json = await JsonRepository.Execute(base.Auth, endpoint, jsonBoardMembership, ct);
		if (TrelloConfiguration.EnableConsistencyProcessing && member.Boards is ReadOnlyCollection<IBoard> readOnlyCollection)
		{
			IBoard board = TrelloConfiguration.Cache.OfType<IBoard>().FirstOrDefault((IBoard b) => b.Id == base.OwnerId);
			if (board != null)
			{
				readOnlyCollection.Items.Add(board);
			}
		}
		return new BoardMembership(json, base.OwnerId, base.Auth);
	}

	public async Task Remove(IMember member, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<IMember>.Instance.Validate(null, member);
		if (text != null)
		{
			throw new ValidationException<IMember>(member, new string[1] { text });
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_RemoveMember, new Dictionary<string, object>
		{
			{ "_id", base.OwnerId },
			{ "_memberId", member.Id }
		});
		await JsonRepository.Execute(base.Auth, endpoint, ct);
		if (TrelloConfiguration.EnableConsistencyProcessing && member.Boards is ReadOnlyCollection<IBoard> readOnlyCollection)
		{
			IBoard board = TrelloConfiguration.Cache.OfType<IBoard>().FirstOrDefault((IBoard b) => b.Id == base.OwnerId);
			if (board != null)
			{
				readOnlyCollection.Items.Remove(board);
			}
		}
	}
}
