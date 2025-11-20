using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerInputSubmit : ControllerInputElement
{
	[SerializeField]
	private Selectable m_Selectable;

	private void Awake()
	{
		if (m_Selectable == null)
		{
			m_Selectable = GetComponent<Selectable>();
		}
	}

	private void OnDestroy()
	{
		if (Singleton<InputManager>.Instance != null)
		{
			Singleton<InputManager>.Instance.PlayerControl.UISubmit.OnPressed -= Click;
		}
	}

	private void OnValidate()
	{
		if (m_Selectable == null)
		{
			m_Selectable = GetComponent<Selectable>();
		}
	}

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		Singleton<InputManager>.Instance.PlayerControl.UISubmit.OnPressed += Click;
	}

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		Singleton<InputManager>.Instance.PlayerControl.UISubmit.OnPressed -= Click;
	}

	private void Click()
	{
		if (m_Selectable.IsInteractable())
		{
			ExecuteEvents.Execute(m_Selectable.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
		}
	}
}
