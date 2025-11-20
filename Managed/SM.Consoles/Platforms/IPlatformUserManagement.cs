using System;
using System.Collections.Generic;

namespace Platforms;

public interface IPlatformUserManagement : IDisposable
{
	event Action PlatformUserUpdatedEvent;

	IPlatformUserData GetCurrentUser();

	List<IPlatformUserData> GetCurrentUsers();

	void SignIn(Action<bool> onSignInCompleted, bool isSignInUiRequired = false);

	void ResetCurrentUser();
}
