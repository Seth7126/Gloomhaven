using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public class PropertyDependency : IJsonSchemaDependency, IJsonSerializable, IEquatable<IJsonSchemaDependency>, IEquatable<PropertyDependency>
{
	private readonly IEnumerable<string> _dependencies;

	public static string ErrorTemplate { get; set; } = "Properties {{required}} are required when {{dependency}} is present.";

	public string PropertyName { get; }

	public JsonSchemaVersion SupportedVersions
	{
		get
		{
			if (!_dependencies.Any())
			{
				return JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07 | JsonSchemaVersion.Draft2019_09;
			}
			return JsonSchemaVersion.All;
		}
	}

	public PropertyDependency(string propertyName, IEnumerable<string> dependencies)
	{
		if (dependencies == null)
		{
			throw new ArgumentNullException("dependencies");
		}
		IList<string> dependencies2 = (dependencies as IList<string>) ?? dependencies.ToList();
		PropertyName = propertyName;
		_dependencies = dependencies2;
	}

	public PropertyDependency(string propertyName, string firstDependency, params string[] otherDependencies)
		: this(propertyName, new string[1] { firstDependency }.Concat<string>(otherDependencies))
	{
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(PropertyName, context)
		{
			Keyword = string.Format("{0}/{1}", context.Misc["dependencyParent"], PropertyName)
		};
		if (context.Instance.Type != JsonValueType.Object)
		{
			Log.Schema(() => "Instance not an object; not applicable");
			return schemaValidationResults;
		}
		if (!context.Instance.Object.ContainsKey(PropertyName))
		{
			Log.Schema(() => "Property " + PropertyName + " not found; not applicable");
			return schemaValidationResults;
		}
		List<string> missingProperties = _dependencies.Except<string>(context.Instance.Object.Keys).ToList();
		if (missingProperties.Any())
		{
			Log.Schema(() => $"Properties {missingProperties} not found but required by property {PropertyName}");
			schemaValidationResults.IsValid = false;
			schemaValidationResults.AdditionalInfo["required"] = missingProperties.ToJson();
			schemaValidationResults.AdditionalInfo["dependency"] = PropertyName;
			schemaValidationResults.ErrorMessage = ErrorTemplate.ResolveTokens(schemaValidationResults.AdditionalInfo);
		}
		return schemaValidationResults;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry locaRegistry)
	{
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		return null;
	}

	public bool Equals(PropertyDependency? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (string.Equals(PropertyName, other.PropertyName))
		{
			return _dependencies.ContentsEqual(other._dependencies);
		}
		return false;
	}

	public bool Equals(IJsonSchemaDependency? other)
	{
		return Equals(other as PropertyDependency);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as PropertyDependency);
	}

	public override int GetHashCode()
	{
		return (((_dependencies != null) ? _dependencies.GetCollectionHashCode() : 0) * 397) ^ ((PropertyName != null) ? PropertyName.GetHashCode() : 0);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		JsonValue jsonValue = _dependencies.ToJson();
		jsonValue.Array.EqualityStandard = ArrayEquality.ContentsEqual;
		return jsonValue;
	}
}
