using System;
using Assets.Script.Misc;

namespace GLOO.Introduction;

public class UIIntroduceProcessBasic : UIIntroduceProcess
{
	private Action cancelAction;

	public override ICallbackPromise Process(IntroductionConfigUI introductionConfig)
	{
		if (introductionConfig == null)
		{
			Debug.LogErrorGUI("Introduction config is null at " + base.name);
			return CallbackPromise.Resolved();
		}
		if (Singleton<UIIntroductionManager>.Instance == null)
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise promise = new CallbackPromise();
		Singleton<UIIntroductionManager>.Instance.Show(introductionConfig, promise.Resolve);
		promise.Done(delegate
		{
			cancelAction = null;
		});
		cancelAction = delegate
		{
			cancelAction = null;
			Singleton<UIIntroductionManager>.Instance.HideById(introductionConfig.name);
			promise.Cancel();
		};
		return promise;
	}

	public override void Cancel()
	{
		cancelAction?.Invoke();
	}
}
