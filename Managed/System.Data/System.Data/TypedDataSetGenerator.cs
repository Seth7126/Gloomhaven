using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Data;

/// <summary>Used to create a strongly typed <see cref="T:System.Data.DataSet" />.</summary>
/// <filterpriority>3</filterpriority>
[Obsolete("TypedDataSetGenerator class will be removed in a future release. Please use System.Data.Design.TypedDataSetGenerator in System.Design.dll.")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true, Synchronization = true)]
public class TypedDataSetGenerator
{
	private bool useExtendedNaming;

	private ICodeGenerator codeGen;

	private ArrayList errorList;

	private ArrayList conflictingTables;

	private Hashtable lookupIdentifiers;

	/// <summary>Generates a strongly typed <see cref="T:System.Data.DataSet" />.</summary>
	/// <param name="dataSet">The source <see cref="T:System.Data.DataSet" /> that specifies the metadata for the typed <see cref="T:System.Data.DataSet" />. </param>
	/// <param name="codeNamespace">The namespace that provides the target namespace for the typed <see cref="T:System.Data.DataSet" />. </param>
	/// <param name="codeGen">The generator used to create the typed <see cref="T:System.Data.DataSet" />. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public static void Generate(DataSet dataSet, CodeNamespace codeNamespace, ICodeGenerator codeGen)
	{
		new TypedDataSetGenerator().GenerateCode(dataSet, codeNamespace, codeGen);
		CodeGenerator.ValidateIdentifiers(codeNamespace);
	}

	/// <summary>Transforms a string in a valid, typed <see cref="T:System.Data.DataSet" /> name.</summary>
	/// <returns>A string that is the converted name.</returns>
	/// <param name="name">The source name to transform into a valid, typed <see cref="T:System.Data.DataSet" /> name. </param>
	/// <param name="codeGen">The generator used to perform the conversion. </param>
	/// <filterpriority>2</filterpriority>
	public static string GenerateIdName(string name, ICodeGenerator codeGen)
	{
		if (codeGen.IsValidIdentifier(name))
		{
			return name;
		}
		string text = name.Replace(' ', '_');
		if (!codeGen.IsValidIdentifier(text))
		{
			text = "_" + text;
			for (int i = 1; i < text.Length; i++)
			{
				UnicodeCategory unicodeCategory = char.GetUnicodeCategory(text[i]);
				if (unicodeCategory != UnicodeCategory.UppercaseLetter && UnicodeCategory.LowercaseLetter != unicodeCategory && UnicodeCategory.TitlecaseLetter != unicodeCategory && UnicodeCategory.ModifierLetter != unicodeCategory && UnicodeCategory.OtherLetter != unicodeCategory && UnicodeCategory.LetterNumber != unicodeCategory && UnicodeCategory.NonSpacingMark != unicodeCategory && UnicodeCategory.SpacingCombiningMark != unicodeCategory && UnicodeCategory.DecimalDigitNumber != unicodeCategory && UnicodeCategory.ConnectorPunctuation != unicodeCategory)
				{
					text = text.Replace(text[i], '_');
				}
			}
		}
		return text;
	}

	internal CodeTypeDeclaration GenerateCode(DataSet dataSet, CodeNamespace codeNamespace, ICodeGenerator codeGen)
	{
		useExtendedNaming = false;
		errorList = new ArrayList();
		conflictingTables = new ArrayList();
		this.codeGen = codeGen;
		CodeTypeDeclaration codeTypeDeclaration = CreateTypedDataSet(dataSet);
		foreach (DataTable table3 in dataSet.Tables)
		{
			codeTypeDeclaration.Members.Add(CreateTypedRowEventHandler(table3));
		}
		foreach (DataTable table4 in dataSet.Tables)
		{
			codeTypeDeclaration.Members.Add(CreateTypedTable(table4));
			codeTypeDeclaration.Members.Add(CreateTypedRow(table4));
			codeTypeDeclaration.Members.Add(CreateTypedRowEvent(table4));
		}
		if (errorList.Count > 0)
		{
			throw new TypedDataSetGeneratorException(errorList);
		}
		codeNamespace.Types.Add(codeTypeDeclaration);
		return codeTypeDeclaration;
	}

	private void InitLookupIdentifiers()
	{
		lookupIdentifiers = new Hashtable();
		PropertyInfo[] properties = typeof(DataRow).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			lookupIdentifiers[propertyInfo.Name] = "_" + propertyInfo.Name;
		}
	}

	private string FixIdName(string inVarName)
	{
		if (lookupIdentifiers == null)
		{
			InitLookupIdentifiers();
		}
		string text = (string)lookupIdentifiers[inVarName];
		if (text == null)
		{
			text = GenerateIdName(inVarName, codeGen);
			while (lookupIdentifiers.ContainsValue(text))
			{
				text = "_" + text;
			}
			lookupIdentifiers[inVarName] = text;
			if (!codeGen.IsValidIdentifier(text))
			{
				errorList.Add(Res.GetString("Cannot generate identifier for name '{0}'", text));
			}
		}
		return text;
	}

	private static bool isEmpty(string s)
	{
		if (s != null)
		{
			return s.Length == 0;
		}
		return true;
	}

	private string RowClassName(DataTable table)
	{
		string text = (string)table.ExtendedProperties["typedName"];
		if (isEmpty(text))
		{
			text = FixIdName(table.TableName) + "Row";
		}
		return text;
	}

	private string RowBaseClassName(DataTable table)
	{
		if (useExtendedNaming)
		{
			string text = (string)table.ExtendedProperties["typedBaseClass"];
			if (isEmpty(text))
			{
				text = (string)table.DataSet.ExtendedProperties["typedBaseClass"];
				if (isEmpty(text))
				{
					text = "DataRow";
				}
			}
			return text;
		}
		return "DataRow";
	}

	private string RowConcreteClassName(DataTable table)
	{
		if (useExtendedNaming)
		{
			string text = (string)table.ExtendedProperties["typedConcreteClass"];
			if (isEmpty(text))
			{
				text = RowClassName(table);
			}
			return text;
		}
		return RowClassName(table);
	}

	private string TableClassName(DataTable table)
	{
		string text = (string)table.ExtendedProperties["typedPlural"];
		if (isEmpty(text))
		{
			text = (string)table.ExtendedProperties["typedName"];
			if (isEmpty(text))
			{
				if (table.DataSet.Tables.InternalIndexOf(table.TableName) == -3 && !conflictingTables.Contains(table.TableName))
				{
					conflictingTables.Add(table.TableName);
					errorList.Add(Res.GetString("There is more than one table with the same name '{0}' (even if namespace is different)", table.TableName));
				}
				text = FixIdName(table.TableName);
			}
		}
		return text + "DataTable";
	}

	private string TablePropertyName(DataTable table)
	{
		string text = (string)table.ExtendedProperties["typedPlural"];
		if (isEmpty(text))
		{
			text = (string)table.ExtendedProperties["typedName"];
			text = ((!isEmpty(text)) ? (text + "Table") : FixIdName(table.TableName));
		}
		return text;
	}

	private string TableFieldName(DataTable table)
	{
		return "table" + TablePropertyName(table);
	}

	private string RowColumnPropertyName(DataColumn column)
	{
		string text = (string)column.ExtendedProperties["typedName"];
		if (isEmpty(text))
		{
			text = FixIdName(column.ColumnName);
		}
		return text;
	}

	private string TableColumnFieldName(DataColumn column)
	{
		string text = RowColumnPropertyName(column);
		if (string.Compare("column", text, StringComparison.OrdinalIgnoreCase) != 0)
		{
			return "column" + text;
		}
		return "columnField" + text;
	}

	private string TableColumnPropertyName(DataColumn column)
	{
		return RowColumnPropertyName(column) + "Column";
	}

	private static int TablesConnectedness(DataTable parentTable, DataTable childTable)
	{
		int num = 0;
		DataRelationCollection parentRelations = childTable.ParentRelations;
		for (int i = 0; i < parentRelations.Count; i++)
		{
			if (parentRelations[i].ParentTable == parentTable)
			{
				num++;
			}
		}
		return num;
	}

	private string ChildPropertyName(DataRelation relation)
	{
		string text = (string)relation.ExtendedProperties["typedChildren"];
		if (isEmpty(text))
		{
			string text2 = (string)relation.ChildTable.ExtendedProperties["typedPlural"];
			if (isEmpty(text2))
			{
				text2 = (string)relation.ChildTable.ExtendedProperties["typedName"];
				if (isEmpty(text2))
				{
					text = "Get" + relation.ChildTable.TableName + "Rows";
					if (1 < TablesConnectedness(relation.ParentTable, relation.ChildTable))
					{
						text = text + "By" + relation.RelationName;
					}
					return FixIdName(text);
				}
				text2 += "Rows";
			}
			text = "Get" + text2;
		}
		return text;
	}

	private string ParentPropertyName(DataRelation relation)
	{
		string text = null;
		text = (string)relation.ExtendedProperties["typedParent"];
		if (isEmpty(text))
		{
			text = RowClassName(relation.ParentTable);
			if (relation.ChildTable == relation.ParentTable || relation.ChildColumnsReference.Length != 1)
			{
				text += "Parent";
			}
			if (1 < TablesConnectedness(relation.ParentTable, relation.ChildTable))
			{
				text = text + "By" + FixIdName(relation.RelationName);
			}
		}
		return text;
	}

	private string RelationFieldName(DataRelation relation)
	{
		return FixIdName("relation" + relation.RelationName);
	}

	private string GetTypeName(Type t)
	{
		return t.FullName;
	}

	private bool ChildRelationFollowable(DataRelation relation)
	{
		if (relation != null)
		{
			if (relation.ChildTable == relation.ParentTable && relation.ChildTable.Columns.Count == 1)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private static CodeMemberMethod CreateOnRowEventMethod(string eventName, string rowClassName)
	{
		CodeMemberMethod codeMemberMethod = MethodDecl(typeof(void), "OnRow" + eventName, (MemberAttributes)12292);
		codeMemberMethod.Parameters.Add(ParameterDecl(typeof(DataRowChangeEventArgs), "e"));
		codeMemberMethod.Statements.Add(MethodCall(Base(), "OnRow" + eventName, Argument("e")));
		codeMemberMethod.Statements.Add(If(IdNotEQ(Event(rowClassName + eventName), Primitive(null)), Stm(DelegateCall(Event(rowClassName + eventName), New(rowClassName + "ChangeEvent", new CodeExpression[2]
		{
			Cast(rowClassName, Property(Argument("e"), "Row")),
			Property(Argument("e"), "Action")
		})))));
		return codeMemberMethod;
	}

	private CodeTypeDeclaration CreateTypedTable(DataTable table)
	{
		string text = RowClassName(table);
		string text2 = TableClassName(table);
		string type = RowConcreteClassName(table);
		CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(text2);
		codeTypeDeclaration.BaseTypes.Add(typeof(DataTable));
		codeTypeDeclaration.BaseTypes.Add(typeof(IEnumerable));
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl("System.Serializable"));
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));
		for (int i = 0; i < table.Columns.Count; i++)
		{
			codeTypeDeclaration.Members.Add(FieldDecl(typeof(DataColumn), TableColumnFieldName(table.Columns[i])));
		}
		codeTypeDeclaration.Members.Add(EventDecl(text + "ChangeEventHandler", text + "Changed"));
		codeTypeDeclaration.Members.Add(EventDecl(text + "ChangeEventHandler", text + "Changing"));
		codeTypeDeclaration.Members.Add(EventDecl(text + "ChangeEventHandler", text + "Deleted"));
		codeTypeDeclaration.Members.Add(EventDecl(text + "ChangeEventHandler", text + "Deleting"));
		CodeConstructor codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = (MemberAttributes)4098;
		codeConstructor.BaseConstructorArgs.Add(Str(table.TableName));
		codeConstructor.Statements.Add(MethodCall(This(), "InitClass"));
		codeTypeDeclaration.Members.Add(codeConstructor);
		codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = MemberAttributes.Family;
		codeConstructor.Parameters.Add(ParameterDecl(typeof(SerializationInfo), "info"));
		codeConstructor.Parameters.Add(ParameterDecl(typeof(StreamingContext), "context"));
		codeConstructor.BaseConstructorArgs.AddRange(new CodeExpression[2]
		{
			Argument("info"),
			Argument("context")
		});
		codeConstructor.Statements.Add(MethodCall(This(), "InitVars"));
		codeTypeDeclaration.Members.Add(codeConstructor);
		codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = (MemberAttributes)4098;
		codeConstructor.Parameters.Add(ParameterDecl(typeof(DataTable), "table"));
		codeConstructor.BaseConstructorArgs.Add(Property(Argument("table"), "TableName"));
		codeConstructor.Statements.Add(If(IdNotEQ(Property(Argument("table"), "CaseSensitive"), Property(Property(Argument("table"), "DataSet"), "CaseSensitive")), Assign(Property(This(), "CaseSensitive"), Property(Argument("table"), "CaseSensitive"))));
		codeConstructor.Statements.Add(If(IdNotEQ(MethodCall(Property(Argument("table"), "Locale"), "ToString"), MethodCall(Property(Property(Argument("table"), "DataSet"), "Locale"), "ToString")), Assign(Property(This(), "Locale"), Property(Argument("table"), "Locale"))));
		codeConstructor.Statements.Add(If(IdNotEQ(Property(Argument("table"), "Namespace"), Property(Property(Argument("table"), "DataSet"), "Namespace")), Assign(Property(This(), "Namespace"), Property(Argument("table"), "Namespace"))));
		codeConstructor.Statements.Add(Assign(Property(This(), "Prefix"), Property(Argument("table"), "Prefix")));
		codeConstructor.Statements.Add(Assign(Property(This(), "MinimumCapacity"), Property(Argument("table"), "MinimumCapacity")));
		codeConstructor.Statements.Add(Assign(Property(This(), "DisplayExpression"), Property(Argument("table"), "DisplayExpression")));
		codeTypeDeclaration.Members.Add(codeConstructor);
		CodeMemberProperty codeMemberProperty = PropertyDecl(typeof(int), "Count", (MemberAttributes)24578);
		codeMemberProperty.CustomAttributes.Add(AttributeDecl("System.ComponentModel.Browsable", Primitive(false)));
		codeMemberProperty.GetStatements.Add(Return(Property(Property(This(), "Rows"), "Count")));
		codeTypeDeclaration.Members.Add(codeMemberProperty);
		for (int j = 0; j < table.Columns.Count; j++)
		{
			DataColumn column = table.Columns[j];
			CodeMemberProperty codeMemberProperty2 = PropertyDecl(typeof(DataColumn), TableColumnPropertyName(column), (MemberAttributes)4098);
			codeMemberProperty2.GetStatements.Add(Return(Field(This(), TableColumnFieldName(column))));
			codeTypeDeclaration.Members.Add(codeMemberProperty2);
		}
		CodeMemberProperty codeMemberProperty3 = PropertyDecl(type, "Item", (MemberAttributes)24578);
		codeMemberProperty3.Parameters.Add(ParameterDecl(typeof(int), "index"));
		codeMemberProperty3.GetStatements.Add(Return(Cast(type, Indexer(Property(This(), "Rows"), Argument("index")))));
		codeTypeDeclaration.Members.Add(codeMemberProperty3);
		CodeMemberMethod codeMemberMethod = MethodDecl(typeof(void), "Add" + text, (MemberAttributes)24578);
		codeMemberMethod.Parameters.Add(ParameterDecl(type, "row"));
		codeMemberMethod.Statements.Add(MethodCall(Property(This(), "Rows"), "Add", Argument("row")));
		codeTypeDeclaration.Members.Add(codeMemberMethod);
		ArrayList arrayList = new ArrayList();
		for (int k = 0; k < table.Columns.Count; k++)
		{
			if (!table.Columns[k].AutoIncrement)
			{
				arrayList.Add(table.Columns[k]);
			}
		}
		CodeMemberMethod codeMemberMethod2 = MethodDecl(type, "Add" + text, (MemberAttributes)24578);
		DataColumn[] array = new DataColumn[arrayList.Count];
		arrayList.CopyTo(array, 0);
		for (int l = 0; l < array.Length; l++)
		{
			Type dataType = array[l].DataType;
			DataRelation dataRelation = array[l].FindParentRelation();
			if (ChildRelationFollowable(dataRelation))
			{
				string text3 = RowClassName(dataRelation.ParentTable);
				string name = FixIdName("parent" + text3 + "By" + dataRelation.RelationName);
				codeMemberMethod2.Parameters.Add(ParameterDecl(text3, name));
			}
			else
			{
				codeMemberMethod2.Parameters.Add(ParameterDecl(GetTypeName(dataType), RowColumnPropertyName(array[l])));
			}
		}
		codeMemberMethod2.Statements.Add(VariableDecl(type, "row" + text, Cast(type, MethodCall(This(), "NewRow"))));
		CodeExpression codeExpression = Variable("row" + text);
		CodeAssignStatement codeAssignStatement = new CodeAssignStatement();
		codeAssignStatement.Left = Property(codeExpression, "ItemArray");
		CodeArrayCreateExpression codeArrayCreateExpression = new CodeArrayCreateExpression();
		codeArrayCreateExpression.CreateType = Type(typeof(object));
		array = new DataColumn[table.Columns.Count];
		table.Columns.CopyTo(array, 0);
		for (int m = 0; m < array.Length; m++)
		{
			if (array[m].AutoIncrement)
			{
				codeArrayCreateExpression.Initializers.Add(Primitive(null));
				continue;
			}
			DataRelation dataRelation2 = array[m].FindParentRelation();
			if (ChildRelationFollowable(dataRelation2))
			{
				string text4 = RowClassName(dataRelation2.ParentTable);
				string argument = FixIdName("parent" + text4 + "By" + dataRelation2.RelationName);
				codeArrayCreateExpression.Initializers.Add(Indexer(Argument(argument), Primitive(dataRelation2.ParentColumnsReference[0].Ordinal)));
			}
			else
			{
				codeArrayCreateExpression.Initializers.Add(Argument(RowColumnPropertyName(array[m])));
			}
		}
		codeAssignStatement.Right = codeArrayCreateExpression;
		codeMemberMethod2.Statements.Add(codeAssignStatement);
		codeMemberMethod2.Statements.Add(MethodCall(Property(This(), "Rows"), "Add", codeExpression));
		codeMemberMethod2.Statements.Add(Return(codeExpression));
		codeTypeDeclaration.Members.Add(codeMemberMethod2);
		for (int n = 0; n < table.Constraints.Count; n++)
		{
			if (!(table.Constraints[n] is UniqueConstraint) || !((UniqueConstraint)table.Constraints[n]).IsPrimaryKey)
			{
				continue;
			}
			DataColumn[] columnsReference = ((UniqueConstraint)table.Constraints[n]).ColumnsReference;
			string text5 = "FindBy";
			bool flag = true;
			for (int num = 0; num < columnsReference.Length; num++)
			{
				text5 += RowColumnPropertyName(columnsReference[num]);
				if (columnsReference[num].ColumnMapping != MappingType.Hidden)
				{
					flag = false;
				}
			}
			if (!flag)
			{
				CodeMemberMethod codeMemberMethod3 = MethodDecl(text, FixIdName(text5), (MemberAttributes)24578);
				for (int num2 = 0; num2 < columnsReference.Length; num2++)
				{
					codeMemberMethod3.Parameters.Add(ParameterDecl(GetTypeName(columnsReference[num2].DataType), RowColumnPropertyName(columnsReference[num2])));
				}
				CodeArrayCreateExpression codeArrayCreateExpression2 = new CodeArrayCreateExpression(typeof(object), columnsReference.Length);
				for (int num3 = 0; num3 < columnsReference.Length; num3++)
				{
					codeArrayCreateExpression2.Initializers.Add(Argument(RowColumnPropertyName(columnsReference[num3])));
				}
				codeMemberMethod3.Statements.Add(Return(Cast(text, MethodCall(Property(This(), "Rows"), "Find", codeArrayCreateExpression2))));
				codeTypeDeclaration.Members.Add(codeMemberMethod3);
			}
		}
		CodeMemberMethod codeMemberMethod4 = MethodDecl(typeof(IEnumerator), "GetEnumerator", (MemberAttributes)24578);
		codeMemberMethod4.ImplementationTypes.Add(Type("System.Collections.IEnumerable"));
		codeMemberMethod4.Statements.Add(Return(MethodCall(Property(This(), "Rows"), "GetEnumerator")));
		codeTypeDeclaration.Members.Add(codeMemberMethod4);
		CodeMemberMethod codeMemberMethod5 = MethodDecl(typeof(DataTable), "Clone", (MemberAttributes)24580);
		codeMemberMethod5.Statements.Add(VariableDecl(text2, "cln", Cast(text2, MethodCall(Base(), "Clone", new CodeExpression[0]))));
		codeMemberMethod5.Statements.Add(MethodCall(Variable("cln"), "InitVars", new CodeExpression[0]));
		codeMemberMethod5.Statements.Add(Return(Variable("cln")));
		codeTypeDeclaration.Members.Add(codeMemberMethod5);
		CodeMemberMethod codeMemberMethod6 = MethodDecl(typeof(DataTable), "CreateInstance", (MemberAttributes)12292);
		codeMemberMethod6.Statements.Add(Return(New(text2, new CodeExpression[0])));
		codeTypeDeclaration.Members.Add(codeMemberMethod6);
		CodeMemberMethod codeMemberMethod7 = MethodDecl(typeof(void), "InitClass", MemberAttributes.Private);
		CodeMemberMethod codeMemberMethod8 = MethodDecl(typeof(void), "InitVars", (MemberAttributes)4098);
		for (int num4 = 0; num4 < table.Columns.Count; num4++)
		{
			DataColumn dataColumn = table.Columns[num4];
			string field = TableColumnFieldName(dataColumn);
			CodeExpression left = Field(This(), field);
			codeMemberMethod7.Statements.Add(Assign(left, New(typeof(DataColumn), new CodeExpression[4]
			{
				Str(dataColumn.ColumnName),
				TypeOf(GetTypeName(dataColumn.DataType)),
				Primitive(null),
				Field(TypeExpr(typeof(MappingType)), (dataColumn.ColumnMapping == MappingType.SimpleContent) ? "SimpleContent" : ((dataColumn.ColumnMapping == MappingType.Attribute) ? "Attribute" : ((dataColumn.ColumnMapping == MappingType.Hidden) ? "Hidden" : "Element")))
			})));
			codeMemberMethod7.Statements.Add(MethodCall(Property(This(), "Columns"), "Add", Field(This(), field)));
		}
		for (int num5 = 0; num5 < table.Constraints.Count; num5++)
		{
			if (table.Constraints[num5] is UniqueConstraint)
			{
				UniqueConstraint uniqueConstraint = (UniqueConstraint)table.Constraints[num5];
				DataColumn[] columnsReference2 = uniqueConstraint.ColumnsReference;
				CodeExpression[] array2 = new CodeExpression[columnsReference2.Length];
				for (int num6 = 0; num6 < columnsReference2.Length; num6++)
				{
					array2[num6] = Field(This(), TableColumnFieldName(columnsReference2[num6]));
				}
				codeMemberMethod7.Statements.Add(MethodCall(Property(This(), "Constraints"), "Add", New(typeof(UniqueConstraint), new CodeExpression[3]
				{
					Str(uniqueConstraint.ConstraintName),
					new CodeArrayCreateExpression(typeof(DataColumn), array2),
					Primitive(uniqueConstraint.IsPrimaryKey)
				})));
			}
		}
		for (int num7 = 0; num7 < table.Columns.Count; num7++)
		{
			DataColumn dataColumn2 = table.Columns[num7];
			string field2 = TableColumnFieldName(dataColumn2);
			CodeExpression codeExpression2 = Field(This(), field2);
			codeMemberMethod8.Statements.Add(Assign(codeExpression2, Indexer(Property(This(), "Columns"), Str(dataColumn2.ColumnName))));
			if (dataColumn2.AutoIncrement)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "AutoIncrement"), Primitive(true)));
			}
			if (dataColumn2.AutoIncrementSeed != 0L)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "AutoIncrementSeed"), Primitive(dataColumn2.AutoIncrementSeed)));
			}
			if (dataColumn2.AutoIncrementStep != 1)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "AutoIncrementStep"), Primitive(dataColumn2.AutoIncrementStep)));
			}
			if (!dataColumn2.AllowDBNull)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "AllowDBNull"), Primitive(false)));
			}
			if (dataColumn2.ReadOnly)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "ReadOnly"), Primitive(true)));
			}
			if (dataColumn2.Unique)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "Unique"), Primitive(true)));
			}
			if (!ADP.IsEmpty(dataColumn2.Prefix))
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "Prefix"), Str(dataColumn2.Prefix)));
			}
			if (dataColumn2._columnUri != null)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "Namespace"), Str(dataColumn2.Namespace)));
			}
			if (dataColumn2.Caption != dataColumn2.ColumnName)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "Caption"), Str(dataColumn2.Caption)));
			}
			if (dataColumn2.DefaultValue != DBNull.Value)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "DefaultValue"), Primitive(dataColumn2.DefaultValue)));
			}
			if (dataColumn2.MaxLength != -1)
			{
				codeMemberMethod7.Statements.Add(Assign(Property(codeExpression2, "MaxLength"), Primitive(dataColumn2.MaxLength)));
			}
		}
		if (table.ShouldSerializeCaseSensitive())
		{
			codeMemberMethod7.Statements.Add(Assign(Property(This(), "CaseSensitive"), Primitive(table.CaseSensitive)));
		}
		if (table.ShouldSerializeLocale())
		{
			codeMemberMethod7.Statements.Add(Assign(Property(This(), "Locale"), New(typeof(CultureInfo), new CodeExpression[1] { Str(table.Locale.ToString()) })));
		}
		if (!ADP.IsEmpty(table.Prefix))
		{
			codeMemberMethod7.Statements.Add(Assign(Property(This(), "Prefix"), Str(table.Prefix)));
		}
		if (table._tableNamespace != null)
		{
			codeMemberMethod7.Statements.Add(Assign(Property(This(), "Namespace"), Str(table.Namespace)));
		}
		if (table.MinimumCapacity != 50)
		{
			codeMemberMethod7.Statements.Add(Assign(Property(This(), "MinimumCapacity"), Primitive(table.MinimumCapacity)));
		}
		if (table._displayExpression != null)
		{
			codeMemberMethod7.Statements.Add(Assign(Property(This(), "DisplayExpression"), Str(table.DisplayExpressionInternal)));
		}
		codeTypeDeclaration.Members.Add(codeMemberMethod8);
		codeTypeDeclaration.Members.Add(codeMemberMethod7);
		CodeMemberMethod codeMemberMethod9 = MethodDecl(type, "New" + text, (MemberAttributes)24578);
		codeMemberMethod9.Statements.Add(Return(Cast(type, MethodCall(This(), "NewRow"))));
		codeTypeDeclaration.Members.Add(codeMemberMethod9);
		CodeMemberMethod codeMemberMethod10 = MethodDecl(typeof(DataRow), "NewRowFromBuilder", (MemberAttributes)12292);
		codeMemberMethod10.Parameters.Add(ParameterDecl(typeof(DataRowBuilder), "builder"));
		codeMemberMethod10.Statements.Add(Return(New(type, new CodeExpression[1] { Argument("builder") })));
		codeTypeDeclaration.Members.Add(codeMemberMethod10);
		CodeMemberMethod codeMemberMethod11 = MethodDecl(typeof(Type), "GetRowType", (MemberAttributes)12292);
		codeMemberMethod11.Statements.Add(Return(TypeOf(type)));
		codeTypeDeclaration.Members.Add(codeMemberMethod11);
		codeTypeDeclaration.Members.Add(CreateOnRowEventMethod("Changed", text));
		codeTypeDeclaration.Members.Add(CreateOnRowEventMethod("Changing", text));
		codeTypeDeclaration.Members.Add(CreateOnRowEventMethod("Deleted", text));
		codeTypeDeclaration.Members.Add(CreateOnRowEventMethod("Deleting", text));
		CodeMemberMethod codeMemberMethod12 = MethodDecl(typeof(void), "Remove" + text, (MemberAttributes)24578);
		codeMemberMethod12.Parameters.Add(ParameterDecl(type, "row"));
		codeMemberMethod12.Statements.Add(MethodCall(Property(This(), "Rows"), "Remove", Argument("row")));
		codeTypeDeclaration.Members.Add(codeMemberMethod12);
		return codeTypeDeclaration;
	}

	private CodeTypeDeclaration CreateTypedRow(DataTable table)
	{
		string text = RowClassName(table);
		string type = TableClassName(table);
		string text2 = TableFieldName(table);
		bool flag = false;
		CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration();
		codeTypeDeclaration.Name = text;
		string text3 = RowBaseClassName(table);
		if (string.Compare(text3, "DataRow", StringComparison.Ordinal) == 0)
		{
			codeTypeDeclaration.BaseTypes.Add(typeof(DataRow));
		}
		else
		{
			codeTypeDeclaration.BaseTypes.Add(text3);
		}
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));
		codeTypeDeclaration.Members.Add(FieldDecl(type, text2));
		CodeConstructor codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = (MemberAttributes)4098;
		codeConstructor.Parameters.Add(ParameterDecl(typeof(DataRowBuilder), "rb"));
		codeConstructor.BaseConstructorArgs.Add(Argument("rb"));
		codeConstructor.Statements.Add(Assign(Field(This(), text2), Cast(type, Property(This(), "Table"))));
		codeTypeDeclaration.Members.Add(codeConstructor);
		foreach (DataColumn column in table.Columns)
		{
			if (column.ColumnMapping == MappingType.Hidden)
			{
				continue;
			}
			Type dataType = column.DataType;
			string text4 = RowColumnPropertyName(column);
			string property = TableColumnPropertyName(column);
			CodeMemberProperty codeMemberProperty = PropertyDecl(dataType, text4, (MemberAttributes)24578);
			CodeStatement codeStatement = Return(Cast(GetTypeName(dataType), Indexer(This(), Property(Field(This(), text2), property))));
			if (column.AllowDBNull)
			{
				string text5 = (string)column.ExtendedProperties["nullValue"];
				if (text5 == null || text5 == "_throw")
				{
					codeStatement = Try(codeStatement, Catch(typeof(InvalidCastException), "e", Throw(typeof(StrongTypingException), "StrongTyping_CananotAccessDBNull", "e")));
				}
				else
				{
					CodeExpression codeExpression = null;
					CodeExpression expr;
					if (text5 == "_null")
					{
						if (column.DataType.IsSubclassOf(typeof(System.ValueType)))
						{
							errorList.Add(Res.GetString("Column '{0}': Type '{1}' cannot be null", column.ColumnName, column.DataType.Name));
							continue;
						}
						expr = Primitive(null);
					}
					else if (text5 == "_empty")
					{
						if (column.DataType == typeof(string))
						{
							expr = Property(TypeExpr(column.DataType), "Empty");
						}
						else
						{
							expr = Field(TypeExpr(text), text4 + "_nullValue");
							ConstructorInfo constructor = column.DataType.GetConstructor(new Type[1] { typeof(string) });
							if (constructor == null)
							{
								errorList.Add(Res.GetString("Column '{0}': Type '{1}' does not have parameterless constructor", column.ColumnName, column.DataType.Name));
								continue;
							}
							constructor.Invoke(new object[0]);
							codeExpression = New(column.DataType, new CodeExpression[0]);
						}
					}
					else
					{
						if (!flag)
						{
							table.NewRow();
							flag = true;
						}
						object obj = column.ConvertXmlToObject(text5);
						if (column.DataType == typeof(char) || column.DataType == typeof(string) || column.DataType == typeof(decimal) || column.DataType == typeof(bool) || column.DataType == typeof(float) || column.DataType == typeof(double) || column.DataType == typeof(sbyte) || column.DataType == typeof(byte) || column.DataType == typeof(short) || column.DataType == typeof(ushort) || column.DataType == typeof(int) || column.DataType == typeof(uint) || column.DataType == typeof(long) || column.DataType == typeof(ulong))
						{
							expr = Primitive(obj);
						}
						else
						{
							expr = Field(TypeExpr(text), text4 + "_nullValue");
							if (column.DataType == typeof(byte[]))
							{
								codeExpression = MethodCall(TypeExpr(typeof(Convert)), "FromBase64String", Primitive(text5));
							}
							else if (column.DataType == typeof(DateTime) || column.DataType == typeof(TimeSpan))
							{
								codeExpression = MethodCall(TypeExpr(column.DataType), "Parse", Primitive(obj.ToString()));
							}
							else
							{
								ConstructorInfo constructor2 = column.DataType.GetConstructor(new Type[1] { typeof(string) });
								if (constructor2 == null)
								{
									errorList.Add(Res.GetString("Column '{0}': Type '{1}' does not have constructor with string argument", column.ColumnName, column.DataType.Name));
									continue;
								}
								constructor2.Invoke(new object[1] { text5 });
								codeExpression = New(column.DataType, new CodeExpression[1] { Primitive(text5) });
							}
						}
					}
					codeStatement = If(MethodCall(This(), "Is" + text4 + "Null"), new CodeStatement[1] { Return(expr) }, new CodeStatement[1] { codeStatement });
					if (codeExpression != null)
					{
						CodeMemberField codeMemberField = FieldDecl(column.DataType, text4 + "_nullValue");
						codeMemberField.Attributes = (MemberAttributes)20483;
						codeMemberField.InitExpression = codeExpression;
						codeTypeDeclaration.Members.Add(codeMemberField);
					}
				}
			}
			codeMemberProperty.GetStatements.Add(codeStatement);
			codeMemberProperty.SetStatements.Add(Assign(Indexer(This(), Property(Field(This(), text2), property)), Value()));
			codeTypeDeclaration.Members.Add(codeMemberProperty);
			if (column.AllowDBNull)
			{
				CodeMemberMethod codeMemberMethod = MethodDecl(typeof(bool), "Is" + text4 + "Null", (MemberAttributes)24578);
				codeMemberMethod.Statements.Add(Return(MethodCall(This(), "IsNull", Property(Field(This(), text2), property))));
				codeTypeDeclaration.Members.Add(codeMemberMethod);
				CodeMemberMethod codeMemberMethod2 = MethodDecl(typeof(void), "Set" + text4 + "Null", (MemberAttributes)24578);
				codeMemberMethod2.Statements.Add(Assign(Indexer(This(), Property(Field(This(), text2), property)), Field(TypeExpr(typeof(Convert)), "DBNull")));
				codeTypeDeclaration.Members.Add(codeMemberMethod2);
			}
		}
		DataRelationCollection childRelations = table.ChildRelations;
		for (int i = 0; i < childRelations.Count; i++)
		{
			DataRelation dataRelation = childRelations[i];
			string type2 = RowConcreteClassName(dataRelation.ChildTable);
			CodeMemberMethod codeMemberMethod3 = Method(Type(type2, 1), ChildPropertyName(dataRelation), (MemberAttributes)24578);
			codeMemberMethod3.Statements.Add(Return(Cast(Type(type2, 1), MethodCall(This(), "GetChildRows", Indexer(Property(Property(This(), "Table"), "ChildRelations"), Str(dataRelation.RelationName))))));
			codeTypeDeclaration.Members.Add(codeMemberMethod3);
		}
		DataRelationCollection parentRelations = table.ParentRelations;
		for (int j = 0; j < parentRelations.Count; j++)
		{
			DataRelation dataRelation2 = parentRelations[j];
			string type3 = RowClassName(dataRelation2.ParentTable);
			CodeMemberProperty codeMemberProperty2 = PropertyDecl(type3, ParentPropertyName(dataRelation2), (MemberAttributes)24578);
			codeMemberProperty2.GetStatements.Add(Return(Cast(type3, MethodCall(This(), "GetParentRow", Indexer(Property(Property(This(), "Table"), "ParentRelations"), Str(dataRelation2.RelationName))))));
			codeMemberProperty2.SetStatements.Add(MethodCall(This(), "SetParentRow", new CodeExpression[2]
			{
				Value(),
				Indexer(Property(Property(This(), "Table"), "ParentRelations"), Str(dataRelation2.RelationName))
			}));
			codeTypeDeclaration.Members.Add(codeMemberProperty2);
		}
		return codeTypeDeclaration;
	}

	private CodeTypeDeclaration CreateTypedRowEvent(DataTable table)
	{
		string text = RowClassName(table);
		TableClassName(table);
		string type = RowConcreteClassName(table);
		CodeTypeDeclaration obj = new CodeTypeDeclaration
		{
			Name = text + "ChangeEvent",
			BaseTypes = { typeof(EventArgs) },
			CustomAttributes = { AttributeDecl("System.Diagnostics.DebuggerStepThrough") },
			Members = 
			{
				(CodeTypeMember)FieldDecl(type, "eventRow"),
				(CodeTypeMember)FieldDecl(typeof(DataRowAction), "eventAction")
			}
		};
		CodeConstructor value = new CodeConstructor
		{
			Attributes = (MemberAttributes)24578,
			Parameters = 
			{
				ParameterDecl(type, "row"),
				ParameterDecl(typeof(DataRowAction), "action")
			},
			Statements = 
			{
				Assign(Field(This(), "eventRow"), Argument("row")),
				Assign(Field(This(), "eventAction"), Argument("action"))
			}
		};
		obj.Members.Add(value);
		CodeMemberProperty codeMemberProperty = PropertyDecl(type, "Row", (MemberAttributes)24578);
		codeMemberProperty.GetStatements.Add(Return(Field(This(), "eventRow")));
		obj.Members.Add(codeMemberProperty);
		codeMemberProperty = PropertyDecl(typeof(DataRowAction), "Action", (MemberAttributes)24578);
		codeMemberProperty.GetStatements.Add(Return(Field(This(), "eventAction")));
		obj.Members.Add(codeMemberProperty);
		return obj;
	}

	private CodeTypeDelegate CreateTypedRowEventHandler(DataTable table)
	{
		string text = RowClassName(table);
		CodeTypeDelegate codeTypeDelegate = new CodeTypeDelegate(text + "ChangeEventHandler");
		codeTypeDelegate.TypeAttributes |= TypeAttributes.Public;
		codeTypeDelegate.Parameters.Add(ParameterDecl(typeof(object), "sender"));
		codeTypeDelegate.Parameters.Add(ParameterDecl(text + "ChangeEvent", "e"));
		return codeTypeDelegate;
	}

	private CodeTypeDeclaration CreateTypedDataSet(DataSet dataSet)
	{
		string text = FixIdName(dataSet.DataSetName);
		CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(text);
		codeTypeDeclaration.BaseTypes.Add(typeof(DataSet));
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl("System.Serializable"));
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl("System.ComponentModel.DesignerCategoryAttribute", Str("code")));
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl("System.Diagnostics.DebuggerStepThrough"));
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl("System.ComponentModel.ToolboxItem", Primitive(true)));
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl(typeof(XmlSchemaProviderAttribute).FullName, Primitive("GetTypedDataSetSchema")));
		codeTypeDeclaration.CustomAttributes.Add(AttributeDecl(typeof(XmlRootAttribute).FullName, Primitive(text)));
		for (int i = 0; i < dataSet.Tables.Count; i++)
		{
			codeTypeDeclaration.Members.Add(FieldDecl(TableClassName(dataSet.Tables[i]), TableFieldName(dataSet.Tables[i])));
		}
		for (int j = 0; j < dataSet.Relations.Count; j++)
		{
			codeTypeDeclaration.Members.Add(FieldDecl(typeof(DataRelation), RelationFieldName(dataSet.Relations[j])));
		}
		CodeConstructor codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = MemberAttributes.Public;
		codeConstructor.Statements.Add(MethodCall(This(), "BeginInit"));
		codeConstructor.Statements.Add(MethodCall(This(), "InitClass"));
		codeConstructor.Statements.Add(VariableDecl(typeof(CollectionChangeEventHandler), "schemaChangedHandler", new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)), This(), "SchemaChanged")));
		codeConstructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Tables"), "CollectionChanged"), Variable("schemaChangedHandler")));
		codeConstructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Relations"), "CollectionChanged"), Variable("schemaChangedHandler")));
		codeConstructor.Statements.Add(MethodCall(This(), "EndInit"));
		codeTypeDeclaration.Members.Add(codeConstructor);
		codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = MemberAttributes.Family;
		codeConstructor.Parameters.Add(ParameterDecl(typeof(SerializationInfo), "info"));
		codeConstructor.Parameters.Add(ParameterDecl(typeof(StreamingContext), "context"));
		codeConstructor.BaseConstructorArgs.AddRange(new CodeExpression[2]
		{
			Argument("info"),
			Argument("context")
		});
		codeConstructor.Statements.Add(If(EQ(MethodCall(This(), "IsBinarySerialized", new CodeExpression[2]
		{
			Argument("info"),
			Argument("context")
		}), Primitive(true)), new CodeStatement[5]
		{
			Stm(MethodCall(This(), "InitVars", Primitive(false))),
			VariableDecl(typeof(CollectionChangeEventHandler), "schemaChangedHandler1", new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)), This(), "SchemaChanged")),
			new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Tables"), "CollectionChanged"), Variable("schemaChangedHandler1")),
			new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Relations"), "CollectionChanged"), Variable("schemaChangedHandler1")),
			Return()
		}));
		codeConstructor.Statements.Add(VariableDecl(typeof(string), "strSchema", Cast("System.String", MethodCall(Argument("info"), "GetValue", new CodeExpression[2]
		{
			Str("XmlSchema"),
			TypeOf("System.String")
		}))));
		ArrayList arrayList = new ArrayList();
		arrayList.Add(VariableDecl(typeof(DataSet), "ds", New(typeof(DataSet), new CodeExpression[0])));
		arrayList.Add(Stm(MethodCall(Variable("ds"), "ReadXmlSchema", new CodeExpression[1] { New(typeof(XmlTextReader), new CodeExpression[1] { New("System.IO.StringReader", new CodeExpression[1] { Variable("strSchema") }) }) })));
		for (int k = 0; k < dataSet.Tables.Count; k++)
		{
			arrayList.Add(If(IdNotEQ(Indexer(Property(Variable("ds"), "Tables"), Str(dataSet.Tables[k].TableName)), Primitive(null)), Stm(MethodCall(Property(This(), "Tables"), "Add", New(TableClassName(dataSet.Tables[k]), new CodeExpression[1] { Indexer(Property(Variable("ds"), "Tables"), Str(dataSet.Tables[k].TableName)) })))));
		}
		arrayList.Add(Assign(Property(This(), "DataSetName"), Property(Variable("ds"), "DataSetName")));
		arrayList.Add(Assign(Property(This(), "Prefix"), Property(Variable("ds"), "Prefix")));
		arrayList.Add(Assign(Property(This(), "Namespace"), Property(Variable("ds"), "Namespace")));
		arrayList.Add(Assign(Property(This(), "Locale"), Property(Variable("ds"), "Locale")));
		arrayList.Add(Assign(Property(This(), "CaseSensitive"), Property(Variable("ds"), "CaseSensitive")));
		arrayList.Add(Assign(Property(This(), "EnforceConstraints"), Property(Variable("ds"), "EnforceConstraints")));
		arrayList.Add(Stm(MethodCall(This(), "Merge", new CodeExpression[3]
		{
			Variable("ds"),
			Primitive(false),
			Field(TypeExpr(typeof(MissingSchemaAction)), "Add")
		})));
		arrayList.Add(Stm(MethodCall(This(), "InitVars")));
		CodeStatement[] array = new CodeStatement[arrayList.Count];
		arrayList.CopyTo(array);
		codeConstructor.Statements.Add(If(IdNotEQ(Variable("strSchema"), Primitive(null)), array, new CodeStatement[3]
		{
			Stm(MethodCall(This(), "BeginInit")),
			Stm(MethodCall(This(), "InitClass")),
			Stm(MethodCall(This(), "EndInit"))
		}));
		codeConstructor.Statements.Add(MethodCall(This(), "GetSerializationData", new CodeExpression[2]
		{
			Argument("info"),
			Argument("context")
		}));
		codeConstructor.Statements.Add(VariableDecl(typeof(CollectionChangeEventHandler), "schemaChangedHandler", new CodeDelegateCreateExpression(Type(typeof(CollectionChangeEventHandler)), This(), "SchemaChanged")));
		codeConstructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Tables"), "CollectionChanged"), Variable("schemaChangedHandler")));
		codeConstructor.Statements.Add(new CodeAttachEventStatement(new CodeEventReferenceExpression(Property(This(), "Relations"), "CollectionChanged"), Variable("schemaChangedHandler")));
		codeTypeDeclaration.Members.Add(codeConstructor);
		CodeMemberMethod codeMemberMethod = MethodDecl(typeof(DataSet), "Clone", (MemberAttributes)24580);
		codeMemberMethod.Statements.Add(VariableDecl(text, "cln", Cast(text, MethodCall(Base(), "Clone", new CodeExpression[0]))));
		codeMemberMethod.Statements.Add(MethodCall(Variable("cln"), "InitVars", new CodeExpression[0]));
		codeMemberMethod.Statements.Add(Return(Variable("cln")));
		codeTypeDeclaration.Members.Add(codeMemberMethod);
		CodeMemberMethod codeMemberMethod2 = MethodDecl(typeof(void), "InitVars", (MemberAttributes)4098);
		codeMemberMethod2.Statements.Add(MethodCall(This(), "InitVars", new CodeExpression[1] { Primitive(true) }));
		codeTypeDeclaration.Members.Add(codeMemberMethod2);
		CodeMemberMethod codeMemberMethod3 = MethodDecl(typeof(void), "InitClass", MemberAttributes.Private);
		CodeMemberMethod codeMemberMethod4 = MethodDecl(typeof(void), "InitVars", (MemberAttributes)4098);
		codeMemberMethod4.Parameters.Add(ParameterDecl(typeof(bool), "initTable"));
		codeMemberMethod3.Statements.Add(Assign(Property(This(), "DataSetName"), Str(dataSet.DataSetName)));
		codeMemberMethod3.Statements.Add(Assign(Property(This(), "Prefix"), Str(dataSet.Prefix)));
		codeMemberMethod3.Statements.Add(Assign(Property(This(), "Namespace"), Str(dataSet.Namespace)));
		codeMemberMethod3.Statements.Add(Assign(Property(This(), "Locale"), New(typeof(CultureInfo), new CodeExpression[1] { Str(dataSet.Locale.ToString()) })));
		codeMemberMethod3.Statements.Add(Assign(Property(This(), "CaseSensitive"), Primitive(dataSet.CaseSensitive)));
		codeMemberMethod3.Statements.Add(Assign(Property(This(), "EnforceConstraints"), Primitive(dataSet.EnforceConstraints)));
		for (int l = 0; l < dataSet.Tables.Count; l++)
		{
			CodeExpression codeExpression = Field(This(), TableFieldName(dataSet.Tables[l]));
			codeMemberMethod3.Statements.Add(Assign(codeExpression, New(TableClassName(dataSet.Tables[l]), new CodeExpression[0])));
			codeMemberMethod3.Statements.Add(MethodCall(Property(This(), "Tables"), "Add", codeExpression));
			codeMemberMethod4.Statements.Add(Assign(codeExpression, Cast(TableClassName(dataSet.Tables[l]), Indexer(Property(This(), "Tables"), Str(dataSet.Tables[l].TableName)))));
			codeMemberMethod4.Statements.Add(If(EQ(Variable("initTable"), Primitive(true)), new CodeStatement[1] { If(IdNotEQ(codeExpression, Primitive(null)), Stm(MethodCall(codeExpression, "InitVars"))) }));
		}
		CodeMemberMethod codeMemberMethod5 = MethodDecl(typeof(bool), "ShouldSerializeTables", (MemberAttributes)12292);
		codeMemberMethod5.Statements.Add(Return(Primitive(false)));
		codeTypeDeclaration.Members.Add(codeMemberMethod5);
		CodeMemberMethod codeMemberMethod6 = MethodDecl(typeof(bool), "ShouldSerializeRelations", (MemberAttributes)12292);
		codeMemberMethod6.Statements.Add(Return(Primitive(false)));
		codeTypeDeclaration.Members.Add(codeMemberMethod6);
		CodeMemberMethod codeMemberMethod7 = MethodDecl(typeof(XmlSchemaComplexType), "GetTypedDataSetSchema", (MemberAttributes)24579);
		codeMemberMethod7.Parameters.Add(ParameterDecl(typeof(XmlSchemaSet), "xs"));
		codeMemberMethod7.Statements.Add(VariableDecl(text, "ds", New(text, new CodeExpression[0])));
		codeMemberMethod7.Statements.Add(MethodCall(Argument("xs"), "Add", new CodeExpression[1] { MethodCall(Variable("ds"), "GetSchemaSerializable", new CodeExpression[0]) }));
		codeMemberMethod7.Statements.Add(VariableDecl(typeof(XmlSchemaComplexType), "type", New(typeof(XmlSchemaComplexType), new CodeExpression[0])));
		codeMemberMethod7.Statements.Add(VariableDecl(typeof(XmlSchemaSequence), "sequence", New(typeof(XmlSchemaSequence), new CodeExpression[0])));
		codeMemberMethod7.Statements.Add(VariableDecl(typeof(XmlSchemaAny), "any", New(typeof(XmlSchemaAny), new CodeExpression[0])));
		codeMemberMethod7.Statements.Add(Assign(Property(Variable("any"), "Namespace"), Property(Variable("ds"), "Namespace")));
		codeMemberMethod7.Statements.Add(MethodCall(Property(Variable("sequence"), "Items"), "Add", new CodeExpression[1] { Variable("any") }));
		codeMemberMethod7.Statements.Add(Assign(Property(Variable("type"), "Particle"), Variable("sequence")));
		codeMemberMethod7.Statements.Add(Return(Variable("type")));
		codeTypeDeclaration.Members.Add(codeMemberMethod7);
		CodeMemberMethod codeMemberMethod8 = MethodDecl(typeof(void), "ReadXmlSerializable", (MemberAttributes)12292);
		codeMemberMethod8.Parameters.Add(ParameterDecl(typeof(XmlReader), "reader"));
		codeMemberMethod8.Statements.Add(MethodCall(This(), "Reset", new CodeExpression[0]));
		codeMemberMethod8.Statements.Add(VariableDecl(typeof(DataSet), "ds", New(typeof(DataSet), new CodeExpression[0])));
		codeMemberMethod8.Statements.Add(MethodCall(Variable("ds"), "ReadXml", new CodeExpression[1] { Argument("reader") }));
		for (int m = 0; m < dataSet.Tables.Count; m++)
		{
			codeMemberMethod8.Statements.Add(If(IdNotEQ(Indexer(Property(Variable("ds"), "Tables"), Str(dataSet.Tables[m].TableName)), Primitive(null)), Stm(MethodCall(Property(This(), "Tables"), "Add", New(TableClassName(dataSet.Tables[m]), new CodeExpression[1] { Indexer(Property(Variable("ds"), "Tables"), Str(dataSet.Tables[m].TableName)) })))));
		}
		codeMemberMethod8.Statements.Add(Assign(Property(This(), "DataSetName"), Property(Variable("ds"), "DataSetName")));
		codeMemberMethod8.Statements.Add(Assign(Property(This(), "Prefix"), Property(Variable("ds"), "Prefix")));
		codeMemberMethod8.Statements.Add(Assign(Property(This(), "Namespace"), Property(Variable("ds"), "Namespace")));
		codeMemberMethod8.Statements.Add(Assign(Property(This(), "Locale"), Property(Variable("ds"), "Locale")));
		codeMemberMethod8.Statements.Add(Assign(Property(This(), "CaseSensitive"), Property(Variable("ds"), "CaseSensitive")));
		codeMemberMethod8.Statements.Add(Assign(Property(This(), "EnforceConstraints"), Property(Variable("ds"), "EnforceConstraints")));
		codeMemberMethod8.Statements.Add(MethodCall(This(), "Merge", new CodeExpression[3]
		{
			Variable("ds"),
			Primitive(false),
			Field(TypeExpr(typeof(MissingSchemaAction)), "Add")
		}));
		codeMemberMethod8.Statements.Add(MethodCall(This(), "InitVars"));
		codeTypeDeclaration.Members.Add(codeMemberMethod8);
		CodeMemberMethod codeMemberMethod9 = MethodDecl(typeof(XmlSchema), "GetSchemaSerializable", (MemberAttributes)12292);
		codeMemberMethod9.Statements.Add(VariableDecl(typeof(MemoryStream), "stream", New(typeof(MemoryStream), new CodeExpression[0])));
		codeMemberMethod9.Statements.Add(MethodCall(This(), "WriteXmlSchema", New(typeof(XmlTextWriter), new CodeExpression[2]
		{
			Argument("stream"),
			Primitive(null)
		})));
		codeMemberMethod9.Statements.Add(Assign(Property(Argument("stream"), "Position"), Primitive(0)));
		codeMemberMethod9.Statements.Add(Return(MethodCall(TypeExpr("System.Xml.Schema.XmlSchema"), "Read", new CodeExpression[2]
		{
			New(typeof(XmlTextReader), new CodeExpression[1] { Argument("stream") }),
			Primitive(null)
		})));
		codeTypeDeclaration.Members.Add(codeMemberMethod9);
		CodeExpression codeExpression2 = null;
		foreach (DataTable table in dataSet.Tables)
		{
			foreach (Constraint constraint in table.Constraints)
			{
				if (constraint is ForeignKeyConstraint)
				{
					ForeignKeyConstraint foreignKeyConstraint = (ForeignKeyConstraint)constraint;
					CodeArrayCreateExpression codeArrayCreateExpression = new CodeArrayCreateExpression(typeof(DataColumn), 0);
					DataColumn[] columns = foreignKeyConstraint.Columns;
					foreach (DataColumn dataColumn in columns)
					{
						codeArrayCreateExpression.Initializers.Add(Property(Field(This(), TableFieldName(dataColumn.Table)), TableColumnPropertyName(dataColumn)));
					}
					CodeArrayCreateExpression codeArrayCreateExpression2 = new CodeArrayCreateExpression(typeof(DataColumn), 0);
					columns = foreignKeyConstraint.RelatedColumnsReference;
					foreach (DataColumn dataColumn2 in columns)
					{
						codeArrayCreateExpression2.Initializers.Add(Property(Field(This(), TableFieldName(dataColumn2.Table)), TableColumnPropertyName(dataColumn2)));
					}
					if (codeExpression2 == null)
					{
						codeMemberMethod3.Statements.Add(VariableDecl(typeof(ForeignKeyConstraint), "fkc"));
						codeExpression2 = Variable("fkc");
					}
					codeMemberMethod3.Statements.Add(Assign(codeExpression2, New(typeof(ForeignKeyConstraint), new CodeExpression[3]
					{
						Str(foreignKeyConstraint.ConstraintName),
						codeArrayCreateExpression2,
						codeArrayCreateExpression
					})));
					codeMemberMethod3.Statements.Add(MethodCall(Property(Field(This(), TableFieldName(table)), "Constraints"), "Add", codeExpression2));
					string field = foreignKeyConstraint.AcceptRejectRule.ToString();
					string field2 = foreignKeyConstraint.DeleteRule.ToString();
					string field3 = foreignKeyConstraint.UpdateRule.ToString();
					codeMemberMethod3.Statements.Add(Assign(Property(codeExpression2, "AcceptRejectRule"), Field(TypeExpr(foreignKeyConstraint.AcceptRejectRule.GetType()), field)));
					codeMemberMethod3.Statements.Add(Assign(Property(codeExpression2, "DeleteRule"), Field(TypeExpr(foreignKeyConstraint.DeleteRule.GetType()), field2)));
					codeMemberMethod3.Statements.Add(Assign(Property(codeExpression2, "UpdateRule"), Field(TypeExpr(foreignKeyConstraint.UpdateRule.GetType()), field3)));
				}
			}
		}
		foreach (DataRelation relation in dataSet.Relations)
		{
			CodeArrayCreateExpression codeArrayCreateExpression3 = new CodeArrayCreateExpression(typeof(DataColumn), 0);
			string field4 = TableFieldName(relation.ParentTable);
			DataColumn[] columns = relation.ParentColumnsReference;
			foreach (DataColumn column in columns)
			{
				codeArrayCreateExpression3.Initializers.Add(Property(Field(This(), field4), TableColumnPropertyName(column)));
			}
			CodeArrayCreateExpression codeArrayCreateExpression4 = new CodeArrayCreateExpression(typeof(DataColumn), 0);
			string field5 = TableFieldName(relation.ChildTable);
			columns = relation.ChildColumnsReference;
			foreach (DataColumn column2 in columns)
			{
				codeArrayCreateExpression4.Initializers.Add(Property(Field(This(), field5), TableColumnPropertyName(column2)));
			}
			codeMemberMethod3.Statements.Add(Assign(Field(This(), RelationFieldName(relation)), New(typeof(DataRelation), new CodeExpression[4]
			{
				Str(relation.RelationName),
				codeArrayCreateExpression3,
				codeArrayCreateExpression4,
				Primitive(false)
			})));
			if (relation.Nested)
			{
				codeMemberMethod3.Statements.Add(Assign(Property(Field(This(), RelationFieldName(relation)), "Nested"), Primitive(true)));
			}
			codeMemberMethod3.Statements.Add(MethodCall(Property(This(), "Relations"), "Add", Field(This(), RelationFieldName(relation))));
			codeMemberMethod4.Statements.Add(Assign(Field(This(), RelationFieldName(relation)), Indexer(Property(This(), "Relations"), Str(relation.RelationName))));
		}
		codeTypeDeclaration.Members.Add(codeMemberMethod4);
		codeTypeDeclaration.Members.Add(codeMemberMethod3);
		for (int num = 0; num < dataSet.Tables.Count; num++)
		{
			string text2 = TablePropertyName(dataSet.Tables[num]);
			CodeMemberProperty codeMemberProperty = PropertyDecl(TableClassName(dataSet.Tables[num]), text2, (MemberAttributes)24578);
			codeMemberProperty.CustomAttributes.Add(AttributeDecl("System.ComponentModel.Browsable", Primitive(false)));
			codeMemberProperty.CustomAttributes.Add(AttributeDecl("System.ComponentModel.DesignerSerializationVisibilityAttribute", Field(TypeExpr(typeof(DesignerSerializationVisibility)), "Content")));
			codeMemberProperty.GetStatements.Add(Return(Field(This(), TableFieldName(dataSet.Tables[num]))));
			codeTypeDeclaration.Members.Add(codeMemberProperty);
			CodeMemberMethod codeMemberMethod10 = MethodDecl(typeof(bool), "ShouldSerialize" + text2, MemberAttributes.Private);
			codeMemberMethod10.Statements.Add(Return(Primitive(false)));
			codeTypeDeclaration.Members.Add(codeMemberMethod10);
		}
		CodeMemberMethod codeMemberMethod11 = MethodDecl(typeof(void), "SchemaChanged", MemberAttributes.Private);
		codeMemberMethod11.Parameters.Add(ParameterDecl(typeof(object), "sender"));
		codeMemberMethod11.Parameters.Add(ParameterDecl(typeof(CollectionChangeEventArgs), "e"));
		codeMemberMethod11.Statements.Add(If(EQ(Property(Argument("e"), "Action"), Field(TypeExpr(typeof(CollectionChangeAction)), "Remove")), Stm(MethodCall(This(), "InitVars"))));
		codeTypeDeclaration.Members.Add(codeMemberMethod11);
		bool flag = false;
		CodeMemberMethod codeMemberMethod12 = MethodDecl(typeof(void), "InitExpressions", MemberAttributes.Private);
		foreach (DataTable table2 in dataSet.Tables)
		{
			for (int num2 = 0; num2 < table2.Columns.Count; num2++)
			{
				DataColumn dataColumn3 = table2.Columns[num2];
				CodeExpression exp = Property(Field(This(), TableFieldName(table2)), TableColumnPropertyName(dataColumn3));
				if (dataColumn3.Expression.Length > 0)
				{
					flag = true;
					codeMemberMethod12.Statements.Add(Assign(Property(exp, "Expression"), Str(dataColumn3.Expression)));
				}
			}
		}
		if (flag)
		{
			codeTypeDeclaration.Members.Add(codeMemberMethod12);
			codeMemberMethod3.Statements.Add(MethodCall(This(), "InitExpressions"));
		}
		return codeTypeDeclaration;
	}

	private static CodeExpression This()
	{
		return new CodeThisReferenceExpression();
	}

	private static CodeExpression Base()
	{
		return new CodeBaseReferenceExpression();
	}

	private static CodeExpression Value()
	{
		return new CodePropertySetValueReferenceExpression();
	}

	private static CodeTypeReference Type(string type)
	{
		return new CodeTypeReference(type);
	}

	private static CodeTypeReference Type(Type type)
	{
		return new CodeTypeReference(type);
	}

	private static CodeTypeReference Type(string type, int rank)
	{
		return new CodeTypeReference(type, rank);
	}

	private static CodeTypeReferenceExpression TypeExpr(Type type)
	{
		return new CodeTypeReferenceExpression(type);
	}

	private static CodeTypeReferenceExpression TypeExpr(string type)
	{
		return new CodeTypeReferenceExpression(type);
	}

	private static CodeExpression Cast(string type, CodeExpression expr)
	{
		return new CodeCastExpression(type, expr);
	}

	private static CodeExpression Cast(CodeTypeReference type, CodeExpression expr)
	{
		return new CodeCastExpression(type, expr);
	}

	private static CodeExpression TypeOf(string type)
	{
		return new CodeTypeOfExpression(type);
	}

	private static CodeExpression Field(CodeExpression exp, string field)
	{
		return new CodeFieldReferenceExpression(exp, field);
	}

	private static CodeExpression Property(CodeExpression exp, string property)
	{
		return new CodePropertyReferenceExpression(exp, property);
	}

	private static CodeExpression Argument(string argument)
	{
		return new CodeArgumentReferenceExpression(argument);
	}

	private static CodeExpression Variable(string variable)
	{
		return new CodeVariableReferenceExpression(variable);
	}

	private static CodeExpression Event(string eventName)
	{
		return new CodeEventReferenceExpression(This(), eventName);
	}

	private static CodeExpression New(string type, CodeExpression[] parameters)
	{
		return new CodeObjectCreateExpression(type, parameters);
	}

	private static CodeExpression New(Type type, CodeExpression[] parameters)
	{
		return new CodeObjectCreateExpression(type, parameters);
	}

	private static CodeExpression Primitive(object primitive)
	{
		return new CodePrimitiveExpression(primitive);
	}

	private static CodeExpression Str(string str)
	{
		return Primitive(str);
	}

	private static CodeExpression MethodCall(CodeExpression targetObject, string methodName, CodeExpression[] parameters)
	{
		return new CodeMethodInvokeExpression(targetObject, methodName, parameters);
	}

	private static CodeExpression MethodCall(CodeExpression targetObject, string methodName)
	{
		return new CodeMethodInvokeExpression(targetObject, methodName);
	}

	private static CodeExpression MethodCall(CodeExpression targetObject, string methodName, CodeExpression par)
	{
		return new CodeMethodInvokeExpression(targetObject, methodName, par);
	}

	private static CodeExpression DelegateCall(CodeExpression targetObject, CodeExpression par)
	{
		return new CodeDelegateInvokeExpression(targetObject, This(), par);
	}

	private static CodeExpression Indexer(CodeExpression targetObject, CodeExpression indices)
	{
		return new CodeIndexerExpression(targetObject, indices);
	}

	private static CodeBinaryOperatorExpression BinOperator(CodeExpression left, CodeBinaryOperatorType op, CodeExpression right)
	{
		return new CodeBinaryOperatorExpression(left, op, right);
	}

	private static CodeBinaryOperatorExpression IdNotEQ(CodeExpression left, CodeExpression right)
	{
		return BinOperator(left, CodeBinaryOperatorType.IdentityInequality, right);
	}

	private static CodeBinaryOperatorExpression EQ(CodeExpression left, CodeExpression right)
	{
		return BinOperator(left, CodeBinaryOperatorType.ValueEquality, right);
	}

	private static CodeStatement Stm(CodeExpression expr)
	{
		return new CodeExpressionStatement(expr);
	}

	private static CodeStatement Return(CodeExpression expr)
	{
		return new CodeMethodReturnStatement(expr);
	}

	private static CodeStatement Return()
	{
		return new CodeMethodReturnStatement();
	}

	private static CodeStatement Assign(CodeExpression left, CodeExpression right)
	{
		return new CodeAssignStatement(left, right);
	}

	private static CodeStatement Throw(Type exception, string arg, string inner)
	{
		return new CodeThrowExceptionStatement(New(exception, new CodeExpression[2]
		{
			Str(Res.GetString(arg)),
			Variable(inner)
		}));
	}

	private static CodeStatement If(CodeExpression cond, CodeStatement[] trueStms, CodeStatement[] falseStms)
	{
		return new CodeConditionStatement(cond, trueStms, falseStms);
	}

	private static CodeStatement If(CodeExpression cond, CodeStatement[] trueStms)
	{
		return new CodeConditionStatement(cond, trueStms);
	}

	private static CodeStatement If(CodeExpression cond, CodeStatement trueStm)
	{
		return If(cond, new CodeStatement[1] { trueStm });
	}

	private static CodeMemberField FieldDecl(string type, string name)
	{
		return new CodeMemberField(type, name);
	}

	private static CodeMemberField FieldDecl(Type type, string name)
	{
		return new CodeMemberField(type, name);
	}

	private static CodeMemberMethod Method(CodeTypeReference type, string name, MemberAttributes attributes)
	{
		return new CodeMemberMethod
		{
			ReturnType = type,
			Name = name,
			Attributes = attributes
		};
	}

	private static CodeMemberMethod MethodDecl(Type type, string name, MemberAttributes attributes)
	{
		return Method(Type(type), name, attributes);
	}

	private static CodeMemberMethod MethodDecl(string type, string name, MemberAttributes attributes)
	{
		return Method(Type(type), name, attributes);
	}

	private static CodeMemberProperty PropertyDecl(string type, string name, MemberAttributes attributes)
	{
		return new CodeMemberProperty
		{
			Type = Type(type),
			Name = name,
			Attributes = attributes
		};
	}

	private static CodeMemberProperty PropertyDecl(Type type, string name, MemberAttributes attributes)
	{
		return new CodeMemberProperty
		{
			Type = Type(type),
			Name = name,
			Attributes = attributes
		};
	}

	private static CodeStatement VariableDecl(Type type, string name)
	{
		return new CodeVariableDeclarationStatement(type, name);
	}

	private static CodeStatement VariableDecl(string type, string name, CodeExpression initExpr)
	{
		return new CodeVariableDeclarationStatement(type, name, initExpr);
	}

	private static CodeStatement VariableDecl(Type type, string name, CodeExpression initExpr)
	{
		return new CodeVariableDeclarationStatement(type, name, initExpr);
	}

	private static CodeMemberEvent EventDecl(string type, string name)
	{
		return new CodeMemberEvent
		{
			Name = name,
			Type = Type(type),
			Attributes = (MemberAttributes)24578
		};
	}

	private static CodeParameterDeclarationExpression ParameterDecl(string type, string name)
	{
		return new CodeParameterDeclarationExpression(type, name);
	}

	private static CodeParameterDeclarationExpression ParameterDecl(Type type, string name)
	{
		return new CodeParameterDeclarationExpression(type, name);
	}

	private static CodeAttributeDeclaration AttributeDecl(string name)
	{
		return new CodeAttributeDeclaration(name);
	}

	private static CodeAttributeDeclaration AttributeDecl(string name, CodeExpression value)
	{
		return new CodeAttributeDeclaration(name, new CodeAttributeArgument(value));
	}

	private static CodeStatement Try(CodeStatement tryStmnt, CodeCatchClause catchClause)
	{
		return new CodeTryCatchFinallyStatement(new CodeStatement[1] { tryStmnt }, new CodeCatchClause[1] { catchClause });
	}

	private static CodeCatchClause Catch(Type type, string name, CodeStatement catchStmnt)
	{
		return new CodeCatchClause
		{
			CatchExceptionType = Type(type),
			LocalName = name,
			Statements = { catchStmnt }
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.TypedDataSetGenerator" /> class.</summary>
	public TypedDataSetGenerator()
	{
	}
}
