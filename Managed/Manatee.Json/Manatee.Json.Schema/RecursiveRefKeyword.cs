using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

[DebuggerDisplay("Name={Name} Value={Reference}")]
public class RecursiveRefKeyword : IJsonSchemaKeyword, IJsonSerializable, IEquatable<IJsonSchemaKeyword>, IEquatable<RecursiveRefKeyword>
{
	private readonly List<JsonPointer> _validatingLocations = new List<JsonPointer>();

	private JsonSchema? _resolvedRoot;

	private JsonPointer? _resolvedFragment;

	public string Name => "$recursiveRef";

	public JsonSchemaVersion SupportedVersions { get; } = JsonSchemaVersion.Draft2019_09;

	public int ValidationSequence => 0;

	public SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

	public string Reference { get; private set; }

	public JsonSchema? Resolved { get; private set; }

	[DeserializationUseOnly]
	[UsedImplicitly]
	public RecursiveRefKeyword()
	{
	}

	public RecursiveRefKeyword(string reference)
	{
		Reference = reference;
	}

	public SchemaValidationResults Validate(SchemaValidationContext context)
	{
		if (_validatingLocations.Any((JsonPointer l) => object.Equals(l, context.InstanceLocation)))
		{
			return new SchemaValidationResults(Name, context)
			{
				RecursionDetected = true,
				AnnotationValue = "Detected recursive loop. Processing halted on this branch."
			};
		}
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
		schemaValidationContext.BaseUri = _resolvedRoot.DocumentPath;
		schemaValidationContext.Root = _resolvedRoot ?? context.Root;
		schemaValidationContext.BaseRelativeLocation = _resolvedFragment.WithHash();
		schemaValidationContext.RelativeLocation = context.RelativeLocation.CloneAndAppend(Name);
		SchemaValidationContext schemaValidationContext2 = schemaValidationContext;
		_validatingLocations.Add(context.InstanceLocation);
		SchemaValidationResults schemaValidationResults2 = Resolved.Validate(schemaValidationContext2);
		_validatingLocations.Remove(context.InstanceLocation);
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

	public bool Equals(RecursiveRefKeyword? other)
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
		return Equals(other as RecursiveRefKeyword);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as RecursiveRefKeyword);
	}

	public override int GetHashCode()
	{
		return Reference?.GetHashCode() ?? 0;
	}

	private void _ResolveReference(SchemaValidationContext context)
	{
		Log.Schema(() => "Resolving `" + Reference + "`");
		if (context.RecursiveAnchor != null)
		{
			Log.Schema(() => "Finding anchor of root schema");
			if (context.BaseUri == null)
			{
				throw new InvalidOperationException("BaseUri not set");
			}
			if (JsonSchemaRegistry.Get(context.BaseUri.OriginalString)?.Get<RecursiveAnchorKeyword>() != null)
			{
				_resolvedRoot = context.RecursiveAnchor;
			}
		}
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
		Uri uri = _resolvedRoot?.DocumentPath ?? context.BaseUri;
		string[] array = Reference.Split(new char[1] { '#' }, StringSplitOptions.None);
		string text = ((!string.IsNullOrWhiteSpace(array[0])) ? array[0] : uri?.OriginalString);
		_resolvedFragment = ((array.Length > 1) ? JsonPointer.Parse(array[1]) : new JsonPointer());
		if (_resolvedRoot == null)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				if (!Uri.TryCreate(text, UriKind.Absolute, out var result))
				{
					text = context.Local.Id + text;
				}
				if (uri != null && !Uri.TryCreate(text, UriKind.Absolute, out result))
				{
					text = new Uri(uri.OriginalString.EndsWith("/") ? uri : uri.GetParentUri(), text).OriginalString;
				}
				_resolvedRoot = JsonSchemaRegistry.Get(text);
			}
			else
			{
				_resolvedRoot = context.Root;
			}
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
