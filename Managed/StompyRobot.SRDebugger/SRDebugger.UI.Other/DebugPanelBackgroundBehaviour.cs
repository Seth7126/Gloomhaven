using SRF;
using SRF.UI;
using UnityEngine;

namespace SRDebugger.UI.Other;

[RequireComponent(typeof(StyleComponent))]
public class DebugPanelBackgroundBehaviour : SRMonoBehaviour
{
	private StyleComponent _styleComponent;

	public string TransparentStyleKey = "";

	[SerializeField]
	private StyleSheet _styleSheet;

	private void Awake()
	{
		_styleComponent = GetComponent<StyleComponent>();
		if (Settings.Instance.EnableBackgroundTransparency)
		{
			Style style = _styleSheet.GetStyle(TransparentStyleKey);
			Color normalColor = style.NormalColor;
			normalColor.a = Settings.Instance.BackgroundTransparency;
			style.NormalColor = normalColor;
			_styleComponent.StyleKey = TransparentStyleKey;
		}
	}
}
