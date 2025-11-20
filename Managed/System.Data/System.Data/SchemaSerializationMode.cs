namespace System.Data;

/// <summary>Indicates the schema serialization mode for a typed <see cref="T:System.Data.DataSet" />.</summary>
public enum SchemaSerializationMode
{
	/// <summary>Includes schema serialization for a typed <see cref="T:System.Data.DataSet" />. The default.</summary>
	IncludeSchema = 1,
	/// <summary>Skips schema serialization for a typed <see cref="T:System.Data.DataSet" />.</summary>
	ExcludeSchema
}
