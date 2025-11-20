using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CompendiumTooltip : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public string sectionId;

	private UIWindow window;

	private CompendiumWindow compendium;

	private void Awake()
	{
		window = UIWindow.GetWindow(UIWindowID.CompendiumPanel);
		compendium = window.GetComponent<CompendiumWindow>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (InputManager.GetKey(Key.F1))
		{
			window.Show();
			compendium.GoToSection(sectionId);
		}
	}
}
