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

public class OrganizationMembershipCollection : ReadOnlyOrganizationMembershipCollection, IOrganizationMembershipCollection, IReadOnlyOrganizationMembershipCollection, IReadOnlyCollection<IOrganizationMembership>, IEnumerable<IOrganizationMembership>, IEnumerable, IRefreshable
{
	internal OrganizationMembershipCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public async Task<IOrganizationMembership> Add(IMember member, OrganizationMembershipType membership, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<IMember>.Instance.Validate(null, member);
		if (text != null)
		{
			throw new ValidationException<IMember>(member, new string[1] { text });
		}
		IJsonOrganizationMembership jsonOrganizationMembership = TrelloConfiguration.JsonFactory.Create<IJsonOrganizationMembership>();
		jsonOrganizationMembership.Member = ((Member)member).Json;
		jsonOrganizationMembership.MemberType = membership;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Organization_Write_AddOrUpdateMember, new Dictionary<string, object>
		{
			{ "_id", base.OwnerId },
			{ "_memberId", member.Id }
		});
		IJsonOrganizationMembership json = await JsonRepository.Execute(base.Auth, endpoint, jsonOrganizationMembership, ct);
		if (TrelloConfiguration.EnableConsistencyProcessing && member.Organizations is ReadOnlyCollection<IOrganization> readOnlyCollection)
		{
			IOrganization organization = TrelloConfiguration.Cache.OfType<IOrganization>().FirstOrDefault((IOrganization o) => o.Id == base.OwnerId);
			if (organization != null)
			{
				readOnlyCollection.Items.Add(organization);
			}
		}
		return new OrganizationMembership(json, base.OwnerId, base.Auth);
	}

	public async Task Remove(IMember member, CancellationToken ct = default(CancellationToken))
	{
		string text = NotNullRule<IMember>.Instance.Validate(null, member);
		if (text != null)
		{
			throw new ValidationException<IMember>(member, new string[1] { text });
		}
		IJsonParameter jsonParameter = TrelloConfiguration.JsonFactory.Create<IJsonParameter>();
		jsonParameter.String = member.Id;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Organization_Write_RemoveMember, new Dictionary<string, object>
		{
			{ "_id", base.OwnerId },
			{ "_memberId", member.Id }
		});
		await JsonRepository.Execute(base.Auth, endpoint, jsonParameter, ct);
		if (TrelloConfiguration.EnableConsistencyProcessing && member.Organizations is ReadOnlyCollection<IOrganization> readOnlyCollection)
		{
			IOrganization organization = TrelloConfiguration.Cache.OfType<IOrganization>().FirstOrDefault((IOrganization o) => o.Id == base.OwnerId);
			if (organization != null)
			{
				readOnlyCollection.Items.Remove(organization);
			}
		}
	}
}
