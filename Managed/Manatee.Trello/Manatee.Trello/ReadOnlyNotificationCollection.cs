using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyNotificationCollection : ReadOnlyCollection<INotification>, IReadOnlyNotificationCollection, IReadOnlyCollection<INotification>, IEnumerable<INotification>, IEnumerable, IRefreshable
{
	internal ReadOnlyNotificationCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	public void Filter(NotificationType filter)
	{
		IEnumerable<NotificationType> filters = filter.GetFlags().Cast<NotificationType>();
		Filter(filters);
	}

	public void Filter(IEnumerable<NotificationType> filters)
	{
		string text = (string)base.AdditionalParameters["filter"];
		if (!text.IsNullOrWhiteSpace())
		{
			text += ",";
		}
		text += filters.Select((NotificationType a) => a.GetDescription()).Join(",");
		base.AdditionalParameters["filter"] = text;
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		Dictionary<string, object> parameters = base.AdditionalParameters.Concat(NotificationContext.CurrentParameters).ToDictionary((KeyValuePair<string, object> kvp) => kvp.Key, (KeyValuePair<string, object> kvp) => kvp.Value);
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Read_Notifications, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonNotification> source = await JsonRepository.Execute<List<IJsonNotification>>(base.Auth, endpoint, ct, parameters);
		base.Items.Clear();
		base.Items.AddRange(source.Select(delegate(IJsonNotification jn)
		{
			Notification fromCache = jn.GetFromCache<Notification, IJsonNotification>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jn;
			return fromCache;
		}));
	}
}
