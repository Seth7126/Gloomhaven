using System;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public interface IJsonSchemaKeyword : IJsonSerializable, IEquatable<IJsonSchemaKeyword>
{
	string Name { get; }

	JsonSchemaVersion SupportedVersions { get; }

	int ValidationSequence { get; }

	SchemaVocabulary Vocabulary { get; }

	SchemaValidationResults Validate(SchemaValidationContext context);

	void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry);

	JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri);
}
