using System.Collections.Generic;
using Epic.OnlineServices.IntegratedPlatform;

namespace PlayEveryWare.EpicOnlineServices;

public class EOSAndroidConfig : ICloneableGeneric<EOSAndroidConfig>, IEmpty
{
	public List<string> flags;

	public EOSConfig overrideValues;

	public EOSAndroidConfig Clone()
	{
		return (EOSAndroidConfig)MemberwiseClone();
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
