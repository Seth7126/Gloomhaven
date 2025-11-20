using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Marks the attributed class as an event class. This class cannot be inherited.</summary>
[ComVisible(false)]
[AttributeUsage(AttributeTargets.Class)]
public sealed class EventClassAttribute : Attribute
{
	private bool allowInProcSubscribers;

	private bool fireInParallel;

	private string publisherFilter;

	/// <summary>Gets or sets a value that indicates whether subscribers can be activated in the publisher's process.</summary>
	/// <returns>true if subscribers can be activated in the publisher's process; otherwise, false.</returns>
	public bool AllowInprocSubscribers
	{
		get
		{
			return allowInProcSubscribers;
		}
		set
		{
			allowInProcSubscribers = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether events are to be delivered to subscribers in parallel.</summary>
	/// <returns>true if events are to be delivered to subscribers in parallel; otherwise, false.</returns>
	public bool FireInParallel
	{
		get
		{
			return fireInParallel;
		}
		set
		{
			fireInParallel = value;
		}
	}

	/// <summary>Gets or sets a publisher filter for an event method.</summary>
	/// <returns>The publisher filter.</returns>
	public string PublisherFilter
	{
		get
		{
			return publisherFilter;
		}
		set
		{
			publisherFilter = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.EnterpriseServices.EventClassAttribute" /> class.</summary>
	public EventClassAttribute()
	{
		allowInProcSubscribers = true;
		fireInParallel = false;
		publisherFilter = null;
	}
}
