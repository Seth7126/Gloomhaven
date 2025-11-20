using System;

namespace XUnity.ResourceRedirector;

public class ResourceLoadedParameters
{
	public string Path { get; set; }

	public Type Type { get; set; }

	public ResourceLoadType LoadType { get; }

	internal ResourceLoadedParameters(string path, Type type, ResourceLoadType loadType)
	{
		Path = path;
		Type = type;
		LoadType = loadType;
	}
}
