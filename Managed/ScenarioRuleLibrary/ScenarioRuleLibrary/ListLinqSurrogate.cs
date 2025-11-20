using System;
using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public static class ListLinqSurrogate
{
	public static List<T> WhereCustom<T>(this List<T> list, Func<T, bool> predicate)
	{
		List<T> list2 = new List<T>();
		for (int i = 0; i < list.Count; i++)
		{
			if (predicate(list[i]))
			{
				list2.Add(list[i]);
			}
		}
		return list2;
	}

	public static List<K> SelectManyCustom<T, K>(this List<T> list, Func<T, List<K>> predicate)
	{
		List<K> list2 = new List<K>();
		for (int i = 0; i < list.Count; i++)
		{
			list2.AddRange(predicate(list[i]));
		}
		return list2;
	}

	public static List<K> SelectManyWhere<T, K>(this List<T> list, Func<T, List<K>> predicateSelcetMany, Func<K, bool> predicateWhere)
	{
		List<K> list2 = new List<K>();
		for (int i = 0; i < list.Count; i++)
		{
			List<K> list3 = predicateSelcetMany(list[i]);
			for (int j = 0; j < list3.Count; j++)
			{
				if (predicateWhere(list3[j]))
				{
					list2.Add(list3[j]);
				}
			}
		}
		return list2;
	}
}
