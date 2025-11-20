#define ENABLE_LOGS
using System.Text;
using GLOOM;
using ScenarioRuleLibrary;

public static class LocalizationScenarioModifierConveter
{
	public static string LocalizeText(this CScenarioModifier scenarioModifier, string scenarioID)
	{
		string Translation = "";
		if (!LocalizationManager.TryGetTranslation(scenarioID + "_" + scenarioModifier.Name + "_DESC", out Translation))
		{
			if (scenarioModifier.LocKey.IsNullOrEmpty())
			{
				Debug.LogWarningFormat("Missin LocKey in CScenarioModifier {0}", scenarioModifier.ScenarioAbilityID);
				return Translation;
			}
			Translation = LocalizationManager.GetTranslation(scenarioModifier.LocKey);
			string Translation2 = string.Empty;
			LocalizationManager.TryGetTranslation(scenarioModifier.TriggerLocKey, out Translation2);
			switch (scenarioModifier.ScenarioModifierType)
			{
			case EScenarioModifierType.SetElements:
			{
				CScenarioModifierSetElements cScenarioModifierSetElements = scenarioModifier as CScenarioModifierSetElements;
				StringBuilder stringBuilder = new StringBuilder();
				for (int k = 0; k < cScenarioModifierSetElements.StrongElements.Count; k++)
				{
					string translation3 = LocalizationManager.GetTranslation("GUI_ELEMENT_" + cScenarioModifierSetElements.StrongElements[k].ToString().ToUpper());
					if (k < cScenarioModifierSetElements.StrongElements.Count - 1)
					{
						stringBuilder.Append(translation3 + ", ");
					}
					else
					{
						stringBuilder.Append(translation3);
					}
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int l = 0; l < cScenarioModifierSetElements.WaningElements.Count; l++)
				{
					string translation4 = LocalizationManager.GetTranslation("GUI_ELEMENT_" + cScenarioModifierSetElements.WaningElements[l].ToString().ToUpper());
					if (l < cScenarioModifierSetElements.WaningElements.Count - 1)
					{
						stringBuilder2.Append(translation4 + ", ");
					}
					else
					{
						stringBuilder2.Append(translation4);
					}
				}
				StringBuilder stringBuilder3 = new StringBuilder();
				for (int m = 0; m < cScenarioModifierSetElements.InertElements.Count; m++)
				{
					string translation5 = LocalizationManager.GetTranslation("GUI_ELEMENT_" + cScenarioModifierSetElements.InertElements[m].ToString().ToUpper());
					if (m < cScenarioModifierSetElements.InertElements.Count - 1)
					{
						stringBuilder3.Append(translation5 + ", ");
					}
					else
					{
						stringBuilder3.Append(translation5);
					}
				}
				if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.WaningElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					Translation = string.Format(Translation, stringBuilder, stringBuilder2, stringBuilder3, Translation2);
				}
				else if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.WaningElements.Count > 0)
				{
					Translation = string.Format(Translation, stringBuilder, stringBuilder2, Translation2);
				}
				else if (cScenarioModifierSetElements.StrongElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					Translation = string.Format(Translation, stringBuilder, stringBuilder3, Translation2);
				}
				else if (cScenarioModifierSetElements.WaningElements.Count > 0 && cScenarioModifierSetElements.InertElements.Count > 0)
				{
					Translation = string.Format(Translation, stringBuilder2, stringBuilder3, Translation2);
				}
				else if (cScenarioModifierSetElements.StrongElements.Count > 0)
				{
					Translation = string.Format(Translation, stringBuilder, Translation2);
				}
				else if (cScenarioModifierSetElements.WaningElements.Count > 0)
				{
					Translation = string.Format(Translation, stringBuilder2, Translation2);
				}
				else if (cScenarioModifierSetElements.InertElements.Count > 0)
				{
					Translation = string.Format(Translation, stringBuilder3, Translation2);
				}
				break;
			}
			case EScenarioModifierType.TriggerAbility:
			{
				CScenarioModifierTriggerAbility cScenarioModifierTriggerAbility = scenarioModifier as CScenarioModifierTriggerAbility;
				Translation = string.Format(Translation, (cScenarioModifierTriggerAbility.TriggerAbility() != null) ? cScenarioModifierTriggerAbility.TriggerAbility().AbilityType.ToString() : "NULL", Translation2);
				break;
			}
			case EScenarioModifierType.AddConditionsToAbilities:
			{
				CScenarioModifierAddConditionsToAbilities cScenarioModifierAddConditionsToAbilities = scenarioModifier as CScenarioModifierAddConditionsToAbilities;
				string text = "";
				for (int i = 0; i < cScenarioModifierAddConditionsToAbilities.PositiveConditions.Count; i++)
				{
					text = text + LocalizationManager.GetTranslation(cScenarioModifierAddConditionsToAbilities.PositiveConditions[i].ToString()) + ", ";
					if (i < cScenarioModifierAddConditionsToAbilities.PositiveConditions.Count - 1 || cScenarioModifierAddConditionsToAbilities.NegativeConditions.Count > 0)
					{
						text += ", ";
					}
				}
				for (int j = 0; j < cScenarioModifierAddConditionsToAbilities.NegativeConditions.Count; j++)
				{
					text += LocalizationManager.GetTranslation(cScenarioModifierAddConditionsToAbilities.NegativeConditions[j].ToString());
					if (j < cScenarioModifierAddConditionsToAbilities.NegativeConditions.Count - 1)
					{
						text += ", ";
					}
				}
				if (scenarioModifier.ScenarioModifierFilter != null && scenarioModifier.ScenarioModifierFilter.FilterAbilityType != CAbility.EAbilityType.None)
				{
					string translation = LocalizationManager.GetTranslation(scenarioModifier.ScenarioModifierFilter.FilterAbilityType.ToString());
					if (scenarioModifier.ScenarioModifierFilter.FilterActorType != CAbilityFilter.EFilterActorType.None)
					{
						string translation2 = LocalizationManager.GetTranslation(scenarioModifier.ScenarioModifierFilter.ActorLocKey);
						Translation = string.Format(Translation, translation, text, translation2);
					}
					else
					{
						Translation = string.Format(Translation, translation, text);
					}
				}
				Translation = string.Format(Translation, text);
				break;
			}
			}
		}
		return Translation;
	}
}
