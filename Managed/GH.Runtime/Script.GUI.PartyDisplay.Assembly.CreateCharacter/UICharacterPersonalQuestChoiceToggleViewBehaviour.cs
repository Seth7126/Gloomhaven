using Script.GUI.Buttons;
using TMPro;
using UnityEngine;

namespace Script.GUI.PartyDisplay.Assembly.CreateCharacter;

public class UICharacterPersonalQuestChoiceToggleViewBehaviour : MonoBehaviour
{
	[SerializeField]
	private ToggleView _toggleView;

	[SerializeField]
	private UICharacterCreatorPersonalQuestChoice _choice;

	[SerializeField]
	private TextMeshProUGUI _text;

	private void OnEnable()
	{
		UpdateView();
		_choice.OnHover += _toggleView.Hover;
		_choice.OnToggle += OnSelect;
		_choice.OnFocus += UpdateView;
	}

	private void OnDisable()
	{
		_choice.OnHover -= _toggleView.Hover;
		_choice.OnToggle -= OnSelect;
		_choice.OnFocus -= UpdateView;
	}

	private void OnSelect(bool selected)
	{
		_toggleView.Select(_choice.IsSelected);
		_text.enabled = selected;
	}

	private void UpdateView()
	{
		_toggleView.Hover(_choice.IsHovered);
		OnSelect(_choice.IsSelected);
	}
}
