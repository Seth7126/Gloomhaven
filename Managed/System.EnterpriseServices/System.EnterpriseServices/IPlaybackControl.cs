using System.Runtime.InteropServices;

namespace System.EnterpriseServices;

/// <summary>Functions in Queued Components in the abnormal handling of server-side playback errors and client-side failures of the Message Queuing delivery mechanism.</summary>
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("51372AFD-CAE7-11CF-BE81-00AA00A2FA25")]
public interface IPlaybackControl
{
	/// <summary>Informs the client-side exception-handling component that all Message Queuing attempts to deliver the message to the server were rejected, and the message ended up on the client-side Xact Dead Letter queue.</summary>
	void FinalClientRetry();

	/// <summary>Informs the server-side exception class implementation that all attempts to play back the deferred activation to the server have failed, and the message is about to be moved to its final resting queue.</summary>
	void FinalServerRetry();
}
