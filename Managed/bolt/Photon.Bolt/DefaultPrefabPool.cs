using UnityEngine;

namespace Photon.Bolt;

public class DefaultPrefabPool : IPrefabPool
{
	GameObject IPrefabPool.Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = Object.Instantiate(((IPrefabPool)this).LoadPrefab(prefabId), position, rotation);
		gameObject.GetComponent<BoltEntity>().enabled = true;
		return gameObject;
	}

	GameObject IPrefabPool.LoadPrefab(PrefabId prefabId)
	{
		return PrefabDatabase.Find(prefabId);
	}

	void IPrefabPool.Destroy(GameObject gameObject)
	{
		Object.Destroy(gameObject);
	}
}
