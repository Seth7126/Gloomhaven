using UnityEngine;

public abstract class CustomLeanTweenAnimator : MonoBehaviour
{
	public abstract void SetFinalValue();

	public abstract void RestorOriginalValue();

	public abstract LTDescr BuildTweenAction(float duration);
}
