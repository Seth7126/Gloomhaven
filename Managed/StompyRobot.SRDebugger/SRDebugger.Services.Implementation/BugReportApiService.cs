using System;
using SRDebugger.Internal;
using SRF.Service;
using UnityEngine;

namespace SRDebugger.Services.Implementation;

[Service(typeof(IBugReportService))]
internal class BugReportApiService : IBugReportService
{
	private IBugReporterHandler _handler = new InternalBugReporterHandler();

	public bool IsUsable
	{
		get
		{
			if (_handler != null)
			{
				return _handler.IsUsable;
			}
			return false;
		}
	}

	public void SetHandler(IBugReporterHandler handler)
	{
		Debug.LogFormat("[SRDebugger] Bug Report handler set to {0}", handler);
		_handler = handler;
	}

	public void SendBugReport(BugReport report, BugReportCompleteCallback completeHandler, IProgress<float> progress = null)
	{
		if (_handler == null)
		{
			throw new InvalidOperationException("No bug report handler has been configured.");
		}
		if (!_handler.IsUsable)
		{
			throw new InvalidOperationException("Bug report handler is not usable.");
		}
		if (report == null)
		{
			throw new ArgumentNullException("report");
		}
		if (completeHandler == null)
		{
			throw new ArgumentNullException("completeHandler");
		}
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			completeHandler(didSucceed: false, "No Internet Connection");
			return;
		}
		_handler.Submit(report, delegate(BugReportSubmitResult result)
		{
			completeHandler(result.IsSuccessful, result.ErrorMessage);
		}, progress);
	}
}
