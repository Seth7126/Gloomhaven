using System.ComponentModel;
using System.Data.ProviderBase;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace System.Data.Common;

/// <summary>Represents a set of SQL commands and a database connection that are used to fill the <see cref="T:System.Data.DataSet" /> and update the data source.</summary>
/// <filterpriority>1</filterpriority>
public class DataAdapter : Component, IDataAdapter
{
	private static readonly object s_eventFillError = new object();

	private bool _acceptChangesDuringUpdate = true;

	private bool _acceptChangesDuringUpdateAfterInsert = true;

	private bool _continueUpdateOnError;

	private bool _hasFillErrorHandler;

	private bool _returnProviderSpecificTypes;

	private bool _acceptChangesDuringFill = true;

	private LoadOption _fillLoadOption;

	private MissingMappingAction _missingMappingAction = MissingMappingAction.Passthrough;

	private MissingSchemaAction _missingSchemaAction = MissingSchemaAction.Add;

	private DataTableMappingCollection _tableMappings;

	private static int s_objectTypeCount;

	internal readonly int _objectID = Interlocked.Increment(ref s_objectTypeCount);

	/// <summary>Gets or sets a value indicating whether <see cref="M:System.Data.DataRow.AcceptChanges" /> is called on a <see cref="T:System.Data.DataRow" /> after it is added to the <see cref="T:System.Data.DataTable" /> during any of the Fill operations.</summary>
	/// <returns>true if <see cref="M:System.Data.DataRow.AcceptChanges" /> is called on the <see cref="T:System.Data.DataRow" />; otherwise false. The default is true.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue(true)]
	public bool AcceptChangesDuringFill
	{
		get
		{
			return _acceptChangesDuringFill;
		}
		set
		{
			_acceptChangesDuringFill = value;
		}
	}

	/// <summary>Gets or sets whether <see cref="M:System.Data.DataRow.AcceptChanges" /> is called during a <see cref="M:System.Data.Common.DataAdapter.Update(System.Data.DataSet)" />.</summary>
	/// <returns>true if <see cref="M:System.Data.DataRow.AcceptChanges" /> is called during an <see cref="M:System.Data.Common.DataAdapter.Update(System.Data.DataSet)" />; otherwise false. The default is true.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue(true)]
	public bool AcceptChangesDuringUpdate
	{
		get
		{
			return _acceptChangesDuringUpdate;
		}
		set
		{
			_acceptChangesDuringUpdate = value;
		}
	}

	/// <summary>Gets or sets a value that specifies whether to generate an exception when an error is encountered during a row update.</summary>
	/// <returns>true to continue the update without generating an exception; otherwise false. The default is false.</returns>
	/// <filterpriority>1</filterpriority>
	[DefaultValue(false)]
	public bool ContinueUpdateOnError
	{
		get
		{
			return _continueUpdateOnError;
		}
		set
		{
			_continueUpdateOnError = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Data.LoadOption" /> that determines how the adapter fills the <see cref="T:System.Data.DataTable" /> from the <see cref="T:System.Data.Common.DbDataReader" />.</summary>
	/// <returns>A <see cref="T:System.Data.LoadOption" /> value.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[RefreshProperties(RefreshProperties.All)]
	public LoadOption FillLoadOption
	{
		get
		{
			if (_fillLoadOption == (LoadOption)0)
			{
				return LoadOption.OverwriteChanges;
			}
			return _fillLoadOption;
		}
		set
		{
			if ((uint)value <= 3u)
			{
				_fillLoadOption = value;
				return;
			}
			throw ADP.InvalidLoadOption(value);
		}
	}

	/// <summary>Determines the action to take when incoming data does not have a matching table or column.</summary>
	/// <returns>One of the <see cref="T:System.Data.MissingMappingAction" /> values. The default is Passthrough.</returns>
	/// <exception cref="T:System.ArgumentException">The value set is not one of the <see cref="T:System.Data.MissingMappingAction" /> values. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(MissingMappingAction.Passthrough)]
	public MissingMappingAction MissingMappingAction
	{
		get
		{
			return _missingMappingAction;
		}
		set
		{
			if ((uint)(value - 1) <= 2u)
			{
				_missingMappingAction = value;
				return;
			}
			throw ADP.InvalidMissingMappingAction(value);
		}
	}

	/// <summary>Determines the action to take when existing <see cref="T:System.Data.DataSet" /> schema does not match incoming data.</summary>
	/// <returns>One of the <see cref="T:System.Data.MissingSchemaAction" /> values. The default is Add.</returns>
	/// <exception cref="T:System.ArgumentException">The value set is not one of the <see cref="T:System.Data.MissingSchemaAction" /> values. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	[DefaultValue(MissingSchemaAction.Add)]
	public MissingSchemaAction MissingSchemaAction
	{
		get
		{
			return _missingSchemaAction;
		}
		set
		{
			if ((uint)(value - 1) <= 3u)
			{
				_missingSchemaAction = value;
				return;
			}
			throw ADP.InvalidMissingSchemaAction(value);
		}
	}

	internal int ObjectID => _objectID;

	/// <summary>Gets or sets whether the Fill method should return provider-specific values or common CLS-compliant values.</summary>
	/// <returns>true if the Fill method should return provider-specific values; otherwise false to return common CLS-compliant values.</returns>
	/// <filterpriority>2</filterpriority>
	[DefaultValue(false)]
	public virtual bool ReturnProviderSpecificTypes
	{
		get
		{
			return _returnProviderSpecificTypes;
		}
		set
		{
			_returnProviderSpecificTypes = value;
		}
	}

	/// <summary>Gets a collection that provides the master mapping between a source table and a <see cref="T:System.Data.DataTable" />.</summary>
	/// <returns>A collection that provides the master mapping between the returned records and the <see cref="T:System.Data.DataSet" />. The default value is an empty collection.</returns>
	/// <filterpriority>1</filterpriority>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public DataTableMappingCollection TableMappings
	{
		get
		{
			DataTableMappingCollection dataTableMappingCollection = _tableMappings;
			if (dataTableMappingCollection == null)
			{
				dataTableMappingCollection = CreateTableMappings();
				if (dataTableMappingCollection == null)
				{
					dataTableMappingCollection = new DataTableMappingCollection();
				}
				_tableMappings = dataTableMappingCollection;
			}
			return dataTableMappingCollection;
		}
	}

	/// <summary>Indicates how a source table is mapped to a dataset table.</summary>
	/// <returns>A collection that provides the master mapping between the returned records and the <see cref="T:System.Data.DataSet" />. The default value is an empty collection.</returns>
	ITableMappingCollection IDataAdapter.TableMappings => TableMappings;

	/// <summary>Returned when an error occurs during a fill operation.</summary>
	/// <filterpriority>2</filterpriority>
	public event FillErrorEventHandler FillError
	{
		add
		{
			_hasFillErrorHandler = true;
			base.Events.AddHandler(s_eventFillError, value);
		}
		remove
		{
			base.Events.RemoveHandler(s_eventFillError, value);
		}
	}

	[Conditional("DEBUG")]
	private void AssertReaderHandleFieldCount(DataReaderContainer readerHandler)
	{
	}

	[Conditional("DEBUG")]
	private void AssertSchemaMapping(SchemaMapping mapping)
	{
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Data.Common.DataAdapter" /> class.</summary>
	protected DataAdapter()
	{
		GC.SuppressFinalize(this);
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Data.Common.DataAdapter" /> class from an existing object of the same type.</summary>
	/// <param name="from">A <see cref="T:System.Data.Common.DataAdapter" /> object used to create the new <see cref="T:System.Data.Common.DataAdapter" />. </param>
	protected DataAdapter(DataAdapter from)
	{
		CloneFrom(from);
	}

	/// <summary>Determines whether the <see cref="P:System.Data.Common.DataAdapter.AcceptChangesDuringFill" /> property should be persisted.</summary>
	/// <returns>true if the <see cref="P:System.Data.Common.DataAdapter.AcceptChangesDuringFill" /> property is persisted; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual bool ShouldSerializeAcceptChangesDuringFill()
	{
		return _fillLoadOption == (LoadOption)0;
	}

	/// <summary>Resets <see cref="P:System.Data.Common.DataAdapter.FillLoadOption" /> to its default state and causes <see cref="M:System.Data.Common.DataAdapter.Fill(System.Data.DataSet)" /> to honor <see cref="P:System.Data.Common.DataAdapter.AcceptChangesDuringFill" />.</summary>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void ResetFillLoadOption()
	{
		_fillLoadOption = (LoadOption)0;
	}

	/// <summary>Determines whether the <see cref="P:System.Data.Common.DataAdapter.FillLoadOption" /> property should be persisted.</summary>
	/// <returns>true if the <see cref="P:System.Data.Common.DataAdapter.FillLoadOption" /> property is persisted; otherwise false.</returns>
	/// <filterpriority>1</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual bool ShouldSerializeFillLoadOption()
	{
		return _fillLoadOption != (LoadOption)0;
	}

	/// <summary>Determines whether one or more <see cref="T:System.Data.Common.DataTableMapping" /> objects exist and they should be persisted.</summary>
	/// <returns>true if one or more <see cref="T:System.Data.Common.DataTableMapping" /> objects exist; otherwise false.</returns>
	protected virtual bool ShouldSerializeTableMappings()
	{
		return true;
	}

	/// <summary>Indicates whether a <see cref="T:System.Data.Common.DataTableMappingCollection" /> has been created.</summary>
	/// <returns>true if a <see cref="T:System.Data.Common.DataTableMappingCollection" /> has been created; otherwise false.</returns>
	protected bool HasTableMappings()
	{
		if (_tableMappings != null)
		{
			return 0 < TableMappings.Count;
		}
		return false;
	}

	/// <summary>Creates a copy of this instance of <see cref="T:System.Data.Common.DataAdapter" />.</summary>
	/// <returns>The cloned instance of <see cref="T:System.Data.Common.DataAdapter" />.</returns>
	[Obsolete("CloneInternals() has been deprecated.  Use the DataAdapter(DataAdapter from) constructor.  http://go.microsoft.com/fwlink/?linkid=14202")]
	protected virtual DataAdapter CloneInternals()
	{
		DataAdapter obj = (DataAdapter)Activator.CreateInstance(GetType(), BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture, null);
		obj.CloneFrom(this);
		return obj;
	}

	private void CloneFrom(DataAdapter from)
	{
		_acceptChangesDuringUpdate = from._acceptChangesDuringUpdate;
		_acceptChangesDuringUpdateAfterInsert = from._acceptChangesDuringUpdateAfterInsert;
		_continueUpdateOnError = from._continueUpdateOnError;
		_returnProviderSpecificTypes = from._returnProviderSpecificTypes;
		_acceptChangesDuringFill = from._acceptChangesDuringFill;
		_fillLoadOption = from._fillLoadOption;
		_missingMappingAction = from._missingMappingAction;
		_missingSchemaAction = from._missingSchemaAction;
		if (from._tableMappings == null || 0 >= from.TableMappings.Count)
		{
			return;
		}
		DataTableMappingCollection tableMappings = TableMappings;
		foreach (object tableMapping in from.TableMappings)
		{
			tableMappings.Add((tableMapping is ICloneable) ? ((ICloneable)tableMapping).Clone() : tableMapping);
		}
	}

	/// <summary>Creates a new <see cref="T:System.Data.Common.DataTableMappingCollection" />.</summary>
	/// <returns>A new table mapping collection.</returns>
	protected virtual DataTableMappingCollection CreateTableMappings()
	{
		DataCommonEventSource.Log.Trace("<comm.DataAdapter.CreateTableMappings|API> {0}", ObjectID);
		return new DataTableMappingCollection();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Data.Common.DataAdapter" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_tableMappings = null;
		}
		base.Dispose(disposing);
	}

	/// <summary>Adds a <see cref="T:System.Data.DataTable" /> to the specified <see cref="T:System.Data.DataSet" /> and configures the schema to match that in the data source based on the specified <see cref="T:System.Data.SchemaType" />.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> object that contains schema information returned from the data source.</returns>
	/// <param name="dataSet">The <see cref="T:System.Data.DataSet" /> to be filled with the schema from the data source. </param>
	/// <param name="schemaType">One of the <see cref="T:System.Data.SchemaType" /> values. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType)
	{
		throw ADP.NotSupported();
	}

	/// <summary>Adds a <see cref="T:System.Data.DataTable" /> to the specified <see cref="T:System.Data.DataSet" />.</summary>
	/// <returns>A reference to a collection of <see cref="T:System.Data.DataTable" /> objects that were added to the <see cref="T:System.Data.DataSet" />.</returns>
	/// <param name="dataSet">The <see cref="T:System.Data.DataTable" /> to be filled from the <see cref="T:System.Data.IDataReader" />.</param>
	/// <param name="schemaType">One of the <see cref="T:System.Data.SchemaType" /> values.</param>
	/// <param name="srcTable">The name of the source table to use for table mapping.</param>
	/// <param name="dataReader">The <see cref="T:System.Data.IDataReader" /> to be used as the data source when filling the <see cref="T:System.Data.DataTable" />.</param>
	protected virtual DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType, string srcTable, IDataReader dataReader)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<comm.DataAdapter.FillSchema|API> {0}, dataSet, schemaType={1}, srcTable, dataReader", ObjectID, schemaType);
		try
		{
			if (dataSet == null)
			{
				throw ADP.ArgumentNull("dataSet");
			}
			if (SchemaType.Source != schemaType && SchemaType.Mapped != schemaType)
			{
				throw ADP.InvalidSchemaType(schemaType);
			}
			if (string.IsNullOrEmpty(srcTable))
			{
				throw ADP.FillSchemaRequiresSourceTableName("srcTable");
			}
			if (dataReader == null || dataReader.IsClosed)
			{
				throw ADP.FillRequires("dataReader");
			}
			return (DataTable[])FillSchemaFromReader(dataSet, null, schemaType, srcTable, dataReader);
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Adds a <see cref="T:System.Data.DataTable" /> to the specified <see cref="T:System.Data.DataSet" />.</summary>
	/// <returns>A <see cref="T:System.Data.DataTable" /> object that contains schema information returned from the data source.</returns>
	/// <param name="dataTable">The <see cref="T:System.Data.DataTable" /> to be filled from the <see cref="T:System.Data.IDataReader" />.</param>
	/// <param name="schemaType">One of the <see cref="T:System.Data.SchemaType" /> values.</param>
	/// <param name="dataReader">The <see cref="T:System.Data.IDataReader" /> to be used as the data source when filling the <see cref="T:System.Data.DataTable" />.</param>
	protected virtual DataTable FillSchema(DataTable dataTable, SchemaType schemaType, IDataReader dataReader)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<comm.DataAdapter.FillSchema|API> {0}, dataTable, schemaType, dataReader", ObjectID);
		try
		{
			if (dataTable == null)
			{
				throw ADP.ArgumentNull("dataTable");
			}
			if (SchemaType.Source != schemaType && SchemaType.Mapped != schemaType)
			{
				throw ADP.InvalidSchemaType(schemaType);
			}
			if (dataReader == null || dataReader.IsClosed)
			{
				throw ADP.FillRequires("dataReader");
			}
			return (DataTable)FillSchemaFromReader(null, dataTable, schemaType, null, dataReader);
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	internal object FillSchemaFromReader(DataSet dataset, DataTable datatable, SchemaType schemaType, string srcTable, IDataReader dataReader)
	{
		DataTable[] array = null;
		int num = 0;
		do
		{
			DataReaderContainer dataReaderContainer = DataReaderContainer.Create(dataReader, ReturnProviderSpecificTypes);
			if (0 < dataReaderContainer.FieldCount)
			{
				string sourceTableName = null;
				if (dataset != null)
				{
					sourceTableName = GetSourceTableName(srcTable, num);
					num++;
				}
				SchemaMapping schemaMapping = new SchemaMapping(this, dataset, datatable, dataReaderContainer, keyInfo: true, schemaType, sourceTableName, gettingData: false, null, null);
				if (datatable != null)
				{
					return schemaMapping.DataTable;
				}
				if (schemaMapping.DataTable != null)
				{
					array = ((array != null) ? AddDataTableToArray(array, schemaMapping.DataTable) : new DataTable[1] { schemaMapping.DataTable });
				}
			}
		}
		while (dataReader.NextResult());
		object obj = array;
		if (obj == null && datatable == null)
		{
			obj = Array.Empty<DataTable>();
		}
		return obj;
	}

	/// <summary>Adds or refreshes rows in the <see cref="T:System.Data.DataSet" /> to match those in the data source.</summary>
	/// <returns>The number of rows successfully added to or refreshed in the <see cref="T:System.Data.DataSet" />. This does not include rows affected by statements that do not return rows.</returns>
	/// <param name="dataSet">A <see cref="T:System.Data.DataSet" /> to fill with records and, if necessary, schema. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual int Fill(DataSet dataSet)
	{
		throw ADP.NotSupported();
	}

	/// <summary>Adds or refreshes rows in a specified range in the <see cref="T:System.Data.DataSet" /> to match those in the data source using the <see cref="T:System.Data.DataSet" /> and <see cref="T:System.Data.DataTable" /> names.</summary>
	/// <returns>The number of rows successfully added to or refreshed in the <see cref="T:System.Data.DataSet" />. This does not include rows affected by statements that do not return rows.</returns>
	/// <param name="dataSet">A <see cref="T:System.Data.DataSet" /> to fill with records.</param>
	/// <param name="srcTable">A string indicating the name of the source table.</param>
	/// <param name="dataReader">An instance of <see cref="T:System.Data.IDataReader" />.</param>
	/// <param name="startRecord">The zero-based index of the starting record.</param>
	/// <param name="maxRecords">An integer indicating the maximum number of records.</param>
	protected virtual int Fill(DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<comm.DataAdapter.Fill|API> {0}, dataSet, srcTable, dataReader, startRecord, maxRecords", ObjectID);
		try
		{
			if (dataSet == null)
			{
				throw ADP.FillRequires("dataSet");
			}
			if (string.IsNullOrEmpty(srcTable))
			{
				throw ADP.FillRequiresSourceTableName("srcTable");
			}
			if (dataReader == null)
			{
				throw ADP.FillRequires("dataReader");
			}
			if (startRecord < 0)
			{
				throw ADP.InvalidStartRecord("startRecord", startRecord);
			}
			if (maxRecords < 0)
			{
				throw ADP.InvalidMaxRecords("maxRecords", maxRecords);
			}
			if (dataReader.IsClosed)
			{
				return 0;
			}
			DataReaderContainer dataReader2 = DataReaderContainer.Create(dataReader, ReturnProviderSpecificTypes);
			return FillFromReader(dataSet, null, srcTable, dataReader2, startRecord, maxRecords, null, null);
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	/// <summary>Adds or refreshes rows in the <see cref="T:System.Data.DataTable" /> to match those in the data source using the <see cref="T:System.Data.DataTable" /> name and the specified <see cref="T:System.Data.IDataReader" />.</summary>
	/// <returns>The number of rows successfully added to or refreshed in the <see cref="T:System.Data.DataTable" />. This does not include rows affected by statements that do not return rows.</returns>
	/// <param name="dataTable">A <see cref="T:System.Data.DataTable" /> to fill with records.</param>
	/// <param name="dataReader">An instance of <see cref="T:System.Data.IDataReader" />.</param>
	protected virtual int Fill(DataTable dataTable, IDataReader dataReader)
	{
		DataTable[] dataTables = new DataTable[1] { dataTable };
		return Fill(dataTables, dataReader, 0, 0);
	}

	/// <summary>Adds or refreshes rows in a specified range in the collection of <see cref="T:System.Data.DataTable" /> objects to match those in the data source.</summary>
	/// <returns>The number of rows successfully added to or refreshed in the <see cref="T:System.Data.DataTable" />. This does not include rows affected by statements that do not return rows.</returns>
	/// <param name="dataTables">A collection of <see cref="T:System.Data.DataTable" /> objects to fill with records.</param>
	/// <param name="dataReader">An instance of <see cref="T:System.Data.IDataReader" />.</param>
	/// <param name="startRecord">The zero-based index of the starting record.</param>
	/// <param name="maxRecords">An integer indicating the maximum number of records.</param>
	protected virtual int Fill(DataTable[] dataTables, IDataReader dataReader, int startRecord, int maxRecords)
	{
		long scopeId = DataCommonEventSource.Log.EnterScope("<comm.DataAdapter.Fill|API> {0}, dataTables[], dataReader, startRecord, maxRecords", ObjectID);
		try
		{
			ADP.CheckArgumentLength(dataTables, "dataTables");
			if (dataTables == null || dataTables.Length == 0 || dataTables[0] == null)
			{
				throw ADP.FillRequires("dataTable");
			}
			if (dataReader == null)
			{
				throw ADP.FillRequires("dataReader");
			}
			if (1 < dataTables.Length && (startRecord != 0 || maxRecords != 0))
			{
				throw ADP.NotSupported();
			}
			int result = 0;
			bool flag = false;
			DataSet dataSet = dataTables[0].DataSet;
			try
			{
				if (dataSet != null)
				{
					flag = dataSet.EnforceConstraints;
					dataSet.EnforceConstraints = false;
				}
				for (int i = 0; i < dataTables.Length && !dataReader.IsClosed; i++)
				{
					DataReaderContainer dataReaderContainer = DataReaderContainer.Create(dataReader, ReturnProviderSpecificTypes);
					if (dataReaderContainer.FieldCount <= 0)
					{
						if (i != 0)
						{
							continue;
						}
						bool flag2;
						do
						{
							flag2 = FillNextResult(dataReaderContainer);
						}
						while (flag2 && dataReaderContainer.FieldCount <= 0);
						if (!flag2)
						{
							break;
						}
					}
					if (0 >= i || FillNextResult(dataReaderContainer))
					{
						int num = FillFromReader(null, dataTables[i], null, dataReaderContainer, startRecord, maxRecords, null, null);
						if (i == 0)
						{
							result = num;
						}
						continue;
					}
					break;
				}
			}
			catch (ConstraintException)
			{
				flag = false;
				throw;
			}
			finally
			{
				if (flag)
				{
					dataSet.EnforceConstraints = true;
				}
			}
			return result;
		}
		finally
		{
			DataCommonEventSource.Log.ExitScope(scopeId);
		}
	}

	internal int FillFromReader(DataSet dataset, DataTable datatable, string srcTable, DataReaderContainer dataReader, int startRecord, int maxRecords, DataColumn parentChapterColumn, object parentChapterValue)
	{
		int result = 0;
		int num = 0;
		do
		{
			if (0 >= dataReader.FieldCount)
			{
				continue;
			}
			SchemaMapping schemaMapping = FillMapping(dataset, datatable, srcTable, dataReader, num, parentChapterColumn, parentChapterValue);
			num++;
			if (schemaMapping == null || schemaMapping.DataValues == null || schemaMapping.DataTable == null)
			{
				continue;
			}
			schemaMapping.DataTable.BeginLoadData();
			try
			{
				if (1 == num && (0 < startRecord || 0 < maxRecords))
				{
					result = FillLoadDataRowChunk(schemaMapping, startRecord, maxRecords);
				}
				else
				{
					int num2 = FillLoadDataRow(schemaMapping);
					if (1 == num)
					{
						result = num2;
					}
				}
			}
			finally
			{
				schemaMapping.DataTable.EndLoadData();
			}
			if (datatable != null)
			{
				break;
			}
		}
		while (FillNextResult(dataReader));
		return result;
	}

	private int FillLoadDataRowChunk(SchemaMapping mapping, int startRecord, int maxRecords)
	{
		DataReaderContainer dataReader = mapping.DataReader;
		while (0 < startRecord)
		{
			if (!dataReader.Read())
			{
				return 0;
			}
			startRecord--;
		}
		int num = 0;
		if (0 < maxRecords)
		{
			while (num < maxRecords && dataReader.Read())
			{
				if (_hasFillErrorHandler)
				{
					try
					{
						mapping.LoadDataRowWithClear();
						num++;
					}
					catch (Exception e) when (ADP.IsCatchableExceptionType(e))
					{
						ADP.TraceExceptionForCapture(e);
						OnFillErrorHandler(e, mapping.DataTable, mapping.DataValues);
					}
				}
				else
				{
					mapping.LoadDataRow();
					num++;
				}
			}
		}
		else
		{
			num = FillLoadDataRow(mapping);
		}
		return num;
	}

	private int FillLoadDataRow(SchemaMapping mapping)
	{
		int num = 0;
		DataReaderContainer dataReader = mapping.DataReader;
		if (_hasFillErrorHandler)
		{
			while (dataReader.Read())
			{
				try
				{
					mapping.LoadDataRowWithClear();
					num++;
				}
				catch (Exception e) when (ADP.IsCatchableExceptionType(e))
				{
					ADP.TraceExceptionForCapture(e);
					OnFillErrorHandler(e, mapping.DataTable, mapping.DataValues);
				}
			}
		}
		else
		{
			while (dataReader.Read())
			{
				mapping.LoadDataRow();
				num++;
			}
		}
		return num;
	}

	private SchemaMapping FillMappingInternal(DataSet dataset, DataTable datatable, string srcTable, DataReaderContainer dataReader, int schemaCount, DataColumn parentChapterColumn, object parentChapterValue)
	{
		bool keyInfo = MissingSchemaAction.AddWithKey == MissingSchemaAction;
		string sourceTableName = null;
		if (dataset != null)
		{
			sourceTableName = GetSourceTableName(srcTable, schemaCount);
		}
		return new SchemaMapping(this, dataset, datatable, dataReader, keyInfo, SchemaType.Mapped, sourceTableName, gettingData: true, parentChapterColumn, parentChapterValue);
	}

	private SchemaMapping FillMapping(DataSet dataset, DataTable datatable, string srcTable, DataReaderContainer dataReader, int schemaCount, DataColumn parentChapterColumn, object parentChapterValue)
	{
		SchemaMapping result = null;
		if (_hasFillErrorHandler)
		{
			try
			{
				result = FillMappingInternal(dataset, datatable, srcTable, dataReader, schemaCount, parentChapterColumn, parentChapterValue);
			}
			catch (Exception e) when (ADP.IsCatchableExceptionType(e))
			{
				ADP.TraceExceptionForCapture(e);
				OnFillErrorHandler(e, null, null);
			}
		}
		else
		{
			result = FillMappingInternal(dataset, datatable, srcTable, dataReader, schemaCount, parentChapterColumn, parentChapterValue);
		}
		return result;
	}

	private bool FillNextResult(DataReaderContainer dataReader)
	{
		bool result = true;
		if (_hasFillErrorHandler)
		{
			try
			{
				result = dataReader.NextResult();
			}
			catch (Exception e) when (ADP.IsCatchableExceptionType(e))
			{
				ADP.TraceExceptionForCapture(e);
				OnFillErrorHandler(e, null, null);
			}
		}
		else
		{
			result = dataReader.NextResult();
		}
		return result;
	}

	/// <summary>Gets the parameters set by the user when executing an SQL SELECT statement.</summary>
	/// <returns>An array of <see cref="T:System.Data.IDataParameter" /> objects that contains the parameters set by the user.</returns>
	/// <filterpriority>2</filterpriority>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public virtual IDataParameter[] GetFillParameters()
	{
		return Array.Empty<IDataParameter>();
	}

	internal DataTableMapping GetTableMappingBySchemaAction(string sourceTableName, string dataSetTableName, MissingMappingAction mappingAction)
	{
		return DataTableMappingCollection.GetTableMappingBySchemaAction(_tableMappings, sourceTableName, dataSetTableName, mappingAction);
	}

	internal int IndexOfDataSetTable(string dataSetTable)
	{
		if (_tableMappings != null)
		{
			return TableMappings.IndexOfDataSetTable(dataSetTable);
		}
		return -1;
	}

	/// <summary>Invoked when an error occurs during a Fill.</summary>
	/// <param name="value">A <see cref="T:System.Data.FillErrorEventArgs" /> object.</param>
	protected virtual void OnFillError(FillErrorEventArgs value)
	{
		((FillErrorEventHandler)base.Events[s_eventFillError])?.Invoke(this, value);
	}

	private void OnFillErrorHandler(Exception e, DataTable dataTable, object[] dataValues)
	{
		FillErrorEventArgs e2 = new FillErrorEventArgs(dataTable, dataValues);
		e2.Errors = e;
		OnFillError(e2);
		if (!e2.Continue)
		{
			if (e2.Errors != null)
			{
				throw e2.Errors;
			}
			throw e;
		}
	}

	/// <summary>Calls the respective INSERT, UPDATE, or DELETE statements for each inserted, updated, or deleted row in the specified <see cref="T:System.Data.DataSet" /> from a <see cref="T:System.Data.DataTable" /> named "Table."</summary>
	/// <returns>The number of rows successfully updated from the <see cref="T:System.Data.DataSet" />.</returns>
	/// <param name="dataSet">The <see cref="T:System.Data.DataSet" /> used to update the data source. </param>
	/// <exception cref="T:System.InvalidOperationException">The source table is invalid. </exception>
	/// <exception cref="T:System.Data.DBConcurrencyException">An attempt to execute an INSERT, UPDATE, or DELETE statement resulted in zero records affected. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public virtual int Update(DataSet dataSet)
	{
		throw ADP.NotSupported();
	}

	private static DataTable[] AddDataTableToArray(DataTable[] tables, DataTable newTable)
	{
		for (int i = 0; i < tables.Length; i++)
		{
			if (tables[i] == newTable)
			{
				return tables;
			}
		}
		DataTable[] array = new DataTable[tables.Length + 1];
		for (int j = 0; j < tables.Length; j++)
		{
			array[j] = tables[j];
		}
		array[tables.Length] = newTable;
		return array;
	}

	private static string GetSourceTableName(string srcTable, int index)
	{
		if (index == 0)
		{
			return srcTable;
		}
		return srcTable + index.ToString(CultureInfo.InvariantCulture);
	}
}
