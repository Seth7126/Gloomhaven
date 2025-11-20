using System;
using UnityEngine;

namespace Apparance.Unity;

public static class EditorServices
{
	public static bool isEditor;

	public static Vector3 cameraPosition;

	public static event Action repaintHandler;

	public static event Func<GameObject, bool> prefabCheckHandler;

	public static event Func<GameObject, GameObject> prefabInstancingHandler;

	public static event Action updateEvent;

	public static void RepaintAll()
	{
		if (EditorServices.repaintHandler != null)
		{
			EditorServices.repaintHandler();
		}
	}

	public static bool IsPrefab(GameObject o)
	{
		if (EditorServices.prefabCheckHandler != null)
		{
			return EditorServices.prefabCheckHandler(o);
		}
		return false;
	}

	public static GameObject InstancePrefab(GameObject prefab)
	{
		if (EditorServices.prefabInstancingHandler != null)
		{
			return EditorServices.prefabInstancingHandler(prefab);
		}
		return null;
	}

	public static void Update()
	{
		if (EditorServices.updateEvent != null)
		{
			EditorServices.updateEvent();
		}
	}
}
