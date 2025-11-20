using UnityEngine;
using UnityEngine.UI;

namespace Gloomhaven;

public sealed class InterfaceSettings : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler uiCanvasScaler;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private ExtendedScrollRect scrollRect;

	[SerializeField]
	private ButtonSwitch toggleAbilityFocusOutlines;

	[SerializeField]
	private ButtonSwitch toggleHoverOutlines;

	private const float widthRatio = 0.000965f;

	private const float heightRatio = 0.001333f;

	private void OnSelected(ButtonSwitch button)
	{
		if (InputManager.GamePadInUse)
		{
			scrollRect.ScrollToFit(button.transform as RectTransform);
		}
	}

	private void EnableNavigation()
	{
		toggleAbilityFocusOutlines.SetNavigation(Navigation.Mode.Vertical);
		toggleHoverOutlines.SetNavigation(Navigation.Mode.Vertical);
	}

	private void DisableNavigation()
	{
		toggleAbilityFocusOutlines.DisableNavigation();
		toggleHoverOutlines.DisableNavigation();
	}

	private void OnDestroy()
	{
		toggleAbilityFocusOutlines.OnValueChanged.RemoveAllListeners();
		toggleHoverOutlines.OnValueChanged.RemoveAllListeners();
	}

	public void Initialize()
	{
		toggleAbilityFocusOutlines.IsOn = !SaveData.Instance.Global.DisableAbilityFocusOutlines;
		toggleHoverOutlines.IsOn = !SaveData.Instance.Global.DisableHoverOutlines;
		toggleAbilityFocusOutlines.OnValueChanged.AddListener(ToggleAbilityFocusOutlines);
		toggleHoverOutlines.OnValueChanged.AddListener(ToggleHoverOutlines);
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		toggleAbilityFocusOutlines.OnSelected.AddListener(delegate
		{
			OnSelected(toggleAbilityFocusOutlines);
		});
		toggleHoverOutlines.OnSelected.AddListener(delegate
		{
			OnSelected(toggleHoverOutlines);
		});
		if (controllerArea.IsFocused)
		{
			EnableNavigation();
		}
	}

	public void ToggleAbilityFocusOutlines(bool enable)
	{
		SaveData.Instance.Global.DisableAbilityFocusOutlines = !enable;
		SaveData.Instance.SaveGlobalData();
	}

	public void ToggleHoverOutlines(bool enable)
	{
		SaveData.Instance.Global.DisableHoverOutlines = !enable;
		SaveData.Instance.SaveGlobalData();
	}

	public void SetUIScale(float scaleFactor)
	{
		uiCanvasScaler.scaleFactor = Utility.Round(scaleFactor, 2);
	}
}
