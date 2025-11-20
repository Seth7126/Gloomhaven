using UnityEngine;

namespace MeshSimplification;

public class MeshPreview : MonoBehaviour
{
	[SerializeField]
	private MeshFilter _filter;

	[SerializeField]
	private MeshRenderer _meshRenderer;

	public void Show(Mesh mesh)
	{
		_filter.sharedMesh = mesh;
		_meshRenderer.enabled = true;
	}

	public void Hide()
	{
		_meshRenderer.enabled = false;
	}
}
