using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIMultiplayerSelectPlayerScreen : Singleton<UIMultiplayerSelectPlayerScreen>
{
	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private List<UIMultiplayerPlayerOption> slots;

	[SerializeField]
	private VerticalPointerUI pointer;

	[SerializeField]
	private UIMultiplayerConfirmationBox confirmationBox;

	[SerializeField]
	private float defaultPositionX;

	[SerializeField]
	private LocalHotkeys _localHotkeys;

	private NetworkPlayer currentSelection;

	private Dictionary<NetworkPlayer, UIMultiplayerPlayerOption> assignedSlots = new Dictionary<NetworkPlayer, UIMultiplayerPlayerOption>();

	private UIWindow window;

	private INetworkHeroAssignService service;

	private string characterID;

	private string characterName;

	private Action onHiddenCallback;

	private Action<NetworkPlayer> onSelectedPlayer;

	private int m_CurrentSlotIndex;

	private NetworkPlayer _currentHoveredPlayer;

	public bool IsOpen => window.IsOpen;

	public string Id { get; private set; }

	public INetworkHeroAssignService Service => service;

	public IHotkeySession HotkeySession { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		slots = slots.OrderByDescending((UIMultiplayerPlayerOption it) => it.transform.GetSiblingIndex()).ToList();
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(delegate
		{
			controllerArea.Destroy();
			onHiddenCallback?.Invoke();
		});
		window.onShown.AddListener(OnWindowShown);
		service = GetComponent<INetworkHeroAssignService>();
		int num = 0;
		foreach (UIMultiplayerPlayerOption slot in slots)
		{
			slot.OnSelect.AddListener(OnSelectedPlayer);
			slot.OnRemove.AddListener(OnSelectedRemovePlayer);
			slot.OnHoveredPlayer.AddListener(OnHoveredPlayer);
			slot.SlotIndex = num;
			num++;
		}
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		HotkeySession = _localHotkeys.GetSession();
		InitGamepadInput();
	}

	private void OnWindowShown()
	{
		InitGamepadInput();
	}

	private void InitGamepadInput()
	{
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_KICK_PLAYER, OnRemoveClicked).AddBlocker(new UIWindowOpenKeyActionBlocker(window)));
	}

	private void OnRemoveClicked()
	{
		if (!(_currentHoveredPlayer != null))
		{
			return;
		}
		NetworkPlayer currentPlayer = PlayerRegistry.MyPlayer;
		NetworkPlayer selectedPlayer = _currentHoveredPlayer;
		if (!(currentPlayer != selectedPlayer))
		{
			return;
		}
		if (PlatformLayer.Instance.IsConsole)
		{
			confirmationBox.ShowConfirmation(selectedPlayer, delegate
			{
				OnKickButtonPressed(selectedPlayer);
			}, delegate
			{
				OnReportButtonPressed(currentPlayer, selectedPlayer);
			});
		}
		else
		{
			confirmationBox.ShowConfirmation(selectedPlayer, delegate
			{
				OnKickButtonPressed(selectedPlayer);
			});
		}
		void OnKickButtonPressed(NetworkPlayer player)
		{
			service.RemoveClient(player, sendKickMessage: true);
		}
		void OnReportButtonPressed(NetworkPlayer reporter, NetworkPlayer reported)
		{
			service.ReportClient(reported, delegate
			{
				OnReportCallback(reporter, reported);
			});
		}
		void OnReportCallback(NetworkPlayer reporter, NetworkPlayer reported)
		{
			UIMultiplayerNotifications.ShowPlayerReported(reporter.UserNameWithPlatformIcon(), reported.UserNameWithPlatformIcon());
			if (!reporter.IsClient)
			{
				service.RemoveClient(reported, sendKickMessage: true);
			}
		}
	}

	private void DeinitGamepadInput()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_KICK_PLAYER, OnRemoveClicked);
		}
	}

	private void OnHoveredPlayer(bool isHovered, NetworkPlayer arg1)
	{
		if (isHovered)
		{
			_currentHoveredPlayer = arg1;
		}
	}

	private void EnableNavigation()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerSelectPlayer);
	}

	private void DisableNavigation()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerOnlineContainerWithSelected);
	}

	private void SelectFirstElement()
	{
		if (currentSelection == null)
		{
			assignedSlots.Values.OrderBy((UIMultiplayerPlayerOption it) => it.transform.GetSiblingIndex()).First().EnableNavigation(select: true);
		}
		else
		{
			assignedSlots[currentSelection].EnableNavigation(select: true);
		}
	}

	private void OnSelectedPlayer(NetworkPlayer player)
	{
		if (currentSelection == null || currentSelection != player)
		{
			if (currentSelection != null)
			{
				assignedSlots[currentSelection].SetSelected(selected: false);
			}
			currentSelection = player;
			Synchronizer.SendSideAction(GameActionType.AssignSlot, null, canBeUnreliable: false, sendToHostOnly: false, 0, player.PlayerID, m_CurrentSlotIndex);
			if (characterID == null)
			{
				service.AssignSlotToPlayer(player, m_CurrentSlotIndex);
				UIMultiplayerNotifications.ShowPlayerControlsCharacter(player, string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HERO_SLOT"), m_CurrentSlotIndex + 1));
			}
			else
			{
				service.AssignHeroToPlayer(player, characterID, characterName, m_CurrentSlotIndex);
			}
			onSelectedPlayer?.Invoke(player);
		}
	}

	private void OnSelectedRemovePlayer(NetworkPlayer player)
	{
		if (currentSelection == player && player.IsClient)
		{
			confirmationBox.ShowConfirmation(player, delegate
			{
				service.RemoveClient(player, sendKickMessage: true);
			});
		}
	}

	public void RefreshPlayers()
	{
		if (window.IsOpen)
		{
			List<NetworkPlayer> list = (from w in service.GetAllPlayers()
				where w.PlayerReadyForAssignment || w.PlayerID == PlayerRegistry.HostPlayerID
				select w).ToList();
			if (currentSelection != null && !list.Contains(currentSelection))
			{
				assignedSlots[currentSelection].SetSelected(selected: false);
				currentSelection = null;
			}
			slots.AddRange(assignedSlots.Values.OrderByDescending((UIMultiplayerPlayerOption it) => it.transform.GetSiblingIndex()));
			assignedSlots.Clear();
			for (int num = 0; num < list.Count; num++)
			{
				AddPlayer(list[num]);
			}
			for (int num2 = 0; num2 < slots.Count; num2++)
			{
				slots[num2].gameObject.SetActive(value: false);
			}
			if (controllerArea.IsFocused)
			{
				assignedSlots[(currentSelection == null) ? list[0] : currentSelection].EnableNavigation(select: true);
			}
		}
	}

	public void AddPlayer(NetworkPlayer player)
	{
		if (window.IsOpen && !assignedSlots.ContainsKey(player))
		{
			UIMultiplayerPlayerOption slot = slots[slots.Count - 1];
			slots.RemoveAt(slots.Count - 1);
			assignedSlots[player] = slot;
			slot.SetPlayer(player, player == currentSelection);
			slot.gameObject.SetActive(value: true);
			int num = (from it in slots.Concat(assignedSlots.Values)
				orderby it.transform.GetSiblingIndex() descending
				select it).ToList().FindIndex((UIMultiplayerPlayerOption x) => x == slot);
			if (controllerArea.IsFocused)
			{
				slot.EnableNavigation();
			}
			FFSNet.Console.LogCoreInfo(player.Username + " (PlayerID: " + player.PlayerID + ") was ASSIGNED slot #" + num, customFlag: true);
		}
	}

	public void RemovePlayer(NetworkPlayer player)
	{
		if (window.IsOpen && assignedSlots.ContainsKey(player))
		{
			UIMultiplayerPlayerOption slot = assignedSlots[player];
			assignedSlots.Remove(player);
			slot.gameObject.SetActive(value: false);
			slots.Add(slot);
			if (currentSelection == player)
			{
				slot.SetSelected(selected: false);
				currentSelection = null;
			}
			int num = (from it in slots.Concat(assignedSlots.Values)
				orderby it.transform.GetSiblingIndex() descending
				select it).ToList().FindIndex((UIMultiplayerPlayerOption x) => x == slot);
			if (controllerArea.IsFocused && EventSystem.current.currentSelectedGameObject == null)
			{
				SelectFirstElement();
			}
			FFSNet.Console.LogCoreInfo(player.Username + " (PlayerID: " + player.PlayerID + ") was UNASSIGNED from slot #" + num, customFlag: true);
		}
	}

	public void SetSelected(NetworkPlayer selection)
	{
		if (currentSelection != null)
		{
			assignedSlots[currentSelection].SetSelected(selected: false);
		}
		currentSelection = selection;
		if (currentSelection != null)
		{
			assignedSlots[currentSelection].SetSelected(selected: true);
		}
	}

	public void Show(string id, string characterID, string characterName, int slotIndex, NetworkPlayer selection, RectTransform point, Action onHidden = null, Action<NetworkPlayer> onSelectedPlayer = null, float? displacement = null)
	{
		Id = id;
		this.characterID = characterID;
		this.characterName = characterName;
		onHiddenCallback = onHidden;
		this.onSelectedPlayer = onSelectedPlayer;
		m_CurrentSlotIndex = slotIndex;
		(base.transform as RectTransform).anchoredPosition = new Vector2(displacement ?? defaultPositionX, 0f);
		window.Show();
		currentSelection = selection;
		RefreshPlayers();
		pointer.PointAt(point);
		RemoveEvents();
		INetworkHeroAssignService networkHeroAssignService = service;
		networkHeroAssignService.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(networkHeroAssignService.OnPlayerLeft, new PlayersChangedEvent(RemovePlayer));
		INetworkHeroAssignService networkHeroAssignService2 = service;
		networkHeroAssignService2.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(networkHeroAssignService2.OnPlayerLeft, new PlayersChangedEvent(RemovePlayer));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnersChanged));
		controllerArea.Enable();
	}

	private void RemoveEvents()
	{
		INetworkHeroAssignService networkHeroAssignService = service;
		networkHeroAssignService.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(networkHeroAssignService.OnPlayerLeft, new PlayersChangedEvent(RemovePlayer));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllableOwnersChanged));
	}

	private void OnControllableOwnersChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		string text = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(controllable.ID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(controllable.ID));
		if (characterID == text)
		{
			SetSelected(newController);
		}
	}

	public void Hide()
	{
		RemoveEvents();
		window.Hide();
		confirmationBox.Hide(isToPreviousStateRequired: false);
		if (currentSelection != null)
		{
			assignedSlots[currentSelection].SetSelected(selected: false);
		}
		currentSelection = null;
	}

	protected override void OnDestroy()
	{
		RemoveEvents();
		DeinitGamepadInput();
		base.OnDestroy();
	}

	public void ServerACKClientLoadedInAndReady(GameAction action)
	{
		Task.Run(async delegate
		{
			while (PlayerRegistry.IsProfanityCheckInProcess)
			{
				await Task.Delay(50);
			}
			UnityMainThreadDispatcher.Instance().Enqueue(delegate
			{
				NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
				if (player != null)
				{
					AddPlayer(player);
					return;
				}
				throw new Exception("Error ACKing client load-in. NetworkPlayer returns null.");
			});
		});
	}
}
