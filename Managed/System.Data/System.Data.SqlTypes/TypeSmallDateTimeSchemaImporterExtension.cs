namespace System.Data.SqlTypes;

/// <summary>The TypeSmallDateTimeSchemaImporterExtension class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
public sealed class TypeSmallDateTimeSchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the TypeSmallDateTimeSchemaImporterExtension class.</summary>
	public TypeSmallDateTimeSchemaImporterExtension()
		: base("smalldatetime", "System.Data.SqlTypes.SqlDateTime")
	{
	}
}
