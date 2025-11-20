using UnityEngine;

namespace VolumetricFogAndMist;

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(Camera))]
[ImageEffectAllowedInSceneView]
public class VolumetricFogPosT : MonoBehaviour, IVolumetricFogRenderComponent
{
	private Material copyOpaqueMat;

	public VolumetricFog fog { get; set; }

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (fog == null || !fog.enabled)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (fog.transparencyBlendMode == TRANSPARENT_MODE.None)
		{
			fog.DoOnRenderImage(source, destination);
			return;
		}
		RenderTexture temporary = RenderTexture.GetTemporary(source.descriptor);
		if (copyOpaqueMat == null)
		{
			copyOpaqueMat = new Material(Shader.Find("VolumetricFogAndMist/CopyOpaque"));
		}
		copyOpaqueMat.SetFloat("_BlendPower", fog.transparencyBlendPower);
		Graphics.Blit(source, destination, copyOpaqueMat, (fog.computeDepth && fog.downsampling == 1) ? 1 : 0);
		RenderTexture.ReleaseTemporary(temporary);
	}

	public void DestroySelf()
	{
		Object.DestroyImmediate(this);
	}
}
