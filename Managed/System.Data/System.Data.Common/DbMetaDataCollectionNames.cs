namespace System.Data.Common;

/// <summary>Provides a list of constants for the well-known MetaDataCollections: DataSourceInformation, DataTypes, MetaDataCollections, ReservedWords, and Restrictions.</summary>
/// <filterpriority>2</filterpriority>
public static class DbMetaDataCollectionNames
{
	/// <summary>A constant for use with the <see cref="M:System.Data.Common.DbConnection.GetSchema" /> method that represents the MetaDataCollections collection.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly string MetaDataCollections = "MetaDataCollections";

	/// <summary>A constant for use with the <see cref="M:System.Data.Common.DbConnection.GetSchema" /> method that represents the DataSourceInformation collection.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly string DataSourceInformation = "DataSourceInformation";

	/// <summary>A constant for use with the <see cref="M:System.Data.Common.DbConnection.GetSchema" /> method that represents the DataTypes collection.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly string DataTypes = "DataTypes";

	/// <summary>A constant for use with the <see cref="M:System.Data.Common.DbConnection.GetSchema" /> method that represents the Restrictions collection.  </summary>
	/// <filterpriority>2</filterpriority>
	public static readonly string Restrictions = "Restrictions";

	/// <summary>A constant for use with the <see cref="M:System.Data.Common.DbConnection.GetSchema" /> method that represents the ReservedWords collection.</summary>
	/// <filterpriority>2</filterpriority>
	public static readonly string ReservedWords = "ReservedWords";
}
