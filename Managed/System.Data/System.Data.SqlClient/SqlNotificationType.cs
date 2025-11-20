namespace System.Data.SqlClient;

/// <summary>Describes the different notification types that can be received by an <see cref="T:System.Data.SqlClient.OnChangeEventHandler" /> event handler through the <see cref="T:System.Data.SqlClient.SqlNotificationEventArgs" /> parameter.</summary>
/// <filterpriority>2</filterpriority>
public enum SqlNotificationType
{
	/// <summary>Data on the server being monitored changed. Use the <see cref="T:System.Data.SqlClient.SqlNotificationInfo" /> item to determine the details of the change.</summary>
	Change = 0,
	/// <summary>There was a failure to create a notification subscription. Use the <see cref="T:System.Data.SqlClient.SqlNotificationEventArgs" /> object's <see cref="T:System.Data.SqlClient.SqlNotificationInfo" /> item to determine the cause of the failure.</summary>
	Subscribe = 1,
	/// <summary>Used when the type option sent by the server was not recognized by the client.</summary>
	Unknown = -1
}
