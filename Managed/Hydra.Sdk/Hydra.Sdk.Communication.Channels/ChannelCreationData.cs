using System;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.Channels;

public class ChannelCreationData
{
	public Uri Uri { get; set; }

	public string Token { get; set; }

	public IHydraSdkLogger Logger { get; set; }
}
