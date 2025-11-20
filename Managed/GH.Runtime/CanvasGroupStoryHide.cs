using UnityEngine;

public class CanvasGroupStoryHide : StoryHide
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	public override void Hide()
	{
		m_CanvasGroup.alpha = 0f;
		CanvasGroup canvasGroup = m_CanvasGroup;
		bool interactable = (m_CanvasGroup.blocksRaycasts = false);
		canvasGroup.interactable = interactable;
	}

	public override void Show()
	{
		m_CanvasGroup.alpha = 1f;
		CanvasGroup canvasGroup = m_CanvasGroup;
		bool interactable = (m_CanvasGroup.blocksRaycasts = true);
		canvasGroup.interactable = interactable;
	}
}
