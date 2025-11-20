using System;
using UnityEngine.Scripting;

namespace SRF.Service;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ServiceSelectorAttribute : PreserveAttribute
{
	public Type ServiceType { get; private set; }

	public ServiceSelectorAttribute(Type serviceType)
	{
		ServiceType = serviceType;
	}
}
