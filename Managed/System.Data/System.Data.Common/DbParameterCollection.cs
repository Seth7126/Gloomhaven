using System.Collections;
using System.ComponentModel;

namespace System.Data.Common;

/// <summary>The base class for a collection of parameters relevant to a <see cref="T:System.Data.Common.DbCommand" />. </summary>
/// <filterpriority>2</filterpriority>
public abstract class DbParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
{
	/// <summary>Specifies the number of items in the collection.</summary>
	/// <returns>The number of items in the collection.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public abstract int Count { get; }

	/// <summary>Specifies whether the collection is a fixed size.</summary>
	/// <returns>true if the collection is a fixed size; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool IsFixedSize => false;

	/// <summary>Specifies whether the collection is read-only.</summary>
	/// <returns>true if the collection is read-only; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool IsReadOnly => false;

	/// <summary>Specifies whether the collection is synchronized.</summary>
	/// <returns>true if the collection is synchronized; otherwise false.</returns>
	/// <filterpriority>2</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	public virtual bool IsSynchronized => false;

	/// <summary>Specifies the <see cref="T:System.Object" /> to be used to synchronize access to the collection.</summary>
	/// <returns>A <see cref="T:System.Object" /> to be used to synchronize access to the <see cref="T:System.Data.Common.DbParameterCollection" />.</returns>
	/// <filterpriority>2</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public abstract object SyncRoot { get; }

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	object IList.this[int index]
	{
		get
		{
			return GetParameter(index);
		}
		set
		{
			SetParameter(index, (DbParameter)value);
		}
	}

	/// <summary>Gets or sets the parameter at the specified index.</summary>
	/// <returns>An <see cref="T:System.Object" /> at the specified index.</returns>
	/// <param name="parameterName">The name of the parameter to retrieve.</param>
	object IDataParameterCollection.this[string parameterName]
	{
		get
		{
			return GetParameter(parameterName);
		}
		set
		{
			SetParameter(parameterName, (DbParameter)value);
		}
	}

	/// <summary>Gets and sets the <see cref="T:System.Data.Common.DbParameter" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Data.Common.DbParameter" /> at the specified index.</returns>
	/// <param name="index">The zero-based index of the parameter.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The specified index does not exist. </exception>
	/// <filterpriority>1</filterpriority>
	public DbParameter this[int index]
	{
		get
		{
			return GetParameter(index);
		}
		set
		{
			SetParameter(index, value);
		}
	}

	/// <summary>Gets and sets the <see cref="T:System.Data.Common.DbParameter" /> with the specified name.</summary>
	/// <returns>The <see cref="T:System.Data.Common.DbParameter" /> with the specified name.</returns>
	/// <param name="parameterName">The name of the parameter.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">The specified index does not exist. </exception>
	/// <filterpriority>1</filterpriority>
	public DbParameter this[string parameterName]
	{
		get
		{
			return GetParameter(parameterName);
		}
		set
		{
			SetParameter(parameterName, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DbParameterCollection" /> class.</summary>
	protected DbParameterCollection()
	{
	}

	/// <summary>Adds the specified <see cref="T:System.Data.Common.DbParameter" /> object to the <see cref="T:System.Data.Common.DbParameterCollection" />.</summary>
	/// <returns>The index of the <see cref="T:System.Data.Common.DbParameter" /> object in the collection.</returns>
	/// <param name="value">The <see cref="P:System.Data.Common.DbParameter.Value" /> of the <see cref="T:System.Data.Common.DbParameter" /> to add to the collection.</param>
	/// <filterpriority>1</filterpriority>
	public abstract int Add(object value);

	/// <summary>Adds an array of items with the specified values to the <see cref="T:System.Data.Common.DbParameterCollection" />.</summary>
	/// <param name="values">An array of values of type <see cref="T:System.Data.Common.DbParameter" /> to add to the collection.</param>
	/// <filterpriority>2</filterpriority>
	public abstract void AddRange(Array values);

	/// <summary>Indicates whether a <see cref="T:System.Data.Common.DbParameter" /> with the specified <see cref="P:System.Data.Common.DbParameter.Value" /> is contained in the collection.</summary>
	/// <returns>true if the <see cref="T:System.Data.Common.DbParameter" /> is in the collection; otherwise false.</returns>
	/// <param name="value">The <see cref="P:System.Data.Common.DbParameter.Value" /> of the <see cref="T:System.Data.Common.DbParameter" /> to look for in the collection.</param>
	/// <filterpriority>1</filterpriority>
	public abstract bool Contains(object value);

	/// <summary>Indicates whether a <see cref="T:System.Data.Common.DbParameter" /> with the specified name exists in the collection.</summary>
	/// <returns>true if the <see cref="T:System.Data.Common.DbParameter" /> is in the collection; otherwise false.</returns>
	/// <param name="value">The name of the <see cref="T:System.Data.Common.DbParameter" /> to look for in the collection.</param>
	/// <filterpriority>1</filterpriority>
	public abstract bool Contains(string value);

	/// <summary>Copies an array of items to the collection starting at the specified index.</summary>
	/// <param name="array">The array of items to copy to the collection.</param>
	/// <param name="index">The index in the collection to copy the items.</param>
	/// <filterpriority>2</filterpriority>
	public abstract void CopyTo(Array array, int index);

	/// <summary>Removes all <see cref="T:System.Data.Common.DbParameter" /> values from the <see cref="T:System.Data.Common.DbParameterCollection" />.</summary>
	/// <filterpriority>1</filterpriority>
	public abstract void Clear();

	/// <summary>Exposes the <see cref="M:System.Collections.IEnumerable.GetEnumerator" /> method, which supports a simple iteration over a collection by a .NET Framework data provider.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	/// <filterpriority>2</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract IEnumerator GetEnumerator();

	/// <summary>Returns the <see cref="T:System.Data.Common.DbParameter" /> object at the specified index in the collection.</summary>
	/// <returns>The <see cref="T:System.Data.Common.DbParameter" /> object at the specified index in the collection.</returns>
	/// <param name="index">The index of the <see cref="T:System.Data.Common.DbParameter" /> in the collection.</param>
	protected abstract DbParameter GetParameter(int index);

	/// <summary>Returns <see cref="T:System.Data.Common.DbParameter" /> the object with the specified name.</summary>
	/// <returns>The <see cref="T:System.Data.Common.DbParameter" /> the object with the specified name.</returns>
	/// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter" /> in the collection.</param>
	protected abstract DbParameter GetParameter(string parameterName);

	/// <summary>Returns the index of the specified <see cref="T:System.Data.Common.DbParameter" /> object.</summary>
	/// <returns>The index of the specified <see cref="T:System.Data.Common.DbParameter" /> object.</returns>
	/// <param name="value">The <see cref="T:System.Data.Common.DbParameter" /> object in the collection.</param>
	/// <filterpriority>2</filterpriority>
	public abstract int IndexOf(object value);

	/// <summary>Returns the index of the <see cref="T:System.Data.Common.DbParameter" /> object with the specified name.</summary>
	/// <returns>The index of the <see cref="T:System.Data.Common.DbParameter" /> object with the specified name.</returns>
	/// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter" /> object in the collection.</param>
	/// <filterpriority>2</filterpriority>
	public abstract int IndexOf(string parameterName);

	/// <summary>Inserts the specified index of the <see cref="T:System.Data.Common.DbParameter" /> object with the specified name into the collection at the specified index.</summary>
	/// <param name="index">The index at which to insert the <see cref="T:System.Data.Common.DbParameter" /> object.</param>
	/// <param name="value">The <see cref="T:System.Data.Common.DbParameter" /> object to insert into the collection.</param>
	/// <filterpriority>1</filterpriority>
	public abstract void Insert(int index, object value);

	/// <summary>Removes the specified <see cref="T:System.Data.Common.DbParameter" /> object from the collection.</summary>
	/// <param name="value">The <see cref="T:System.Data.Common.DbParameter" /> object to remove.</param>
	/// <filterpriority>1</filterpriority>
	public abstract void Remove(object value);

	/// <summary>Removes the <see cref="T:System.Data.Common.DbParameter" /> object at the specified from the collection.</summary>
	/// <param name="index">The index where the <see cref="T:System.Data.Common.DbParameter" /> object is located.</param>
	/// <filterpriority>2</filterpriority>
	public abstract void RemoveAt(int index);

	/// <summary>Removes the <see cref="T:System.Data.Common.DbParameter" /> object with the specified name from the collection.</summary>
	/// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter" /> object to remove.</param>
	/// <filterpriority>2</filterpriority>
	public abstract void RemoveAt(string parameterName);

	/// <summary>Sets the <see cref="T:System.Data.Common.DbParameter" /> object at the specified index to a new value. </summary>
	/// <param name="index">The index where the <see cref="T:System.Data.Common.DbParameter" /> object is located.</param>
	/// <param name="value">The new <see cref="T:System.Data.Common.DbParameter" /> value.</param>
	protected abstract void SetParameter(int index, DbParameter value);

	/// <summary>Sets the <see cref="T:System.Data.Common.DbParameter" /> object with the specified name to a new value.</summary>
	/// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter" /> object in the collection.</param>
	/// <param name="value">The new <see cref="T:System.Data.Common.DbParameter" /> value.</param>
	protected abstract void SetParameter(string parameterName, DbParameter value);
}
