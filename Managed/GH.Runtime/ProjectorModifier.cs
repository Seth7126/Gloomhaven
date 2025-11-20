using UnityEngine;
using UnityEngine.Rendering;

public class ProjectorModifier : MonoBehaviour
{
	private MeshRenderer _meshRenderer;

	private void Awake()
	{
		if (PlatformLayer.Setting.UseDecalOptimization)
		{
			ReplaceProjector();
		}
	}

	private void OnEnable()
	{
		if (_meshRenderer != null)
		{
			_meshRenderer.enabled = true;
		}
	}

	private void OnDisable()
	{
		if (_meshRenderer != null)
		{
			_meshRenderer.enabled = false;
		}
	}

	private void ReplaceProjector()
	{
		Projector component = GetComponent<Projector>();
		if (!(component == null) && component.orthographic && component.ignoreLayers == 0)
		{
			component.enabled = false;
			base.gameObject.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
			_meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
			_meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			_meshRenderer.sharedMaterial = Resources.Load<Material>("Decals/MeshDecalMaterial");
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			materialPropertyBlock.SetTexture("_MainTex", component.material.mainTexture);
			materialPropertyBlock.SetColor("_Color", component.material.GetColor("_TintColor"));
			_meshRenderer.SetPropertyBlock(materialPropertyBlock);
			Vector3 forward = base.transform.forward;
			base.transform.forward = base.transform.up;
			Transform parent = base.transform.parent;
			base.transform.parent = null;
			base.transform.localScale = new Vector3(component.aspectRatio * 2f, component.farClipPlane - component.nearClipPlane, component.aspectRatio * 2f);
			base.transform.SetParent(parent, worldPositionStays: true);
			base.transform.position += forward * (component.farClipPlane - (component.farClipPlane - component.nearClipPlane) * 0.5f);
		}
	}
}
