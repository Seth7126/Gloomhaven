using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Sets the queuing exception class for the queued class. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class)]
[ComVisible(false)]
public sealed class ExceptionClassAttribute : Attribute
{
	private string name;

	/// <summary>Gets the name of the exception class for the player to activate and play back before the message is routed to the dead letter queue.</summary>
	/// <returns>The name of the exception class for the player to activate and play back before the message is routed to the dead letter queue.</returns>
	public string Value => name;

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ExceptionClassAttribute" /> class.</summary>
	/// <param name="name">The name of the exception class for the player to activate and play back before the message is routed to the dead letter queue. </param>
	public ExceptionClassAttribute(string name)
	{
		this.name = name;
	}
}
