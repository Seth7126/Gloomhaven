using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Manatee.Json.Serialization;
using Manatee.Json.Serialization.Internal;

namespace Manatee.Json.Schema;

public static class SchemaKeywordCatalog
{
	private static readonly Dictionary<string, List<Type>> _cache;

	private static readonly ConstructorResolver _resolver;

	private static readonly Dictionary<string, SchemaVocabulary> _vocabularies;

	static SchemaKeywordCatalog()
	{
		_cache = new Dictionary<string, List<Type>>();
		_resolver = new ConstructorResolver();
		_vocabularies = new Dictionary<string, SchemaVocabulary>();
		IEnumerable<Type> enumerable = from ti in typeof(IJsonSchemaKeyword).GetTypeInfo().Assembly.DefinedTypes
			where typeof(IJsonSchemaKeyword).GetTypeInfo().IsAssignableFrom(ti) && !ti.IsAbstract
			select ti.AsType();
		MethodInfo methodInfo = typeof(SchemaKeywordCatalog).GetTypeInfo().DeclaredMethods.Single((MethodInfo m) => m.Name == "Add");
		foreach (Type item in enumerable)
		{
			methodInfo.MakeGenericMethod(item).Invoke(null, new object[0]);
		}
	}

	public static void Add<T>() where T : IJsonSchemaKeyword, new()
	{
		T val = (T)_resolver.Resolve(typeof(T), null);
		if (val == null)
		{
			throw new ArgumentException($"Cannot resolve instance of type {typeof(T)}", "T");
		}
		if (!_cache.TryGetValue(val.Name, out List<Type> value))
		{
			value = new List<Type>();
			_cache[val.Name] = value;
		}
		if (!value.Contains(typeof(T)))
		{
			value.Add(typeof(T));
		}
		if (!_vocabularies.TryGetValue(val.Vocabulary.Id, out SchemaVocabulary value2))
		{
			value2 = val.Vocabulary;
			_vocabularies[val.Vocabulary.Id] = value2;
		}
		if (!value2.DefinedKeywords.Contains(typeof(T)))
		{
			value2.DefinedKeywords.Add(typeof(T));
		}
	}

	public static void Remove<T>() where T : IJsonSchemaKeyword, new()
	{
		T val = (T)_resolver.Resolve(typeof(T), null);
		if (val == null || !_cache.TryGetValue(val.Name, out List<Type> value))
		{
			return;
		}
		value.Remove(typeof(T));
		if (_vocabularies.TryGetValue(val.Vocabulary.Id, out SchemaVocabulary value2))
		{
			value2.DefinedKeywords.Remove(typeof(T));
			if (!value2.DefinedKeywords.Any())
			{
				_vocabularies.Remove(value2.Id);
			}
		}
	}

	internal static IJsonSchemaKeyword? Build(string keywordName, JsonValue json, JsonSerializer serializer)
	{
		if (!_cache.TryGetValue(keywordName, out List<Type> value) || !value.Any())
		{
			return null;
		}
		IJsonSchemaKeyword jsonSchemaKeyword = null;
		List<IJsonSchemaKeywordPlus> source = (from t in value
			where t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IJsonSchemaKeywordPlus))
			select (IJsonSchemaKeywordPlus)_resolver.Resolve(t, null)).ToList();
		if (source.Any())
		{
			jsonSchemaKeyword = source.FirstOrDefault((IJsonSchemaKeywordPlus k) => k?.Handles(json) ?? false);
		}
		if (jsonSchemaKeyword == null)
		{
			jsonSchemaKeyword = (IJsonSchemaKeyword)_resolver.Resolve(value.First(), null);
		}
		jsonSchemaKeyword?.FromJson(json, serializer);
		return jsonSchemaKeyword;
	}

	internal static SchemaVocabulary? GetVocabulary(string id)
	{
		if (!_vocabularies.TryGetValue(id, out SchemaVocabulary value))
		{
			return null;
		}
		return value;
	}
}
