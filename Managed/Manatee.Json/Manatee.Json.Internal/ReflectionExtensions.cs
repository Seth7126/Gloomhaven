using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;
using Manatee.Json.Serialization.Internal;

namespace Manatee.Json.Internal;

internal static class ReflectionExtensions
{
	public static bool InheritsFrom(this Type tDerived, Type tBase)
	{
		if (tDerived == tBase)
		{
			return true;
		}
		if (tDerived._IsSubtypeOf(tBase))
		{
			return true;
		}
		return tDerived.GetTypeInfo().ImplementedInterfaces.SelectMany(_GetAllInterfaces).Contains<Type>(tBase);
	}

	private static IEnumerable<Type> _GetAllInterfaces(Type type)
	{
		if (type.GetTypeInfo().IsGenericType)
		{
			yield return type.GetGenericTypeDefinition();
		}
		yield return type;
	}

	private static bool _IsSubtypeOf(this Type tDerived, Type tBase)
	{
		Type type = tDerived.GetTypeInfo().BaseType;
		while (type != null)
		{
			TypeInfo typeInfo = type.GetTypeInfo();
			if (typeInfo.IsGenericType)
			{
				type = type.GetGenericTypeDefinition();
			}
			if (type == tBase)
			{
				return true;
			}
			type = typeInfo.BaseType;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Type[] GetTypeArguments(this Type type)
	{
		return type.GetTypeInfo().GenericTypeArguments;
	}

	public static IEnumerable<PropertyInfo> GetAllProperties(this TypeInfo? type)
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		while (type != null)
		{
			list.AddRange(type.DeclaredProperties);
			type = type.BaseType?.GetTypeInfo();
		}
		return list;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNumericType(this Type value)
	{
		if (!(value == typeof(decimal)) && !(value == typeof(double)) && !(value == typeof(float)) && !(value == typeof(int)) && !(value == typeof(uint)) && !(value == typeof(short)) && !(value == typeof(ushort)) && !(value == typeof(byte)) && !(value == typeof(long)))
		{
			return value == typeof(ulong);
		}
		return true;
	}

	public static object? Default(this Type type)
	{
		if (!type.GetTypeInfo().IsValueType)
		{
			return null;
		}
		return Activator.CreateInstance(type);
	}

	public static void SetMember(this object? obj, JsonPointer pointer, object? value)
	{
		while (obj != null)
		{
			if (pointer.Count == 0)
			{
				throw new ArgumentException("Pointer must have at least one segment.");
			}
			string segment = pointer[0];
			SerializationInfo serializationInfo;
			if (int.TryParse(segment, out var result))
			{
				serializationInfo = ReflectionCache.GetMembers(obj.GetType(), PropertySelectionStrategy.ReadWriteOnly, includeFields: false).Single((SerializationInfo m) => m.MemberInfo is PropertyInfo propertyInfo3 && propertyInfo3.GetIndexParameters().Length == 1);
				if (serializationInfo == null)
				{
					break;
				}
				PropertyInfo propertyInfo = (PropertyInfo)serializationInfo.MemberInfo;
				if (pointer.Count == 1)
				{
					propertyInfo.SetValue(obj, value, new object[1] { result });
					break;
				}
				obj = propertyInfo.GetValue(obj, new object[1] { result });
				pointer = new JsonPointer(pointer.Skip(1));
				continue;
			}
			serializationInfo = ReflectionCache.GetMembers(obj.GetType(), PropertySelectionStrategy.ReadWriteOnly, includeFields: true).Single((SerializationInfo m) => m.SerializationName == segment);
			if (serializationInfo == null)
			{
				break;
			}
			if (serializationInfo.MemberInfo is PropertyInfo propertyInfo2)
			{
				if (pointer.Count == 1)
				{
					propertyInfo2.SetValue(obj, value);
					break;
				}
				obj = propertyInfo2.GetValue(obj);
				pointer = new JsonPointer(pointer.Skip(1));
				continue;
			}
			if (serializationInfo.MemberInfo is FieldInfo fieldInfo)
			{
				if (pointer.Count == 1)
				{
					fieldInfo.SetValue(obj, value);
					break;
				}
				obj = fieldInfo.GetValue(obj);
				pointer = new JsonPointer(pointer.Skip(1));
				continue;
			}
			break;
		}
	}

	public static string CSharpName(this Type type)
	{
		StringBuilder stringBuilder = new StringBuilder();
		type._CSharpName(stringBuilder);
		return stringBuilder.ToString();
	}

	private static void _CSharpName(this Type type, StringBuilder sb)
	{
		if (type._TryCSharpKeyword(out string name))
		{
			sb.Append(name);
			return;
		}
		name = type.Name;
		bool flag;
		if (type.IsAnonymousType())
		{
			sb.Append("[anon]{");
			flag = false;
			PropertyInfo[] properties = type.GetProperties();
			foreach (PropertyInfo obj in properties)
			{
				if (flag)
				{
					sb.Append(",");
				}
				obj.PropertyType._CSharpName(sb);
				flag = true;
			}
			sb.Append("}");
			return;
		}
		if (!type.IsGenericType)
		{
			sb.Append(type.Name);
			return;
		}
		sb.Append(name.Substring(0, name.IndexOf('`')));
		sb.Append("<");
		flag = false;
		Type[] genericArguments = type.GetGenericArguments();
		foreach (Type type2 in genericArguments)
		{
			if (flag)
			{
				sb.Append(",");
			}
			type2._CSharpName(sb);
			flag = true;
		}
		sb.Append(">");
	}

	public static bool IsAnonymousType(this Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), inherit: false) && type.IsGenericType && type.Name.Contains("AnonymousType") && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$")))
		{
			return type.Attributes.HasFlag(TypeAttributes.NotPublic);
		}
		return false;
	}

	private static bool _TryCSharpKeyword(this Type type, [NotNullWhen(true)] out string? name)
	{
		if (type == typeof(string))
		{
			name = "string";
			return true;
		}
		if (type == typeof(byte))
		{
			name = "byte";
			return true;
		}
		if (type == typeof(sbyte))
		{
			name = "sbyte";
			return true;
		}
		if (type == typeof(short))
		{
			name = "short";
			return true;
		}
		if (type == typeof(ushort))
		{
			name = "ushort";
			return true;
		}
		if (type == typeof(int))
		{
			name = "int";
			return true;
		}
		if (type == typeof(uint))
		{
			name = "uint";
			return true;
		}
		if (type == typeof(long))
		{
			name = "long";
			return true;
		}
		if (type == typeof(ulong))
		{
			name = "ulong";
			return true;
		}
		if (type == typeof(float))
		{
			name = "float";
			return true;
		}
		if (type == typeof(double))
		{
			name = "double";
			return true;
		}
		if (type == typeof(char))
		{
			name = "char";
			return true;
		}
		if (type == typeof(decimal))
		{
			name = "decimal";
			return true;
		}
		if (type == typeof(bool))
		{
			name = "bool";
			return true;
		}
		if (type == typeof(object))
		{
			name = "object";
			return true;
		}
		name = null;
		return false;
	}
}
