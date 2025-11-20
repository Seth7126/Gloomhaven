using UnityEngine;

public class ObjectOcclusionVolume : MonoBehaviour
{
	private void OnEnable()
	{
		TilesOcclusionGenerator.s_Instance.AddObjectRenderer(GetComponent<MeshRenderer>());
	}

	private void OnDisable()
	{
		if (TilesOcclusionGenerator.s_Instance != null)
		{
			TilesOcclusionGenerator.s_Instance.RemoveObjectRenderer(GetComponent<MeshRenderer>());
		}
	}
}
