using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class SearchContext : SynchronizationContext<IJsonSearch>
{
	private static readonly Dictionary<Type, Func<ICacheable, IJsonCacheable>> JsonExtraction;

	static SearchContext()
	{
		JsonExtraction = new Dictionary<Type, Func<ICacheable, IJsonCacheable>>
		{
			{
				typeof(Action),
				(ICacheable o) => ((Action)o).Json
			},
			{
				typeof(Board),
				(ICacheable o) => ((Board)o).Json
			},
			{
				typeof(Card),
				(ICacheable o) => ((Card)o).Json
			},
			{
				typeof(List),
				(ICacheable o) => ((List)o).Json
			},
			{
				typeof(Member),
				(ICacheable o) => ((Member)o).Json
			},
			{
				typeof(Organization),
				(ICacheable o) => ((Organization)o).Json
			}
		};
		SynchronizationContext<IJsonSearch>.Properties = new Dictionary<string, Property<IJsonSearch>>
		{
			{
				"Actions",
				new Property<IJsonSearch, IEnumerable<Action>>(delegate(IJsonSearch d, TrelloAuthorization a)
				{
					IEnumerable<Action> enumerable = d.Actions?.Select((IJsonAction j) => j.GetFromCache<Action, IJsonAction>(a, overwrite: true, Array.Empty<object>())).ToList();
					return enumerable ?? Enumerable.Empty<Action>();
				}, delegate(IJsonSearch d, IEnumerable<Action> o)
				{
					d.Actions = o?.Select((Action a) => a.Json).ToList();
				})
			},
			{
				"Boards",
				new Property<IJsonSearch, IEnumerable<Board>>(delegate(IJsonSearch d, TrelloAuthorization a)
				{
					IEnumerable<Board> enumerable = d.Boards?.Select((IJsonBoard j) => j.GetFromCache<Board, IJsonBoard>(a, overwrite: true, Array.Empty<object>())).ToList();
					return enumerable ?? Enumerable.Empty<Board>();
				}, delegate(IJsonSearch d, IEnumerable<Board> o)
				{
					d.Boards = o?.Select((Board a) => a.Json).ToList();
				})
			},
			{
				"Cards",
				new Property<IJsonSearch, IEnumerable<Card>>(delegate(IJsonSearch d, TrelloAuthorization a)
				{
					IEnumerable<Card> enumerable = d.Cards?.Select((IJsonCard j) => j.GetFromCache<Card, IJsonCard>(a, overwrite: true, Array.Empty<object>())).ToList();
					return enumerable ?? Enumerable.Empty<Card>();
				}, delegate(IJsonSearch d, IEnumerable<Card> o)
				{
					d.Cards = o?.Select((Card a) => a.Json).ToList();
				})
			},
			{
				"Members",
				new Property<IJsonSearch, IEnumerable<Member>>(delegate(IJsonSearch d, TrelloAuthorization a)
				{
					IEnumerable<Member> enumerable = d.Members?.Select((IJsonMember j) => j.GetFromCache<Member, IJsonMember>(a, overwrite: true, Array.Empty<object>())).ToList();
					return enumerable ?? Enumerable.Empty<Member>();
				}, delegate(IJsonSearch d, IEnumerable<Member> o)
				{
					d.Members = o?.Select((Member a) => a.Json).ToList();
				})
			},
			{
				"Organizations",
				new Property<IJsonSearch, IEnumerable<Organization>>(delegate(IJsonSearch d, TrelloAuthorization a)
				{
					IEnumerable<Organization> enumerable = d.Organizations?.Select((IJsonOrganization j) => j.GetFromCache<Organization, IJsonOrganization>(a, overwrite: true, Array.Empty<object>())).ToList();
					return enumerable ?? Enumerable.Empty<Organization>();
				}, delegate(IJsonSearch d, IEnumerable<Organization> o)
				{
					d.Organizations = o?.Select((Organization a) => a.Json).ToList();
				})
			},
			{
				"Query",
				new Property<IJsonSearch, string>((IJsonSearch d, TrelloAuthorization a) => d.Query, delegate(IJsonSearch d, string o)
				{
					if (!o.IsNullOrWhiteSpace())
					{
						d.Query = o;
					}
				})
			},
			{
				"Context",
				new Property<IJsonSearch, List<IQueryable>>((IJsonSearch d, TrelloAuthorization a) => d.Context?.Select((IJsonCacheable j) => j.GetFromCache(a)).Cast<IQueryable>().ToList(), delegate(IJsonSearch d, List<IQueryable> o)
				{
					if (o != null)
					{
						d.Context = o.Select(ExtractData).ToList();
					}
				})
			},
			{
				"Types",
				new Property<IJsonSearch, SearchModelType?>((IJsonSearch d, TrelloAuthorization a) => d.Types, delegate(IJsonSearch d, SearchModelType? o)
				{
					if (o != (SearchModelType)0)
					{
						d.Types = o;
					}
				})
			},
			{
				"Limit",
				new Property<IJsonSearch, int?>((IJsonSearch d, TrelloAuthorization a) => d.Limit, delegate(IJsonSearch d, int? o)
				{
					if (o != 0)
					{
						d.Limit = o;
					}
				})
			},
			{
				"IsPartial",
				new Property<IJsonSearch, bool?>((IJsonSearch d, TrelloAuthorization a) => d.Partial, delegate(IJsonSearch d, bool? o)
				{
					if (o.HasValue)
					{
						d.Partial = o.Value;
					}
				})
			}
		};
	}

	public SearchContext(TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
	}

	protected override async Task<IJsonSearch> GetData(CancellationToken ct)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{
				"query",
				base.Data.Query
			},
			{
				"modelTypes",
				base.Data.Types.FlagsEnumToCommaList()
			}
		};
		if (base.Data.Context != null)
		{
			TryAddContext<IJsonCard>(dictionary, "idCards");
			TryAddContext<IJsonBoard>(dictionary, "idBoards");
			TryAddContext<IJsonOrganization>(dictionary, "idOrganizations");
		}
		if (base.Data.Limit.HasValue)
		{
			dictionary.Add("boards_limit", base.Data.Limit);
			dictionary.Add("cards_limit", base.Data.Limit);
			dictionary.Add("organizations_limit", base.Data.Limit);
			dictionary.Add("members_limit", base.Data.Limit);
		}
		if (base.Data.Partial)
		{
			dictionary.Add("partial", base.Data.Partial.ToLowerString());
		}
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Service_Read_Search);
		return await JsonRepository.Execute<IJsonSearch>(base.Auth, endpoint, ct, dictionary);
	}

	private static IJsonCacheable ExtractData(ICacheable obj)
	{
		return JsonExtraction[obj.GetType()](obj);
	}

	private void TryAddContext<T>(Dictionary<string, object> json, string key) where T : IJsonCacheable
	{
		string value = (from o in base.Data.Context.OfType<T>()
			select o.Id).Take(24).Join(",");
		if (!value.IsNullOrWhiteSpace())
		{
			json[key] = value;
		}
	}
}
