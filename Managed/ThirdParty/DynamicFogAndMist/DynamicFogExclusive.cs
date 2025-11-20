using UnityEngine;

namespace DynamicFogAndMist;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[HelpURL("http://kronnect.com/taptapgo")]
[ImageEffectAllowedInSceneView]
public class DynamicFogExclusive : DynamicFogBase
{
	private RenderTexture rt;

	[Range(0.1f, 2f)]
	public float renderScale = 1f;

	private void OnPreRender()
	{
		if (!(fogMat == null) && _alpha != 0f && !(currentCamera == null))
		{
			int width = (int)((float)currentCamera.pixelWidth * renderScale);
			int num = (int)((float)currentCamera.pixelHeight * renderScale);
			rt = RenderTexture.GetTemporary(width, num, 24, RenderTextureFormat.ARGB32);
			rt.antiAliasing = 1;
			rt.wrapMode = TextureWrapMode.Clamp;
			currentCamera.targetTexture = rt;
		}
	}

	private void OnPostRender()
	{
		if (fogMat == null || _alpha == 0f || currentCamera == null)
		{
			return;
		}
		if (shouldUpdateMaterialProperties)
		{
			shouldUpdateMaterialProperties = false;
			UpdateMaterialPropertiesNow();
		}
		if (currentCamera.orthographic)
		{
			if (!matOrtho)
			{
				ResetMaterial();
			}
			fogMat.SetVector("_ClipDir", currentCamera.transform.forward);
		}
		else if (matOrtho)
		{
			ResetMaterial();
		}
		fogMat.SetMatrix("_ClipToWorld", currentCamera.cameraToWorldMatrix * currentCamera.projectionMatrix.inverse);
		currentCamera.targetTexture = null;
		Graphics.Blit(rt, null, fogMat);
		RenderTexture.ReleaseTemporary(rt);
	}
}
