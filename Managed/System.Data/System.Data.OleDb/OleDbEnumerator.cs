using System.Data.Common;

namespace System.Data.OleDb;

/// <summary>Provides a mechanism for enumerating all available OLE DB providers within the local network.</summary>
/// <filterpriority>2</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public sealed class OleDbEnumerator
{
	/// <summary>Retrieves a <see cref="T:System.Data.DataTable" /> that contains information about all visible OLE DB providers.</summary>
	/// <returns>Returns a <see cref="T:System.Data.DataTable" /> that contains information about the visible OLE DB providers.</returns>
	/// <exception cref="T:System.InvalidCastException">The provider does not support ISourcesRowset.</exception>
	/// <exception cref="T:System.Data.OleDb.OleDbException">Exception has occurred in the underlying provider.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public DataTable GetElements()
	{
		throw ADP.OleDb();
	}

	/// <summary>Uses a specific OLE DB enumerator to return an <see cref="T:System.Data.OleDb.OleDbDataReader" /> that contains information about the currently installed OLE DB providers, without requiring an instance of the <see cref="T:System.Data.OleDb.OleDbEnumerator" /> class.</summary>
	/// <returns>Returns an <see cref="T:System.Data.OleDb.OleDbDataReader" /> that contains information about the requested OLE DB providers, using the specified OLE DB enumerator.</returns>
	/// <param name="type">A <see cref="T:System.Type" />.</param>
	/// <exception cref="T:System.InvalidCastException">The provider does not support ISourcesRowset.</exception>
	/// <exception cref="T:System.Data.OleDb.OleDbException">An exception has occurred in the underlying provider.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static OleDbDataReader GetEnumerator(Type type)
	{
		throw ADP.OleDb();
	}

	/// <summary>Returns an <see cref="T:System.Data.OleDb.OleDbDataReader" /> that contains information about the currently installed OLE DB providers, without requiring an instance of the <see cref="T:System.Data.OleDb.OleDbEnumerator" /> class.</summary>
	/// <returns>Returns a <see cref="T:System.Data.OleDb.OleDbDataReader" /> that contains information about the visible OLE DB providers.</returns>
	/// <exception cref="T:System.InvalidCastException">The provider does not support ISourcesRowset.</exception>
	/// <exception cref="T:System.Data.OleDb.OleDbException">Exception has occurred in the underlying provider.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static OleDbDataReader GetRootEnumerator()
	{
		throw ADP.OleDb();
	}

	/// <summary>Creates an instance of the <see cref="T:System.Data.OleDb.OleDbEnumerator" /> class.</summary>
	public OleDbEnumerator()
	{
	}
}
