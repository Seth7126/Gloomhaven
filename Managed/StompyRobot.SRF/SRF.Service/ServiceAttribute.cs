using System;
using UnityEngine.Scripting;

namespace SRF.Service;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ServiceAttribute : PreserveAttribute
{
	public Type ServiceType { get; private set; }

	public ServiceAttribute(Type serviceType)
	{
		ServiceType = serviceType;
	}
}
