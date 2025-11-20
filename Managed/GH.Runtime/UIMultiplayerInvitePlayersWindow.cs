using System;
using System.Collections.Generic;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;

public class UIMultiplayerInvitePlayersWindow : MonoBehaviour
{
	[SerializeField]
	private ExtendedScrollRect scroll;

	[SerializeField]
	private UIMultiplayerInvitePlayerSlot slotPrefab;

	[SerializeField]
	private VerticalPointerUI pointerTo;

	[SerializeField]
	private List<UIMultiplayerInvitePlayerSlot> slots;

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private Dictionary<IInvitePlayer, UIMultiplayerInvitePlayerSlot> assignedSlots = new Dictionary<IInvitePlayer, UIMultiplayerInvitePlayerSlot>();

	private INetworkInviteService inviteService;

	private Action onClosed;

	private void Awake()
	{
		inviteService = CreateDummyInviteService();
		window.onHidden.AddListener(delegate
		{
			controllerArea.Destroy();
			onClosed?.Invoke();
		});
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
	}

	private INetworkInviteService CreateDummyInviteService()
	{
		return new DummyNetworkInviteService();
	}

	public void Show(Action onClosed, RectTransform pointTo)
	{
		this.onClosed = onClosed;
		pointerTo.PointAt(pointTo);
		if (inviteService == null)
		{
			inviteService = CreateDummyInviteService();
		}
		Decorate(inviteService.GetPlayersToInvite());
		scroll.ScrollToTop();
		controllerArea.Enable();
		window.ShowOrUpdateStartingState();
	}

	public void Hide()
	{
		assignedSlots.Clear();
		window.Hide();
	}

	private void Decorate(List<IInvitePlayer> invitePlayers)
	{
		assignedSlots.Clear();
		HelperTools.NormalizePool(ref slots, slotPrefab.gameObject, scroll.content, invitePlayers.Count);
		for (int i = 0; i < invitePlayers.Count; i++)
		{
			IInvitePlayer player = invitePlayers[i];
			slots[i].SetPlayer(player, delegate
			{
				InvitePlayer(player);
			});
			assignedSlots[player] = slots[i];
		}
	}

	private void InvitePlayer(IInvitePlayer player)
	{
		inviteService.SendInvite(player).Done(delegate
		{
			if (window.IsOpen)
			{
				assignedSlots[player].SetInvited(selected: true);
			}
		});
	}

	private void DisableNavigation()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerOnlineContainerWithSelected);
	}

	private void EnableNavigation()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerInvitePlayers);
	}
}
