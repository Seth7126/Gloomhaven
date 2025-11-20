using OdinSerializer.Utilities;
using UnityEngine;

namespace OdinSerializer;

public abstract class SerializedScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
	[SerializeField]
	[HideInInspector]
	private SerializationData serializationData;

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
