using System;

namespace XUnity.ResourceRedirector;

public class AssetLoadingParameters
{
	public string Name { get; set; }

	public Type Type { get; set; }

	public AssetLoadType LoadType { get; }

	internal AssetLoadingParameters(string name, Type type, AssetLoadType loadType)
	{
		Name = name;
		Type = type;
		LoadType = loadType;
	}

	internal AssetLoadedParameters ToAssetLoadedParameters()
	{
		return new AssetLoadedParameters(Name, Type, LoadType);
	}
}
