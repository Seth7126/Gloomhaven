using UnityEngine;
using UnityEngine.UI;

public class CanvasStoryHide : StoryHide
{
	[SerializeField]
	private Canvas m_Canvas;

	[SerializeField]
	private GraphicRaycaster m_GraphicRaycaster;

	public override void Hide()
	{
		m_Canvas.enabled = false;
		if (m_GraphicRaycaster != null)
		{
			m_GraphicRaycaster.enabled = false;
		}
	}

	public override void Show()
	{
		m_Canvas.enabled = true;
		if (m_GraphicRaycaster != null)
		{
			m_GraphicRaycaster.enabled = true;
		}
	}
}
