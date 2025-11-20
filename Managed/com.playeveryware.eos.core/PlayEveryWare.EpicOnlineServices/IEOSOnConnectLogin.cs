using Epic.OnlineServices.Connect;

namespace PlayEveryWare.EpicOnlineServices;

public interface IEOSOnConnectLogin
{
	void OnConnectLogin(LoginCallbackInfo loginCallbackInfo);
}
