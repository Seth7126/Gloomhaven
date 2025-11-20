namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeTinyIntSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeTinyIntSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeTinyIntSchemaImporterExtension" /> class.</summary>
	public TypeTinyIntSchemaImporterExtension()
		: base("tinyint", "System.Data.SqlTypes.SqlByte")
	{
	}
}
