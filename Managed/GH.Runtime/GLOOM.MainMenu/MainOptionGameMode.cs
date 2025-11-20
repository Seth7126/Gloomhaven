using System.Collections.Generic;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MainOptionGameMode : MainOptionOpenSuboptions
{
	[SerializeField]
	private MenuOptionIcon newIcon;

	[SerializeField]
	private MenuOptionIcon continueIcon;

	[SerializeField]
	private MenuOptionIcon loadIcon;

	[SerializeField]
	private UILoadGameWindow loadGameWindow;

	[SerializeField]
	private UICreateGameWindow createGameWindow;

	private IGameModeService service;

	private MenuSuboption loadOpt;

	private MenuSuboption resumeOpt;

	protected override void Awake()
	{
		base.Awake();
		service = GetComponent<IGameModeService>();
		service.RegisterToDeletedGame(RefreshOptions);
		resumeOpt = new MenuSuboption("GUI_CONTINUE", continueIcon, service.ResumeLastGame);
		loadOpt = new MenuSuboption("GUI_LOAD", loadIcon, delegate
		{
			loadGameWindow.Show(service, loadOpt.Deselect);
		}, loadGameWindow.Hide);
	}

	private void RefreshOptions()
	{
		resumeOpt.IsInteractable = service.CanResume();
		loadOpt.IsInteractable = service.CanLoadGames();
		if (!loadOpt.IsInteractable)
		{
			loadOpt.ClearFrame();
		}
	}

	public override List<MenuSuboption> BuildOptions()
	{
		resumeOpt.Reset();
		resumeOpt.IsInteractable = service.CanResume();
		loadOpt.Reset();
		loadOpt.IsInteractable = service.CanLoadGames();
		MenuSuboption createOpt = null;
		createOpt = new MenuSuboption("GUI_NEW", newIcon, delegate
		{
			createGameWindow.Show(service, createOpt.Deselect);
		}, createGameWindow.Hide);
		return new List<MenuSuboption> { resumeOpt, loadOpt, createOpt };
	}
}
