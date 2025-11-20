using System;
using Hydra.Sdk.Interfaces;

namespace Hydra.Sdk.Communication.States;

public class SdkSessionInfo : IHydraSdkStateWrapper
{
	public string SessionId { get; set; }

	public string UserId { get; set; }

	public string TitleId { get; set; }

	public string EnvironmentId { get; set; }

	public DateTime InitTime { get; }

	public DateTime StartTime { get; set; }

	public SdkSessionInfo()
	{
		InitTime = DateTime.UtcNow;
		StartTime = DateTime.UtcNow;
	}

	public SdkSessionInfo(SdkSessionInfo previousInfo)
	{
		InitTime = previousInfo.InitTime;
		StartTime = previousInfo.StartTime;
	}
}
