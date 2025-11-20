using System;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public interface IJsonSchemaDependency : IJsonSerializable, IEquatable<IJsonSchemaDependency>
{
	string PropertyName { get; }

	JsonSchemaVersion SupportedVersions { get; }

	SchemaValidationResults Validate(SchemaValidationContext context);

	void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry);

	JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri);
}
