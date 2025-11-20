namespace System.Collections.Specialized;

/// <summary>Represents the method that handles the <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event. </summary>
/// <param name="sender">The object that raised the event.</param>
/// <param name="e">Information about the event.</param>
public delegate void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e);
