using System;

namespace Platforms;

public interface IAppFlowInformer
{
	bool AppStarted { get; }

	bool AppSuspended { get; }

	event Action EventAppStarting;

	event Action EventAppQuiting;

	event Action EventAppSuspended;

	event Action EventAppUnsuspended;

	void OnSuspend();

	void OnResume();
}
