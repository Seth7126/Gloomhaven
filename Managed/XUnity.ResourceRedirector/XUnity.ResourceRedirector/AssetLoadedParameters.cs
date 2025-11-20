using System;

namespace XUnity.ResourceRedirector;

public class AssetLoadedParameters
{
	public string Name { get; }

	public Type Type { get; }

	public AssetLoadType LoadType { get; }

	internal AssetLoadedParameters(string name, Type type, AssetLoadType loadType)
	{
		Name = name;
		Type = type;
		LoadType = loadType;
	}
}
