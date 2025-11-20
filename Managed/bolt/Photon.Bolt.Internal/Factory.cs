#define DEBUG
using System;
using System.Collections.Generic;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Utils;

namespace Photon.Bolt.Internal;

internal static class Factory
{
	private static Dictionary<byte, Type> _id2token = new Dictionary<byte, Type>();

	private static Dictionary<Type, byte> _token2id = new Dictionary<Type, byte>();

	private static Dictionary<Type, ObjectPool<PooledProtocolTokenBase>> _protocolTokenPool = new Dictionary<Type, ObjectPool<PooledProtocolTokenBase>>();

	private static Dictionary<Type, IFactory> _factoriesByType = new Dictionary<Type, IFactory>();

	private static Dictionary<TypeId, IFactory> _factoriesById = new Dictionary<TypeId, IFactory>(128, TypeId.EqualityComparer.Instance);

	private static Dictionary<UniqueId, IFactory> _factoriesByKey = new Dictionary<UniqueId, IFactory>(128, UniqueId.EqualityComparer.Instance);

	internal static bool IsEmpty => _factoriesById.Count == 0;

	internal static void Register(IFactory factory)
	{
		_factoriesById.Add(factory.TypeId, factory);
		_factoriesByKey.Add(factory.TypeKey, factory);
		_factoriesByType.Add(factory.TypeObject, factory);
	}

	internal static IFactory GetFactory(TypeId id)
	{
		if (_factoriesById.TryGetValue(id, out var value))
		{
			return value;
		}
		BoltLog.Error("Unknown factory {0}", id);
		return null;
	}

	internal static IFactory GetFactory(UniqueId id)
	{
		if (_factoriesByKey.TryGetValue(id, out var value))
		{
			return value;
		}
		BoltLog.Error("Unknown factory {0}", id);
		return null;
	}

	internal static IEventFactory GetEventFactory(TypeId id)
	{
		if (_factoriesById.TryGetValue(id, out var value))
		{
			return (IEventFactory)value;
		}
		BoltLog.Error("Unknown event factory {0}", id);
		return null;
	}

	internal static IEventFactory GetEventFactory(UniqueId id)
	{
		if (_factoriesByKey.TryGetValue(id, out var value))
		{
			return (IEventFactory)value;
		}
		BoltLog.Error("Unknown event factory {0}", id);
		return null;
	}

	internal static Event NewEvent(TypeId id)
	{
		Event obj = Create(id) as Event;
		obj?.InitNetworkStorage();
		return obj;
	}

	internal static Event NewEvent(UniqueId id)
	{
		Event obj = Create(id) as Event;
		obj?.InitNetworkStorage();
		return obj;
	}

	internal static byte GetTokenId<T>() where T : IProtocolToken
	{
		return GetTokenId(typeof(T));
	}

	internal static byte GetTokenId(IProtocolToken obj)
	{
		return GetTokenId(obj.GetType());
	}

	private static byte GetTokenId(Type tokenType)
	{
		if (_token2id.TryGetValue(tokenType, out var value))
		{
			return value;
		}
		BoltLog.Error("Unknown token type {0}", tokenType);
		throw new BoltException("Unknown token type {0}", tokenType);
	}

	internal static IProtocolToken NewToken(byte id)
	{
		if (_id2token.TryGetValue(id, out var value))
		{
			IProtocolToken protocolToken;
			if (_protocolTokenPool.TryGetValue(value, out var value2))
			{
				if (value2.Available)
				{
					protocolToken = value2.Get();
					((PooledProtocolTokenBase)protocolToken).IsPooled = false;
				}
				else
				{
					protocolToken = (IProtocolToken)Activator.CreateInstance(value);
					((PooledProtocolTokenBase)protocolToken).Pool = value2;
				}
			}
			else
			{
				protocolToken = (IProtocolToken)Activator.CreateInstance(value);
			}
			return protocolToken;
		}
		BoltLog.Error("Unknown token id {0}", id);
		throw new BoltException("Unknown token id {0}", id);
	}

	internal static Command NewCommand(TypeId id)
	{
		Command command = Create(id) as Command;
		command?.InitNetworkStorage();
		return command;
	}

	internal static Command NewCommand(UniqueId id)
	{
		Command command = Create(id) as Command;
		command?.InitNetworkStorage();
		return command;
	}

	internal static IEntitySerializer NewSerializer(TypeId id)
	{
		return Create(id) as IEntitySerializer;
	}

	internal static IEntitySerializer NewSerializer(UniqueId guid)
	{
		return Create(guid) as IEntitySerializer;
	}

	private static object Create(TypeId id)
	{
		if (_factoriesById.TryGetValue(id, out var value))
		{
			return value.Create();
		}
		BoltLog.Error("Unknown {0}", id);
		return null;
	}

	private static object Create(UniqueId id)
	{
		if (_factoriesByKey.TryGetValue(id, out var value))
		{
			return value.Create();
		}
		BoltLog.Error("Unknown {0}", id);
		return null;
	}

	internal static void UnregisterAll()
	{
		_token2id.Clear();
		_id2token.Clear();
		_protocolTokenPool.Clear();
		_factoriesById.Clear();
		_factoriesByKey.Clear();
		_factoriesByType.Clear();
		_token2id = new Dictionary<Type, byte>();
		_id2token = new Dictionary<byte, Type>();
		_protocolTokenPool = new Dictionary<Type, ObjectPool<PooledProtocolTokenBase>>();
		_factoriesById = new Dictionary<TypeId, IFactory>(128, TypeId.EqualityComparer.Instance);
		_factoriesByKey = new Dictionary<UniqueId, IFactory>(128, UniqueId.EqualityComparer.Instance);
		_factoriesByType = new Dictionary<Type, IFactory>();
	}

	internal static void RegisterTokenClass(Type type)
	{
		if (_token2id.Count == 254)
		{
			throw new ArgumentException("Can only register 254 different token types");
		}
		byte b = (byte)(_token2id.Count + 1);
		if (!_id2token.ContainsKey(b) && !_token2id.ContainsKey(type))
		{
			_token2id.Add(type, b);
			_id2token.Add(b, type);
			if (typeof(PooledProtocolTokenBase).IsAssignableFrom(type))
			{
				_protocolTokenPool.Add(type, new ObjectPool<PooledProtocolTokenBase>());
			}
			BoltLog.Debug("Registered token class {0} as id {1}", type, b);
		}
	}
}
