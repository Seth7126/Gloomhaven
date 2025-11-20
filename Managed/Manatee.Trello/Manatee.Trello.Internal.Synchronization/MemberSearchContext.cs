using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class MemberSearchContext : SynchronizationContext<IJsonMemberSearch>
{
	static MemberSearchContext()
	{
		SynchronizationContext<IJsonMemberSearch>.Properties = new Dictionary<string, Property<IJsonMemberSearch>>
		{
			{
				"Board",
				new Property<IJsonMemberSearch, Board>((IJsonMemberSearch d, TrelloAuthorization a) => d.Board?.GetFromCache<Board, IJsonBoard>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonMemberSearch d, Board o)
				{
					if (o != null)
					{
						d.Board = o.Json;
					}
				})
			},
			{
				"Limit",
				new Property<IJsonMemberSearch, int?>((IJsonMemberSearch d, TrelloAuthorization a) => d.Limit, delegate(IJsonMemberSearch d, int? o)
				{
					d.Limit = o;
				})
			},
			{
				"Results",
				new Property<IJsonMemberSearch, IEnumerable<MemberSearchResult>>(delegate(IJsonMemberSearch d, TrelloAuthorization a)
				{
					IEnumerable<MemberSearchResult> enumerable = d.Members?.Select((IJsonMember m) => GetResult(m, a)).ToList();
					return enumerable ?? Enumerable.Empty<MemberSearchResult>();
				}, delegate(IJsonMemberSearch d, IEnumerable<MemberSearchResult> o)
				{
					d.Members = o?.Select((MemberSearchResult a) => ((Member)a.Member).Json).ToList();
				})
			},
			{
				"Organization",
				new Property<IJsonMemberSearch, Organization>((IJsonMemberSearch d, TrelloAuthorization a) => d.Organization?.GetFromCache<Organization, IJsonOrganization>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonMemberSearch d, Organization o)
				{
					d.Organization = o?.Json;
				})
			},
			{
				"Query",
				new Property<IJsonMemberSearch, string>((IJsonMemberSearch d, TrelloAuthorization a) => d.Query, delegate(IJsonMemberSearch d, string o)
				{
					if (!o.IsNullOrWhiteSpace())
					{
						d.Query = o;
					}
				})
			},
			{
				"RestrictToOrganization",
				new Property<IJsonMemberSearch, bool?>((IJsonMemberSearch d, TrelloAuthorization a) => d.OnlyOrgMembers, delegate(IJsonMemberSearch d, bool? o)
				{
					d.OnlyOrgMembers = o;
				})
			}
		};
	}

	public MemberSearchContext(TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
	}

	protected override async Task<IJsonMemberSearch> GetData(CancellationToken ct)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object> { 
		{
			"query",
			base.Data.Query
		} };
		if (base.Data.Board != null)
		{
			dictionary.Add("idBoard", base.Data.Board.Id);
		}
		if (base.Data.Organization != null)
		{
			dictionary.Add("idOrganization", base.Data.Organization.Id);
			dictionary.Add("onlyOrgMembers", base.Data.OnlyOrgMembers);
		}
		if (base.Data.Limit.HasValue)
		{
			dictionary.Add("limit", base.Data.Limit);
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Service_Read_SearchMembers);
		return await JsonRepository.Execute<IJsonMemberSearch>(base.Auth, endpoint, ct, dictionary);
	}

	private static MemberSearchResult GetResult(IJsonMember json, TrelloAuthorization auth)
	{
		return new MemberSearchResult(json.GetFromCache<Member, IJsonMember>(auth, overwrite: true, Array.Empty<object>()), json.Similarity);
	}
}
