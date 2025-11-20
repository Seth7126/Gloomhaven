using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Manatee.Json.Serialization.Internal;

internal class ConstructorResolver : IResolver
{
	public object Resolve(Type type, Dictionary<SerializationInfo, object?>? parameters)
	{
		try
		{
			if (parameters == null)
			{
				return _ResolveSimple(type);
			}
			return _Resolve(type, parameters);
		}
		catch (Exception innerException)
		{
			throw new TypeInstantiationException(type, innerException);
		}
	}

	private static object _Resolve(Type type, Dictionary<SerializationInfo, object?>? parameters)
	{
		var list = (from c in type.GetTypeInfo().DeclaredConstructors
			select new
			{
				Method = c,
				Parameters = c.GetParameters()
			} into c
			orderby c.Parameters.Length
			select c).ToList();
		if (list.Count == 0)
		{
			return Activator.CreateInstance(type);
		}
		if (list[0].Parameters.Length == 0)
		{
			return list[0].Method.Invoke(null);
		}
		var anon = (from c in list.Select(c =>
			{
				var list2 = Enumerable.Join(c.Parameters, parameters, (ParameterInfo cp) => cp.Name.ToLowerInvariant(), (KeyValuePair<SerializationInfo, object> p) => (p.Key.SerializationName ?? p.Key.MemberInfo.Name).ToLowerInvariant(), (ParameterInfo cp, KeyValuePair<SerializationInfo, object> p) => new
				{
					ConstructorParameter = cp,
					JsonParameter = p
				}).ToList();
				double score = (double)list2.Count / (double)c.Parameters.Length;
				return new
				{
					Constructor = c.Method,
					Matched = list2,
					Score = score
				};
			})
			orderby c.Score descending, c.Matched.Count descending
			select c).First();
		return anon.Constructor.Invoke(anon.Matched.Select(m => m.JsonParameter.Value).ToArray());
	}

	private static object _ResolveSimple(Type type)
	{
		ConstructorInfo constructorInfo = null;
		foreach (ConstructorInfo declaredConstructor in type.GetTypeInfo().DeclaredConstructors)
		{
			if (declaredConstructor.GetParameters().Length == 0)
			{
				return declaredConstructor.Invoke(null);
			}
			if (constructorInfo == null)
			{
				constructorInfo = declaredConstructor;
			}
			else if (declaredConstructor.GetParameters().Length < constructorInfo.GetParameters().Length)
			{
				constructorInfo = declaredConstructor;
			}
		}
		if (constructorInfo != null)
		{
			object[] parameters = (from p in constructorInfo.GetParameters()
				select _ResolveSimple(p.ParameterType)).ToArray();
			return constructorInfo.Invoke(parameters);
		}
		return Activator.CreateInstance(type);
	}
}
