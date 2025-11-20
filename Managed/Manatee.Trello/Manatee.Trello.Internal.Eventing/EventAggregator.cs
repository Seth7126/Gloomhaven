using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Manatee.Trello.Internal.Eventing;

internal static class EventAggregator
{
	private class Handler
	{
		private readonly WeakReference _reference;

		private readonly Dictionary<Type, MethodInfo> _supportedHandlers = new Dictionary<Type, MethodInfo>();

		public Handler(object handler)
		{
			_reference = new WeakReference(handler);
			foreach (Type item in from x in handler.GetType().GetInterfaces()
				where typeof(IHandle).IsAssignableFrom(x) && x.IsGenericType()
				select x)
			{
				Type type = item.GetGenericArguments().First();
				MethodInfo method = item.GetMethod("Handle", type);
				if (method != null)
				{
					_supportedHandlers[type] = method;
				}
			}
		}

		public bool Matches(object instance)
		{
			return _reference.Target == instance;
		}

		public bool Handle(Type messageType, object message)
		{
			object target = _reference.Target;
			if (target == null)
			{
				return false;
			}
			foreach (KeyValuePair<Type, MethodInfo> supportedHandler in _supportedHandlers)
			{
				if (supportedHandler.Key.IsAssignableFrom(messageType))
				{
					supportedHandler.Value.Invoke(target, new object[1] { message });
				}
			}
			return true;
		}
	}

	private static readonly List<Handler> Handlers = new List<Handler>();

	public static void Subscribe(IHandle subscriber)
	{
		if (!TrelloConfiguration.EnableConsistencyProcessing)
		{
			return;
		}
		if (subscriber == null)
		{
			throw new ArgumentNullException("subscriber");
		}
		lock (Handlers)
		{
			if (!Handlers.Any((Handler x) => x.Matches(subscriber)))
			{
				Handlers.Add(new Handler(subscriber));
			}
		}
	}

	public static void Unsubscribe(IHandle subscriber)
	{
		if (!TrelloConfiguration.EnableConsistencyProcessing)
		{
			return;
		}
		if (subscriber == null)
		{
			throw new ArgumentNullException("subscriber");
		}
		lock (Handlers)
		{
			Handler handler = Handlers.FirstOrDefault((Handler x) => x.Matches(subscriber));
			if (handler != null)
			{
				Handlers.Remove(handler);
			}
		}
	}

	public static void Publish(object message)
	{
		if (!TrelloConfiguration.EnableConsistencyProcessing)
		{
			return;
		}
		if (message == null)
		{
			throw new ArgumentNullException("message");
		}
		Handler[] source;
		lock (Handlers)
		{
			source = Handlers.ToArray();
		}
		Type messageType = message.GetType();
		List<Handler> list = source.Where((Handler handler) => !handler.Handle(messageType, message)).ToList();
		if (!list.Any())
		{
			return;
		}
		lock (Handlers)
		{
			list.Apply(delegate(Handler x)
			{
				Handlers.Remove(x);
			});
		}
	}
}
