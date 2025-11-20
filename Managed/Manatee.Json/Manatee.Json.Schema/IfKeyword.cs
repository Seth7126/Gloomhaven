using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}")]
public class IfKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<IfKeyword>
{
	public string Name => "if";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Applicator;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public IfKeyword()
	{
	}

	public IfKeyword(JsonSchema value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		ThenKeyword thenKeyword = context.Local.Get<ThenKeyword>();
		ElseKeyword elseKeyword = context.Local.Get<ElseKeyword>();
		if (thenKeyword != null || elseKeyword != null)
		{
			SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
			schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
			schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name);
			SchemaValidationContext context2 = schemaValidationContext;
			SchemaValidationResults schemaValidationResults = Value.Validate(context2);
			context.Misc["ifKeywordValid"] = schemaValidationResults.IsValid;
		}
		else
		{
			Log.Schema(() => "`then` and `else` keywords not present; skipping `if` validation");
		}
		return new SchemaValidationResults(Name, context);
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		Value.RegisterSubschemas(baseUri, localRegistry);
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return null;
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		Value = serializer.Deserialize<JsonSchema>(json);
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return serializer.Serialize(Value);
	}

	public bool Equals(IfKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return object.Equals(Value, other.Value);
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as IfKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as IfKeyword);
	}

	public override int GetHashCode()
	{
		if (!(Value != null))
		{
			return 0;
		}
		return Value.GetHashCode();
	}
}
