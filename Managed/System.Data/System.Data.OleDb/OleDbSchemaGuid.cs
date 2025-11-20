namespace System.Data.OleDb;

/// <summary>Returns the type of schema table specified by the <see cref="M:System.Data.OleDb.OleDbConnection.GetOleDbSchemaTable(System.Guid,System.Object[])" /> method.</summary>
/// <filterpriority>2</filterpriority>
[System.MonoTODO("OleDb is not implemented.")]
public sealed class OleDbSchemaGuid
{
	/// <summary>Returns the assertions defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Assertions;

	/// <summary>Returns the physical attributes associated with catalogs accessible from the data source. Returns the assertions defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Catalogs;

	/// <summary>Returns the character sets defined in the catalog that is accessible to a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Character_Sets;

	/// <summary>Returns the check constraints defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Check_Constraints;

	/// <summary>Returns the check constraints defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Check_Constraints_By_Table;

	/// <summary>Returns the character collations defined in the catalog that is accessible to a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Collations;

	/// <summary>Returns the columns defined in the catalog that are dependent on a domain defined in the catalog and owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Column_Domain_Usage;

	/// <summary>Returns the privileges on columns of tables defined in the catalog that are available to or granted by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Column_Privileges;

	/// <summary>Returns the columns of tables (including views) defined in the catalog that is accessible to a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Columns;

	/// <summary>Returns the columns used by referential constraints, unique constraints, check constraints, and assertions, defined in the catalog and owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Constraint_Column_Usage;

	/// <summary>Returns the tables that are used by referential constraints, unique constraints, check constraints, and assertions defined in the catalog and owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Constraint_Table_Usage;

	/// <summary>Returns a list of provider-specific keywords.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid DbInfoKeywords;

	/// <summary>Returns a list of provider-specific literals used in text commands.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid DbInfoLiterals;

	/// <summary>Returns the foreign key columns defined in the catalog by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Foreign_Keys;

	/// <summary>Returns the indexes defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Indexes;

	/// <summary>Returns the columns defined in the catalog that is constrained as keys by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Key_Column_Usage;

	/// <summary>Returns the primary key columns defined in the catalog by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Primary_Keys;

	/// <summary>Returns information about the columns of rowsets returned by procedures.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Procedure_Columns;

	/// <summary>Returns information about the parameters and return codes of procedures.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Procedure_Parameters;

	/// <summary>Returns the procedures defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Procedures;

	/// <summary>Returns the base data types supported by the .NET Framework Data Provider for OLE DB.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Provider_Types;

	/// <summary>Returns the referential constraints defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Referential_Constraints;

	/// <summary>Returns a list of schema rowsets, identified by their GUIDs, and a pointer to the descriptions of the restriction columns.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid SchemaGuids;

	/// <summary>Returns the schema objects that are owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Schemata;

	/// <summary>Returns the conformance levels, options, and dialects supported by the SQL-implementation processing data defined in the catalog.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Sql_Languages;

	/// <summary>Returns the statistics defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Statistics;

	/// <summary>Returns the table constraints defined in the catalog that is owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Table_Constraints;

	/// <summary>Returns the privileges on tables defined in the catalog that are available to, or granted by, a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Table_Privileges;

	/// <summary>Describes the available set of statistics on tables in the provider.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Table_Statistics;

	/// <summary>Returns the tables (including views) defined in the catalog that are accessible to a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Tables;

	/// <summary>Returns the tables (including views) that are accessible to a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Tables_Info;

	/// <summary>Returns the character translations defined in the catalog that is accessible to a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Translations;

	/// <summary>Identifies the trustees defined in the data source.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Trustee;

	/// <summary>Returns the USAGE privileges on objects defined in the catalog that are available to or granted by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Usage_Privileges;

	/// <summary>Returns the columns on which viewed tables depend, as defined in the catalog and owned by a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid View_Column_Usage;

	/// <summary>Returns the tables on which viewed tables, defined in the catalog and owned by a given user, are dependent.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid View_Table_Usage;

	/// <summary>Returns the views defined in the catalog that is accessible to a given user.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly Guid Views;

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.OleDb.OleDbSchemaGuid" /> class.</summary>
	public OleDbSchemaGuid()
	{
	}
}
