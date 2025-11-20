using System;
using Gloomhaven;
using UnityEngine;
using UnityStandardAssets.Utility.Ambience;

namespace LowGraphicEffects;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class GraphicEffects : MonoBehaviour
{
	[Serializable]
	private struct PostProcessData
	{
		public Color ColorFilter;

		public float ACESForce;

		public Color ColorVignette;

		[Range(0f, 1f)]
		public float RadiusVignette;

		[Range(0.1f, 10f)]
		public float PowVignette;

		[Range(0f, 1f)]
		public float WeightVignette;
	}

	private static readonly int _colorFilter = Shader.PropertyToID("_ColorFilter");

	private static readonly int _acesForce = Shader.PropertyToID("_ACESForce");

	private static readonly int _colorVignette = Shader.PropertyToID("_ColorVignette");

	private static readonly int _radiusVignette = Shader.PropertyToID("_RadiusVignette");

	private static readonly int _powVignette = Shader.PropertyToID("_PowVignette");

	private static readonly int _weightVignette = Shader.PropertyToID("_WeightVignette");

	[SerializeField]
	private Shader _lowPostProcessShader;

	[SerializeField]
	private PostProcessData _postProcessData;

	private Material _material;

	private PostProcessData _oldPostProcessData;

	private void Awake()
	{
		CreateMaterial();
	}

	private void CreateMaterial()
	{
		_material = new Material(_lowPostProcessShader);
		FillData();
		_oldPostProcessData = _postProcessData;
	}

	private void FillData()
	{
		_material.SetColor(_colorFilter, _postProcessData.ColorFilter);
		_material.SetFloat(_acesForce, _postProcessData.ACESForce);
		_material.SetColor(_colorVignette, _postProcessData.ColorVignette);
		_material.SetFloat(_radiusVignette, _postProcessData.RadiusVignette);
		_material.SetFloat(_powVignette, _postProcessData.PowVignette);
		_material.SetFloat(_weightVignette, _postProcessData.WeightVignette);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!_postProcessData.Equals(_oldPostProcessData))
		{
			_oldPostProcessData = _postProcessData;
			FillData();
		}
		if (GraphicSettings.s_Antialiasing != AntialiasingRender.DISABLED && PlatformLayer.Setting.UseSimpleAntialiasTechnique)
		{
			Graphics.Blit(source, destination, _material, 1);
		}
		else
		{
			Graphics.Blit(source, destination, _material, 0);
		}
	}

	public void SetExposure(AmbienceConfig ambienceConfig)
	{
		_postProcessData.ACESForce = ambienceConfig.Exposure;
	}
}
