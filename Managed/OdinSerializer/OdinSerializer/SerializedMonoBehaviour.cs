using OdinSerializer.Utilities;
using UnityEngine;

namespace OdinSerializer;

public abstract class SerializedMonoBehaviour : MonoBehaviour, ISerializationCallbackReceiver, ISupportsPrefabSerialization
{
	[SerializeField]
	[HideInInspector]
	private SerializationData serializationData;

	SerializationData ISupportsPrefabSerialization.SerializationData
	{
		get
		{
			return serializationData;
		}
		set
		{
			serializationData = value;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (!this.SafeIsUnityNull())
		{
			UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
			OnAfterDeserialize();
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (!this.SafeIsUnityNull())
		{
			OnBeforeSerialize();
			UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
		}
	}

	protected virtual void OnAfterDeserialize()
	{
	}

	protected virtual void OnBeforeSerialize()
	{
	}
}
