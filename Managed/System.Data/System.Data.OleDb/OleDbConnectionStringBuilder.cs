using System.Collections;
using System.Data.Common;

namespace System.Data.OleDb;

/// <summary>Provides a simple way to create and manage the contents of connection strings used by the <see cref="T:System.Data.OleDb.OleDbConnection" /> class.</summary>
/// <filterpriority>2</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public sealed class OleDbConnectionStringBuilder : DbConnectionStringBuilder
{
	/// <summary>Gets or sets the name of the data source to connect to.</summary>
	/// <returns>The value of the <see cref="P:System.Data.OleDb.OleDbConnectionStringBuilder.DataSource" /> property, or String.Empty if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string DataSource
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets the name of the Universal Data Link (UDL) file for connecting to the data source.</summary>
	/// <returns>The value of the <see cref="P:System.Data.OleDb.OleDbConnectionStringBuilder.FileName" /> property, or String.Empty if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string FileName
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	public object Item
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> that contains the keys in the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that contains the keys in the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" />.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Security.Permissions.UIPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Window="SafeSubWindows" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override ICollection Keys
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets the value to be passed for the OLE DB Services key within the connection string.</summary>
	/// <returns>Returns the value corresponding to the OLE DB Services key within the connection string. By default, the value is -13.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public int OleDbServices
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets a Boolean value that indicates whether security-sensitive information, such as the password, is returned as part of the connection if the connection is open or has ever been in an open state.</summary>
	/// <returns>The value of the <see cref="P:System.Data.OleDb.OleDbConnectionStringBuilder.PersistSecurityInfo" /> property, or false if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool PersistSecurityInfo
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Gets or sets a string that contains the name of the data provider associated with the internal connection string.</summary>
	/// <returns>The value of the <see cref="P:System.Data.OleDb.OleDbConnectionStringBuilder.Provider" /> property, or String.Empty if none has been supplied.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public string Provider
	{
		get
		{
			throw ADP.OleDb();
		}
		set
		{
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" /> class.</summary>
	public OleDbConnectionStringBuilder()
	{
		throw ADP.OleDb();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" /> class. The provided connection string provides the data for the instance's internal connection information.</summary>
	/// <param name="connectionString">The basis for the object's internal connection information. Parsed into key/value pairs.</param>
	/// <exception cref="T:System.ArgumentException">The connection string is incorrectly formatted (perhaps missing the required "=" within a key/value pair).</exception>
	public OleDbConnectionStringBuilder(string connectionString)
	{
		throw ADP.OleDb();
	}

	/// <summary>Clears the contents of the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" /> instance.</summary>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	/// </PermissionSet>
	public override void Clear()
	{
		throw ADP.OleDb();
	}

	/// <summary>Determines whether the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" /> contains a specific key.</summary>
	/// <returns>true if the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" /> contains an element that has the specified key; otherwise false.</returns>
	/// <param name="keyword">The key to locate in the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is null (Nothing in Visual Basic).</exception>
	/// <filterpriority>1</filterpriority>
	public override bool ContainsKey(string keyword)
	{
		throw ADP.OleDb();
	}

	protected override void GetProperties(Hashtable propertyDescriptors)
	{
		throw ADP.OleDb();
	}

	/// <summary>Removes the entry with the specified key from the <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" /> instance.</summary>
	/// <returns>true if the key existed within the connection string and was removed, false if the key did not exist.</returns>
	/// <param name="keyword">The key of the key/value pair to be removed from the connection string in this <see cref="T:System.Data.OleDb.OleDbConnectionStringBuilder" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is null (Nothing in Visual Basic).</exception>
	/// <filterpriority>1</filterpriority>
	public override bool Remove(string keyword)
	{
		throw ADP.OleDb();
	}

	public bool TryGetValue(string keyword, object value)
	{
		throw ADP.OleDb();
	}
}
