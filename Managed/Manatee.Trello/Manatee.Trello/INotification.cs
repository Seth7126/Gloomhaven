using System;
using System.Collections.Generic;

namespace Manatee.Trello;

public interface INotification : ICacheable, IRefreshable
{
	DateTime CreationDate { get; }

	IMember Creator { get; }

	INotificationData Data { get; }

	DateTime? Date { get; }

	bool? IsUnread { get; set; }

	NotificationType? Type { get; }

	event Action<INotification, IEnumerable<string>> Updated;
}
