using Google.Protobuf.Reflection;

namespace Hydra.Api.UserReports;

public enum SendUserReportResult
{
	[OriginalName("SEND_USER_REPORT_RESULT_NONE")]
	None,
	[OriginalName("SEND_USER_REPORT_RESULT_SUCCESS")]
	Success,
	[OriginalName("SEND_USER_REPORT_RESULT_DAILY_LIMIT_REACHED")]
	DailyLimitReached,
	[OriginalName("SEND_USER_REPORT_RESULT_USER_LIMIT_REACHED")]
	UserLimitReached
}
