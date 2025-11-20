using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CheckListCollection : ReadOnlyCheckListCollection, ICheckListCollection, IReadOnlyCheckListCollection, IReadOnlyCollection<ICheckList>, IEnumerable<ICheckList>, IEnumerable, IRefreshable
{
	internal CheckListCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<ICheckList> Add(string name, ICheckList source = null, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullOrWhiteSpaceRule.Instance.Validate(null, name);
		if (text != null)
		{
			throw new ValidationException<string>(name, new string[1] { text });
		}
		IJsonCheckList jsonCheckList = TrelloConfiguration.JsonFactory.Create<IJsonCheckList>();
		jsonCheckList.Name = name;
		IJsonCard jsonCard = TrelloConfiguration.JsonFactory.Create<IJsonCard>();
		jsonCard.Id = base.OwnerId;
		jsonCheckList.Card = jsonCard;
		if (source != null)
		{
			jsonCheckList.CheckListSource = ((CheckList)source).Json;
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Write_AddChecklist);
		return new CheckList(await JsonRepository.Execute(base.Auth, endpoint, jsonCheckList, ct), base.Auth);
	}
}
