namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeBitSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeBitSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeBitSchemaImporterExtension" /> class. </summary>
	public TypeBitSchemaImporterExtension()
		: base("bit", "System.Data.SqlTypes.SqlBoolean")
	{
	}
}
