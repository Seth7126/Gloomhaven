namespace System.ComponentModel;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> event.</summary>
public class ListChangedEventArgs : EventArgs
{
	/// <summary>Gets the type of change.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.ListChangedType" /> value indicating the type of change.</returns>
	public ListChangedType ListChangedType { get; }

	/// <summary>Gets the index of the item affected by the change.</summary>
	/// <returns>The index of the affected by the change.</returns>
	public int NewIndex { get; }

	/// <summary>Gets the old index of an item that has been moved.</summary>
	/// <returns>The old index of the moved item.</returns>
	public int OldIndex { get; }

	/// <summary>Gets the <see cref="T:System.ComponentModel.PropertyDescriptor" /> that was added, changed, or deleted.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor" /> affected by the change.</returns>
	public PropertyDescriptor PropertyDescriptor { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ListChangedEventArgs" /> class given the type of change and the index of the affected item.</summary>
	/// <param name="listChangedType">A <see cref="T:System.ComponentModel.ListChangedType" /> value indicating the type of change.</param>
	/// <param name="newIndex">The index of the item that was added, changed, or removed.</param>
	public ListChangedEventArgs(ListChangedType listChangedType, int newIndex)
		: this(listChangedType, newIndex, -1)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ListChangedEventArgs" /> class given the type of change, the index of the affected item, and a <see cref="T:System.ComponentModel.PropertyDescriptor" /> describing the affected item.</summary>
	/// <param name="listChangedType">A <see cref="T:System.ComponentModel.ListChangedType" /> value indicating the type of change.</param>
	/// <param name="newIndex">The index of the item that was added or changed.</param>
	/// <param name="propDesc">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> describing the item.</param>
	public ListChangedEventArgs(ListChangedType listChangedType, int newIndex, PropertyDescriptor propDesc)
		: this(listChangedType, newIndex)
	{
		PropertyDescriptor = propDesc;
		OldIndex = newIndex;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ListChangedEventArgs" /> class given the type of change and the <see cref="T:System.ComponentModel.PropertyDescriptor" /> affected.</summary>
	/// <param name="listChangedType">A <see cref="T:System.ComponentModel.ListChangedType" /> value indicating the type of change.</param>
	/// <param name="propDesc">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> that was added, removed, or changed.</param>
	public ListChangedEventArgs(ListChangedType listChangedType, PropertyDescriptor propDesc)
	{
		ListChangedType = listChangedType;
		PropertyDescriptor = propDesc;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ListChangedEventArgs" /> class given the type of change and the old and new index of the item that was moved.</summary>
	/// <param name="listChangedType">A <see cref="T:System.ComponentModel.ListChangedType" /> value indicating the type of change.</param>
	/// <param name="newIndex">The new index of the item that was moved.</param>
	/// <param name="oldIndex">The old index of the item that was moved.</param>
	public ListChangedEventArgs(ListChangedType listChangedType, int newIndex, int oldIndex)
	{
		ListChangedType = listChangedType;
		NewIndex = newIndex;
		OldIndex = oldIndex;
	}
}
