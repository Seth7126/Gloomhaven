using SM.Gamepad;
using UnityEngine;
using UnityEngine.EventSystems;

public class GuildmasterLevelUpTab : MonoBehaviour, INavigationTab
{
	[SerializeField]
	private GameObject _highlightOutline;

	[SerializeField]
	private GameObject _highlightIcon;

	public void Select()
	{
		ExecuteEvents.Execute(base.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
		_highlightOutline.SetActive(value: true);
		_highlightIcon.SetActive(value: true);
	}

	public void Deselect()
	{
		_highlightOutline.SetActive(value: false);
		_highlightIcon.SetActive(value: false);
	}
}
