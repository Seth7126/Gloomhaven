using System.Collections;
using System.Reflection;

internal static class CountPropertyHelper
{
	public static bool TryGetCount(object value, out int count)
	{
		if (value is ICollection collection)
		{
			count = collection.Count;
			return true;
		}
		PropertyInfo runtimeProperty = value.GetType().GetRuntimeProperty("Count");
		if (runtimeProperty != null && runtimeProperty.CanRead && runtimeProperty.PropertyType == typeof(int))
		{
			count = (int)runtimeProperty.GetValue(value);
			return true;
		}
		count = -1;
		return false;
	}
}
