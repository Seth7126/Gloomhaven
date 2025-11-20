using System;

namespace UnityEngine.UIElements;

[Serializable]
public class ThemeStyleSheet : StyleSheet
{
	internal override void OnEnable()
	{
		base.isDefaultStyleSheet = true;
		base.OnEnable();
	}
}
