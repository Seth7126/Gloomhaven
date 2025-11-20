using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;

namespace System.Data.SqlTypes;

/// <summary>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</summary>
public class SqlTypesSchemaImporterExtensionHelper : SchemaImporterExtension
{
	private string m_name;

	private string m_targetNamespace;

	private string[] m_references;

	private CodeNamespaceImport[] m_namespaceImports;

	private string m_destinationType;

	private bool m_direct;

	/// <summary>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality. </summary>
	protected static readonly string SqlTypesNamespace = "http://schemas.microsoft.com/sqlserver/2004/sqltypes";

	/// <summary>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</summary>
	/// <param name="name">The name as a string.</param>
	/// <param name="targetNamespace">The target namespace.</param>
	/// <param name="references">String array of references.</param>
	/// <param name="namespaceImports">Array of CodeNamespaceImport objects.</param>
	/// <param name="destinationType">The destination type as a string.</param>
	/// <param name="direct">A Boolean for direct.</param>
	public SqlTypesSchemaImporterExtensionHelper(string name, string targetNamespace, string[] references, CodeNamespaceImport[] namespaceImports, string destinationType, bool direct)
	{
		Init(name, targetNamespace, references, namespaceImports, destinationType, direct);
	}

	/// <summary>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</summary>
	/// <param name="name">The name as a string.</param>
	/// <param name="destinationType">The destination type as a string.</param>
	public SqlTypesSchemaImporterExtensionHelper(string name, string destinationType)
	{
		Init(name, SqlTypesNamespace, null, null, destinationType, direct: true);
	}

	/// <summary>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</summary>
	/// <param name="name">The name as a string.</param>
	/// <param name="destinationType">The destination type as a string.</param>
	/// <param name="direct">A Boolean.</param>
	public SqlTypesSchemaImporterExtensionHelper(string name, string destinationType, bool direct)
	{
		Init(name, SqlTypesNamespace, null, null, destinationType, direct);
	}

	private void Init(string name, string targetNamespace, string[] references, CodeNamespaceImport[] namespaceImports, string destinationType, bool direct)
	{
		m_name = name;
		m_targetNamespace = targetNamespace;
		if (references == null)
		{
			m_references = new string[1];
			m_references[0] = "System.Data.dll";
		}
		else
		{
			m_references = references;
		}
		if (namespaceImports == null)
		{
			m_namespaceImports = new CodeNamespaceImport[2];
			m_namespaceImports[0] = new CodeNamespaceImport("System.Data");
			m_namespaceImports[1] = new CodeNamespaceImport("System.Data.SqlTypes");
		}
		else
		{
			m_namespaceImports = namespaceImports;
		}
		m_destinationType = destinationType;
		m_direct = direct;
	}

	/// <summary>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</summary>
	/// <returns>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</returns>
	/// <param name="name">
	///   <paramref name="name" />
	/// </param>
	/// <param name="xmlNamespace">
	///   <paramref name="xmlNamespace" />
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
	public override string ImportSchemaType(string name, string xmlNamespace, XmlSchemaObject context, XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeGenerationOptions options, CodeDomProvider codeProvider)
	{
		if (m_direct && context is XmlSchemaElement && string.CompareOrdinal(m_name, name) == 0 && string.CompareOrdinal(m_targetNamespace, xmlNamespace) == 0)
		{
			compileUnit.ReferencedAssemblies.AddRange(m_references);
			mainNamespace.Imports.AddRange(m_namespaceImports);
			return m_destinationType;
		}
		return null;
	}

	/// <summary>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</summary>
	/// <returns>The <see cref="T:System.Data.SqlTypes.SqlTypesSchemaImporterExtensionHelper" /> class is not intended for use as a stand-alone component, but as a class from which other classes derive standard functionality.</returns>
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
		if (!m_direct && type is XmlSchemaSimpleType && context is XmlSchemaElement)
		{
			XmlQualifiedName qualifiedName = ((XmlSchemaSimpleType)type).BaseXmlSchemaType.QualifiedName;
			if (string.CompareOrdinal(m_name, qualifiedName.Name) == 0 && string.CompareOrdinal(m_targetNamespace, qualifiedName.Namespace) == 0)
			{
				compileUnit.ReferencedAssemblies.AddRange(m_references);
				mainNamespace.Imports.AddRange(m_namespaceImports);
				return m_destinationType;
			}
		}
		return null;
	}
}
