using System;
using System.Threading.Tasks;
using Hydra.Sdk.Generated;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.Channels.Unity;

internal class HydraUnityChannel : IHydraSdkChannel
{
	private IHydraSdkLogger _logger;

	private HydraUnityCaller _caller;

	private ChannelInfo _info;

	public HydraUnityChannel(ChannelCreationData data)
	{
		_logger = data.Logger;
		_caller = new HydraUnityCaller(data.Uri, _logger, data.Token);
		_info = new ChannelInfo(data.Uri.Authority, data.Uri.Scheme == Uri.UriSchemeHttps);
	}

	public ChannelInfo GetInfo()
	{
		return _info;
	}

	public ICaller GetInvoker()
	{
		return _caller;
	}

	public Task Shutdown()
	{
		_caller.Dispose();
		return Task.CompletedTask;
	}

	public void UpdateToken(string token)
	{
		_caller.SetToken(token);
	}
}
