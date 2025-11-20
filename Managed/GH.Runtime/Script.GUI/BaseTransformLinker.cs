using UnityEngine;

namespace Script.GUI;

public abstract class BaseTransformLinker : MonoBehaviour
{
	public abstract Transform LinkedTransform { get; }
}
