using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GraphicsListEnabler
{
	[SerializeField]
	private List<Graphic> _graphics;

	public void SetEnable(bool enable)
	{
		foreach (Graphic graphic in _graphics)
		{
			graphic.enabled = enable;
		}
	}
}
