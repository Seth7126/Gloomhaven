using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using VoiceChat;

namespace Script.GUI.IngameMenu.EscMenuVoiceChat;

public class EscMenuVoiceChatRow : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private TMP_Text _userNameText;

	[SerializeField]
	private float _visibilityWhenSpeaking;

	[SerializeField]
	private float _visibilityWhenNotSpeaking;

	private IUserVoice _voice;

	private bool _isInited;

	private List<IVoiceComponent> _voiceComponents;

	private CanvasGroup _canvasGroup;

	private void Awake()
	{
		EnsureInited();
		_canvasGroup = GetComponent<CanvasGroup>();
		base.gameObject.SetActive(value: false);
	}

	private void EnsureInited()
	{
		if (!_isInited)
		{
			_voiceComponents = new List<IVoiceComponent>();
			GetComponentsInChildren(includeInactive: true, _voiceComponents);
			_isInited = true;
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Show(IUserVoice voice)
	{
		base.gameObject.SetActive(value: true);
		_voice = voice;
		_voiceComponents.ForEach(delegate(IVoiceComponent x)
		{
			x.Init(new VoiceContext
			{
				UserVoice = _voice
			});
		});
	}

	[UsedImplicitly]
	private void Update()
	{
		_voiceComponents.ForEach(delegate(IVoiceComponent x)
		{
			x.UpdateComponent();
		});
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		if (_voice.IsSpeaking)
		{
			_canvasGroup.alpha = _visibilityWhenSpeaking;
		}
		else
		{
			_canvasGroup.alpha = _visibilityWhenNotSpeaking;
		}
	}
}
