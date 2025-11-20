using UnityEngine;
using UnityEngine.UI;

namespace SRF.UI;

[RequireComponent(typeof(CanvasScaler))]
[AddComponentMenu("SRF/UI/Retina Scaler")]
public class SRRetinaScaler : SRMonoBehaviour
{
	[SerializeField]
	private bool _disablePixelPerfect;

	[SerializeField]
	private int _designDpi = 120;

	private float _lastDpi;

	private void Start()
	{
		ApplyScaling();
	}

	private void ApplyScaling()
	{
		float num = (_lastDpi = Screen.dpi);
		if (!(num <= 0f))
		{
			CanvasScaler component = GetComponent<CanvasScaler>();
			component.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			float num2 = num / (float)_designDpi;
			num2 = Mathf.Max(1f, Mathf.Round(num2 * 2f) / 2f);
			component.scaleFactor = num2;
			if (_disablePixelPerfect)
			{
				GetComponent<Canvas>().pixelPerfect = false;
			}
		}
	}

	private void Update()
	{
		if (Screen.dpi != _lastDpi)
		{
			ApplyScaling();
		}
	}
}
