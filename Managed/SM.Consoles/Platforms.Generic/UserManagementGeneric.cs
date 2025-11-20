using System;
using System.Collections.Generic;
using SM.Utils;

namespace Platforms.Generic;

public class UserManagementGeneric : IPlatformUserManagement, IDisposable
{
	private readonly List<IPlatformUserData> _users = new List<IPlatformUserData>();

	public event Action PlatformUserUpdatedEvent;

	public void Dispose()
	{
	}

	public IPlatformUserData GetCurrentUser()
	{
		return _users[0];
	}

	public List<IPlatformUserData> GetCurrentUsers()
	{
		return _users;
	}

	public void SignIn(Action<bool> onSignInCompleted, bool isSignInUiRequired = false)
	{
		onSignInCompleted?.Invoke(obj: true);
		this.PlatformUserUpdatedEvent?.Invoke();
	}

	public void ResetCurrentUser()
	{
	}

	internal void AddPlatformUser(IPlatformUserData platformUser)
	{
		foreach (IPlatformUserData user in _users)
		{
			if (user.GetUnityInputUser() == platformUser.GetUnityInputUser())
			{
				return;
			}
		}
		_users.Add(platformUser);
		this.PlatformUserUpdatedEvent?.Invoke();
	}

	internal void RemovePlatformUser(IPlatformUserData platformUser)
	{
		foreach (IPlatformUserData user in _users)
		{
			if (user.GetUnityInputUser() == platformUser.GetUnityInputUser())
			{
				_users.Remove(platformUser);
				this.PlatformUserUpdatedEvent?.Invoke();
				return;
			}
		}
		LogUtils.LogError("[UserManagementGeneric] RemovePlatformUser() Was not able to remove platform user " + platformUser.GetPlatformDisplayName() + " " + platformUser.GetPlatformUniqueUserID() + " because it was not found!");
	}

	public IPlatformUserData GetPlatformUserWithUserId(string userId)
	{
		foreach (IPlatformUserData user in _users)
		{
			if (int.Parse(user.GetPlatformUniqueUserID()) == int.Parse(userId))
			{
				return user;
			}
		}
		LogUtils.LogError("[UserManagementGeneric] GetPlatformUserWithUserId(" + userId + ") Was not able to find platform user with this Id. Null returned.");
		return null;
	}
}
