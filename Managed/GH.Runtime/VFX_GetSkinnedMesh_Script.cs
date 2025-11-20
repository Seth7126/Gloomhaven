#define ENABLE_LOGS
using UnityEngine;

public class VFX_GetSkinnedMesh_Script : MonoBehaviour
{
	public bool getSkinnedMeshfromParent;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnEnable()
	{
		if (getSkinnedMeshfromParent)
		{
			SkinnedMeshRenderer componentInParent = GetComponentInParent<SkinnedMeshRenderer>();
			ParticleSystem component = GetComponent<ParticleSystem>();
			if (component != null)
			{
				ParticleSystem.ShapeModule shape = component.shape;
				shape.skinnedMeshRenderer = componentInParent;
			}
		}
		else
		{
			Debug.Log("This script doesn't serve any function atm. Enable bool or extend script");
		}
	}
}
