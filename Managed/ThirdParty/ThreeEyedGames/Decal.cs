using UnityEngine;
using UnityEngine.Rendering;
using Utilities.MemoryManager;

namespace ThreeEyedGames;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class Decal : MonoBehaviour
{
	public enum DecalRenderMode
	{
		Deferred,
		Unlit,
		Invalid
	}

	private const string _deferredShaderName = "Decalicious/Deferred Decal";

	private const string _unlitShaderName = "Decalicious/Unlit Decal";

	private Shader _deferredShader;

	private Shader _unlitShader;

	private bool _useAnotherColor;

	private Color _anotherColor;

	private bool _startLoad;

	public DecalRenderMode RenderMode = DecalRenderMode.Invalid;

	[Tooltip("Set a Material with a Decalicious shader.")]
	[SerializeField]
	private Material Material;

	public ReferenceToObject<Material> ReferenceToMaterial;

	private Material _newMaterial;

	[Tooltip("Should this decal be drawn early (low number) or late (high number)?")]
	public int RenderOrder = 100;

	[Tooltip("To which degree should the Decal be drawn? At 1, the Decal will be drawn with full effect. At 0, the Decal will not be drawn. Experiment with values greater than one.")]
	public float Fade = 1f;

	[Tooltip("Set a GameObject here to only draw this Decal on the MeshRenderer of the GO or any of its children.")]
	public GameObject LimitTo;

	[Tooltip("Enable to draw the Albedo / Emission pass of the Decal.")]
	public bool DrawAlbedo = true;

	[Tooltip("Use an interpolated light probe for this decal for indirect light. This breaks instancing for the decal and thus comes with a performance impact, so use with caution.")]
	public bool UseLightProbes = true;

	[Tooltip("Enable to draw the Normal / SpecGloss pass of the Decal.")]
	public bool DrawNormalAndGloss = true;

	[Tooltip("Enable perfect Normal / SpecGloss blending between decals. Costly and has no effect when decals don't overlap, so use with caution.")]
	public bool HighQualityBlending;

	public Material CurrentMaterial
	{
		get
		{
			if (ReferenceToMaterial == null || !ReferenceToMaterial.IsLoaded())
			{
				return Material;
			}
			return _newMaterial;
		}
	}

	private void Awake()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (component.sharedMesh == null)
		{
			component.sharedMesh = Resources.Load<Mesh>("DecalCube");
		}
		if (_deferredShader == null)
		{
			_deferredShader = Shader.Find("Decalicious/Deferred Decal");
		}
		if (_unlitShader == null)
		{
			_unlitShader = Shader.Find("Decalicious/Unlit Decal");
		}
		MeshRenderer component2 = GetComponent<MeshRenderer>();
		component2.shadowCastingMode = ShadowCastingMode.Off;
		component2.receiveShadows = false;
		component2.materials = new Material[0];
		component2.lightProbeUsage = LightProbeUsage.BlendProbes;
		component2.reflectionProbeUsage = ReflectionProbeUsage.Off;
	}

	private void OnWillRenderObject()
	{
		if (Camera.current == null)
		{
			return;
		}
		DecaliciousRenderer decaliciousRenderer = Camera.current.GetComponent<DecaliciousRenderer>();
		if (decaliciousRenderer == null)
		{
			decaliciousRenderer = Camera.current.gameObject.AddComponent<DecaliciousRenderer>();
		}
		if (!decaliciousRenderer.isActiveAndEnabled || Fade <= 0f || ReferenceToMaterial == null)
		{
			return;
		}
		if (CurrentMaterial == null && !_startLoad)
		{
			_startLoad = true;
			ReferenceToMaterial.GetAsyncObject(OnLoadMaterial);
		}
		if (!(CurrentMaterial == null))
		{
			if (CurrentMaterial.shader == null)
			{
				RenderMode = DecalRenderMode.Invalid;
			}
			else if (CurrentMaterial.shader.name == _deferredShader.name)
			{
				RenderMode = DecalRenderMode.Deferred;
			}
			else if (CurrentMaterial.shader.name == _unlitShader.name)
			{
				RenderMode = DecalRenderMode.Unlit;
			}
			else
			{
				RenderMode = DecalRenderMode.Invalid;
			}
			CurrentMaterial.enableInstancing = decaliciousRenderer.UseInstancing;
			decaliciousRenderer.Add(this, LimitTo);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.clear;
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.color = Color.white * 0.2f;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.white * 0.5f;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}

	private void OnDestroy()
	{
		if (ReferenceToMaterial != null)
		{
			ReferenceToMaterial.Release();
		}
	}

	public void SetUseAnotherColor(Color color)
	{
		_useAnotherColor = true;
		_anotherColor = color;
	}

	private void OnLoadMaterial(Material material, AsyncLoadingState asyncLoadingState)
	{
		_startLoad = false;
		if (!(material == null))
		{
			_newMaterial = new Material(material);
			_ = _newMaterial.name;
			if (_newMaterial.mainTexture != null)
			{
				_ = _newMaterial.mainTexture.name;
			}
			if (_useAnotherColor)
			{
				_newMaterial.color = _anotherColor;
			}
		}
	}
}
