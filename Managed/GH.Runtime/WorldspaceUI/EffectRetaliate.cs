using System;
using UnityEngine;
using UnityEngine.UI;

namespace WorldspaceUI;

public class EffectRetaliate : MonoBehaviour
{
	[SerializeField]
	private Image _retaliateRingImage;

	[SerializeField]
	private Image _retaliateIconImage;

	private bool _effectDeactivated;

	private bool _isWarningActive;

	private Transform _effectTransform;

	private RectTransform _ringRectTransform;

	private const string DebugPlayWarning = "PlayWarning";

	private const string DebugPlayActivate = "PlayActivated";

	private const string DebugCancelAnim = "PlayWarning";

	public void Initialise()
	{
		_effectDeactivated = false;
		_effectTransform = GetComponent<Transform>();
		_ringRectTransform = _retaliateRingImage.GetComponent<RectTransform>();
	}

	public void Activate()
	{
		PlayActivated();
	}

	public void Warn(bool active)
	{
		if (active)
		{
			_isWarningActive = true;
			PlayWarning();
		}
		else
		{
			_isWarningActive = false;
		}
	}

	private void PlayWarning()
	{
		if (!_isWarningActive || _effectDeactivated)
		{
			return;
		}
		Vector3 vectorOne = Vector3.one;
		Vector3 to = new Vector3(1.05f, 1.05f, 1.05f);
		_ringRectTransform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
		_retaliateRingImage.SetAlpha(0f);
		LeanTween.cancel(_ringRectTransform, "PlayWarning");
		LeanTween.alpha(_ringRectTransform, 1f, 0.5f);
		LeanTween.scale(_ringRectTransform, to, 0.5f).setOnComplete((Action)delegate
		{
			LeanTween.delayedCall(0.15f, (Action)delegate
			{
				LeanTween.alpha(_ringRectTransform, 0f, 0.5f);
				LeanTween.scale(_ringRectTransform, vectorOne, 0.5f).setOnComplete((Action)delegate
				{
					PlayWarning();
				});
			});
		});
	}

	public void PlayActivated()
	{
		if (_effectDeactivated)
		{
			return;
		}
		if (_isWarningActive)
		{
			LeanTween.cancel(_ringRectTransform, "PlayActivated");
			_isWarningActive = false;
		}
		Vector3 vectorOne = Vector3.one;
		_retaliateRingImage.SetAlpha(0f);
		_ringRectTransform.localScale = vectorOne;
		_effectTransform.localScale = vectorOne;
		float time = 0.1f;
		float keyFrameDuration1 = 0.225f;
		float keyFrameDuration2 = 0.3f;
		float keyFrameDuration3 = 0.5f;
		float keyFrameDuration4 = 0.5f;
		float glowDelay = 0.15f;
		Vector3 to = new Vector3(0.4f, 0.4f, 0.4f);
		Vector3 maxedSize = new Vector3(1.8f, 1.8f, 1.8f);
		Vector3 glowRingSize = new Vector3(1.05f, 1.05f, 1.05f);
		new Vector3(1.1f, 1.1f, 1.1f);
		LeanTween.alpha(_ringRectTransform, 0.9f, time);
		LeanTween.scale(_effectTransform.gameObject, to, time).setOnComplete((Action)delegate
		{
			LeanTween.scale(_effectTransform.gameObject, vectorOne, keyFrameDuration1).setOnComplete((Action)delegate
			{
				LeanTween.alpha(_ringRectTransform, 0f, keyFrameDuration2);
				LeanTween.scale(_ringRectTransform, maxedSize, keyFrameDuration2).setOnComplete((Action)delegate
				{
					_ringRectTransform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
					_retaliateRingImage.SetAlpha(0f);
					LeanTween.alpha(_ringRectTransform, 1f, keyFrameDuration3);
					LeanTween.scale(_ringRectTransform, glowRingSize, keyFrameDuration3).setOnComplete((Action)delegate
					{
						LeanTween.delayedCall(glowDelay, (Action)delegate
						{
							LeanTween.alpha(_ringRectTransform, 0f, keyFrameDuration4);
							LeanTween.scale(_ringRectTransform, vectorOne, keyFrameDuration4);
						});
					});
				});
			});
		});
	}

	public void CancelEffectRetaliateTweens()
	{
		_effectDeactivated = true;
		LeanTween.cancel(_ringRectTransform, "PlayWarning");
	}

	private void OnDisable()
	{
		CancelEffectRetaliateTweens();
	}
}
