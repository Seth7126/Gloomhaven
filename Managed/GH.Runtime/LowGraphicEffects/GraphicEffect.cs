using System;
using UnityEngine;

namespace LowGraphicEffects;

[Serializable]
public class GraphicEffect
{
	protected bool _isInit;

	public void Init()
	{
		_isInit = true;
		OnInit();
	}

	protected virtual void OnInit()
	{
	}

	public void Disable()
	{
		_isInit = false;
		OnDisable();
	}

	protected virtual void OnDisable()
	{
	}

	public bool IsInit()
	{
		return _isInit;
	}

	public virtual void RenderEffect(RenderTexture source, RenderTexture destination)
	{
	}
}
