#define DEBUG
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

[Documentation]
public abstract class BoltSingletonPrefab<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	protected static string _resourcePath;

	public static T instance
	{
		get
		{
			Instantiate();
			return _instance;
		}
	}

	public static void Instantiate()
	{
		if ((bool)_instance)
		{
			return;
		}
		Object obj = Object.FindObjectOfType(typeof(T));
		if ((bool)obj)
		{
			_instance = (T)obj;
			return;
		}
		obj = Object.Instantiate(Resources.Load((_resourcePath == null) ? typeof(T).Name : _resourcePath, typeof(GameObject)));
		if ((bool)obj)
		{
			_instance = ((GameObject)obj).GetComponent<T>();
		}
		else
		{
			BoltLog.Error("could not load auto instance of {0}", typeof(T));
		}
	}
}
