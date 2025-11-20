namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeSmallIntSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeSmallIntSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeSmallIntSchemaImporterExtension" /> class.</summary>
	public TypeSmallIntSchemaImporterExtension()
		: base("smallint", "System.Data.SqlTypes.SqlInt16")
	{
	}
}
