using Hydra.Api.Facts;

namespace Hydra.Sdk.Components.Facts.Core;

public class FactConstants
{
	public static readonly string ServiceName = FactsApi.Descriptor.FullName;

	public const string AdfContextUpdate = "ADF/GLOBAL_CONTEXT/UPDATE";

	public const string ContextInformation = "INFO";

	public const string ContextWarning = "WARNING";

	public const string ContextError = "ERROR";

	public const string Ksiva = "KSIVA";

	public const string KernelSessionId = "KERNEL_SESSION_ID";

	public const string Role = "ROLE";

	public const string SdkVersion = "SDK_VERSION";

	public const string ClientVersion = "CLIENT_VERSION";

	public const string TitleId = "TITLE_ID";

	public const string EnvironmentId = "ENVIRONMENT_ID";

	public const string UserId = "USER_ID";

	public const string Provider = "PROVIDER";

	public const string Platform = "PLATFORM";

	public const string AccountName = "ACCOUNT_NAME";

	public const string HttpVersion = "HTTP_VERSION";
}
