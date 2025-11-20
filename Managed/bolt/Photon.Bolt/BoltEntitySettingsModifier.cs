using System;

namespace Photon.Bolt;

[Documentation]
public class BoltEntitySettingsModifier : IDisposable
{
	private readonly BoltEntity _entity;

	public PrefabId prefabId
	{
		get
		{
			return _entity.PrefabId;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._prefabId = value.Value;
		}
	}

	public UniqueId sceneId
	{
		get
		{
			return _entity.SceneGuid;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity.SceneGuid = value;
		}
	}

	public UniqueId serializerId
	{
		get
		{
			return _entity.SerializerGuid;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity.SerializerGuid = value;
		}
	}

	public int updateRate
	{
		get
		{
			return _entity._updateRate;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._updateRate = value;
		}
	}

	public int autoFreezeProxyFrames
	{
		get
		{
			return _entity._autoFreezeProxyFrames;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._autoFreezeProxyFrames = value;
		}
	}

	public bool clientPredicted
	{
		get
		{
			return _entity._clientPredicted;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._clientPredicted = value;
		}
	}

	public bool allowInstantiateOnClient
	{
		get
		{
			return _entity._allowInstantiateOnClient;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._allowInstantiateOnClient = value;
		}
	}

	public bool persistThroughSceneLoads
	{
		get
		{
			return _entity._persistThroughSceneLoads;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._persistThroughSceneLoads = value;
		}
	}

	public bool sceneObjectDestroyOnDetach
	{
		get
		{
			return _entity._sceneObjectDestroyOnDetach;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._sceneObjectDestroyOnDetach = value;
		}
	}

	public bool sceneObjectAutoAttach
	{
		get
		{
			return _entity._sceneObjectAutoAttach;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._sceneObjectAutoAttach = value;
		}
	}

	public bool alwaysProxy
	{
		get
		{
			return _entity._alwaysProxy;
		}
		set
		{
			_entity.VerifyNotAttached();
			_entity._alwaysProxy = value;
		}
	}

	internal BoltEntitySettingsModifier(BoltEntity entity)
	{
		_entity = entity;
	}

	void IDisposable.Dispose()
	{
	}
}
