using System;
using Platforms;

namespace Script.PlatformLayer;

public class AppFlowInformerService : IAppFlowInformer
{
	public bool AppStarted { get; private set; } = true;

	public bool AppSuspended { get; private set; }

	public event Action EventAppStarting;

	public event Action EventAppQuiting;

	public event Action EventAppSuspended;

	public event Action EventAppUnsuspended;

	public void OnSuspend()
	{
		AppSuspended = true;
		this.EventAppSuspended?.Invoke();
	}

	public void OnResume()
	{
		AppSuspended = false;
		this.EventAppUnsuspended?.Invoke();
	}
}
