using UnityEngine;
using UnityEngine.UI;

public class UIBlurDisabler : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private bool _useBlackFilter;

	[SerializeField]
	private Color _color = new Color(17f, 17f, 17f, 85f);

	private void Awake()
	{
		if (PlatformLayer.Setting.SimplifiedUI && PlatformLayer.Setting.SimplifiedUISettings.DisableUIBlur && _image != null)
		{
			_image.material = null;
			if (_useBlackFilter)
			{
				_image.color = _color;
				return;
			}
			_image.color = Color.clear;
			_image.enabled = false;
		}
	}
}
