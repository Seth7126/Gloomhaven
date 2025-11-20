using UnityEngine;

public interface IInputModulePointer
{
	GameObject GameObjectUnderPointer(int pointerId = -1);
}
