using System;
using FFSNet;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSymbolTab : MonoBehaviour
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private TextMeshProUGUI selectedNumberText;

	[SerializeField]
	private InteractabilityIsolatedUIControl m_IsolatedControl;

	[SerializeField]
	private Image _characterPortraitImage;

	[SerializeField]
	private GameObject exhaustedMask;

	[SerializeField]
	private float exhaustedOppacity = 0.6f;

	[SerializeField]
	private CanvasGroup tabCanvasGroup;

	[Header("Multiplayer")]
	[SerializeField]
	private Color otherPlayerIconColor;

	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private float highlightScale = 1.2f;

	[SerializeField]
	private float highlightAlpha = 0.2f;

	[SerializeField]
	private CharacterSelectedAnimation _characterSelectedAnimation;

	private bool selectFromOutside;

	private int maxNumber = 2;

	private int selectedNumber;

	private CPlayerActor playerActor;

	private Action<CharacterClickedData> onClickAction;

	public CPlayerActor PlayerActor => playerActor;

	public bool IsSelected => toggle.isOn;

	public void Init(ToggleGroup toggleGroup, CPlayerActor playerActor, Action<CharacterClickedData> onClickAction)
	{
		this.playerActor = playerActor;
		m_IsolatedControl.ControlSecondIdentifier = playerActor.ActorGuid;
		Sprite characterHeroPortrait = UIInfoTools.Instance.GetCharacterHeroPortrait(playerActor.Class.DefaultModel, playerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		_characterPortraitImage.sprite = characterHeroPortrait;
		toggle.group = toggleGroup;
		this.onClickAction = onClickAction;
		UpdateSelectedCardsNum(0);
		RefreshPlayerOnlineState();
		RefreshDead();
		UpdateMask(toggle.isOn);
	}

	public void RefreshPlayerOnlineState()
	{
		_characterPortraitImage.color = ((!FFSNetwork.IsOnline || playerActor.IsUnderMyControl) ? UIInfoTools.Instance.White : otherPlayerIconColor);
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		if (!FFSNetwork.IsOnline || !playerActor.IsUnderMyControl || toggle.isOn || playerActor.IsDead || maxNumber <= selectedNumber)
		{
			highlightImage.gameObject.SetActive(value: false);
		}
	}

	public void UpdateSelectedCardsNum(int number)
	{
		selectedNumber = number;
		selectedNumberText.text = $"{number}/{maxNumber}";
		RefreshHighlight();
	}

	public void UpdateMaxSelectedCardsNum(int maxNumber)
	{
		this.maxNumber = maxNumber;
		selectedNumberText.text = $"{selectedNumber}/{maxNumber}";
		RefreshHighlight();
	}

	public void Select()
	{
		toggle.isOn = true;
	}

	public void Click()
	{
		toggle.isOn = true;
	}

	public void OnClick(bool isOn)
	{
		if (isOn && onClickAction != null)
		{
			onClickAction(new CharacterClickedData(playerActor, this));
		}
		if (selectFromOutside)
		{
			selectFromOutside = false;
		}
		AnimateSelection(isOn);
		RefreshHighlight();
		UpdateMask(isOn);
	}

	private void UpdateMask(bool isOn)
	{
		_characterPortraitImage.material = ((!playerActor.IsDead && !isOn) ? UIInfoTools.Instance.greyedOutMaterial : null);
	}

	public void AnimateSelection(bool isOn)
	{
		if (_characterSelectedAnimation != null)
		{
			_characterSelectedAnimation.Animate(isOn);
		}
	}

	public void GrayOutTab(bool isGrayedOut)
	{
		_characterPortraitImage.material = (isGrayedOut ? UIInfoTools.Instance.greyedOutMaterial : null);
	}

	public void SetInteractable(bool interactable)
	{
		FFSNet.Console.LogInfo((interactable ? "Enabling" : "Disabling") + " card hand tab for " + playerActor.CharacterClass.ID);
		toggle.interactable = interactable;
	}

	public void SetHighlightProgress(float percent)
	{
		if (!playerActor.IsDead)
		{
			highlightImage.SetAlpha((1f - percent) * highlightAlpha);
			float num = (highlightScale - 1f) * percent + 1f;
			highlightImage.transform.localScale = new Vector3(num, num, 1f);
		}
	}

	public void RefreshDead()
	{
		RefreshHighlight();
		_characterPortraitImage.material = ((playerActor.IsDead || !toggle.isOn) ? UIInfoTools.Instance.greyedOutMaterial : null);
		exhaustedMask.SetActive(playerActor.IsDead);
		selectedNumberText.gameObject.SetActive(!playerActor.IsDead);
		tabCanvasGroup.alpha = (playerActor.IsDead ? exhaustedOppacity : 1f);
	}
}
