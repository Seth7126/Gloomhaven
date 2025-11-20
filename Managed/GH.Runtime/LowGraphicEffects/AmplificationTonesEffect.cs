using System;
using UnityEngine;
using UnityStandardAssets.Utility.Ambience;

namespace LowGraphicEffects;

[Serializable]
public class AmplificationTonesEffect : GraphicEffect
{
	private static readonly int _exposure1 = Shader.PropertyToID("_Exposure");

	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private AmbienceConfig _ambienceConfig;

	private Material _material;

	protected override void OnInit()
	{
		base.OnInit();
		_material = new Material(_shader);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_material = null;
	}

	public override void RenderEffect(RenderTexture source, RenderTexture destination)
	{
		_material.SetFloat(_exposure1, _ambienceConfig.Exposure);
		Graphics.Blit(source, destination, _material);
	}

	public void SetExposure(AmbienceConfig ambienceConfig)
	{
		_ambienceConfig = ambienceConfig;
	}
}
