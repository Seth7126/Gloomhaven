using System.Collections.Generic;
using System.Linq;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;

public class UIMapFTUEClassHighlight : UIIntroduceElementHighlight
{
	private Dictionary<ICharacterCreatorClass, UICharacterCreatorClassRosterSlot> classData;

	public void SetClass(Dictionary<ICharacterCreatorClass, UICharacterCreatorClassRosterSlot> classData)
	{
		this.classData = classData;
	}

	protected override void SetHighlighted(bool highlighted)
	{
		if (highlighted)
		{
			interactabilityControl.ExtendedButtonsToAllow.Clear();
			foreach (KeyValuePair<ICharacterCreatorClass, UICharacterCreatorClassRosterSlot> item in classData.Where((KeyValuePair<ICharacterCreatorClass, UICharacterCreatorClassRosterSlot> it) => AdventureState.MapState.MapParty.SelectedCharacters.All((CMapCharacter c) => c.CharacterYMLData != it.Key.Data)))
			{
				interactabilityControl.ExtendedButtonsToAllow.Add(item.Value.GetComponent<ExtendedButton>());
				item.Value.GetComponent<UIIntroduceElementHighlight>().Highlight();
			}
		}
		else
		{
			foreach (KeyValuePair<ICharacterCreatorClass, UICharacterCreatorClassRosterSlot> classDatum in classData)
			{
				classDatum.Value.GetComponent<UIIntroduceElementHighlight>().Unhighlight();
			}
		}
		base.SetHighlighted(highlighted);
	}
}
