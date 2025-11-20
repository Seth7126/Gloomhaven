using UnityEngine;

namespace Script.GUI;

public class DirectTransformLinker : BaseTransformLinker
{
	[SerializeField]
	private Transform _linkedTransform;

	public override Transform LinkedTransform => _linkedTransform;
}
