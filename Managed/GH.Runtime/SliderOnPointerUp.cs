using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
[AddComponentMenu("Event/SliderOnPointerUp")]
public sealed class SliderOnPointerUp : MonoBehaviour, IPointerUpHandler, IEventSystemHandler
{
	[Serializable]
	public class SliderOnPointerUpEvent : UnityEvent<float>
	{
	}

	public SliderOnPointerUpEvent sliderOnPointerUp;

	private Slider slider;

	private float oldValue;

	private void Start()
	{
		slider = GetComponent<Slider>();
		oldValue = slider.value;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (slider.value != oldValue)
		{
			sliderOnPointerUp.Invoke(slider.value);
			oldValue = slider.value;
		}
	}
}
