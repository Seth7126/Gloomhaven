using System.Collections.Generic;
using System.Linq;

namespace AsmodeeNet.Foundation;

public class NotificationCenter
{
	private class NotificationInvoker
	{
		public NotificationEntry entry;

		public object subject;

		public object observer;

		public NotificationHandler action;

		public NotificationInvoker(NotificationEntry entry, object subject, object observer, NotificationHandler action)
		{
			this.entry = entry;
			this.subject = subject;
			this.observer = observer;
			this.action = action;
		}

		public void Invoke(Notification notification)
		{
			action?.Invoke(notification);
		}
	}

	private class NotificationEntry
	{
		private Dictionary<object, List<NotificationInvoker>> _subjectToInvokers = new Dictionary<object, List<NotificationInvoker>>();

		private List<NotificationInvoker> _globalInvokers = new List<NotificationInvoker>();

		public string NotificationName { get; private set; }

		public NotificationEntry(string notificationName)
		{
			NotificationName = notificationName;
		}

		public bool IsEmpty()
		{
			if (!_subjectToInvokers.Any())
			{
				return !_globalInvokers.Any();
			}
			return false;
		}

		public void AddInvoker(NotificationInvoker invoker)
		{
			if (invoker.subject != null)
			{
				if (!_subjectToInvokers.TryGetValue(invoker.subject, out var value))
				{
					value = new List<NotificationInvoker>();
					_subjectToInvokers[invoker.subject] = value;
				}
				value.Add(invoker);
			}
			else
			{
				_globalInvokers.Add(invoker);
			}
		}

		public void RemoveInvoker(NotificationInvoker invoker)
		{
			if (invoker.subject != null)
			{
				if (_subjectToInvokers.TryGetValue(invoker.subject, out var value))
				{
					value.Remove(invoker);
					if (!value.Any())
					{
						_subjectToInvokers.Remove(invoker.subject);
					}
				}
			}
			else
			{
				_globalInvokers.Remove(invoker);
			}
		}

		public void PostNotification(Notification notification, object subject)
		{
			if (subject != null && _subjectToInvokers.TryGetValue(subject, out var value))
			{
				foreach (NotificationInvoker item in new List<NotificationInvoker>(value))
				{
					item.Invoke(notification);
				}
			}
			foreach (NotificationInvoker item2 in new List<NotificationInvoker>(_globalInvokers))
			{
				item2.Invoke(notification);
			}
		}
	}

	private Dictionary<string, NotificationEntry> _notificationNameToEntry = new Dictionary<string, NotificationEntry>();

	private Dictionary<object, List<NotificationInvoker>> _observerToInvokers = new Dictionary<object, List<NotificationInvoker>>();

	public void AddObserver(object observer, string notificationName, NotificationHandler action, object subject = null)
	{
		if (!_notificationNameToEntry.TryGetValue(notificationName, out var value))
		{
			value = new NotificationEntry(notificationName);
			_notificationNameToEntry[notificationName] = value;
		}
		NotificationInvoker notificationInvoker = new NotificationInvoker(value, subject, observer, action);
		value.AddInvoker(notificationInvoker);
		if (!_observerToInvokers.ContainsKey(observer))
		{
			_observerToInvokers.Add(observer, new List<NotificationInvoker>());
		}
		_observerToInvokers[observer].Add(notificationInvoker);
	}

	public void RemoveObserver(object observer)
	{
		if (!_observerToInvokers.TryGetValue(observer, out var value))
		{
			return;
		}
		foreach (NotificationInvoker item in value)
		{
			item.entry.RemoveInvoker(item);
			if (item.entry.IsEmpty())
			{
				_notificationNameToEntry.Remove(item.entry.NotificationName);
			}
		}
		_observerToInvokers.Remove(observer);
	}

	public void RemoveObserver(object observer, string notificationName, object subject = null)
	{
		if (!_observerToInvokers.TryGetValue(observer, out var value))
		{
			return;
		}
		List<NotificationInvoker> list = new List<NotificationInvoker>();
		foreach (NotificationInvoker item in value)
		{
			if ((subject == null || item.subject == subject) && item.entry != null && item.entry.NotificationName == notificationName)
			{
				item.entry.RemoveInvoker(item);
				if (item.entry.IsEmpty())
				{
					_notificationNameToEntry.Remove(item.entry.NotificationName);
				}
				list.Add(item);
			}
		}
		foreach (NotificationInvoker item2 in list)
		{
			value.Remove(item2);
		}
		if (!value.Any())
		{
			_observerToInvokers.Remove(observer);
		}
	}

	public void RemoveAllObservers()
	{
		_notificationNameToEntry.Clear();
		_observerToInvokers.Clear();
	}

	public bool HasObserver(object observer, string notificationName, object subject = null)
	{
		if (_observerToInvokers.TryGetValue(observer, out var value))
		{
			foreach (NotificationInvoker item in value)
			{
				if ((subject == null || item.subject == subject) && item.entry != null && item.entry.NotificationName == notificationName)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PostNotification(string notificationName, object subject = null)
	{
		PostNotification(new Notification(notificationName, subject));
	}

	public void PostNotification(Notification notification)
	{
		if (_notificationNameToEntry.TryGetValue(notification.Name, out var value))
		{
			value.PostNotification(notification, notification.Subject);
		}
	}
}
