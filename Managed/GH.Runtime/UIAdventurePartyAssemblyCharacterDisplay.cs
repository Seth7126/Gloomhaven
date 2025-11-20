using System;
using System.Collections.Generic;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIAdventurePartyAssemblyCharacterDisplay : MonoBehaviour
{
	[Serializable]
	private struct CharacterPosition
	{
		public ECharacter character;

		public Vector3 position;
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private List<UICharacterInformation> information;

	[SerializeField]
	private UICharacterSkinSelector skinSelector;

	[SerializeField]
	private UILocalTooltip questTooltip;

	[SerializeField]
	private UITextTooltipTarget charTooltip;

	[SerializeField]
	private List<ControllerInputElement> controllerElements;

	protected CMapCharacter character;

	private Action onHidden;

	private void Awake()
	{
		window.onTransitionComplete.AddListener(delegate(UIWindow window, UIWindow.VisualState state)
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				DisableNavigation();
				skinSelector.Hide();
				onHidden?.Invoke();
			}
		});
	}

	public void Display(CMapCharacter characterData)
	{
		HideMercenaryInfoTooltip();
		window.Show(instant: false);
		string skinForCharacter = UIInfoTools.Instance.GetSkinForCharacter(characterData);
		Singleton<Character3DDisplayManager>.Instance.Display(this, characterData.CharacterYMLData.Model, skinForCharacter);
		foreach (UICharacterInformation item in information)
		{
			item.Display(characterData);
		}
		character = characterData;
		onHidden = null;
		skinSelector.Display(characterData, OnSelectedSkin);
	}

	private void OnSelectedSkin(string skin)
	{
		HideMercenaryInfoTooltip();
		character.SkinId = skin;
		Singleton<Character3DDisplayManager>.Instance.Display(this, character.CharacterYMLData.Model, character.SkinId);
		SaveData.Instance.SaveCurrentAdventureData();
	}

	public void Hide(bool instant = false)
	{
		HideMercenaryInfoTooltip();
		onHidden = null;
		if (instant)
		{
			window.Hide(instant: true);
			Singleton<Character3DDisplayManager>.Instance.Hide(this);
			return;
		}
		onHidden = delegate
		{
			Singleton<Character3DDisplayManager>.Instance.Hide(this);
		};
		window.Hide();
	}

	public void EnableNavigation()
	{
		if (window.IsOpen)
		{
			for (int i = 0; i < controllerElements.Count; i++)
			{
				controllerElements[i].enabled = true;
			}
		}
	}

	public void DisableNavigation()
	{
		for (int i = 0; i < controllerElements.Count; i++)
		{
			controllerElements[i].enabled = false;
		}
		HideMercenaryInfoTooltip();
	}

	public void HideMercenaryInfoTooltip()
	{
		if (questTooltip != null && questTooltip.IsShown)
		{
			questTooltip.Hide();
		}
		if (charTooltip != null && charTooltip.TooltipShown)
		{
			charTooltip.HideTooltip();
		}
	}

	public void ToggleMercenaryInfoTooltip()
	{
		if (questTooltip != null)
		{
			if (questTooltip.IsShown)
			{
				questTooltip.Hide();
			}
			else
			{
				questTooltip.Show();
			}
		}
		if (charTooltip != null)
		{
			charTooltip.ToggleTooltip();
		}
	}
}
