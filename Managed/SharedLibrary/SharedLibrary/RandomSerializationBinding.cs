using System;
using System.Runtime.Serialization;

namespace SharedLibrary;

public class RandomSerializationBinding : SerializationBinder
{
	public override Type BindToType(string assemblyName, string typeName)
	{
		if (typeName.Contains("System.Random"))
		{
			return typeof(Random);
		}
		return Type.GetType($"{typeName}, {assemblyName}");
	}
}
