namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeDecimalSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeDecimalSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeDecimalSchemaImporterExtension" /> class.</summary>
	public TypeDecimalSchemaImporterExtension()
		: base("decimal", "System.Data.SqlTypes.SqlDecimal", direct: false)
	{
	}
}
