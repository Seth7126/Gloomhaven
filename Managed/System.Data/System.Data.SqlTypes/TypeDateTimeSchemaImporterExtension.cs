namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeDateTimeSchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeDateTimeSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeDateTimeSchemaImporterExtension" /> class.</summary>
	public TypeDateTimeSchemaImporterExtension()
		: base("datetime", "System.Data.SqlTypes.SqlDateTime")
	{
	}
}
