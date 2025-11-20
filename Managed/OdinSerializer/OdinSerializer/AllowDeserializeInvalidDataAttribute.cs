using System;

namespace OdinSerializer;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class AllowDeserializeInvalidDataAttribute : Attribute
{
}
