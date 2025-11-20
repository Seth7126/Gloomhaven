using System;
using JetBrains.Annotations;

namespace OdinSerializer;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class OdinSerializeAttribute : Attribute
{
}
