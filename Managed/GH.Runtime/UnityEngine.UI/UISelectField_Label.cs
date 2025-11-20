namespace UnityEngine.UI;

[ExecuteInEditMode]
[AddComponentMenu("UI/Select Field - Label", 58)]
public class UISelectField_Label : MonoBehaviour
{
	public enum TransitionType
	{
		None,
		CrossFade
	}

	public Text textComponent;

	public TransitionType transitionType;

	public ColorBlockExtended colors = ColorBlockExtended.defaultColorBlock;

	protected void Awake()
	{
		if (textComponent == null)
		{
			textComponent = GetComponent<Text>();
		}
	}

	public void UpdateState(UISelectField.VisualState state)
	{
		UpdateState(state, instant: false);
	}

	public void UpdateState(UISelectField.VisualState state, bool instant)
	{
		if (!(textComponent == null) && base.gameObject.activeInHierarchy && transitionType != TransitionType.None)
		{
			Color color = colors.normalColor;
			switch (state)
			{
			case UISelectField.VisualState.Normal:
				color = colors.normalColor;
				break;
			case UISelectField.VisualState.Highlighted:
				color = colors.highlightedColor;
				break;
			case UISelectField.VisualState.Pressed:
				color = colors.pressedColor;
				break;
			case UISelectField.VisualState.Selected:
				color = colors.selectedColor;
				break;
			case UISelectField.VisualState.Active:
				color = colors.activeColor;
				break;
			case UISelectField.VisualState.ActiveHighlighted:
				color = colors.activeHighlightedColor;
				break;
			case UISelectField.VisualState.ActivePressed:
				color = colors.activePressedColor;
				break;
			case UISelectField.VisualState.ActiveSelected:
				color = colors.activeSelectedColor;
				break;
			case UISelectField.VisualState.Disabled:
				color = colors.disabledColor;
				break;
			}
			if (transitionType == TransitionType.CrossFade)
			{
				StartColorTween(color * colors.colorMultiplier, instant || colors.fadeDuration == 0f);
			}
		}
	}

	private void StartColorTween(Color color, bool instant)
	{
		if (!(textComponent == null))
		{
			if (instant)
			{
				textComponent.canvasRenderer.SetColor(color);
			}
			else
			{
				textComponent.CrossFadeColor(color, colors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
			}
		}
	}

	private void TriggerAnimation(string trigger)
	{
		Animator component = GetComponent<Animator>();
		if (!(component == null) && component.isActiveAndEnabled)
		{
			component.SetTrigger(trigger);
		}
	}
}
