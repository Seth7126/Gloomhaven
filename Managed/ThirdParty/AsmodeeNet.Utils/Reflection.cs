using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace AsmodeeNet.Utils;

public static class Reflection
{
	public static Hashtable HashtableFromMessage(IMessage message, HashSet<string> excludedFields = null)
	{
		Hashtable hashtable = new Hashtable();
		foreach (FieldDescriptor item in message.Descriptor.Fields.InDeclarationOrder())
		{
			if (!excludedFields.Contains(item.Name))
			{
				hashtable.Add(item.Name, item.Accessor.GetValue(message));
			}
		}
		return hashtable;
	}

	public static string PrettyPrintFromMessage(IMessage message, HashSet<string> excludedFields = null)
	{
		string text = "";
		foreach (FieldDescriptor item in message.Descriptor.Fields.InDeclarationOrder())
		{
			if (!excludedFields.Contains(item.Name))
			{
				text += $"#{item.FieldNumber} {item.Name} - {item.Accessor.GetValue(message)}\n";
			}
		}
		return text;
	}

	public static Hashtable HashtableFromObject(object obj, HashSet<string> excludedFields = null, uint maxDepth = 30u)
	{
		object obj2 = ParseObject(obj, "root", excludedFields, 1u, maxDepth);
		Hashtable hashtable = obj2 as Hashtable;
		if (obj2 != null && hashtable == null)
		{
			hashtable = new Hashtable { { "array", obj2 } };
		}
		return hashtable;
	}

	private static object ParseObject(object obj, string varPath, HashSet<string> excludedFields, uint depth, uint maxDepth)
	{
		if (depth > maxDepth)
		{
			return null;
		}
		if (CanBeLogged(obj))
		{
			return obj;
		}
		if (IsCollection(obj))
		{
			return ParseCollection(obj as ICollection, depth, maxDepth, varPath, excludedFields);
		}
		return ParseUnknownObject(obj, depth, maxDepth, varPath, excludedFields);
	}

	private static object ParseCollection(ICollection collec, uint depth, uint maxDepth, string varPath, HashSet<string> excludedFields)
	{
		if (depth > maxDepth)
		{
			return null;
		}
		if (collec == null)
		{
			return null;
		}
		Hashtable hashtable = new Hashtable();
		int num = 0;
		foreach (object item in collec)
		{
			object value = ParseObject(item, varPath + "." + num, excludedFields, depth + 1, maxDepth);
			hashtable.Add(num, value);
			num++;
		}
		return hashtable;
	}

	private static object ParseUnknownObject(object obj, uint depth, uint maxDepth, string varPath, HashSet<string> excludedFields)
	{
		if (depth > maxDepth)
		{
			return null;
		}
		if (obj == null)
		{
			return null;
		}
		Hashtable hashtable = new Hashtable();
		Type type = obj.GetType();
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = type.GetField(fieldInfo.Name).GetValue(obj);
			string text = varPath + "." + fieldInfo.Name;
			if (!PathMatchesExclusion(text, excludedFields))
			{
				object value2 = ParseObject(value, text, excludedFields, depth + 1, maxDepth);
				hashtable.Add(fieldInfo.Name, value2);
			}
		}
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			try
			{
				object value3 = type.GetProperty(propertyInfo.Name).GetValue(obj, null);
				string text2 = varPath + "." + propertyInfo.Name;
				if (!PathMatchesExclusion(text2, excludedFields))
				{
					object value4 = ParseObject(value3, text2, excludedFields, depth + 1, maxDepth);
					hashtable.Add(propertyInfo.Name, value4);
				}
			}
			catch (Exception)
			{
				hashtable.Add(propertyInfo.Name, "failed to parse");
			}
		}
		return hashtable;
	}

	public static bool CanBeLogged(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return CanLogType(obj.GetType());
	}

	public static bool CanLogType(Type type)
	{
		if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
		{
			return true;
		}
		if (type.IsArray)
		{
			return CanLogType(type.GetElementType());
		}
		if (typeof(ICollection).IsAssignableFrom(type))
		{
			Type[] genericArguments = type.GetGenericArguments();
			bool result = true;
			Type[] array = genericArguments;
			for (int i = 0; i < array.Length; i++)
			{
				if (!CanLogType(array[i]))
				{
					result = false;
				}
			}
			return result;
		}
		return false;
	}

	public static bool IsCollection(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return typeof(ICollection).IsAssignableFrom(obj.GetType());
	}

	private static bool PathMatchesExclusion(string path, HashSet<string> excludedFields)
	{
		if (excludedFields == null)
		{
			return false;
		}
		foreach (string excludedField in excludedFields)
		{
			if (path.Contains(excludedField))
			{
				return true;
			}
		}
		return false;
	}
}
