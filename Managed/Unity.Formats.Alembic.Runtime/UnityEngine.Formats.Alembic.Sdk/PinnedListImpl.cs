using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEngine.Formats.Alembic.Sdk;

internal static class PinnedListImpl
{
	private class ListData
	{
		public object items;

		public int size;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct Caster
	{
		[FieldOffset(0)]
		public object list;

		[FieldOffset(0)]
		public ListData data;
	}

	internal static T[] GetInternalArray<T>(List<T> list) where T : struct
	{
		if (list == null)
		{
			return null;
		}
		return (T[])new Caster
		{
			list = list
		}.data.items;
	}

	internal static List<T> CreateIntrusiveList<T>(T[] data) where T : struct
	{
		if (data == null)
		{
			return null;
		}
		List<T> list = new List<T>();
		Caster caster = new Caster
		{
			list = list,
			data = 
			{
				items = data,
				size = data.Length
			}
		};
		return list;
	}

	internal static void SetCount<T>(List<T> list, int count) where T : struct
	{
		if (list != null)
		{
			Caster caster = new Caster
			{
				list = list,
				data = 
				{
					size = count
				}
			};
		}
	}
}
