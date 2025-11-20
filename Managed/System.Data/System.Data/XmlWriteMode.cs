namespace System.Data;

/// <summary>Specifies how to write XML data and a relational schema from a <see cref="T:System.Data.DataSet" />.</summary>
/// <filterpriority>2</filterpriority>
public enum XmlWriteMode
{
	/// <summary>Writes the current contents of the <see cref="T:System.Data.DataSet" /> as XML data with the relational structure as inline XSD schema. If the <see cref="T:System.Data.DataSet" /> has only a schema with no data, only the inline schema is written. If the <see cref="T:System.Data.DataSet" /> does not have a current schema, nothing is written.</summary>
	WriteSchema,
	/// <summary>Writes the current contents of the <see cref="T:System.Data.DataSet" /> as XML data, without an XSD schema. If no data is loaded into the <see cref="T:System.Data.DataSet" />, nothing is written.</summary>
	IgnoreSchema,
	/// <summary>Writes the entire <see cref="T:System.Data.DataSet" /> as a DiffGram, including original and current values. To generate a DiffGram containing only changed values, call <see cref="M:System.Data.DataSet.GetChanges" />, and then call <see cref="M:System.Data.DataSet.WriteXml(System.IO.Stream)" /> as a DiffGram on the returned <see cref="T:System.Data.DataSet" />.</summary>
	DiffGram
}
