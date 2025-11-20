using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace System.Collections.ObjectModel;

/// <summary>Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.</summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
[Serializable]
[DebuggerTypeProxy(typeof(CollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
	[Serializable]
	private sealed class SimpleMonitor : IDisposable
	{
		internal int _busyCount;

		[NonSerialized]
		internal ObservableCollection<T> _collection;

		public SimpleMonitor(ObservableCollection<T> collection)
		{
			_collection = collection;
		}

		public void Dispose()
		{
			_collection._blockReentrancyCount--;
		}
	}

	private SimpleMonitor _monitor;

	[NonSerialized]
	private int _blockReentrancyCount;

	/// <summary>Occurs when a property value changes.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PropertyChanged += value;
		}
		remove
		{
			PropertyChanged -= value;
		}
	}

	/// <summary>Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.</summary>
	public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

	/// <summary>Occurs when a property value changes.</summary>
	protected virtual event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> class.</summary>
	public ObservableCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> class that contains elements copied from the specified collection.</summary>
	/// <param name="collection">The collection from which the elements are copied.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="collection" /> parameter cannot be null.</exception>
	public ObservableCollection(IEnumerable<T> collection)
		: base((IList<T>)CreateCopy(collection, "collection"))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> class that contains elements copied from the specified list.</summary>
	/// <param name="list">The list from which the elements are copied.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="list" /> parameter cannot be null.</exception>
	public ObservableCollection(List<T> list)
		: base((IList<T>)CreateCopy(list, "list"))
	{
	}

	private static List<T> CreateCopy(IEnumerable<T> collection, string paramName)
	{
		if (collection == null)
		{
			throw new ArgumentNullException(paramName);
		}
		return new List<T>(collection);
	}

	/// <summary>Moves the item at the specified index to a new location in the collection.</summary>
	/// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
	/// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
	public void Move(int oldIndex, int newIndex)
	{
		MoveItem(oldIndex, newIndex);
	}

	/// <summary>Removes all items from the collection.</summary>
	protected override void ClearItems()
	{
		CheckReentrancy();
		base.ClearItems();
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionReset();
	}

	/// <summary>Removes the item at the specified index of the collection.</summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	protected override void RemoveItem(int index)
	{
		CheckReentrancy();
		T val = base[index];
		base.RemoveItem(index);
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, val, index);
	}

	/// <summary>Inserts an item into the collection at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert.</param>
	protected override void InsertItem(int index, T item)
	{
		CheckReentrancy();
		base.InsertItem(index, item);
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
	}

	/// <summary>Replaces the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to replace.</param>
	/// <param name="item">The new value for the element at the specified index.</param>
	protected override void SetItem(int index, T item)
	{
		CheckReentrancy();
		T val = base[index];
		base.SetItem(index, item);
		OnIndexerPropertyChanged();
		OnCollectionChanged(NotifyCollectionChangedAction.Replace, val, item, index);
	}

	/// <summary>Moves the item at the specified index to a new location in the collection.</summary>
	/// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
	/// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
	protected virtual void MoveItem(int oldIndex, int newIndex)
	{
		CheckReentrancy();
		T val = base[oldIndex];
		base.RemoveItem(oldIndex);
		base.InsertItem(newIndex, val);
		OnIndexerPropertyChanged();
		OnCollectionChanged(NotifyCollectionChangedAction.Move, val, newIndex, oldIndex);
	}

	/// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.PropertyChanged" /> event with the provided arguments.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event with the provided arguments.</summary>
	/// <param name="e">Arguments of the event being raised.</param>
	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		NotifyCollectionChangedEventHandler notifyCollectionChangedEventHandler = this.CollectionChanged;
		if (notifyCollectionChangedEventHandler != null)
		{
			_blockReentrancyCount++;
			try
			{
				notifyCollectionChangedEventHandler(this, e);
			}
			finally
			{
				_blockReentrancyCount--;
			}
		}
	}

	/// <summary>Disallows reentrant attempts to change this collection.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that can be used to dispose of the object.</returns>
	protected IDisposable BlockReentrancy()
	{
		_blockReentrancyCount++;
		return EnsureMonitorInitialized();
	}

	/// <summary>Checks for reentrant attempts to change this collection.</summary>
	/// <exception cref="T:System.InvalidOperationException">If there was a call to <see cref="M:System.Collections.ObjectModel.ObservableCollection`1.BlockReentrancy" /> of which the <see cref="T:System.IDisposable" /> return value has not yet been disposed of. Typically, this means when there are additional attempts to change this collection during a <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event. However, it depends on when derived classes choose to call <see cref="M:System.Collections.ObjectModel.ObservableCollection`1.BlockReentrancy" />.</exception>
	protected void CheckReentrancy()
	{
		if (_blockReentrancyCount > 0)
		{
			NotifyCollectionChangedEventHandler notifyCollectionChangedEventHandler = this.CollectionChanged;
			if (notifyCollectionChangedEventHandler != null && notifyCollectionChangedEventHandler.GetInvocationList().Length > 1)
			{
				throw new InvalidOperationException("Cannot change ObservableCollection during a CollectionChanged event.");
			}
		}
	}

	private void OnCountPropertyChanged()
	{
		OnPropertyChanged(EventArgsCache.CountPropertyChanged);
	}

	private void OnIndexerPropertyChanged()
	{
		OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
	}

	private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
	{
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
	}

	private void OnCollectionReset()
	{
		OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
	}

	private SimpleMonitor EnsureMonitorInitialized()
	{
		return _monitor ?? (_monitor = new SimpleMonitor(this));
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext context)
	{
		EnsureMonitorInitialized();
		_monitor._busyCount = _blockReentrancyCount;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext context)
	{
		if (_monitor != null)
		{
			_blockReentrancyCount = _monitor._busyCount;
			_monitor._collection = this;
		}
	}
}
