using System;

namespace Photon.Bolt;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BoltExecutionOrderAttribute : Attribute
{
	private readonly int _executionOrder;

	public int executionOrder => _executionOrder;

	public BoltExecutionOrderAttribute(int order)
	{
		_executionOrder = order;
	}
}
