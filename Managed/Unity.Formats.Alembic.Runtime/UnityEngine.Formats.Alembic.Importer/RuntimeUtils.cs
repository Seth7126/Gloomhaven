using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Formats.Alembic.Importer;

internal static class RuntimeUtils
{
	public static void DisposeIfPossible<T>(this ref NativeArray<T> array) where T : struct
	{
		if (array.IsCreated)
		{
			array.Dispose();
		}
	}

	public static NativeArray<T> ResizeIfNeeded<T>(this ref NativeArray<T> array, int newLength, Allocator a = Allocator.Persistent) where T : struct
	{
		if (array.Length != newLength)
		{
			DisposeIfPossible(ref array);
			array = new NativeArray<T>(newLength, a);
		}
		if (!array.IsCreated)
		{
			array = new NativeArray<T>(0, a);
		}
		return array;
	}

	public unsafe static void* GetPointer<T>(this NativeArray<T> array) where T : struct
	{
		if (array.Length != 0)
		{
			return array.GetUnsafePtr();
		}
		return null;
	}

	public static ulong CombineHash(this ulong h1, ulong h2)
	{
		return h1 ^ (h2 + 2654435769u + (h1 << 6) + (h1 >> 2));
	}

	public static GameObject CreateGameObjectWithUndo(string message)
	{
		return new GameObject();
	}

	public static T GetOrAddComponent<T>(this GameObject go) where T : Component
	{
		T component = go.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		return go.AddComponent<T>();
	}

	public static void DestroyUnityObject(Object o)
	{
		Object.Destroy(o);
	}

	public static void DepthFirstVisitor(this GameObject root, Action<GameObject> lambda)
	{
		for (int i = 0; i < root.transform.childCount; i++)
		{
			root.transform.GetChild(i).gameObject.DepthFirstVisitor(lambda);
		}
		lambda(root);
	}
}
