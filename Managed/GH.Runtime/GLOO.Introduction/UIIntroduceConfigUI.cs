using System;
using UnityEngine;

namespace GLOO.Introduction;

public class UIIntroduceConfigUI : UIIntroduceBase
{
	[SerializeField]
	protected IntroductionConfigUI config;

	public override void Show(Action onFinished = null)
	{
		Show(config, onFinished);
	}
}
