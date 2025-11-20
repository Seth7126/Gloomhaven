using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Value}")]
public class TypeKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<TypeKeyword>
{
	public static string ErrorTemplate { get; set; } = "Values of type {{actual}} are not one of the allowed types {{allowed}}.";

	public string Name => "type";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 1;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Validation;

	public JsonSchemaType Value { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public TypeKeyword()
	{
	}

	public TypeKeyword(JsonSchemaType type)
	{
		Value = type;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		bool flag = true;
		Log.Schema(() => $"Validating that instance is of type {Value.ToJson()}");
		switch (context.Instance.Type)
		{
		case JsonValueType.Number:
			if (!Value.HasFlag(JsonSchemaType.Number) && (!context.Instance.Number.IsInt() || !Value.HasFlag(JsonSchemaType.Integer)))
			{
				flag = false;
			}
			break;
		case JsonValueType.String:
		{
			JsonValue jsonValue = Value.ToJson();
			if ((jsonValue.Type != JsonValueType.String || !(jsonValue == context.Instance)) && (jsonValue.Type != JsonValueType.Array || !jsonValue.Array.Contains(context.Instance)) && !Value.HasFlag(JsonSchemaType.String))
			{
				flag = false;
			}
			break;
		}
		case JsonValueType.Boolean:
			if (!Value.HasFlag(JsonSchemaType.Boolean))
			{
				flag = false;
			}
			break;
		case JsonValueType.Object:
			if (!Value.HasFlag(JsonSchemaType.Object))
			{
				flag = false;
			}
			break;
		case JsonValueType.Array:
			if (!Value.HasFlag(JsonSchemaType.Array))
			{
				flag = false;
			}
			break;
		case JsonValueType.Null:
			if (!Value.HasFlag(JsonSchemaType.Null))
			{
				flag = false;
			}
			break;
		}
		if (!flag)
		{
			Log.Schema(() => "Type check failed: found " + context.Instance.Type.ToString().ToLower());
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["allowed"] = Value.ToJson();
			schemaValidationResults.AdditionalInfo["actual"] = context.Instance.Type.ToString().ToLower();
			schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
		}
		return schemaValidationResults;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return null;
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		Value = json.ToSchemaType();
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Value.ToJson();
	}

	public bool Equals(TypeKeyword? other)
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
		return Equals(other as TypeKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as TypeKeyword);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
