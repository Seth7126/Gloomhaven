using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class OrganizationCollection : ReadOnlyOrganizationCollection, IOrganizationCollection, IReadOnlyOrganizationCollection, IReadOnlyCollection<IOrganization>, IEnumerable<IOrganization>, IEnumerable, IRefreshable
{
	internal OrganizationCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<IOrganization> Add(string displayName, string description = null, string name = null, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullOrWhiteSpaceRule.Instance.Validate(null, displayName);
		if (text != null)
		{
			throw new ValidationException<string>(displayName, new string[1] { text });
		}
		if (name != null)
		{
			text = OrganizationNameRule.Instance.Validate(null, name);
			if (text != null)
			{
				throw new ValidationException<string>(name, new string[1] { text });
			}
		}
		IJsonOrganization jsonOrganization = TrelloConfiguration.JsonFactory.Create<IJsonOrganization>();
		jsonOrganization.DisplayName = displayName;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Write_CreateOrganization);
		return new Organization(await JsonRepository.Execute(base.Auth, endpoint, jsonOrganization, ct), base.Auth);
	}
}
