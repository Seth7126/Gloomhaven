namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.TypeMoneySchemaImporterExtension" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</summary>
public sealed class TypeMoneySchemaImporterExtension : SqlTypesSchemaImporterExtensionHelper
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Data.SqlTypes.TypeMoneySchemaImporterExtension" /> class.</summary>
	public TypeMoneySchemaImporterExtension()
		: base("money", "System.Data.SqlTypes.SqlMoney")
	{
	}
}
