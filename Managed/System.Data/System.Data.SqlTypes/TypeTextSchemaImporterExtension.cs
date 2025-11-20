namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeTextSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeTextSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeTextSchemaImporterExtension" /> class.</summary>
	public TypeTextSchemaImporterExtension()
		: base("text", "System.Data.SqlTypes.SqlString", direct: false)
	{
	}
}
