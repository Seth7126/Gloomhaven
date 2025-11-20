using UnityEngine;

public interface IApparancePlacementProvider
{
	GameObject CreateInstance(GameObject template, int object_type_id, Vector3 position, Vector3 scale, Quaternion rotation, Transform parent, IApparancePlacementProvider default_provider);

	void BoundsOverride(GameObject template, int object_type_id, ref Vector3 min, ref Vector3 max);
}
