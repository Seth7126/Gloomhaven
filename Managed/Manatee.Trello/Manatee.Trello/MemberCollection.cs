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

public class MemberCollection : ReadOnlyMemberCollection, IMemberCollection, IReadOnlyMemberCollection, IReadOnlyCollection<IMember>, IEnumerable<IMember>, IEnumerable, IRefreshable
{
	internal MemberCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(EntityRequestType.Card_Read_Members, getOwnerId, auth)
	{
	}

	public async Task Add(IMember member, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<IMember>.Instance.Validate(null, member);
		if (text != null)
		{
			throw new ValidationException<IMember>(member, new string[1] { text });
		}
		IJsonParameter jsonParameter = TrelloConfiguration.JsonFactory.Create<IJsonParameter>();
		jsonParameter.String = member.Id;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_AssignMember, new Dictionary<string, object> { { "_id", base.OwnerId } });
		await JsonRepository.Execute(base.Auth, endpoint, jsonParameter, ct);
		base.Items.Add(member);
		if (TrelloConfiguration.EnableConsistencyProcessing && member.Cards is ReadOnlyCollection<ICard> readOnlyCollection)
		{
			ICard card = TrelloConfiguration.Cache.OfType<ICard>().FirstOrDefault((ICard c) => c.Id == base.OwnerId);
			if (card != null)
			{
				readOnlyCollection.Items.Add(card);
			}
		}
	}

	public async Task Remove(IMember member, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<IMember>.Instance.Validate(null, member);
		if (text != null)
		{
			throw new ValidationException<IMember>(member, new string[1] { text });
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_RemoveMember, new Dictionary<string, object>
		{
			{ "_id", base.OwnerId },
			{ "_memberId", member.Id }
		});
		await JsonRepository.Execute(base.Auth, endpoint, ct);
		base.Items.Remove(member);
		if (TrelloConfiguration.EnableConsistencyProcessing && member.Cards is ReadOnlyCollection<ICard> readOnlyCollection)
		{
			ICard card = TrelloConfiguration.Cache.OfType<ICard>().FirstOrDefault((ICard c) => c.Id == base.OwnerId);
			if (card != null)
			{
				readOnlyCollection.Items.Remove(card);
			}
		}
	}
}
