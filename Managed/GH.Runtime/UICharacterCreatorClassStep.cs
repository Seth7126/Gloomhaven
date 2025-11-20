using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using Assets.Script.Misc;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UICharacterCreatorClassStep : UICharacterCreatorStep<ICharacterCreatorClass>
{
	[SerializeField]
	private UICharacterCreatorClassInformation classInformation;

	[SerializeField]
	private Button abilityCardsButton;

	[SerializeField]
	private UICharacterCreatorCardsViewer abilityCardsViewer;

	[SerializeField]
	private UICharacterCreatorClassRoster classRoster;

	[SerializeField]
	private UIMapFTUEStep ftueClass;

	private Action<ICharacterCreatorClass, bool> onPreviewedClass;

	private Action<ICharacterCreatorClass> onHoveredClass;

	protected override void Awake()
	{
		base.Awake();
		abilityCardsButton.onClick.AddListener(OpenCardsView);
		classRoster.OnCharacterSelected.AddListener(UpdateInfo);
		if (InputManager.GamePadInUse)
		{
			classRoster.OnCharacterHover.AddListener(UpdateInfo);
		}
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.CREATOR_SHOW_ABILITY_CARDS, OpenCardsView).AddBlocker(new UIWindowOpenKeyActionBlocker(window)));
	}

	protected override void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			abilityCardsButton.onClick.RemoveAllListeners();
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.CREATOR_SHOW_ABILITY_CARDS, OpenCardsView);
			base.OnDestroy();
		}
	}

	public void Setup(List<ICharacterCreatorClass> classes, Action<ICharacterCreatorClass, bool> onPreviewedClass, Action<ICharacterCreatorClass> onHoveredClass = null)
	{
		this.onHoveredClass = onHoveredClass;
		this.onPreviewedClass = onPreviewedClass;
		classRoster.Setup(classes);
	}

	public ICallbackPromise<ICharacterCreatorClass> Show(ICharacterCreatorClass selectedClass, bool instant = false)
	{
		classRoster.ShowCharacterSelected(selectedClass);
		classRoster.Show(instant: true);
		UpdateInfo(selectedClass);
		return ProcessStep(instant);
	}

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		if (!MapFTUEManager.IsPlaying || Singleton<MapFTUEManager>.Instance.HasCompletedStep(ftueClass.Step))
		{
			return;
		}
		window.escapeKeyAction = UIWindow.EscapeKeyAction.Skip;
		if (!InputManager.GamePadInUse)
		{
			confirmButton.gameObject.SetActive(value: false);
		}
		cancelButton.gameObject.SetActive(value: false);
		Singleton<MapFTUEManager>.Instance.StartStep(ftueClass).Done(delegate
		{
			if (!InputManager.GamePadInUse)
			{
				confirmButton.gameObject.SetActive(value: true);
			}
			if (!MapFTUEManager.IsPlaying)
			{
				ResetButtons();
			}
		});
	}

	private void UpdateInfo(ICharacterCreatorClass character)
	{
		classInformation.gameObject.SetActive(value: true);
		classInformation.Display(character);
		onPreviewedClass?.Invoke(character, arg2: true);
		EnableConfirmationButton(enable: true);
		onHoveredClass?.Invoke(character);
	}

	private void ResetButtons()
	{
		window.escapeKeyAction = UIWindow.EscapeKeyAction.HideOnlyThis;
		if (!InputManager.GamePadInUse)
		{
			confirmButton.gameObject.SetActive(value: true);
			cancelButton.gameObject.SetActive(value: true);
		}
	}

	protected override ICharacterCreatorClass GetSelectedValue()
	{
		return classRoster.SelectedCharacter;
	}

	private void Deselect()
	{
		if (GetSelectedValue() != null)
		{
			onPreviewedClass?.Invoke(GetSelectedValue(), arg2: false);
			classRoster.ClearCharacterSelected();
			classInformation.gameObject.SetActive(value: false);
			EnableConfirmationButton(enable: false);
		}
	}

	protected override void OnHidden()
	{
		window.escapeKeyAction = UIWindow.EscapeKeyAction.None;
		base.OnHidden();
		classRoster.Hide();
		abilityCardsViewer.Hide();
	}

	private void OpenCardsView()
	{
		if (!Singleton<ESCMenu>.Instance.IsOpen)
		{
			abilityCardsViewer.Show(GetSelectedValue());
		}
	}

	public void TogglePerksTooltip()
	{
		classRoster.TogglePerksTooltip();
	}

	protected override void OnControllerAreaFocused()
	{
		if (!MapFTUEManager.IsPlaying || Singleton<MapFTUEManager>.Instance.HasCompletedStep(ftueClass.Step))
		{
			ResetButtons();
		}
		base.OnControllerAreaFocused();
		classRoster.OnFocused();
	}

	protected override void OnControllerAreaUnfocused()
	{
		base.OnControllerAreaUnfocused();
		classRoster.OnUnfocused();
	}
}
