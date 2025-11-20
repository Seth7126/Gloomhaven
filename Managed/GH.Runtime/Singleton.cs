using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T Instance => _instance;

	public static bool IsInitialized => _instance != null;

	protected virtual void Awake()
	{
		_instance = this as T;
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}

	protected void SetInstance(T newInstance)
	{
		_instance = newInstance;
	}
}
