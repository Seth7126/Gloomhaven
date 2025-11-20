using System;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class ClientInfo : HydraSdkSettings, IHydraSdkStateWrapper
{
	private static string _defaultTitle = "0000000000";

	private static string _edsLocal = "https://127.0.0.1:40000";

	private static string _edsStaging = "https://eds-stg.hydrapi.net:11701";

	private static string _edsProduction = "https://hdrprod.hydrapi.net:11701";

	private static string _productionTitlePrefix = "p";

	public ClientInfo(HydraSdkSettings settings)
	{
		if (string.IsNullOrWhiteSpace(settings.TitleId))
		{
			base.TitleId = _defaultTitle;
		}
		else
		{
			base.TitleId = settings.TitleId;
		}
		if (string.IsNullOrWhiteSpace(settings.HydraEndpoint))
		{
			if (base.TitleId == _defaultTitle)
			{
				base.HydraEndpoint = _edsLocal;
			}
			else if (base.TitleId.StartsWith(_productionTitlePrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				base.HydraEndpoint = _edsProduction;
			}
			else
			{
				base.HydraEndpoint = _edsStaging;
			}
		}
		else
		{
			base.HydraEndpoint = settings.HydraEndpoint;
		}
		base.ClientVersion = settings.ClientVersion;
		base.SecretKey = settings.SecretKey;
		base.Platform = settings.Platform;
		base.ManualComponentsHandling = settings.ManualComponentsHandling;
	}
}
