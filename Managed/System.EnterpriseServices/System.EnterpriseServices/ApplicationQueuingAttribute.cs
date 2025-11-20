using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Enables queuing support for the marked assembly and enables the application to read method calls from Message Queuing queues. This class cannot be inherited.</summary>
[ComVisible(false)]
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ApplicationQueuingAttribute : Attribute
{
	private bool enabled;

	private int maxListenerThreads;

	private bool queueListenerEnabled;

	/// <summary>Gets or sets a value indicating whether queuing support is enabled.</summary>
	/// <returns>true if queuing support is enabled; otherwise, false. The default value set by the constructor is true.</returns>
	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
		}
	}

	/// <summary>Gets or sets the number of threads used to extract messages from the queue and activate the corresponding component.</summary>
	/// <returns>The maximum number of threads to use for processing messages arriving in the queue. The default is zero.</returns>
	public int MaxListenerThreads
	{
		get
		{
			return maxListenerThreads;
		}
		set
		{
			maxListenerThreads = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the application will accept queued component calls from clients.</summary>
	/// <returns>true if the application accepts queued component calls; otherwise, false. The default is false.</returns>
	public bool QueueListenerEnabled
	{
		get
		{
			return queueListenerEnabled;
		}
		set
		{
			queueListenerEnabled = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.ApplicationQueuingAttribute" /> class, enabling queuing support for the assembly and initializing <see cref="P:System.EnterpriseServices.ApplicationQueuingAttribute.Enabled" />, <see cref="P:System.EnterpriseServices.ApplicationQueuingAttribute.QueueListenerEnabled" />, and <see cref="P:System.EnterpriseServices.ApplicationQueuingAttribute.MaxListenerThreads" />.</summary>
	public ApplicationQueuingAttribute()
	{
		enabled = true;
		queueListenerEnabled = false;
		maxListenerThreads = 0;
	}
}
