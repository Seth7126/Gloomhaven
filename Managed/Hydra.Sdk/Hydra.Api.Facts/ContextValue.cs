using Google.Protobuf.Reflection;

namespace Hydra.Api.Facts;

public enum ContextValue
{
	[OriginalName("CONTEXT_VALUE_UNKNOWN")]
	Unknown,
	[OriginalName("CONTEXT_VALUE_GLOBAL_CONTEXT_UPDATE")]
	GlobalContextUpdate,
	[OriginalName("CONTEXT_VALUE_GLOBAL_CONTEXT_REMOVE")]
	GlobalContextRemove,
	[OriginalName("CONTEXT_VALUE_SDK_VERSION")]
	SdkVersion,
	[OriginalName("CONTEXT_VALUE_CLIENT_VERSION")]
	ClientVersion,
	[OriginalName("CONTEXT_VALUE_PLATFORM")]
	Platform,
	[OriginalName("CONTEXT_VALUE_KERNEL_SESSION_ID")]
	KernelSessionId,
	[OriginalName("CONTEXT_VALUE_KSIVA")]
	Ksiva,
	[OriginalName("CONTEXT_VALUE_GAME_CONFIGURATION")]
	GameConfiguration,
	[OriginalName("CONTEXT_VALUE_ROLE")]
	Role,
	[OriginalName("CONTEXT_VALUE_TITLE_ID")]
	TitleId,
	[OriginalName("CONTEXT_VALUE_ENVIRONMENT_ID")]
	EnvironmentId,
	[OriginalName("CONTEXT_VALUE_USER_ID")]
	UserId,
	[OriginalName("CONTEXT_VALUE_ACCOUNT_NAME")]
	AccountName,
	[OriginalName("CONTEXT_VALUE_PROVIDER")]
	Provider
}
