using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyCustomFieldCollection : ReadOnlyCollection<ICustomField>
{
	internal ReadOnlyCustomFieldCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Card_Read_CustomFields, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonCustomField> source = await JsonRepository.Execute<List<IJsonCustomField>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		base.Items.AddRange(source.Select(delegate(IJsonCustomField ja)
		{
			CustomField fromCache = ja.GetFromCache<CustomField, IJsonCustomField>(base.Auth, overwrite: true, new object[1] { base.OwnerId });
			fromCache.Json = ja;
			return fromCache;
		}));
	}
}
