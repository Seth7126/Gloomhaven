using UnityEngine;

public class UIEnchantressEffect : MonoBehaviour
{
	[SerializeField]
	private GameObject enchantressEffect;

	[SerializeField]
	private float rotationTime = 20f;

	[SerializeField]
	private LoopAnimator idleAnimator;

	[SerializeField]
	private GameObject buyEffect;

	[SerializeField]
	private GameObject sellEffect;

	[SerializeField]
	private float rotationSpeed = 1f;

	private LTDescr rotationAnimation;

	public void ShowModeEffect(bool isBuy)
	{
		buyEffect.SetActive(isBuy);
		sellEffect.SetActive(!isBuy);
	}

	public void Play()
	{
		idleAnimator.StartLoop();
		if (rotationAnimation == null)
		{
			Rotate();
		}
	}

	private void Rotate()
	{
		float z = enchantressEffect.transform.rotation.eulerAngles.z;
		rotationAnimation = LeanTween.value(enchantressEffect, z, z + rotationSpeed * 360f, rotationTime).setOnUpdate(delegate(float val)
		{
			enchantressEffect.transform.eulerAngles = new Vector3(0f, 0f, val);
		}).setOnComplete(Rotate);
	}

	public void Stop()
	{
		if (rotationAnimation != null)
		{
			LeanTween.cancel(rotationAnimation.id);
			rotationAnimation = null;
		}
		idleAnimator.StopLoop(resetToInitial: true);
	}

	private void OnDisable()
	{
		Stop();
	}
}
