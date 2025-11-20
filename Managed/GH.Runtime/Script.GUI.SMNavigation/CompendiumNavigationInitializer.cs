using System;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation;

public class CompendiumNavigationInitializer : MonoBehaviour
{
	[SerializeField]
	private CompendiumWindow _compendium;

	[SerializeField]
	private UiNavigationRoot _root;

	private UIWindow _uiWindow;

	private UINavigationSelectable _firstSelectable;

	[UsedImplicitly]
	private void Awake()
	{
		_uiWindow = _compendium.GetComponent<UIWindow>();
		if (!InputManager.GamePadInUse)
		{
			return;
		}
		_firstSelectable = _compendium.sectionButtons[0].Subsections[0].GetComponent<UINavigationSelectable>();
		_compendium.sectionButtons.ForEach(delegate(UICompendiumButton section)
		{
			section.OnActivated.AddListener(delegate
			{
				UpdateNavigation(section);
			});
		});
		_compendium.subsectionButtons.ForEach(delegate(UICompendiumButton subsection)
		{
			subsection.GetComponent<UINavigationSelectable>().OnNavigationSelectedEvent += ActivateTab;
		});
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		foreach (UICompendiumButton sectionButton in _compendium.sectionButtons)
		{
			sectionButton.OnActivated.RemoveAllListeners();
		}
		foreach (UICompendiumButton subsectionButton in _compendium.subsectionButtons)
		{
			subsectionButton.GetComponent<UINavigationSelectable>().OnNavigationSelectedEvent -= ActivateTab;
		}
	}

	private void ActivateTab(IUiNavigationSelectable selectable)
	{
		(selectable as UINavigationSelectable)?.GetComponent<UITab>().Activate();
	}

	private void OnEnable()
	{
		UIWindow uiWindow = _uiWindow;
		uiWindow.OnShow = (Action)Delegate.Combine(uiWindow.OnShow, new Action(OnShow));
	}

	private void OnDisable()
	{
		UIWindow uiWindow = _uiWindow;
		uiWindow.OnShow = (Action)Delegate.Remove(uiWindow.OnShow, new Action(OnShow));
	}

	private void UpdateNavigation(UICompendiumButton compendiumSection)
	{
		UINavigationSelectable component = compendiumSection.Subsections[0].GetComponent<UINavigationSelectable>();
		Singleton<UINavigation>.Instance.NavigationManager.TrySelect(component);
	}

	private void OnShow()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.Compendium);
		Singleton<UINavigation>.Instance.NavigationManager.TrySelect(_firstSelectable);
	}
}
