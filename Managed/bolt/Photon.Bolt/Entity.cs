#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Bolt.Channel;
using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

public class Entity : IBoltListNode<Entity>, IPriorityCalculator, IEntityReplicationFilter
{
	private bool _canQueueCommands = false;

	private bool _canQueueCallbacks = false;

	internal UniqueId SceneId;

	internal NetworkId NetworkId;

	internal PrefabId PrefabId;

	internal EntityFlags Flags;

	internal bool AttachIsRunning;

	internal Vector3 SpawnPosition;

	internal Quaternion SpawnRotation;

	internal Entity Parent;

	internal BoltEntity UnityObject;

	internal BoltConnection Source;

	internal BoltConnection Controller;

	internal IEntitySerializer Serializer;

	internal IEntityBehaviour[] Behaviours;

	internal IPriorityCalculator PriorityCalculator;

	internal IEntityReplicationFilter ReplicationFilter;

	internal bool IsOwner;

	internal bool IsFrozen;

	internal bool AutoRemoveChildEntities;

	internal bool AllowFirstReplicationWhenFrozen;

	internal QueryComponentOptions EntityBehaviourQueryOption;

	internal QueryComponentOptions EntityPriorityCalculatorQueryOption;

	internal QueryComponentOptions EntityReplicationFilterQueryOption;

	internal int UpdateRate;

	internal int LastFrameReceived;

	internal int AutoFreezeProxyFrames;

	internal bool CanFreeze = true;

	internal ushort CommandSequence = 0;

	internal Command CommandLastExecuted = null;

	internal EventDispatcher EventDispatcher = new EventDispatcher();

	internal BoltDoubleList<Command> CommandQueue = new BoltDoubleList<Command>();

	internal BoltDoubleList<EntityProxy> Proxies = new BoltDoubleList<EntityProxy>();

	internal CommandHistory InputsSent = new CommandHistory(8);

	internal CommandHistory InputsReceived = new CommandHistory(24);

	internal CommandHistory ResultsSent = new CommandHistory(8);

	internal CommandHistory ResultsReceived = new CommandHistory(24);

	internal BoltRingBuffer<int> ProcessedCommandFrames = new BoltRingBuffer<int>(30);

	internal BoltRingBuffer<int> ProcessedCommandTypes = new BoltRingBuffer<int>(30);

	private readonly BoltRingBuffer<int> _queuedCommandFrames = new BoltRingBuffer<int>(30);

	private readonly BoltRingBuffer<int> _queuedCommandTypes = new BoltRingBuffer<int>(30);

	private readonly BoltRingBuffer<int> _commandsCount = new BoltRingBuffer<int>(60);

	private int _commandDejitterDelay;

	private bool _dynamicCommandDejitterDelay;

	private IProtocolToken detachToken;

	private IProtocolToken attachToken;

	private IProtocolToken controlLostToken;

	private IProtocolToken controlGainedToken;

	internal IProtocolToken ControlLostToken
	{
		get
		{
			return controlLostToken;
		}
		set
		{
			controlLostToken.Release();
			controlLostToken = value;
		}
	}

	internal IProtocolToken ControlGainedToken
	{
		get
		{
			return controlGainedToken;
		}
		set
		{
			controlGainedToken.Release();
			controlGainedToken = value;
		}
	}

	internal IProtocolToken AttachToken
	{
		get
		{
			return attachToken;
		}
		set
		{
			attachToken.Release();
			attachToken = value;
		}
	}

	internal IProtocolToken DetachToken
	{
		get
		{
			return detachToken;
		}
		set
		{
			detachToken.Release();
			detachToken = value;
		}
	}

	internal int Frame
	{
		get
		{
			if (IsOwner)
			{
				return BoltCore.frame;
			}
			if (HasPredictedControl)
			{
				return BoltCore.frame;
			}
			return Source.RemoteFrame;
		}
	}

	internal int SendRate
	{
		get
		{
			if (IsOwner)
			{
				return UpdateRate * BoltCore.localSendRate;
			}
			return UpdateRate * BoltCore.remoteSendRate;
		}
	}

	internal bool IsSceneObject => Flags & EntityFlags.SCENE_OBJECT;

	internal bool HasParent => Parent != null && Parent.IsAttached;

	internal bool IsAttached => Flags & EntityFlags.ATTACHED;

	internal bool IsDummy => !IsOwner && !HasPredictedControl;

	internal bool HasControl => Flags & EntityFlags.HAS_CONTROL;

	internal bool HasPredictedControl => HasControl && (bool)(Flags & EntityFlags.CONTROLLER_LOCAL_PREDICTION);

	public bool PersistsOnSceneLoad => Flags & EntityFlags.PERSIST_ON_LOAD;

	internal bool CanQueueCommands => _canQueueCommands;

	Entity IBoltListNode<Entity>.prev { get; set; }

	Entity IBoltListNode<Entity>.next { get; set; }

	object IBoltListNode<Entity>.list { get; set; }

	bool IPriorityCalculator.Always => false;

	internal bool QueueInput(Command cmd, bool force = false)
	{
		if (_canQueueCommands)
		{
			Assert.True(HasControl);
			if (force && CommandQueue.count == BoltCore._config.commandQueueSize)
			{
				BoltLog.Warn("Input queue for {0} is full, forcing input", this);
				CommandQueue.RemoveFirst().Free();
			}
			if (cmd.Meta.LimitOnePerFrame)
			{
				for (int i = 0; i < _queuedCommandFrames.count; i++)
				{
					if (_queuedCommandFrames[i] == BoltCore.serverFrame && _queuedCommandTypes[i] == cmd.Meta.TypeId.Value)
					{
						return false;
					}
				}
			}
			if (CommandQueue.count < BoltCore._config.commandQueueSize)
			{
				cmd.ServerFrame = BoltCore.serverFrame;
				cmd.Sequence = (CommandSequence = UdpMath.SeqNext(CommandSequence, 255));
				if (cmd.Meta.LimitOnePerFrame)
				{
					_queuedCommandFrames.Enqueue(cmd.ServerFrame);
					_queuedCommandTypes.Enqueue(cmd.Meta.TypeId.Value);
				}
				try
				{
					CommandQueue.AddLast(cmd);
				}
				catch (InvalidOperationException)
				{
					return false;
				}
				return true;
			}
			BoltLog.Error("Input queue for {0} is full", this);
			return false;
		}
		BoltLog.Error("You can only queue commands to in the 'SimulateController' callback");
		return false;
	}

	internal void TakeControl(IProtocolToken token)
	{
		if (IsOwner)
		{
			if (HasControl)
			{
				BoltLog.Warn("You already have control of {0}", this);
				return;
			}
			RevokeControl(token);
			TakeControlInternal(token);
			Freeze(freeze: false);
		}
		else
		{
			BoltLog.Error("Only the owner of {0} can take control of it", this);
		}
	}

	internal void TakeControlInternal(IProtocolToken token)
	{
		Assert.False(Flags & EntityFlags.HAS_CONTROL);
		Flags |= EntityFlags.HAS_CONTROL;
		Reset();
		ControlGainedToken = token;
		ControlLostToken = null;
		Serializer.OnControlGained();
		GlobalEventListenerBase.ControlOfEntityGainedInvoke(UnityObject);
		IEntityBehaviour[] behaviours = Behaviours;
		foreach (IEntityBehaviour entityBehaviour in behaviours)
		{
			if ((object)entityBehaviour.entity == UnityObject)
			{
				entityBehaviour.ControlGained();
			}
		}
		Freeze(freeze: false);
	}

	internal void ReleaseControl(IProtocolToken token)
	{
		if (IsOwner)
		{
			if (HasControl)
			{
				ReleaseControlInternal(token);
				Freeze(freeze: false);
			}
			else
			{
				BoltLog.Warn("You are not controlling {0}", this);
			}
		}
		else
		{
			BoltLog.Error("You can not release control of {0}, you are not the owner", this);
		}
	}

	internal void ReleaseControlInternal(IProtocolToken token)
	{
		Assert.True(Flags & EntityFlags.HAS_CONTROL);
		Flags &= ~EntityFlags.HAS_CONTROL;
		Reset();
		ControlLostToken = token;
		ControlGainedToken = null;
		Serializer.OnControlLost();
		IEntityBehaviour[] behaviours = Behaviours;
		foreach (IEntityBehaviour entityBehaviour in behaviours)
		{
			if ((object)entityBehaviour.entity == UnityObject)
			{
				entityBehaviour.ControlLost();
			}
		}
		GlobalEventListenerBase.ControlOfEntityLostInvoke(UnityObject);
		Freeze(freeze: false);
	}

	internal void AssignControl(BoltConnection connection, IProtocolToken token)
	{
		if (IsOwner)
		{
			if (HasControl)
			{
				ReleaseControl(token);
			}
			Reset();
			Controller = connection;
			Controller._controlling.Add(this);
			Controller._entityChannel.CreateOnRemote(this, out var proxy);
			Controller._entityChannel.ForceSync(this);
			proxy.ControlTokenLost = null;
			proxy.ControlTokenGained = token;
			Freeze(freeze: false);
			BoltLog.Info("Control of entity {0} assigned to connection {1}", this, connection);
		}
		else
		{
			BoltLog.Error("You can not assign control of {0}, you are not the owner", this);
		}
	}

	internal void RevokeControl(IProtocolToken token)
	{
		if (IsOwner)
		{
			if ((bool)Controller)
			{
				Controller._controlling.Remove(this);
				Controller._entityChannel.ForceSync(this, out var proxy);
				Controller = null;
				Reset();
				if (proxy != null)
				{
					proxy.ControlTokenLost = token;
					proxy.ControlTokenGained = null;
				}
				Freeze(freeze: false);
			}
		}
		else
		{
			BoltLog.Error("You can not revoke control of {0}, you are not the owner", this);
		}
	}

	internal void Reset(bool all = true)
	{
		if (all)
		{
			CommandSequence = 0;
			CommandLastExecuted = null;
		}
		CommandQueue.Clear();
	}

	public override string ToString()
	{
		return $"[Entity {NetworkId} {Serializer}]";
	}

	public override bool Equals(object obj)
	{
		return this == obj;
	}

	public override int GetHashCode()
	{
		return NetworkId.GetHashCode();
	}

	internal void SetParent(Entity entity)
	{
		if (IsOwner || HasPredictedControl)
		{
			SetParentInternal(entity);
		}
		else
		{
			BoltLog.Error("You are not allowed to assign the parent of this entity, only the owner or a controller with local prediction can");
		}
	}

	internal void SetParentInternal(Entity entity)
	{
		if (!(entity != Parent))
		{
			return;
		}
		if (entity != null && !entity.IsAttached)
		{
			BoltLog.Error("You can't assign a detached entity as the parent of another entity");
			return;
		}
		try
		{
			Serializer.OnParentChanging(entity, Parent);
		}
		finally
		{
			Parent = entity;
		}
	}

	internal void SetScopeAll(bool inScope, bool force = false)
	{
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SetScope(enumerator.Current, inScope, force);
		}
	}

	internal void SetScope(BoltConnection connection, bool inScope, bool force = false)
	{
		if (IsController(connection) && !inScope && !force)
		{
			BoltLog.Error("It's not possible to remove from scope the entity {0} from it's controller connection {0}, skipping.", this, connection);
		}
		else
		{
			connection._entityChannel.SetScope(this, inScope);
		}
	}

	internal void Freeze(bool freeze)
	{
		if (IsFrozen != freeze)
		{
			if (IsFrozen)
			{
				IsFrozen = false;
				BoltCore._entitiesFZ.Remove(this);
				BoltCore._entitiesOK.AddLast(this);
				GlobalEventListenerBase.EntityThawedInvoke(UnityObject);
			}
			else if (CanFreeze)
			{
				IsFrozen = true;
				BoltCore._entitiesOK.Remove(this);
				BoltCore._entitiesFZ.AddLast(this);
				GlobalEventListenerBase.EntityFrozenInvoke(UnityObject);
			}
		}
	}

	internal EntityProxy CreateProxy()
	{
		EntityProxy entityProxy = new EntityProxy();
		entityProxy.Entity = this;
		entityProxy.Combine(Serializer.GetDefaultMask());
		Proxies.AddLast(entityProxy);
		Serializer.InitProxy(entityProxy);
		return entityProxy;
	}

	internal void Attach()
	{
		Assert.NotNull(UnityObject);
		Assert.False(IsAttached);
		Assert.True(NetworkId.Packed == 0L || Source != null);
		try
		{
			AttachIsRunning = true;
			UnityEngine.Object.DontDestroyOnLoad(UnityObject.gameObject);
			if (Source == null)
			{
				NetworkId = NetworkIdAllocator.Allocate();
			}
			BoltCore._entitiesOK.AddLast(this);
			Flags |= EntityFlags.ATTACHED;
			IEntityBehaviour[] behaviours = Behaviours;
			foreach (IEntityBehaviour entityBehaviour in behaviours)
			{
				try
				{
					if (entityBehaviour.invoke && (object)entityBehaviour.entity == UnityObject)
					{
						entityBehaviour.Attached();
					}
				}
				catch (Exception exception)
				{
					BoltLog.Error("User code threw exception inside Attached callback");
					BoltLog.Exception(exception);
				}
			}
			try
			{
				GlobalEventListenerBase.EntityAttachedInvoke(UnityObject);
			}
			catch (Exception exception2)
			{
				BoltLog.Error("User code threw exception inside Attached callback");
				BoltLog.Exception(exception2);
			}
			BoltLog.Debug("Attached {0} (Token: {1})", this, AttachToken);
		}
		finally
		{
			AttachIsRunning = false;
		}
	}

	internal void Detach()
	{
		Assert.NotNull(UnityObject);
		Assert.True(IsAttached);
		Assert.True(NetworkId.Packed != 0);
		if (!IsAttached || UnityObject == null)
		{
			return;
		}
		if (AutoRemoveChildEntities)
		{
			Component[] componentsInChildren = UnityObject.GetComponentsInChildren(typeof(BoltEntity), includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				BoltEntity boltEntity = (BoltEntity)componentsInChildren[i];
				if (boltEntity.IsAttached && (object)boltEntity._entity != this)
				{
					boltEntity.transform.parent = null;
				}
			}
		}
		if ((bool)Controller)
		{
			RevokeControl(null);
		}
		LinkedList<BoltConnection>.Enumerator enumerator = BoltCore._connections.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current._entityChannel.DestroyOnRemote(this);
		}
		IEntityBehaviour[] behaviours = Behaviours;
		foreach (IEntityBehaviour entityBehaviour in behaviours)
		{
			try
			{
				if (entityBehaviour.invoke && (object)entityBehaviour.entity == UnityObject)
				{
					entityBehaviour.Detached();
					entityBehaviour.entity = null;
				}
			}
			catch (Exception exception)
			{
				BoltLog.Error("User code threw exception inside Detach callback");
				BoltLog.Exception(exception);
			}
		}
		try
		{
			GlobalEventListenerBase.EntityDetachedInvoke(UnityObject);
		}
		catch (Exception exception2)
		{
			BoltLog.Error("User code threw exception inside Detach callback");
			BoltLog.Exception(exception2);
		}
		Flags &= ~EntityFlags.ATTACHED;
		if (BoltCore._entitiesFZ.Contains(this))
		{
			BoltCore._entitiesFZ.Remove(this);
		}
		if (BoltCore._entitiesOK.Contains(this))
		{
			BoltCore._entitiesOK.Remove(this);
		}
		AttachToken = null;
		ControlLostToken = null;
		ControlGainedToken = null;
		UnityObject._entity = null;
		BoltLog.Debug("Detached {0}", this);
	}

	internal void AddEventListener(MonoBehaviour behaviour)
	{
		EventDispatcher.Add(behaviour);
	}

	internal void RemoveEventListener(MonoBehaviour behaviour)
	{
		EventDispatcher.Remove(behaviour);
	}

	internal bool IsController(BoltConnection connection)
	{
		return connection != null && Controller != null && Controller == connection;
	}

	internal void Render()
	{
		Serializer.OnRender();
	}

	private T[] QueryComponents<T>(QueryComponentOptions queryComponentOption, bool queryIncludesInactiveChildren, bool returnNullOnEmpty)
	{
		T[] result = null;
		switch (queryComponentOption)
		{
		case QueryComponentOptions.None:
			if (!returnNullOnEmpty)
			{
				result = new T[0];
			}
			break;
		case QueryComponentOptions.Components:
			result = UnityObject.GetComponents<T>();
			break;
		case QueryComponentOptions.ComponentsInChildren:
			result = UnityObject.GetComponentsInChildren<T>(queryIncludesInactiveChildren);
			break;
		case QueryComponentOptions.Component:
		{
			T component = UnityObject.GetComponent<T>();
			if (component != null)
			{
				result = new T[1] { component };
			}
			else if (!returnNullOnEmpty)
			{
				result = new T[0];
			}
			break;
		}
		case QueryComponentOptions.UseGlobal:
			BoltLog.Error("QueryComponents <T>: UseGlobal is set!  This should never happen.  Defaulting to GetComponentsInChildren<>.");
			result = UnityObject.GetComponentsInChildren<T>(queryIncludesInactiveChildren);
			break;
		}
		return result;
	}

	internal void Initialize()
	{
		IsOwner = Source == null;
		Behaviours = QueryComponents<IEntityBehaviour>(EntityBehaviourQueryOption, queryIncludesInactiveChildren: true, returnNullOnEmpty: false);
		UnityObject._entity = this;
		IPriorityCalculator[] array = QueryComponents<IPriorityCalculator>(EntityPriorityCalculatorQueryOption, queryIncludesInactiveChildren: true, returnNullOnEmpty: true);
		if (array != null)
		{
			IPriorityCalculator[] array2 = array;
			foreach (IPriorityCalculator priorityCalculator in array2)
			{
				BoltEntity componentInParent = ((MonoBehaviour)priorityCalculator).GetComponentInParent<BoltEntity>();
				if ((bool)componentInParent && (object)componentInParent._entity == this)
				{
					PriorityCalculator = priorityCalculator;
					break;
				}
			}
		}
		if (PriorityCalculator == null)
		{
			PriorityCalculator = this;
		}
		IEntityReplicationFilter[] array3 = QueryComponents<IEntityReplicationFilter>(EntityReplicationFilterQueryOption, queryIncludesInactiveChildren: true, returnNullOnEmpty: true);
		if (array3 != null)
		{
			IEntityReplicationFilter[] array4 = array3;
			foreach (IEntityReplicationFilter entityReplicationFilter in array4)
			{
				BoltEntity componentInParent2 = ((MonoBehaviour)entityReplicationFilter).GetComponentInParent<BoltEntity>();
				if ((bool)componentInParent2 && (object)componentInParent2._entity == this)
				{
					ReplicationFilter = entityReplicationFilter;
					break;
				}
			}
		}
		if (ReplicationFilter == null)
		{
			ReplicationFilter = this;
		}
		Serializer.OnInitialized();
		IEntityBehaviour[] behaviours = Behaviours;
		foreach (IEntityBehaviour entityBehaviour in behaviours)
		{
			if (entityBehaviour.invoke && (object)entityBehaviour.entity == UnityObject)
			{
				entityBehaviour.Initialized();
			}
		}
		ProcessedCommandFrames.autofree = true;
		ProcessedCommandTypes.autofree = true;
		_queuedCommandFrames.autofree = true;
		_queuedCommandTypes.autofree = true;
		if (BoltCore._config.commandDejitterDelay > 0)
		{
			_commandDejitterDelay = BoltCore._config.commandDejitterDelay;
			_dynamicCommandDejitterDelay = false;
		}
		else
		{
			_commandDejitterDelay = BoltCore._config.commandDejitterDelayMax;
			_dynamicCommandDejitterDelay = true;
		}
	}

	internal void SetIdle(BoltConnection connection, bool idle)
	{
		if (idle && IsController(connection))
		{
			BoltLog.Error("You can not idle {0} on {1}, as it is the controller for this entity", this, connection);
		}
		else
		{
			connection._entityChannel.SetIdle(this, idle);
		}
	}

	internal void Simulate()
	{
		Serializer.OnSimulateBefore();
		if (IsOwner)
		{
			IEntityBehaviour[] behaviours = Behaviours;
			foreach (IEntityBehaviour entityBehaviour in behaviours)
			{
				try
				{
					if (entityBehaviour != null && (bool)(MonoBehaviour)entityBehaviour && entityBehaviour.invoke && (object)entityBehaviour.entity == UnityObject)
					{
						entityBehaviour.SimulateOwner();
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		if (HasControl)
		{
			Assert.Null(Controller);
			BoltIterator<Command> iterator = CommandQueue.GetIterator();
			while (iterator.Next())
			{
				Command val = iterator.val;
				Assert.True(val.Flags & CommandFlags.HAS_EXECUTED);
				bool flag = val == CommandQueue.first;
				if (flag)
				{
					if (LocalAndRemoteResultEqual(val))
					{
						break;
					}
					val.SmoothCorrection();
				}
				ExecuteCommand(val, flag, skipExecuted: false);
			}
			try
			{
				_canQueueCommands = true;
				IEntityBehaviour[] behaviours2 = Behaviours;
				foreach (IEntityBehaviour entityBehaviour2 in behaviours2)
				{
					if (entityBehaviour2.invoke && (object)entityBehaviour2.entity == UnityObject)
					{
						entityBehaviour2.SimulateController();
					}
				}
			}
			finally
			{
				_canQueueCommands = false;
			}
			iterator = CommandQueue.GetIterator();
			while (iterator.Next())
			{
				if (!(iterator.val.Flags & CommandFlags.HAS_EXECUTED))
				{
					ExecuteCommand(iterator.val, resetState: false, skipExecuted: false);
				}
			}
			if (IsOwner)
			{
				while (CommandQueue.count > 0)
				{
					CommandQueue.RemoveFirst().Free();
				}
			}
		}
		else if (IsOwner && Controller != null)
		{
			if (ExecuteCommandsFromRemote(out var lastExecutedCommand, out var lastCommandIteration))
			{
				ExecuteInstantCommand(lastExecutedCommand, lastCommandIteration);
			}
			else
			{
				Command lastOrDefault = CommandQueue.lastOrDefault;
				for (int k = 0; k < Behaviours.Length; k++)
				{
					if ((object)Behaviours[k].entity == UnityObject)
					{
						Behaviours[k].MissingCommand(lastOrDefault);
					}
				}
			}
		}
		Serializer.OnSimulateAfter();
	}

	private bool ExecuteCommandsFromRemote(out Command lastExecutedCommand, out BoltIterator<Command> lastCommandIteration)
	{
		Assert.True(IsOwner);
		lastExecutedCommand = null;
		lastCommandIteration = default(BoltIterator<Command>);
		do
		{
			BoltIterator<Command> iterator = CommandQueue.GetIterator();
			while (iterator.Next())
			{
				Command val = iterator.val;
				if (val.IsFirstExecution)
				{
					try
					{
						ExecuteCommand(val, resetState: false, skipExecuted: false);
						lastExecutedCommand = val;
						lastCommandIteration = iterator;
					}
					finally
					{
						val.Flags |= CommandFlags.SEND_STATE;
					}
					break;
				}
			}
		}
		while (UnexecutedCommandCount() > _commandDejitterDelay);
		if (_dynamicCommandDejitterDelay && CommandQueue.count > 0)
		{
			_commandsCount.Enqueue(CommandQueue.count);
			if (_commandsCount.full)
			{
				double num = _commandsCount.Average();
				if (num > (double)BoltCore._config.commandDejitterDelayMin)
				{
					_commandDejitterDelay = Mathf.Max(BoltCore._config.commandDejitterDelayMin, _commandDejitterDelay - 1);
				}
				else if (num < (double)BoltCore._config.commandDejitterDelayMin)
				{
					_commandDejitterDelay = Mathf.Min(_commandDejitterDelay + 1, BoltCore._config.commandDejitterDelayMax);
				}
				_commandsCount.Clear();
			}
		}
		return lastExecutedCommand != null;
	}

	private void ExecuteInstantCommand(Command lastExecutedCommand, BoltIterator<Command> lastCommandIteration, int instantMaxPredict = 1)
	{
		if (lastExecutedCommand.Meta.IsInstant)
		{
			return;
		}
		Assert.Equal((object)lastExecutedCommand, (object)lastCommandIteration.val);
		BoltIterator<Command> boltIterator = lastCommandIteration;
		int num = 0;
		do
		{
			Command val = boltIterator.val;
			if (!(val.Flags & CommandFlags.HAS_EXECUTED) && val.Meta.IsInstant)
			{
				num++;
				instantMaxPredict--;
			}
		}
		while (instantMaxPredict > 0 && boltIterator.Next());
		if (num <= 0)
		{
			return;
		}
		boltIterator = lastCommandIteration;
		Assert.Equal((object)lastExecutedCommand, (object)boltIterator.val);
		do
		{
			Command val2 = boltIterator.val;
			if ((bool)(val2.Flags & CommandFlags.HAS_EXECUTED))
			{
				continue;
			}
			if (val2.Meta.IsInstant)
			{
				num--;
			}
			try
			{
				ExecuteCommand(val2, resetState: false, !val2.Meta.IsInstant);
			}
			finally
			{
				if (val2.Meta.IsInstant)
				{
					val2.Flags |= CommandFlags.SEND_STATE;
				}
			}
		}
		while (num > 0 && boltIterator.Next());
		ExecuteCommand(lastExecutedCommand, resetState: true, skipExecuted: false);
	}

	private bool LocalAndRemoteResultEqual(Command cmd)
	{
		bool flag = false;
		IEntityBehaviour[] behaviours = Behaviours;
		foreach (IEntityBehaviour entityBehaviour in behaviours)
		{
			if (entityBehaviour.invoke && (object)entityBehaviour.entity == UnityObject)
			{
				flag = flag || entityBehaviour.LocalAndRemoteResultEqual(cmd);
			}
		}
		return flag;
	}

	private void ExecuteCommand(Command cmd, bool resetState, bool skipExecuted)
	{
		try
		{
			_canQueueCallbacks = cmd.IsFirstExecution;
			IEntityBehaviour[] behaviours = Behaviours;
			foreach (IEntityBehaviour entityBehaviour in behaviours)
			{
				if (entityBehaviour.invoke && (object)entityBehaviour.entity == UnityObject)
				{
					entityBehaviour.ExecuteCommand(cmd, resetState);
				}
			}
		}
		finally
		{
			_canQueueCallbacks = false;
			if (!skipExecuted)
			{
				cmd.Flags |= CommandFlags.HAS_EXECUTED;
			}
		}
	}

	private int UnexecutedCommandCount()
	{
		int num = 0;
		BoltIterator<Command> iterator = CommandQueue.GetIterator();
		while (iterator.Next())
		{
			if (iterator.val.IsFirstExecution)
			{
				num++;
			}
		}
		return num;
	}

	internal static Entity CreateFor(PrefabId prefabId, TypeId serializerId, Vector3 position, Quaternion rotation)
	{
		return CreateFor(BoltCore.PrefabPool.Instantiate(prefabId, position, rotation), prefabId, serializerId);
	}

	internal static Entity CreateFor(GameObject instance, PrefabId prefabId, TypeId serializerId)
	{
		return CreateFor(instance, prefabId, serializerId, EntityFlags.ZERO);
	}

	internal static Entity CreateFor(GameObject instance, PrefabId prefabId, TypeId serializerId, EntityFlags flags)
	{
		Entity entity = new Entity();
		entity.UnityObject = instance.GetComponent<BoltEntity>();
		entity.UpdateRate = entity.UnityObject._updateRate;
		entity.AutoFreezeProxyFrames = entity.UnityObject._autoFreezeProxyFrames;
		entity.AllowFirstReplicationWhenFrozen = entity.UnityObject._allowFirstReplicationWhenFrozen;
		entity.AutoRemoveChildEntities = entity.UnityObject._autoRemoveChildEntities;
		entity.EntityBehaviourQueryOption = GetQueryComponentOption(BoltRuntimeSettings.instance.globalEntityBehaviourQueryOption, entity.UnityObject._entityBehaviourQueryOption);
		entity.EntityPriorityCalculatorQueryOption = GetQueryComponentOption(BoltRuntimeSettings.instance.globalEntityPriorityCalculatorQueryOption, entity.UnityObject._entityPriorityCalculatorQueryOption);
		entity.EntityReplicationFilterQueryOption = GetQueryComponentOption(BoltRuntimeSettings.instance.globalEntityReplicationFilterQueryOption, entity.UnityObject._entityReplicationFilterQueryOption);
		entity.UnityObject._sceneObjectDestroyOnDetach = true;
		entity.PrefabId = prefabId;
		entity.Flags = flags;
		if (prefabId.Value == 0)
		{
			entity.Flags |= EntityFlags.SCENE_OBJECT;
			entity.SceneId = entity.UnityObject.SceneGuid;
		}
		if (entity.UnityObject._persistThroughSceneLoads)
		{
			entity.Flags |= EntityFlags.PERSIST_ON_LOAD;
		}
		if (entity.UnityObject._clientPredicted)
		{
			entity.Flags |= EntityFlags.CONTROLLER_LOCAL_PREDICTION;
		}
		entity.Serializer = Factory.NewSerializer(serializerId);
		entity.Serializer?.OnCreated(entity);
		return entity;
	}

	private static QueryComponentOptions GetQueryComponentOption(QueryComponentOptionsGlobal globalOption, QueryComponentOptions entityOption)
	{
		QueryComponentOptions result = entityOption;
		if (entityOption == QueryComponentOptions.UseGlobal)
		{
			switch (globalOption)
			{
			case QueryComponentOptionsGlobal.None:
				result = QueryComponentOptions.None;
				break;
			case QueryComponentOptionsGlobal.Component:
				result = QueryComponentOptions.Component;
				break;
			case QueryComponentOptionsGlobal.Components:
				result = QueryComponentOptions.Components;
				break;
			case QueryComponentOptionsGlobal.ComponentsInChildren:
				result = QueryComponentOptions.ComponentsInChildren;
				break;
			}
		}
		return result;
	}

	public static implicit operator bool(Entity entity)
	{
		return entity != null;
	}

	public static bool operator ==(Entity a, Entity b)
	{
		return (object)a == b;
	}

	public static bool operator !=(Entity a, Entity b)
	{
		return (object)a != b;
	}

	float IPriorityCalculator.CalculateStatePriority(BoltConnection connection, int skipped)
	{
		return Mathf.Max(1, skipped);
	}

	float IPriorityCalculator.CalculateEventPriority(BoltConnection connection, Event evnt)
	{
		if (HasControl)
		{
			return 3f;
		}
		if (IsController(connection))
		{
			return 2f;
		}
		return 1f;
	}

	bool IEntityReplicationFilter.AllowReplicationTo(BoltConnection connection)
	{
		return true;
	}
}
