using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyOrganizationMembershipCollection : ReadOnlyCollection<IOrganizationMembership>, IReadOnlyOrganizationMembershipCollection, IReadOnlyCollection<IOrganizationMembership>, IEnumerable<IOrganizationMembership>, IEnumerable, IRefreshable
{
	public IOrganizationMembership this[string key] => GetByKey(key);

	internal ReadOnlyOrganizationMembershipCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		base.AdditionalParameters["fields"] = "all";
	}

	public void Filter(MembershipFilter filter)
	{
		IEnumerable<MembershipFilter> filters = filter.GetFlags().Cast<MembershipFilter>();
		Filter(filters);
	}

	public void Filter(IEnumerable<MembershipFilter> filters)
	{
		string text = (string)base.AdditionalParameters["filter"];
		if (!text.IsNullOrWhiteSpace())
		{
			text += ",";
		}
		text += filters.Select((MembershipFilter a) => a.GetDescription()).Join(",");
		base.AdditionalParameters["filter"] = text;
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Organization_Read_Memberships, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonOrganizationMembership> source = await JsonRepository.Execute<List<IJsonOrganizationMembership>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		base.Items.AddRange(source.Select(delegate(IJsonOrganizationMembership jom)
		{
			OrganizationMembership obj = TrelloConfiguration.Cache.Find<OrganizationMembership>(jom.Id) ?? new OrganizationMembership(jom, base.OwnerId, base.Auth);
			obj.Json = jom;
			return obj;
		}));
	}

	private IOrganizationMembership GetByKey(string key)
	{
		return this.FirstOrDefault((IOrganizationMembership bm) => key.In(bm.Id, bm.Member.Id, bm.Member.FullName, bm.Member.UserName));
	}
}
