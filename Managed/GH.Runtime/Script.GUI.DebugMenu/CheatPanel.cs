#define ENABLE_LOGS
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using FrameTimeLogger;
using Platforms.Generic;
using SRDebugger;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using VoiceChat;

namespace Script.GUI.DebugMenu;

[Preserve]
public class CheatPanel
{
	[Preserve]
	private float _voiceDetectionThreshold = 18f;

	[Preserve]
	[Category("Achievements")]
	private List<string> _achievementName = new List<string>();

	private int _i;

	[Preserve]
	[Category("Utils")]
	[NumberRange(1.0, 100.0)]
	public float VoiceDetectionThreshold
	{
		get
		{
			return _voiceDetectionThreshold;
		}
		set
		{
			_voiceDetectionThreshold = value;
			Singleton<BoltVoiceChatService>.Instance.Recorder.VoiceDetectionThreshold = _voiceDetectionThreshold / 100f;
		}
	}

	[Preserve]
	[Category("Utils")]
	public void ShowAndLogKSIVA()
	{
		string text = global::PlatformLayer.Platform.HydraKsivaProvider?.Ksiva;
		string text2 = (text.IsNullOrEmpty() ? "KSIVA is null or empty" : ("KSIVA: " + text));
		Singleton<UINotificationManager>.Instance.ShowNotification("GUI_ACHIEVEMENT_FILTER_General", text2, 15f);
		UnityEngine.Debug.Log(text2);
	}

	[Preserve]
	[Category("Utils")]
	public void TraceCurrentQuest()
	{
		UnityEngine.Debug.LogError(global::PlatformLayer.Setting.GetCurrentQuest());
	}

	[Preserve]
	[Category("Utils")]
	public void LogMemoryStats()
	{
		ProfilerRecorder totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
		ProfilerRecorder systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
		UnityMainThreadDispatcher.Instance().Enqueue(delegate
		{
			PrintMemoryStats(totalReservedMemoryRecorder, systemUsedMemoryRecorder);
		});
	}

	public void PrintMemoryStats(ProfilerRecorder totalReservedMemoryRecorder, ProfilerRecorder systemUsedMemoryRecorder)
	{
		StringBuilder stringBuilder = new StringBuilder(500);
		if (totalReservedMemoryRecorder.Valid)
		{
			stringBuilder.AppendLine($"Total Reserved Memory: {totalReservedMemoryRecorder.CurrentValue / 1048576}");
		}
		if (systemUsedMemoryRecorder.Valid)
		{
			stringBuilder.AppendLine($"System Used Memory: {systemUsedMemoryRecorder.CurrentValue / 1048576}");
		}
		stringBuilder.AppendLine($"untracked Memory: {(systemUsedMemoryRecorder.CurrentValue - totalReservedMemoryRecorder.CurrentValue) / 1048576}");
		string text = stringBuilder.ToString();
		UnityEngine.Debug.LogError("MEMSTATS: " + text);
		totalReservedMemoryRecorder.Dispose();
		systemUsedMemoryRecorder.Dispose();
	}

	[Preserve]
	[Category("Utils")]
	public void EnableStackTrace()
	{
		SetStackTrace(StackTraceLogType.Full);
	}

	[Preserve]
	[Category("Utils")]
	public void DisableStackTrace()
	{
		SetStackTrace(StackTraceLogType.None);
	}

	[Preserve]
	[Category("Utils")]
	public void AddUserToBlockList()
	{
		((PlatformSocialGeneric)global::PlatformLayer.Platform.PlatformSocial).AddToBlocklist("0", null);
	}

	[Preserve]
	[Category("Utils")]
	public void TestShadows()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			foreach (GameObject gameObject in rootGameObjects)
			{
				MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
				foreach (MeshRenderer meshRenderer in componentsInChildren)
				{
					UnityEngine.Debug.LogError($"MeshRenderer {meshRenderer.gameObject.name} {meshRenderer.receiveShadows} {meshRenderer.shadowCastingMode}");
				}
				SkinnedMeshRenderer[] componentsInChildren2 = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
				{
					UnityEngine.Debug.LogError($"SkinnedMeshRenderer {skinnedMeshRenderer.gameObject.name} {skinnedMeshRenderer.receiveShadows} {skinnedMeshRenderer.shadowCastingMode}");
				}
			}
		}
	}

	private void SetStackTrace(StackTraceLogType stackTraceLogType)
	{
		Application.SetStackTraceLogType(LogType.Log, stackTraceLogType);
		Application.SetStackTraceLogType(LogType.Warning, stackTraceLogType);
		Application.SetStackTraceLogType(LogType.Assert, stackTraceLogType);
		Application.SetStackTraceLogType(LogType.Error, stackTraceLogType);
		Application.SetStackTraceLogType(LogType.Exception, stackTraceLogType);
	}

	[Preserve]
	[Category("Utils")]
	public void ToggleDisableInputOnAppUnfocus()
	{
		global::DebugMenu.ToggleDisableInputOnAppUnfocus();
	}

	[Preserve]
	[Category("Utils")]
	public void LogCurrentLoadedLevelName()
	{
		string text = string.Empty;
		if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.QuestName != null)
		{
			text = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentMapScenarioState.QuestName;
		}
		else if (SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario != null)
		{
			text = SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.CurrentSingleScenario.Name;
		}
		UnityEngine.Debug.Log("Loaded \"" + text + "\" level!");
	}

	[Preserve]
	[Category("Scene cheats")]
	public void LoadEmptyScene()
	{
		SceneController.Instance.LoadEmptyScene();
	}

	[Preserve]
	[Category("Scene cheats")]
	public void ClearGraphicRegistry()
	{
		SceneController.Instance.ClearGraphicRegistry();
	}

	[Preserve]
	[Category("Error Window")]
	public void ShowErrorReportPopup()
	{
		SceneController.Instance.GlobalErrorMessage.ShowSendErrorReport("Debug Error Report");
	}

	[Preserve]
	[Category("Error Window")]
	public void ShowMessagePopup()
	{
		SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MAP_CHOREOGRAPHER_00020", "GUI_ERROR_MAIN_MENU_BUTTON", "Stack Trace", delegate
		{
			Debug.Log("Showing Message Popup");
		}, "Message");
	}

	[Preserve]
	[Category("Error Window")]
	public void ShowMultiChoicePopup()
	{
		List<ErrorMessage.LabelAction> buttons = new List<ErrorMessage.LabelAction>
		{
			new ErrorMessage.LabelAction("GUI_ERROR_RESET_SCENARIO_BUTTON", delegate
			{
				Debug.Log($"Fired: {KeyAction.UI_SUBMIT}");
			}, KeyAction.UI_SUBMIT),
			new ErrorMessage.LabelAction("GUI_ERROR_MAIN_MENU_BUTTON", delegate
			{
				Debug.Log($"Fired: {KeyAction.UI_CANCEL}");
			}, KeyAction.UI_CANCEL)
		};
		SceneController.Instance.GlobalErrorMessage.ShowMultiChoiceMessageDefaultTitle("ERROR_CHOREO_00154", "Stack Trace", buttons, "Message");
	}

	[Preserve]
	[Category("Battle Cheats")]
	public void Win()
	{
		global::DebugMenu.WinNoToggle();
	}

	[Preserve]
	[Category("Battle Cheats")]
	public void Lose()
	{
		global::DebugMenu.LoseNoToggle();
	}

	[Preserve]
	[Category("Battle Cheats")]
	public void KillEnemies()
	{
		global::DebugMenu.KillAllEnemies();
	}

	[Preserve]
	[Category("Battle Cheats")]
	public void RestoreCards()
	{
		global::DebugMenu.RestoreCards();
	}

	[Preserve]
	[Category("Battle Cheats")]
	public void RevealAllRooms()
	{
		global::DebugMenu.RevealAllRooms();
	}

	[Preserve]
	[Category("Optimization")]
	public void DisableTextLog()
	{
		UnityEngine.Debug.unityLogger.logEnabled = false;
	}

	[Preserve]
	[Category("Optimization")]
	public void EnableTextLog()
	{
		UnityEngine.Debug.unityLogger.logEnabled = true;
	}

	[Preserve]
	[Category("Optimization")]
	public void DisableWorldspaceCanvas()
	{
		WorldspaceUITools.Instance.WorldspaceCanvas.gameObject.SetActive(value: false);
	}

	[Preserve]
	[Category("Optimization")]
	public void EnableWorldspaceCanvas()
	{
		WorldspaceUITools.Instance.WorldspaceCanvas.gameObject.SetActive(value: true);
	}

	[Preserve]
	[Category("Meshes edit")]
	public void ChangeMeshesByCube()
	{
		global::DebugMenu.ChangeMeshesByCube();
	}

	[Preserve]
	[Category("Meshes edit")]
	public void SimplifyMeshesTo07()
	{
		global::DebugMenu.SimplifyMeshes(0.7f);
	}

	[Preserve]
	[Category("Meshes edit")]
	public void SimplifyMeshesTo04()
	{
		global::DebugMenu.SimplifyMeshes(0.4f);
	}

	[Preserve]
	[Category("Meshes edit")]
	public void SimplifyMeshesTo02()
	{
		global::DebugMenu.SimplifyMeshes(0.2f);
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent1()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_1ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent2()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_2ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent3()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_3ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent4()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_4ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent5()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_5ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent6()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_6ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent7()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_7ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent8()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_8ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent9()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_9ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent10()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_10ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent11()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_11ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent12()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_12ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent13()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_13ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent14()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_14ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent15()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_15ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent16()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_16ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent17()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_17ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent18()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_18ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent19()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_19ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent20()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_20ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent21()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_21ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent22()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_22ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent23()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_23ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent24()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_24ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent25()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_25ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent26()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_26ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent27()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_27ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent28()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_28ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent29()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_29ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent30()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_30ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent31()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_31ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent32()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_32ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent33()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_33ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent34()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_34ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent35()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_35ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent36()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_36ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent37()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_37ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent38()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_38ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent39()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_39ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent40()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_40ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent41()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_41ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent42()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_42ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent43()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_43ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent44()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_44ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent45()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_45ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent46()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_46ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent47()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_47ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent48()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_48ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent49()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_49ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent50()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_50ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent51()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_51ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent52()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_52ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent53()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_53ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent54()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_54ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent55()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_55ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent56()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_56ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent57()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_57ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent58()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_58ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent59()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_59ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent60()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_60ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent61()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_61ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent62()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_62ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent63()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_63ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent64()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_64ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent65()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_65ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent66()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_66ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent67()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_67ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent68()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_68ID");
	}

	[Preserve]
	[Category("Roads Events")]
	public void SetNextRoadEvent69()
	{
		global::DebugMenu.SetRoadEvent("Event_Road_Campaign_59ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent1()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_1ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent2()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_2ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent3()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_3ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent4()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_4ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent5()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_5ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent6()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_6ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent7()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_7ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent8()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_8ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent9()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_9ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent10()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_10ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent11()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_11ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent12()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_12ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent13()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_13ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent14()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_14ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent15()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_15ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent16()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_16ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent17()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_17ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent18()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_18ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent19()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_19ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent20()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_20ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent21()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_21ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent22()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_22ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent23()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_23ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent24()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_24ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent25()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_25ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent26()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_26ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent27()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_27ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent28()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_28ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent29()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_29ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent30()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_30ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent31()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_31ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent32()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_32ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent33()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_33ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent34()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_34ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent35()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_35ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent36()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_36ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent37()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_37ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent38()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_38ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent39()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_39ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent40()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_40ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent41()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_41ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent42()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_42ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent43()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_43ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent44()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_44ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent45()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_45ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent46()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_46ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent47()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_47ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent48()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_48ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent49()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_49ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent50()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_50ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent51()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_51ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent52()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_52ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent53()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_53ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent54()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_54ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent55()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_55ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent56()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_56ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent57()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_57ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent58()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_58ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent59()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_59ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent60()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_60ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent61()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_61ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent62()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_62ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent63()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_63ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent64()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_64ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent65()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_65ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent66()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_66ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent67()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_67ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent68()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_68ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent69()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_59ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent70()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_70ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent71()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_71ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent72()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_72ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent73()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_73ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent74()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_74ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent75()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_75ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent76()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_76ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent77()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_77ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent78()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_78ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent79()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_79ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent80()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_80ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent81()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_81ID");
	}

	[Preserve]
	[Category("City Events")]
	public void SetNextCityEvent82()
	{
		global::DebugMenu.SetCityEvent("Event_City_Campaign_82ID");
	}

	[Preserve]
	[Category("Map Cheats")]
	public void Give200XPToSelectedCharacter()
	{
		global::DebugMenu.GainSelectedCharactersXP(200);
	}

	[Preserve]
	[Category("Map Cheats")]
	public void Give200GoldToSelectedCharacter()
	{
		global::DebugMenu.GainSelectedCharactersGold(200);
	}

	[Preserve]
	[Category("Map Cheats")]
	public void Give5EnhancmentPoints()
	{
		global::DebugMenu.AddEnhancmentPoints(5);
	}

	[Preserve]
	[Category("Map Cheats")]
	public void SetMaxProsperity()
	{
		global::DebugMenu.SetProsperityXP(999);
	}

	[Preserve]
	[Category("Map Cheats")]
	public void ShowAllScenarios()
	{
		global::DebugMenu.ShowAllScenariosNoToggle();
	}

	[Preserve]
	[Category("Map Cheats")]
	public void UnlockEnchantress()
	{
		global::DebugMenu.UnlockEnchantress();
	}

	[Preserve]
	[Category("Map Cheats")]
	public void Give3PerksChecks()
	{
		global::DebugMenu.SetPerkChecks(3);
	}

	[Preserve]
	[Category("Map Cheats")]
	public void UnlockMercenaries()
	{
		global::DebugMenu.UnlockAllCharacters();
	}

	[Preserve]
	[Category("Map Cheats")]
	public void Inc5Reputation()
	{
		global::DebugMenu.ChangeReputation(5);
	}

	[Preserve]
	[Category("Map Cheats")]
	public void Sub5Reputation()
	{
		global::DebugMenu.ChangeReputation(-5);
	}

	[Preserve]
	[Category("Map Cheats")]
	public void GetAllEquip()
	{
		global::DebugMenu.GetAllItems();
	}

	[Preserve]
	[Category("Map Cheats")]
	public void AutoCompleteAllPersonalQuests()
	{
		global::DebugMenu.AutoCompleteAllPersonalQuests();
	}

	[Preserve]
	[Category("Map Cheats")]
	public void AutoCompleteMinePersonalQuest()
	{
		global::DebugMenu.AutoCompleteMinePersonalQuest();
	}

	[Preserve]
	[Category("Logger")]
	public void StartLogsRecord()
	{
		Singleton<FrameTimeRecorder>.Instance.StartRecord();
	}

	[Preserve]
	[Category("Logger")]
	public void StopLogsRecord()
	{
		Singleton<FrameTimeRecorder>.Instance.StopRecord();
	}

	[Preserve]
	[Category("Logger")]
	public void SaveLogs()
	{
		Singleton<LogsSaver>.Instance.SaveLogs();
	}

	[Preserve]
	[Category("SaveData")]
	public void FulfillSaveData()
	{
		byte[] data = new byte[1048576];
		global::PlatformLayer.FileSystem.CreateDirectory(Path.Combine(SaveData.Instance.PersistentDataPath, "Dummy"));
		for (int i = 0; i < 110; i++)
		{
			global::PlatformLayer.FileSystem.WriteFile(data, Path.Combine(SaveData.Instance.PersistentDataPath, "Dummy", i.ToString()));
		}
	}

	[Preserve]
	[Category("SaveData")]
	public void GetFreeSpace()
	{
		Debug.LogError("It supported on on switch platform");
	}

	[Preserve]
	[Category("Achievements")]
	public void LoadAllAvailableAchievements()
	{
		if (!global::PlatformLayer.Stats.AchievementsSupported)
		{
			Debug.LogError("[Cheat] Achievements are not supported (disabled) on this platform!");
		}
		else if (_achievementName == null || _achievementName.Count == 0)
		{
			Debug.LogError("[Cheat] LoadAllAchievements() called. Loading...");
			_achievementName = global::PlatformLayer.Stats.GetAllPlatformAchievements();
			Debug.LogError("[Cheat] LoadAllAchievements() Loaded " + _achievementName.Count + " achievements.");
		}
		else
		{
			Debug.LogError("[Cheat] LoadAllAchievements() called. List of all available platform Achievements is already loaded and contains " + _achievementName.Count + " items.");
		}
	}

	[Preserve]
	[Category("Achievements")]
	public void SetNextAchievementCompleted()
	{
		if (!global::PlatformLayer.Stats.AchievementsSupported)
		{
			Debug.LogError("[Cheat] Achievements are not supported (disabled) on this platform!");
			return;
		}
		if (_achievementName == null || _achievementName.Count == 0)
		{
			Debug.LogError("[Cheat] SetAchievementCompleted() called, but no achievements has been loaded yet. LoadAllAchievements first!");
			return;
		}
		if (_i >= _achievementName.Count)
		{
			Debug.LogError("[Cheat] SetAchievementCompleted() called, but all achievements are already unlocked.");
			return;
		}
		if (_i == 0)
		{
			Debug.LogError("[Cheat] SetAchievementCompleted(" + _achievementName[_i] + ") called. Usually this is a Platinum trophy and it is unlocked automatically when all other trophies are unlocked by the player.");
			_i++;
		}
		if (0 < _i && _i < _achievementName.Count)
		{
			Debug.LogError("[Cheat] SetAchievementCompleted(" + _achievementName[_i] + ") called.");
			List<string> achievementCompleted = new List<string> { _achievementName[_i] };
			global::PlatformLayer.Stats.SetAchievementCompleted(achievementCompleted);
			_i++;
		}
	}

	[Preserve]
	[Category("Achievements")]
	public void SetAllAchievementsCompleted()
	{
		if (!global::PlatformLayer.Stats.AchievementsSupported)
		{
			Debug.LogError("[Cheat] Achievements are not supported (disabled) on this platform!");
			return;
		}
		if (_achievementName == null || _achievementName.Count == 0)
		{
			Debug.LogError("[Cheat] SetAchievementCompleted() called, but no achievements has been loaded yet. LoadAllAchievements first!");
			return;
		}
		if (_i >= _achievementName.Count)
		{
			Debug.LogError("[Cheat] SetAllAchievementCompleted() called, but all achievements are already unlocked.");
			return;
		}
		List<string> list = new List<string>();
		while (_i < _achievementName.Count)
		{
			list.Add(_achievementName[_i]);
			_i++;
		}
		Debug.LogError("[Cheat] SetAllAchievementsCompleted() has been called for remaining " + list.Count + " achievements.");
		global::PlatformLayer.Stats.SetAchievementCompleted(list);
	}

	[Preserve]
	[Category("Achievements")]
	public void ClearAllAchievements()
	{
		if (!global::PlatformLayer.Stats.AchievementsSupported)
		{
			Debug.LogError("[Cheat] Achievements are not supported (disabled) on this platform!");
			return;
		}
		Debug.LogError("[Cheat] ClearAllAchievements() called.");
		global::PlatformLayer.Stats.ClearAllAchievements();
	}

	[Preserve]
	[Category("SaveData")]
	public void SaveGlobalData()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		Debug.LogError("BRYNN: SaveData ApplicationSuspend Start.");
		GlobalData global = SaveData.Instance.Global;
		if (global != null && global.CurrentGameState == EGameState.Scenario)
		{
			Debug.LogError("BRYNN: SaveData ApplicationSuspend, start EndScenario.");
			SaveData.Instance.EndScenario(EResult.InProgressAppKill);
			Debug.LogError($"BRYNN: SaveData ApplicationSuspend, finish EndScenario. Spent {stopwatch.Elapsed.Milliseconds} ms.");
		}
		SaveData.Instance.SaveGlobalData();
		Debug.LogError($"BRYNN: SaveData ApplicationSuspend Finish. Spent {stopwatch.Elapsed.Milliseconds} ms.");
	}

	private void UnlockSingleAchievement(int achievementId)
	{
		if (!global::PlatformLayer.Stats.AchievementsSupported)
		{
			Debug.LogError("[Cheat] Achievements are not supported (disabled) on this platform!");
			return;
		}
		List<string> achievementCompleted = new List<string> { achievementId.ToString() };
		Debug.LogError("[Cheat] Trying to unlock Achievement id:" + achievementId + " ");
		global::PlatformLayer.Stats.SetAchievementCompleted(achievementCompleted);
	}
}
