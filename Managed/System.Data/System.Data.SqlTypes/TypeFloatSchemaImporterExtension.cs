namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeFloatSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeFloatSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeFloatSchemaImporterExtension" /> class.</summary>
	public TypeFloatSchemaImporterExtension()
		: base("float", "System.Data.SqlTypes.SqlDouble")
	{
	}
}
