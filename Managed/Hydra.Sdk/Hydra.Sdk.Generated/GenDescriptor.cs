using System;
using Google.Protobuf.Reflection;

namespace Hydra.Sdk.Generated;

public class GenDescriptor : IDescriptor
{
	public string Name { get; }

	public string FullName { get; }

	public FileDescriptor File
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public GenDescriptor(string name, string fullName)
	{
		Name = name;
		FullName = fullName;
	}
}
