using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SRF.UI;

[AddComponentMenu("SRF/UI/Flash Graphic")]
[ExecuteInEditMode]
public class FlashGraphic : UIBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	public float DecayTime = 0.15f;

	public Color DefaultColor = new Color(1f, 1f, 1f, 0f);

	public Color FlashColor = Color.white;

	public Graphic Target;

	private bool _isHoldingUntilNextPress;

	public void OnPointerDown(PointerEventData eventData)
	{
		Target.CrossFadeColor(FlashColor, 0f, ignoreTimeScale: true, useAlpha: true);
		_isHoldingUntilNextPress = false;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (!_isHoldingUntilNextPress)
		{
			Target.CrossFadeColor(DefaultColor, DecayTime, ignoreTimeScale: true, useAlpha: true);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!_isHoldingUntilNextPress)
		{
			Target.CrossFadeColor(DefaultColor, 0f, ignoreTimeScale: true, useAlpha: true);
		}
	}

	public void Flash()
	{
		Target.CrossFadeColor(FlashColor, 0f, ignoreTimeScale: true, useAlpha: true);
		Target.CrossFadeColor(DefaultColor, DecayTime, ignoreTimeScale: true, useAlpha: true);
		_isHoldingUntilNextPress = false;
	}

	public void FlashAndHoldUntilNextPress()
	{
		Target.CrossFadeColor(FlashColor, 0f, ignoreTimeScale: true, useAlpha: true);
		_isHoldingUntilNextPress = true;
	}
}
