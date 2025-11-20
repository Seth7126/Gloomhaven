#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GLOOM;
using Platforms;
using Platforms.Social;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Events;

public class PlatformNetworking : MonoBehaviour, IPlatformNetworking
{
	public INetworkSessionService EpicCurrentSessionService;

	private IEnumerator m_EPICWaitForLoadingRoutine;

	public string EPICPendingInviteLobbyID;

	private readonly UiNavigationBlocker _navigationBlocker = new UiNavigationBlocker("PlatformNetworking");

	private const string c_SessionIdLobbyKey = "SessionID";

	private bool m_CommandLineInviteProcessed;

	private string m_PendingInviteLobbyID;

	private UnityAction<string> m_JoinCompleteAction;

	private Lobby? m_CurrentSteamLobby;

	private INetworkSessionService m_CurrentSessionService;

	private bool m_TryingToJoinSession;

	private IEnumerator m_WaitForLoadingRoutine;

	public bool EPICInvitesSupported => true;

	public bool HasEPICInvitePending => EPICPendingInviteLobbyID.IsNOTNullOrEmpty();

	public bool PlatformInvitesSupported => true;

	public bool HasInvitePending => m_PendingInviteLobbyID.IsNOTNullOrEmpty();

	public void JoinOnPendingEPICInvite(UnityAction<string> joinCompleteAction)
	{
		string ePICPendingInviteLobbyID = EPICPendingInviteLobbyID;
		EPICPendingInviteLobbyID = null;
		joinCompleteAction(ePICPendingInviteLobbyID);
	}

	public void RegisterNetworkSessionServiceWithEpic(INetworkSessionService serviceToRegister)
	{
		EpicCurrentSessionService = serviceToRegister;
	}

	public void EpicGameLobbyJoinRequestedCallback()
	{
		Singleton<UIMultiplayerEscSubmenu>.Instance.signInConfirmationBox.ShowConfirmation(delegate
		{
			EpicJoinGame();
		});
	}

	public void EpicJoinGame()
	{
		PlayEveryWare.EpicOnlineServices.Samples.Session currentInvite = EOSManager.Instance.GetOrCreateManager<EOSSessionsManager>().GetCurrentInvite();
		EPICPendingInviteLobbyID = null;
		if (currentInvite != null)
		{
			foreach (SessionAttribute attribute in currentInvite.Attributes)
			{
				if (attribute.Key == "PHOTONKEY")
				{
					EPICPendingInviteLobbyID = attribute.AsString;
				}
			}
		}
		if (!HasEPICInvitePending)
		{
			return;
		}
		UnityAction unityAction = delegate
		{
			Main.Unpause3DWorld(forceUnpause: true);
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
			{
				LevelEditorController s_Instance = LevelEditorController.s_Instance;
				if ((object)s_Instance != null && s_Instance.IsEditing)
				{
					LevelEditorController.s_Instance.QuitLevelEditor();
				}
				if (LevelEventsController.s_EventsControllerActive)
				{
					Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
				}
				Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: false);
				UIManager.LoadMainMenu();
			}
			else
			{
				LevelEditorController s_Instance2 = LevelEditorController.s_Instance;
				if ((object)s_Instance2 != null && s_Instance2.IsEditing)
				{
					LevelEditorController.s_Instance.QuitLevelEditor();
				}
				UIManager.LoadMainMenu();
			}
		};
		if (SceneController.Instance.IsLoading)
		{
			if (m_EPICWaitForLoadingRoutine != null)
			{
				StopCoroutine(m_EPICWaitForLoadingRoutine);
				m_EPICWaitForLoadingRoutine = null;
			}
			m_EPICWaitForLoadingRoutine = AcceptEPICInviteAfterFinishedLoading(unityAction);
			StartCoroutine(m_EPICWaitForLoadingRoutine);
		}
		else
		{
			unityAction?.Invoke();
		}
	}

	private IEnumerator AcceptEPICInviteAfterFinishedLoading(UnityAction inviteAcceptAction)
	{
		while (SceneController.Instance.IsLoading)
		{
			yield return null;
		}
		inviteAcceptAction?.Invoke();
	}

	private IEnumerator Start()
	{
		while (!PlatformLayer.Initialised)
		{
			yield return null;
		}
		Initialize(PlatformLayer.Platform);
	}

	public void CheckNetworkAvailabilityAsync(Action<bool> onComplete)
	{
		bool flag = PlatformLayer.Platform.IsNetworkConnectionAvailable();
		if (!flag)
		{
			Debug.LogWarning("[PlatformNetworking] CheckNetworkAvailabilityAsync failed: No Internet!");
			PlatformLayer.Platform.PlatformMessage.ShowSystemMessage(IPlatformMessage.MessageType.Ok, LocalizationManager.GetTranslation("ERROR_MULTIPLAYER_00016"), LocalizationManager.GetTranslation("Consoles/ERROR_MULTIPLAYER_00117"), delegate
			{
			});
		}
		onComplete?.Invoke(flag);
	}

	public void CheckForPrivilegeValidityAsync(Privilege privilege, Action<bool> onPermissionValidation, PrivilegePlatform platformsToCheck, bool askUserToAcquirePrivilege = true)
	{
		Debug.Log("[PlatformNetworking] CheckForPrivilegeValidityAsync is about to check " + privilege.ToString() + " privilege. \n" + Environment.StackTrace);
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(_navigationBlocker);
		PlatformLayer.Networking.GetCurrentUserPrivilegesAsync(OnGetCurrentUserPrivileges, platformsToCheck, privilege);
		void OnAskUserToAcquirePrivileges(OperationResult operationResult)
		{
			if (operationResult == OperationResult.Success)
			{
				onPermissionValidation?.Invoke(obj: true);
			}
			else
			{
				onPermissionValidation?.Invoke(obj: false);
			}
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navigationBlocker);
		}
		void OnGetCurrentUserPrivileges(OperationResult operationResult, bool privilegeProvided)
		{
			if (operationResult == OperationResult.Success)
			{
				if (privilegeProvided || !askUserToAcquirePrivilege)
				{
					onPermissionValidation?.Invoke(obj: true);
					InputManager.RequestEnableInput(this, EKeyActionTag.All);
					Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navigationBlocker);
				}
				else
				{
					PlatformLayer.Networking.AskUserToAcquirePrivilegeAsync(privilege, OnAskUserToAcquirePrivileges);
				}
			}
			else
			{
				onPermissionValidation?.Invoke(obj: false);
				InputManager.RequestEnableInput(this, EKeyActionTag.All);
				Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navigationBlocker);
			}
		}
	}

	public void Initialize(IPlatform platform)
	{
		SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
		SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChangedCallback;
		SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
	}

	public void Shutdown()
	{
		LeaveLobby();
	}

	public void OpenPlatformInviteOverlay()
	{
		if (SteamClient.IsValid)
		{
			AsyncOpenSteamFriendList();
		}
	}

	public void JoinSession(string sessionId, Action<OperationResult> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.Success);
	}

	public void CreateClientSession(string sessionId, Action<OperationResult> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.Success);
	}

	public bool CheckCommandLineForPendingInvite()
	{
		if (SteamClient.IsValid)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if (commandLineArgs.Count() == 3 && commandLineArgs[1] == "+connect_lobby" && !m_CommandLineInviteProcessed)
			{
				m_CommandLineInviteProcessed = true;
				if (!m_PendingInviteLobbyID.IsNOTNullOrEmpty())
				{
					m_PendingInviteLobbyID = commandLineArgs[2];
					return true;
				}
			}
		}
		return false;
	}

	public void JoinOnPendingInvite(UnityAction<string> joinCompleteAction)
	{
		m_JoinCompleteAction = joinCompleteAction;
		JoinLobbyFromPendingInvite();
	}

	public void RegisterNetworkSessionService(INetworkSessionService sessionService, INetworkHeroAssignService heroAssignService)
	{
		m_CurrentSessionService = sessionService;
	}

	public void LeaveSession(Action<OperationResult> resultCallback)
	{
		LeaveLobby();
		resultCallback?.Invoke(OperationResult.Success);
	}

	public byte[] GetRecentPlayerKey()
	{
		return null;
	}

	public void InviteUser(string userId, Action<OperationResult> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.Success);
	}

	public void GetCurrentUserPrivilegesAsync(Action<OperationResult, bool> resultCallback, PrivilegePlatform privilegePlatform, Privilege target)
	{
		resultCallback?.Invoke(OperationResult.Success, arg2: true);
	}

	public async Task AsyncOpenSteamFriendList()
	{
		if (!m_CurrentSteamLobby.HasValue)
		{
			Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(4);
			if (!lobby.HasValue)
			{
				Debug.LogFormat("[PLATFORM LAYER] - AsyncOpenSteamFriendList | Create Lobby Failed");
				return;
			}
			m_CurrentSteamLobby = lobby.Value;
			m_CurrentSteamLobby?.SetData("SessionID", m_CurrentSessionService.GetInviteCode());
			m_CurrentSteamLobby?.SetInvisible();
		}
		SteamFriends.OpenGameInviteOverlay(m_CurrentSteamLobby?.Id ?? ((SteamId)0uL));
	}

	private async Task JoinLobbyFromPendingInvite()
	{
		if (m_PendingInviteLobbyID.IsNOTNullOrEmpty())
		{
			SteamId lobbyId = new SteamId
			{
				Value = ulong.Parse(m_PendingInviteLobbyID)
			};
			m_PendingInviteLobbyID = null;
			m_TryingToJoinSession = true;
			await SteamMatchmaking.JoinLobbyAsync(lobbyId);
		}
		m_PendingInviteLobbyID = null;
	}

	private void OnLobbyDataChangedCallback(Lobby lobby)
	{
		lobby.GetData("SessionID");
	}

	private void OnLobbyEnteredCallback(Lobby lobby)
	{
		string data = lobby.GetData("SessionID");
		if (m_TryingToJoinSession && !data.IsNullOrEmpty())
		{
			m_CurrentSteamLobby = lobby;
			m_JoinCompleteAction?.Invoke(data);
			m_JoinCompleteAction = null;
			m_TryingToJoinSession = false;
		}
	}

	private void OnGameLobbyJoinRequestedCallback(Lobby lobby, SteamId id)
	{
		UnityAction unityAction = delegate
		{
			m_PendingInviteLobbyID = lobby.Id.ToString();
			Main.Unpause3DWorld(forceUnpause: true);
			if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
			{
				LevelEditorController s_Instance = LevelEditorController.s_Instance;
				if ((object)s_Instance != null && s_Instance.IsEditing)
				{
					LevelEditorController.s_Instance.QuitLevelEditor();
				}
				if (LevelEventsController.s_EventsControllerActive)
				{
					Singleton<LevelEventsController>.Instance?.CompleteLevel(completionSuccess: false, userQuit: true);
				}
				Choreographer.s_Choreographer.m_GameScenarioScreen.SetActive(value: false);
				UIManager.LoadMainMenu();
			}
			else
			{
				LevelEditorController s_Instance2 = LevelEditorController.s_Instance;
				if ((object)s_Instance2 != null && s_Instance2.IsEditing)
				{
					LevelEditorController.s_Instance.QuitLevelEditor();
				}
				UIManager.LoadMainMenu();
			}
		};
		if (SceneController.Instance.IsLoading)
		{
			if (m_WaitForLoadingRoutine != null)
			{
				StopCoroutine(m_WaitForLoadingRoutine);
				m_WaitForLoadingRoutine = null;
			}
			m_WaitForLoadingRoutine = AcceptInviteAfterFinishedLoading(unityAction);
			StartCoroutine(m_WaitForLoadingRoutine);
		}
		else
		{
			unityAction?.Invoke();
		}
	}

	private void LeaveLobby()
	{
		if (m_CurrentSteamLobby.HasValue)
		{
			m_CurrentSteamLobby?.Leave();
			m_CurrentSteamLobby = null;
		}
		m_TryingToJoinSession = false;
		m_JoinCompleteAction = null;
	}

	public void CheckNetworkSubscription(Action<bool> callback)
	{
		callback?.Invoke(obj: true);
	}

	public void SendUserToBuyNetworkSubscription()
	{
	}

	public void AskUserToAcquirePrivilegeAsync(Privilege privilege, Action<OperationResult> resultCallback)
	{
		resultCallback?.Invoke(OperationResult.Success);
	}

	public void GetPermissionsTowardsPlatformUsersAsync(HashSet<string> usersIds, Action<OperationResult, Dictionary<string, Dictionary<Permission, List<PermissionOperationResult>>>> resultCallback)
	{
		Dictionary<Permission, List<PermissionOperationResult>> allPermissions = ((Permission[])Enum.GetValues(typeof(Permission))).ToDictionary((Permission permission) => permission, (Permission _) => new List<PermissionOperationResult> { PermissionOperationResult.Success });
		Dictionary<string, Dictionary<Permission, List<PermissionOperationResult>>> arg = usersIds.ToDictionary((string user) => user, (string _) => allPermissions);
		resultCallback?.Invoke(OperationResult.Success, arg);
	}

	private IEnumerator AcceptInviteAfterFinishedLoading(UnityAction inviteAcceptAction)
	{
		while (SceneController.Instance.IsLoading)
		{
			yield return null;
		}
		inviteAcceptAction?.Invoke();
	}
}
