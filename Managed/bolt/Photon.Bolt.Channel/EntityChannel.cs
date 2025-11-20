#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Bolt.Collections;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using Photon.Bolt.SceneManagement;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt.Channel;

internal class EntityChannel : BoltChannel
{
	internal class CommandChannel : BoltChannel
	{
		private int inputFullSyncSendRate = 3;

		private int resultFullSyncSendRate = 3;

		private float nextInputSendRateUpdateTime;

		private float nextResultSendRateUpdateTime;

		private int pingFrames => Mathf.CeilToInt(base.connection.udpConnection.AliasedPing * BoltCore._config.commandPingMultiplier / BoltCore.frameDeltaTime);

		private Dictionary<NetworkId, EntityProxy> incommingProxiesByNetworkId => base.connection._entityChannel._incommingDict;

		private Dictionary<NetworkId, EntityProxy> outgoingProxiesByNetworkId => base.connection._entityChannel._outgoingDict;

		public override void Pack(Packet packet)
		{
			BoltCore._timer2.Stop();
			BoltCore._timer2.Reset();
			BoltCore._timer2.Start();
			int position = packet.UdpPacket.Position;
			PackResult(packet);
			PackInput(packet);
			packet.Stats.CommandBits = packet.UdpPacket.Position - position;
			BoltCore._timer2.Stop();
			DebugInfo.SendCommandPackTime += DebugInfo.GetStopWatchElapsedMilliseconds(BoltCore._timer2);
		}

		public override void Read(Packet packet)
		{
			int position = packet.UdpPacket.Position;
			ReadResult(packet);
			ReadInput(packet);
			packet.Stats.CommandBits = packet.UdpPacket.Position - position;
		}

		private bool EntityHasUnsentState(Entity entity)
		{
			BoltIterator<Command> iterator = entity.CommandQueue.GetIterator();
			while (iterator.Next())
			{
				if ((bool)(iterator.val.Flags & CommandFlags.SEND_STATE))
				{
					return true;
				}
			}
			return false;
		}

		private void PackResult(Packet packet)
		{
			Dictionary<NetworkId, EntityProxy>.Enumerator enumerator = outgoingProxiesByNetworkId.GetEnumerator();
			while (enumerator.MoveNext())
			{
				EntityProxy value = enumerator.Current.Value;
				Entity entity = value.Entity;
				if (!(entity != null) || entity.Controller != base.connection || !base.connection._entityChannel.ExistsOnRemote(entity) || (!EntityHasUnsentState(entity) && entity.CommandQueue.count != 1))
				{
					continue;
				}
				Assert.True(entity.IsOwner);
				int position = packet.UdpPacket.Position;
				int num = 0;
				packet.UdpPacket.WriteBool(value: true);
				packet.UdpPacket.WriteNetworkId(value.NetworkId);
				packet.UdpPacket.WriteEntity(value.Entity.Parent);
				BoltIterator<Command> iterator = entity.CommandQueue.GetIterator();
				int count = entity.CommandQueue.count;
				while (iterator.Next())
				{
					Command val = iterator.val;
					if (!(val.Flags & CommandFlags.HAS_EXECUTED) || (!(val.Flags & CommandFlags.SEND_STATE) && count != 1))
					{
						continue;
					}
					int position2 = packet.UdpPacket.Position;
					NetworkCommand_Data resultObject = iterator.val.ResultObject;
					packet.UdpPacket.WriteBool(value: true);
					packet.UdpPacket.WriteTypeId(val.Meta.TypeId);
					packet.UdpPacket.WriteUShort(val.Sequence, 8);
					packet.UdpPacket.WriteToken(resultObject.Token);
					if (val.Meta.EnableInputDeltaCompression)
					{
						WriteInvalidReferenceCommandSequence(packet, val, entity.InputsReceived);
					}
					if (val.Meta.EnableResultDeltaCompression)
					{
						int num2 = -1;
						NetworkStorage networkStorage = null;
						if (!val.AssignedResultDeltaCompression)
						{
							val.ShouldDeltaCompressResult = entity.ResultsSent.ShouldDeltaCompress(val.Meta.TypeId.Value, resultFullSyncSendRate);
						}
						if (!val.ShouldDeltaCompressResult)
						{
							if (!entity.ResultsSent.Contains(val))
							{
								entity.ResultsSent.Add(val);
								if (nextResultSendRateUpdateTime <= Time.realtimeSinceStartup)
								{
									nextResultSendRateUpdateTime = Time.realtimeSinceStartup + 1f;
									resultFullSyncSendRate = System.Math.Min(resultFullSyncSendRate + 1, 10);
								}
							}
						}
						else
						{
							num2 = entity.ResultsSent.GetSequenceForSmallestDiff(val, val.ResultObject);
							if (num2 >= 0)
							{
								networkStorage = entity.ResultsSent.GetStorageForSequence(val.Meta.TypeId.Value, num2);
							}
						}
						if (networkStorage == null)
						{
							packet.UdpPacket.WriteBool(value: true);
							val.PackResult(base.connection, packet.UdpPacket);
						}
						else
						{
							packet.UdpPacket.WriteBool(value: false);
							packet.UdpPacket.WriteUShort((ushort)num2);
							val.PackResultDiff(base.connection, packet.UdpPacket, networkStorage);
						}
					}
					else
					{
						val.PackResult(base.connection, packet.UdpPacket);
					}
					if (packet.UdpPacket.Overflowing)
					{
						packet.UdpPacket.Position = position2;
						break;
					}
					num++;
					val.Flags &= ~CommandFlags.SEND_STATE;
					val.Flags |= CommandFlags.SEND_STATE_PERFORMED;
				}
				if (packet.UdpPacket.Overflowing || num == 0)
				{
					packet.UdpPacket.Position = position;
					break;
				}
				packet.UdpPacket.WriteStopMarker();
				while (entity.CommandQueue.count > 1 && (bool)(entity.CommandQueue.first.Flags & CommandFlags.SEND_STATE_PERFORMED))
				{
					entity.CommandQueue.RemoveFirst().Free();
				}
			}
			packet.UdpPacket.WriteStopMarker();
		}

		private void ReadResult(Packet packet)
		{
			while (packet.UdpPacket.CanRead() && packet.UdpPacket.ReadBool())
			{
				NetworkId key = packet.UdpPacket.ReadNetworkId();
				Entity parentInternal = packet.UdpPacket.ReadEntity();
				EntityProxy entityProxy = incommingProxiesByNetworkId[key];
				Entity entity = entityProxy.Entity;
				while (packet.UdpPacket.CanRead() && packet.UdpPacket.ReadBool())
				{
					TypeId id = packet.UdpPacket.ReadTypeId();
					ushort to = packet.UdpPacket.ReadUShort(8);
					Command command = null;
					if (entity != null)
					{
						BoltIterator<Command> iterator = entity.CommandQueue.GetIterator();
						while (iterator.Next())
						{
							int num = UdpMath.SeqDistance(iterator.val.Sequence, to, 8);
							if (num > 0)
							{
								break;
							}
							if (num < 0)
							{
								iterator.val.Flags |= CommandFlags.DISPOSE;
							}
							if (num == 0)
							{
								command = iterator.val;
								break;
							}
						}
						while (entity.CommandQueue.count > 1 && (bool)(entity.CommandQueue.first.Flags & CommandFlags.DISPOSE))
						{
							entity.CommandQueue.RemoveFirst().Free();
						}
					}
					IProtocolToken token = packet.UdpPacket.ReadToken();
					entity.SetParentInternal(parentInternal);
					bool flag = command != null;
					if (flag)
					{
						command.ResultObject.Token = token;
						command.Flags |= CommandFlags.CORRECTION_RECEIVED;
						if (command.Meta.SmoothFrames > 0)
						{
							command.BeginSmoothing();
						}
					}
					else
					{
						token.Release();
						command = Factory.NewCommand(id);
					}
					if (command.Meta.EnableInputDeltaCompression && ReadAndRemoveInvalidReferenceCommandSequence(packet, command, entity.InputsSent))
					{
						inputFullSyncSendRate = System.Math.Max(1, inputFullSyncSendRate - 1);
					}
					if (command.Meta.EnableResultDeltaCompression)
					{
						if (packet.UdpPacket.ReadBool())
						{
							command.ReadResult(base.connection, packet.UdpPacket);
							if (entityProxy != null && entityProxy.Entity != null && !entityProxy.Entity.ResultsReceived.Contains(command))
							{
								entityProxy.Entity.ResultsReceived.Add(command);
							}
						}
						else
						{
							int sequence = packet.UdpPacket.ReadUShort();
							NetworkStorage networkStorage = null;
							if (entityProxy != null && entityProxy.Entity != null)
							{
								networkStorage = entityProxy.Entity.ResultsReceived.GetStorageForSequence(command.Meta.TypeId.Value, sequence);
							}
							if (networkStorage == null)
							{
								entityProxy.Entity.ResultsReceived.SetInvalidSequence(command.Meta.TypeId.Value, sequence);
							}
							command.ReadResultDiff(base.connection, packet.UdpPacket, networkStorage);
						}
					}
					else
					{
						command.ReadResult(base.connection, packet.UdpPacket);
					}
					if (!flag)
					{
						command.Free();
					}
				}
			}
		}

		private void PackInput(Packet packet)
		{
			Dictionary<NetworkId, EntityProxy>.Enumerator enumerator = incommingProxiesByNetworkId.GetEnumerator();
			while (enumerator.MoveNext())
			{
				EntityProxy value = enumerator.Current.Value;
				Entity entity = value.Entity;
				if (!entity || !entity.HasControl || entity.CommandQueue.count <= 0)
				{
					continue;
				}
				int position = packet.UdpPacket.Position;
				packet.UdpPacket.WriteContinueMarker();
				packet.UdpPacket.WriteNetworkId(value.NetworkId);
				int num = Mathf.Min(entity.CommandQueue.count, BoltCore._config.commandRedundancy);
				if (entity.CommandQueue.count == num && (bool)(entity.CommandQueue.first.Flags & CommandFlags.CORRECTION_RECEIVED))
				{
					num--;
				}
				Command command = entity.CommandQueue.last;
				for (int i = 0; i < num - 1; i++)
				{
					command = entity.CommandQueue.Prev(command);
				}
				for (int j = 0; j < num; j++)
				{
					int position2 = packet.UdpPacket.Position;
					packet.UdpPacket.WriteContinueMarker();
					packet.UdpPacket.WriteTypeId(command.Meta.TypeId);
					packet.UdpPacket.WriteUShort(command.Sequence, 8);
					packet.UdpPacket.WriteIntVB(command.ServerFrame);
					packet.UdpPacket.WriteToken(command.InputObject.Token);
					if (command.Meta.EnableResultDeltaCompression)
					{
						WriteInvalidReferenceCommandSequence(packet, command, entity.ResultsReceived);
					}
					if (command.Meta.EnableInputDeltaCompression)
					{
						int num2 = -1;
						NetworkStorage networkStorage = null;
						if (!command.AssignedInputDeltaCompression)
						{
							command.ShouldDeltaCompressInput = entity.InputsSent.ShouldDeltaCompress(command.Meta.TypeId.Value, inputFullSyncSendRate);
						}
						if (!command.ShouldDeltaCompressInput)
						{
							if (!entity.InputsSent.Contains(command))
							{
								entity.InputsSent.Add(command);
								if (nextInputSendRateUpdateTime <= Time.realtimeSinceStartup)
								{
									nextInputSendRateUpdateTime = Time.realtimeSinceStartup + 1f;
									inputFullSyncSendRate = System.Math.Min(inputFullSyncSendRate + 1, 10);
								}
							}
						}
						else
						{
							num2 = entity.InputsSent.GetSequenceForSmallestDiff(command, command.InputObject);
							if (num2 >= 0)
							{
								networkStorage = entity.InputsSent.GetStorageForSequence(command.Meta.TypeId.Value, num2);
							}
						}
						if (networkStorage == null)
						{
							packet.UdpPacket.WriteBool(value: true);
							command.PackInput(base.connection, packet.UdpPacket);
						}
						else
						{
							packet.UdpPacket.WriteBool(value: false);
							packet.UdpPacket.WriteUShort((ushort)num2);
							command.PackInputDiff(base.connection, packet.UdpPacket, networkStorage);
						}
					}
					else
					{
						command.PackInput(base.connection, packet.UdpPacket);
					}
					command = entity.CommandQueue.Next(command);
					if (packet.UdpPacket.Overflowing)
					{
						packet.UdpPacket.Position = position2;
						break;
					}
				}
				if (packet.UdpPacket.Overflowing)
				{
					packet.UdpPacket.Position = position;
					break;
				}
				packet.UdpPacket.WriteStopMarker();
			}
			packet.UdpPacket.WriteStopMarker();
		}

		private void ReadInput(Packet packet)
		{
			while (packet.UdpPacket.ReadStopMarker())
			{
				NetworkId key = packet.UdpPacket.ReadNetworkId();
				EntityProxy entityProxy = null;
				if (outgoingProxiesByNetworkId.ContainsKey(key))
				{
					entityProxy = outgoingProxiesByNetworkId[key];
				}
				while (packet.UdpPacket.ReadStopMarker())
				{
					Command command = Factory.NewCommand(packet.UdpPacket.ReadTypeId());
					command.Sequence = packet.UdpPacket.ReadUShort(8);
					command.ServerFrame = packet.UdpPacket.ReadIntVB();
					command.InputObject.Token = packet.UdpPacket.ReadToken();
					if (command.Meta.EnableResultDeltaCompression)
					{
						CommandHistory history = ((entityProxy != null && entityProxy.Entity != null) ? entityProxy.Entity.ResultsSent : null);
						if (ReadAndRemoveInvalidReferenceCommandSequence(packet, command, history))
						{
							resultFullSyncSendRate = System.Math.Max(1, resultFullSyncSendRate - 1);
						}
					}
					if (command.Meta.EnableInputDeltaCompression)
					{
						if (packet.UdpPacket.ReadBool())
						{
							command.ReadInput(base.connection, packet.UdpPacket);
							if (entityProxy != null && entityProxy.Entity != null && !entityProxy.Entity.InputsReceived.Contains(command))
							{
								entityProxy.Entity.InputsReceived.Add(command);
							}
						}
						else
						{
							int sequence = packet.UdpPacket.ReadUShort();
							NetworkStorage networkStorage = null;
							if (entityProxy != null && entityProxy.Entity != null)
							{
								networkStorage = entityProxy.Entity.InputsReceived.GetStorageForSequence(command.Meta.TypeId.Value, sequence);
							}
							if (networkStorage == null && entityProxy != null && entityProxy.Entity != null)
							{
								entityProxy.Entity.InputsReceived.SetInvalidSequence(command.Meta.TypeId.Value, sequence);
								command.ReadInputDiff(base.connection, packet.UdpPacket, command.Storage);
								command.Free();
								continue;
							}
							if (networkStorage != null)
							{
								command.ReadInputDiff(base.connection, packet.UdpPacket, networkStorage);
							}
						}
					}
					else
					{
						command.ReadInput(base.connection, packet.UdpPacket);
					}
					if (!entityProxy || !entityProxy.Entity)
					{
						command.Free();
						continue;
					}
					Entity entity = entityProxy.Entity;
					if (entity.Controller != base.connection)
					{
						command.Free();
						continue;
					}
					if (UdpMath.SeqDistance(command.Sequence, entity.CommandSequence, 8) <= 0)
					{
						command.Free();
						continue;
					}
					if (command.Meta.LimitOnePerFrame)
					{
						BoltRingBuffer<int> processedCommandFrames = entity.ProcessedCommandFrames;
						BoltRingBuffer<int> processedCommandTypes = entity.ProcessedCommandTypes;
						bool flag = false;
						for (int i = 0; i < processedCommandFrames.count; i++)
						{
							if (processedCommandFrames[i] == command.ServerFrame && processedCommandTypes[i] == command.Meta.TypeId.Value)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							BoltLog.Warn("Skipping command {0} (already queued), Frame: {1}", command.GetType(), command.ServerFrame);
							command.Free();
							continue;
						}
						processedCommandFrames.Enqueue(command.ServerFrame);
						processedCommandTypes.Enqueue(command.Meta.TypeId.Value);
					}
					entity.CommandQueue.AddLast(command);
					entity.CommandSequence = command.Sequence;
				}
			}
		}

		private static void WriteInvalidReferenceCommandSequence(Packet packet, Command command, CommandHistory history)
		{
			int andRemoveInvalidSequence = history.GetAndRemoveInvalidSequence(command.Meta.TypeId.Value);
			if (andRemoveInvalidSequence >= 0)
			{
				packet.UdpPacket.WriteBool(value: true);
				packet.UdpPacket.WriteUShort((ushort)andRemoveInvalidSequence);
			}
			else
			{
				packet.UdpPacket.WriteBool(value: false);
			}
		}

		private static bool ReadAndRemoveInvalidReferenceCommandSequence(Packet packet, Command command, CommandHistory history)
		{
			int num = -1;
			if (packet.UdpPacket.ReadBool())
			{
				num = packet.UdpPacket.ReadUShort();
			}
			if (num >= 0 && history != null && history.Remove(command, num))
			{
				return true;
			}
			return false;
		}

		[Conditional("DEBUG_DELTA_COMPRESSION")]
		private static void LogDeltaCompression(LogType type, string format, params object[] arguments)
		{
			switch (type)
			{
			case LogType.Log:
				BoltLog.Info(format, arguments);
				break;
			case LogType.Warning:
				BoltLog.Warn(format, arguments);
				break;
			case LogType.Error:
				BoltLog.Error(format, arguments);
				break;
			}
		}
	}

	internal EntityLookup _outgoingLookup;

	internal EntityLookup _incommingLookup;

	internal Dictionary<NetworkId, EntityProxy> _outgoingDict;

	internal Dictionary<NetworkId, EntityProxy> _incommingDict;

	private List<EntityProxy> _prioritized;

	public EntityChannel()
	{
		_outgoingDict = new Dictionary<NetworkId, EntityProxy>(2048, NetworkId.EqualityComparer.Instance);
		_incommingDict = new Dictionary<NetworkId, EntityProxy>(2048, NetworkId.EqualityComparer.Instance);
		_outgoingLookup = new EntityLookup(_outgoingDict);
		_incommingLookup = new EntityLookup(_incommingDict);
		_prioritized = new List<EntityProxy>();
	}

	public void ForceSync(Entity en)
	{
		ForceSync(en, out var _);
	}

	public void ForceSync(Entity en, out EntityProxy proxy)
	{
		if (_outgoingDict.TryGetValue(en.NetworkId, out proxy))
		{
			proxy.Flags |= ProxyFlags.FORCE_SYNC;
			proxy.Flags &= ~ProxyFlags.IDLE;
		}
	}

	public bool TryFindProxy(Entity en, out EntityProxy proxy)
	{
		return _incommingDict.TryGetValue(en.NetworkId, out proxy) || _outgoingDict.TryGetValue(en.NetworkId, out proxy);
	}

	public void SetIdle(Entity entity, bool idle)
	{
		if (_outgoingDict.TryGetValue(entity.NetworkId, out var value))
		{
			if (idle)
			{
				value.Flags |= ProxyFlags.IDLE;
			}
			else
			{
				value.Flags &= ~ProxyFlags.IDLE;
			}
		}
	}

	public void SetScope(Entity entity, bool inScope)
	{
		if (BoltCore._config.scopeMode == ScopeMode.Automatic)
		{
			BoltLog.Error("SetScope has no effect when Scope Mode is set to Automatic");
		}
		else
		{
			if (entity.Source == base.connection)
			{
				return;
			}
			if (inScope)
			{
				if (_incommingDict.ContainsKey(entity.NetworkId))
				{
					return;
				}
				if (_outgoingDict.TryGetValue(entity.NetworkId, out var value))
				{
					if ((bool)(value.Flags & ProxyFlags.DESTROY_REQUESTED))
					{
						if ((bool)(value.Flags & ProxyFlags.DESTROY_PENDING))
						{
							value.Flags |= ProxyFlags.DESTROY_IGNORE;
							return;
						}
						value.Flags &= ~ProxyFlags.DESTROY_IGNORE;
						value.Flags &= ~ProxyFlags.DESTROY_REQUESTED;
					}
				}
				else
				{
					CreateOnRemote(entity);
				}
			}
			else if (_outgoingDict.ContainsKey(entity.NetworkId))
			{
				DestroyOnRemote(entity);
			}
		}
	}

	public bool ExistsOnRemote(Entity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (_incommingDict.ContainsKey(entity.NetworkId))
		{
			return true;
		}
		if (_outgoingDict.TryGetValue(entity.NetworkId, out var value))
		{
			return (bool)(value.Flags & ProxyFlags.CREATE_DONE) && !(value.Flags & ProxyFlags.DESTROY_REQUESTED);
		}
		return false;
	}

	public ExistsResult ExistsOnRemote(Entity entity, bool allowMaybe)
	{
		if (entity == null)
		{
			return ExistsResult.No;
		}
		if (_incommingDict.ContainsKey(entity.NetworkId))
		{
			return ExistsResult.Yes;
		}
		if (_outgoingDict.TryGetValue(entity.NetworkId, out var value))
		{
			if ((bool)(value.Flags & ProxyFlags.CREATE_DONE) && !(value.Flags & ProxyFlags.DESTROY_REQUESTED))
			{
				return ExistsResult.Yes;
			}
			if (allowMaybe)
			{
				return ExistsResult.Maybe;
			}
		}
		return ExistsResult.No;
	}

	public bool MightExistOnRemote(Entity entity)
	{
		return _incommingDict.ContainsKey(entity.NetworkId) || _outgoingDict.ContainsKey(entity.NetworkId);
	}

	public void DestroyOnRemote(Entity entity)
	{
		if (_outgoingDict.TryGetValue(entity.NetworkId, out var value))
		{
			if (value.Envelopes.Count == 0 && !(value.Flags & ProxyFlags.CREATE_DONE))
			{
				DestroyOutgoingProxy(value);
				return;
			}
			value.Flags |= ProxyFlags.DESTROY_REQUESTED;
			value.Flags &= ~ProxyFlags.IDLE;
		}
	}

	public void CreateOnRemote(Entity entity)
	{
		CreateOnRemote(entity, out var _);
	}

	public void CreateOnRemote(Entity entity, out EntityProxy proxy)
	{
		if (!_incommingDict.TryGetValue(entity.NetworkId, out proxy) && !_outgoingDict.TryGetValue(entity.NetworkId, out proxy))
		{
			proxy = entity.CreateProxy();
			proxy.NetworkId = entity.NetworkId;
			proxy.Flags = ProxyFlags.CREATE_REQUESTED;
			proxy.Connection = base.connection;
			if (proxy.Entity.IsAttached && !proxy.Entity.UnityObject._sceneObjectDestroyOnDetach)
			{
				proxy.Flags |= ProxyFlags.DESTROY_IGNORE;
			}
			_outgoingDict.Add(proxy.NetworkId, proxy);
			BoltLog.Debug("Created {0} on {1}", entity, base.connection);
		}
	}

	public float GetPriority(Entity entity)
	{
		if (_outgoingDict.TryGetValue(entity.NetworkId, out var value))
		{
			return value.Priority;
		}
		return float.NegativeInfinity;
	}

	public override void Pack(Packet packet)
	{
		BoltCore._timer2.Stop();
		BoltCore._timer2.Reset();
		BoltCore._timer2.Start();
		int position = packet.UdpPacket.Position;
		_prioritized.Clear();
		bool flag = base.connection.IsLoadingMap || BoltSceneLoader.IsLoading || !base.connection._canReceiveEntities;
		Dictionary<NetworkId, EntityProxy>.Enumerator enumerator = _outgoingDict.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EntityProxy value = enumerator.Current.Value;
			if ((bool)(value.Flags & ProxyFlags.DESTROY_REQUESTED))
			{
				if ((bool)(value.Flags & ProxyFlags.DESTROY_PENDING))
				{
					continue;
				}
				value.ClearAll();
				value.Priority = 131072f;
			}
			else
			{
				if (value.Entity.IsFrozen || packet.Number % value.Entity.UpdateRate != 0)
				{
					continue;
				}
				if (value.Envelopes.Count >= 256)
				{
					BoltLog.Error("Envelopes for {0} to {1} full", value, base.connection);
					continue;
				}
				if (!value.Entity.UnityObject._alwaysProxy && flag)
				{
					continue;
				}
				if ((bool)(value.Flags & ProxyFlags.FORCE_SYNC))
				{
					if (!value.Entity.ReplicationFilter.AllowReplicationTo(base.connection))
					{
						continue;
					}
					value.Priority = 1048576f;
				}
				else if ((bool)(value.Flags & ProxyFlags.CREATE_DONE))
				{
					if (value.IsZero || (bool)(value.Flags & ProxyFlags.IDLE))
					{
						continue;
					}
					value.Priority = value.Entity.PriorityCalculator.CalculateStatePriority(base.connection, value.Skipped);
					value.Priority = Mathf.Clamp(value.Priority, 0f, Mathf.Min(65536, BoltCore._config.maxEntityPriority));
				}
				else if (!value.Entity.IsFrozen || value.Entity.AllowFirstReplicationWhenFrozen)
				{
					if (!value.Entity.ReplicationFilter.AllowReplicationTo(base.connection))
					{
						continue;
					}
					value.Priority = 262144f;
				}
			}
			if (value.Entity.IsController(base.connection))
			{
				value.Priority = 524288f;
			}
			_prioritized.Add(value);
		}
		if (_prioritized.Count > 0)
		{
			try
			{
				_prioritized.Sort(EntityProxy.PriorityComparer.Instance);
				int num = 0;
				for (int i = 0; i < _prioritized.Count; i++)
				{
					if (_prioritized[i].Priority <= 0f || num >= 2)
					{
						_prioritized[i].Skipped++;
						continue;
					}
					switch (PackUpdate(packet, _prioritized[i]))
					{
					case -1:
						num++;
						_prioritized[i].Skipped++;
						break;
					case 0:
						_prioritized[i].Skipped++;
						break;
					case 1:
						_prioritized[i].Skipped = 0;
						_prioritized[i].Priority = 0f;
						break;
					}
				}
			}
			finally
			{
				_prioritized.Clear();
			}
		}
		packet.UdpPacket.WriteStopMarker();
		packet.Stats.StateBits = packet.UdpPacket.Position - position;
		BoltCore._timer2.Stop();
		DebugInfo.SendStatePackTime += DebugInfo.GetStopWatchElapsedMilliseconds(BoltCore._timer2);
	}

	public override void Read(Packet packet)
	{
		int position = packet.UdpPacket.Position;
		while (packet.UdpPacket.CanRead() && ReadUpdate(packet))
		{
		}
		packet.Stats.StateBits = packet.UdpPacket.Position - position;
	}

	public override void Lost(Packet packet)
	{
		while (packet.EntityUpdates.Count > 0)
		{
			EntityProxyEnvelope entityProxyEnvelope = packet.EntityUpdates.Dequeue();
			EntityProxyEnvelope entityProxyEnvelope2 = entityProxyEnvelope.Proxy.Envelopes.Dequeue();
			ApplyPropertyPriorities(entityProxyEnvelope);
			entityProxyEnvelope.Proxy.Skipped++;
			if ((bool)(entityProxyEnvelope.Flags & ProxyFlags.FORCE_SYNC))
			{
				entityProxyEnvelope.Proxy.Flags |= ProxyFlags.FORCE_SYNC;
			}
			if ((bool)(entityProxyEnvelope.Flags & ProxyFlags.DESTROY_PENDING))
			{
				Assert.True(entityProxyEnvelope.Proxy.Flags & ProxyFlags.DESTROY_PENDING);
				entityProxyEnvelope.Proxy.Flags &= ~ProxyFlags.DESTROY_PENDING;
			}
			entityProxyEnvelope.Dispose();
		}
	}

	public override void Delivered(Packet packet)
	{
		while (packet.EntityUpdates.Count > 0)
		{
			EntityProxyEnvelope entityProxyEnvelope = packet.EntityUpdates.Dequeue();
			EntityProxyEnvelope entityProxyEnvelope2 = entityProxyEnvelope.Proxy.Envelopes.Dequeue();
			if ((bool)(entityProxyEnvelope.Flags & ProxyFlags.DESTROY_PENDING))
			{
				Assert.True(entityProxyEnvelope.Proxy.Flags & ProxyFlags.DESTROY_PENDING);
				DestroyOutgoingProxy(entityProxyEnvelope.Proxy);
			}
			else if ((bool)(entityProxyEnvelope.Flags & ProxyFlags.CREATE_REQUESTED))
			{
				if (entityProxyEnvelope.ControlTokenGained == entityProxyEnvelope.Proxy.ControlTokenGained)
				{
					entityProxyEnvelope.Proxy.ControlTokenGained = null;
				}
				entityProxyEnvelope.Proxy.Flags &= ~ProxyFlags.CREATE_REQUESTED;
				entityProxyEnvelope.Proxy.Flags |= ProxyFlags.CREATE_DONE;
			}
			entityProxyEnvelope.Dispose();
		}
	}

	public override void Disconnected()
	{
		EntityProxy[] array = _outgoingDict.Values.ToArray();
		foreach (EntityProxy entityProxy in array)
		{
			if ((bool)entityProxy)
			{
				DestroyOutgoingProxy(entityProxy);
			}
		}
		EntityProxy[] array2 = _incommingDict.Values.ToArray();
		foreach (EntityProxy entityProxy2 in array2)
		{
			if ((bool)entityProxy2)
			{
				DestroyIncommingProxy(entityProxy2, null);
			}
		}
	}

	public int GetSkippedUpdates(Entity en)
	{
		if (_outgoingDict.TryGetValue(en.NetworkId, out var value))
		{
			return value.Skipped;
		}
		return -1;
	}

	private void ApplyPropertyPriorities(EntityProxyEnvelope env)
	{
		for (int i = 0; i < env.Written.Count; i++)
		{
			Priority priority = env.Written[i];
			env.Proxy.Set(priority.PropertyIndex);
			env.Proxy.PropertyPriority[priority.PropertyIndex].PropertyPriority += priority.PropertyPriority;
		}
	}

	private int PackUpdate(Packet packet, EntityProxy proxy)
	{
		int position = packet.UdpPacket.Position;
		int num = 0;
		EntityProxyEnvelope entityProxyEnvelope = proxy.CreateEnvelope();
		packet.UdpPacket.WriteBool(value: true);
		packet.UdpPacket.WriteNetworkId(proxy.NetworkId);
		if (packet.UdpPacket.WriteBool(proxy.Entity.IsController(base.connection)))
		{
			packet.UdpPacket.WriteToken(proxy.ControlTokenGained);
			proxy.ControlTokenLost = null;
		}
		else
		{
			packet.UdpPacket.WriteToken(proxy.ControlTokenLost);
			proxy.ControlTokenGained = null;
		}
		if (packet.UdpPacket.WriteBool(proxy.Flags & ProxyFlags.DESTROY_REQUESTED))
		{
			packet.UdpPacket.WriteToken(proxy.Entity.DetachToken);
		}
		else
		{
			if (packet.UdpPacket.WriteBool(proxy.Flags & ProxyFlags.CREATE_REQUESTED))
			{
				bool overflowing = packet.UdpPacket.Overflowing;
				int position2 = packet.UdpPacket.Position;
				packet.UdpPacket.WriteToken(proxy.Entity.AttachToken);
				if (packet.UdpPacket.Overflowing && !overflowing)
				{
					double num2 = (double)BoltCore._config.packetSize * 0.85;
					int num3 = packet.UdpPacket.Position - position2 >> 3;
					if ((double)num3 > num2)
					{
						BoltLog.Warn("Package overflow on Entity {0} Instantiation and will be retried. HINT: AttachToken size is {1} bytes and should be no more than {2} bytes.", proxy.Entity, num3, num2);
					}
				}
				packet.UdpPacket.WritePrefabId(proxy.Entity.PrefabId);
				packet.UdpPacket.WriteTypeId(proxy.Entity.Serializer.TypeId);
				NetworkState_Meta stateMeta = BoltStateMetas.GetStateMeta(proxy.Entity.Serializer.TypeId);
				stateMeta.InstantiationPositionCompression.Pack(packet.UdpPacket, proxy.Entity.UnityObject.transform.position);
				stateMeta.InstantiationRotationCompression.Pack(packet.UdpPacket, proxy.Entity.UnityObject.transform.rotation);
				if (packet.UdpPacket.WriteBool(proxy.Entity.IsSceneObject))
				{
					Assert.False(proxy.Entity.SceneId.IsNone, $"'{proxy.Entity.UnityObject.gameObject}' is marked a scene object but has no scene id ");
					packet.UdpPacket.WriteUniqueId(proxy.Entity.SceneId);
				}
			}
			num = proxy.Entity.Serializer.Pack(base.connection, packet.UdpPacket, entityProxyEnvelope);
		}
		if (packet.UdpPacket.Overflowing)
		{
			packet.UdpPacket.Position = position;
			entityProxyEnvelope.Dispose();
			return -1;
		}
		if (num == -1)
		{
			packet.UdpPacket.Position = position;
			entityProxyEnvelope.Dispose();
			return 0;
		}
		ProxyFlags proxyFlags = proxy.Flags & ProxyFlags.FORCE_SYNC;
		ProxyFlags proxyFlags2 = proxy.Flags & ProxyFlags.CREATE_REQUESTED;
		ProxyFlags proxyFlags3 = proxy.Flags & ProxyFlags.DESTROY_REQUESTED;
		if (num == 0 && !proxyFlags2 && !proxyFlags3 && !proxyFlags)
		{
			packet.UdpPacket.Position = position;
			entityProxyEnvelope.Dispose();
			return 0;
		}
		if ((bool)proxyFlags3)
		{
			entityProxyEnvelope.Flags = (proxy.Flags |= ProxyFlags.DESTROY_PENDING);
		}
		proxy.Flags &= ~ProxyFlags.FORCE_SYNC;
		proxy.Skipped = 0;
		entityProxyEnvelope.PacketNumber = packet.Number;
		packet.EntityUpdates.Enqueue(entityProxyEnvelope);
		proxy.Envelopes.Enqueue(entityProxyEnvelope);
		return 1;
	}

	private bool ReadUpdate(Packet packet)
	{
		if (!packet.UdpPacket.ReadBool())
		{
			return false;
		}
		NetworkId networkId = packet.UdpPacket.ReadNetworkId();
		bool flag = packet.UdpPacket.ReadBool();
		IProtocolToken token = packet.UdpPacket.ReadToken();
		if (packet.UdpPacket.ReadBool())
		{
			IProtocolToken token2 = packet.UdpPacket.ReadToken();
			if (_incommingDict.TryGetValue(networkId, out var value))
			{
				if (value.Entity.HasControl)
				{
					value.Entity.ReleaseControlInternal(token);
				}
				DestroyIncommingProxy(value, token2);
			}
			else
			{
				BoltLog.Warn("Received destroy of {0} but no such proxy was found", networkId);
			}
		}
		else
		{
			IProtocolToken attachToken = null;
			bool flag2 = false;
			bool flag3 = packet.UdpPacket.ReadBool();
			UniqueId uniqueId = UniqueId.None;
			PrefabId prefabId = default(PrefabId);
			TypeId typeId = default(TypeId);
			Vector3 position = default(Vector3);
			Quaternion rotation = default(Quaternion);
			if (flag3)
			{
				attachToken = packet.UdpPacket.ReadToken();
				prefabId = packet.UdpPacket.ReadPrefabId();
				typeId = packet.UdpPacket.ReadTypeId();
				NetworkState_Meta stateMeta = BoltStateMetas.GetStateMeta(typeId);
				position = stateMeta.InstantiationPositionCompression.Read(packet.UdpPacket);
				rotation = stateMeta.InstantiationRotationCompression.Read(packet.UdpPacket);
				flag2 = packet.UdpPacket.ReadBool();
				if (flag2)
				{
					uniqueId = packet.UdpPacket.ReadUniqueId();
				}
			}
			Entity entity = null;
			EntityProxy entityProxy = null;
			if (flag3 && !_incommingDict.ContainsKey(networkId))
			{
				if (flag2)
				{
					GameObject gameObject = BoltCore.FindSceneObject(uniqueId);
					if (!gameObject)
					{
						BoltLog.Warn("Could not find scene object with {0}", uniqueId);
						gameObject = BoltCore.PrefabPool.Instantiate(prefabId, position, rotation);
					}
					entity = Entity.CreateFor(gameObject, prefabId, typeId, EntityFlags.SCENE_OBJECT);
				}
				else
				{
					GameObject gameObject2 = BoltCore.PrefabPool.LoadPrefab(prefabId);
					if ((bool)gameObject2 && BoltCore.isServer && !BoltCore.CanClientInstantiate(gameObject2.GetComponent<BoltEntity>()))
					{
						throw new BoltException("Received entity of prefab {0} from client at {1}, but this entity is not allowed to be instantiated from clients", gameObject2.name, base.connection.RemoteEndPoint);
					}
					entity = Entity.CreateFor(prefabId, typeId, position, rotation);
				}
				entity.Source = base.connection;
				entity.SceneId = uniqueId;
				entity.NetworkId = networkId;
				if (flag)
				{
					entity.Flags |= EntityFlags.HAS_CONTROL;
				}
				entity.Initialize();
				entityProxy = entity.CreateProxy();
				entityProxy.NetworkId = networkId;
				entityProxy.Connection = base.connection;
				_incommingDict.Add(entityProxy.NetworkId, entityProxy);
				entityProxy.Entity.AttachToken = attachToken;
				entityProxy.Entity.Attach();
				entity.Serializer.Read(base.connection, packet.UdpPacket, packet.Frame);
				if (flag)
				{
					entityProxy.Entity.Flags &= ~EntityFlags.HAS_CONTROL;
					entityProxy.Entity.TakeControlInternal(token);
				}
				BoltLog.Debug("Received {0} from {1}", entity, base.connection);
				entityProxy.Entity.LastFrameReceived = BoltNetwork.Frame;
				entityProxy.Entity.Freeze(freeze: false);
				GlobalEventListenerBase.EntityReceivedInvoke(entityProxy.Entity.UnityObject);
			}
			else
			{
				entityProxy = _incommingDict[networkId];
				if (entityProxy == null)
				{
					throw new BoltException("Couldn't find entity for {0}", networkId);
				}
				if (entityProxy.Entity.HasControl ^ flag)
				{
					if (flag)
					{
						entityProxy.Entity.TakeControlInternal(token);
					}
					else
					{
						entityProxy.Entity.ReleaseControlInternal(token);
					}
				}
				entityProxy.Entity.Serializer.Read(base.connection, packet.UdpPacket, packet.Frame);
				entityProxy.Entity.LastFrameReceived = BoltNetwork.Frame;
				entityProxy.Entity.Freeze(freeze: false);
			}
		}
		return true;
	}

	private void DestroyOutgoingProxy(EntityProxy proxy)
	{
		_outgoingDict.Remove(proxy.NetworkId);
		if ((bool)proxy.Entity && proxy.Entity.IsAttached)
		{
			proxy.Entity.Proxies.Remove(proxy);
		}
		if ((bool)(proxy.Flags & ProxyFlags.DESTROY_IGNORE))
		{
			CreateOnRemote(proxy.Entity);
		}
	}

	private void DestroyIncommingProxy(EntityProxy proxy, IProtocolToken token)
	{
		_incommingDict.Remove(proxy.NetworkId);
		proxy.Entity.DetachToken = token;
		BoltCore.DestroyForce(proxy.Entity);
	}
}
