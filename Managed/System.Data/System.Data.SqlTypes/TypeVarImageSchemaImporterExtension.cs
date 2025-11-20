namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeVarImageSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeVarImageSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeVarImageSchemaImporterExtension" /> class.</summary>
	public TypeVarImageSchemaImporterExtension()
		: base("image", "System.Data.SqlTypes.SqlBinary", direct: false)
	{
	}
}
