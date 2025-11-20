using System;
using UnityEngine.Scripting;

namespace SRF.Service;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ServiceConstructorAttribute : PreserveAttribute
{
	public Type ServiceType { get; private set; }

	public ServiceConstructorAttribute(Type serviceType)
	{
		ServiceType = serviceType;
	}
}
