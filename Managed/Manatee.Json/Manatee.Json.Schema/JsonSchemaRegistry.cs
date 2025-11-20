using System;
using System.Collections.Concurrent;
using Manatee.Json.Internal;
using Manatee.Json.Patch;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema;

public class JsonSchemaRegistry
{
	private static readonly ConcurrentDictionary<string, JsonSchema> _schemaLookup;

	private static readonly JsonSerializer _serializer;

	private readonly ConcurrentDictionary<string, JsonSchema> _contextLookup;

	static JsonSchemaRegistry()
	{
		_schemaLookup = new ConcurrentDictionary<string, JsonSchema>();
		_serializer = new JsonSerializer();
		Clear();
	}

	internal JsonSchemaRegistry()
	{
		_contextLookup = new ConcurrentDictionary<string, JsonSchema>();
	}

	public static JsonSchema? Get(string uri)
	{
		JsonSchema value;
		lock (_schemaLookup)
		{
			uri = uri.TrimEnd(new char[1] { '#' });
			if (!_schemaLookup.TryGetValue(uri, out value))
			{
				string text = JsonSchemaOptions.Download(uri);
				if (text == null)
				{
					return null;
				}
				JsonValue json = JsonValue.Parse(text);
				value = new JsonSchema
				{
					DocumentPath = new Uri(uri, UriKind.RelativeOrAbsolute)
				};
				value.FromJson(json, _serializer);
				MetaSchemaValidationResults metaSchemaValidationResults = value.ValidateSchema();
				if (!metaSchemaValidationResults.IsValid)
				{
					throw new SchemaLoadException("The given path does not contain a valid schema.", metaSchemaValidationResults);
				}
				_schemaLookup[uri] = value;
			}
		}
		return value;
	}

	internal static JsonSchema? GetWellKnown(string uri)
	{
		lock (_schemaLookup)
		{
			uri = uri.TrimEnd(new char[1] { '#' });
			_schemaLookup.TryGetValue(uri, out JsonSchema value);
			return value;
		}
	}

	internal JsonSchema? GetLocal(string uri)
	{
		lock (_contextLookup)
		{
			uri = uri.TrimEnd(new char[1] { '#' });
			_contextLookup.TryGetValue(uri, out JsonSchema value);
			return value;
		}
	}

	public static void Register(JsonSchema schema)
	{
		if (schema.DocumentPath == null)
		{
			return;
		}
		Log.Schema(() => "Registering \"" + schema.DocumentPath.OriginalString + "\"");
		lock (_schemaLookup)
		{
			_schemaLookup[schema.DocumentPath.OriginalString] = schema;
		}
	}

	internal void RegisterLocal(JsonSchema schema)
	{
		if (schema.Id != null && schema.Id.IsLocalSchemaId())
		{
			Log.Schema(() => "Registering \"" + schema.Id + "\"");
			lock (_contextLookup)
			{
				_contextLookup[schema.Id] = schema;
			}
		}
		AnchorKeyword anchorKeyword = schema.Get<AnchorKeyword>();
		if (anchorKeyword != null)
		{
			string anchorUri = $"{schema.DocumentPath}#{anchorKeyword.Value}";
			Log.Schema(() => "Registering \"" + anchorUri + "\"");
			lock (_contextLookup)
			{
				_contextLookup[anchorUri] = schema;
			}
		}
	}

	public static void Unregister(JsonSchema schema)
	{
		if (schema.DocumentPath == null)
		{
			return;
		}
		lock (_schemaLookup)
		{
			_schemaLookup.TryRemove(schema.DocumentPath.OriginalString, out JsonSchema _);
		}
	}

	public static void Unregister(string uri)
	{
		if (string.IsNullOrWhiteSpace(uri))
		{
			return;
		}
		lock (_schemaLookup)
		{
			_schemaLookup.TryRemove(uri, out JsonSchema _);
		}
	}

	public static void Clear()
	{
		string key = MetaSchemas.Draft04.Id.Split(new char[1] { '#' })[0];
		string key2 = MetaSchemas.Draft06.Id.Split(new char[1] { '#' })[0];
		string key3 = MetaSchemas.Draft07.Id.Split(new char[1] { '#' })[0];
		string key4 = MetaSchemas.Draft2019_09.Id.Split(new char[1] { '#' })[0];
		string key5 = JsonPatch.Schema.Id.Split(new char[1] { '#' })[0];
		lock (_schemaLookup)
		{
			_schemaLookup.Clear();
			_schemaLookup[key] = MetaSchemas.Draft04;
			_schemaLookup[key2] = MetaSchemas.Draft06;
			_schemaLookup[key3] = MetaSchemas.Draft07;
			_schemaLookup[key4] = MetaSchemas.Draft2019_09;
			_schemaLookup[MetaSchemas.Draft2019_09_Core.Id] = MetaSchemas.Draft2019_09_Core;
			_schemaLookup[MetaSchemas.Draft2019_09_MetaData.Id] = MetaSchemas.Draft2019_09_MetaData;
			_schemaLookup[MetaSchemas.Draft2019_09_Applicator.Id] = MetaSchemas.Draft2019_09_Applicator;
			_schemaLookup[MetaSchemas.Draft2019_09_Validation.Id] = MetaSchemas.Draft2019_09_Validation;
			_schemaLookup[MetaSchemas.Draft2019_09_Format.Id] = MetaSchemas.Draft2019_09_Format;
			_schemaLookup[MetaSchemas.Draft2019_09_Content.Id] = MetaSchemas.Draft2019_09_Content;
			_schemaLookup[key5] = JsonPatch.Schema;
		}
	}
}
