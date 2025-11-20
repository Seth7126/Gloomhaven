using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading;

namespace System.Data.Common;

/// <summary>Provides a base class for strongly typed connection string builders.</summary>
/// <filterpriority>1</filterpriority>
public class DbConnectionStringBuilder : IDictionary, ICollection, IEnumerable, ICustomTypeDescriptor
{
	private Dictionary<string, object> _currentValues;

	private string _connectionString = string.Empty;

	private PropertyDescriptorCollection _propertyDescriptors;

	private bool _browsableConnectionString = true;

	private readonly bool _useOdbcRules;

	private static int s_objectTypeCount;

	internal readonly int _objectID = Interlocked.Increment(ref s_objectTypeCount);

	private ICollection Collection => CurrentValues;

	private IDictionary Dictionary => CurrentValues;

	private Dictionary<string, object> CurrentValues
	{
		get
		{
			Dictionary<string, object> dictionary = _currentValues;
			if (dictionary == null)
			{
				dictionary = (_currentValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
			}
			return dictionary;
		}
	}

	/// <summary>Gets or sets the element with the specified key.</summary>
	/// <returns>The element with the specified key.</returns>
	/// <param name="keyword">The key of the element to get or set.</param>
	object IDictionary.this[object keyword]
	{
		get
		{
			return this[ObjectToString(keyword)];
		}
		set
		{
			this[ObjectToString(keyword)] = value;
		}
	}

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value associated with the specified key. If the specified key is not found, trying to get it returns a null reference (Nothing in Visual Basic), and trying to set it creates a new element using the specified key.Passing a null (Nothing in Visual Basic) key throws an <see cref="T:System.ArgumentNullException" />. Assigning a null value removes the key/value pair.</returns>
	/// <param name="keyword">The key of the item to get or set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.NotSupportedException">The property is set, and the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> is read-only. -or-The property is set, <paramref name="keyword" /> does not exist in the collection, and the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> has a fixed size.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Browsable(false)]
	public virtual object this[string keyword]
	{
		get
		{
			DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.get_Item|API> {0}, keyword='{1}'", ObjectID, keyword);
			ADP.CheckArgumentNull(keyword, "keyword");
			if (CurrentValues.TryGetValue(keyword, out var value))
			{
				return value;
			}
			throw ADP.KeywordNotSupported(keyword);
		}
		set
		{
			ADP.CheckArgumentNull(keyword, "keyword");
			bool flag = false;
			if (value != null)
			{
				string value2 = DbConnectionStringBuilderUtil.ConvertToString(value);
				DbConnectionOptions.ValidateKeyValuePair(keyword, value2);
				flag = CurrentValues.ContainsKey(keyword);
				CurrentValues[keyword] = value2;
			}
			else
			{
				flag = Remove(keyword);
			}
			_connectionString = null;
			if (flag)
			{
				_propertyDescriptors = null;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="P:System.Data.Common.DbConnectionStringBuilder.ConnectionString" /> property is visible in Visual Studio designers.</summary>
	/// <returns>true if the connection string is visible within designers; false otherwise. The default is true.</returns>
	/// <filterpriority>2</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[DesignOnly(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool BrowsableConnectionString
	{
		get
		{
			return _browsableConnectionString;
		}
		set
		{
			_browsableConnectionString = value;
			_propertyDescriptors = null;
		}
	}

	/// <summary>Gets or sets the connection string associated with the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</summary>
	/// <returns>The current connection string, created from the key/value pairs that are contained within the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />. The default value is an empty string.</returns>
	/// <exception cref="T:System.ArgumentException">An invalid connection string argument has been supplied.</exception>
	/// <filterpriority>1</filterpriority>
	[RefreshProperties(RefreshProperties.All)]
	public string ConnectionString
	{
		get
		{
			DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.get_ConnectionString|API> {0}", ObjectID);
			string text = _connectionString;
			if (text == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string key in Keys)
				{
					if (ShouldSerialize(key) && TryGetValue(key, out var value))
					{
						string value2 = ConvertValueToString(value);
						AppendKeyValuePair(stringBuilder, key, value2, _useOdbcRules);
					}
				}
				text = (_connectionString = stringBuilder.ToString());
			}
			return text;
		}
		set
		{
			DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.set_ConnectionString|API> {0}", ObjectID);
			DbConnectionOptions dbConnectionOptions = new DbConnectionOptions(value, null, _useOdbcRules);
			string connectionString = ConnectionString;
			Clear();
			try
			{
				for (NameValuePair nameValuePair = dbConnectionOptions._keyChain; nameValuePair != null; nameValuePair = nameValuePair.Next)
				{
					if (nameValuePair.Value != null)
					{
						this[nameValuePair.Name] = nameValuePair.Value;
					}
					else
					{
						Remove(nameValuePair.Name);
					}
				}
				_connectionString = null;
			}
			catch (ArgumentException)
			{
				ConnectionString = connectionString;
				_connectionString = connectionString;
				throw;
			}
		}
	}

	/// <summary>Gets the current number of keys that are contained within the <see cref="P:System.Data.Common.DbConnectionStringBuilder.ConnectionString" /> property.</summary>
	/// <returns>The number of keys that are contained within the connection string maintained by the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> instance.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Browsable(false)]
	public virtual int Count => CurrentValues.Count;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> is read-only; otherwise false. The default is false.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public bool IsReadOnly => false;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> has a fixed size; otherwise false.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public virtual bool IsFixedSize => false;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => Collection.IsSynchronized;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> that contains the keys in the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that contains the keys in the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Browsable(false)]
	public virtual ICollection Keys
	{
		get
		{
			DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.Keys|API> {0}", ObjectID);
			return Dictionary.Keys;
		}
	}

	internal int ObjectID => _objectID;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
	object ICollection.SyncRoot => Collection.SyncRoot;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> that contains the values in the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that contains the values in the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[Browsable(false)]
	public virtual ICollection Values
	{
		get
		{
			DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.Values|API> {0}", ObjectID);
			ICollection<string> obj = (ICollection<string>)Keys;
			IEnumerator<string> enumerator = obj.GetEnumerator();
			object[] array = new object[obj.Count];
			for (int i = 0; i < array.Length; i++)
			{
				enumerator.MoveNext();
				array[i] = this[enumerator.Current];
			}
			return new ReadOnlyCollection<object>(array);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> class.</summary>
	public DbConnectionStringBuilder()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> class, optionally using ODBC rules for quoting values.</summary>
	/// <param name="useOdbcRules">true to use {} to delimit fields; false to use quotation marks.</param>
	public DbConnectionStringBuilder(bool useOdbcRules)
	{
		_useOdbcRules = useOdbcRules;
	}

	internal virtual string ConvertValueToString(object value)
	{
		if (value != null)
		{
			return Convert.ToString(value, CultureInfo.InvariantCulture);
		}
		return null;
	}

	/// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary" /> object.</summary>
	/// <param name="keyword">The <see cref="T:System.Object" /> to use as the key of the element to add.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to use as the value of the element to add.</param>
	void IDictionary.Add(object keyword, object value)
	{
		Add(ObjectToString(keyword), value);
	}

	/// <summary>Adds an entry with the specified key and value into the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</summary>
	/// <param name="keyword">The key to add to the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</param>
	/// <param name="value">The value for the specified key.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> is read-only. -or-The <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> has a fixed size.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void Add(string keyword, object value)
	{
		this[keyword] = value;
	}

	/// <summary>Provides an efficient and safe way to append a key and value to an existing <see cref="T:System.Text.StringBuilder" /> object.</summary>
	/// <param name="builder">The <see cref="T:System.Text.StringBuilder" /> to which to add the key/value pair.</param>
	/// <param name="keyword">The key to be added.</param>
	/// <param name="value">The value for the supplied key.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value)
	{
		DbConnectionOptions.AppendKeyValuePairBuilder(builder, keyword, value, useOdbcRules: false);
	}

	/// <summary>Provides an efficient and safe way to append a key and value to an existing <see cref="T:System.Text.StringBuilder" /> object.</summary>
	/// <param name="builder">The <see cref="T:System.Text.StringBuilder" /> to which to add the key/value pair.</param>
	/// <param name="keyword">The key to be added.</param>
	/// <param name="value">The value for the supplied key.</param>
	/// <param name="useOdbcRules">true to use {} to delimit fields, false to use quotation marks.</param>
	public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value, bool useOdbcRules)
	{
		DbConnectionOptions.AppendKeyValuePairBuilder(builder, keyword, value, useOdbcRules);
	}

	/// <summary>Clears the contents of the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> instance.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> is read-only.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual void Clear()
	{
		DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.Clear|API>");
		_connectionString = string.Empty;
		_propertyDescriptors = null;
		CurrentValues.Clear();
	}

	/// <summary>Clears the collection of <see cref="T:System.ComponentModel.PropertyDescriptor" /> objects on the associated <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</summary>
	protected internal void ClearPropertyDescriptors()
	{
		_propertyDescriptors = null;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IDictionary" /> object contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> contains an element with the key; otherwise, false.</returns>
	/// <param name="keyword">The key to locate in the <see cref="T:System.Collections.IDictionary" /> object.</param>
	bool IDictionary.Contains(object keyword)
	{
		return ContainsKey(ObjectToString(keyword));
	}

	/// <summary>Determines whether the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> contains an entry with the specified key; otherwise false.</returns>
	/// <param name="keyword">The key to locate in the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is a null reference (Nothing in Visual Basic).</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual bool ContainsKey(string keyword)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		return CurrentValues.ContainsKey(keyword);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.ICollection.CopyTo|API> {0}", ObjectID);
		Collection.CopyTo(array, index);
	}

	/// <summary>Compares the connection information in this <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> object with the connection information in the supplied object.</summary>
	/// <returns>true if the connection information in both of the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> objects causes an equivalent connection string; otherwise false.</returns>
	/// <param name="connectionStringBuilder">The <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> to be compared with this <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> object.</param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual bool EquivalentTo(DbConnectionStringBuilder connectionStringBuilder)
	{
		ADP.CheckArgumentNull(connectionStringBuilder, "connectionStringBuilder");
		DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.EquivalentTo|API> {0}, connectionStringBuilder={1}", ObjectID, connectionStringBuilder.ObjectID);
		if (GetType() != connectionStringBuilder.GetType() || CurrentValues.Count != connectionStringBuilder.CurrentValues.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, object> currentValue in CurrentValues)
		{
			if (!connectionStringBuilder.CurrentValues.TryGetValue(currentValue.Key, out var value) || !currentValue.Value.Equals(value))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.IEnumerable.GetEnumerator|API> {0}", ObjectID);
		return Collection.GetEnumerator();
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the <see cref="T:System.Collections.IDictionary" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> object for the <see cref="T:System.Collections.IDictionary" /> object.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.IDictionary.GetEnumerator|API> {0}", ObjectID);
		return Dictionary.GetEnumerator();
	}

	private string ObjectToString(object keyword)
	{
		try
		{
			return (string)keyword;
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException("not a string", "keyword");
		}
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" /> object.</summary>
	/// <param name="keyword">The key of the element to remove.</param>
	void IDictionary.Remove(object keyword)
	{
		Remove(ObjectToString(keyword));
	}

	/// <summary>Removes the entry with the specified key from the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> instance.</summary>
	/// <returns>true if the key existed within the connection string and was removed; false if the key did not exist.</returns>
	/// <param name="keyword">The key of the key/value pair to be removed from the connection string in this <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is null (Nothing in Visual Basic)</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> is read-only, or the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> has a fixed size.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual bool Remove(string keyword)
	{
		DataCommonEventSource.Log.Trace("<comm.DbConnectionStringBuilder.Remove|API> {0}, keyword='{1}'", ObjectID, keyword);
		ADP.CheckArgumentNull(keyword, "keyword");
		if (CurrentValues.Remove(keyword))
		{
			_connectionString = null;
			_propertyDescriptors = null;
			return true;
		}
		return false;
	}

	/// <summary>Indicates whether the specified key exists in this <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> instance.</summary>
	/// <returns>true if the <see cref="T:System.Data.Common.DbConnectionStringBuilder" /> contains an entry with the specified key; otherwise false.</returns>
	/// <param name="keyword">The key to locate in the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual bool ShouldSerialize(string keyword)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		return CurrentValues.ContainsKey(keyword);
	}

	/// <summary>Returns the connection string associated with this <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</summary>
	/// <returns>The current <see cref="P:System.Data.Common.DbConnectionStringBuilder.ConnectionString" /> property.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public override string ToString()
	{
		return ConnectionString;
	}

	/// <summary>Retrieves a value corresponding to the supplied key from this <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</summary>
	/// <returns>true if <paramref name="keyword" /> was found within the connection string, false otherwise.</returns>
	/// <param name="keyword">The key of the item to retrieve.</param>
	/// <param name="value">The value corresponding to the <paramref name="key" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> contains a null value (Nothing in Visual Basic).</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual bool TryGetValue(string keyword, out object value)
	{
		ADP.CheckArgumentNull(keyword, "keyword");
		return CurrentValues.TryGetValue(keyword, out value);
	}

	internal Attribute[] GetAttributesFromCollection(AttributeCollection collection)
	{
		Attribute[] array = new Attribute[collection.Count];
		collection.CopyTo(array, 0);
		return array;
	}

	private PropertyDescriptorCollection GetProperties()
	{
		PropertyDescriptorCollection propertyDescriptorCollection = _propertyDescriptors;
		if (propertyDescriptorCollection == null)
		{
			long scopeId = DataCommonEventSource.Log.EnterScope("<comm.DbConnectionStringBuilder.GetProperties|INFO> {0}", ObjectID);
			try
			{
				Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
				GetProperties(hashtable);
				PropertyDescriptor[] array = new PropertyDescriptor[hashtable.Count];
				hashtable.Values.CopyTo(array, 0);
				propertyDescriptorCollection = (_propertyDescriptors = new PropertyDescriptorCollection(array));
			}
			finally
			{
				DataCommonEventSource.Log.ExitScope(scopeId);
			}
		}
		return propertyDescriptorCollection;
	}

	/// <summary>Fills a supplied <see cref="T:System.Collections.Hashtable" /> with information about all the properties of this <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</summary>
	/// <param name="propertyDescriptors">The <see cref="T:System.Collections.Hashtable" /> to be filled with information about this <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.</param>
	protected virtual void GetProperties(Hashtable propertyDescriptors)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<comm.DbConnectionStringBuilder.GetProperties|API> {0}", ObjectID);
		try
		{
			Attribute[] attributesFromCollection;
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this, noCustomTypeDesc: true))
			{
				if ("ConnectionString" != property.Name)
				{
					string displayName = property.DisplayName;
					if (!propertyDescriptors.ContainsKey(displayName))
					{
						attributesFromCollection = GetAttributesFromCollection(property.Attributes);
						PropertyDescriptor value = new DbConnectionStringBuilderDescriptor(property.Name, property.ComponentType, property.PropertyType, property.IsReadOnly, attributesFromCollection);
						propertyDescriptors[displayName] = value;
					}
				}
				else if (BrowsableConnectionString)
				{
					propertyDescriptors["ConnectionString"] = property;
				}
				else
				{
					propertyDescriptors.Remove("ConnectionString");
				}
			}
			if (IsFixedSize)
			{
				return;
			}
			attributesFromCollection = null;
			foreach (string key in Keys)
			{
				if (propertyDescriptors.ContainsKey(key))
				{
					continue;
				}
				object obj = this[key];
				Type type;
				if (obj != null)
				{
					type = obj.GetType();
					if (typeof(string) == type)
					{
						bool result2;
						if (int.TryParse((string)obj, out var _))
						{
							type = typeof(int);
						}
						else if (bool.TryParse((string)obj, out result2))
						{
							type = typeof(bool);
						}
					}
				}
				else
				{
					type = typeof(string);
				}
				Attribute[] attributes = attributesFromCollection;
				if (StringComparer.OrdinalIgnoreCase.Equals("Password", key) || StringComparer.OrdinalIgnoreCase.Equals("pwd", key))
				{
					attributes = new Attribute[3]
					{
						BrowsableAttribute.Yes,
						PasswordPropertyTextAttribute.Yes,
						RefreshPropertiesAttribute.All
					};
				}
				else if (attributesFromCollection == null)
				{
					attributesFromCollection = new Attribute[2]
					{
						BrowsableAttribute.Yes,
						RefreshPropertiesAttribute.All
					};
					attributes = attributesFromCollection;
				}
				PropertyDescriptor value2 = new DbConnectionStringBuilderDescriptor(key, GetType(), type, isReadOnly: false, attributes);
				propertyDescriptors[key] = value2;
			}
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	private PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		PropertyDescriptorCollection properties = GetProperties();
		if (attributes == null || attributes.Length == 0)
		{
			return properties;
		}
		PropertyDescriptor[] array = new PropertyDescriptor[properties.Count];
		int num = 0;
		foreach (PropertyDescriptor item in properties)
		{
			bool flag = true;
			foreach (Attribute attribute in attributes)
			{
				Attribute attribute2 = item.Attributes[attribute.GetType()];
				if ((attribute2 == null && !attribute.IsDefaultAttribute()) || !attribute2.Match(attribute))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				array[num] = item;
				num++;
			}
		}
		PropertyDescriptor[] array2 = new PropertyDescriptor[num];
		Array.Copy(array, array2, num);
		return new PropertyDescriptorCollection(array2);
	}

	/// <summary>Returns the class name of this instance of a component.</summary>
	/// <returns>The class name of the object, or null if the class does not have a name.</returns>
	string ICustomTypeDescriptor.GetClassName()
	{
		return TypeDescriptor.GetClassName(this, noCustomTypeDesc: true);
	}

	/// <summary>Returns the name of this instance of a component.</summary>
	/// <returns>The name of the object, or null if the object does not have a name.</returns>
	string ICustomTypeDescriptor.GetComponentName()
	{
		return TypeDescriptor.GetComponentName(this, noCustomTypeDesc: true);
	}

	/// <summary>Returns a collection of custom attributes for this instance of a component.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.AttributeCollection" /> containing the attributes for this object.</returns>
	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(this, noCustomTypeDesc: true);
	}

	/// <summary>Returns an editor of the specified type for this instance of a component.</summary>
	/// <returns>An <see cref="T:System.Object" /> of the specified type that is the editor for this object, or null if the editor cannot be found.</returns>
	/// <param name="editorBaseType">A <see cref="T:System.Type" /> that represents the editor for this object.</param>
	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(this, editorBaseType, noCustomTypeDesc: true);
	}

	/// <summary>Returns a type converter for this instance of a component.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> that is the converter for this object, or null if there is no <see cref="T:System.ComponentModel.TypeConverter" /> for this object.</returns>
	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return TypeDescriptor.GetConverter(this, noCustomTypeDesc: true);
	}

	/// <summary>Returns the default property for this instance of a component.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that represents the default property for this object, or null if this object does not have properties.</returns>
	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(this, noCustomTypeDesc: true);
	}

	/// <summary>Returns the properties for this instance of a component.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that represents the properties for this component instance.</returns>
	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return GetProperties();
	}

	/// <summary>Returns the properties for this instance of a component using the attribute array as a filter.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> that represents the filtered properties for this component instance.</returns>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter.</param>
	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return GetProperties(attributes);
	}

	/// <summary>Returns the default event for this instance of a component.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EventDescriptor" /> that represents the default event for this object, or null if this object does not have events.</returns>
	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(this, noCustomTypeDesc: true);
	}

	/// <summary>Returns the events for this instance of a component.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EventDescriptorCollection" /> that represents the events for this component instance.</returns>
	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return TypeDescriptor.GetEvents(this, noCustomTypeDesc: true);
	}

	/// <summary>Returns the events for this instance of a component using the specified attribute array as a filter.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EventDescriptorCollection" /> that represents the filtered events for this component instance.</returns>
	/// <param name="attributes">An array of type <see cref="T:System.Attribute" /> that is used as a filter.</param>
	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(this, attributes, noCustomTypeDesc: true);
	}

	/// <summary>Returns an object that contains the property described by the specified property descriptor.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the owner of the specified property.</returns>
	/// <param name="pd">A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that represents the property whose owner is to be found.</param>
	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}
}
