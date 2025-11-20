using System.Collections.ObjectModel;
using Unity;

namespace System.Diagnostics.Tracing;

/// <summary>Provides data for the <see cref="M:System.Diagnostics.Tracing.EventListener.OnEventWritten(System.Diagnostics.Tracing.EventWrittenEventArgs)" /> callback.</summary>
public class EventWrittenEventArgs : EventArgs
{
	/// <summary>Gets the activity ID on the thread that the event was written to. </summary>
	/// <returns>The activity ID on the thread that the event was written to. </returns>
	public Guid ActivityId => EventSource.CurrentThreadActivityId;

	public EventChannel Channel => EventChannel.None;

	/// <summary>Gets the event identifier.</summary>
	/// <returns>The event identifier.</returns>
	public int EventId { get; internal set; }

	public long OSThreadId { get; internal set; }

	public DateTime TimeStamp { get; internal set; }

	public string EventName { get; internal set; }

	/// <summary>Gets the event source object.</summary>
	/// <returns>The event source object.</returns>
	public EventSource EventSource { get; private set; }

	/// <summary>Gets the keywords for the event.</summary>
	/// <returns>The keywords for the event.</returns>
	public EventKeywords Keywords => EventKeywords.None;

	/// <summary>Gets the level of the event.</summary>
	/// <returns>The level of the event.</returns>
	public EventLevel Level => EventLevel.LogAlways;

	/// <summary>Gets the message for the event.</summary>
	/// <returns>The message for the event.</returns>
	public string Message { get; internal set; }

	/// <summary>Gets the operation code for the event.</summary>
	/// <returns>The operation code for the event.</returns>
	public EventOpcode Opcode => EventOpcode.Info;

	/// <summary>Gets the payload for the event.</summary>
	/// <returns>The payload for the event.</returns>
	public ReadOnlyCollection<object> Payload { get; internal set; }

	public ReadOnlyCollection<string> PayloadNames { get; internal set; }

	/// <summary>Gets the identifier of an activity that is related to the activity represented by the current instance. </summary>
	/// <returns>The identifier of the related activity, or <see cref="F:System.Guid.Empty" /> if there is no related activity.</returns>
	public Guid RelatedActivityId { get; internal set; }

	public EventTags Tags => EventTags.None;

	/// <summary>Gets the task for the event.</summary>
	/// <returns>The task for the event.</returns>
	public EventTask Task => EventTask.None;

	/// <summary>Gets the version of the event.</summary>
	/// <returns>The version of the event.</returns>
	public byte Version => 0;

	internal EventWrittenEventArgs(EventSource eventSource)
	{
		EventSource = eventSource;
	}

	internal EventWrittenEventArgs()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
