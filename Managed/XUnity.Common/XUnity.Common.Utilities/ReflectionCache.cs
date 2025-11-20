using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XUnity.Common.Utilities;

public static class ReflectionCache
{
	private struct MemberLookupKey
	{
		public Type Type { get; set; }

		public string MemberName { get; set; }

		public MemberLookupKey(Type type, string memberName)
		{
			Type = type;
			MemberName = memberName;
		}

		public override bool Equals(object obj)
		{
			if (obj is MemberLookupKey memberLookupKey)
			{
				if (Type == memberLookupKey.Type)
				{
					return MemberName == memberLookupKey.MemberName;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode() + MemberName.GetHashCode();
		}
	}

	private static Dictionary<MemberLookupKey, CachedMethod> Methods = new Dictionary<MemberLookupKey, CachedMethod>();

	private static Dictionary<MemberLookupKey, CachedProperty> Properties = new Dictionary<MemberLookupKey, CachedProperty>();

	private static Dictionary<MemberLookupKey, CachedField> Fields = new Dictionary<MemberLookupKey, CachedField>();

	public static CachedMethod CachedMethod(this Type type, string name)
	{
		return type.CachedMethod(name, (Type[])null);
	}

	public static CachedMethod CachedMethod(this Type type, string name, params Type[] types)
	{
		MemberLookupKey key = new MemberLookupKey(type, name);
		if (!Methods.TryGetValue(key, out var value))
		{
			Type type2 = type;
			MethodInfo methodInfo = null;
			while (methodInfo == null && type2 != null)
			{
				methodInfo = ((types != null && types.Length != 0) ? type2.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null) : type2.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
				type2 = type2.BaseType;
			}
			if (methodInfo != null)
			{
				value = new CachedMethod(methodInfo);
			}
			Methods[key] = value;
		}
		return value;
	}

	public static CachedProperty CachedProperty(this Type type, string name)
	{
		MemberLookupKey key = new MemberLookupKey(type, name);
		if (!Properties.TryGetValue(key, out var value))
		{
			Type type2 = type;
			PropertyInfo propertyInfo = null;
			while (propertyInfo == null && type2 != null)
			{
				propertyInfo = type2.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				type2 = type2.BaseType;
			}
			if (propertyInfo != null)
			{
				value = new CachedProperty(propertyInfo);
			}
			Properties[key] = value;
		}
		return value;
	}

	public static CachedField CachedField(this Type type, string name)
	{
		MemberLookupKey key = new MemberLookupKey(type, name);
		if (!Fields.TryGetValue(key, out var value))
		{
			Type type2 = type;
			FieldInfo fieldInfo = null;
			while (fieldInfo == null && type2 != null)
			{
				fieldInfo = type2.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				type2 = type2.BaseType;
			}
			if (fieldInfo != null)
			{
				value = new CachedField(fieldInfo);
			}
			Fields[key] = value;
		}
		return value;
	}

	public static CachedField CachedFieldByIndex(this Type type, int index, Type fieldType, BindingFlags flags)
	{
		FieldInfo[] array = (from x in type.GetFields(flags)
			where x.FieldType == fieldType
			select x).ToArray();
		if (index < array.Length)
		{
			return new CachedField(array[index]);
		}
		return null;
	}
}
