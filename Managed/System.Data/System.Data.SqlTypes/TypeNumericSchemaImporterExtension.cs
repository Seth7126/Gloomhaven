namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeNumericSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeNumericSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeNumericSchemaImporterExtension" /> class.</summary>
	public TypeNumericSchemaImporterExtension()
		: base("numeric", "System.Data.SqlTypes.SqlDecimal", direct: false)
	{
	}
}
