using Epic.OnlineServices.Auth;

namespace PlayEveryWare.EpicOnlineServices;

public interface IEOSOnAuthLogout
{
	void OnAuthLogout(LogoutCallbackInfo logoutCallbackInfo);
}
