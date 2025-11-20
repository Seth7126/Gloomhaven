namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeVarBinarySchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeVarBinarySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeVarBinarySchemaImporterExtension" /> class.</summary>
	public TypeVarBinarySchemaImporterExtension()
		: base("varbinary", "System.Data.SqlTypes.SqlBinary", direct: false)
	{
	}
}
