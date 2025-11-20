using System;
using System.Collections.Generic;

namespace PlayEveryWare.EpicOnlineServices;

[Serializable]
public class EOSSteamConfig : ICloneableGeneric<EOSSteamConfig>, IEmpty
{
	public List<string> flags;

	public string overrideLibraryPath;

	public uint steamSDKMajorVersion;

	public uint steamSDKMinorVersion;

	public EOSSteamConfig Clone()
	{
		return (EOSSteamConfig)MemberwiseClone();
	}

	public bool IsEmpty()
	{
		if (EmptyPredicates.IsEmptyOrNullOrContainsOnlyEmpty(flags) && EmptyPredicates.IsEmptyOrNull(overrideLibraryPath) && steamSDKMajorVersion == 0)
		{
			return steamSDKMinorVersion == 0;
		}
		return false;
	}
}
