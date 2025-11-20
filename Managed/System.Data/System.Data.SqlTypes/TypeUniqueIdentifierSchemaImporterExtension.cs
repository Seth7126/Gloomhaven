namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeUniqueIdentifierSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeUniqueIdentifierSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeUniqueIdentifierSchemaImporterExtension" /> class.</summary>
	public TypeUniqueIdentifierSchemaImporterExtension()
		: base("uniqueidentifier", "System.Data.SqlTypes.SqlGuid")
	{
	}
}
