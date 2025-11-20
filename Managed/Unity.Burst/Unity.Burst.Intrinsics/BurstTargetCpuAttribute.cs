using System;

namespace Unity.Burst.Intrinsics;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class BurstTargetCpuAttribute : Attribute
{
	public readonly BurstTargetCpu TargetCpu;

	public BurstTargetCpuAttribute(BurstTargetCpu TargetCpu)
	{
		this.TargetCpu = TargetCpu;
	}
}
