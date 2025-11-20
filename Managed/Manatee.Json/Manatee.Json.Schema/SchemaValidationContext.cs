using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;

namespace Manatee.Json.Schema;

public class SchemaValidationContext
{
	private HashSet<string>? _evaluatedPropertyNames;

	private HashSet<string>? _locallyEvaluatedPropertyNames;

	private HashSet<int>? _validatedIndices;

	private HashSet<int>? _locallyValidatedIndices;

	public JsonSchema Local { get; set; }

	public JsonSchema Root { get; set; }

	public JsonSchema? RecursiveAnchor { get; set; }

	public JsonValue Instance { get; set; }

	public HashSet<string> EvaluatedPropertyNames => _evaluatedPropertyNames ?? (_evaluatedPropertyNames = new HashSet<string>());

	public HashSet<string> LocallyEvaluatedPropertyNames => _locallyEvaluatedPropertyNames ?? (_locallyEvaluatedPropertyNames = new HashSet<string>());

	public HashSet<int> ValidatedIndices => _validatedIndices ?? (_validatedIndices = new HashSet<int>());

	public HashSet<int> LocallyValidatedIndices => _locallyValidatedIndices ?? (_locallyValidatedIndices = new HashSet<int>());

	public int LastEvaluatedIndex { get; set; } = -1;

	public int LocalTierLastEvaluatedIndex { get; set; } = -1;

	public Uri? BaseUri { get; set; }

	public JsonPointer InstanceLocation { get; set; }

	public JsonPointer RelativeLocation { get; set; }

	public JsonPointer? BaseRelativeLocation { get; set; }

	public bool IsMetaSchemaValidation { get; set; }

	public Dictionary<string, object> Misc { get; } = new Dictionary<string, object>();

	internal JsonSchemaRegistry LocalRegistry { get; }

	internal SchemaValidationContext(JsonSchema root, JsonValue instance, JsonPointer? baseRelativeLocation, JsonPointer relativeLocation, JsonPointer instanceLocation)
	{
		Root = root;
		Instance = instance;
		BaseRelativeLocation = baseRelativeLocation;
		RelativeLocation = relativeLocation;
		InstanceLocation = instanceLocation;
		LocalRegistry = new JsonSchemaRegistry();
	}

	public SchemaValidationContext(SchemaValidationContext source)
		: this(source.Root, source.Instance, source.BaseRelativeLocation, source.RelativeLocation, source.InstanceLocation)
	{
		Local = source.Local;
		Root = source.Root;
		RecursiveAnchor = source.RecursiveAnchor;
		Instance = source.Instance;
		EvaluatedPropertyNames.UnionWith(source.EvaluatedPropertyNames);
		LocallyEvaluatedPropertyNames.UnionWith(source.LocallyEvaluatedPropertyNames);
		ValidatedIndices.UnionWith(source.ValidatedIndices);
		LocallyValidatedIndices.UnionWith(source.LocallyValidatedIndices);
		LastEvaluatedIndex = source.LastEvaluatedIndex;
		LocalTierLastEvaluatedIndex = source.LocalTierLastEvaluatedIndex;
		BaseUri = source.BaseUri;
		InstanceLocation = source.InstanceLocation;
		RelativeLocation = source.RelativeLocation;
		BaseRelativeLocation = source.BaseRelativeLocation;
		IsMetaSchemaValidation = source.IsMetaSchemaValidation;
		LocalRegistry = source.LocalRegistry;
	}

	public void UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(SchemaValidationContext other)
	{
		EvaluatedPropertyNames.UnionWith(other.EvaluatedPropertyNames);
		EvaluatedPropertyNames.UnionWith(other.LocallyEvaluatedPropertyNames);
		if (other.EvaluatedPropertyNames.Any())
		{
			Log.Schema(() => "Properties [" + EvaluatedPropertyNames.ToStringList() + "] have now been validated");
		}
		LastEvaluatedIndex = Math.Max(LastEvaluatedIndex, other.LastEvaluatedIndex);
		LastEvaluatedIndex = Math.Max(LastEvaluatedIndex, other.LocalTierLastEvaluatedIndex);
		ValidatedIndices.UnionWith(other.ValidatedIndices);
		ValidatedIndices.UnionWith(other.LocallyValidatedIndices);
		if (other.EvaluatedPropertyNames.Any())
		{
			Log.Schema(() => "Indices [" + EvaluatedPropertyNames.ToStringList() + "] have now been validated");
		}
	}
}
