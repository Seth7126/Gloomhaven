#define ENABLE_LOGS
using System;
using System.Collections;
using GLOOM;
using GLOOM.MainMenu;
using JetBrains.Annotations;
using Photon.Bolt;
using Platforms;
using Platforms.Social;
using SM.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace FFSNet;

[Preserve]
public class NetworkBehaviour : MonoBehaviour
{
	[SerializeField]
	private NetworkManager networkManager;

	[Header("Debug Shortcuts")]
	[SerializeField]
	private Key toggleServerShortcut = Key.F9;

	[SerializeField]
	private Key toggleClientShortcut = Key.F10;

	[SerializeField]
	private Key forceDesyncShortcut = Key.F11;

	private bool playerEntityRequestSent;

	private bool _isPrivilegesAcquireInProcess;

	private bool _isAcquiringRequiredPrivilegesFailed;

	private bool _newLeaveAttemptRequired;

	private IEnumerator _leaveSessionCor;

	[UsedImplicitly]
	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		FFSNetwork.Initialize(networkManager, this);
		SceneManager.sceneLoaded += OnSceneLoaded;
		_ = PlatformLayer.Instance.IsDelayedInit;
		PlatformLayer.Platform.OnApplicationFocus += CheckIfPlayerIsSignedInOnFocusRegained;
		PlatformLayer.Platform.PlatformSocial.EventAttemptJoinOrCreateSession += OnAttemptJoinOrCreateSession;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		_ = PlatformLayer.Instance.IsDelayedInit;
		PlatformLayer.Platform.OnApplicationFocus -= CheckIfPlayerIsSignedInOnFocusRegained;
		PlatformLayer.Platform.PlatformSocial.EventAttemptJoinOrCreateSession -= OnAttemptJoinOrCreateSession;
	}

	private void CheckIfPlayerIsSignedInOnFocusRegained()
	{
		LogUtils.Log("[NetworkBehaviour.cs] CheckIfPlayerIsSignedInOnFocusRegained() Application just got focus back... Checking for PS compliance with TRC 5063.");
		string platformID = PlatformLayer.Instance.PlatformID;
		if (UIMultiplayerJoinSessionWindow.s_This != null && UIMultiplayerJoinSessionWindow.s_This.CurrentConnectionState != ConnectionState.None && platformID.StartsWith("GameCore", StringComparison.InvariantCulture))
		{
			UIMultiplayerJoinSessionWindow.s_This.OnConnectionFailed(ConnectionErrorCode.SessionShutDown);
		}
		else if (!platformID.StartsWith("PlayStation", StringComparison.InvariantCulture))
		{
			LogUtils.Log("[NetworkBehaviour.cs] CheckIfPlayerIsSignedInOnFocusRegained() Current platform is: " + platformID + ". This is not a PS platform. Skipping further checking.");
		}
		else if (!FFSNetwork.IsOnline)
		{
			LogUtils.Log("[NetworkBehaviour.cs] CheckIfPlayerIsSignedInOnFocusRegained() Player is not online. Skipping further checking.");
		}
		else if (!KickPlayerIfHeIsNotSignedIn())
		{
			Invoke("KickPlayerIfHeIsNotSignedIn", 1f);
		}
	}

	private bool KickPlayerIfHeIsNotSignedIn()
	{
		string platformDisplayName = PlatformLayer.Platform.UserManagement.GetCurrentUser().GetPlatformDisplayName();
		string platformUniqueUserID = PlatformLayer.Platform.UserManagement.GetCurrentUser().GetPlatformUniqueUserID();
		string platformNetworkAccountUserID = PlatformLayer.Platform.UserManagement.GetCurrentUser().GetPlatformNetworkAccountUserID();
		bool flag = PlatformLayer.Platform.UserManagement.GetCurrentUser().IsSignedInOnline();
		LogUtils.Log($"[NetworkBehaviour.cs] KickPlayerIfHeIsNotSignedIn() User details before userData reset: Player ({platformDisplayName} / {platformUniqueUserID} / {platformNetworkAccountUserID} / {flag})");
		PlatformLayer.Platform.UserManagement.ResetCurrentUser();
		platformDisplayName = PlatformLayer.Platform.UserManagement.GetCurrentUser().GetPlatformDisplayName();
		platformUniqueUserID = PlatformLayer.Platform.UserManagement.GetCurrentUser().GetPlatformUniqueUserID();
		platformNetworkAccountUserID = PlatformLayer.Platform.UserManagement.GetCurrentUser().GetPlatformNetworkAccountUserID();
		flag = PlatformLayer.Platform.UserManagement.GetCurrentUser().IsSignedInOnline();
		LogUtils.Log($"[NetworkBehaviour.cs] KickPlayerIfHeIsNotSignedIn() User details after userData reset: Player ({platformDisplayName} / {platformUniqueUserID} / {platformNetworkAccountUserID} / {flag})");
		if (flag)
		{
			LogUtils.Log("[NetworkBehaviour.cs] KickPlayerIfHeIsNotSignedIn() Player is SignedInOnline. Everything is OK. Exiting check.");
			return false;
		}
		if (FFSNetwork.IsHost)
		{
			LogUtils.LogWarning("[NetworkBehaviour.cs] KickPlayerIfHeIsNotSignedIn() Player is not SignedInOnline! and is a Host! Closing Multiplayer Session!");
			global::Singleton<UIMultiplayerEscSubmenu>.Instance.EndSession();
			global::Singleton<UIMultiplayerEscSubmenu>.Instance.Hide();
			PlatformLayer.Platform.PlatformSocial.DeleteActivityAsync(null);
			string translation = LocalizationManager.GetTranslation("Consoles/ERROR_SIGN_OUT_FROM_PSN_HOST");
			PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, translation, null);
		}
		else
		{
			LogUtils.LogWarning("[NetworkBehaviour.cs] KickPlayerIfHeIsNotSignedIn() Player is not SignedInOnline! and is a Client! Sending him to main menu!");
			global::Singleton<ESCMenu>.Instance.LoadMainMenu(skipConfirmation: true);
			PlatformLayer.Platform.PlatformSocial.DeleteActivityAsync(null);
			string translation = LocalizationManager.GetTranslation("Consoles/ERROR_SIGN_OUT_FROM_PSN_CLIENT");
			PlatformLayer.Message.ShowSystemMessage(IPlatformMessage.MessageType.Ok, translation, null);
		}
		return true;
	}

	[UsedImplicitly]
	private void OnFocusRegained()
	{
		if (FFSNetwork.IsOnline && !_isPrivilegesAcquireInProcess && !_isAcquiringRequiredPrivilegesFailed)
		{
			_isPrivilegesAcquireInProcess = true;
			PlatformLayer.Networking.AskUserToAcquirePrivilegeAsync(Privilege.Multiplayer, OnAcquireMultiplayerPrivilege);
		}
		void OnAcquireCrossPlayPrivilege(OperationResult operationResult)
		{
			if (operationResult != OperationResult.Success)
			{
				if (FFSNetwork.IsOnline)
				{
					if (FFSNetwork.IsHost)
					{
						global::Singleton<UIMultiplayerEscSubmenu>.Instance.EndSession();
					}
					else
					{
						global::Singleton<ESCMenu>.Instance.LoadMainMenu(skipConfirmation: true);
					}
					_isAcquiringRequiredPrivilegesFailed = true;
					_isPrivilegesAcquireInProcess = false;
				}
			}
			else
			{
				_isPrivilegesAcquireInProcess = false;
			}
		}
		void OnAcquireMultiplayerPrivilege(OperationResult operationResult)
		{
			if (operationResult == OperationResult.Success)
			{
				if (IsCrossPlayInSession())
				{
					PlatformLayer.Networking.AskUserToAcquirePrivilegeAsync(Privilege.CrossPlay, OnAcquireCrossPlayPrivilege);
				}
				else
				{
					_isPrivilegesAcquireInProcess = false;
				}
			}
			else if (FFSNetwork.IsOnline)
			{
				if (FFSNetwork.IsHost)
				{
					global::Singleton<UIMultiplayerEscSubmenu>.Instance.EndSession();
				}
				else
				{
					global::Singleton<ESCMenu>.Instance.LoadMainMenu(skipConfirmation: true);
				}
				_isAcquiringRequiredPrivilegesFailed = true;
				_isPrivilegesAcquireInProcess = false;
			}
		}
	}

	private bool IsCrossPlayInSession()
	{
		if (!FFSNetwork.IsHost)
		{
			return ((UserToken)BoltNetwork.Server.ConnectToken).CrossplayEnabled;
		}
		return SaveData.Instance.Global.CrossplayEnabled;
	}

	public bool IsCrossplaySession()
	{
		if (!FFSNetwork.IsHost)
		{
			return ((GameToken)BoltNetwork.Server.AcceptToken).IsCrossplaySession;
		}
		return SaveData.Instance.Global.CrossplayEnabled;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (FFSNetwork.IsClient && !playerEntityRequestSent)
		{
			playerEntityRequestSent = true;
			Synchronizer.RequestPlayerEntity();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (FFSNetwork.IsOnline)
		{
			Console.LogWarning("Application " + (hasFocus ? " GAINED " : " LOST ") + " focus.");
		}
	}

	private void Update()
	{
		if (PlayerRegistry.CheckForReceivedAvatarUpdate())
		{
			global::Singleton<UIMultiplayerEscSubmenu>.Instance.Refresh();
		}
	}

	private void ProcessDebugActions()
	{
		if (InputManager.GetKeyDown(forceDesyncShortcut) && FFSNetwork.IsOnline)
		{
			ControllableRegistry.Print();
			NewPartyDisplayUI.PartyDisplay?.PrintBenchedCharacters();
			PlayerRegistry.PrintAllPlayerControllables();
		}
	}

	public void Reset()
	{
		_isAcquiringRequiredPrivilegesFailed = false;
		playerEntityRequestSent = false;
	}

	private void OnAttemptJoinOrCreateSession()
	{
		StopLeaveSessionCor();
	}

	private void StopLeaveSessionCor()
	{
		if (_leaveSessionCor != null)
		{
			_newLeaveAttemptRequired = false;
			StopCoroutine(_leaveSessionCor);
			_leaveSessionCor = null;
		}
	}

	private void LeaveSessionCompleted(OperationResult operationResult)
	{
		if (operationResult == OperationResult.Success)
		{
			StopLeaveSessionCor();
		}
		else if (_leaveSessionCor != null)
		{
			_newLeaveAttemptRequired = true;
		}
	}

	private IEnumerator LeaveSession()
	{
		while (true)
		{
			string pSPlayerSession = PlatformLayer.Platform.PlatformSocial.GetPSPlayerSession();
			if (!IsSessionShouldBeTerminated(pSPlayerSession))
			{
				break;
			}
			if (_newLeaveAttemptRequired)
			{
				_newLeaveAttemptRequired = false;
				PlatformLayer.Networking.LeaveSession(LeaveSessionCompleted);
			}
			yield return new WaitForSeconds(1f);
		}
		_newLeaveAttemptRequired = false;
		_leaveSessionCor = null;
	}

	private bool IsSessionShouldBeTerminated(string session)
	{
		if (!string.IsNullOrEmpty(session))
		{
			return !session.StartsWith("-", StringComparison.Ordinal);
		}
		return false;
	}
}
