using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Manatee.Json.Schema;

public static class GetterExtensions
{
	public static JsonSchema? AdditionalItems(this JsonSchema schema)
	{
		return schema.Get<AdditionalItemsKeyword>()?.Value;
	}

	public static JsonSchema? AdditionalProperties(this JsonSchema schema)
	{
		return schema.Get<AdditionalPropertiesKeyword>()?.Value;
	}

	public static List<JsonSchema>? AllOf(this JsonSchema schema)
	{
		return schema.Get<AllOfKeyword>();
	}

	public static List<JsonSchema>? AnyOf(this JsonSchema schema)
	{
		return schema.Get<AnyOfKeyword>();
	}

	public static string? Comment(this JsonSchema schema)
	{
		return schema.Get<CommentKeyword>()?.Value;
	}

	public static JsonValue? Const(this JsonSchema schema)
	{
		return schema.Get<ConstKeyword>()?.Value;
	}

	public static JsonSchema? Contains(this JsonSchema schema)
	{
		return schema.Get<ContainsKeyword>()?.Value;
	}

	public static string? ContentEncoding(this JsonSchema schema)
	{
		return schema.Get<ContentEncodingKeyword>()?.Value;
	}

	public static string? ContentMediaType(this JsonSchema schema)
	{
		return schema.Get<ContentMediaTypeKeyword>()?.Value;
	}

	public static JsonSchema? ContentSchema(this JsonSchema schema)
	{
		return schema.Get<ContentSchemaKeyword>()?.Value;
	}

	public static JsonValue? Default(this JsonSchema schema)
	{
		return schema.Get<DefaultKeyword>()?.Value;
	}

	public static Dictionary<string, JsonSchema>? Defs(this JsonSchema schema)
	{
		return schema.Get<DefsKeyword>();
	}

	public static Dictionary<string, JsonSchema>? Definitions(this JsonSchema schema)
	{
		return schema.Get<DefinitionsKeyword>();
	}

	public static string? Description(this JsonSchema schema)
	{
		return schema.Get<DescriptionKeyword>()?.Value;
	}

	public static JsonSchema? Else(this JsonSchema schema)
	{
		return schema.Get<ElseKeyword>()?.Value;
	}

	public static List<JsonValue>? Enum(this JsonSchema schema)
	{
		return schema.Get<EnumKeyword>();
	}

	public static List<JsonValue>? Examples(this JsonSchema schema)
	{
		return schema.Get<ExamplesKeyword>();
	}

	public static double? ExclusiveMaximum(this JsonSchema schema)
	{
		return schema.Get<ExclusiveMaximumKeyword>()?.Value;
	}

	public static bool ExclusiveMaximumDraft04(this JsonSchema schema)
	{
		return schema.Get<ExclusiveMaximumDraft04Keyword>()?.Value ?? false;
	}

	public static double? ExclusiveMinimum(this JsonSchema schema)
	{
		return schema.Get<ExclusiveMinimumKeyword>()?.Value;
	}

	public static bool ExclusiveMinimumDraft04(this JsonSchema schema)
	{
		return schema.Get<ExclusiveMinimumDraft04Keyword>()?.Value ?? false;
	}

	public static Format? Format(this JsonSchema schema)
	{
		return schema.Get<FormatKeyword>()?.Value;
	}

	public static string? Id(this JsonSchema schema)
	{
		object obj = schema.Get<IdKeyword>()?.Value;
		if (obj == null)
		{
			IdKeywordDraft04 idKeywordDraft = schema.Get<IdKeywordDraft04>();
			if (idKeywordDraft == null)
			{
				return null;
			}
			obj = idKeywordDraft.Value;
		}
		return (string?)obj;
	}

	public static List<JsonSchema>? Items(this JsonSchema schema)
	{
		return schema.Get<ItemsKeyword>();
	}

	public static double? MaxContains(this JsonSchema schema)
	{
		return schema.Get<MaxContainsKeyword>()?.Value;
	}

	public static double? Maximum(this JsonSchema schema)
	{
		return schema.Get<MaximumKeyword>()?.Value;
	}

	public static double? MaxItems(this JsonSchema schema)
	{
		return schema.Get<MaxItemsKeyword>()?.Value;
	}

	public static double? MaxLength(this JsonSchema schema)
	{
		return schema.Get<MaxLengthKeyword>()?.Value;
	}

	public static double? MaxProperties(this JsonSchema schema)
	{
		return schema.Get<MaxPropertiesKeyword>()?.Value;
	}

	public static double? MinContains(this JsonSchema schema)
	{
		return schema.Get<MinContainsKeyword>()?.Value;
	}

	public static double? Minimum(this JsonSchema schema)
	{
		return schema.Get<MinimumKeyword>()?.Value;
	}

	public static double? MinItems(this JsonSchema schema)
	{
		return schema.Get<MinItemsKeyword>()?.Value;
	}

	public static double? MinLength(this JsonSchema schema)
	{
		return schema.Get<MinLengthKeyword>()?.Value;
	}

	public static double? MinProperties(this JsonSchema schema)
	{
		return schema.Get<MinPropertiesKeyword>()?.Value;
	}

	public static double? MultipleOf(this JsonSchema schema)
	{
		return schema.Get<MultipleOfKeyword>()?.Value;
	}

	public static JsonSchema? Not(this JsonSchema schema)
	{
		return schema.Get<NotKeyword>()?.Value;
	}

	public static List<JsonSchema>? OneOf(this JsonSchema schema)
	{
		return schema.Get<OneOfKeyword>();
	}

	public static Regex? Pattern(this JsonSchema schema)
	{
		return schema.Get<PatternKeyword>()?.Value;
	}

	public static Dictionary<string, JsonSchema>? PatternProperties(this JsonSchema schema)
	{
		return schema.Get<PatternPropertiesKeyword>();
	}

	public static Dictionary<string, JsonSchema>? Properties(this JsonSchema schema)
	{
		return schema.Get<PropertiesKeyword>();
	}

	public static JsonSchema? PropertyNames(this JsonSchema schema)
	{
		return schema.Get<PropertyNamesKeyword>()?.Value;
	}

	public static bool ReadOnly(this JsonSchema schema)
	{
		return schema.Get<ReadOnlyKeyword>()?.Value ?? false;
	}

	public static bool RecursiveAnchor(this JsonSchema schema)
	{
		return schema.Get<RecursiveAnchorKeyword>()?.Value ?? false;
	}

	public static string? RecursiveRef(this JsonSchema schema)
	{
		return schema.Get<RecursiveRefKeyword>()?.Reference;
	}

	public static string? Ref(this JsonSchema schema)
	{
		return schema.Get<RefKeyword>()?.Reference;
	}

	public static JsonSchema? RefResolved(this JsonSchema schema)
	{
		return schema.Get<RefKeyword>()?.Resolved;
	}

	public static List<string>? Required(this JsonSchema schema)
	{
		return schema.Get<RequiredKeyword>();
	}

	public static string? Schema(this JsonSchema schema)
	{
		return schema.Get<SchemaKeyword>()?.Value;
	}

	public static JsonSchema? Then(this JsonSchema schema)
	{
		return schema.Get<ThenKeyword>()?.Value;
	}

	public static string? Title(this JsonSchema schema)
	{
		return schema.Get<TitleKeyword>()?.Value;
	}

	public static JsonSchema? UnevaluatedItems(this JsonSchema schema)
	{
		return schema.Get<UnevaluatedItemsKeyword>()?.Value;
	}

	public static JsonSchema? UnevaluatedProperties(this JsonSchema schema)
	{
		return schema.Get<UnevaluatedPropertiesKeyword>()?.Value;
	}

	public static bool UniqueItems(this JsonSchema schema)
	{
		return schema.Get<UniqueItemsKeyword>()?.Value ?? false;
	}

	public static Dictionary<SchemaVocabulary, bool>? Vocabulary(this JsonSchema schema)
	{
		return schema.Get<VocabularyKeyword>();
	}

	public static bool WriteOnly(this JsonSchema schema)
	{
		return schema.Get<WriteOnlyKeyword>()?.Value ?? false;
	}

	[return: MaybeNull]
	public static T Get<T>(this JsonSchema schema) where T : IJsonSchemaKeyword?
	{
		return schema.OfType<T>().FirstOrDefault();
	}
}
