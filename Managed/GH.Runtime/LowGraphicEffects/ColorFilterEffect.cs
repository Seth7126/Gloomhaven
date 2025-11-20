using System;
using UnityEngine;

namespace LowGraphicEffects;

[Serializable]
public class ColorFilterEffect : GraphicEffect
{
	private static readonly int _color1 = Shader.PropertyToID("_Color");

	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private Color _color;

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
		Graphics.Blit(source, destination, _material);
	}
}
