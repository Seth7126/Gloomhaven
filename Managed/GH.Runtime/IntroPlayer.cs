using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Utilities;

public class IntroPlayer : MonoBehaviour
{
	[Header("Video")]
	[SerializeField]
	private VideoPlayer _player;

	[SerializeField]
	private float _videoDuration;

	[Header("Logo References")]
	[SerializeField]
	private Image _logo;

	[SerializeField]
	private List<Sprite> _logos;

	[SerializeField]
	private Sprite _unityLogo;

	[Header("Logo Animation Settings")]
	[SerializeField]
	private float _logoShowingDuration;

	[SerializeField]
	private float _alphaChangeDuration = 1f;

	[SerializeField]
	private Vector2 _logoStartedScale = Vector2.one / 2f;

	[SerializeField]
	private Vector2 _logoTweenMultiplier = Vector2.one * 2f;

	[SerializeField]
	private AnimationCurve _logoEaseCurve;

	[SerializeField]
	private GameObject _loading;

	private Coroutine _videoCoroutine;

	private RectTransform _logoRectTransform;

	private WaitForSeconds _logoShowingWaitForSeconds;

	private WaitForSeconds _alphaWaitForSeconds;

	public event Action EventCompleted;

	private void Start()
	{
		InputSystemUtilities.DisableMouseCursor();
		_logoRectTransform = _logo.GetComponent<RectTransform>();
		_logoShowingWaitForSeconds = new WaitForSeconds(_logoShowingDuration - _alphaChangeDuration);
		_alphaWaitForSeconds = new WaitForSeconds(_alphaChangeDuration);
		ShowVideo();
	}

	private void ShowVideo()
	{
		_videoCoroutine = StartCoroutine(WaitVideoEnd());
		_player.Play();
	}

	private IEnumerator WaitVideoEnd()
	{
		yield return new WaitForSeconds(_videoDuration);
		_player.gameObject.SetActive(value: false);
		StopCoroutine(_videoCoroutine);
		StartCoroutine(ShowLogos(delegate
		{
			this.EventCompleted?.Invoke();
			InputSystemUtilities.EnableMouseCursor();
		}));
	}

	private IEnumerator ShowLogos(Action onCompleted)
	{
		foreach (Sprite logo in _logos)
		{
			_logo.sprite = logo;
			_logoRectTransform.localScale = _logoStartedScale;
			LTDescr alphaTween = LeanTween.alpha(_logoRectTransform, 1f, _alphaChangeDuration).setEase(LeanTweenType.linear);
			LeanTween.scale(_logoRectTransform, _logoTweenMultiplier, _logoShowingDuration).setEase(_logoEaseCurve);
			yield return _logoShowingWaitForSeconds;
			LeanTween.cancel(alphaTween.id);
			LeanTween.alpha(_logoRectTransform, 0f, _alphaChangeDuration).setEase(LeanTweenType.linear);
			yield return _alphaWaitForSeconds;
			LeanTween.cancel(_logoRectTransform);
		}
		_loading.SetActive(value: true);
		onCompleted?.Invoke();
	}
}
