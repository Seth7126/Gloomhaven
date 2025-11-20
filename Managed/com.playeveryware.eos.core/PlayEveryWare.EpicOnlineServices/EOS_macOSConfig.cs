using System.Collections.Generic;
using Epic.OnlineServices.IntegratedPlatform;

namespace PlayEveryWare.EpicOnlineServices;

public class EOS_macOSConfig : ICloneableGeneric<EOS_macOSConfig>, IEmpty
{
	public List<string> flags;

	public EOSConfig overrideValues;

	public EOS_macOSConfig Clone()
	{
		return (EOS_macOSConfig)MemberwiseClone();
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
