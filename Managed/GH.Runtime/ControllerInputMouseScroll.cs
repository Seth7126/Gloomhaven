using InControl;
using UnityEngine;
using UnityEngine.UI;

internal class ControllerInputMouseScroll : ControllerInputElement
{
	[SerializeField]
	private ScrollRect m_Scroll;

	[SerializeField]
	private float m_ScrollSpeed = 0.3f;

	[SerializeField]
	private bool m_IsAbsoluteSpeed;

	[SerializeField]
	private bool _disableWhenInEscMenu;

	private void Awake()
	{
		if (m_Scroll == null)
		{
			m_Scroll = GetComponent<ScrollRect>();
		}
	}

	private void OnValidate()
	{
		if (m_Scroll == null)
		{
			m_Scroll = GetComponent<ScrollRect>();
		}
	}

	protected override void OnEnable()
	{
		OnEnabledControllerControl();
	}

	private void Update()
	{
		if (Singleton<InputManager>.Instance.PlayerControl == null || !m_Scroll.verticalScrollbar.IsInteractable())
		{
			return;
		}
		float num = Singleton<InputManager>.Instance.PlayerControl.MouseWheelDelta.Value;
		if (!Mathf.Approximately(num, float.Epsilon))
		{
			if (m_IsAbsoluteSpeed)
			{
				num /= m_Scroll.content.rect.height - m_Scroll.viewport.rect.height;
			}
			m_Scroll.verticalNormalizedPosition = Mathf.Clamp01(m_Scroll.verticalNormalizedPosition + num * m_ScrollSpeed * InControl.InputManager.Sensitivity);
		}
	}
}
