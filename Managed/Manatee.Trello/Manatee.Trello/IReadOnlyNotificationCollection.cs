using System.Collections;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface IReadOnlyNotificationCollection : IReadOnlyCollection<INotification>, IEnumerable<INotification>, IEnumerable, IRefreshable
{
	void Filter(NotificationType filter);

	void Filter(IEnumerable<NotificationType> filters);
}
