using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Reference}")]
public class RefKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<RefKeyword>
{
	private JsonSchema? _resolvedRoot;

	private JsonPointer? _resolvedFragment;

	public string Name => "$ref";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.All;

	public int ValidationSequence => 0;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

	public string Reference { get; private set; }

	public JsonSchema? Resolved { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public RefKeyword()
	{
	}

	public RefKeyword(string reference)
	{
		Reference = reference;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (Resolved == null)
		{
			_ResolveReference(context);
			if (Resolved == null)
			{
				throw new SchemaReferenceNotFoundException(context.RelativeLocation);
			}
			Log.Schema(() => "Reference found");
		}
		SchemaValidationResults schemaValidationResults = new SchemaValidationResults(Name, context);
		SchemaValidationContext schemaValidationContext = new SchemaValidationContext(context);
		schemaValidationContext.BaseUri = _resolvedRoot?.DocumentPath;
		schemaValidationContext.Instance = context.Instance;
		schemaValidationContext.Root = _resolvedRoot ?? context.Root;
		schemaValidationContext.BaseRelativeLocation = _resolvedFragment?.WithHash();
		schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name);
		SchemaValidationContext schemaValidationContext2 = schemaValidationContext;
		SchemaValidationResults schemaValidationResults2 = Resolved.Validate(schemaValidationContext2);
		schemaValidationResults.IsValid = schemaValidationResults2.IsValid;
		schemaValidationResults.NestedResults.Add(schemaValidationResults2);
		context.UpdateEvaluatedPropertiesAndItemsFromSubschemaValidation(schemaValidationContext2);
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
		Reference = json.String;
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		return Reference;
	}

	public bool Equals(RefKeyword? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return string.Equals(Reference, other.Reference);
	}

	public bool Equals(IJsonSchemaKeyword? other)
	{
		return Equals(other as RefKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as RefKeyword);
	}

	public override int GetHashCode()
	{
		return Reference?.GetHashCode() ?? 0;
	}

	private void _ResolveReference(SchemaValidationContext context)
	{
		Log.Schema(() => "Resolving `" + Reference + "`");
		if (Reference.IsLocalSchemaId())
		{
			Log.Schema(() => "Reference recognized as anchor or local ID");
			Resolved = context.LocalRegistry.GetLocal(Reference);
			if (Resolved != null)
			{
				return;
			}
			Log.Schema(() => "`" + Reference + "` is an unknown anchor");
		}
		Uri baseUri = context.BaseUri;
		string[] array = Reference.Split(new char[1] { '#' }, StringSplitOptions.None);
		string text = ((!string.IsNullOrWhiteSpace(array[0])) ? array[0] : (((object)baseUri != null) ? baseUri.OriginalString.Split(new char[1] { '#' })[0] : null));
		_resolvedFragment = ((array.Length > 1) ? JsonPointer.Parse(array[1]) : new JsonPointer());
		if (!string.IsNullOrWhiteSpace(text))
		{
			if (!Uri.TryCreate(text, UriKind.Absolute, out var result) && (JsonSchemaOptions.RefResolution == RefResolutionStrategy.ProcessSiblingId || context.Root.SupportedVersions == JsonSchemaVersion.Draft2019_09))
			{
				text = context.Local.Id + text;
			}
			if (baseUri != null && !Uri.TryCreate(text, UriKind.Absolute, out result))
			{
				result = new Uri(baseUri.OriginalString.EndsWith("/") ? baseUri : baseUri.GetParentUri(), text);
				text = result.OriginalString;
			}
			_resolvedRoot = JsonSchemaRegistry.Get(text);
		}
		else
		{
			_resolvedRoot = context.Root;
		}
		if (_resolvedRoot == null)
		{
			Log.Schema(() => "Could not resolve root of reference");
			return;
		}
		JsonSchema wellKnown = JsonSchemaRegistry.GetWellKnown(Reference);
		if (wellKnown != null)
		{
			Log.Schema(() => "Well known reference found");
			Resolved = wellKnown;
		}
		else
		{
			_ResolveLocalReference(_resolvedRoot?.DocumentPath ?? context.BaseUri);
		}
	}

	private void _ResolveLocalReference(Uri baseUri)
	{
		if (!_resolvedFragment.Any())
		{
			Resolved = _resolvedRoot;
			return;
		}
		Log.Schema(() => $"Resolving local reference {_resolvedFragment}");
		Resolved = _resolvedRoot.ResolveSubschema(_resolvedFragment, baseUri);
	}
}
