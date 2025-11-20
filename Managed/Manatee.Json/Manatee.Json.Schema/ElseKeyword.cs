using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name}")]
public class ElseKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<ElseKeyword>
{
	public static string ErrorTemplate { get; set; } = "Validation of `if` failed, but validation of `else` also failed";

	public string Name => "else";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Applicator;

	public JsonSchema Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public ElseKeyword()
	{
	}

	public ElseKeyword(JsonSchema value)
	{
		Value = value;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (!context.Misc.TryGetValue("ifKeywordValid", out object value))
		{
			Log.Schema(() => "`if` keyword not present; not applicable");
			return new SchemaValidationResults(Name, context);
		}
		if ((bool)value)
		{
			Log.Schema(() => "`if` subschema succeeded; not applicable");
			return new SchemaValidationResults(Name, context);
		}
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
		schemaValidationContext.BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name);
		schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name);
		SchemaValidationContext context2 = schemaValidationContext;
		SchemaValidationResults schemaValidationResults2 = Value.Validate(context2);
		if (!schemaValidationResults2.IsValid)
		{
			Log.Schema(() => "`if` subschema failed, but `else` subschema also failed");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.Keyword = Name;
			schemaValidationResults.ErrorMessage = ErrorTemplate;
			if (JsonSchemaOptions.ShouldReportChildErrors(this, context))
			{
				schemaValidationResults.NestedResults.Add(schemaValidationResults2);
			}
		}
		return schemaValidationResults;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		Value.RegisterSubschemas(baseUri, localRegistry);
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return Value.ResolveSubschema(pointer, baseUri);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		Value = serializer.Deserialize<JsonSchema>(json);
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return serializer.Serialize(Value);
	}

	public bool Equals(ElseKeyword? other)
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
		return Equals(other as ElseKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ElseKeyword);
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
