using System;
using System.Collections.Generic;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UICharacterCreatorConfirmationBox : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener titleBanner;

	[SerializeField]
	private List<Image> characterIcons;

	[SerializeField]
	private List<Image> characterGlows;

	[SerializeField]
	private GUIAnimator showAnimator;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	private ReferenceToSprite _referenceOnIcon;

	private ReferenceToSprite _referenceOnGlow;

	private UIWindow window;

	private ECharacter character;

	private bool _isLoaded;

	protected void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHidden);
		showAnimator.OnAnimationFinished.AddListener(Hide);
	}

	public void HighlightCharacter()
	{
		Singleton<Character3DDisplayManager>.Instance.Display(this, character, "", UIInfoTools.Instance.GetCharacterCreateAnimation(character));
	}

	public void Show(CMapCharacter character, Action onFinished)
	{
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		StopAllCoroutines();
		CharacterYMLData characterYMLData = character.CharacterYMLData;
		_referenceOnIcon = UIInfoTools.Instance.GetCharacterAssemblyIcon(characterYMLData.Model, characterYMLData.CustomCharacterConfig);
		_referenceOnGlow = UIInfoTools.Instance.GetCharacterSpriteRef(characterYMLData.Model, highlight: true, characterYMLData.CustomCharacterConfig);
		this.character = characterYMLData.Model;
		Singleton<Character3DDisplayManager>.Instance.Display(this, characterYMLData.Model);
		titleBanner.SetArguments(character.CharacterName.IsNullOrEmpty() ? LocalizationManager.GetTranslation(characterYMLData.LocKey) : character.CharacterName);
		window.onTransitionComplete.RemoveAllListeners();
		window.onTransitionComplete.AddListener(delegate(UIWindow _, UIWindow.VisualState stat)
		{
			if (stat == UIWindow.VisualState.Hidden)
			{
				Singleton<Character3DDisplayManager>.Instance.HideAll(this);
				onFinished?.Invoke();
			}
		});
		for (int num = 0; num < characterGlows.Count; num++)
		{
			_imageSpriteLoader.AddReferenceToSpriteForImage(characterGlows[num], _referenceOnGlow, delegate
			{
				OnFinishLoad();
			});
		}
		for (int num2 = 0; num2 < characterIcons.Count; num2++)
		{
			_imageSpriteLoader.AddReferenceToSpriteForImage(characterIcons[num2], _referenceOnIcon, delegate
			{
				OnFinishLoad();
			});
		}
	}

	public void Hide()
	{
		window.Hide();
	}

	private void OnFinishLoad()
	{
		if (!_isLoaded)
		{
			showAnimator.Play();
			window.Show();
		}
		_isLoaded = true;
	}

	protected void OnHidden()
	{
		_imageSpriteLoader.Release();
		_isLoaded = false;
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
		StopAllCoroutines();
		showAnimator.Stop();
	}

	protected void OnDisable()
	{
		showAnimator.Stop();
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
	}
}
