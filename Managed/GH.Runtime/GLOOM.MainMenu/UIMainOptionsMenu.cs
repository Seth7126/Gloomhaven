using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UIMainOptionsMenu : MonoBehaviour
{
	[SerializeField]
	protected CanvasGroup canvasGroupInteraction;

	[SerializeField]
	private UIMainMenuSubptionsPanel suboptionsMenu;

	[SerializeField]
	private GUIAnimator showAnimator;

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private UIStreamingInstallInfoWindow streamingInstallInfoWindow;

	[Header("Options")]
	[SerializeField]
	private MainOptionOpenSuboptions sandboxButton;

	[SerializeField]
	private MainOptionOpenSuboptions campaignButton;

	[SerializeField]
	private MainOptionOpenSuboptions guildmasterButton;

	[SerializeField]
	private MainOptionMultiplayer multiplayerButton;

	[SerializeField]
	private MainOptionOpenSuboptions extraButton;

	[SerializeField]
	private MainOption optionsButton;

	[SerializeField]
	private MainOption tutorialButton;

	[SerializeField]
	private UIMainMenuOption exitButton;

	private List<UIMainMenuOption> menuOptions = new List<UIMainMenuOption>();

	private UIWindow window;

	[UsedImplicitly]
	private void Awake()
	{
		window = GetComponent<UIWindow>();
		showAnimator.OnAnimationStarted.AddListener(SetInteractableFalse);
		showAnimator.OnAnimationFinished.AddListener(SetInteractableTrue);
		showAnimator.OnAnimationStopped.AddListener(SetInteractableTrue);
		controllerArea.OnFocused.AddListener(EnableNavigation);
		controllerArea.OnUnfocused.AddListener(DisableNavigation);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		showAnimator.OnAnimationStarted.RemoveListener(SetInteractableFalse);
		showAnimator.OnAnimationFinished.RemoveListener(SetInteractableTrue);
		showAnimator.OnAnimationStopped.RemoveListener(SetInteractableTrue);
		controllerArea.OnFocused.RemoveListener(EnableNavigation);
		controllerArea.OnUnfocused.RemoveListener(DisableNavigation);
		PlatformLayer.StreamingInstall.EventProgressChanged -= UpdateStreamingInstallInfoPanel;
		PlatformLayer.StreamingInstall.EventFilesDownloaded -= OnStreamingInstallFinished;
	}

	private void SetInteractableFalse()
	{
		SetInteractable(interactable: false);
	}

	private void SetInteractableTrue()
	{
		SetInteractable(interactable: true);
	}

	private void Start()
	{
		InitializeOption(campaignButton);
		InitializeOption(guildmasterButton);
		InitializeOption(multiplayerButton);
		InitializeOption(extraButton);
		InitializeOption(tutorialButton);
		InitializeOption(optionsButton);
		if (!PlatformLayer.Instance.IsConsole)
		{
			exitButton.gameObject.SetActive(value: true);
			InitializeButton(exitButton, Application.Quit);
		}
		sandboxButton.gameObject.SetActive(value: false);
		UpdateStreamingInstallInfoPanel(PlatformLayer.StreamingInstall.NormalizedProgress, PlatformLayer.StreamingInstall.EstimatedRequiredTime);
		if (!PlatformLayer.StreamingInstall.AllFilesAccessible)
		{
			LockButtons();
			streamingInstallInfoWindow.Show();
			PlatformLayer.StreamingInstall.EventProgressChanged += UpdateStreamingInstallInfoPanel;
			PlatformLayer.StreamingInstall.EventFilesDownloaded += OnStreamingInstallFinished;
		}
		else
		{
			streamingInstallInfoWindow.Hide();
		}
	}

	private void LockButtons()
	{
		guildmasterButton.Button.IsInteractable = false;
		campaignButton.Button.IsInteractable = false;
		multiplayerButton.Button.IsInteractable = false;
		if (sandboxButton.gameObject.activeSelf)
		{
			sandboxButton.Button.IsInteractable = false;
		}
	}

	private void UnlockButtons()
	{
		guildmasterButton.Button.IsInteractable = true;
		campaignButton.Button.IsInteractable = true;
		multiplayerButton.Button.IsInteractable = true;
		if (sandboxButton.gameObject.activeSelf)
		{
			sandboxButton.Button.IsInteractable = true;
		}
	}

	private void OnStreamingInstallFinished()
	{
		UnlockButtons();
		streamingInstallInfoWindow.Hide();
		PlatformLayer.StreamingInstall.EventProgressChanged -= UpdateStreamingInstallInfoPanel;
		PlatformLayer.StreamingInstall.EventFilesDownloaded -= OnStreamingInstallFinished;
	}

	private void UpdateStreamingInstallInfoPanel(float normalizedProgress, int timeEta)
	{
		streamingInstallInfoWindow.UpdateStreamingInstallWindow(normalizedProgress, timeEta);
	}

	private void InitializeOption(MainOptionOpenSuboptions option)
	{
		InitializeButton(option.Button, delegate
		{
			suboptionsMenu.Show(option.BuildOptions(), option.Button.transform as RectTransform, option.Button.Deselect);
		}, suboptionsMenu.Hide);
	}

	private void InitializeOption(MainOption option)
	{
		InitializeButton(option.Button, option.Select, option.Deselect);
	}

	private void InitializeButton(UIMainMenuOption button, Action onSelected, Action onDeselected = null)
	{
		menuOptions.Add(button);
		if (controllerArea.IsFocused)
		{
			button.EnableNavigation();
		}
		button.Init(delegate
		{
			SetFocused(isFocused: false);
			onSelected?.Invoke();
		}, delegate
		{
			SetFocused(isFocused: true);
			controllerArea.Focus();
			SetSelectedOption(button);
			onDeselected?.Invoke();
		});
	}

	private void SetFocused(bool isFocused)
	{
		campaignButton.SetFocused(isFocused);
		guildmasterButton.SetFocused(isFocused);
		extraButton.SetFocused(isFocused);
		optionsButton.SetFocused(isFocused);
		multiplayerButton.SetFocused(isFocused);
		tutorialButton.SetFocused(isFocused);
		if (!PlatformLayer.Instance.IsConsole)
		{
			exitButton.SetFocused(isFocused);
		}
		if (sandboxButton.gameObject.activeSelf)
		{
			sandboxButton.SetFocused(isFocused);
		}
	}

	public void SetInteractable(bool interactable)
	{
		if (window.IsOpen)
		{
			canvasGroupInteraction.blocksRaycasts = interactable;
		}
	}

	public void Show(bool instant = false)
	{
		if (PlatformLayer.Instance.IsDelayedInit)
		{
			multiplayerButton.RefreshJoinSessionButton();
		}
		showAnimator.Stop();
		SetFocused(isFocused: true);
		window.Show(instant);
		if (Singleton<UILoadGameWindow>.Instance != null && Singleton<UILoadGameWindow>.Instance.UICreateGameDlcStep != null)
		{
			Singleton<UILoadGameWindow>.Instance.UICreateGameDlcStep.DlcSelector.UpdateOptions();
		}
		if (instant)
		{
			showAnimator.GoToFinishState();
		}
		else
		{
			showAnimator.Play();
		}
		controllerArea.Focus();
	}

	public void OpenJoinMultiplayer()
	{
		Show(instant: true);
		multiplayerButton.SelectJoinMultiplayer();
	}

	public void BackFromLevelEditor()
	{
		SceneController.Instance.YML.Unload(regenCards: false);
		OpenExtras();
	}

	public void OpenExtras()
	{
		Show(instant: true);
		extraButton.Select();
	}

	public void BackFromSandbox()
	{
		SceneController.Instance.YML.Unload(regenCards: false);
		OpenSandbox();
	}

	public void OpenSandbox()
	{
		Show(instant: true);
		sandboxButton.Select();
	}

	public void Hide()
	{
		showAnimator.Stop();
		suboptionsMenu.Hide();
		window.Hide();
		controllerArea.Unfocus();
		Singleton<PromotionDLCManager>.Instance?.Hide();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			ControllerInputAreaManager.Instance.SetDefaultFocusArea(EControllerInputAreaType.None);
		}
	}

	private void DisableNavigation()
	{
		StopAllCoroutines();
		for (int i = 0; i < menuOptions.Count; i++)
		{
			menuOptions[i].DisableNavigation();
		}
	}

	private void EnableNavigation()
	{
		StopAllCoroutines();
		UIMainMenuOption uIMainMenuOption = null;
		for (int i = 0; i < menuOptions.Count; i++)
		{
			menuOptions[i].EnableNavigation();
			if (uIMainMenuOption == null && menuOptions[i].IsSelected)
			{
				uIMainMenuOption = menuOptions[i];
			}
		}
		if (uIMainMenuOption != null)
		{
			SetSelectedOption(uIMainMenuOption);
		}
	}

	private void SetSelectedOption(UIMainMenuOption option)
	{
		StopAllCoroutines();
		if (controllerArea.IsFocused)
		{
			EventSystem.current.SetSelectedGameObject(option.gameObject);
		}
	}
}
