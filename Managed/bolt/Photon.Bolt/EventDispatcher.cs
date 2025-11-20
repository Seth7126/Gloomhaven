#define DEBUG
using System;
using System.Collections.Generic;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

internal class EventDispatcher
{
	private struct EventListener
	{
		public IEventListener Listener;

		public GameObject GameObject;

		public MonoBehaviour Behaviour;
	}

	private struct CallbackWrapper
	{
		public object Original;

		public Action<Event> Wrapper;
	}

	private List<EventListener> _targets = new List<EventListener>();

	private Dictionary<Type, List<CallbackWrapper>> _callbacks = new Dictionary<Type, List<CallbackWrapper>>();

	private static readonly Queue<Event> _dispatchQueue = new Queue<Event>();

	private void Raise(Event ev)
	{
		IEventFactory eventFactory = Factory.GetEventFactory(ev.Meta.TypeId);
		if (eventFactory == null)
		{
			return;
		}
		if (_callbacks.TryGetValue(ev.GetType(), out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].Wrapper(ev);
			}
		}
		for (int j = 0; j < _targets.Count; j++)
		{
			EventListener eventListener = _targets[j];
			if ((bool)eventListener.Behaviour)
			{
				if ((eventListener.Behaviour.enabled || (eventListener.Listener != null && eventListener.Listener.InvokeIfDisabled)) && (eventListener.GameObject.activeInHierarchy || (eventListener.Listener != null && eventListener.Listener.InvokeIfGameObjectIsInactive)))
				{
					try
					{
						eventFactory.Dispatch(ev, eventListener.Behaviour);
					}
					catch (Exception exception)
					{
						BoltLog.Error("User code threw exception when invoking {0}", ev);
						BoltLog.Exception(exception);
					}
				}
			}
			else
			{
				_targets.RemoveAt(j);
				j--;
			}
		}
	}

	public void Add<T>(Action<T> callback) where T : Event
	{
		if (!_callbacks.TryGetValue(typeof(T), out var value))
		{
			_callbacks.Add(typeof(T), value = new List<CallbackWrapper>());
		}
		CallbackWrapper item = default(CallbackWrapper);
		item.Original = callback;
		item.Wrapper = delegate(Event ev)
		{
			callback((T)ev);
		};
		value.Add(item);
	}

	public void Remove<T>(Action<T> callback) where T : Event
	{
		if (_callbacks.TryGetValue(typeof(T), out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				Action<T> action = (Action<T>)value[i].Original;
				if (action == callback)
				{
					value.RemoveAt(i);
					return;
				}
			}
		}
		BoltLog.Warn("Could not find delegate registered as callback");
	}

	public void Add(MonoBehaviour behaviour)
	{
		for (int i = 0; i < _targets.Count; i++)
		{
			if ((object)_targets[i].Behaviour == behaviour)
			{
				BoltLog.Warn("Behaviour is already registered in this dispatcher, ignoring call to Add.");
				return;
			}
		}
		_targets.Add(new EventListener
		{
			Behaviour = behaviour,
			GameObject = behaviour.gameObject,
			Listener = (behaviour as IEventListener)
		});
	}

	public void Remove(MonoBehaviour behaviour)
	{
		for (int i = 0; i < _targets.Count; i++)
		{
			if ((object)_targets[i].Behaviour == behaviour)
			{
				_targets.RemoveAt(i);
				return;
			}
		}
		BoltLog.Info("Behaviour not available in this dispatcher, ignoring call to Remove.");
	}

	public void Clear()
	{
		_callbacks = new Dictionary<Type, List<CallbackWrapper>>();
		for (int i = 0; i < _targets.Count; i++)
		{
			GlobalEventListenerBase globalEventListenerBase = _targets[i].Behaviour as GlobalEventListenerBase;
			if (!(globalEventListenerBase != null) || !globalEventListenerBase.PersistBetweenStartupAndShutdown())
			{
				_targets.RemoveAt(i);
				i--;
			}
		}
	}

	public static void Enqueue(Event ev)
	{
		_dispatchQueue.Enqueue(ev);
	}

	public static void Received(Event ev)
	{
		if (BoltCore.EventFilter.EventReceived(ev))
		{
			_dispatchQueue.Enqueue(ev);
		}
	}

	public static void DispatchAllEvents()
	{
		while (_dispatchQueue.Count > 0)
		{
			Dispatch(_dispatchQueue.Dequeue());
		}
	}

	private static void Dispatch(Event ev)
	{
		try
		{
			ev.IncrementRefs();
			switch (ev.Targets)
			{
			case 1:
				Entity_Everyone(ev);
				break;
			case 5:
				Entity_Everyone_Except_Controller(ev);
				break;
			case 3:
				Entity_Everyone_Except_Owner(ev);
				break;
			case 13:
				Entity_Everyone_Except_Owner_And_Controller(ev);
				break;
			case 7:
				Entity_Only_Controller(ev);
				break;
			case 15:
				Entity_Only_Controller_And_Owner(ev);
				break;
			case 9:
				Entity_Only_Owner(ev);
				break;
			case 11:
				Entity_Only_Self(ev);
				break;
			case 2:
				Global_Everyone(ev);
				break;
			case 4:
				Global_Others(ev);
				break;
			case 8:
				Global_All_Clients(ev);
				break;
			case 6:
				Global_Server(ev);
				break;
			case 10:
				Global_Specific_Connection(ev);
				break;
			case 12:
				Global_Only_Self(ev);
				break;
			case 14:
				break;
			}
		}
		catch (Exception exception)
		{
			if (ev != null)
			{
				BoltLog.Error("Exception while invoking event '{0}'", ev.GetType().FullName);
			}
			BoltLog.Exception(exception);
		}
		finally
		{
			ev.DecrementRefs();
		}
	}

	private static void Entity_Only_Controller_And_Owner(Event ev)
	{
		if ((bool)ev.TargetEntity)
		{
			if (ev.TargetEntity.HasControl)
			{
				RaiseLocal(ev);
				if (ev.TargetEntity.IsOwner)
				{
					ev.FreeStorage();
				}
				else if (ev.SourceConnection != ev.TargetEntity.Source)
				{
					ev.TargetEntity.Source._eventChannel.Queue(ev);
				}
			}
			else if (ev.TargetEntity.IsOwner)
			{
				RaiseLocal(ev);
				if (ev.TargetEntity.Controller != null)
				{
					if (ev.SourceConnection != ev.TargetEntity.Controller)
					{
						ev.TargetEntity.Controller._eventChannel.Queue(ev);
					}
				}
				else
				{
					BoltLog.Warn("NetworkEvent sent to controller but no controller exists, event will NOT be raised");
				}
			}
			else
			{
				ev.TargetEntity.Source._eventChannel.Queue(ev);
			}
		}
		else
		{
			BoltLog.Warn("NetworkEvent with NULL target, event will NOT be forwarded or raised");
		}
	}

	private static void Global_Only_Self(Event ev)
	{
		if (!RaiseLocal(ev))
		{
			ev.FreeStorage();
		}
	}

	private static void Entity_Only_Self(Event ev)
	{
		if ((bool)ev.TargetEntity && !RaiseLocal(ev))
		{
			ev.FreeStorage();
		}
	}

	private static void Entity_Only_Owner(Event ev)
	{
		if (!ev.TargetEntity)
		{
			return;
		}
		if (ev.TargetEntity.IsOwner)
		{
			if (!RaiseLocal(ev))
			{
				ev.FreeStorage();
			}
		}
		else
		{
			ev.TargetEntity.Source._eventChannel.Queue(ev);
		}
	}

	private static void Entity_Only_Controller(Event ev)
	{
		if ((bool)ev.TargetEntity)
		{
			if (ev.TargetEntity.HasControl)
			{
				if (!RaiseLocal(ev))
				{
					ev.FreeStorage();
				}
			}
			else if (ev.TargetEntity.IsOwner)
			{
				if (ev.TargetEntity.Controller != null)
				{
					ev.TargetEntity.Controller._eventChannel.Queue(ev);
				}
				else
				{
					BoltLog.Warn("NetworkEvent sent to controller but no controller exists, event will NOT be raised");
				}
			}
			else
			{
				ev.TargetEntity.Source._eventChannel.Queue(ev);
			}
		}
		else
		{
			BoltLog.Warn("NetworkEvent with NULL target, event will NOT be forwarded or raised");
		}
	}

	private static void Entity_Everyone_Except_Owner_And_Controller(Event ev)
	{
		if (!(ev.TargetEntity != null))
		{
			return;
		}
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current != ev.SourceConnection)
			{
				enumerator.Current._eventChannel.Queue(ev);
			}
		}
		if (!ev.TargetEntity.IsOwner && !ev.TargetEntity.HasControl)
		{
			RaiseLocal(ev);
		}
	}

	private static void Entity_Everyone_Except_Owner(Event ev)
	{
		if (!(ev.TargetEntity != null))
		{
			return;
		}
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current != ev.SourceConnection)
			{
				enumerator.Current._eventChannel.Queue(ev);
			}
		}
		if (!ev.TargetEntity.IsOwner)
		{
			RaiseLocal(ev);
		}
	}

	private static void Entity_Everyone_Except_Controller(Event ev)
	{
		if (!(ev.TargetEntity != null))
		{
			return;
		}
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!ev.TargetEntity.IsController(enumerator.Current) && enumerator.Current != ev.SourceConnection)
			{
				enumerator.Current._eventChannel.Queue(ev);
			}
		}
		if (!ev.TargetEntity.HasControl)
		{
			RaiseLocal(ev);
		}
	}

	private static void Entity_Everyone(Event ev)
	{
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		if (!(ev.TargetEntity != null))
		{
			return;
		}
		while (enumerator.MoveNext())
		{
			if (enumerator.Current != ev.SourceConnection)
			{
				enumerator.Current._eventChannel.Queue(ev);
			}
		}
		RaiseLocal(ev);
	}

	private static void Global_Specific_Connection(Event ev)
	{
		if (ev == null)
		{
			return;
		}
		if (ev.FromSelf)
		{
			if (ev.TargetConnection != null && ev.TargetConnection._eventChannel != null)
			{
				ev.TargetConnection._eventChannel.Queue(ev);
			}
		}
		else
		{
			RaiseLocal(ev);
		}
	}

	private static void Global_Server(Event ev)
	{
		if (BoltCore.isServer)
		{
			RaiseLocal(ev);
		}
		else
		{
			BoltCore.server._eventChannel.Queue(ev);
		}
	}

	private static void Global_All_Clients(Event ev)
	{
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current != ev.SourceConnection)
			{
				enumerator.Current._eventChannel.Queue(ev);
			}
		}
		if (BoltCore.isClient)
		{
			RaiseLocal(ev);
		}
	}

	private static void Global_Others(Event ev)
	{
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current != ev.SourceConnection)
			{
				enumerator.Current._eventChannel.Queue(ev);
			}
		}
		if (!ev.FromSelf)
		{
			RaiseLocal(ev);
		}
	}

	private static void Global_Everyone(Event ev)
	{
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current != ev.SourceConnection)
			{
				enumerator.Current._eventChannel.Queue(ev);
			}
		}
		RaiseLocal(ev);
	}

	private static bool RaiseLocal(Event ev)
	{
		BoltLog.Debug("Raising {0}", ev);
		if (ev.IsEntityEvent)
		{
			ev.TargetEntity.EventDispatcher.Raise(ev);
		}
		else
		{
			BoltCore._globalEventDispatcher.Raise(ev);
		}
		if (BoltCore.isClient && !ev.FromSelf)
		{
			ev.FreeStorage();
			return true;
		}
		return false;
	}
}
