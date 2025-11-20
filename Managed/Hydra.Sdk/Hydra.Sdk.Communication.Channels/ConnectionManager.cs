using System;
using System.Collections.Concurrent;
using Hydra.Api.EndpointDispatcher;
using Hydra.Api.Errors;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Generated;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.Channels;

public class ConnectionManager : IConnectionManager
{
	private ConcurrentDictionary<Type, ClientBase> _clients;

	private IChannelManager _channelManager;

	public ConnectionManager(IChannelManager channelManager)
	{
		if (channelManager == null)
		{
			throw new ArgumentNullException("channelManager");
		}
		_channelManager = channelManager;
		_clients = new ConcurrentDictionary<Type, ClientBase>();
	}

	public T GetConnection<T>() where T : ClientBase<T>
	{
		Type typeFromHandle = typeof(T);
		string text = typeFromHandle.DeclaringType?.FullName ?? throw new HydraSdkException(ErrorCode.SdkInvalidParameter, "Invalid API client: failed to get declaring type from " + typeFromHandle.Name);
		if (_channelManager.TryGetChannel(text, out var channel))
		{
			if (!_clients.TryGetValue(typeof(T), out var value))
			{
				_clients.TryAdd(typeFromHandle, (T)Activator.CreateInstance(typeFromHandle, channel.GetInvoker()));
				return (T)_clients[typeFromHandle];
			}
			return (T)value;
		}
		throw new HydraSdkException(ErrorCode.SdkNoServiceEndpoint, "Failed to get endpoint for " + text + ".");
	}

	public IHydraSdkChannel GetChannel(string serviceName)
	{
		if (_channelManager.TryGetChannel(serviceName, out var channel))
		{
			return channel;
		}
		throw new HydraSdkException(ErrorCode.SdkNoServiceEndpoint, "Failed to get channel for " + serviceName);
	}

	public void AddConnections(params EndpointInfo[] endpointInfos)
	{
		foreach (EndpointInfo endpointInfo in endpointInfos)
		{
			_channelManager.AddEndpoint(endpointInfo.Name, endpointInfo.ToUri());
		}
	}

	public void UpdateToken(string token)
	{
		_channelManager.UpdateToken(token);
	}
}
