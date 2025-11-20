using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyOrganizationCollection : ReadOnlyCollection<IOrganization>, IReadOnlyOrganizationCollection, IReadOnlyCollection<IOrganization>, IEnumerable<IOrganization>, IEnumerable, IRefreshable, IHandle<EntityDeletedEvent<IJsonOrganization>>, IHandle
{
	public IOrganization this[string key] => GetByKey(key);

	internal ReadOnlyOrganizationCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	public void Filter(OrganizationFilter filter)
	{
		base.AdditionalParameters["filter"] = filter.GetDescription();
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(OrganizationContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Read_Organizations, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonOrganization> source = await JsonRepository.Execute<List<IJsonOrganization>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonOrganization jo)
		{
			Organization fromCache = jo.GetFromCache<Organization, IJsonOrganization>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jo;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	private IOrganization GetByKey(string key)
	{
		return this.FirstOrDefault((IOrganization o) => key.In(o.Id, o.Name, o.DisplayName));
	}

	void IHandle<EntityDeletedEvent<IJsonOrganization>>.Handle(EntityDeletedEvent<IJsonOrganization> message)
	{
		IOrganization item = base.Items.FirstOrDefault((IOrganization c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
