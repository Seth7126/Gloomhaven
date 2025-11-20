using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonSwitch : MonoBehaviour
{
	[SerializeField]
	private ExtendedToggle toggle;

	[SerializeField]
	private Image image;

	[SerializeField]
	private TextLocalizedListener text;

	[Header("On")]
	[SerializeField]
	private Sprite onSprite;

	[SerializeField]
	private string onText;

	[Header("Off")]
	[SerializeField]
	private Sprite offSprite;

	[SerializeField]
	private string offText;

	public UnityEvent OnSelected => toggle.onSelected;

	public bool IsOn
	{
		get
		{
			return toggle.isOn;
		}
		set
		{
			toggle.isOn = value;
		}
	}

	public Toggle.ToggleEvent OnValueChanged => toggle.onValueChanged;

	private void Awake()
	{
		toggle.onValueChanged.AddListener(Refresh);
		Refresh(toggle.isOn);
	}

	private void OnDestroy()
	{
		toggle.onValueChanged.RemoveAllListeners();
		toggle.onDeselected.RemoveAllListeners();
		toggle.onSelected.RemoveAllListeners();
		toggle.onMouseEnter.RemoveAllListeners();
		toggle.onMouseExit.RemoveAllListeners();
	}

	public void Refresh(bool on)
	{
		if (image != null)
		{
			image.sprite = (IsOn ? onSprite : offSprite);
		}
		text.SetTextKey(IsOn ? onText : offText);
	}

	public void SetValue(bool value)
	{
		toggle.SetValue(value);
		Refresh(value);
	}

	public void SetNavigation(Navigation.Mode mode)
	{
		toggle.SetNavigation(mode);
	}

	public void DisableNavigation()
	{
		toggle.DisableNavigation();
	}
}
