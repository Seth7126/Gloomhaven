namespace System.Data;

/// <summary>Specifies how a <see cref="T:System.Data.DataColumn" /> is mapped.</summary>
/// <filterpriority>2</filterpriority>
public enum MappingType
{
	/// <summary>The column is mapped to an XML element.</summary>
	Element = 1,
	/// <summary>The column is mapped to an XML attribute.</summary>
	Attribute,
	/// <summary>The column is mapped to an <see cref="T:System.Xml.XmlText" /> node.</summary>
	SimpleContent,
	/// <summary>The column is mapped to an internal structure.</summary>
	Hidden
}
