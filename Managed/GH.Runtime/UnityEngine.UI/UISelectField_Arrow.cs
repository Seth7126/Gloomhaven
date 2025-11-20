namespace UnityEngine.UI;

[ExecuteInEditMode]
[AddComponentMenu("UI/Select Field - Arrow", 58)]
[RequireComponent(typeof(Image))]
public class UISelectField_Arrow : MonoBehaviour
{
	public Graphic targetGraphic;

	public Selectable.Transition transitionType;

	public ColorBlockExtended colors = ColorBlockExtended.defaultColorBlock;

	public SpriteStateExtended spriteState;

	public AnimationTriggersExtended animationTriggers = new AnimationTriggersExtended();

	protected void Awake()
	{
		if (targetGraphic == null)
		{
			targetGraphic = GetComponent<Graphic>();
		}
	}

	public void UpdateState(UISelectField.VisualState state)
	{
		UpdateState(state, instant: false);
	}

	public void UpdateState(UISelectField.VisualState state, bool instant)
	{
		if (!(targetGraphic == null) && base.gameObject.activeInHierarchy && transitionType != Selectable.Transition.None)
		{
			Color color = colors.normalColor;
			Sprite newSprite = null;
			string trigger = animationTriggers.normalTrigger;
			switch (state)
			{
			case UISelectField.VisualState.Normal:
				color = colors.normalColor;
				newSprite = null;
				trigger = animationTriggers.normalTrigger;
				break;
			case UISelectField.VisualState.Highlighted:
				color = colors.highlightedColor;
				newSprite = spriteState.highlightedSprite;
				trigger = animationTriggers.highlightedTrigger;
				break;
			case UISelectField.VisualState.Pressed:
				color = colors.pressedColor;
				newSprite = spriteState.pressedSprite;
				trigger = animationTriggers.pressedTrigger;
				break;
			case UISelectField.VisualState.Selected:
				color = colors.selectedColor;
				newSprite = spriteState.selectedSprite;
				trigger = animationTriggers.selectedTrigger;
				break;
			case UISelectField.VisualState.Active:
				color = colors.activeColor;
				newSprite = spriteState.activeSprite;
				trigger = animationTriggers.activeTrigger;
				break;
			case UISelectField.VisualState.ActiveHighlighted:
				color = colors.activeHighlightedColor;
				newSprite = spriteState.activeHighlightedSprite;
				trigger = animationTriggers.activeHighlightedTrigger;
				break;
			case UISelectField.VisualState.ActivePressed:
				color = colors.activePressedColor;
				newSprite = spriteState.activePressedSprite;
				trigger = animationTriggers.activePressedTrigger;
				break;
			case UISelectField.VisualState.ActiveSelected:
				color = colors.activeSelectedColor;
				newSprite = spriteState.activeSelectedSprite;
				trigger = animationTriggers.activeSelectedTrigger;
				break;
			case UISelectField.VisualState.Disabled:
				color = colors.disabledColor;
				newSprite = spriteState.disabledSprite;
				trigger = animationTriggers.disabledTrigger;
				break;
			}
			switch (transitionType)
			{
			case Selectable.Transition.ColorTint:
				StartColorTween(color * colors.colorMultiplier, instant || colors.fadeDuration == 0f);
				break;
			case Selectable.Transition.SpriteSwap:
				DoSpriteSwap(newSprite);
				break;
			case Selectable.Transition.Animation:
				TriggerAnimation(trigger);
				break;
			}
		}
	}

	private void StartColorTween(Color color, bool instant)
	{
		if (!(targetGraphic == null))
		{
			if (instant)
			{
				targetGraphic.canvasRenderer.SetColor(color);
			}
			else
			{
				targetGraphic.CrossFadeColor(color, colors.fadeDuration, ignoreTimeScale: true, useAlpha: true);
			}
		}
	}

	private void DoSpriteSwap(Sprite newSprite)
	{
		if (!(targetGraphic == null))
		{
			Image image = targetGraphic as Image;
			if (image != null)
			{
				image.overrideSprite = newSprite;
			}
		}
	}

	private void TriggerAnimation(string trigger)
	{
		Animator component = GetComponent<Animator>();
		if (!(component == null) && component.isActiveAndEnabled && !(component.runtimeAnimatorController == null) && !string.IsNullOrEmpty(trigger))
		{
			component.ResetTrigger(animationTriggers.normalTrigger);
			component.ResetTrigger(animationTriggers.pressedTrigger);
			component.ResetTrigger(animationTriggers.highlightedTrigger);
			component.ResetTrigger(animationTriggers.activeTrigger);
			component.ResetTrigger(animationTriggers.activeHighlightedTrigger);
			component.ResetTrigger(animationTriggers.activePressedTrigger);
			component.ResetTrigger(animationTriggers.disabledTrigger);
			component.ResetTrigger(animationTriggers.selectedTrigger);
			component.ResetTrigger(animationTriggers.activeSelectedTrigger);
			component.SetTrigger(trigger);
		}
	}
}
