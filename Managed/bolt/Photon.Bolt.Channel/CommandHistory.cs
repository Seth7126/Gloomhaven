using System;
using System.Collections.Generic;
using Photon.Bolt.Collections;

namespace Photon.Bolt.Channel;

internal sealed class CommandHistory
{
	private class CommandRecord : IBoltListNode<CommandRecord>
	{
		internal int Sequence;

		internal NetworkStorage Storage;

		private static readonly ObjectPool<CommandRecord> m_Pool = new ObjectPool<CommandRecord>();

		CommandRecord IBoltListNode<CommandRecord>.prev { get; set; }

		CommandRecord IBoltListNode<CommandRecord>.next { get; set; }

		object IBoltListNode<CommandRecord>.list { get; set; }

		internal static CommandRecord Get(int sequence, NetworkStorage storage)
		{
			CommandRecord commandRecord = m_Pool.Get();
			commandRecord.Sequence = sequence;
			commandRecord.Storage = storage;
			return commandRecord;
		}

		internal void Return()
		{
			Sequence = 0;
			Storage = null;
			m_Pool.Return(this);
		}
	}

	private Dictionary<int, BoltDoubleList<CommandRecord>> m_Records = new Dictionary<int, BoltDoubleList<CommandRecord>>();

	private Dictionary<int, int> m_InvalidSequences = new Dictionary<int, int>();

	private Dictionary<int, int> m_FullSyncCounters = new Dictionary<int, int>();

	private int m_MaxRecordsPerCommandType = 3;

	internal CommandHistory(int maxRecordsPerCommandType)
	{
		m_MaxRecordsPerCommandType = maxRecordsPerCommandType;
	}

	internal int GetSequenceForSmallestDiff(Command command, NetworkCommand_Data data)
	{
		int result = -1;
		int num = 999;
		if (!m_Records.TryGetValue(command.Meta.TypeId.Value, out var value) || value == null || value.count == 0)
		{
			return result;
		}
		for (int i = 0; i < value.count; i++)
		{
			int num2 = value[i].Sequence - command.Sequence;
			if (num2 >= 0 && num2 <= 3)
			{
				continue;
			}
			int diffDistance = command.GetDiffDistance(data, value[i].Storage);
			if (diffDistance < num)
			{
				result = value[i].Sequence;
				num = diffDistance;
				if (num == 0)
				{
					break;
				}
			}
		}
		return result;
	}

	internal NetworkStorage GetStorageForSequence(int commandType, int sequence)
	{
		if (!m_Records.TryGetValue(commandType, out var value) || value == null || value.count == 0)
		{
			return null;
		}
		for (int i = 0; i < value.count; i++)
		{
			if (value[i].Sequence == sequence)
			{
				return value[i].Storage;
			}
		}
		return null;
	}

	internal void Add(Command command)
	{
		int value = command.Meta.TypeId.Value;
		int sequence = command.Sequence;
		if (!m_Records.TryGetValue(value, out var value2) || value2 == null)
		{
			value2 = new BoltDoubleList<CommandRecord>();
			m_Records[value] = value2;
		}
		while (value2.count >= m_MaxRecordsPerCommandType)
		{
			CommandRecord commandRecord = value2.RemoveLast();
			if (commandRecord.Storage != null)
			{
				command.FreeStorage(commandRecord.Storage);
			}
			commandRecord.Return();
		}
		value2.AddFirst(CommandRecord.Get(command.Sequence, command.DuplicateStorage(command.Storage)));
	}

	internal bool Contains(Command command)
	{
		int value = command.Meta.TypeId.Value;
		int sequence = command.Sequence;
		if (!m_Records.TryGetValue(value, out var value2) || value2 == null || value2.count == 0)
		{
			return false;
		}
		for (int i = 0; i < value2.count; i++)
		{
			if (value2[i].Sequence == sequence)
			{
				return true;
			}
		}
		return false;
	}

	internal bool Remove(Command command, int sequence)
	{
		int value = command.Meta.TypeId.Value;
		if (!m_Records.TryGetValue(value, out var value2) || value2 == null)
		{
			return false;
		}
		for (int i = 0; i < value2.count; i++)
		{
			if (value2[i].Sequence == sequence)
			{
				CommandRecord commandRecord = value2.Remove(value2[i]);
				if (commandRecord.Storage != null)
				{
					command.FreeStorage(commandRecord.Storage);
				}
				commandRecord.Return();
				return true;
			}
		}
		return false;
	}

	internal bool ShouldDeltaCompress(int commandType, int fullSyncSendRate)
	{
		fullSyncSendRate = Math.Max(1, fullSyncSendRate);
		m_FullSyncCounters.TryGetValue(commandType, out var value);
		for (value++; value >= fullSyncSendRate; value -= fullSyncSendRate)
		{
		}
		m_FullSyncCounters[commandType] = value;
		return value != 0;
	}

	internal void SetInvalidSequence(int commandType, int sequence)
	{
		m_InvalidSequences[commandType] = sequence;
	}

	internal int GetAndRemoveInvalidSequence(int commandType)
	{
		if (m_InvalidSequences.TryGetValue(commandType, out var value))
		{
			m_InvalidSequences.Remove(commandType);
			return value;
		}
		return -1;
	}
}
