using System;
using System.Collections.Generic;
using Epic.OnlineServices.IntegratedPlatform;

namespace PlayEveryWare.EpicOnlineServices;

[Serializable]
public class EOSLinuxConfig : ICloneableGeneric<EOSLinuxConfig>, IEmpty
{
	public List<string> flags;

	public EOSConfig overrideValues;

	public EOSLinuxConfig Clone()
	{
		return (EOSLinuxConfig)MemberwiseClone();
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
