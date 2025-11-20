using UnityEngine.Events;

namespace OdinSerializer;

public class UnityEventFormatter<T> : ReflectionFormatter<T> where T : UnityEventBase, new()
{
	protected override T GetUninitializedObject()
	{
		return new T();
	}
}
