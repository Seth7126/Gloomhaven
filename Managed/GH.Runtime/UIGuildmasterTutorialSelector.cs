using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGuildmasterTutorialSelector : UIModeSelector<EGuildmasterTutorial>
{
	protected override void Awake()
	{
		base.Awake();
		foreach (ModeConfig mode in m_Modes)
		{
			Toggle toggle = mode.toggle;
			toggle.onValueChanged.AddListener(OnValueChanged);
			void OnValueChanged(bool value)
			{
				if (value)
				{
					toggle.OnPointerEnter(new PointerEventData(EventSystem.current));
				}
				else
				{
					toggle.OnPointerExit(new PointerEventData(EventSystem.current));
				}
			}
		}
	}
}
