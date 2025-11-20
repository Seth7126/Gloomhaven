using System;
using UnityEngine;

namespace LowGraphicEffects;

[Serializable]
public class VignetteEffect : GraphicEffect
{
	private static readonly int _color1 = Shader.PropertyToID("_Color");

	private static readonly int _radius1 = Shader.PropertyToID("_Radius");

	private static readonly int _pow1 = Shader.PropertyToID("_Pow");

	private static readonly int _weight1 = Shader.PropertyToID("_Weight");

	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private Color _color;

	[Range(0f, 1f)]
	[SerializeField]
	private float _radius;

	[Range(0.1f, 10f)]
	[SerializeField]
	private float _pow;

	[Range(0f, 1f)]
	[SerializeField]
	private float _weight;

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
		_material.SetColor(_color1, _color);
		_material.SetFloat(_radius1, _radius);
		_material.SetFloat(_pow1, _pow);
		_material.SetFloat(_weight1, _weight);
		Graphics.Blit(source, destination, _material);
	}
}
