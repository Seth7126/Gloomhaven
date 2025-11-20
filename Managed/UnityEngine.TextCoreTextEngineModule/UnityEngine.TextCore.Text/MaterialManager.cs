using System.Collections.Generic;

namespace UnityEngine.TextCore.Text;

internal static class MaterialManager
{
	private static Dictionary<long, Material> s_FallbackMaterials = new Dictionary<long, Material>();

	public static Material GetFallbackMaterial(Material sourceMaterial, Material targetMaterial)
	{
		int instanceID = sourceMaterial.GetInstanceID();
		Texture texture = targetMaterial.GetTexture(TextShaderUtilities.ID_MainTex);
		int instanceID2 = texture.GetInstanceID();
		long key = ((long)instanceID << 32) | (uint)instanceID2;
		if (s_FallbackMaterials.TryGetValue(key, out var value))
		{
			return value;
		}
		if (sourceMaterial.HasProperty(TextShaderUtilities.ID_GradientScale) && targetMaterial.HasProperty(TextShaderUtilities.ID_GradientScale))
		{
			value = new Material(sourceMaterial);
			value.hideFlags = HideFlags.HideAndDontSave;
			value.SetTexture(TextShaderUtilities.ID_MainTex, texture);
			value.SetFloat(TextShaderUtilities.ID_GradientScale, targetMaterial.GetFloat(TextShaderUtilities.ID_GradientScale));
			value.SetFloat(TextShaderUtilities.ID_TextureWidth, targetMaterial.GetFloat(TextShaderUtilities.ID_TextureWidth));
			value.SetFloat(TextShaderUtilities.ID_TextureHeight, targetMaterial.GetFloat(TextShaderUtilities.ID_TextureHeight));
			value.SetFloat(TextShaderUtilities.ID_WeightNormal, targetMaterial.GetFloat(TextShaderUtilities.ID_WeightNormal));
			value.SetFloat(TextShaderUtilities.ID_WeightBold, targetMaterial.GetFloat(TextShaderUtilities.ID_WeightBold));
		}
		else
		{
			value = new Material(targetMaterial);
		}
		s_FallbackMaterials.Add(key, value);
		return value;
	}

	public static Material GetFallbackMaterial(FontAsset fontAsset, Material sourceMaterial, int atlasIndex)
	{
		int instanceID = sourceMaterial.GetInstanceID();
		Texture texture = fontAsset.atlasTextures[atlasIndex];
		int instanceID2 = texture.GetInstanceID();
		long key = ((long)instanceID << 32) | (uint)instanceID2;
		if (s_FallbackMaterials.TryGetValue(key, out var value))
		{
			return value;
		}
		Material material = new Material(sourceMaterial);
		material.SetTexture(TextShaderUtilities.ID_MainTex, texture);
		material.hideFlags = HideFlags.HideAndDontSave;
		s_FallbackMaterials.Add(key, material);
		return material;
	}
}
