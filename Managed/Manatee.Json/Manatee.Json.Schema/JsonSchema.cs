using System;
using System.Collections.Generic;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public class JsonSchema : List<IJsonSchemaKeyword>, IJsonSerializable, IEquatable<JsonSchema>
{
	public static readonly JsonSchema Empty = new JsonSchema();

	public static readonly JsonSchema True = new JsonSchema(value: true);

	public static readonly JsonSchema False = new JsonSchema(value: false);

	private bool? _inherentValue;

	private Uri? _documentPath;

	private bool _hasRegistered;

	private MetaSchemaValidationResults? _metaSchemaResults;

	public static string ErrorTemplate { get; set; } = "No value is valid against the false schema.";

	public Uri? DocumentPath
	{
		get
		{
			return _documentPath ?? (_documentPath = _BuildDocumentPath());
		}
		set
		{
			_documentPath = value;
		}
	}

	public string? Id => this.Get<IdKeyword>()?.Value;

	public string? Schema => this.Get<SchemaKeyword>()?.Value;

	public JsonSchemaVersion SupportedVersions
	{
		get
		{
			if (_metaSchemaResults == null)
			{
				_metaSchemaResults = ValidateSchema();
			}
			return _metaSchemaResults.SupportedVersions;
		}
		internal set
		{
			_metaSchemaResults = new MetaSchemaValidationResults
			{
				SupportedVersions = value
			};
		}
	}

	public JsonObject OtherData { get; set; } = new JsonObject();

	public JsonSchema()
	{
	}

	public JsonSchema(bool value)
	{
		_inherentValue = value;
	}

	public MetaSchemaValidationResults ValidateSchema()
	{
		if (_metaSchemaResults != null)
		{
			return _metaSchemaResults;
		}
		MetaSchemaValidationResults metaSchemaValidationResults = new MetaSchemaValidationResults();
		JsonSchemaVersion seed;
		if (Schema == MetaSchemas.Draft04.Id)
		{
			seed = JsonSchemaVersion.Draft04;
		}
		else if (Schema == MetaSchemas.Draft06.Id)
		{
			seed = JsonSchemaVersion.Draft06;
		}
		else if (Schema == MetaSchemas.Draft07.Id)
		{
			seed = JsonSchemaVersion.Draft07;
		}
		else if (Schema == MetaSchemas.Draft2019_09.Id)
		{
			seed = JsonSchemaVersion.Draft2019_09;
		}
		else
		{
			seed = JsonSchemaVersion.All;
			string schema = Schema;
			if (!string.IsNullOrEmpty(schema))
			{
				JsonSchema jsonSchema = JsonSchemaRegistry.Get(schema);
				if (jsonSchema != null)
				{
					JsonValue instance = ToJson(new JsonSerializer());
					SchemaValidationContext context = new SchemaValidationContext(jsonSchema, instance, new JsonPointer("#"), new JsonPointer("#"), new JsonPointer("#"))
					{
						IsMetaSchemaValidation = true
					};
					SchemaValidationResults value = jsonSchema.Validate(context);
					metaSchemaValidationResults.MetaSchemaValidations[schema] = value;
				}
			}
		}
		JsonSchemaVersion jsonSchemaVersion = this.Aggregate(seed, (JsonSchemaVersion version, IJsonSchemaKeyword keyword) => version & keyword.SupportedVersions);
		if (jsonSchemaVersion == JsonSchemaVersion.None)
		{
			metaSchemaValidationResults.OtherErrors.Add("The provided keywords do not support a common schema version.");
		}
		else
		{
			JsonValue instance2 = ToJson(new JsonSerializer());
			SchemaValidationContext schemaValidationContext = new SchemaValidationContext(this, instance2, new JsonPointer("#"), new JsonPointer("#"), new JsonPointer("#"))
			{
				IsMetaSchemaValidation = true
			};
			if (jsonSchemaVersion.HasFlag(JsonSchemaVersion.Draft04))
			{
				schemaValidationContext.Root = MetaSchemas.Draft04;
				SchemaValidationResults schemaValidationResults = MetaSchemas.Draft04.Validate(schemaValidationContext);
				metaSchemaValidationResults.MetaSchemaValidations[MetaSchemas.Draft04.Id] = schemaValidationResults;
				if (schemaValidationResults.IsValid)
				{
					metaSchemaValidationResults.SupportedVersions |= JsonSchemaVersion.Draft04;
				}
			}
			if (jsonSchemaVersion.HasFlag(JsonSchemaVersion.Draft06))
			{
				schemaValidationContext.Root = MetaSchemas.Draft06;
				SchemaValidationResults schemaValidationResults2 = MetaSchemas.Draft06.Validate(schemaValidationContext);
				metaSchemaValidationResults.MetaSchemaValidations[MetaSchemas.Draft06.Id] = schemaValidationResults2;
				if (schemaValidationResults2.IsValid)
				{
					metaSchemaValidationResults.SupportedVersions |= JsonSchemaVersion.Draft06;
				}
			}
			if (jsonSchemaVersion.HasFlag(JsonSchemaVersion.Draft07))
			{
				schemaValidationContext.Root = MetaSchemas.Draft07;
				SchemaValidationResults schemaValidationResults3 = MetaSchemas.Draft07.Validate(schemaValidationContext);
				metaSchemaValidationResults.MetaSchemaValidations[MetaSchemas.Draft07.Id] = schemaValidationResults3;
				if (schemaValidationResults3.IsValid)
				{
					metaSchemaValidationResults.SupportedVersions |= JsonSchemaVersion.Draft07;
				}
			}
			if (jsonSchemaVersion.HasFlag(JsonSchemaVersion.Draft2019_09))
			{
				schemaValidationContext.Root = MetaSchemas.Draft2019_09;
				SchemaValidationResults schemaValidationResults4 = MetaSchemas.Draft2019_09.Validate(schemaValidationContext);
				metaSchemaValidationResults.MetaSchemaValidations[MetaSchemas.Draft2019_09.Id] = schemaValidationResults4;
				if (schemaValidationResults4.IsValid)
				{
					metaSchemaValidationResults.SupportedVersions |= JsonSchemaVersion.Draft2019_09;
				}
			}
		}
		metaSchemaValidationResults.SupportedVersions = jsonSchemaVersion;
		List<string> list = (from k in this
			group k by k.Name into g
			where g.Count() > 1
			select g.Key).ToList();
		if (list.Any())
		{
			metaSchemaValidationResults.OtherErrors.Add("The following keywords have been entered more than once: " + string.Join(", ", list));
		}
		_metaSchemaResults = metaSchemaValidationResults;
		return metaSchemaValidationResults;
	}

	public SchemaValidationResults Validate(JsonValue json)
	{
		SchemaValidationResults schemaValidationResults = Validate(new SchemaValidationContext(this, json, new JsonPointer("#"), new JsonPointer("#"), new JsonPointer("#")));
		switch (JsonSchemaOptions.OutputFormat)
		{
		case SchemaValidationOutputFormat.Flag:
			schemaValidationResults.AdditionalInfo = new JsonObject();
			schemaValidationResults.RelativeLocation = null;
			schemaValidationResults.AbsoluteLocation = null;
			schemaValidationResults.InstanceLocation = null;
			schemaValidationResults.NestedResults = new List<SchemaValidationResults>();
			break;
		case SchemaValidationOutputFormat.Basic:
			schemaValidationResults = schemaValidationResults.Flatten();
			break;
		case SchemaValidationOutputFormat.Detailed:
			schemaValidationResults = schemaValidationResults.Condense();
			break;
		}
		return schemaValidationResults;
	}

	public void RegisterSubschemas(Uri? baseUri, JsonSchemaRegistry localRegistry)
	{
		if (_hasRegistered)
		{
			return;
		}
		_hasRegistered = true;
		localRegistry.RegisterLocal(this);
		string text = Id;
		if (baseUri != null && text != null)
		{
			text = new Uri(baseUri, text).OriginalString;
		}
		if (_documentPath == null && text != null && !text.StartsWith("#"))
		{
			if (!Uri.TryCreate(text, UriKind.Absolute, out var result))
			{
				result = new Uri(JsonSchemaOptions.DefaultBaseUri, text);
			}
			DocumentPath = result;
			JsonSchemaRegistry.Register(this);
			baseUri = result;
		}
		using List<IJsonSchemaKeyword>.Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.RegisterSubschemas(baseUri, localRegistry);
		}
	}

	public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri)
	{
		string first = pointer.FirstOrDefault();
		if (first == null)
		{
			DocumentPath = ((DocumentPath != null) ? new Uri(baseUri, DocumentPath) : baseUri);
			return this;
		}
		if (Uri.TryCreate(Id, UriKind.Absolute, out var result))
		{
			baseUri = result;
		}
		else if (Uri.TryCreate(Id, UriKind.Relative, out result))
		{
			baseUri = new Uri(baseUri, result);
		}
		JsonSchema jsonSchema = this.FirstOrDefault((IJsonSchemaKeyword k) => k.Name == first)?.ResolveSubschema(new JsonPointer(pointer.Skip(1)), baseUri);
		if (jsonSchema != null)
		{
			return jsonSchema;
		}
		PointerEvaluationResults pointerEvaluationResults = pointer.Evaluate(OtherData);
		if (pointerEvaluationResults.Result == null)
		{
			return null;
		}
		return new JsonSerializer().Deserialize<JsonSchema>(pointerEvaluationResults.Result);
	}

	internal SchemaValidationResults Validate(SchemaValidationContext context)
	{
		Log.Schema(() => $"Begin validation of {context.InstanceLocation} by {context.RelativeLocation}");
		if (_inherentValue.HasValue)
		{
			if (_inherentValue.Value)
			{
				Log.Schema(() => "`true` schema; all instances valid");
				return new SchemaValidationResults(context);
			}
			Log.Schema(() => "`false` schema; all instances invalid");
			return new SchemaValidationResults(context)
			{
				IsValid = false,
				ErrorMessage = ErrorTemplate
			};
		}
		RegisterSubschemas(null, context.LocalRegistry);
		context.LocalRegistry.RegisterLocal(this);
		context.Local = this;
		RefKeyword refKeyword = this.Get<RefKeyword>();
		if (refKeyword == null || JsonSchemaOptions.RefResolution == RefResolutionStrategy.ProcessSiblingId || context.Root.SupportedVersions == JsonSchemaVersion.Draft2019_09)
		{
			if (context.BaseUri == null)
			{
				context.BaseUri = DocumentPath;
			}
			else if (DocumentPath != null)
			{
				if (DocumentPath.IsAbsoluteUri)
				{
					context.BaseUri = DocumentPath;
				}
				else
				{
					context.BaseUri = new Uri(context.BaseUri, DocumentPath);
				}
			}
		}
		if (context.BaseUri != null && context.BaseUri.OriginalString.EndsWith("#"))
		{
			context.BaseUri = new Uri(context.BaseUri.OriginalString.TrimEnd(new char[1] { '#' }), UriKind.RelativeOrAbsolute);
		}
		List<SchemaValidationResults> list = new List<SchemaValidationResults>();
		if (refKeyword != null && !context.Root.SupportedVersions.HasFlag(JsonSchemaVersion.Draft2019_09))
		{
			return refKeyword.Validate(context);
		}
		list.AddRange(this.OrderBy((IJsonSchemaKeyword k) => k.ValidationSequence).Select(delegate(IJsonSchemaKeyword k)
		{
			Log.Schema(() => "Processing `" + k.Name + "`");
			SchemaValidationResults localResults = k.Validate(context);
			Log.Schema(() => "`" + k.Name + "` complete: " + (localResults.IsValid ? "valid" : "invalid"));
			return localResults;
		}));
		SchemaValidationResults results = new SchemaValidationResults(context);
		if (list.Any((SchemaValidationResults r) => !r.IsValid))
		{
			results.IsValid = false;
		}
		results.NestedResults = list;
		Log.Schema(() => string.Format("Validation of {0} by {1} complete: {2}", context.InstanceLocation, context.RelativeLocation, results.IsValid ? "valid" : "invalid"));
		return results;
	}

	private Uri? _BuildDocumentPath()
	{
		if (Id == null)
		{
			return null;
		}
		return new Uri(Id, UriKind.RelativeOrAbsolute);
	}

	public void FromJson(JsonValue json, JsonSerializer serializer)
	{
		if (json.Type == JsonValueType.Boolean)
		{
			_inherentValue = json.Boolean;
			return;
		}
		List<IJsonSchemaKeyword> list = new List<IJsonSchemaKeyword>();
		foreach (KeyValuePair<string, JsonValue> item in json.Object)
		{
			IJsonSchemaKeyword jsonSchemaKeyword = SchemaKeywordCatalog.Build(item.Key, item.Value, serializer);
			if (jsonSchemaKeyword != null)
			{
				list.Add(jsonSchemaKeyword);
			}
			else
			{
				OtherData[item.Key] = item.Value;
			}
		}
		AddRange(list);
	}

	public JsonValue ToJson(JsonSerializer serializer)
	{
		if (_inherentValue.HasValue)
		{
			bool? inherentValue = _inherentValue;
			if (!inherentValue.HasValue)
			{
				return null;
			}
			return inherentValue == true;
		}
		JsonObject jsonObject = this.Select((IJsonSchemaKeyword k) => new KeyValuePair<string, JsonValue>(k.Name, k.ToJson(serializer))).ToJson();
		if (OtherData != null)
		{
			foreach (KeyValuePair<string, JsonValue> otherDatum in OtherData)
			{
				jsonObject[otherDatum.Key] = otherDatum.Value;
			}
		}
		return jsonObject;
	}

	public static implicit operator JsonSchema(bool value)
	{
		if (!value)
		{
			return False;
		}
		return True;
	}

	public bool Equals(JsonSchema? other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		var source = this.FullOuterJoin(other, (IJsonSchemaKeyword tk) => tk.Name, (IJsonSchemaKeyword ok) => ok.Name, (IJsonSchemaKeyword tk, IJsonSchemaKeyword ok) => new
		{
			ThisKeyword = tk,
			OtherKeyword = ok
		}).ToList();
		if (_inherentValue == other._inherentValue && object.Equals(OtherData, other.OtherData))
		{
			return source.All(k => object.Equals(k.ThisKeyword, k.OtherKeyword));
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as JsonSchema);
	}

	public override int GetHashCode()
	{
		return this.GetCollectionHashCode();
	}

	public static bool operator ==(JsonSchema? left, JsonSchema? right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(JsonSchema? left, JsonSchema? right)
	{
		return !object.Equals(left, right);
	}
}
