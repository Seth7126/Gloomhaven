using System.Collections.Generic;

namespace System.Diagnostics.Tracing;

/// <summary>Provides the ability to create events for event tracing for Windows (ETW).</summary>
public class EventSource : IDisposable
{
	/// <summary>Provides the event data for creating fast <see cref="Overload:System.Diagnostics.Tracing.EventSource.WriteEvent" /> overloads by using the <see cref="M:System.Diagnostics.Tracing.EventSource.WriteEventCore(System.Int32,System.Int32,System.Diagnostics.Tracing.EventSource.EventData*)" /> method.</summary>
	protected internal struct EventData
	{
		/// <summary>Gets or sets the pointer to the data for the new <see cref="Overload:System.Diagnostics.Tracing.EventSource.WriteEvent" /> overload.</summary>
		/// <returns>The pointer to the data.</returns>
		public IntPtr DataPointer { get; set; }

		/// <summary>Gets or sets the number of payload items in the new <see cref="Overload:System.Diagnostics.Tracing.EventSource.WriteEvent" /> overload.</summary>
		/// <returns>The number of payload items in the new overload.</returns>
		public int Size { get; set; }

		internal int Reserved { get; set; }
	}

	/// <summary>Gets any exception that was thrown during the construction of the event source. </summary>
	/// <returns>The exception that was thrown during the construction of the event source, or null if no exception was thrown. </returns>
	public Exception ConstructionException => null;

	/// <summary>Gets the activity ID of the current thread. </summary>
	/// <returns>The activity ID of the current thread. </returns>
	public static Guid CurrentThreadActivityId => Guid.Empty;

	/// <summary>The unique identifier for the event source.</summary>
	/// <returns>A unique identifier for the event source.</returns>
	public Guid Guid => Guid.Empty;

	/// <summary>The friendly name of the class that is derived from the event source.</summary>
	/// <returns>The friendly name of the derived class.  The default is the simple name of the class.</returns>
	public string Name { get; private set; }

	public EventSourceSettings Settings { get; private set; }

	public event EventHandler<EventCommandEventArgs> EventCommandExecuted
	{
		add
		{
			throw new NotImplementedException();
		}
		remove
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Diagnostics.Tracing.EventSource" /> class.</summary>
	protected EventSource()
	{
		Name = GetType().Name;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Diagnostics.Tracing.EventSource" /> class and specifies whether to throw an exception when an error occurs in the underlying Windows code.</summary>
	/// <param name="throwOnEventWriteErrors">true to throw an exception when an error occurs in the underlying Windows code; otherwise, false.</param>
	protected EventSource(bool throwOnEventWriteErrors)
		: this()
	{
	}

	protected EventSource(EventSourceSettings settings)
		: this()
	{
		Settings = settings;
	}

	protected EventSource(EventSourceSettings settings, params string[] traits)
		: this(settings)
	{
	}

	public EventSource(string eventSourceName)
	{
		Name = eventSourceName;
	}

	public EventSource(string eventSourceName, EventSourceSettings config)
		: this(eventSourceName)
	{
		Settings = config;
	}

	public EventSource(string eventSourceName, EventSourceSettings config, params string[] traits)
		: this(eventSourceName, config)
	{
	}

	internal EventSource(Guid eventSourceGuid, string eventSourceName)
		: this(eventSourceName)
	{
	}

	/// <summary>Allows the <see cref="T:System.Diagnostics.Tracing.EventSource" /> object to attempt to free resources and perform other cleanup operations before the  object is reclaimed by garbage collection.</summary>
	~EventSource()
	{
		Dispose(disposing: false);
	}

	/// <summary>Determines whether the current event source is enabled.</summary>
	/// <returns>true if the current event source is enabled; otherwise, false.</returns>
	public bool IsEnabled()
	{
		return false;
	}

	/// <summary>Determines whether the current event source that has the specified level and keyword is enabled.</summary>
	/// <returns>true if the event source is enabled; otherwise, false.</returns>
	/// <param name="level">The level of the event source.</param>
	/// <param name="keywords">The keyword of the event source.</param>
	public bool IsEnabled(EventLevel level, EventKeywords keywords)
	{
		return false;
	}

	public bool IsEnabled(EventLevel level, EventKeywords keywords, EventChannel channel)
	{
		return false;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Diagnostics.Tracing.EventSource" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public string GetTrait(string key)
	{
		return null;
	}

	public void Write(string eventName)
	{
	}

	public void Write(string eventName, EventSourceOptions options)
	{
	}

	public void Write<T>(string eventName, T data)
	{
	}

	public void Write<T>(string eventName, EventSourceOptions options, T data)
	{
	}

	[CLSCompliant(false)]
	public void Write<T>(string eventName, ref EventSourceOptions options, ref T data)
	{
	}

	public void Write<T>(string eventName, ref EventSourceOptions options, ref Guid activityId, ref Guid relatedActivityId, ref T data)
	{
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Diagnostics.Tracing.EventSource" /> class and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
	}

	/// <summary>Called when the current event source is updated by the controller.</summary>
	/// <param name="command">The arguments for the event.</param>
	protected virtual void OnEventCommand(EventCommandEventArgs command)
	{
	}

	internal void ReportOutOfBandMessage(string msg, bool flush)
	{
	}

	/// <summary>Writes an event by using the provided event identifier.</summary>
	/// <param name="eventId">The event identifier.</param>
	protected void WriteEvent(int eventId)
	{
		WriteEvent(eventId, new object[0]);
	}

	protected void WriteEvent(int eventId, byte[] arg1)
	{
		WriteEvent(eventId, new object[1] { arg1 });
	}

	/// <summary>Writes an event by using the provided event identifier and 32-bit integer argument.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">An integer argument.</param>
	protected void WriteEvent(int eventId, int arg1)
	{
		WriteEvent(eventId, new object[1] { arg1 });
	}

	/// <summary>Writes an event by using the provided event identifier and string argument.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A string argument.</param>
	protected void WriteEvent(int eventId, string arg1)
	{
		WriteEvent(eventId, new object[1] { arg1 });
	}

	/// <summary>Writes an event by using the provided event identifier and 32-bit integer arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">An integer argument.</param>
	/// <param name="arg2">An integer argument.</param>
	protected void WriteEvent(int eventId, int arg1, int arg2)
	{
		WriteEvent(eventId, new object[2] { arg1, arg2 });
	}

	/// <summary>Writes an event by using the provided event identifier and 32-bit integer arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">An integer argument.</param>
	/// <param name="arg2">An integer argument.</param>
	/// <param name="arg3">An integer argument.</param>
	protected void WriteEvent(int eventId, int arg1, int arg2, int arg3)
	{
		WriteEvent(eventId, new object[3] { arg1, arg2, arg3 });
	}

	protected void WriteEvent(int eventId, int arg1, string arg2)
	{
		WriteEvent(eventId, new object[2] { arg1, arg2 });
	}

	/// <summary>Writes an event by using the provided event identifier and 64-bit integer argument.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A 64 bit integer argument.</param>
	protected void WriteEvent(int eventId, long arg1)
	{
		WriteEvent(eventId, new object[1] { arg1 });
	}

	protected void WriteEvent(int eventId, long arg1, byte[] arg2)
	{
		WriteEvent(eventId, new object[2] { arg1, arg2 });
	}

	/// <summary>Writes an event by using the provided event identifier and 64-bit arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A 64 bit integer argument.</param>
	/// <param name="arg2">A 64 bit integer argument.</param>
	protected void WriteEvent(int eventId, long arg1, long arg2)
	{
		WriteEvent(eventId, new object[2] { arg1, arg2 });
	}

	/// <summary>Writes an event by using the provided event identifier and 64-bit arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A 64 bit integer argument.</param>
	/// <param name="arg2">A 64 bit integer argument.</param>
	/// <param name="arg3">A 64 bit integer argument.</param>
	protected void WriteEvent(int eventId, long arg1, long arg2, long arg3)
	{
		WriteEvent(eventId, new object[3] { arg1, arg2, arg3 });
	}

	protected void WriteEvent(int eventId, long arg1, string arg2)
	{
		WriteEvent(eventId, new object[2] { arg1, arg2 });
	}

	/// <summary>Writes an event by using the provided event identifier and array of arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="args">An array of objects.</param>
	protected void WriteEvent(int eventId, params object[] args)
	{
	}

	/// <summary>Writes an event by using the provided event identifier and arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A string argument.</param>
	/// <param name="arg2">A 32 bit integer argument.</param>
	protected void WriteEvent(int eventId, string arg1, int arg2)
	{
		WriteEvent(eventId, new object[2] { arg1, arg2 });
	}

	/// <summary>Writes an event by using the provided event identifier and arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A string argument.</param>
	/// <param name="arg2">A 32 bit integer argument.</param>
	/// <param name="arg3">A 32 bit integer argument.</param>
	protected void WriteEvent(int eventId, string arg1, int arg2, int arg3)
	{
		WriteEvent(eventId, new object[3] { arg1, arg2, arg3 });
	}

	/// <summary>Writes an event by using the provided event identifier and arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A string argument.</param>
	/// <param name="arg2">A 64 bit integer argument.</param>
	protected void WriteEvent(int eventId, string arg1, long arg2)
	{
		WriteEvent(eventId, new object[2] { arg1, arg2 });
	}

	/// <summary>Writes an event by using the provided event identifier and string arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A string argument.</param>
	/// <param name="arg2">A string argument.</param>
	protected void WriteEvent(int eventId, string arg1, string arg2)
	{
		WriteEvent(eventId, new object[2] { arg1, arg2 });
	}

	/// <summary>Writes an event by using the provided event identifier and string arguments.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="arg1">A string argument.</param>
	/// <param name="arg2">A string argument.</param>
	/// <param name="arg3">A string argument.</param>
	protected void WriteEvent(int eventId, string arg1, string arg2, string arg3)
	{
		WriteEvent(eventId, new object[3] { arg1, arg2, arg3 });
	}

	/// <summary>Creates a new <see cref="Overload:System.Diagnostics.Tracing.EventSource.WriteEvent" /> overload by using the provided event identifier and event data.</summary>
	/// <param name="eventId">The event identifier.</param>
	/// <param name="eventDataCount">The number of event data items.</param>
	/// <param name="data">The structure that contains the event data.</param>
	[CLSCompliant(false)]
	protected unsafe void WriteEventCore(int eventId, int eventDataCount, EventData* data)
	{
	}

	/// <summary>Writes an event that indicates that the current activity is related to another activity. </summary>
	/// <param name="eventId">An identifier that uniquely identifies this event within the <see cref="T:System.Diagnostics.Tracing.EventSource" />. </param>
	/// <param name="childActivityID">The related activity identifier. </param>
	/// <param name="args">An array of objects that contain data about the event, or null if only the <paramref name="childActivityID" /> is needed.</param>
	protected void WriteEventWithRelatedActivityId(int eventId, Guid relatedActivityId, params object[] args)
	{
	}

	/// <summary>Writes an event that indicates that the current activity is related to another activity.</summary>
	/// <param name="eventId">An identifier that uniquely identifies this event within the <see cref="T:System.Diagnostics.Tracing.EventSource" />.</param>
	/// <param name="childActivityID">A pointer to the GUID of the child activity ID. </param>
	/// <param name="eventDataCount">The number of items in the <paramref name="data" /> field. </param>
	/// <param name="data">A pointer to the first item in the event data field. </param>
	[CLSCompliant(false)]
	protected unsafe void WriteEventWithRelatedActivityIdCore(int eventId, Guid* relatedActivityId, int eventDataCount, EventData* data)
	{
	}

	/// <summary>Returns a string of the XML manifest that is associated with the current event source.</summary>
	/// <returns>The XML data string.</returns>
	/// <param name="eventSourceType">The type of the event source.</param>
	/// <param name="assemblyPathToIncludeInManifest">The path to the .dll file to include in the manifest. </param>
	public static string GenerateManifest(Type eventSourceType, string assemblyPathToIncludeInManifest)
	{
		throw new NotImplementedException();
	}

	public static string GenerateManifest(Type eventSourceType, string assemblyPathToIncludeInManifest, EventManifestOptions flags)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets the unique identifier for this implementation of the event source.</summary>
	/// <returns>A unique identifier for this event source type.</returns>
	/// <param name="eventSourceType">The type of the event source.</param>
	public static Guid GetGuid(Type eventSourceType)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets the friendly name of the event source.</summary>
	/// <returns>The friendly name of the event source. The default is the simple name of the class.</returns>
	/// <param name="eventSourceType">The type of the event source.</param>
	public static string GetName(Type eventSourceType)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets a snapshot of all the event sources for the application domain.</summary>
	/// <returns>An enumeration of all the event sources in the application domain.</returns>
	public static IEnumerable<EventSource> GetSources()
	{
		throw new NotImplementedException();
	}

	/// <summary>Sends a command to a specified event source.</summary>
	/// <param name="eventSource">The event source to send the command to.</param>
	/// <param name="command">The event command to send.</param>
	/// <param name="commandArguments">The arguments for the event command.</param>
	public static void SendCommand(EventSource eventSource, EventCommand command, IDictionary<string, string> commandArguments)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the activity ID on the current thread.</summary>
	/// <param name="activityId">The current thread's new activity ID, or <see cref="F:System.Guid.Empty" /> to indicate that work on the current thread is not associated with any activity. </param>
	public static void SetCurrentThreadActivityId(Guid activityId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets the activity ID on the current thread, and returns the previous activity ID.</summary>
	/// <param name="activityId">The current thread's new activity ID, or <see cref="F:System.Guid.Empty" /> to indicate that work on the current thread is not associated with any activity.</param>
	/// <param name="oldActivityThatWillContinue">When this method returns, contains the previous activity ID on the current thread. </param>
	public static void SetCurrentThreadActivityId(Guid activityId, out Guid oldActivityThatWillContinue)
	{
		throw new NotImplementedException();
	}
}
