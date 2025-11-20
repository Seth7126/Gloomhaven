namespace System.Reflection;

/// <summary>Retrieves the mapping of an interface into the actual methods on a class that implements that interface.</summary>
public struct InterfaceMapping
{
	/// <summary>Represents the type that was used to create the interface mapping.</summary>
	public Type TargetType;

	/// <summary>Shows the type that represents the interface.</summary>
	public Type InterfaceType;

	/// <summary>Shows the methods that implement the interface.</summary>
	public MethodInfo[] TargetMethods;

	/// <summary>Shows the methods that are defined on the interface.</summary>
	public MethodInfo[] InterfaceMethods;
}
