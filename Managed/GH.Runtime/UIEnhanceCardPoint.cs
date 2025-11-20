using UnityEngine;
using UnityEngine.UI;

public class UIEnhanceCardPoint : MonoBehaviour
{
	[SerializeField]
	private Image enhancedCheck;

	[SerializeField]
	private Image enhancedCheckPulse;

	[SerializeField]
	private GUIAnimator enhanceAnimator;

	[SerializeField]
	private Image selected;

	[SerializeField]
	private LoopAnimator nextPointHighlightAnimator;

	public void Initialize(bool enhanced, Color color, bool isSelected = false)
	{
		SetEnhanced(enhanced);
		UpdateColor(color);
		HighlightSelected(isSelected);
	}

	private void SetEnhanced(bool enhanced)
	{
		enhanceAnimator?.Stop(goToEnd: true);
		enhancedCheck.enabled = enhanced;
		HighlightNextPoint(highlight: false);
	}

	public void Enhance()
	{
		SetEnhanced(enhanced: true);
		enhanceAnimator?.Play();
	}

	public void RemoveEnhance()
	{
		SetEnhanced(enhanced: false);
	}

	public void HighlightNextPoint(bool highlight)
	{
		if (highlight)
		{
			nextPointHighlightAnimator.StartLoop(resetToInitial: true);
			return;
		}
		nextPointHighlightAnimator.StopLoop();
		nextPointHighlightAnimator.ResetToInitialState();
	}

	public void UpdateColor(Color color)
	{
		Image image = enhancedCheck;
		Image image2 = enhancedCheckPulse;
		Color color2 = (selected.color = color);
		Color color4 = (image2.color = color2);
		image.color = color4;
	}

	public void HighlightSelected(bool isSelected)
	{
		selected.enabled = isSelected;
	}
}
