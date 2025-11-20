using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using DCD.Client.Simulation;
using UnityEngine;

namespace Platforms.Utils;

public static class SerializationExtensions
{
	public static byte[] ToByteArray<T>(this T obj)
	{
		if ((typeof(T).Attributes & TypeAttributes.Serializable) == 0)
		{
			throw new Exception("Wrong data type. Data should be [Serializable]");
		}
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		SurrogateSelector surrogateSelector = new SurrogateSelector();
		Vector3SerializationSurrogate surrogate = new Vector3SerializationSurrogate();
		QuaternionSerializationSurrogate surrogate2 = new QuaternionSerializationSurrogate();
		surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), surrogate);
		surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), surrogate2);
		binaryFormatter.SurrogateSelector = surrogateSelector;
		binaryFormatter.Serialize(memoryStream, obj);
		return memoryStream.ToArray();
	}

	public static T FromByteArray<T>(this byte[] arrBytes)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		SurrogateSelector surrogateSelector = new SurrogateSelector();
		Vector3SerializationSurrogate surrogate = new Vector3SerializationSurrogate();
		QuaternionSerializationSurrogate surrogate2 = new QuaternionSerializationSurrogate();
		surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), surrogate);
		surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), surrogate2);
		binaryFormatter.SurrogateSelector = surrogateSelector;
		memoryStream.Write(arrBytes, 0, arrBytes.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return binaryFormatter.Deserialize(memoryStream).TryCastTo<T>();
	}
}
