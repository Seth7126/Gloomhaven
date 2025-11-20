using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Manatee.Json.Internal;
using Manatee.Json.Serialization.Internal;

namespace Manatee.Json.Serialization;

public class AbstractionMap
{
	private readonly Dictionary<Type, Type> _registry;

	public static AbstractionMap Default { get; }

	static AbstractionMap()
	{
		Default = new AbstractionMap();
		Default.MapGeneric(typeof(IEnumerable<>), typeof(List<>));
		Default.MapGeneric(typeof(ICollection<>), typeof(List<>));
		Default.MapGeneric(typeof(IList<>), typeof(List<>));
		Default.MapGeneric(typeof(IDictionary<, >), typeof(Dictionary<, >));
	}

	public AbstractionMap()
	{
		_registry = new Dictionary<Type, Type>();
	}

	public AbstractionMap(AbstractionMap other)
	{
		_registry = other._registry.ToDictionary<KeyValuePair<Type, Type>, Type, Type>((KeyValuePair<Type, Type> kvp) => kvp.Key, (KeyValuePair<Type, Type> kvp) => kvp.Value);
	}

	public void Map<TAbstract, TConcrete>(MapBaseAbstractionBehavior mappingBehavior = MapBaseAbstractionBehavior.Unmapped) where TConcrete : TAbstract, new()
	{
		if (typeof(TConcrete).GetTypeInfo().IsAbstract || typeof(TConcrete).GetTypeInfo().IsInterface)
		{
			throw new JsonTypeMapException<TAbstract, TConcrete>();
		}
		Type typeFromHandle = typeof(TAbstract);
		Type typeFromHandle2 = typeof(TConcrete);
		_MapTypes(typeFromHandle, typeFromHandle2, mappingBehavior);
	}

	public void MapGeneric(Type tAbstract, Type tConcrete, MapBaseAbstractionBehavior mappingBehavior = MapBaseAbstractionBehavior.Unmapped)
	{
		if (tConcrete.GetTypeInfo().IsAbstract || tConcrete.GetTypeInfo().IsInterface)
		{
			throw new JsonTypeMapException(tAbstract, tConcrete);
		}
		if (!tConcrete.InheritsFrom(tAbstract))
		{
			throw new JsonTypeMapException(tAbstract, tConcrete);
		}
		_MapTypes(tAbstract, tConcrete, mappingBehavior);
	}

	public void RemoveMap<TAbstract>(bool removeRelated = true)
	{
		Type typeFromHandle = typeof(TAbstract);
		if (!_registry.TryGetValue(typeFromHandle, out Type tConcrete))
		{
			return;
		}
		_registry.Remove(typeFromHandle);
		if (!removeRelated)
		{
			return;
		}
		foreach (Type item in (from kvp in _registry
			where kvp.Value == tConcrete
			select kvp.Key).ToList())
		{
			_registry.Remove(item);
		}
	}

	public Type GetMap(Type type)
	{
		if (_registry.TryGetValue(type, out Type value))
		{
			return value;
		}
		TypeInfo typeInfo = type.GetTypeInfo();
		if (typeInfo.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			foreach (KeyValuePair<Type, Type> item in _registry)
			{
				if (item.Key.GetTypeInfo().IsGenericTypeDefinition && item.Key.GetGenericTypeDefinition() == genericTypeDefinition)
				{
					Type[] typeArguments = type.GetTypeArguments();
					return item.Value.MakeGenericType(typeArguments);
				}
			}
		}
		if (!typeInfo.IsAbstract && !typeInfo.IsInterface)
		{
			_registry[type] = type;
		}
		return type;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal object CreateInstance(DeserializationContext context)
	{
		return CreateInstance(context.InferredType, context.LocalValue, context.RootSerializer.Options.Resolver, context.ValueMap);
	}

	internal object CreateInstance(Type type, JsonValue? json, IResolver resolver, Dictionary<SerializationInfo, object?>? parameters = null)
	{
		TypeInfo typeInfo = type.GetTypeInfo();
		if (!typeInfo.IsAbstract && !typeInfo.IsInterface && !typeInfo.IsGenericType)
		{
			return resolver.Resolve(type, parameters);
		}
		return _ResolveSlow(type, json, resolver, parameters);
	}

	internal Type IdentifyTypeToResolve(Type type, JsonValue? json)
	{
		if (json != null && json.Type == JsonValueType.Object && json.Object.ContainsKey("$type"))
		{
			string text = json.Object["$type"].String;
			Type type2 = Type.GetType(text);
			if (type2 == null)
			{
				throw new ArgumentException("Cannot find type " + text);
			}
			return type2;
		}
		if (!_registry.TryGetValue(type, out Type value))
		{
			Type key = type;
			if (type.GetTypeInfo().IsGenericType)
			{
				key = type.GetGenericTypeDefinition();
			}
			_registry.TryGetValue(key, out value);
		}
		if (value != null)
		{
			if (value.GetTypeInfo().IsGenericTypeDefinition)
			{
				return value.MakeGenericType(type.GetTypeArguments());
			}
			return value;
		}
		return type;
	}

	private object _ResolveSlow(Type type, JsonValue? json, IResolver resolver, Dictionary<SerializationInfo, object?>? parameters)
	{
		Type type2 = IdentifyTypeToResolve(type, json);
		if (type2.GetTypeInfo().IsInterface)
		{
			return TypeGenerator.Generate(type);
		}
		return resolver.Resolve(type2, parameters);
	}

	private void _MapTypes(Type tAbstract, Type tConcrete, MapBaseAbstractionBehavior mappingBehavior)
	{
		_registry[tAbstract] = tConcrete;
		switch (mappingBehavior)
		{
		case MapBaseAbstractionBehavior.Unmapped:
			_MapBaseTypes(tAbstract, tConcrete, overwrite: false);
			break;
		case MapBaseAbstractionBehavior.Override:
			_MapBaseTypes(tAbstract, tConcrete, overwrite: true);
			break;
		}
	}

	private void _MapBaseTypes(Type? tAbstract, Type tConcrete, bool overwrite)
	{
		if (tAbstract == null)
		{
			return;
		}
		Type baseType = tAbstract.GetTypeInfo().BaseType;
		if (baseType != null && (overwrite || !_registry.ContainsKey(baseType)))
		{
			_registry[baseType] = tConcrete;
		}
		_MapBaseTypes(baseType, tConcrete, overwrite);
		foreach (Type implementedInterface in tAbstract.GetTypeInfo().ImplementedInterfaces)
		{
			if (overwrite || !_registry.ContainsKey(implementedInterface))
			{
				_registry[implementedInterface] = tConcrete;
			}
			_MapBaseTypes(implementedInterface, tConcrete, overwrite);
		}
	}
}
