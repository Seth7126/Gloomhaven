namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeVarCharSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeVarCharSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeVarCharSchemaImporterExtension" /> class.</summary>
	public TypeVarCharSchemaImporterExtension()
		: base("varchar", "System.Data.SqlTypes.SqlString", direct: false)
	{
	}
}
