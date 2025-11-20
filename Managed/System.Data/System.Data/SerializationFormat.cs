namespace System.Data;

/// <summary>Determines the serialization format for a <see cref="T:System.Data.DataSet" />.</summary>
/// <filterpriority>2</filterpriority>
public enum SerializationFormat
{
	/// <summary>Serialize as XML content. The default.</summary>
	Xml,
	/// <summary>Serialize as binary content. Available in ADO.NET 2.0 only.</summary>
	Binary
}
