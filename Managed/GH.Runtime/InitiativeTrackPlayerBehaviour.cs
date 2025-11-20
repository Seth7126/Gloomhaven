using System.Linq;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;

public class InitiativeTrackPlayerBehaviour : InitiativeTrackActorBehaviour
{
	[SerializeField]
	private Transform cardsHighlightHolder;

	[SerializeField]
	private MultiplayerUserState controllerPlayerInfo;

	[SerializeField]
	private GUIAnimator warningAnimation;

	public override CActor.EType ActorType()
	{
		return CActor.EType.Player;
	}

	protected override void OnAvatarHighlight(bool active)
	{
		CPlayerActor playerActor = (CPlayerActor)base.Actor;
		if ((!active || !isSelected || PhaseManager.PhaseType == CPhase.PhaseType.EndRound || !CardsHandManager.Instance.IsShowingPlayerHand(playerActor)) && (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentActor == null || !base.Actor.Equals(Choreographer.s_Choreographer.m_CurrentActor)))
		{
			if (active && PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest && !CardsHandManager.Instance.IsShowingPlayerHand(playerActor))
			{
				CardsHandManager.Instance.Preview(playerActor, cardsHighlightHolder);
			}
			else
			{
				CardsHandManager.Instance.StopPreview();
			}
		}
	}

	private static void ClearShortRest()
	{
		Choreographer.s_Choreographer.readyButton.Toggle(active: true);
		Choreographer.s_Choreographer.m_SkipButton.Toggle(active: false);
	}

	public override void SetAttributes(CActor actor, bool activePlayerButton, bool changeHilight, bool generateCard = true)
	{
		base.SetAttributes(actor, activePlayerButton, changeHilight);
		RefreshPlayerController();
		ShowWarning(show: false);
	}

	public override void SetAttributesDirect(CActor actor, bool activeIDBackplate, bool activeID, string textID, bool activeInitiative, int initiative, int currentHealth, int maxHealth, bool isActive, bool activeHilight, bool activeButton)
	{
		base.SetAttributesDirect(actor, activeIDBackplate, activeID, textID, activeInitiative, initiative, currentHealth, maxHealth, isActive, activeHilight, activeButton);
		RefreshPlayerController();
		ShowWarning(show: false);
	}

	public void ShowWarning(bool show)
	{
		if (show)
		{
			warningAnimation.Play();
			return;
		}
		warningAnimation.Stop();
		warningAnimation.GoInitState();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ShowExtraTurn(show: false);
	}

	public void RefreshPlayerController()
	{
		if (FFSNetwork.IsOnline)
		{
			NetworkPlayer networkPlayer = null;
			if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
			{
				CPlayerActor playerActor = (CPlayerActor)base.Actor;
				networkPlayer = ((playerActor.CharacterName != null) ? ControllableRegistry.GetControllable(playerActor.CharacterName.GetHashCode()).Controller : ControllableRegistry.GetControllable(AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == playerActor.Class.ID).CharacterName.GetHashCode()).Controller);
			}
			else
			{
				networkPlayer = ControllableRegistry.GetControllable((base.Actor.Class as CCharacterClass).ModelInstanceID).Controller;
			}
			controllerPlayerInfo.Show(networkPlayer, PlayerRegistry.AllPlayers.Contains(networkPlayer));
		}
		else
		{
			controllerPlayerInfo.Hide();
		}
	}
}
