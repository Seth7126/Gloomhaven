#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using JetBrains.Annotations;
using MapRuleLibrary.Party;
using SM.Utils;
using ScenarioRuleLibrary;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	private class ElementPreviousState
	{
		private Transform parent;

		private Vector2 anchoredPosition;

		private Vector3 scale;

		public ElementPreviousState(GameObject gameObject)
		{
			RectTransform component = gameObject.GetComponent<RectTransform>();
			if (component != null)
			{
				anchoredPosition = component.anchoredPosition;
			}
			parent = gameObject.transform.parent;
			scale = gameObject.transform.localScale;
		}

		public void RestoreState(GameObject element)
		{
			element.transform.SetParent(parent, worldPositionStays: true);
			RectTransform component = element.GetComponent<RectTransform>();
			if (component != null)
			{
				component.anchoredPosition = anchoredPosition;
			}
			element.transform.localScale = scale;
		}
	}

	[SerializeField]
	private MissionObjectiveContainer missionObjectiveContainer;

	[SerializeField]
	private ScenarioModifierContainer scenarioModifierContainer;

	[SerializeField]
	private BattleGoalContainer battleGoalContainer;

	[HideInInspector]
	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private List<GraphicRaycaster> graphicRaycasters;

	[SerializeField]
	private AudioSource uiAudioSource;

	[SerializeField]
	private Text debugText;

	[SerializeField]
	private RectTransform highlightHolder;

	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private Camera uiCamera;

	[SerializeField]
	private Canvas uiOverlayCanvas;

	[SerializeField]
	private BaseButtons _baseButtons;

	[Tooltip("Define preferable area in which camera should show Characters and enemies, everything outside of this area could be obscured by UI")]
	[SerializeField]
	private RectTransform m_ScreenVisibleArea;

	private Dictionary<GameObject, ElementPreviousState> highlightedElements = new Dictionary<GameObject, ElementPreviousState>();

	private HashSet<GameObject> elementsLockUI = new HashSet<GameObject>();

	public DialogPopup dialogPopup;

	public static UIManager Instance { get; private set; }

	public MissionObjectiveContainer MissionObjectiveContainer => missionObjectiveContainer;

	public ScenarioModifierContainer ScenarioModifierContainer => scenarioModifierContainer;

	public BattleGoalContainer BattleGoalContainer => battleGoalContainer;

	public Camera UICamera => uiCamera;

	public RectTransform ScreenVisibleArea => m_ScreenVisibleArea;

	public static bool IsPointerOverUI
	{
		get
		{
			if (!EventSystem.current.IsPointerOverGameObject() && (PlatformLayer.Instance.IsConsole || !IsControllerOverUI))
			{
				if (PlatformLayer.Instance.IsConsole)
				{
					return !ControllerInputPointer.IsShown;
				}
				return false;
			}
			return true;
		}
	}

	private static bool IsControllerOverUI
	{
		get
		{
			if (!InputManager.GamePadInUse || (SRDebug.IsInitialized && SRDebug.Instance.IsDebugPanelVisible))
			{
				return false;
			}
			PointerEventData eventData = new PointerEventData(EventSystem.current)
			{
				position = InputManager.CursorPosition
			};
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventData, list);
			return list.Any();
		}
	}

	public Canvas UICanvas => uiOverlayCanvas;

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	public void RunBaseButtonsBlocker()
	{
		if (_baseButtons != null)
		{
			_baseButtons.RunBlocker();
		}
	}

	public void ToggleBurnCardBlock(bool isBlocked)
	{
		if (_baseButtons != null)
		{
			_baseButtons.ToggleBurnOneCardBlock(isBlocked);
		}
	}

	public void ToggleButtonOptions(object requester, bool isEnabled)
	{
		if (_baseButtons != null)
		{
			_baseButtons.Toggle(requester, isEnabled);
		}
	}

	public void HighlightElement(GameObject element, bool fadeEverythingElse = false, bool lockUI = true, bool autoUnhighlightElement = false)
	{
		if (highlightedElements.ContainsKey(element))
		{
			Debug.LogWarning("Element already highlighted " + element.name);
			return;
		}
		highlightedElements.Add(element, new ElementPreviousState(element));
		element.transform.SetParent(highlightHolder, worldPositionStays: true);
		if (fadeEverythingElse)
		{
			if (lockUI)
			{
				RequestToggleLockUI(active: true, element);
			}
			highlightImage.enabled = true;
			if (autoUnhighlightElement)
			{
				StartCoroutine(DelayUnfade(element));
			}
		}
	}

	public void InitScenario(string scenarioID, List<CObjective> winObjectives, List<CObjective> loseObjectives, List<CScenarioModifier> scenarioModifiers, List<CBattleGoalState> battleGoalStates)
	{
		try
		{
			missionObjectiveContainer.Init(winObjectives, loseObjectives);
			scenarioModifierContainer.Init(scenarioID, scenarioModifiers);
			battleGoalContainer.Init(battleGoalStates);
			battleGoalContainer.Hide();
			missionObjectiveContainer.UpdateGold(ScenarioManager.Scenario?.PartyGold ?? 0, animate: false);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("UIManager.InitScenario: using UIManager {0} | StackTrace:{1}", base.name, ex.StackTrace);
			if (missionObjectiveContainer == null)
			{
				Debug.LogError("UIManager.InitScenario: missionObjectiveContainer is null");
			}
			if (scenarioModifierContainer == null)
			{
				Debug.LogError("UIManager.InitScenario: scenarioModifierContainer is null");
			}
			if (battleGoalContainer == null)
			{
				Debug.LogError("UIManager.InitScenario: battleGoalContainer is null");
			}
			if (ScenarioManager.Scenario == null)
			{
				Debug.LogError("UIManager.InitScenario: ScenarioRuleLibrary.ScenarioManager.Scenario is null");
			}
			else
			{
				try
				{
					Debug.Log("UIManager.InitScenario: Party Gold = " + ScenarioManager.Scenario.PartyGold);
				}
				catch
				{
					Debug.LogError("UIManager.InitScenario: Exception retrieving Party Gold value");
				}
			}
			throw ex;
		}
	}

	public void OnGoldValueChanged()
	{
		missionObjectiveContainer.UpdateGold(ScenarioManager.Scenario.PartyGold, animate: true);
	}

	public void UnhighlightElement(GameObject element, bool unlockUI = true)
	{
		if (highlightedElements.ContainsKey(element))
		{
			highlightImage.enabled = false;
			if (unlockUI)
			{
				RequestToggleLockUI(active: false, element);
			}
			highlightedElements[element].RestoreState(element);
			highlightedElements.Remove(element);
		}
	}

	private IEnumerator DelayUnfade(GameObject element)
	{
		yield return Timekeeper.instance.WaitForSeconds(2f);
		UnhighlightElement(element);
	}

	public static GameObject GameObjectUnderPointer(int pointerId = -1)
	{
		return (EventSystem.current.currentInputModule as IInputModulePointer)?.GameObjectUnderPointer(pointerId);
	}

	public static T ComponentUnderPointer<T>() where T : Component
	{
		GameObject gameObject = GameObjectUnderPointer();
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<T>();
	}

	public void PlayUISound(AudioClip audioClip)
	{
		if (audioClip != null)
		{
			uiAudioSource.PlayOneShot(audioClip);
		}
	}

	public void DisplayDebugMessage(string message)
	{
		debugText.text = message;
		debugText.enabled = true;
	}

	public void HideDebugMessage()
	{
		debugText.enabled = false;
	}

	public void ToggleLockUI(bool active)
	{
		graphicRaycaster.enabled = !active;
		for (int i = 0; i < graphicRaycasters.Count; i++)
		{
			graphicRaycasters[i].enabled = !active;
		}
	}

	public void RequestToggleLockUI(bool active, GameObject element)
	{
		if (active)
		{
			elementsLockUI.Add(element);
		}
		else
		{
			elementsLockUI.Remove(element);
		}
		ToggleLockUI(elementsLockUI.Count > 0);
	}

	public static void MakeButtonsInteractable(GameObject go, bool bInteractable)
	{
		Button[] componentsInChildren = go.GetComponentsInChildren<Button>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].interactable = bInteractable;
		}
	}

	public static void RestartScenario()
	{
		SaveData.Instance.Global.StopSpeedUp();
		SceneController.Instance.RestartScenario();
	}

	public static void RestartScenarioFromInitial()
	{
		SaveData.Instance.Global.StopSpeedUp();
		SceneController.Instance.RestartScenarioFromInitial();
	}

	public static void RegenerateAndRestartScenario(Action onFinish = null)
	{
		SaveData.Instance.Global.StopSpeedUp();
		SceneController.Instance.RegenerateAndRestartScenario(onFinish);
	}

	public static void RegenerateAndRestartScenarioKeepGoldAndXP()
	{
		SaveData.Instance.Global.StopSpeedUp();
		SaveData.Instance.Global.CurrentAdventureData.EndCurrentScenario(EResult.Lose);
		SceneController.Instance.RegenerateAndRestartScenario();
	}

	private static IEnumerator LoadMainMenuAfterSceneLoadedCoroutine()
	{
		while (SceneController.Instance.IsLoading)
		{
			yield return null;
		}
		LoadMainMenu();
	}

	public static void LoadMainMenuAfterSceneLoaded()
	{
		CoroutineHelper.RunCoroutine(LoadMainMenuAfterSceneLoadedCoroutine());
	}

	public static void LoadMainMenu(Action onLoaded = null, Action onUnloadPreviousScene = null)
	{
		LogUtils.Log("LoadMainMenu...");
		PlatformLayer.GameProvider.MainMenuLoadingStarted();
		SimpleLog.WriteSimpleLogToFile();
		AudioController.StopAmbienceSound();
		if (Singleton<UIConfirmationBoxManager>.Instance != null)
		{
			Singleton<UIConfirmationBoxManager>.Instance.Hide();
		}
		if (UIConfirmationBoxManager.MainMenuInstance != null)
		{
			UIConfirmationBoxManager.MainMenuInstance.Hide();
		}
		SceneController.Instance.ShowLoadingScreen();
		Main.Unpause3DWorld(forceUnpause: true);
		Singleton<UIOptionsWindow>.Instance.Hide();
		Singleton<ESCMenu>.Instance?.Hide();
		ControllerInputAreaManager.Instance?.SetDefaultFocusArea(EControllerInputAreaType.None);
		if (AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			AutoTestController.s_Instance.QuitFromAutotest();
		}
		CoroutineHelper.instance.StartCoroutine(MainMenuCoroutine(onLoaded, onUnloadPreviousScene));
	}

	private static IEnumerator MainMenuCoroutine(Action onFinish = null, Action onUnloadPreviousScene = null)
	{
		SaveData.Instance.Global.StopSpeedUp();
		while ((Singleton<AutoSaveProgress>.Instance != null && Singleton<AutoSaveProgress>.Instance.IsShowing) || SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
		{
			yield return null;
		}
		bool saveScenario = false;
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			Singleton<StoryController>.Instance.Clear();
			yield return SceneController.Instance.EndScenarioSafely();
			if (PlatformLayer.UserData.IsSignedIn && (Singleton<UIResultsManager>.Instance == null || !Singleton<UIResultsManager>.Instance.IsShown))
			{
				SaveData.Instance.EndScenario(EResult.InProgress);
				if (SaveData.Instance.Global.GameMode == EGameMode.Campaign || SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
				{
					saveScenario = true;
				}
			}
		}
		SaveData.Instance.Global.CloseCustomLevel();
		ScenarioRuleClient.ToggleMessageProcessing(process: false);
		if (saveScenario)
		{
			yield return Choreographer.s_Choreographer.EndGameCoroutine(EResult.InProgress);
		}
		Singleton<UIBlackOverlay>.Instance.ParentToDefaultCanvas();
		yield return UnityGameEditorRuntime.UnloadScenario();
		SceneController.Instance.LoadMainMenu(onUnloadPreviousScene, onFinish);
		if (!string.IsNullOrEmpty(GlobalSettings.Instance.m_MainMenuMusicPlaylist))
		{
			AudioController.PlayMusicPlaylist(GlobalSettings.Instance.m_MainMenuMusicPlaylist);
		}
		else
		{
			AudioController.StopMusic();
		}
	}

	private static IEnumerator RestartScenarioCoroutine()
	{
		Singleton<UIBlackOverlay>.Instance.ParentToDefaultCanvas();
		yield return UnityGameEditorRuntime.UnloadScenario();
		switch (SaveData.Instance.Global.GameMode)
		{
		case EGameMode.Campaign:
			SceneController.Instance.CampaignRestart();
			break;
		case EGameMode.Guildmaster:
			SceneController.Instance.NewAdventureRestart();
			break;
		default:
			throw new Exception("Restart is unsupported in this mode");
		}
	}

	public void EnableDisableButton(GameObject go, bool enabled)
	{
		MakeButtonsInteractable(go, enabled);
	}
}
