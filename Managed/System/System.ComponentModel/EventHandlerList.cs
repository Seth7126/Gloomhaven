namespace System.ComponentModel;

/// <summary>Provides a simple list of delegates. This class cannot be inherited.</summary>
public sealed class EventHandlerList : IDisposable
{
	private sealed class ListEntry
	{
		internal ListEntry _next;

		internal object _key;

		internal Delegate _handler;

		public ListEntry(object key, Delegate handler, ListEntry next)
		{
			_next = next;
			_key = key;
			_handler = handler;
		}
	}

	private ListEntry _head;

	private Component _parent;

	/// <summary>Gets or sets the delegate for the specified object.</summary>
	/// <returns>The delegate for the specified key, or null if a delegate does not exist.</returns>
	/// <param name="key">An object to find in the list. </param>
	public Delegate this[object key]
	{
		get
		{
			ListEntry listEntry = null;
			if (_parent == null || _parent.CanRaiseEventsInternal)
			{
				listEntry = Find(key);
			}
			return listEntry?._handler;
		}
		set
		{
			ListEntry listEntry = Find(key);
			if (listEntry != null)
			{
				listEntry._handler = value;
			}
			else
			{
				_head = new ListEntry(key, value, _head);
			}
		}
	}

	internal EventHandlerList(Component parent)
	{
		_parent = parent;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EventHandlerList" /> class. </summary>
	public EventHandlerList()
	{
	}

	/// <summary>Adds a delegate to the list.</summary>
	/// <param name="key">The object that owns the event. </param>
	/// <param name="value">The delegate to add to the list. </param>
	public void AddHandler(object key, Delegate value)
	{
		ListEntry listEntry = Find(key);
		if (listEntry != null)
		{
			listEntry._handler = Delegate.Combine(listEntry._handler, value);
		}
		else
		{
			_head = new ListEntry(key, value, _head);
		}
	}

	/// <summary>Adds a list of delegates to the current list.</summary>
	/// <param name="listToAddFrom">The list to add.</param>
	public void AddHandlers(EventHandlerList listToAddFrom)
	{
		for (ListEntry listEntry = listToAddFrom._head; listEntry != null; listEntry = listEntry._next)
		{
			AddHandler(listEntry._key, listEntry._handler);
		}
	}

	/// <summary>Disposes the delegate list.</summary>
	public void Dispose()
	{
		_head = null;
	}

	private ListEntry Find(object key)
	{
		ListEntry listEntry = _head;
		while (listEntry != null && listEntry._key != key)
		{
			listEntry = listEntry._next;
		}
		return listEntry;
	}

	/// <summary>Removes a delegate from the list.</summary>
	/// <param name="key">The object that owns the event. </param>
	/// <param name="value">The delegate to remove from the list. </param>
	public void RemoveHandler(object key, Delegate value)
	{
		ListEntry listEntry = Find(key);
		if (listEntry != null)
		{
			listEntry._handler = Delegate.Remove(listEntry._handler, value);
		}
	}
}
