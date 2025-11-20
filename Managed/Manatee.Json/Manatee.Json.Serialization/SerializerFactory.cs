using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Manatee.Json.Internal;
using Manatee.Json.Serialization.Internal;
using Manatee.Json.Serialization.Internal.Serializers;

namespace Manatee.Json.Serialization;

public static class SerializerFactory
{
	private static readonly ITypeSerializer _autoSerializer;

	private static readonly List<ISerializer> _serializers;

	private static readonly TypeInfo[] _dependentSerializers;

	private static List<ISerializer> _orderedSerializers;

	static SerializerFactory()
	{
		_dependentSerializers = new TypeInfo[3]
		{
			typeof(ReferencingSerializer).GetTypeInfo(),
			typeof(SchemaValidator).GetTypeInfo(),
			typeof(DefaultValueSerializer).GetTypeInfo()
		};
		_serializers = (from t in typeof(ISerializer).GetTypeInfo().Assembly.DefinedTypes.Where((TypeInfo t) => t.ImplementedInterfaces.Contains(typeof(ISerializer)) && t.IsClass && !t.IsAbstract).Except<TypeInfo>(_dependentSerializers)
			select Activator.CreateInstance(t.AsType())).Cast<ISerializer>().ToList();
		_autoSerializer = _serializers.OfType<AutoSerializer>().FirstOrDefault();
		_UpdateOrderedSerializers();
	}

	public static void AddSerializer(ISerializer serializer)
	{
		if (_serializers.FirstOrDefault((ISerializer s) => s.GetType() == serializer.GetType()) == null)
		{
			_serializers.Add(serializer);
		}
		_UpdateOrderedSerializers();
	}

	public static void RemoveSerializer<T>() where T : ISerializer
	{
		T val = _serializers.OfType<T>().FirstOrDefault();
		_serializers.Remove(val);
		_UpdateOrderedSerializers();
	}

	internal static ISerializer GetSerializer(SerializationContextBase context)
	{
		context.OverrideInferredType(context.RootSerializer.AbstractionMap.GetMap(context.InferredType ?? context.RequestedType));
		ISerializer theChosenOne = _orderedSerializers.First((ISerializer s) => s.Handles(context));
		if (theChosenOne is AutoSerializer && context.RequestedType != typeof(object))
		{
			Type inferredType = context.InferredType;
			context.OverrideInferredType(context.RootSerializer.AbstractionMap.GetMap(context.RequestedType));
			theChosenOne = _orderedSerializers.First((ISerializer s) => s.Handles(context));
			if (theChosenOne is AutoSerializer)
			{
				context.OverrideInferredType(inferredType);
			}
		}
		Log.Serialization(() => "Serializer " + (theChosenOne.GetType().CSharpName() ?? "<not found>") + " selected for type `" + (context.InferredType ?? context.RequestedType).CSharpName() + "`");
		return theChosenOne;
	}

	internal static ITypeSerializer GetTypeSerializer()
	{
		return _autoSerializer;
	}

	private static void _UpdateOrderedSerializers()
	{
		_orderedSerializers = _serializers.OrderBy((ISerializer s) => (s as IPrioritizedSerializer)?.Priority ?? 0).ToList();
	}
}
