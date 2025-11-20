using System.Collections.Generic;
using Epic.OnlineServices.IntegratedPlatform;

namespace PlayEveryWare.EpicOnlineServices;

public class EOS_iOSConfig : ICloneableGeneric<EOS_iOSConfig>, IEmpty
{
	public List<string> flags;

	public EOSConfig overrideValues;

	public EOS_iOSConfig Clone()
	{
		return (EOS_iOSConfig)MemberwiseClone();
	}

	public bool IsEmpty()
	{
		if (EmptyPredicates.IsEmptyOrNullOrContainsOnlyEmpty(flags))
		{
			return EmptyPredicates.IsEmptyOrNull(overrideValues);
		}
		return false;
	}

	public IntegratedPlatformManagementFlags flagsAsIntegratedPlatformManagementFlags()
	{
		return EOSConfig.flagsAsIntegratedPlatformManagementFlags(flags);
	}
}
