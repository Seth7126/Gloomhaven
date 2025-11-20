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

public class ReadOnlyMemberCollection : ReadOnlyCollection<IMember>, IReadOnlyMemberCollection, IReadOnlyCollection<IMember>, IEnumerable<IMember>, IEnumerable, IRefreshable, IHandle<EntityUpdatedEvent<IJsonMember>>, IHandle
{
	private readonly EntityRequestType _updateRequestType;

	public IMember this[string key] => GetByKey(key);

	internal ReadOnlyMemberCollection(EntityRequestType requestType, Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		_updateRequestType = requestType;
		EventAggregator.Subscribe(this);
	}

	public void Filter(MemberFilter filter)
	{
		IEnumerable<MemberFilter> filters = filter.GetFlags().Cast<MemberFilter>();
		Filter(filters);
	}

	public void Filter(IEnumerable<MemberFilter> filters)
	{
		string text = (base.AdditionalParameters.ContainsKey("filter") ? ((string)base.AdditionalParameters["filter"]) : string.Empty);
		if (!text.IsNullOrWhiteSpace())
		{
			text += ",";
		}
		text += filters.Select((MemberFilter a) => a.GetDescription()).Join(",");
		base.AdditionalParameters["filter"] = text;
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(MemberContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(_updateRequestType, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonMember> source = await JsonRepository.Execute<List<IJsonMember>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		EventAggregator.Unsubscribe(this);
		base.Items.AddRange(source.Select(delegate(IJsonMember jm)
		{
			Member fromCache = jm.GetFromCache<Member, IJsonMember>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jm;
			return fromCache;
		}));
		EventAggregator.Subscribe(this);
	}

	private IMember GetByKey(string key)
	{
		return this.FirstOrDefault((IMember m) => key.In(m.Id, m.FullName, m.UserName));
	}

	void IHandle<EntityUpdatedEvent<IJsonMember>>.Handle(EntityUpdatedEvent<IJsonMember> message)
	{
		switch (_updateRequestType)
		{
		case EntityRequestType.Board_Read_Members:
			if (message.Properties.Contains("Members"))
			{
				IMember member = base.Items.FirstOrDefault((IMember b) => b.Id == message.Data.Id);
				List<string> list3 = message.Data.Boards.Select((IJsonBoard m) => m.Id).ToList();
				if (!list3.Contains(base.OwnerId) && member != null)
				{
					base.Items.Remove(member);
				}
				else if (list3.Contains(base.OwnerId) && member == null)
				{
					base.Items.Add(message.Data.GetFromCache<Member>(base.Auth));
				}
			}
			break;
		case EntityRequestType.Card_Read_Members:
			if (message.Properties.Contains("Members"))
			{
				IMember member = base.Items.FirstOrDefault((IMember b) => b.Id == message.Data.Id);
				List<string> list2 = message.Data.Cards.Select((IJsonCard m) => m.Id).ToList();
				if (!list2.Contains(base.OwnerId) && member != null)
				{
					base.Items.Remove(member);
				}
				else if (list2.Contains(base.OwnerId) && member == null)
				{
					base.Items.Add(message.Data.GetFromCache<Member>(base.Auth));
				}
			}
			break;
		case EntityRequestType.Organization_Read_Members:
			if (message.Properties.Contains("Members"))
			{
				IMember member = base.Items.FirstOrDefault((IMember b) => b.Id == message.Data.Id);
				List<string> list = message.Data.Organizations.Select((IJsonOrganization m) => m.Id).ToList();
				if (!list.Contains(base.OwnerId) && member != null)
				{
					base.Items.Remove(member);
				}
				else if (list.Contains(base.OwnerId) && member == null)
				{
					base.Items.Add(message.Data.GetFromCache<Member>(base.Auth));
				}
			}
			break;
		}
	}
}
