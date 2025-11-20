using Google.Protobuf.Reflection;

namespace Hydra.Api.SessionControl;

public enum TeamId
{
	[OriginalName("TEAM_ID_UNKNOWN")]
	Unknown,
	[OriginalName("TEAM_ID_1")]
	_1,
	[OriginalName("TEAM_ID_2")]
	_2,
	[OriginalName("TEAM_ID_SPECTATOR")]
	Spectator
}
