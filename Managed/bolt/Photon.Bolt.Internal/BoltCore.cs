#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using Photon.Bolt.LagCompensation;
using Photon.Bolt.SceneManagement;
using Photon.Bolt.Tokens;
using Photon.Bolt.Utils;
using Photon.Realtime;
using UdpKit;
using UdpKit.Platform;
using UnityEngine;

namespace Photon.Bolt.Internal;

internal static class BoltCore
{
	internal static UdpSocket _udpSocket;

	internal static UdpPlatform _udpPlatform;

	internal static BoltConnection _serverConnection;

	private static readonly object _autoLoadSceneLock = new object();

	internal static AutoLoadSceneInfo _autoLoadSceneInfo = default(AutoLoadSceneInfo);

	internal static Stopwatch _timer = new Stopwatch();

	internal static Stopwatch _timer2 = new Stopwatch();

	internal static Stopwatch _timer3 = new Stopwatch();

	internal static SceneLoadState _localSceneLoading;

	internal static bool _canReceiveEntities = true;

	internal static IPrefabPool PrefabPool = new DefaultPrefabPool();

	internal static IEventFilter EventFilter = new DefaultEventFilter();

	internal static int _frame = 0;

	internal static BoltNetworkModes _mode = BoltNetworkModes.None;

	internal static BoltConfig _config = null;

	internal static UdpConfig _udpConfig = null;

	internal static BoltDoubleList<Entity> _entitiesOK = new BoltDoubleList<Entity>();

	internal static BoltDoubleList<Entity> _entitiesFZ = new BoltDoubleList<Entity>();

	internal static LinkedList<BoltConnection> _connections = new LinkedList<BoltConnection>();

	internal static EventDispatcher _globalEventDispatcher = new EventDispatcher();

	internal static Dictionary<UniqueId, BoltEntity> _sceneObjects = new Dictionary<UniqueId, BoltEntity>(UniqueId.EqualityComparer.Instance);

	internal static Func<GameObject, Vector3, Quaternion, GameObject> _instantiate = (GameObject go, Vector3 p, Quaternion r) => UnityEngine.Object.Instantiate(go, p, r);

	internal static Action<GameObject> _destroy = delegate(GameObject go)
	{
		UnityEngine.Object.Destroy(go);
	};

	internal static GameObject _globalControlObject = null;

	internal static ControlBehaviour _globalControlBehaviour = null;

	internal static GameObject _globalBehaviourObject = null;

	private static List<STuple<BoltGlobalBehaviourAttribute, Type>> _globalBehaviours = new List<STuple<BoltGlobalBehaviourAttribute, Type>>();

	private static readonly List<Entity> _freezeProxyTempList = new List<Entity>();

	public static TimeSpan SendTime;

	public static TimeSpan AutoscopeTime;

	public static TimeSpan PollNetworkTime;

	public static TimeSpan InvokeRemoteSceneCallbacksTime;

	public static TimeSpan AdjustEstimatedRemoteFramesTime;

	public static TimeSpan StepNonControlledRemoteEntitiesTime;

	public static TimeSpan SimulateLocalAndControlledEntitiesTime;

	public static TimeSpan DispatchAllEventsTime;

	public static bool IsDebugMode => true;

	public static bool IsCloud => true;

	public static AsyncOperation currentAsyncOperation => BoltSceneLoader.CurrentAsyncOperation;

	public static int loadedScene => _localSceneLoading.Scene.Index;

	public static string loadedSceneName => BoltNetworkInternal.GetSceneName(_localSceneLoading.Scene.Index);

	public static GameObject globalObject => _globalBehaviourObject;

	public static IEnumerable<BoltEntity> entities => _entities.Select((Entity x) => x.UnityObject);

	public static IEnumerable<BoltConnection> connections => _connections;

	public static IEnumerable<BoltConnection> clients => connections.Where((BoltConnection c) => c.udpConnection.IsServer);

	public static BoltConnection server
	{
		get
		{
			if (isServer)
			{
				return null;
			}
			if (_serverConnection != null)
			{
				return _serverConnection;
			}
			IEnumerator<BoltConnection> enumerator = connections.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.udpConnection.IsClient)
				{
					return _serverConnection = enumerator.Current;
				}
			}
			return null;
		}
	}

	public static int frame => _frame;

	public static int framesPerSecond => (_config != null) ? _config.framesPerSecond : 60;

	public static int frameSlice => 1 / framesPerSecond;

	public static int serverFrame
	{
		get
		{
			if (_mode == BoltNetworkModes.None || !IsRunning)
			{
				return 0;
			}
			if (isServer)
			{
				return _frame - 1;
			}
			if (server != null)
			{
				return server.RemoteFrame;
			}
			return 0;
		}
	}

	public static float serverTime => (float)serverFrame / (float)framesPerSecond;

	public static float time => Time.time;

	public static float frameBeginTime => Time.fixedTime;

	public static float frameDeltaTime => Time.fixedDeltaTime;

	public static float frameAlpha => Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

	public static bool IsRunning => isServer || isClient || _mode == BoltNetworkModes.Shutdown;

	public static bool isClient => HasSocket && _mode == BoltNetworkModes.Client;

	public static bool isServer => HasSocket && _mode == BoltNetworkModes.Server;

	public static bool isSinglePlayer => HasSocket && _udpPlatform != null && _udpPlatform.IsNull;

	internal static IEnumerable<Entity> _entities
	{
		get
		{
			BoltIterator<Entity> it = _entitiesOK.GetIterator();
			while (it.Next())
			{
				yield return it.val;
			}
			it = _entitiesFZ.GetIterator();
			while (it.Next())
			{
				yield return it.val;
			}
		}
	}

	internal static int localSendRate => _mode switch
	{
		BoltNetworkModes.Server => _config.serverSendRate, 
		BoltNetworkModes.Client => _config.clientSendRate, 
		_ => -1, 
	};

	internal static int remoteSendRate => _mode switch
	{
		BoltNetworkModes.Server => _config.clientSendRate, 
		BoltNetworkModes.Client => _config.serverSendRate, 
		_ => -1, 
	};

	internal static int localInterpolationDelay => _mode switch
	{
		BoltNetworkModes.Server => _config.serverDejitterDelay, 
		BoltNetworkModes.Client => _config.clientDejitterDelay, 
		_ => -1, 
	};

	internal static int localInterpolationDelayMin => _mode switch
	{
		BoltNetworkModes.Server => _config.serverDejitterDelayMin, 
		BoltNetworkModes.Client => _config.clientDejitterDelayMin, 
		_ => -1, 
	};

	internal static int localInterpolationDelayMax => _mode switch
	{
		BoltNetworkModes.Server => _config.serverDejitterDelayMax, 
		BoltNetworkModes.Client => _config.clientDejitterDelayMax, 
		_ => -1, 
	};

	internal static UdpSession CurrentSession => _udpSocket?.sessionManager.GetLocalSession();

	internal static Dictionary<string, object> CurrentMetadata => _udpPlatform?.GetPlatformMetadata();

	private static bool HasSocket => _udpSocket != null;

	internal static void Connect(UdpEndPoint endpoint, IProtocolToken token)
	{
		if (server != null)
		{
			BoltLog.Error("You must disconnect from the current server first");
			return;
		}
		DisableLanBroadcast();
		_udpSocket.Connect(endpoint, token.ToByteArray());
	}

	internal static void Connect(string hostName, IProtocolToken token)
	{
		Connect(_udpPlatform.BuildSession(hostName), token);
	}

	internal static void Connect(UdpSession session, IProtocolToken token)
	{
		if (server != null)
		{
			BoltLog.Error("You must disconnect from the current server first");
			return;
		}
		DisableLanBroadcast();
		_udpSocket.Connect(session, token.ToByteArray());
	}

	internal static void ConnectRandom(UdpSessionFilter sessionFilter, IProtocolToken token)
	{
		if (server != null)
		{
			BoltLog.Error("You must disconnect from the current server first");
			return;
		}
		DisableLanBroadcast();
		_udpSocket.ConnectRandom(sessionFilter, token.ToByteArray());
	}

	internal static void CancelConnect(UdpEndPoint endPoint, bool internalOnly = false)
	{
		if (server != null)
		{
			BoltLog.Error("It's not possible to cancel the connection if you are already connected");
		}
		else
		{
			_udpSocket.CancelConnect(endPoint, internalOnly);
		}
	}

	internal static void AcceptConnection(UdpEndPoint endpoint, object userToken, IProtocolToken acceptToken)
	{
		if (!isServer)
		{
			BoltLog.Error("AcceptConnection can only be called on the server");
		}
		else if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual)
		{
			BoltLog.Warn("AcceptConnection can only be called BoltConnectionAcceptMode is set to Manual");
		}
		else
		{
			_udpSocket.Accept(endpoint, userToken, acceptToken.ToByteArray());
		}
	}

	internal static void RefuseConnection(UdpEndPoint endpoint, IProtocolToken token)
	{
		if (!isServer)
		{
			BoltLog.Error("RefuseConnection can only be called on the server");
		}
		else if (_config.serverConnectionAcceptMode != BoltConnectionAcceptMode.Manual)
		{
			BoltLog.Warn("RefuseConnection can only be called BoltConnectionAcceptMode is set to Manual");
		}
		else
		{
			_udpSocket.Refuse(endpoint, token.ToByteArray());
		}
	}

	public static void Shutdown(UdpConnectionDisconnectReason disconnectReason = UdpConnectionDisconnectReason.Disconnected)
	{
		if ((bool)_globalControlObject && (bool)_globalControlBehaviour)
		{
			_globalControlBehaviour.QueueControlCommand(new ControlCommandShutdown
			{
				disconnectReason = disconnectReason
			});
			return;
		}
		throw new BoltException("Could not find BoltControl object");
	}

	internal static void SetServerInfo(string serverName, bool dedicated, IProtocolToken token)
	{
		if (!isServer)
		{
			BoltLog.Error("Only the server can call SetSessionData");
		}
		else
		{
			_udpSocket?.SetHostInfo(serverName, dedicated, token, token.ToByteArray());
		}
	}

	internal static void EnableLanBroadcast(ushort port)
	{
		_udpSocket?.LanBroadcastEnable(port);
	}

	internal static void DisableLanBroadcast()
	{
		_udpSocket?.LanBroadcastDisable();
	}

	internal static void Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, BoltConfig config, UdpPlatform udpPlatform, string sceneToLoad)
	{
		if (!string.IsNullOrEmpty(sceneToLoad))
		{
			_autoLoadSceneInfo = new AutoLoadSceneInfo
			{
				Origin = AutoLoadSceneOrigin.STARTUP,
				Scene = sceneToLoad,
				Token = null
			};
		}
		if (_globalControlObject == null)
		{
			_globalControlObject = new GameObject("BoltControl");
			_globalControlBehaviour = _globalControlObject.AddComponent<ControlBehaviour>();
			UnityEngine.Object.DontDestroyOnLoad(_globalControlObject);
		}
		_globalControlBehaviour.QueueControlCommand(new ControlCommandStart
		{
			Mode = mode,
			EndPoint = endpoint,
			Config = config.Clone(),
			Platform = udpPlatform
		});
	}

	internal static void Update()
	{
		BoltIterator<Entity> iterator = _entitiesOK.GetIterator();
		while (iterator.Next())
		{
			if (!iterator.val.IsFrozen)
			{
				iterator.val.Render();
			}
		}
	}

	internal static void BeginStart(ControlCommandStart cmd)
	{
		_mode = cmd.Mode;
		_config = cmd.Config;
		_udpPlatform = cmd.Platform;
		_canReceiveEntities = true;
		ResetIdAllocator(_mode);
		BoltConsole.Clear();
		if (BoltRuntimeSettings.instance.showDebugInfo)
		{
			DebugInfo.SetupAndShow();
		}
		BoltLog.RemoveAll();
		BoltLog.Setup(_mode, _config.logTargets);
		BoltLog.Debug("{2} starting with a simulation rate of {0} on platform {1}", _config.framesPerSecond, UnitySettings.CurrentPlatform.ToString(), BoltNetwork.CurrentVersion);
		Time.fixedDeltaTime = 1f / (float)_config.framesPerSecond;
		PrefabDatabase.BuildCache();
		UdpLog.SetWriter(UdpLogWriter);
		CreateBoltBehaviourObject();
		BoltNetworkInternal.EnvironmentSetup();
		CreateUdpConfig(_config);
		UpdateCurrentScene();
		_udpSocket = new UdpSocket(Guid.NewGuid(), _udpPlatform, _udpConfig);
		UpdateActiveGlobalBehaviours(_localSceneLoading.Scene.Index);
		_udpPlatform.OnStartBegin();
		GlobalEventListenerBase.BoltStartBeginInvoke();
		DoProtocolTokenRegistration();
		_udpSocket.Start(cmd.EndPoint, cmd.FinishedEvent, (_mode == BoltNetworkModes.Server) ? UdpSocketMode.Host : UdpSocketMode.Client);
	}

	internal static async void BeginShutdown(ControlCommandShutdown cmd)
	{
		if (!BoltNetwork.IsRunning)
		{
			UnityEngine.Debug.LogWarning("Bolt is not running so it can't be shutdown");
			cmd.State = ControlState.Failed;
			cmd.FinishedEvent.Set();
			return;
		}
		BoltLog.Info("Stopping Photon Bolt");
		try
		{
			await ShutdownProcedure(BoltNetworkModes.Shutdown, cmd.disconnectReason, cmd.Callbacks.Add, cmd.FinishedEvent);
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogWarning($"Exception while Shutdown: {arg}");
		}
	}

	internal static async void ShutdownImmediate(UdpConnectionDisconnectReason disconnectReason = UdpConnectionDisconnectReason.Disconnected)
	{
		if (!BoltNetwork.IsRunning)
		{
			return;
		}
		BoltLog.Info("Stopping Photon Bolt Immediate");
		try
		{
			List<Action> addCallbacks = new List<Action>();
			await ShutdownProcedure(BoltNetworkModes.None, disconnectReason, addCallbacks.Add, null, allowImmediateShutdown: true);
			foreach (Action callback in addCallbacks)
			{
				try
				{
					if (Application.isPlaying)
					{
						callback();
					}
				}
				catch (Exception exception)
				{
					BoltLog.Exception(exception);
				}
			}
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogWarning($"Exception while Shutdown: {arg}");
		}
	}

	private static async Task ShutdownProcedure(BoltNetworkModes setMode, UdpConnectionDisconnectReason disconnectReason, AddCallback shutdownAction, ManualResetEvent resetEvent = null, bool allowImmediateShutdown = false)
	{
		GlobalEventListenerBase.BoltShutdownBeginInvoke(shutdownAction, disconnectReason);
		_mode = setMode;
		Entity[] array = _entities.ToArray();
		foreach (Entity entity in array)
		{
			try
			{
				DestroyForce(entity);
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
		_entitiesFZ.Clear();
		_entitiesOK.Clear();
		_connections.Clear();
		_globalEventDispatcher.Clear();
		_globalBehaviours.Clear();
		BoltNetwork._sessionList = new Map<Guid, UdpSession>();
		if (!allowImmediateShutdown)
		{
			await _udpPlatform.OnShutdown();
		}
		else
		{
			_udpPlatform.OnShutdown();
		}
		if ((bool)_globalBehaviourObject)
		{
			_globalBehaviourObject.GetComponent<BoltPoll>().AllowImmediateShutdown = allowImmediateShutdown;
			UnityEngine.Object.Destroy(_globalBehaviourObject);
		}
		BoltNetworkInternal.EnvironmentReset();
		DisableLanBroadcast();
		UdpLog.SetWriter(delegate(uint num, string m)
		{
			UnityEngine.Debug.Log(m);
		});
		_udpSocket.Close(resetEvent);
		if (resetEvent != null)
		{
			await Task.Run(delegate
			{
				while (!resetEvent.WaitOne(0))
				{
				}
			});
		}
		_udpSocket = null;
		_serverConnection = null;
		_config = null;
		_udpPlatform = null;
		BoltSceneLoader.Clear();
		Factory.UnregisterAll();
		BoltLog.RemoveAll();
		BoltConsole.Clear();
		DebugInfo.Hide();
		_frame = 0;
		if (allowImmediateShutdown && (bool)_globalControlBehaviour)
		{
			_globalControlBehaviour.FinishPendingCommands();
		}
	}

	internal static void Quit()
	{
		_udpSocket?.Quit();
	}

	internal static void UpdateActiveGlobalBehaviours(int sceneIndex = -1)
	{
		if ((_config.logTargets & BoltConfigLogTargets.Console) == BoltConfigLogTargets.Console)
		{
			BoltConsole boltConsole = CreateGlobalBehaviour(typeof(BoltConsole)) as BoltConsole;
			if ((bool)boltConsole)
			{
				boltConsole.toggleKey = BoltRuntimeSettings.instance.consoleToggleKey;
				boltConsole.visible = BoltRuntimeSettings.instance.consoleVisibleByDefault;
			}
		}
		else
		{
			DeleteGlobalBehaviour(typeof(BoltConsole));
		}
		CreateGlobalBehaviour(typeof(BoltPhysicsDebug));
		CreateGlobalBehaviour(typeof(BoltPoll));
		CreateGlobalBehaviour(typeof(BoltSend));
		CreateGlobalBehaviour(typeof(BoltSceneLoader));
		if (_udpPlatform is PhotonPlatform)
		{
			GameObject gameObject = new GameObject();
			gameObject.AddComponent(typeof(ConnectionHandler));
			gameObject.name = "PhotonConnectionHandler";
			gameObject.hideFlags = HideFlags.HideInHierarchy;
			CreateGlobalBehaviour(typeof(PhotonCloudGlobalBehavior.PhotonBoltBehavior));
			CreateGlobalBehaviour(typeof(PhotonCloudGlobalBehavior));
		}
		foreach (STuple<BoltGlobalBehaviourAttribute, Type> globalBehaviour in _globalBehaviours)
		{
			BoltGlobalBehaviourAttribute item = globalBehaviour.item0;
			Type item2 = globalBehaviour.item1;
			if ((item.Mode & _mode) == _mode)
			{
				bool flag = item.Scenes == null || item.Scenes.Length == 0;
				bool flag2 = false;
				if (!flag && sceneIndex >= 0 && item.Scenes != null)
				{
					string loadingScene = BoltNetworkInternal.GetSceneName(sceneIndex);
					if (loadingScene != null)
					{
						flag2 = Array.FindIndex(item.Scenes, (string scenePattern) => Regex.IsMatch(loadingScene, scenePattern)) != -1;
					}
				}
				if (flag || flag2)
				{
					CreateGlobalBehaviour(item2);
				}
				else
				{
					DeleteGlobalBehaviour(item2);
				}
			}
			else
			{
				DeleteGlobalBehaviour(item2);
			}
		}
	}

	private static void CreateBoltBehaviourObject()
	{
		if ((bool)_globalBehaviourObject)
		{
			UnityEngine.Object.Destroy(_globalBehaviourObject);
		}
		_globalBehaviours = BoltNetworkInternal.GetGlobalBehaviourTypes();
		_globalBehaviourObject = new GameObject("BoltBehaviours");
		UnityEngine.Object.DontDestroyOnLoad(_globalBehaviourObject);
	}

	private static Component CreateGlobalBehaviour(Type t)
	{
		if ((bool)_globalBehaviourObject && _globalBehaviourObject.GetComponent(t) == null)
		{
			BoltLog.Debug("Creating Global Behaviour: '{0}'", t);
			return _globalBehaviourObject.AddComponent(t);
		}
		return null;
	}

	private static void DeleteGlobalBehaviour(Type t)
	{
		if ((bool)_globalBehaviourObject)
		{
			Component component = _globalBehaviourObject.GetComponent(t);
			if (component != null)
			{
				BoltLog.Debug("Deleting Global Behaviour: '{0}'", t);
				UnityEngine.Object.Destroy(component);
			}
		}
	}

	private static void AdjustLocalFrame()
	{
		if (_frame % framesPerSecond == 0 && isClient && server != null && server.RemoteFrame - frame > localInterpolationDelayMax)
		{
			_frame = server.RemoteFrame;
		}
	}

	private static void AdjustEstimatedRemoteFrames()
	{
		if (HasSocket)
		{
			LinkedList<BoltConnection>.Enumerator enumerator = _connections.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.AdjustRemoteFrame();
			}
		}
	}

	private static void StepNonControlledRemoteEntities()
	{
		if (!HasSocket)
		{
			return;
		}
		bool flag;
		do
		{
			flag = false;
			LinkedList<BoltConnection>.Enumerator enumerator = _connections.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.StepRemoteEntities())
				{
					flag = true;
				}
			}
		}
		while (flag);
	}

	private static void StepControlledEntities()
	{
		if (!HasSocket)
		{
			return;
		}
		BoltIterator<Entity> iterator = _entitiesOK.GetIterator();
		while (iterator.Next())
		{
			if (!iterator.val.IsFrozen && (iterator.val.IsOwner || iterator.val.HasPredictedControl))
			{
				iterator.val.Simulate();
			}
		}
		FreezeProxies();
	}

	internal static void SetSessionList(Map<Guid, UdpSession> sessions)
	{
		BoltNetwork._sessionList = sessions;
		GlobalEventListenerBase.SessionListUpdatedInvoke(BoltNetwork._sessionList);
	}

	internal static UdpPacket AllocateUdpPacket()
	{
		return _udpSocket.PacketPool.Acquire();
	}

	internal static UdpChannelName CreateStreamChannel(string name, UdpChannelMode mode, int priority)
	{
		if (_udpSocket.State != UdpSocketState.Created)
		{
			throw new BoltException("You can only create stream channels in the Bolt.GlobalEventListener.BoltStartBegin callback.");
		}
		return _udpSocket.StreamChannelCreate(name, mode, priority);
	}

	internal static void AutoLoadScene(AutoLoadSceneOrigin loadSceneOrigin)
	{
		lock (_autoLoadSceneLock)
		{
			if (!_autoLoadSceneInfo.Equals(default(AutoLoadSceneInfo)) && _autoLoadSceneInfo.Origin == loadSceneOrigin && !string.IsNullOrEmpty(_autoLoadSceneInfo.Scene))
			{
				BoltNetwork.LoadScene(_autoLoadSceneInfo.Scene, _autoLoadSceneInfo.Token);
				_autoLoadSceneInfo = default(AutoLoadSceneInfo);
			}
		}
	}

	internal static void Destroy(BoltEntity entity, IProtocolToken detachToken)
	{
		if (!entity.IsOwner)
		{
			BoltLog.Warn("Only the owner can destroy an entity, ignoring call to Destroy().");
			return;
		}
		if (!entity.IsAttached)
		{
			BoltLog.Warn("Entity is not attached, ignoring call to Destroy().");
			return;
		}
		entity.Entity.DetachToken = detachToken;
		DestroyForce(entity.Entity);
	}

	internal static void DestroyForce(Entity entity)
	{
		if ((bool)entity && entity.IsAttached)
		{
			entity.Detach();
			PrefabPool.Destroy(entity.UnityObject.gameObject);
		}
	}

	internal static void Detach(BoltEntity entity, IProtocolToken detachToken)
	{
		Assert.NotNull(entity.Entity);
		entity.Entity.DetachToken = detachToken;
		entity.Entity.Detach();
	}

	internal static Entity FindEntity(NetworkId id)
	{
		if (id.Connection == uint.MaxValue && NetworkIdAllocator.LocalConnectionId != uint.MaxValue)
		{
			id = new NetworkId(NetworkIdAllocator.LocalConnectionId, id.Entity);
		}
		foreach (Entity entity in _entities)
		{
			if (entity.NetworkId == id)
			{
				return entity;
			}
		}
		return null;
	}

	internal static bool CanClientInstantiate(BoltEntity entity)
	{
		return BoltRuntimeSettings.instance.instantiateMode switch
		{
			BoltPrefabInstantiateMode.ServerOnly => false, 
			BoltPrefabInstantiateMode.PerPrefab => entity._allowInstantiateOnClient, 
			_ => true, 
		};
	}

	internal static BoltEntity Instantiate(PrefabId prefabId, TypeId serializerId, Vector3 position, Quaternion rotation, InstantiateFlags instanceFlags, BoltConnection controller, IProtocolToken attachToken)
	{
		GameObject gameObject = PrefabPool.LoadPrefab(prefabId);
		BoltEntity component = gameObject.GetComponent<BoltEntity>();
		if (isClient && !CanClientInstantiate(component))
		{
			throw new BoltException("This prefab is not allowed to be instantiated on clients");
		}
		if (component._prefabId != prefabId.Value)
		{
			throw new BoltException("PrefabId for BoltEntity component did not return the same value as prefabId passed in as argument to Instantiate");
		}
		Entity entity = Entity.CreateFor(prefabId, serializerId, position, rotation);
		entity.Initialize();
		entity.AttachToken = attachToken;
		entity.Attach();
		return entity.UnityObject;
	}

	internal static GameObject Attach(GameObject gameObject, EntityFlags flags, IProtocolToken attachToken = null)
	{
		BoltEntity component = gameObject.GetComponent<BoltEntity>();
		return Attach(gameObject, Factory.GetFactory(component.SerializerGuid).TypeId, flags, attachToken);
	}

	internal static GameObject Attach(GameObject gameObject, TypeId serializerId, EntityFlags flags, IProtocolToken attachToken)
	{
		BoltEntity component = gameObject.GetComponent<BoltEntity>();
		Entity entity = Entity.CreateFor(gameObject, new PrefabId(component._prefabId), serializerId, flags);
		entity.Initialize();
		entity.AttachToken = attachToken;
		entity.Attach();
		return entity.UnityObject.gameObject;
	}

	internal static void FreezeProxies()
	{
		BoltIterator<Entity> iterator = _entitiesOK.GetIterator();
		List<Entity> freezeProxyTempList = _freezeProxyTempList;
		freezeProxyTempList.Clear();
		while (iterator.Next())
		{
			if (iterator.val.AutoFreezeProxyFrames > 0 && !iterator.val.IsOwner && !iterator.val.HasControl && iterator.val.LastFrameReceived + iterator.val.AutoFreezeProxyFrames < BoltNetwork.Frame)
			{
				freezeProxyTempList.Add(iterator.val);
			}
		}
		for (int i = 0; i < freezeProxyTempList.Count; i++)
		{
			freezeProxyTempList[i].Freeze(freeze: true);
		}
	}

	private static void PollNetwork()
	{
		UdpEvent ev;
		while (HasSocket && _udpSocket.Poll(out ev))
		{
			switch (ev.EventType)
			{
			case UdpEventType.SocketStartupDone:
				Udp_SocketStartupDone(ev.As<UdpEventStartDone>());
				break;
			case UdpEventType.SocketStartupFailed:
				Udp_SocketStartupFailed(ev.As<UdpEventStartFailed>());
				break;
			case UdpEventType.Connected:
				Udp_Connected((UdpConnection)ev.Object0);
				break;
			case UdpEventType.Disconnected:
				Udp_Disconnect((UdpConnection)ev.Object0);
				break;
			case UdpEventType.ConnectRequest:
				Udp_ConnectRequest(ev.As<UdpEventConnectRequest>());
				break;
			case UdpEventType.ConnectFailed:
				Udp_ConnectFailed(ev.As<UdpEventConnectFailed>());
				break;
			case UdpEventType.ConnectRefused:
				Udp_ConnectRefused(ev.As<UdpEventConnectRefused>());
				break;
			case UdpEventType.ConnectAttempt:
				Udp_ConnectAttempt(ev.As<UdpEventConnectAttempt>());
				break;
			case UdpEventType.PacketLost:
				Udp_PacketLost(ev);
				break;
			case UdpEventType.PacketDelivered:
				Udp_PacketDelivered(ev);
				break;
			case UdpEventType.PacketReceived:
				Udp_PacketReceived(ev);
				break;
			case UdpEventType.SessionListUpdated:
				Udp_SessionListUpdated(ev.As<UdpEventSessionListUpdated>());
				break;
			case UdpEventType.SessionCreated:
				Udp_SessionCreated(ev.As<UdpEventSessionCreated>());
				break;
			case UdpEventType.SessionConnected:
				Udp_SessionConnected(ev.As<UdpEventSessionConnected>());
				break;
			case UdpEventType.SessionConnectFailed:
				Udp_SessionConnectFailed(ev.As<UdpEventSessionConnectFailed>());
				break;
			case UdpEventType.StreamDataReceived:
				Udp_StreamDataReceived(ev.As<UdpEventStreamDataReceived>());
				break;
			case UdpEventType.StreamDataStarted:
				Udp_StreamDataStarted(ev.As<UdpEventStreamDataStarted>());
				break;
			case UdpEventType.StreamDataProgress:
				Udp_StreamDataProgress(ev.As<UdpEventStreamProgress>());
				break;
			case UdpEventType.StreamDataAbort:
				Udp_StreamDataAborted(ev.As<UdpEventStreamAbort>());
				break;
			}
		}
	}

	private static void Udp_Connected(UdpConnection udp)
	{
		if (isClient)
		{
			NetworkIdAllocator.Assigned(udp.ConnectionId);
			foreach (Entity entity in _entities)
			{
				Assert.True(entity.NetworkId.Connection == uint.MaxValue);
				entity.NetworkId = new NetworkId(udp.ConnectionId, entity.NetworkId.Entity);
			}
		}
		BoltConnection boltConnection = new BoltConnection(udp)
		{
			AcceptToken = udp.AcceptToken.ToToken(),
			ConnectToken = udp.ConnectToken.ToToken()
		};
		_connections.AddLast(boltConnection);
		_udpPlatform.OnConnect(boltConnection.udpConnection);
		GlobalEventListenerBase.ConnectedInvoke(boltConnection);
		if (_config.scopeMode != ScopeMode.Automatic)
		{
			return;
		}
		foreach (Entity entity2 in _entities)
		{
			boltConnection._entityChannel.CreateOnRemote(entity2);
		}
	}

	private static void Udp_Disconnect(UdpConnection udp)
	{
		BoltConnection boltConnection = (BoltConnection)udp.UserToken;
		boltConnection.DisconnectToken = udp.DisconnectToken.ToToken();
		GlobalEventListenerBase.DisconnectedInvoke(boltConnection);
		if (HasSocket)
		{
			try
			{
				boltConnection.DisconnectedInternal();
			}
			catch (Exception message)
			{
				BoltLog.Error(message);
			}
			_connections.Remove(boltConnection);
			if (boltConnection.udpConnection.IsClient)
			{
				ShutdownImmediate(boltConnection.DisconnectReason);
			}
		}
	}

	private static void Udp_SessionConnected(UdpEventSessionConnected ev)
	{
		GlobalEventListenerBase.SessionConnectedInvoke(ev.Session, ev.Token.ToToken());
	}

	private static void Udp_SessionConnectFailed(UdpEventSessionConnectFailed ev)
	{
		GlobalEventListenerBase.SessionConnectFailedInvoke(ev.Session, ev.Token.ToToken(), ev.Error);
	}

	private static void Udp_SessionCreated(UdpEventSessionCreated ev)
	{
		if (ev.Success)
		{
			GlobalEventListenerBase.SessionCreatedOrUpdatedInvoke(ev.Session);
			AutoLoadScene(AutoLoadSceneOrigin.SESSION_CREATION);
		}
		else
		{
			GlobalEventListenerBase.SessionCreationFailedInvoke(ev.Session, ev.Error);
		}
	}

	private static void Udp_SessionListUpdated(UdpEventSessionListUpdated ev)
	{
		BoltNetwork.UpdateSessionList(ev.SessionList);
	}

	private static void Udp_StreamDataStarted(UdpEventStreamDataStarted ev)
	{
		GlobalEventListenerBase.StreamDataStartedInvoke(ev.Connection.GetBoltConnection(), ev.ChannelName, ev.streamID);
	}

	private static void Udp_StreamDataProgress(UdpEventStreamProgress ev)
	{
		GlobalEventListenerBase.StreamDataProgressInvoke(ev.Connection.GetBoltConnection(), ev.ChannelName, ev.streamID, ev.percent);
	}

	private static void Udp_StreamDataAborted(UdpEventStreamAbort ev)
	{
		GlobalEventListenerBase.StreamDataAbortedInvoke(ev.Connection.GetBoltConnection(), ev.ChannelName, ev.streamID);
	}

	private static void Udp_StreamDataReceived(UdpEventStreamDataReceived ev)
	{
		GlobalEventListenerBase.StreamDataReceivedInvoke(ev.Connection.GetBoltConnection(), ev.StreamData);
	}

	private static void Udp_PacketReceived(UdpEvent ev)
	{
		UdpPacket udpPacket = (UdpPacket)ev.Object1;
		UdpConnection self = (UdpConnection)ev.Object0;
		self.GetBoltConnection().PacketReceived(udpPacket);
	}

	private static void Udp_PacketDelivered(UdpEvent ev)
	{
		UdpPacket udpPacket = (UdpPacket)ev.Object1;
		UdpConnection self = (UdpConnection)ev.Object0;
		using Packet packet = (Packet)udpPacket.UserToken;
		self.GetBoltConnection().PacketDelivered(packet);
	}

	private static void Udp_PacketLost(UdpEvent ev)
	{
		UdpPacket udpPacket = (UdpPacket)ev.Object1;
		UdpConnection self = (UdpConnection)ev.Object0;
		using Packet packet = (Packet)udpPacket.UserToken;
		self.GetBoltConnection().PacketLost(packet);
	}

	private static void Udp_ConnectAttempt(UdpEventConnectAttempt ev)
	{
		GlobalEventListenerBase.ConnectAttemptInvoke(ev.EndPoint, ev.Token.ToToken());
	}

	private static void Udp_SocketStartupFailed(UdpEventStartFailed ev)
	{
		GlobalEventListenerBase.BoltStartFailedInvoke(ev.disconnectReason);
		ev.ResetEvent.Set();
		_udpPlatform.OnStartupFailed();
		BoltNetwork.Shutdown();
	}

	private static void Udp_SocketStartupDone(UdpEventStartDone ev)
	{
		SceneLoadState localSceneLoading = _localSceneLoading;
		GlobalEventListenerBase.BoltStartDoneInvoke();
		InvokeLocalSceneCallbacks(localSceneLoading);
		ev.ResetEvent.Set();
		AutoLoadScene(AutoLoadSceneOrigin.STARTUP);
	}

	private static void Udp_ConnectFailed(UdpEventConnectFailed ev)
	{
		try
		{
			GlobalEventListenerBase.ConnectFailedInvoke(ev.EndPoint, ev.Token.ToToken());
		}
		finally
		{
			if (_udpPlatform.ShutdownOnConnectFailure)
			{
				Shutdown(UdpConnectionDisconnectReason.Error);
			}
		}
	}

	private static void Udp_ConnectRefused(UdpEventConnectRefused ev)
	{
		try
		{
			GlobalEventListenerBase.ConnectRefusedInvoke(ev.EndPoint, ev.Token.ToToken());
		}
		finally
		{
			if (_udpPlatform.ShutdownOnConnectFailure)
			{
				Shutdown();
			}
		}
	}

	private static void Udp_ConnectRequest(UdpEventConnectRequest ev)
	{
		GlobalEventListenerBase.ConnectRequestInvoke(ev.EndPoint, ev.Token.ToToken());
	}

	internal static void UpdateCurrentScene()
	{
		_localSceneLoading = SceneLoadState.DefaultLocal();
	}

	internal static void LoadScene(int index, IProtocolToken token)
	{
		if (!isServer)
		{
			BoltLog.Error("You are not the server, only the server can initiate a scene load");
		}
		else
		{
			LoadSceneInternal(_localSceneLoading.BeginLoad(index, token));
		}
	}

	internal static void LoadSceneSync()
	{
		if (!isClient)
		{
			BoltLog.Error("You are not a client, only on client you can force a scene sync");
			return;
		}
		LinkedList<BoltConnection>.Enumerator enumerator = _connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.ForceSceneSync();
		}
	}

	internal static void LoadSceneInternal(SceneLoadState loading)
	{
		_localSceneLoading = loading;
		BoltSceneLoader.Enqueue(_localSceneLoading);
	}

	internal static void SceneLoadBegin(SceneLoadState state)
	{
		foreach (Entity entity in _entities)
		{
			if (entity.IsOwner && !entity.PersistsOnSceneLoad)
			{
				DestroyForce(entity);
			}
		}
		_sceneObjects = new Dictionary<UniqueId, BoltEntity>();
		UpdateActiveGlobalBehaviours(state.Scene.Index);
		GlobalEventListenerBase.SceneLoadLocalBeginInvoke(BoltNetworkInternal.GetSceneName(state.Scene.Index), state.Token);
		if (state.Scene == _localSceneLoading.Scene)
		{
			_localSceneLoading.State = 1;
		}
	}

	internal static void SceneLoadDone(SceneLoadState state)
	{
		if (state.Scene == _localSceneLoading.Scene && _localSceneLoading.State == 1)
		{
			_localSceneLoading.State = 2;
			InvokeLocalSceneCallbacks(_localSceneLoading);
		}
	}

	private static void InvokeLocalSceneCallbacks(SceneLoadState state)
	{
		if (state.Scene.Valid && state.State == 2)
		{
			UpdateSceneObjectsLookup();
			GlobalEventListenerBase.SceneLoadLocalDoneInvoke(BoltNetworkInternal.GetSceneName(state.Scene.Index), state.Token);
		}
	}

	private static void InvokeRemoteSceneCallbacks()
	{
		if (_localSceneLoading.State != 2)
		{
			return;
		}
		LinkedList<BoltConnection>.Enumerator enumerator = _connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BoltConnection current = enumerator.Current;
			bool flag = current._remoteSceneLoading.Scene == _localSceneLoading.Scene;
			bool flag2 = current._remoteSceneLoading.State == 2;
			if (!(flag && flag2))
			{
				continue;
			}
			try
			{
				if (BoltNetwork.IsServer)
				{
					GlobalEventListenerBase.SceneLoadRemoteDoneInvoke(current, current.ConnectToken);
				}
				else
				{
					GlobalEventListenerBase.SceneLoadRemoteDoneInvoke(current, _localSceneLoading.Token);
				}
			}
			finally
			{
				current._remoteSceneLoading.State = 3;
			}
		}
	}

	internal static void UpdateSceneObjectsLookup()
	{
		_sceneObjects = (from BoltEntity x in UnityEngine.Object.FindObjectsOfType(typeof(BoltEntity))
			where x.SceneGuid != UniqueId.None
			select x).ToDictionary((BoltEntity x) => x.SceneGuid);
		BoltLog.Debug("Found {0} Scene Objects", _sceneObjects.Count);
		foreach (BoltEntity value in _sceneObjects.Values)
		{
			if (isServer && !value.IsAttached && value._sceneObjectAutoAttach)
			{
				BoltEntity component = Attach(value.gameObject, EntityFlags.SCENE_OBJECT).GetComponent<BoltEntity>();
				component.Entity.SceneId = value.SceneGuid;
			}
		}
	}

	internal static GameObject FindSceneObject(UniqueId uniqueId)
	{
		if (_sceneObjects.TryGetValue(uniqueId, out var value))
		{
			return value.gameObject;
		}
		return null;
	}

	private static void ResetIdAllocator(BoltNetworkModes mode)
	{
		if (mode == BoltNetworkModes.Server)
		{
			NetworkIdAllocator.Reset(1u);
		}
		else
		{
			NetworkIdAllocator.Reset(uint.MaxValue);
		}
	}

	private static void CreateUdpConfig(BoltConfig config)
	{
		bool flag = _mode == BoltNetworkModes.Server;
		_udpConfig = new UdpConfig
		{
			IPv6 = config.EnableIPv6,
			PacketWindow = 512,
			StreamWindow = config.packetStreamSize / 4,
			ConnectionTimeout = (uint)config.connectionTimeout,
			ConnectRequestAttempts = (uint)config.connectionRequestAttempts,
			ConnectRequestTimeout = (uint)config.connectionRequestTimeout,
			ConnectRequestLANAttempts = (uint)config.connectionLocalRequestAttempts,
			ConnectionLimit = (flag ? config.serverConnectionLimit : 0),
			AllowIncommingConnections = flag,
			AutoAcceptIncommingConnections = (flag && config.serverConnectionAcceptMode == BoltConnectionAcceptMode.Auto),
			PingTimeout = (uint)((float)localSendRate * 1.5f * frameDeltaTime * 1000f),
			PacketDatagramSize = Mathf.Clamp(config.packetSize, 1024, 4096),
			StreamDatagramSize = Mathf.Clamp(config.packetStreamSize, 512, 1200),
			IsBuildMono = UnitySettings.IsBuildMono,
			IsBuildDotNet = UnitySettings.IsBuildDotNet,
			IsBuildIL2CPP = UnitySettings.IsBuildIL2CPP,
			CurrentPlatform = UnitySettings.CurrentPlatform,
			RoomCreateTimeout = BoltRuntimeSettings.instance.RoomCreateTimeout,
			RoomJoinTimeout = BoltRuntimeSettings.instance.RoomJoinTimeout,
			NatPunchEnabled = BoltRuntimeSettings.instance.photonUsePunch
		};
		if (config.useNetworkSimulation)
		{
			_udpConfig.SimulatedLoss = Mathf.Clamp01(config.simulatedLoss);
			_udpConfig.SimulatedPingMin = Mathf.Max(0, (config.simulatedPingMean >> 1) - (config.simulatedPingJitter >> 1));
			_udpConfig.SimulatedPingMax = Mathf.Max(0, (config.simulatedPingMean >> 1) + (config.simulatedPingJitter >> 1));
			switch (config.simulatedRandomFunction)
			{
			case BoltRandomFunction.PerlinNoise:
				_udpConfig.NoiseFunction = CreatePerlinNoise();
				break;
			case BoltRandomFunction.SystemRandom:
				_udpConfig.NoiseFunction = CreateRandomNoise();
				break;
			}
		}
	}

	private static Func<float> CreatePerlinNoise()
	{
		float x = UnityEngine.Random.value;
		Stopwatch s = Stopwatch.StartNew();
		return () => Mathf.PerlinNoise(x, (float)s.Elapsed.TotalSeconds);
	}

	private static Func<float> CreateRandomNoise()
	{
		System.Random r = new System.Random();
		return () => (float)r.NextDouble();
	}

	private static void UdpLogWriter(uint level, string message)
	{
		switch (level)
		{
		case 4u:
		case 8u:
			BoltLog.Debug(message);
			break;
		case 1u:
			BoltLog.Info(message);
			break;
		case 16u:
			BoltLog.Warn(message);
			break;
		case 0u:
			BoltLog.Error(message);
			break;
		}
	}

	private static void UnityLogCallback(string condition, string stackTrace, LogType type)
	{
		stackTrace = (stackTrace ?? "").Trim();
		switch (type)
		{
		case LogType.Error:
		case LogType.Assert:
		case LogType.Exception:
			BoltLog.Error(condition);
			if (stackTrace.Length > 0)
			{
				BoltLog.Error(stackTrace);
			}
			break;
		case LogType.Log:
			BoltLog.Info(condition);
			if (stackTrace.Length > 0)
			{
				BoltLog.Info(stackTrace);
			}
			break;
		case LogType.Warning:
			BoltLog.Warn(condition);
			if (stackTrace.Length > 0)
			{
				BoltLog.Warn(stackTrace);
			}
			break;
		}
	}

	private static void DoProtocolTokenRegistration()
	{
		BoltNetwork.RegisterTokenClass<BoltDisconnectToken>();
		BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
		BoltProtocolTokenRegistry instance = BoltProtocolTokenRegistry.Instance;
		if (instance != null)
		{
			BoltProtocolTokenRegistry.TokenRegistry[] protocolTokenRegistry = instance.protocolTokenRegistry;
			for (int i = 0; i < protocolTokenRegistry.Length; i++)
			{
				BoltProtocolTokenRegistry.TokenRegistry tokenRegistry = protocolTokenRegistry[i];
				Type type = Type.GetType(tokenRegistry.AssemblyName);
				if (type != null)
				{
					Factory.RegisterTokenClass(type);
				}
				else
				{
					BoltLog.Warn("Protocol Token of Type {0} found on regitry but not found on Assembly. Update your Protocol Token Registry", tokenRegistry.DisplayName);
				}
			}
		}
		else
		{
			BoltLog.Info("{0} not found, system disabled.", "BoltProtocolTokenRegistry");
		}
	}

	internal static void Poll()
	{
		if (HasSocket)
		{
			AdjustLocalFrame();
			_frame++;
			PollNetwork();
			InvokeRemoteSceneCallbacks();
			AdjustEstimatedRemoteFrames();
			StepNonControlledRemoteEntities();
			StepControlledEntities();
			EventDispatcher.DispatchAllEvents();
		}
	}

	internal static void Send()
	{
		if (!HasSocket)
		{
			return;
		}
		LinkedList<BoltConnection>.Enumerator enumerator;
		if (_config.scopeMode == ScopeMode.Automatic)
		{
			BoltIterator<Entity> iterator = _entitiesOK.GetIterator();
			while (iterator.Next())
			{
				if (!iterator.val.IsFrozen)
				{
					enumerator = _connections.GetEnumerator();
					while (enumerator.MoveNext())
					{
						enumerator.Current._entityChannel.CreateOnRemote(iterator.val);
					}
				}
			}
		}
		_timer3.Stop();
		_timer3.Reset();
		_timer3.Start();
		BoltPhysics.SnapshotWorld();
		_timer3.Stop();
		DebugInfo.PhysicsSnapshotTime = DebugInfo.GetStopWatchElapsedMilliseconds(_timer3);
		if (_frame % framesPerSecond == 0)
		{
			DebugInfo.SendCommandPackTime = 0f;
			DebugInfo.SendStatePackTime = 0f;
			enumerator = _connections.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.SwitchPerfCounters();
			}
		}
		enumerator = _connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			BoltConnection current = enumerator.Current;
			int num = localSendRate * current.SendRateMultiplier;
			if (_frame % num == 0)
			{
				current.Send();
				if (current.SendRateMultiplier != 1)
				{
					BoltLog.Debug("Send Rate: Update sent every {0} frames (Normal Rate {1} * Rate Multiplier {2})", num, localSendRate, current.SendRateMultiplier);
				}
			}
		}
	}
}
