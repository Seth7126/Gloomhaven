using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class AdventureCharacterCreator : MonoBehaviour
{
	[SerializeField]
	private GUIAnimator showAnimator;

	[SerializeField]
	private UICharacterCreatorWindow creatorWindow;

	[SerializeField]
	private UICharacterCreatorConfirmationBox confirmationBox;

	[SerializeField]
	private Image mask;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	private CallbackPromise<CMapCharacter> promise;

	private CMapParty party;

	private ICharacterCreatorService service = new CharacterCreatorService();

	public UICharacterCreatorWindow CreatorWindow => creatorWindow;

	public bool IsCreating
	{
		get
		{
			if (promise != null)
			{
				return promise.IsPending;
			}
			return false;
		}
	}

	private void Awake()
	{
		showAnimator.OnAnimationFinished.AddListener(OnShown);
	}

	public CallbackPromise<CMapCharacter> Create(CMapParty party)
	{
		this.party = party;
		CameraController.s_CameraController?.RequestDisableCameraInput(this);
		promise = new CallbackPromise<CMapCharacter>();
		showAnimator.Play();
		mask.enabled = true;
		if (FFSNetwork.IsOnline)
		{
			PlayerRegistry.MyPlayer.ToggleCharacterCreationScreen(value: true);
		}
		controllerArea.Enable();
		return promise;
	}

	public void Close()
	{
		confirmationBox.Hide();
		if (promise != null && promise.IsPending)
		{
			if (creatorWindow.IsShown)
			{
				creatorWindow.Cancel();
			}
			else
			{
				Cancel();
			}
		}
	}

	private void OnShown()
	{
		List<string> list = party.UnlockedCharacterIDs.Where((string it) => !AdventureState.MapState.MapParty.CharacterPendingReward(it)).ToList();
		creatorWindow.Build(list.ConvertAll((Converter<string, ICharacterCreatorClass>)((string it) => new MapCharacterClassData(ScenarioRuleClient.SRLYML.Characters.SingleOrDefault((CharacterYMLData x) => x.ID == it)))), service, delegate(ICharacterCreatorClass character)
		{
			party.RemoveNewUnlockedCharacter(character.ID);
		}).Done(OnCreated, Cancel);
	}

	private void Cancel()
	{
		OnFinished();
		promise?.Cancel();
		promise = null;
	}

	private void OnCreated(CMapCharacter character)
	{
		CallbackPromise<CMapCharacter> temp = promise;
		promise = null;
		confirmationBox.Show(character, delegate
		{
			OnFinished();
			temp.Resolve(character);
		});
	}

	private void OnFinished()
	{
		creatorWindow.ResetUI();
		CameraController.s_CameraController?.FreeDisableCameraInput(this);
		mask.enabled = false;
		showAnimator.Stop();
		showAnimator.GoInitState();
		controllerArea.Destroy();
	}
}
