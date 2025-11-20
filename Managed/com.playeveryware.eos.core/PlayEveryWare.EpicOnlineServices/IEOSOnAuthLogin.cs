using Epic.OnlineServices.Auth;

namespace PlayEveryWare.EpicOnlineServices;

public interface IEOSOnAuthLogin
{
	void OnAuthLogin(LoginCallbackInfo loginCallbackInfo);
}
