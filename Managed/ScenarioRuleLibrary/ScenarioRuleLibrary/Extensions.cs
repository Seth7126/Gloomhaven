using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SharedLibrary;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public static class Extensions
{
	[Serializable]
	public class RandomState
	{
		public readonly byte[] State;

		public RandomState()
		{
		}

		public RandomState(RandomState state, ReferenceDictionary references)
		{
			State = references.Get(state.State);
			if (State == null && state.State != null)
			{
				State = new byte[state.State.Length];
				for (int i = 0; i < state.State.Length; i++)
				{
					State[i] = state.State[i];
				}
			}
		}

		public RandomState(byte[] state)
		{
			State = state;
		}
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(num);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static RandomState Save(this SharedLibrary.Random random)
	{
		if (random != null)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, random);
			return new RandomState(memoryStream.ToArray());
		}
		return null;
	}

	public static SharedLibrary.Random Restore(this RandomState state)
	{
		if (state != null)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter
			{
				Binder = new RandomSerializationBinding()
			};
			using MemoryStream serializationStream = new MemoryStream(state.State);
			return (SharedLibrary.Random)binaryFormatter.Deserialize(serializationStream);
		}
		return null;
	}
}
