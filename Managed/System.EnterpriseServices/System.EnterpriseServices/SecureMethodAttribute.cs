using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Ensures that the infrastructure calls through an interface for a method or for each method in a class when using the security service. Classes need to use interfaces to use security services. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[ComVisible(false)]
public sealed class SecureMethodAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.SecureMethodAttribute" /> class.</summary>
	public SecureMethodAttribute()
	{
	}
}
