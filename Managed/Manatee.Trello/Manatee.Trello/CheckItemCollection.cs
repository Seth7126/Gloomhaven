using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class CheckItemCollection : ReadOnlyCheckItemCollection, ICheckItemCollection, IReadOnlyCheckItemCollection, IReadOnlyCollection<ICheckItem>, IEnumerable<ICheckItem>, IEnumerable, IRefreshable
{
	internal CheckItemCollection(CheckListContext context, TrelloAuthorization auth)
		: base(context, auth)
	{
	}

	public async Task<ICheckItem> Add(string name, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullOrWhiteSpaceRule.Instance.Validate(null, name);
		if (text != null)
		{
			throw new ValidationException<string>(name, new string[1] { text });
		}
		IJsonCheckItem jsonCheckItem = TrelloConfiguration.JsonFactory.Create<IJsonCheckItem>();
		jsonCheckItem.Name = name;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.CheckList_Write_AddCheckItem, new Dictionary<string, object> { { "_id", base.OwnerId } });
		return new CheckItem(await JsonRepository.Execute(base.Auth, endpoint, jsonCheckItem, ct), base.OwnerId);
	}
}
