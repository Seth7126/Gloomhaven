using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;

namespace System.Data;

/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
/// <filterpriority>2</filterpriority>
public class DataSetSchemaImporterExtension : SchemaImporterExtension
{
	private Hashtable importedTypes = new Hashtable();

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</returns>
	/// <param name="name">
	///   <paramref name="name" />
	/// </param>
	/// <param name="schemaNamespace">
	///   <paramref name="schemaNamespace" />
	/// </param>
	/// <param name="context">
	///   <paramref name="context" />
	/// </param>
	/// <param name="schemas">
	///   <paramref name="schemas" />
	/// </param>
	/// <param name="importer">
	///   <paramref name="importer" />
	/// </param>
	/// <param name="compileUnit">
	///   <paramref name="compileUnit" />
	/// </param>
	/// <param name="mainNamespace">
	///   <paramref name="mainNamespace" />
	/// </param>
	/// <param name="options">
	///   <paramref name="options" />
	/// </param>
	/// <param name="codeProvider">
	///   <paramref name="codeProvider" />
	/// </param>
	public override string ImportSchemaType(string name, string schemaNamespace, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
	{
		IList schemas2 = schemas.GetSchemas(schemaNamespace);
		if (schemas2.Count != 1)
		{
			return null;
		}
		if (!(schemas2[0] is XmlSchema xmlSchema))
		{
			return null;
		}
		XmlSchemaType type = (XmlSchemaType)xmlSchema.SchemaTypes[new XmlQualifiedName(name, schemaNamespace)];
		return ImportSchemaType(type, context, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</returns>
	/// <param name="type">
	///   <paramref name="type" />
	/// </param>
	/// <param name="context">
	///   <paramref name="context" />
	/// </param>
	/// <param name="schemas">
	///   <paramref name="schemas" />
	/// </param>
	/// <param name="importer">
	///   <paramref name="importer" />
	/// </param>
	/// <param name="compileUnit">
	///   <paramref name="compileUnit" />
	/// </param>
	/// <param name="mainNamespace">
	///   <paramref name="mainNamespace" />
	/// </param>
	/// <param name="options">
	///   <paramref name="options" />
	/// </param>
	/// <param name="codeProvider">
	///   <paramref name="codeProvider" />
	/// </param>
	public override string ImportSchemaType(XmlSchemaType type, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
	{
		if (type == null)
		{
			return null;
		}
		if (importedTypes[type] != null)
		{
			mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
			compileUnit.ReferencedAssemblies.Add("System.Data.dll");
			return (string)importedTypes[type];
		}
		if (!(context is XmlSchemaElement))
		{
			return null;
		}
		_ = (XmlSchemaElement)context;
		if (type is XmlSchemaComplexType)
		{
			XmlSchemaComplexType xmlSchemaComplexType = (XmlSchemaComplexType)type;
			if (xmlSchemaComplexType.Particle is XmlSchemaSequence)
			{
				XmlSchemaObjectCollection items = ((XmlSchemaSequence)xmlSchemaComplexType.Particle).Items;
				if (2 == items.Count && items[0] is XmlSchemaAny && items[1] is XmlSchemaAny)
				{
					XmlSchemaAny xmlSchemaAny = (XmlSchemaAny)items[0];
					XmlSchemaAny xmlSchemaAny2 = (XmlSchemaAny)items[1];
					if (xmlSchemaAny.Namespace == "http://www.w3.org/2001/XMLSchema" && xmlSchemaAny2.Namespace == "urn:schemas-microsoft-com:xml-diffgram-v1")
					{
						string text = null;
						foreach (XmlSchemaAttribute attribute in xmlSchemaComplexType.Attributes)
						{
							if (attribute.Name == "namespace")
							{
								text = attribute.FixedValue.Trim();
								break;
							}
						}
						bool flag = false;
						if (((XmlSchemaSequence)xmlSchemaComplexType.Particle).MaxOccurs == decimal.MaxValue)
						{
							flag = true;
						}
						else
						{
							if (!(xmlSchemaAny.MaxOccurs == decimal.MaxValue))
							{
								return null;
							}
							flag = false;
						}
						if (text == null)
						{
							string text2 = (flag ? typeof(DataSet).FullName : typeof(DataTable).FullName);
							importedTypes.Add(type, text2);
							mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
							compileUnit.ReferencedAssemblies.Add("System.Data.dll");
							return text2;
						}
						foreach (XmlSchema schema in schemas.GetSchemas(text))
						{
							if (schema != null && schema.Id != null)
							{
								XmlSchemaElement xmlSchemaElement = FindDataSetElement(schema);
								if (xmlSchemaElement != null)
								{
									return ImportSchemaType(xmlSchemaElement.SchemaType, xmlSchemaElement, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
								}
							}
						}
						return null;
					}
				}
			}
			if (xmlSchemaComplexType.Particle is XmlSchemaSequence || xmlSchemaComplexType.Particle is XmlSchemaAll)
			{
				XmlSchemaObjectCollection items2 = ((XmlSchemaGroupBase)xmlSchemaComplexType.Particle).Items;
				if (items2.Count == 2)
				{
					if (!(items2[0] is XmlSchemaElement) || !(items2[1] is XmlSchemaAny))
					{
						return null;
					}
					XmlSchemaElement xmlSchemaElement2 = (XmlSchemaElement)items2[0];
					if (!(xmlSchemaElement2.RefName.Name == "schema") || !(xmlSchemaElement2.RefName.Namespace == "http://www.w3.org/2001/XMLSchema"))
					{
						return null;
					}
					string fullName = typeof(DataSet).FullName;
					importedTypes.Add(type, fullName);
					mainNamespace.Imports.Add(new CodeNamespaceImport(typeof(DataSet).Namespace));
					compileUnit.ReferencedAssemblies.Add("System.Data.dll");
					return fullName;
				}
				if (1 == items2.Count && items2[0] is XmlSchemaAny { Namespace: not null } xmlSchemaAny3 && xmlSchemaAny3.Namespace.IndexOfAny(new char[2] { '#', ' ' }) < 0)
				{
					foreach (XmlSchema schema2 in schemas.GetSchemas(xmlSchemaAny3.Namespace))
					{
						if (schema2 != null && schema2.Id != null)
						{
							XmlSchemaElement xmlSchemaElement3 = FindDataSetElement(schema2);
							if (xmlSchemaElement3 != null)
							{
								return ImportSchemaType(xmlSchemaElement3.SchemaType, xmlSchemaElement3, schemas, importer, compileUnit, mainNamespace, options, codeProvider);
							}
						}
					}
				}
			}
		}
		return null;
	}

	internal XmlSchemaElement FindDataSetElement(XmlSchema schema)
	{
		foreach (XmlSchemaObject item in schema.Items)
		{
			if (item is XmlSchemaElement && IsDataSet((XmlSchemaElement)item))
			{
				return (XmlSchemaElement)item;
			}
		}
		return null;
	}

	internal string GenerateTypedDataSet(XmlSchemaElement element, XmlSchemas schemas, CodeNamespace codeNamespace, StringCollection references, CodeDomProvider codeProvider)
	{
		if (element == null)
		{
			return null;
		}
		if (importedTypes[element.SchemaType] != null)
		{
			return (string)importedTypes[element.SchemaType];
		}
		IList schemas2 = schemas.GetSchemas(element.QualifiedName.Namespace);
		if (schemas2.Count != 1)
		{
			return null;
		}
		if (!(schemas2[0] is XmlSchema xmlSchema))
		{
			return null;
		}
		DataSet dataSet = new DataSet();
		using (MemoryStream memoryStream = new MemoryStream())
		{
			xmlSchema.Write(memoryStream);
			memoryStream.Position = 0L;
			dataSet.ReadXmlSchema(memoryStream);
		}
		string name = new TypedDataSetGenerator().GenerateCode(dataSet, codeNamespace, codeProvider.CreateGenerator()).Name;
		importedTypes.Add(element.SchemaType, name);
		references.Add("System.Data.dll");
		return name;
	}

	internal static bool IsDataSet(XmlSchemaElement e)
	{
		if (e.UnhandledAttributes != null)
		{
			XmlAttribute[] unhandledAttributes = e.UnhandledAttributes;
			foreach (XmlAttribute xmlAttribute in unhandledAttributes)
			{
				if (xmlAttribute.LocalName == "IsDataSet" && xmlAttribute.NamespaceURI == "urn:schemas-microsoft-com:xml-msdata" && (xmlAttribute.Value == "True" || xmlAttribute.Value == "true" || xmlAttribute.Value == "1"))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.</summary>
	public DataSetSchemaImporterExtension()
	{
	}
}
