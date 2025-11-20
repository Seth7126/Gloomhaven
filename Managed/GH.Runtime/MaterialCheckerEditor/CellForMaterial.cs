using UnityEngine;

namespace MaterialCheckerEditor;

public class CellForMaterial : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer _meshRenderer;

	public void SetMaterial(Material mat)
	{
		_meshRenderer.material = mat;
	}
}
