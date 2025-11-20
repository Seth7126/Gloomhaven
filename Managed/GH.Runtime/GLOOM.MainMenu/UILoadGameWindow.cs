using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using Platforms.Social;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UILoadGameWindow : Singleton<UILoadGameWindow>
{
	[SerializeField]
	private ExtendedScrollRect scroll;

	[SerializeField]
	private UILoadGameSlot slotPrefab;

	[SerializeField]
	private GUIAnimator showAnimator;

	[SerializeField]
	private UIEnableDLCAnimator enableDLCAnimator;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private UICreateGameDLCStep _uiCreateGameDlcStep;

	[SerializeField]
	private MonoHotkeySession _monoHotkeySession;

	private List<UILoadGameSlot> pool = new List<UILoadGameSlot>();

	private Dictionary<IGameSaveData, UILoadGameSlot> slots = new Dictionary<IGameSaveData, UILoadGameSlot>();

	private int _currentDlcCount;

	private Coroutine _delayedSavesUpdateCoroutine;

	private WaitForSeconds _waitForSavesUpdate = new WaitForSeconds(0.2f);

	private UIWindow window;

	private ILoadGameService service;

	private Action onClosed;

	private Hotkey _selectHotkey;

	private Hotkey _activateDLCHotkey;

	private int focusedSlot;

	public UICreateGameDLCStep UICreateGameDlcStep => _uiCreateGameDlcStep;

	public UIWindow Window => window;

	public Hotkey SelectHotkey => _selectHotkey;

	public Hotkey ActivateDLCHotkey => _activateDLCHotkey;

	protected override void Awake()
	{
		base.Awake();
		window = GetComponent<UIWindow>();
		window.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				controllerArea.Destroy();
				onClosed?.Invoke();
			}
		});
		window.onHidden.AddListener(StopAnimations);
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		controllerArea.OnDisabledArea.AddListener(delegate
		{
			FocusSlot(0);
		});
		base.Awake();
	}

	public void InitSelectHotkey()
	{
		if (_selectHotkey == null)
		{
			_monoHotkeySession.TryGetHotkeyByExpectedEvent("Select", out _selectHotkey);
		}
		if (_activateDLCHotkey == null)
		{
			_monoHotkeySession.TryGetHotkeyByExpectedEvent("ActivateDLC", out _activateDLCHotkey);
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			StopAnimations();
		}
	}

	private void EnableNavigation()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			pool[i].EnableNavigation();
		}
		FocusSlot(focusedSlot);
	}

	private void DisableNavigation()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			pool[i].DisableNavigation();
		}
	}

	private void ClearTempAvatarStorage()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			pool[i].ClearTempAvatar();
		}
	}

	public void Show(ILoadGameService service, Action onClosed)
	{
		this.onClosed = onClosed;
		this.service = service;
		showAnimator.Stop();
		focusedSlot = 0;
		List<IGameSaveData> saves = service.GetSaves();
		if (IsCurrentPlatformValid())
		{
			IEnumerable<string> enumerable = from saveData in saves
				where saveData.Owner.PlatformName == PlatformLayer.Instance.PlatformID
				select saveData.Owner.PlatformNetworkAccountID;
			IEnumerable<string> source = (enumerable as string[]) ?? enumerable.ToArray();
			if (!source.Any())
			{
				LoadSaves();
				return;
			}
			UiNavigationBlocker blocker = new UiNavigationBlocker("blocker");
			InputManager.RequestDisableInput(this, EKeyActionTag.All);
			Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(blocker);
			PlatformLayer.Platform.PlatformSocial.GetUsersAsync(source.ToList(), delegate(OperationResult operationResult, List<User> users)
			{
				if (operationResult == OperationResult.Success)
				{
					foreach (IGameSaveData save in saves)
					{
						User user = users.Find((User user2) => user2.UserId == save.Owner.PlatformNetworkAccountID);
						if (user != null && user.PictureUri != null)
						{
							save.Owner.ActualAvatarURL = user.PictureUri;
							save.Owner.Username = user.UserName;
						}
					}
				}
				LoadSaves();
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
				Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(blocker);
			});
		}
		else
		{
			LoadSaves();
		}
		static bool IsCurrentPlatformValid()
		{
			string platformID = PlatformLayer.Instance.PlatformID;
			return platformID == "GameCore" || platformID == "PlayStation4" || platformID == "PlayStation5";
		}
		void LoadSaves()
		{
			LoadSavedGames(saves);
			scroll.ScrollToTop();
			if (_monoHotkeySession != null)
			{
				_monoHotkeySession.Show();
			}
			window.Show();
			showAnimator.Play();
			controllerArea.Enable();
		}
	}

	private void LoadSavedGames(List<IGameSaveData> saves)
	{
		slots.Clear();
		HelperTools.NormalizePool(ref pool, slotPrefab.gameObject, scroll.content, saves.Count, delegate(UILoadGameSlot slot)
		{
			slot.OnLoadData.AddListener(LoadParty);
			slot.OnDeleteData.AddListener(AskDeleteParty);
			slot.OnEnableDLC.AddListener(EnableDLC);
			slot.OnHovered.AddListener(delegate
			{
				if (InputManager.GamePadInUse && EventSystem.current.currentSelectedGameObject == slot.gameObject)
				{
					scroll.ScrollToFit(slot.transform as RectTransform);
				}
				focusedSlot = slots.Values.OrderBy((UILoadGameSlot it) => it.transform.GetSiblingIndex()).FindIndex((UILoadGameSlot it) => it == slot);
			});
		});
		for (int num = 0; num < saves.Count; num++)
		{
			IGameSaveData gameSaveData = saves[num];
			slots[gameSaveData] = pool[num];
			slots[gameSaveData].OnDLCEnabled = OnDLCEnabledGamepad;
			pool[num].SetData(gameSaveData);
			if (controllerArea.IsFocused)
			{
				pool[num].EnableNavigation();
				if (num == focusedSlot)
				{
					EventSystem.current.SetSelectedGameObject(pool[num].gameObject);
				}
			}
		}
	}

	public void Hide()
	{
		Singleton<UIConfirmationBoxManager>.Instance.Hide();
		enableDLCAnimator.Stop();
		if (_monoHotkeySession != null)
		{
			_monoHotkeySession.Hide();
		}
		window.Hide();
		if (_delayedSavesUpdateCoroutine != null)
		{
			CoroutineHelper.instance.StopCoroutine(_delayedSavesUpdateCoroutine);
			_delayedSavesUpdateCoroutine = null;
		}
	}

	private void AskDeleteParty(IGameSaveData data)
	{
		UIConfirmationBoxManager instance = Singleton<UIConfirmationBoxManager>.Instance;
		string translation = LocalizationManager.GetTranslation("GUI_DELETE_PARTY_CONFIRMATION");
		UnityAction onActionConfirmed = delegate
		{
			DeleteParty(data);
		};
		INavigationOperation toPreviousState = NavigationOperation.ToPreviousState;
		instance.ShowGenericConfirmation(null, translation, onActionConfirmed, null, null, null, null, showHeader: true, enableSoftlockReport: false, toPreviousState);
	}

	private void DeleteParty(IGameSaveData data)
	{
		StopAllCoroutines();
		service.DeleteGame(data);
		UILoadGameSlot uILoadGameSlot = slots[data];
		slots.Remove(data);
		uILoadGameSlot.gameObject.SetActive(value: false);
		if (InputManager.GamePadInUse)
		{
			Singleton<UINavigation>.Instance.NavigationManager.TrySelectFirstIn(Singleton<UINavigation>.Instance.NavigationManager.CurrentNavigationRoot);
		}
	}

	private IEnumerator WaitFocusSlot(int slot)
	{
		yield return null;
		FocusSlot(slot);
	}

	public void OnDLCEnabledGamepad(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		_currentDlcCount = GetDlcCount();
		service.EnableDLC(data, dlcToAdd);
		LoadSavedGames(service.GetSaves());
		scroll.ScrollToTop();
		if (_delayedSavesUpdateCoroutine != null)
		{
			CoroutineHelper.instance.StopCoroutine(_delayedSavesUpdateCoroutine);
		}
		_delayedSavesUpdateCoroutine = CoroutineHelper.RunCoroutine(DelayedSavesUpdate());
	}

	private IEnumerator DelayedSavesUpdate()
	{
		while (_currentDlcCount == GetDlcCount())
		{
			yield return _waitForSavesUpdate;
		}
		LoadSavedGames(service.GetSaves());
		scroll.ScrollToTop();
		_delayedSavesUpdateCoroutine = null;
	}

	private int GetDlcCount()
	{
		return service.GetSaves().Sum((IGameSaveData save) => save.GetDLCAvailables().Count((DLCRegistry.EDLCKey dlc) => save.IsDLCActive(dlc)));
	}

	private void EnableDLC(IGameSaveData data, DLCRegistry.EDLCKey dlcToAdd)
	{
		UnityAction onActionConfirmed = delegate
		{
			OnDLCEnabledGamepad(data, dlcToAdd);
			enableDLCAnimator.Play(dlcToAdd);
		};
		Singleton<UIConfirmationBoxManager>.Instance.ShowGenericConfirmation(LocalizationManager.GetTranslation("GUI_ENABLE_DLC"), LocalizationManager.GetTranslation("GUI_ENABLE_DLC_CONFIRMATION"), onActionConfirmed, null, "GUI_ENABLE");
	}

	private void LoadParty(IGameSaveData data)
	{
		window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
		service.LoadGame(data);
	}

	private void StopAnimations()
	{
		ClearTempAvatarStorage();
		StopAllCoroutines();
		showAnimator.Stop();
	}

	private void FocusSlot(int slot)
	{
		StopAllCoroutines();
		focusedSlot = slot;
		if (controllerArea.IsFocused && slot >= 0)
		{
			EventSystem.current.SetSelectedGameObject(slots.Values.OrderBy((UILoadGameSlot it) => it.transform.GetSiblingIndex()).ToList()[focusedSlot].gameObject);
		}
	}
}
