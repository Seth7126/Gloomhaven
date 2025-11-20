using System;
using UnityEngine;

namespace GLOO.Introduction;

public abstract class UIIntroduceBase : UIIntroduce
{
	[SerializeField]
	private UIIntroduceProcess process;

	private bool isShown;

	public override void Hide()
	{
		if (isShown)
		{
			process.Cancel();
			isShown = false;
		}
	}

	protected void Show(IntroductionConfigUI config, Action onFinished = null)
	{
		if (config == null)
		{
			Debug.LogErrorGUI("Missing introduction in " + base.name);
			onFinished?.Invoke();
			return;
		}
		isShown = true;
		process.Process(config).Done(delegate
		{
			isShown = false;
			onFinished?.Invoke();
		}, delegate
		{
			isShown = false;
			onFinished?.Invoke();
		});
	}
}
