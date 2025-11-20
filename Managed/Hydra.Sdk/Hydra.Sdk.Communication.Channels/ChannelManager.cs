using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hydra.Api.Errors;
using Hydra.Sdk.Errors;
using Hydra.Sdk.Extensions;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.Channels;

public class ChannelManager : IChannelManager
{
	private ConcurrentDictionary<string, string> _endpoints;

	private ConcurrentDictionary<string, IHydraSdkChannel> _channels;

	private IHydraSdkLogger _logger;

	private string _token;

	private Func<ChannelCreationData, IHydraSdkChannel> _channelFunc;

	internal ChannelManager(Func<ChannelCreationData, IHydraSdkChannel> channelCreation, IHydraSdkLogger logger)
	{
		_channelFunc = channelCreation;
		_endpoints = new ConcurrentDictionary<string, string>();
		_channels = new ConcurrentDictionary<string, IHydraSdkChannel>();
		_logger = logger;
	}

	public void UpdateToken(string token)
	{
		_token = token;
		foreach (IHydraSdkChannel value in _channels.Values)
		{
			value.UpdateToken(_token);
		}
	}

	public void AddEndpoint(string serviceName, Uri uri)
	{
		if (_endpoints.TryAdd(serviceName, uri.Authority) && !_channels.ContainsKey(uri.Authority))
		{
			try
			{
				IHydraSdkChannel value = CreateChannel(uri);
				_channels.TryAdd(uri.Authority, value);
			}
			catch (Exception innerException)
			{
				throw new HydraSdkException(ErrorCode.SdkInternalError, "Failed to add endpoint for " + serviceName + ".", innerException);
			}
		}
	}

	public bool TryGetEndpoint(string serviceName, out string address)
	{
		return _endpoints.TryGetValue(serviceName, out address);
	}

	public bool TryGetChannel(string serviceName, out IHydraSdkChannel channel)
	{
		if (TryGetEndpoint(serviceName, out var address))
		{
			return _channels.TryGetValue(address, out channel);
		}
		channel = null;
		return false;
	}

	private IHydraSdkChannel CreateChannel(Uri uri)
	{
		return _channelFunc(new ChannelCreationData
		{
			Token = _token,
			Uri = uri,
			Logger = _logger
		});
	}

	public async Task ShutdownChannels()
	{
		try
		{
			await Task.WhenAll(_channels.Select((KeyValuePair<string, IHydraSdkChannel> c) => c.Value.Shutdown()));
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			_logger?.LogException(ErrorCode.SdkInternalError, this.GetLogCatErr(), "Channels shutdown failure.", ex2);
		}
		finally
		{
			_channels.Clear();
			_endpoints.Clear();
			_token = null;
		}
	}
}
