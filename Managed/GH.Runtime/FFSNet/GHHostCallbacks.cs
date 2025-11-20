#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Photon.Bolt;
using SM.Utils;
using ScenarioRuleLibrary;
using SharedLibrary.SimpleLog;

namespace FFSNet;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public sealed class GHHostCallbacks : HostCallbacks
{
	private List<string> m_ModCSVs;

	private string _psPlayerSessionID = "-Not Set";

	private string _devMessage = string.Empty;

	private List<HostPlatformData> _hostPlatformDatas = new List<HostPlatformData>();

	public override void Disconnected(BoltConnection connection)
	{
		RemoveConnection(connection);
		base.Disconnected(connection);
	}

	public override void OnEvent(SessionNegotiationEvent evnt)
	{
		SessionMessageType messageType = (SessionMessageType)evnt.MessageType;
		PlatformType platform = (PlatformType)evnt.Platform;
		LogUtils.Log($"NetworkCallbacksServer {evnt.Data} {messageType} {platform}");
		HostPlatformData hostPlatformData = null;
		switch (messageType)
		{
		case SessionMessageType.ClientSessionRequest:
			hostPlatformData = AddConnection(platform, evnt.RaisedBy);
			if (hostPlatformData.ServerLeader)
			{
				string pSPlayerSession = PlatformLayer.Platform.PlatformSocial.GetPSPlayerSession();
				NetworkUtils.SendSessionCommunicationEvent(SessionMessageType.ServerSessionResponse, pSPlayerSession, evnt.RaisedBy);
			}
			else if (string.IsNullOrEmpty(hostPlatformData.CurrentSession))
			{
				if (hostPlatformData.CurrentLeader == null)
				{
					hostPlatformData.CurrentLeader = evnt.RaisedBy;
					NetworkUtils.SendSessionCommunicationEvent(SessionMessageType.ServerCreateSessionRequest, string.Empty, evnt.RaisedBy);
				}
				else
				{
					LogUtils.Log($"Wait to receive session response from client {evnt.RaisedBy}.");
				}
			}
			else
			{
				LogUtils.Log($"We have client session {hostPlatformData.CurrentSession} for {evnt.RaisedBy}.");
				NetworkUtils.SendSessionCommunicationEvent(SessionMessageType.ServerSessionResponse, hostPlatformData.CurrentSession, evnt.RaisedBy);
			}
			break;
		case SessionMessageType.ClientSessionCreatedResponse:
			hostPlatformData = GetConnection(platform);
			hostPlatformData.CurrentSession = evnt.Data;
			{
				foreach (BoltConnection boltConnection in hostPlatformData.BoltConnections)
				{
					if (!boltConnection.Equals(hostPlatformData.CurrentLeader))
					{
						LogUtils.Log($"Send client session {hostPlatformData.CurrentSession} for {boltConnection}, {evnt.RaisedBy}, {hostPlatformData.CurrentLeader}.");
						NetworkUtils.SendSessionCommunicationEvent(SessionMessageType.ServerSessionResponse, hostPlatformData.CurrentSession, boltConnection);
					}
				}
				break;
			}
		default:
			throw new Exception($"Not implemented {messageType}");
		}
	}

	public override void OnEvent(GameDataRequest request)
	{
		if (request == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00010", "Received a GameDataRequest but the request returns null.");
			return;
		}
		if (request.RaisedBy == null)
		{
			Console.LogError("ERROR_MULTIPLAYER_00011", "Received a GameDataRequest for " + ((DataActionType)request.DataActionID/*cast due to .constrained prefix*/).ToString() + " but the requesting player does not exist anymore.");
			return;
		}
		Console.LogInfo("OnEvent: " + (DataActionType)request.DataActionID/*cast due to .constrained prefix*/);
		switch ((DataActionType)request.DataActionID)
		{
		case DataActionType.SendInitialGameState:
			StartCoroutine(SendGameStateCoroutine(request));
			break;
		case DataActionType.CompareScenarioStates:
			StartCoroutine(SendScenarioStateWhenReady(request));
			break;
		case DataActionType.SendSimpleLogFromHost:
			SimpleLog.WriteSimpleLogToFile();
			StartCoroutine(SendSimpleLogFromHostCoroutine(request));
			break;
		case DataActionType.SendPlayerLogFromHost:
			StartCoroutine(SendPlayerLogFromHostCoroutine(request));
			break;
		case DataActionType.SendModdedRuleset:
		{
			GHRuleset gHRuleset2 = SceneController.Instance.Modding.Rulesets.SingleOrDefault((GHRuleset s) => s.Name == SaveData.Instance.Global.CurrentModdedRuleset);
			if (gHRuleset2 != null && File.Exists(gHRuleset2.RulesetCompiledZip))
			{
				request.RaisedBy.StreamBytes(GHNetworkCallbacks.ReliableStreamChannel, File.ReadAllBytes(gHRuleset2.RulesetCompiledZip));
			}
			break;
		}
		case DataActionType.SendRulesetInstance:
		{
			GHRuleset gHRuleset = SceneController.Instance.Modding.Rulesets.SingleOrDefault((GHRuleset s) => s.Name == SaveData.Instance.Global.CurrentModdedRuleset).DeepCopySerializableObject<GHRuleset>();
			gHRuleset.LinkedCSV = (from s in gHRuleset.LinkedMods
				where s.MetaData.AppliedFiles.Contains(s.LangUpdateFilePath)
				select s.LangUpdateFilePath).ToList();
			gHRuleset.Name += PlatformLayer.UserData.PlatformAccountID;
			m_ModCSVs = gHRuleset.LinkedCSV;
			if (gHRuleset != null)
			{
				request.RaisedBy.StreamBytes(GHNetworkCallbacks.ReliableStreamChannel, Utility.CompressData(Utility.ObjectToByteArray(gHRuleset)));
			}
			break;
		}
		case DataActionType.SendCSVFiles:
			if (m_ModCSVs.Count > 0)
			{
				string text = m_ModCSVs[0];
				m_ModCSVs.Remove(text);
				request.RaisedBy.StreamBytes(GHNetworkCallbacks.ReliableStreamChannel, Utility.CompressData(File.ReadAllBytes(text)));
			}
			break;
		default:
			Console.LogError("ERROR_MULTIPLAYER_00012", "Unprocessable DataActionType found: " + (DataActionType)request.DataActionID/*cast due to .constrained prefix*/);
			break;
		}
	}

	public override void OnEvent(GameDataEvent evnt)
	{
		if (evnt == null)
		{
			FFSNetwork.HandleDesync(new Exception("Received a GameDataEvent but the event returns null."));
			return;
		}
		byte[] array = Singleton<DataStreamingManager>.Instance.ReassembleData(evnt);
		if (array != null)
		{
			switch ((DataActionType)evnt.DataActionID)
			{
			case DataActionType.SendSimpleLogFromClient:
				File.WriteAllBytes(RootSaveData.SimpleLogFromClientPath(PlayerRegistry.GetUserName(evnt.RaisedBy)), array);
				break;
			case DataActionType.SendPlayerLogFromClient:
				File.WriteAllBytes(RootSaveData.PlayerLogFromClientPath(PlayerRegistry.GetUserName(evnt.RaisedBy)), array);
				break;
			default:
				FFSNetwork.HandleDesync(new Exception("Error processing received chunk of game data. Unprocessable DataActionType found: " + (DataActionType)evnt.DataActionID/*cast due to .constrained prefix*/));
				break;
			}
		}
	}

	private IEnumerator SendPlayerLogFromHostCoroutine(GameDataRequest request)
	{
		bool sentLog = false;
		int attempts = 1000;
		while (!sentLog && attempts > 0)
		{
			attempts--;
			try
			{
				if (File.Exists(RootSaveData.PlayerLogPath))
				{
					File.Copy(RootSaveData.PlayerLogPath, RootSaveData.TempPlayerLogPath);
					if (File.Exists(RootSaveData.TempPlayerLogPath))
					{
						request.RaisedBy.StreamBytes(GHNetworkCallbacks.ReliableStreamChannel, Utility.CompressData(File.ReadAllBytes(RootSaveData.TempPlayerLogPath)));
						sentLog = true;
						File.Delete(RootSaveData.TempPlayerLogPath);
					}
				}
				else
				{
					Debug.Log("[SEND HOST PLAYER LOG] Unable to find Player log file at:" + RootSaveData.PlayerLogPath);
				}
			}
			catch (IOException ex)
			{
				Debug.Log("[SEND HOST PLAYER LOG]" + ex.Message + "Stack Trace: " + ex.StackTrace + "\n Attempts left: " + attempts);
				goto IL_0100;
			}
			break;
			IL_0100:
			if (!sentLog)
			{
				yield return null;
			}
		}
		if (!sentLog)
		{
			Debug.LogError("[SEND HOST PLAYER LOG] Unable to send player log from Host");
		}
	}

	private IEnumerator SendSimpleLogFromHostCoroutine(GameDataRequest request)
	{
		bool sentLog = false;
		int attempts = 1000;
		while (!sentLog && attempts > 0)
		{
			attempts--;
			try
			{
				if (File.Exists(RootSaveData.SimpleLogPath))
				{
					request.RaisedBy.StreamBytes(GHNetworkCallbacks.ReliableStreamChannel, Utility.CompressData(File.ReadAllBytes(RootSaveData.SimpleLogPath)));
					sentLog = true;
				}
				else
				{
					Debug.Log("[SEND HOST SIMPLE LOG] Unable to find Player log file at:" + RootSaveData.PlayerLogPath);
				}
			}
			catch (IOException ex)
			{
				Debug.Log("[SEND HOST SIMPLE LOG]" + ex.Message + "\n Attempts left: " + attempts);
				goto IL_00b8;
			}
			break;
			IL_00b8:
			if (!sentLog)
			{
				yield return null;
			}
		}
		if (!sentLog)
		{
			Debug.LogError("[SEND HOST SIMPLE LOG] Unable to send simple log from Host");
		}
	}

	private IEnumerator SendGameStateCoroutine(GameDataRequest request)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		LogUtils.LogWarning("GHHostCallback.SendGameStateCoroutine. Starting stopwatch...");
		if (request != null && request.RaisedBy != null)
		{
			BoltConnection connection = request.RaisedBy;
			while ((global::Singleton<AutoSaveProgress>.Instance != null && global::Singleton<AutoSaveProgress>.Instance.IsShowing) || SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
			{
				yield return null;
			}
			string saveFilePath = SaveData.Instance.RootData.CurrentlyUsedSavePath;
			if (saveFilePath != null && connection != null)
			{
				if ((SaveData.Instance.Global.GameMode == EGameMode.Guildmaster || SaveData.Instance.Global.GameMode == EGameMode.Campaign) && ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ)
				{
					LogUtils.LogWarning($"GHHostCallback.SendGameStateCoroutine(Before SaveCurrentAdventureData). Spend {stopwatch.ElapsedMilliseconds} milliseconds.");
					SaveData.Instance.SaveCurrentAdventureData();
					LogUtils.LogWarning($"GHHostCallback.SendGameStateCoroutine(After SaveCurrentAdventureData). Spend {stopwatch.ElapsedMilliseconds} milliseconds.");
				}
				LogUtils.LogWarning($"GHHostCallback.SendGameStateCoroutine(Before CompressData). Spend {stopwatch.ElapsedMilliseconds} milliseconds.");
				while (SaveData.Instance.SaveQueue.IsAnyOperationExecuting)
				{
					yield return null;
				}
				Task<byte[]> task = new Task<byte[]>(() => Utility.CompressData(PlatformLayer.FileSystem.ReadFile(saveFilePath)));
				task.Start();
				while (!task.IsCompleted)
				{
					yield return null;
				}
				byte[] result = task.Result;
				LogUtils.LogWarning($"GHHostCallback.SendGameStateCoroutine(After CompressData). Spend {stopwatch.ElapsedMilliseconds} milliseconds.");
				LogUtils.LogWarning($"GHHostCallback.SendGameStateCoroutine(Before StreamBytes). Spend {stopwatch.ElapsedMilliseconds} milliseconds.");
				connection.StreamBytes(GHNetworkCallbacks.ReliableStreamChannel, result);
				LogUtils.LogWarning($"GHHostCallback.SendGameStateCoroutine(After StreamBytes). Spend {stopwatch.ElapsedMilliseconds} milliseconds.");
			}
		}
		LogUtils.LogWarning($"GHHostCallback.SendGameStateCoroutine. Spend {stopwatch.ElapsedMilliseconds} milliseconds.");
		stopwatch.Stop();
	}

	public IEnumerator SendScenarioStateWhenReady(GameDataRequest request)
	{
		Console.LogInfo("SendScenarioStateWhenReady: " + (request != null && request.RaisedBy != null));
		if (request != null && request.RaisedBy != null)
		{
			BoltConnection connection = request.RaisedBy;
			while (!Choreographer.s_Choreographer.HostReadyToSendCompareState || DelayedDropSMB.DelayedDropsAreInProgress())
			{
				yield return null;
			}
			if (connection != null)
			{
				Console.LogInfo("SendScenarioStateWhenReady. StreamBytes.");
				connection.StreamBytes(GHNetworkCallbacks.ReliableStreamChannel, Utility.CompressData(Utility.ObjectToByteArray(ScenarioManager.CurrentScenarioState)));
			}
		}
	}

	private HostPlatformData GetConnection(PlatformType platform)
	{
		return _hostPlatformDatas.FirstOrDefault((HostPlatformData x) => x.Platform == platform);
	}

	private HostPlatformData AddConnection(PlatformType platform, BoltConnection boltConnection)
	{
		HostPlatformData hostPlatformData = _hostPlatformDatas.FirstOrDefault((HostPlatformData x) => x.Platform == platform);
		if (hostPlatformData != null)
		{
			hostPlatformData.BoltConnections.Add(boltConnection);
		}
		else
		{
			hostPlatformData = new HostPlatformData
			{
				Platform = platform
			};
			hostPlatformData.BoltConnections.Add(boltConnection);
			hostPlatformData.ServerLeader = platform == PlatformType.PC;
			_hostPlatformDatas.Add(hostPlatformData);
		}
		return hostPlatformData;
	}

	private void RemoveConnection(BoltConnection boltConnection)
	{
		if (object.Equals(boltConnection, BoltNetwork.Server))
		{
			return;
		}
		for (int i = 0; i < _hostPlatformDatas.Count; i++)
		{
			HostPlatformData hostPlatformData = _hostPlatformDatas[i];
			if (!hostPlatformData.BoltConnections.Contains(boltConnection))
			{
				continue;
			}
			hostPlatformData.BoltConnections.Remove(boltConnection);
			if (hostPlatformData.ServerLeader)
			{
				continue;
			}
			if (hostPlatformData.BoltConnections.Count > 0)
			{
				if (object.Equals(hostPlatformData.CurrentLeader, boltConnection))
				{
					hostPlatformData.CurrentLeader = null;
				}
			}
			else
			{
				_hostPlatformDatas.Remove(hostPlatformData);
			}
		}
	}
}
