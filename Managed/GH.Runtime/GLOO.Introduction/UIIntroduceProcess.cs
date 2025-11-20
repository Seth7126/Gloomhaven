using Assets.Script.Misc;
using UnityEngine;

namespace GLOO.Introduction;

public abstract class UIIntroduceProcess : MonoBehaviour
{
	public abstract ICallbackPromise Process(IntroductionConfigUI introductionConfig);

	public abstract void Cancel();
}
