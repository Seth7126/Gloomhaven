namespace Manatee.Json.Schema;

public static class SchemaVocabularies
{
	public static readonly SchemaVocabulary None = new SchemaVocabulary("none");

	public static readonly SchemaVocabulary Core = new SchemaVocabulary("https://json-schema.org/draft/2019-09/vocab/core", "https://json-schema.org/draft/2019-09/meta/core");

	public static readonly SchemaVocabulary Applicator = new SchemaVocabulary("https://json-schema.org/draft/2019-09/vocab/applicator", "https://json-schema.org/draft/2019-09/meta/applicator");

	public static readonly SchemaVocabulary MetaData = new SchemaVocabulary("https://json-schema.org/draft/2019-09/vocab/meta-data", "https://json-schema.org/draft/2019-09/meta/meta-data");

	public static readonly SchemaVocabulary Validation = new SchemaVocabulary("https://json-schema.org/draft/2019-09/vocab/validation", "https://json-schema.org/draft/2019-09/meta/validation");

	public static readonly SchemaVocabulary Format = new SchemaVocabulary("https://json-schema.org/draft/2019-09/vocab/format", "https://json-schema.org/draft/2019-09/meta/format");

	public static readonly SchemaVocabulary Content = new SchemaVocabulary("https://json-schema.org/draft/2019-09/vocab/content", "https://json-schema.org/draft/2019-09/meta/content");
}
