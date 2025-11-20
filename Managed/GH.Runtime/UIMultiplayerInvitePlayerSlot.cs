using System;
using AsmodeeNet.Foundation;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerInvitePlayerSlot : MonoBehaviour
{
	[Header("Highlight")]
	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private Color highlightColor;

	[SerializeField]
	private float unhighlightDuration = 0.1f;

	[Header("Components")]
	[SerializeField]
	private Image inviteMask;

	[SerializeField]
	private Image invitedMask;

	[SerializeField]
	protected ExtendedButton button;

	[SerializeField]
	protected TextMeshProUGUI username;

	[SerializeField]
	protected RawImage userAvatar;

	[SerializeField]
	protected TextMeshProUGUI statusText;

	[SerializeField]
	protected Graphic[] statusGraphics;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	private Action onSelected;

	private LTDescr unhighlightAnimation;

	protected virtual void Awake()
	{
		button.onClick.AddListener(delegate
		{
			onSelected?.Invoke();
		});
		button.onMouseEnter.AddListener(delegate
		{
			RefreshHighlight(isHighlighted: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			RefreshHighlight(isHighlighted: false);
		});
	}

	public void SetPlayer(IInvitePlayer player, Action onSelected)
	{
		this.onSelected = onSelected;
		username.text = player.Username;
		userAvatar.texture = player.Avatar;
		statusText.text = ((!player.IsOnline) ? LocalizationManager.GetTranslation("GUI_PLAYER_OFFLINE") : (player.CurrentlyPlayingGameName.IsNullOrEmpty() ? LocalizationManager.GetTranslation("GUI_PLAYER_ONLINE") : (LocalizationManager.GetTranslation("GUI_PLAYING_GAME") + " " + player.CurrentlyPlayingGameName)));
		Color color = (player.IsOnline ? UIInfoTools.Instance.playerOnlineColor : UIInfoTools.Instance.playerConnectingColor);
		for (int i = 0; i < statusGraphics.Length; i++)
		{
			statusGraphics[i].color = color;
		}
		SetInvited(player.IsInvited);
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}

	public void EnableNavigation(bool select = false)
	{
		button.SetNavigation(Navigation.Mode.Vertical);
		if (select)
		{
			button.Select();
		}
	}

	private void RefreshHighlight(bool isHighlighted)
	{
		if (isHighlighted)
		{
			CancelHighlightAnimations();
			highlightImage.color = highlightColor;
		}
		else if (unhighlightAnimation == null)
		{
			unhighlightAnimation = LeanTween.alpha(highlightImage.transform as RectTransform, 0f, unhighlightDuration).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
			{
				unhighlightAnimation = null;
			});
		}
	}

	private void CancelHighlightAnimations()
	{
		if (unhighlightAnimation != null)
		{
			LeanTween.cancel(unhighlightAnimation.id);
			unhighlightAnimation = null;
			highlightImage.SetAlpha(0f);
		}
	}

	protected void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			CancelHighlightAnimations();
			DisableNavigation();
		}
	}

	public void SetInvited(bool selected)
	{
		invitedMask.enabled = selected;
		inviteMask.enabled = !selected;
		tooltip.SetText(LocalizationManager.GetTranslation(selected ? "Consoles/GUI_MP_ALREADY_INVITED" : "GUI_MP_SEND_INVITE"), refreshTooltip: true);
	}
}
