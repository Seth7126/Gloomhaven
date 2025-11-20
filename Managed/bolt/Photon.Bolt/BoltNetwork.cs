#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Photon.Bolt.Internal;
using Photon.Bolt.LagCompensation;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

[Documentation]
public static class BoltNetwork
{
	internal static Map<Guid, UdpSession> _sessionList = new Map<Guid, UdpSession>();

	internal static Dictionary<Guid, uint> _sessionListTimeout = new Dictionary<Guid, uint>();

	public static IEnumerable<BoltConnection> Connections => BoltCore.connections;

	public static IEnumerable<BoltConnection> Clients => BoltCore.clients;

	public static AsyncOperation CurrentAsyncOperation => BoltCore.currentAsyncOperation;

	public static IEnumerable<BoltEntity> Entities => BoltCore.entities;

	public static int Frame => BoltCore.frame;

	[Obsolete("This property will be removed in a future update")]
	public static float FrameAlpha => BoltCore.frameAlpha;

	[Obsolete("This property will be removed in a future update")]
	public static float FrameBeginTime => BoltCore.frameBeginTime;

	public static float FrameDeltaTime => BoltCore.frameDeltaTime;

	public static int FramesPerSecond => BoltCore.framesPerSecond;

	public static GameObject GlobalObject
	{
		get
		{
			VerifyIsRunning();
			return BoltCore.globalObject;
		}
	}

	public static bool IsConnected => IsServer || (IsClient && BoltCore._connections.Count > 0);

	public static bool IsClient => BoltCore.isClient;

	public static bool IsDebugMode => BoltCore.IsDebugMode;

	public static bool IsRunning => BoltCore.IsRunning;

	public static bool IsServer => BoltCore.isServer;

	public static bool IsSinglePlayer => BoltCore.isSinglePlayer;

	public static int MaxConnections
	{
		get
		{
			if (IsRunning)
			{
				if (IsSinglePlayer)
				{
					return 0;
				}
				return IsClient ? 1 : BoltCore._config.serverConnectionLimit;
			}
			return -1;
		}
	}

	public static IEnumerable<BoltEntity> SceneObjects => BoltCore._sceneObjects.Values;

	public static ScopeMode ScopeMode
	{
		get
		{
			VerifyIsRunning();
			return BoltCore._config.scopeMode;
		}
	}

	public static BoltConnection Server => BoltCore.server;

	public static int ServerFrame => BoltCore.serverFrame;

	public static float ServerTime => BoltCore.serverTime;

	public static Map<Guid, UdpSession> SessionList => _sessionList;

	public static float Time => BoltCore.time;

	public static UdpSocket UdpSocket => BoltCore._udpSocket;

	public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

	public static string CurrentVersion => $"Bolt {VersionConfiguration} v{Version.Major}.{Version.Minor}.{Version.Build}{VersionInfo}";

	public static string VersionInfo
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(inherit: true);
			return customAttributes.OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ?? "";
		}
	}

	public static string VersionConfiguration
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(inherit: true);
			return customAttributes.OfType<AssemblyConfigurationAttribute>().FirstOrDefault()?.Configuration ?? "";
		}
	}

	public static void AddGlobalEventListener(MonoBehaviour mb)
	{
		VerifyIsRunning();
		BoltCore._globalEventDispatcher.Add(mb);
	}

	public static void AddGlobalEventCallback<T>(Action<T> callback) where T : Event
	{
		VerifyIsRunning();
		BoltCore._globalEventDispatcher.Add(callback);
	}

	public static void RemoveGlobalEventListener(MonoBehaviour mb)
	{
		VerifyIsRunning();
		BoltCore._globalEventDispatcher.Remove(mb);
	}

	public static void RemoveGlobalEventCallback<T>(Action<T> callback) where T : Event
	{
		VerifyIsRunning();
		BoltCore._globalEventDispatcher.Remove(callback);
	}

	public static void EnableLanBroadcast(ushort port = 60000)
	{
		RuntimePlatform currentPlatform = UnitySettings.CurrentPlatform;
		RuntimePlatform runtimePlatform = currentPlatform;
		if (runtimePlatform == RuntimePlatform.Switch)
		{
			BoltLog.Debug("EnableLanBroadcast is not supported on platform {1}", UnitySettings.CurrentPlatform.ToString());
			return;
		}
		VerifyIsRunning();
		BoltCore.EnableLanBroadcast(port);
	}

	public static void DisableLanBroadcast()
	{
		VerifyIsRunning();
		BoltCore.DisableLanBroadcast();
	}

	public static void RegisterTokenClass<T>() where T : class, IProtocolToken, new()
	{
		VerifyIsRunning();
		Factory.RegisterTokenClass(typeof(T));
	}

	public static void SetEventFilter(IEventFilter filter)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		BoltCore.EventFilter = filter;
	}

	public static void SetNetworkSimulation(float loss, int pingMin, int pingMax)
	{
		if (UdpSocket == null || UdpSocket.Config == null)
		{
			BoltLog.Warn("No valid socket found, make sure that you've received the 'BoltStartDone' callback before calling this method.");
			return;
		}
		UdpSocket.Config.SimulatedLoss = Mathf.Clamp01(loss);
		UdpSocket.Config.SimulatedPingMin = pingMin;
		UdpSocket.Config.SimulatedPingMax = pingMax;
	}

	public static void UpdateSessionList(Map<Guid, UdpSession> sessions)
	{
		VerifyIsRunning();
		if (IsServer || !BoltCore._udpPlatform.SessionListProvidedExternally)
		{
			return;
		}
		foreach (KeyValuePair<Guid, UdpSession> session in sessions)
		{
			if (session.Value.Source != UdpSessionSource.Photon)
			{
				_sessionListTimeout[session.Key] = UdpSocket.GetCurrentTime() + 10000;
			}
		}
		foreach (KeyValuePair<Guid, UdpSession> session2 in _sessionList)
		{
			if (session2.Value.Source == UdpSessionSource.Photon || !_sessionListTimeout.ContainsKey(session2.Key))
			{
				continue;
			}
			if (_sessionListTimeout[session2.Key] > UdpSocket.GetCurrentTime())
			{
				if (!sessions.TryFind(session2.Key, out var _))
				{
					sessions = sessions.Add(session2.Key, session2.Value);
				}
			}
			else
			{
				_sessionListTimeout.Remove(session2.Key);
			}
		}
		BoltCore.SetSessionList(sessions);
	}

	public static void UpdateSceneObjectsLookup()
	{
		VerifyIsRunning();
		BoltCore.UpdateSceneObjectsLookup();
	}

	[Conditional("DEBUG")]
	internal static void VerifyIsRunning()
	{
		if (!IsRunning)
		{
			throw new InvalidOperationException("You can't do this if Bolt is not running");
		}
	}

	public static bool FindConnection(uint connectionId, out BoltConnection boltConnection)
	{
		VerifyIsRunning();
		IEnumerator<BoltConnection> enumerator = BoltCore.connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.udpConnection.ConnectionId == connectionId)
			{
				boltConnection = enumerator.Current;
				return true;
			}
		}
		BoltLog.Warn("Could not find a BoltConnection with id {0}", connectionId);
		boltConnection = null;
		return false;
	}

	public static void Accept(UdpEndPoint endpoint)
	{
		VerifyIsRunning();
		BoltCore.AcceptConnection(endpoint, null, null);
	}

	public static void Accept(UdpEndPoint endpoint, IProtocolToken acceptToken)
	{
		VerifyIsRunning();
		BoltCore.AcceptConnection(endpoint, null, acceptToken);
	}

	public static void Connect(ushort port, IProtocolToken token = null)
	{
		BoltCore.Connect(new UdpEndPoint(UdpIPv4Address.Localhost, port), token);
	}

	public static UdpChannelName CreateStreamChannel(string name, UdpChannelMode mode, int priority)
	{
		VerifyIsRunning();
		return BoltCore.CreateStreamChannel(name, mode, priority);
	}

	public static void LoadScene(string scene)
	{
		VerifyIsRunning();
		LoadScene(scene, null);
	}

	public static void LoadScene(string scene, IProtocolToken token)
	{
		VerifyIsRunning();
		int num = -1;
		try
		{
			num = BoltNetworkInternal.GetSceneIndex(scene);
		}
		catch (Exception exception)
		{
			BoltLog.Error("Exception thrown while trying to find index of scene '{0}'", scene);
			BoltLog.Exception(exception);
			return;
		}
		if (num != -1)
		{
			BoltCore.LoadScene(num, token);
		}
		else
		{
			BoltLog.Error("Scene {0} not found, please verify the 'Scene in the Build' and make sure to Compile Bolt", scene);
		}
	}

	public static void UpdateCurrentScene()
	{
		VerifyIsRunning();
		BoltCore.UpdateCurrentScene();
	}

	public static void LoadSceneSync()
	{
		BoltCore.LoadSceneSync();
	}

	public static void Refuse(UdpEndPoint endpoint)
	{
		VerifyIsRunning();
		BoltCore.RefuseConnection(endpoint, null);
	}

	public static void Refuse(UdpEndPoint endpoint, IProtocolToken token)
	{
		VerifyIsRunning();
		BoltCore.RefuseConnection(endpoint, token);
	}

	public static void Shutdown()
	{
		BoltCore.Shutdown();
	}

	public static void ShutdownImmediate()
	{
		BoltCore.ShutdownImmediate();
	}

	public static Vector3 PositionAtFrame(BoltHitboxBody hitboxBody, int frame)
	{
		VerifyIsRunning();
		return BoltPhysics.PositionAtFrame(hitboxBody, frame);
	}

	public static BoltPhysicsHits RaycastAll(Ray ray)
	{
		VerifyIsRunning();
		return BoltPhysics.Raycast(ray);
	}

	public static BoltPhysicsHits RaycastAll(Ray ray, int frame)
	{
		VerifyIsRunning();
		return BoltPhysics.Raycast(ray, frame);
	}

	public static BoltPhysicsHits OverlapSphereAll(Vector3 origin, float radius)
	{
		VerifyIsRunning();
		return BoltPhysics.OverlapSphere(origin, radius);
	}

	public static BoltPhysicsHits OverlapSphereAll(Vector3 origin, float radius, int frame)
	{
		VerifyIsRunning();
		return BoltPhysics.OverlapSphere(origin, radius, frame);
	}

	public static GameObject Attach(GameObject gameObject)
	{
		VerifyIsRunning();
		return Attach(gameObject, null);
	}

	public static GameObject Attach(GameObject gameObject, IProtocolToken token)
	{
		VerifyIsRunning();
		return BoltCore.Attach(gameObject, EntityFlags.ZERO, token);
	}

	public static void Detach(GameObject gameObject)
	{
		VerifyIsRunning();
		Detach(gameObject, null);
	}

	public static void Detach(GameObject gameObject, IProtocolToken token)
	{
		VerifyIsRunning();
		BoltCore.Detach(gameObject.GetComponent<BoltEntity>(), token);
	}

	public static void Destroy(GameObject gameObject)
	{
		Destroy(gameObject, null);
	}

	public static void Destroy(GameObject gameObject, IProtocolToken token)
	{
		if (IsRunning)
		{
			BoltEntity component = gameObject.GetComponent<BoltEntity>();
			if ((bool)component)
			{
				BoltCore.Destroy(component, token);
				return;
			}
			if (token != null)
			{
				BoltLog.Warn("Passing protocol token to destroy call for gameobject without bolt entity, token will be ignored");
			}
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			if (token != null)
			{
				BoltLog.Warn("Passing protocol token to destroy call for gameobject when bolt is not running, token will be ignored");
			}
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public static BoltEntity FindEntity(NetworkId id)
	{
		VerifyIsRunning();
		if (id.Packed == 0)
		{
			return null;
		}
		foreach (Entity entity in BoltCore._entities)
		{
			if (entity.IsAttached && (bool)entity.UnityObject && entity.NetworkId.Packed == id.Packed)
			{
				return entity.UnityObject;
			}
		}
		BoltLog.Warn("Could not find entity with {0}", id);
		return null;
	}

	public static BoltEntity Instantiate(GameObject prefab)
	{
		VerifyIsRunning();
		return Instantiate(prefab, null, Vector3.zero, Quaternion.identity);
	}

	public static BoltEntity Instantiate(GameObject prefab, IProtocolToken token)
	{
		VerifyIsRunning();
		return Instantiate(prefab, token, Vector3.zero, Quaternion.identity);
	}

	public static BoltEntity Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		VerifyIsRunning();
		return Instantiate(prefab, null, position, rotation);
	}

	public static BoltEntity Instantiate(GameObject prefab, IProtocolToken token, Vector3 position, Quaternion rotation)
	{
		VerifyIsRunning();
		BoltEntity component = prefab.GetComponent<BoltEntity>();
		if (!component)
		{
			BoltLog.Error("Prefab '{0}' does not have a Bolt Entity component attached", prefab.name);
			return null;
		}
		if (component.SerializerGuid == UniqueId.None)
		{
			BoltLog.Error("Prefab '{0}' does not have a serializer assigned", prefab.name);
			return null;
		}
		return BoltCore.Instantiate(new PrefabId(component._prefabId), Factory.GetFactory(component.SerializerGuid).TypeId, position, rotation, InstantiateFlags.ZERO, null, token);
	}

	public static BoltEntity Instantiate(PrefabId prefabId)
	{
		VerifyIsRunning();
		return Instantiate(prefabId, null, Vector3.zero, Quaternion.identity);
	}

	public static BoltEntity Instantiate(PrefabId prefabId, IProtocolToken token)
	{
		VerifyIsRunning();
		return Instantiate(prefabId, token, Vector3.zero, Quaternion.identity);
	}

	public static BoltEntity Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation)
	{
		VerifyIsRunning();
		return Instantiate(prefabId, null, position, rotation);
	}

	public static BoltEntity Instantiate(PrefabId prefabId, IProtocolToken token, Vector3 position, Quaternion rotation)
	{
		VerifyIsRunning();
		return Instantiate(BoltCore.PrefabPool.LoadPrefab(prefabId), token, position, rotation);
	}

	public static void PreLoadPrefabDatabase()
	{
		PrefabDatabase.BuildCacheAsync();
	}

	public static void SetPrefabPool(IPrefabPool pool)
	{
		BoltCore.PrefabPool = pool ?? throw new ArgumentNullException("pool");
	}

	public static void SetCanReceiveEntities(bool canReceiveEntities)
	{
		VerifyIsRunning();
		BoltCore._canReceiveEntities = canReceiveEntities;
	}
}
