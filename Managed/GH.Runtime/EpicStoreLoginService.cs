using Assets.Script.Misc;
using PlayEveryWare.EpicOnlineServices;

public class EpicStoreLoginService : INetworkLoginService
{
	public bool IsSignedIn()
	{
		return SaveData.Instance.Global.EpicLogin;
	}

	public ICallbackPromise SignIn()
	{
		PlatformLayer.Instance.EOSInitialise();
		return CallbackPromise.Resolved();
	}

	public ICallbackPromise SignOut()
	{
		EOSManager.Instance.StartLogout(EOSManager.Instance.GetLocalUserId(), null);
		SaveData.Instance.Global.EpicLogin = false;
		return CallbackPromise.Resolved();
	}

	public bool IsSigningOut()
	{
		return false;
	}

	public bool IsSigningIn()
	{
		return false;
	}
}
