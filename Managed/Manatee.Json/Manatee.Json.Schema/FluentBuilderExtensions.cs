using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Manatee.Json.Schema;

public static class FluentBuilderExtensions
{
	public static JsonSchema AdditionalItems(this JsonSchema schema, JsonSchema additionalItems)
	{
		schema.Add(new AdditionalItemsKeyword(additionalItems));
		return schema;
	}

	public static JsonSchema AdditionalProperties(this JsonSchema schema, JsonSchema otherSchema)
	{
		schema.Add(new AdditionalPropertiesKeyword(otherSchema));
		return schema;
	}

	public static JsonSchema AllOf(this JsonSchema schema, params JsonSchema[] definitions)
	{
		AllOfKeyword allOfKeyword = schema.OfType<AllOfKeyword>().FirstOrDefault();
		if (allOfKeyword == null)
		{
			allOfKeyword = new AllOfKeyword();
			schema.Add(allOfKeyword);
		}
		allOfKeyword.AddRange(definitions);
		return schema;
	}

	public static JsonSchema AnyOf(this JsonSchema schema, params JsonSchema[] definitions)
	{
		AnyOfKeyword anyOfKeyword = schema.OfType<AnyOfKeyword>().FirstOrDefault();
		if (anyOfKeyword == null)
		{
			anyOfKeyword = new AnyOfKeyword();
			schema.Add(anyOfKeyword);
		}
		anyOfKeyword.AddRange(definitions);
		return schema;
	}

	public static JsonSchema Comment(this JsonSchema schema, string comment)
	{
		schema.Add(new CommentKeyword(comment));
		return schema;
	}

	public static JsonSchema Const(this JsonSchema schema, JsonValue value)
	{
		schema.Add(new ConstKeyword(value));
		return schema;
	}

	public static JsonSchema Contains(this JsonSchema schema, JsonSchema match)
	{
		schema.Add(new ContainsKeyword(match));
		return schema;
	}

	public static JsonSchema ContentEncoding(this JsonSchema schema, string encoding)
	{
		schema.Add(new ContentEncodingKeyword(encoding));
		return schema;
	}

	public static JsonSchema ContentMediaType(this JsonSchema schema, string mediaType)
	{
		schema.Add(new ContentMediaTypeKeyword(mediaType));
		return schema;
	}

	public static JsonSchema ContentSchema(this JsonSchema schema, JsonSchema match)
	{
		schema.Add(new ContentSchemaKeyword(match));
		return schema;
	}

	public static JsonSchema Default(this JsonSchema schema, JsonValue value)
	{
		schema.Add(new DefaultKeyword(value));
		return schema;
	}

	public static JsonSchema Def(this JsonSchema schema, string name, JsonSchema definition)
	{
		DefsKeyword defsKeyword = schema.OfType<DefsKeyword>().FirstOrDefault();
		if (defsKeyword == null)
		{
			defsKeyword = new DefsKeyword();
			schema.Add(defsKeyword);
		}
		defsKeyword.Add(name, definition);
		return schema;
	}

	public static JsonSchema Definition(this JsonSchema schema, string name, JsonSchema definition)
	{
		DefinitionsKeyword definitionsKeyword = schema.OfType<DefinitionsKeyword>().FirstOrDefault();
		if (definitionsKeyword == null)
		{
			definitionsKeyword = new DefinitionsKeyword();
			schema.Add(definitionsKeyword);
		}
		definitionsKeyword.Add(name, definition);
		return schema;
	}

	public static JsonSchema Dependency(this JsonSchema schema, string name, params string[] dependencies)
	{
		DependenciesKeyword dependenciesKeyword = schema.OfType<DependenciesKeyword>().FirstOrDefault();
		if (dependenciesKeyword == null)
		{
			dependenciesKeyword = new DependenciesKeyword();
			schema.Add(dependenciesKeyword);
		}
		dependenciesKeyword.Add(new PropertyDependency(name, dependencies));
		return schema;
	}

	public static JsonSchema DependentRequired(this JsonSchema schema, string name, params string[] dependencies)
	{
		DependentRequiredKeyword dependentRequiredKeyword = schema.OfType<DependentRequiredKeyword>().FirstOrDefault();
		if (dependentRequiredKeyword == null)
		{
			dependentRequiredKeyword = new DependentRequiredKeyword();
			schema.Add(dependentRequiredKeyword);
		}
		dependentRequiredKeyword.Add(new PropertyDependency(name, dependencies));
		return schema;
	}

	public static JsonSchema Dependency(this JsonSchema schema, string name, JsonSchema dependency)
	{
		DependenciesKeyword dependenciesKeyword = schema.OfType<DependenciesKeyword>().FirstOrDefault();
		if (dependenciesKeyword == null)
		{
			dependenciesKeyword = new DependenciesKeyword();
			schema.Add(dependenciesKeyword);
		}
		dependenciesKeyword.Add(new SchemaDependency(name, dependency));
		return schema;
	}

	public static JsonSchema DependentSchema(this JsonSchema schema, string name, JsonSchema dependency)
	{
		DependentSchemasKeyword dependentSchemasKeyword = schema.OfType<DependentSchemasKeyword>().FirstOrDefault();
		if (dependentSchemasKeyword == null)
		{
			dependentSchemasKeyword = new DependentSchemasKeyword();
			schema.Add(dependentSchemasKeyword);
		}
		dependentSchemasKeyword.Add(new SchemaDependency(name, dependency));
		return schema;
	}

	public static JsonSchema Description(this JsonSchema schema, string description)
	{
		schema.Add(new DescriptionKeyword(description));
		return schema;
	}

	public static JsonSchema Else(this JsonSchema schema, JsonSchema elseSchema)
	{
		schema.Add(new ElseKeyword(elseSchema));
		return schema;
	}

	public static JsonSchema Enum(this JsonSchema schema, params JsonValue[] values)
	{
		schema.Add(new EnumKeyword(values));
		return schema;
	}

	public static JsonSchema Examples(this JsonSchema schema, params JsonValue[] value)
	{
		schema.Add(new ExamplesKeyword(value));
		return schema;
	}

	public static JsonSchema ExclusiveMaximum(this JsonSchema schema, double maximum)
	{
		schema.Add(new ExclusiveMaximumKeyword(maximum));
		return schema;
	}

	public static JsonSchema ExclusiveMaximumDraft04(this JsonSchema schema, bool isExclusive)
	{
		schema.Add(new ExclusiveMaximumDraft04Keyword(isExclusive));
		return schema;
	}

	public static JsonSchema ExclusiveMinimum(this JsonSchema schema, double minimum)
	{
		schema.Add(new ExclusiveMinimumKeyword(minimum));
		return schema;
	}

	public static JsonSchema ExclusiveMinimumDraft04(this JsonSchema schema, bool isExclusive)
	{
		schema.Add(new ExclusiveMinimumDraft04Keyword(isExclusive));
		return schema;
	}

	public static JsonSchema Format(this JsonSchema schema, Format format)
	{
		schema.Add(new FormatKeyword(format));
		return schema;
	}

	public static JsonSchema Id(this JsonSchema schema, string id)
	{
		schema.Add(new IdKeyword(id));
		return schema;
	}

	public static JsonSchema IdDraft04(this JsonSchema schema, string id)
	{
		schema.Add(new IdKeywordDraft04(id));
		return schema;
	}

	public static JsonSchema If(this JsonSchema schema, JsonSchema ifSchema)
	{
		schema.Add(new IfKeyword(ifSchema));
		return schema;
	}

	public static JsonSchema Item(this JsonSchema schema, JsonSchema definition)
	{
		ItemsKeyword itemsKeyword = schema.OfType<ItemsKeyword>().FirstOrDefault();
		if (itemsKeyword == null)
		{
			itemsKeyword = new ItemsKeyword
			{
				IsArray = true
			};
			schema.Add(itemsKeyword);
		}
		itemsKeyword.Add(definition);
		return schema;
	}

	public static JsonSchema Items(this JsonSchema schema, JsonSchema definition)
	{
		ItemsKeyword item = new ItemsKeyword { definition };
		schema.Add(item);
		return schema;
	}

	public static JsonSchema MaxContains(this JsonSchema schema, uint count)
	{
		schema.Add(new MaxContainsKeyword(count));
		return schema;
	}

	public static JsonSchema Maximum(this JsonSchema schema, double maximum)
	{
		schema.Add(new MaximumKeyword(maximum));
		return schema;
	}

	public static JsonSchema MaxItems(this JsonSchema schema, uint count)
	{
		schema.Add(new MaxItemsKeyword(count));
		return schema;
	}

	public static JsonSchema MaxLength(this JsonSchema schema, uint length)
	{
		schema.Add(new MaxLengthKeyword(length));
		return schema;
	}

	public static JsonSchema MaxProperties(this JsonSchema schema, uint count)
	{
		schema.Add(new MaxPropertiesKeyword(count));
		return schema;
	}

	public static JsonSchema MinContains(this JsonSchema schema, uint count)
	{
		schema.Add(new MinContainsKeyword(count));
		return schema;
	}

	public static JsonSchema Minimum(this JsonSchema schema, double minimum)
	{
		schema.Add(new MinimumKeyword(minimum));
		return schema;
	}

	public static JsonSchema MinItems(this JsonSchema schema, uint count)
	{
		schema.Add(new MinItemsKeyword(count));
		return schema;
	}

	public static JsonSchema MinLength(this JsonSchema schema, uint length)
	{
		schema.Add(new MinLengthKeyword(length));
		return schema;
	}

	public static JsonSchema MinProperties(this JsonSchema schema, uint count)
	{
		schema.Add(new MinPropertiesKeyword(count));
		return schema;
	}

	public static JsonSchema MultipleOf(this JsonSchema schema, double divisor)
	{
		schema.Add(new MultipleOfKeyword(divisor));
		return schema;
	}

	public static JsonSchema Not(this JsonSchema schema, JsonSchema notSchema)
	{
		schema.Add(new NotKeyword(notSchema));
		return schema;
	}

	public static JsonSchema OneOf(this JsonSchema schema, params JsonSchema[] definitions)
	{
		OneOfKeyword oneOfKeyword = schema.OfType<OneOfKeyword>().FirstOrDefault();
		if (oneOfKeyword == null)
		{
			oneOfKeyword = new OneOfKeyword();
			schema.Add(oneOfKeyword);
		}
		oneOfKeyword.AddRange(definitions);
		return schema;
	}

	public static JsonSchema Pattern(this JsonSchema schema, [RegexPattern] string pattern)
	{
		schema.Add(new PatternKeyword(pattern));
		return schema;
	}

	public static JsonSchema Pattern(this JsonSchema schema, Regex pattern)
	{
		schema.Add(new PatternKeyword(pattern));
		return schema;
	}

	public static JsonSchema PatternProperty(this JsonSchema schema, [RegexPattern] string name, JsonSchema property)
	{
		PatternPropertiesKeyword patternPropertiesKeyword = schema.OfType<PatternPropertiesKeyword>().FirstOrDefault();
		if (patternPropertiesKeyword == null)
		{
			patternPropertiesKeyword = new PatternPropertiesKeyword();
			schema.Add(patternPropertiesKeyword);
		}
		patternPropertiesKeyword.Add(name, property);
		return schema;
	}

	public static JsonSchema Property(this JsonSchema schema, string name, JsonSchema property)
	{
		PropertiesKeyword propertiesKeyword = schema.OfType<PropertiesKeyword>().FirstOrDefault();
		if (propertiesKeyword == null)
		{
			propertiesKeyword = new PropertiesKeyword();
			schema.Add(propertiesKeyword);
		}
		propertiesKeyword.Add(name, property);
		return schema;
	}

	public static JsonSchema PropertyNames(this JsonSchema schema, JsonSchema otherSchema)
	{
		schema.Add(new PropertyNamesKeyword(otherSchema));
		return schema;
	}

	public static JsonSchema ReadOnly(this JsonSchema schema, bool isReadOnly)
	{
		schema.Add(new ReadOnlyKeyword(isReadOnly));
		return schema;
	}

	public static JsonSchema RecursiveAnchor(this JsonSchema schema, bool value)
	{
		schema.Add(new RecursiveAnchorKeyword(value));
		return schema;
	}

	public static JsonSchema RecursiveRef(this JsonSchema schema, string reference)
	{
		schema.Add(new RecursiveRefKeyword(reference));
		return schema;
	}

	public static JsonSchema RecursiveRefRoot(this JsonSchema schema)
	{
		schema.Add(new RecursiveRefKeyword("#"));
		return schema;
	}

	public static JsonSchema Ref(this JsonSchema schema, string reference)
	{
		schema.Add(new RefKeyword(reference));
		return schema;
	}

	public static JsonSchema RefRoot(this JsonSchema schema)
	{
		schema.Add(new RefKeyword("#"));
		return schema;
	}

	public static JsonSchema Required(this JsonSchema schema, params string[] values)
	{
		schema.Add(new RequiredKeyword(values));
		return schema;
	}

	public static JsonSchema Schema(this JsonSchema schema, string schemaCallout)
	{
		schema.Add(new SchemaKeyword(schemaCallout));
		return schema;
	}

	public static JsonSchema Then(this JsonSchema schema, JsonSchema thenSchema)
	{
		schema.Add(new ThenKeyword(thenSchema));
		return schema;
	}

	public static JsonSchema Title(this JsonSchema schema, string title)
	{
		schema.Add(new TitleKeyword(title));
		return schema;
	}

	public static JsonSchema Type(this JsonSchema schema, JsonSchemaType type)
	{
		schema.Add(new TypeKeyword(type));
		return schema;
	}

	public static JsonSchema UnevaluatedItems(this JsonSchema schema, JsonSchema otherSchema)
	{
		schema.Add(new UnevaluatedItemsKeyword(otherSchema));
		return schema;
	}

	public static JsonSchema UnevaluatedProperties(this JsonSchema schema, JsonSchema otherSchema)
	{
		schema.Add(new UnevaluatedPropertiesKeyword(otherSchema));
		return schema;
	}

	public static JsonSchema UniqueItems(this JsonSchema schema, bool unique)
	{
		schema.Add(new UniqueItemsKeyword(unique));
		return schema;
	}

	public static JsonSchema Vocabulary(this JsonSchema schema, string id, bool required)
	{
		VocabularyKeyword vocabularyKeyword = schema.OfType<VocabularyKeyword>().FirstOrDefault();
		if (vocabularyKeyword == null)
		{
			vocabularyKeyword = new VocabularyKeyword();
			schema.Add(vocabularyKeyword);
		}
		SchemaVocabulary key = SchemaKeywordCatalog.GetVocabulary(id) ?? new SchemaVocabulary(id);
		vocabularyKeyword[key] = required;
		return schema;
	}

	public static JsonSchema Vocabulary(this JsonSchema schema, SchemaVocabulary vocabulary, bool required)
	{
		VocabularyKeyword vocabularyKeyword = schema.OfType<VocabularyKeyword>().FirstOrDefault();
		if (vocabularyKeyword == null)
		{
			vocabularyKeyword = new VocabularyKeyword();
			schema.Add(vocabularyKeyword);
		}
		vocabularyKeyword[vocabulary] = required;
		return schema;
	}

	public static JsonSchema WriteOnly(this JsonSchema schema, bool isWriteOnly)
	{
		schema.Add(new WriteOnlyKeyword(isWriteOnly));
		return schema;
	}
}
