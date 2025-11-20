using UnityEngine;

namespace GLOO.Introduction;

public abstract class IntroductionConditionConfigUI : ScriptableObject
{
	public abstract bool IsValid();
}
