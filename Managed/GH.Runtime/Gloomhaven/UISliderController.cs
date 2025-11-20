using System;
using AsmodeeNet.Foundation;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gloomhaven;

public class UISliderController : UISliderBar, IPointerUpHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler
{
	public UnityEvent OnSelected;

	public UnityEvent OnDeselected;

	private int oldValue;

	private event Action<float> SliderOnPointerUp;

	private event Action<float> OnValueChanged;

	private void Start()
	{
		progress.onValueChanged.AddListener(delegate(float val)
		{
			SetAmountText((int)val);
		});
		progress.onValueChanged.AddListener(OnProgressValueChanged);
	}

	private void OnDestroy()
	{
		progress.onValueChanged.RemoveListener(OnProgressValueChanged);
	}

	private void OnProgressValueChanged(float value)
	{
		this.OnValueChanged?.Invoke(value);
	}

	public void SubscribeOnSliderValueChanged(Action<float> call)
	{
		if (InputManager.GamePadInUse)
		{
			OnValueChanged += call;
		}
		else
		{
			SliderOnPointerUp += call;
		}
	}

	public void ClearOnSliderValueChanged()
	{
		this.SliderOnPointerUp = null;
		this.OnValueChanged = null;
	}

	public override void SetAmount(int amount)
	{
		oldValue = amount;
		base.SetAmount(amount);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if ((int)progress.value != oldValue)
		{
			this.SliderOnPointerUp?.Invoke(progress.value);
			oldValue = (int)progress.value;
		}
	}

	public void EnableNavigation(bool select = false)
	{
		progress.SetNavigation(Navigation.Mode.Automatic);
		if (select)
		{
			progress.Select();
		}
	}

	public void DisableNavigation()
	{
		progress.DisableNavigation();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == base.gameObject)
			{
				OnDeselect(null);
			}
			DisableNavigation();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (InputManager.GamePadInUse)
		{
			OnSelected?.Invoke();
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if ((int)progress.value != oldValue)
		{
			this.SliderOnPointerUp?.Invoke(progress.value);
			oldValue = (int)progress.value;
		}
		OnDeselected?.Invoke();
	}
}
