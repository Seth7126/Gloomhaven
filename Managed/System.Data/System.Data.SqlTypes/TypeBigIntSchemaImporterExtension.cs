namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeBigIntSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeBigIntSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeBigIntSchemaImporterExtension" /> class.</summary>
	public TypeBigIntSchemaImporterExtension()
		: base("bigint", "System.Data.SqlTypes.SqlInt64")
	{
	}
}
