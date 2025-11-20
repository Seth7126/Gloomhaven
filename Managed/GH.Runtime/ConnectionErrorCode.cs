public enum ConnectionErrorCode
{
	[Name("None", "Unknown Error Occured.")]
	None,
	[Name("Different Build Type", "Game build types do not match.")]
	DifferentBuildType,
	[Name("Different Version", "Game versions do not match.")]
	DifferentVersion,
	[Name("No Session Found", "Timed out searching for the session.")]
	SessionNotFound,
	[Name("Session Full", "The session is full.")]
	SessionFull,
	[Name("User Blocked", "The host has blocked you from the session.")]
	UserBlocked,
	[Name("Incorrect Password", "The password is incorrect.")]
	IncorrectPassword,
	[Name("Invalid Code", "The code used is invalid.")]
	InvalidCode,
	[Name("Invalid User Data", "The user data provided is invalid.")]
	InvalidUserData,
	[Name("Invalid File Path", "Could not process the target save file path. Disconnecting.")]
	InvalidFilePath,
	[Name("Invalid Session Data", "Invalid session data provided. Disconnecting.")]
	InvalidSessionData,
	[Name("Invalid Game Mode", "Unsuitable or unfinished game mode found.")]
	InvalidGameMode,
	[Name("Connection To Backend Failed", "Could connect to the multiplayer backend.")]
	ConnectionToBackendFailed,
	[Name("Connection To Session Failed", "Could not connect to the session.")]
	ConnectionToSessionFailed,
	[Name("Mods do not match", "The host has mods enabled.  All clients must have the same mods.")]
	ModsDoNotMatch,
	[Name("Session Shutting Down", "The session is shutting down")]
	SessionShuttingDown,
	[Name("Downloadable content was not found on this device:\n\"Solo Scenarios\"")]
	DLCDoNotMatchSoloScenarious,
	[Name("Downloadable content was not found on this device:\n\"Jaws Of The Lion\"")]
	DLCDoNotMatchJotl,
	[Name("Downloadable content was not found on this device:\n\"Jaws Of The Lion\"\n\"Solo Scenarios\"")]
	DLCDoNotMatchJotlAndSoloScenarious,
	[Name("Crossplay disabled by client", "Server has option 'Crossplay' = OFF")]
	CrossplayDisabledByClient,
	[Name("Crossplay disabled by server", "Client has option 'Crossplay' = OFF")]
	CrossplayDisabledByServer,
	[Name("Not enough savedata size for host save")]
	NotEnoughMemory,
	[Name("Multiplayer privilege is not available")]
	MultiplayerValidationFail,
	[Name("User in session in your blocklist")]
	UserInSessionBlockedByCurrentUser,
	[Name("User in session has blocked you")]
	CurrentUserBlockedByUserInSession,
	[Name("You were kicked by the host")]
	KickedByHost,
	[Name("User has no NSO subscription")]
	AuthenticationFail,
	[Name("User can not connect because of blocklist")]
	SessionCouldNotBeJoined,
	[Name("Session Shut Down", "The session was shut down")]
	SessionShutDown
}
