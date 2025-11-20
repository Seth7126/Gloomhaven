using UnityEngine;

namespace DynamicFogAndMist;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[HelpURL("http://kronnect.com/taptapgo")]
[ImageEffectAllowedInSceneView]
public class DynamicFog : DynamicFogBase
{
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (fogMat == null || _alpha == 0f || currentCamera == null)
		{
			Graphics.Blit(source, destination);
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
		Graphics.Blit(source, destination, fogMat);
	}
}
