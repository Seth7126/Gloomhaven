#define DEBUG
using System;
using System.Collections;
using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

[Documentation]
[ExecuteInEditMode]
public class BoltEntity : MonoBehaviour, IBoltListNode<BoltEntity>
{
	internal Entity _entity;

	[SerializeField]
	internal string _sceneGuid;

	[SerializeField]
	internal string _serializerGuid;

	[SerializeField]
	internal int _prefabId = -1;

	[SerializeField]
	internal int _updateRate = 1;

	[SerializeField]
	internal int _autoFreezeProxyFrames = 0;

	[SerializeField]
	internal bool _clientPredicted = true;

	[SerializeField]
	internal bool _allowInstantiateOnClient = true;

	[SerializeField]
	internal bool _persistThroughSceneLoads = false;

	[SerializeField]
	internal bool _sceneObjectDestroyOnDetach = true;

	[SerializeField]
	internal bool _sceneObjectAutoAttach = true;

	[SerializeField]
	internal bool _alwaysProxy = false;

	[SerializeField]
	internal bool _detachOnDisable = true;

	[SerializeField]
	internal bool _allowFirstReplicationWhenFrozen = false;

	[SerializeField]
	internal bool _autoRemoveChildEntities = false;

	[SerializeField]
	internal QueryComponentOptions _entityBehaviourQueryOption = QueryComponentOptions.UseGlobal;

	[SerializeField]
	internal QueryComponentOptions _entityPriorityCalculatorQueryOption = QueryComponentOptions.None;

	[SerializeField]
	internal QueryComponentOptions _entityReplicationFilterQueryOption = QueryComponentOptions.None;

	BoltEntity IBoltListNode<BoltEntity>.prev { get; set; }

	BoltEntity IBoltListNode<BoltEntity>.next { get; set; }

	object IBoltListNode<BoltEntity>.list { get; set; }

	internal Entity Entity
	{
		get
		{
			if (_entity == null)
			{
				throw new BoltException("You can't access any Bolt specific methods or properties on an entity which is detached");
			}
			return _entity;
		}
	}

	internal UniqueId SceneGuid
	{
		get
		{
			return UniqueId.Parse(_sceneGuid);
		}
		set
		{
			_sceneGuid = value.guid.ToString();
		}
	}

	internal UniqueId SerializerGuid
	{
		get
		{
			return UniqueId.Parse(_serializerGuid);
		}
		set
		{
			_serializerGuid = value.guid.ToString();
		}
	}

	public PrefabId PrefabId => new PrefabId(_prefabId);

	public BoltConnection Source => Entity.Source;

	public IProtocolToken AttachToken => Entity.AttachToken;

	public IProtocolToken DetachToken => Entity.DetachToken;

	public IProtocolToken ControlGainedToken => Entity.ControlGainedToken;

	public IProtocolToken ControlLostToken => Entity.ControlLostToken;

	public NetworkId NetworkId => Entity.NetworkId;

	public bool CanFreeze
	{
		get
		{
			return Entity.CanFreeze;
		}
		set
		{
			Entity.CanFreeze = value;
		}
	}

	public BoltConnection Controller => Entity.Controller;

	public bool IsAttached => BoltNetwork.IsRunning && _entity != null && _entity.IsAttached;

	public bool IsControlled => HasControl || Controller != null;

	public bool IsControllerOrOwner => HasControl || IsOwner;

	public bool IsFrozen => Entity.IsFrozen;

	public bool IsSceneObject => Entity.IsSceneObject;

	public bool IsOwner => Entity.IsOwner;

	public bool HasControl => Entity.HasControl;

	public bool HasControlWithPrediction => Entity.HasPredictedControl;

	public bool PersistsOnSceneLoad => Entity.PersistsOnSceneLoad;

	public bool HasParent => Entity.HasParent;

	public bool IsInputQueueFull => Entity.CommandQueue.count == BoltCore._config.commandQueueSize;

	public void ClearInputQueue()
	{
		if (IsAttached)
		{
			Entity.Reset(all: false);
		}
		else
		{
			BoltLog.Warn("Bolt Entity [{0}] is not Attached", Entity);
		}
	}

	public BoltEntitySettingsModifier ModifySettings()
	{
		VerifyNotAttached();
		return new BoltEntitySettingsModifier(this);
	}

	public void SetScopeAll(bool inScope, bool force = false)
	{
		Entity.SetScopeAll(inScope, force);
	}

	public void SetScope(BoltConnection connection, bool inScope, bool force = false)
	{
		Entity.SetScope(connection, inScope, force);
	}

	public void SetParent(BoltEntity parent)
	{
		if ((bool)parent && parent.IsAttached)
		{
			Entity.SetParent(parent.Entity);
		}
		else
		{
			Entity.SetParent(null);
		}
	}

	public void TakeControl()
	{
		Entity.TakeControl(null);
	}

	public void TakeControl(IProtocolToken token)
	{
		Entity.TakeControl(token);
	}

	public void ReleaseControl()
	{
		Entity.ReleaseControl(null);
	}

	public void ReleaseControl(IProtocolToken token)
	{
		Entity.ReleaseControl(token);
	}

	public void AssignControl(BoltConnection connection)
	{
		AssignControl(connection, null);
	}

	public void AssignControl(BoltConnection connection, IProtocolToken token)
	{
		if (IsController(connection))
		{
			BoltLog.Warn("Connection {0} is the controller of entity {1}, skipping assign", connection, Entity);
		}
		else
		{
			Entity.AssignControl(connection, token);
		}
	}

	public void RevokeControl()
	{
		Entity.RevokeControl(null);
	}

	public void RevokeControl(IProtocolToken token)
	{
		Entity.RevokeControl(token);
	}

	public bool IsController(BoltConnection connection)
	{
		return Entity.Controller == connection;
	}

	public bool QueueInput(INetworkCommandData data, bool force = false)
	{
		return Entity.QueueInput(((NetworkCommand_Data)data).RootCommand, force);
	}

	public void Idle(BoltConnection connection, bool idle)
	{
		Entity.SetIdle(connection, idle);
	}

	public void Freeze(bool pause)
	{
		Entity.Freeze(pause);
	}

	public void AddEventListener(MonoBehaviour behaviour)
	{
		Entity.AddEventListener(behaviour);
	}

	public void AddEventCallback<T>(Action<T> callback) where T : Event
	{
		Entity.EventDispatcher.Add(callback);
	}

	public void RemoveEventListener(MonoBehaviour behaviour)
	{
		Entity.RemoveEventListener(behaviour);
	}

	public void RemoveEventCallback<T>(Action<T> callback) where T : Event
	{
		Entity.EventDispatcher.Remove(callback);
	}

	public TState GetState<TState>()
	{
		if (Entity.Serializer is TState)
		{
			return (TState)Entity.Serializer;
		}
		BoltLog.Error("You are trying to access the state of {0} as '{1}'", Entity, typeof(TState));
		return default(TState);
	}

	public bool TryFindState<TState>(out TState state)
	{
		if (Entity.Serializer is TState)
		{
			state = (TState)Entity.Serializer;
			return true;
		}
		state = default(TState);
		return false;
	}

	public bool StateIs<TState>()
	{
		return Entity.Serializer is TState;
	}

	public bool StateIs(Type t)
	{
		return t.IsAssignableFrom(Entity.Serializer.GetType());
	}

	public override string ToString()
	{
		if (IsAttached)
		{
			return Entity.ToString();
		}
		return string.Format("[DetachedEntity {2} SceneId={0} SerializerId={1} {3}]", SceneGuid, SerializerGuid, PrefabId, base.gameObject.name);
	}

	public void DestroyDelayed(float time)
	{
		StartCoroutine(DestroyDelayedInternal(time));
	}

	internal void VerifyNotAttached()
	{
		if (IsAttached)
		{
			throw new InvalidOperationException("You can't modify a BoltEntity behaviour which is attached to Bolt");
		}
	}

	private IEnumerator DestroyDelayedInternal(float time)
	{
		yield return new WaitForSeconds(time);
		if (IsAttached)
		{
			BoltNetwork.Destroy(base.gameObject);
		}
	}

	private void Awake()
	{
		if (Application.isEditor && !Application.isPlaying && SceneGuid == UniqueId.None)
		{
			BoltLog.Warn("Bolt Entity [{0}] in this Scene has an Empty Scene ID, please run 'Generate Scene Object Ids' in the Bolt menu", base.gameObject.name);
		}
	}

	private void OnDisable()
	{
		if (_detachOnDisable && Application.isPlaying)
		{
			OnDestroy();
		}
	}

	private void OnDestroy()
	{
		if ((bool)_entity && _entity.IsAttached && Application.isPlaying)
		{
			if (_entity.IsOwner)
			{
				BoltLog.Warn("{0} is being destroyed/disabled without being detached, forcing detach", Entity);
			}
			else
			{
				BoltLog.Error("{0} is being destroyed/disabled without being detached by the owner, this will cause this peer to disconnect the next time it receives an update for this entity", Entity);
			}
			_entity.Detach();
			_entity = null;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, "BoltEntity Gizmo", allowScaling: true);
	}

	public static implicit operator GameObject(BoltEntity entity)
	{
		return entity?.gameObject;
	}
}
