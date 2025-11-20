#define DEBUG
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

[Documentation]
public abstract class EntityBehaviour : MonoBehaviour, IEntityBehaviour
{
	internal BoltEntity _entity;

	public BoltEntity entity
	{
		get
		{
			if (!_entity)
			{
				Transform parent = base.transform;
				while ((bool)parent && !_entity)
				{
					_entity = parent.GetComponent<BoltEntity>();
					parent = parent.parent;
				}
				if (!_entity)
				{
					BoltLog.Error("Could not find a Bolt Entity component attached to '{0}' or any of its parents", base.gameObject.name);
				}
			}
			return _entity;
		}
		set
		{
			_entity = value;
		}
	}

	public bool invoke => base.enabled;

	public virtual void Initialized()
	{
	}

	public virtual void Attached()
	{
	}

	public virtual void Detached()
	{
	}

	public virtual void SimulateOwner()
	{
	}

	public virtual void SimulateController()
	{
	}

	public virtual void ControlGained()
	{
	}

	public virtual void ControlLost()
	{
	}

	public virtual void MissingCommand(Command previous)
	{
	}

	public virtual void ExecuteCommand(Command command, bool resetState)
	{
	}

	public virtual bool LocalAndRemoteResultEqual(Command command)
	{
		return false;
	}
}
[Documentation(Alias = "Photon.Bolt.EntityBehaviour<TState>")]
public abstract class EntityBehaviour<TState> : EntityBehaviour
{
	public TState state => base.entity.GetState<TState>();
}
