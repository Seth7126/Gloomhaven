namespace System.Collections.Specialized;

/// <summary>Notifies listeners of dynamic changes, such as when items get added and removed or the whole list is refreshed.</summary>
public interface INotifyCollectionChanged
{
	/// <summary>Occurs when the collection changes.</summary>
	event NotifyCollectionChangedEventHandler CollectionChanged;
}
