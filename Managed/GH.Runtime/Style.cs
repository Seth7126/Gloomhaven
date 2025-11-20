using UnityEngine;

public abstract class Style<T> : ScriptableObject
{
	public abstract void Apply(T go);
}
