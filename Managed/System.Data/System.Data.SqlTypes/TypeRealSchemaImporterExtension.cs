namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeRealSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeRealSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeRealSchemaImporterExtension" /> class.</summary>
	public TypeRealSchemaImporterExtension()
		: base("real", "System.Data.SqlTypes.SqlSingle")
	{
	}
}
