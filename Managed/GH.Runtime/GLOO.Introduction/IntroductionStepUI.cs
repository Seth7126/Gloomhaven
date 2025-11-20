using System;
using System.Collections.Generic;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.Serialization;

namespace GLOO.Introduction;

[Serializable]
public class IntroductionStepUI
{
	public string LocalizationTextKey;

	public string LocalizationTextKeyController;

	public CLevelMessage.ELevelMessageLayoutType LayoutType = CLevelMessage.ELevelMessageLayoutType.FixedLowerRight;

	[Tooltip("Defines whether displaying the message should show the black bg")]
	public bool ShowScreenBG = true;

	[SerializeField]
	private IntroductionConditionConfigUI condition;

	public string Tag;

	[ConditionalField("LayoutType", "HelpText", true)]
	[SerializeField]
	[FormerlySerializedAs("AutoCloseCondition")]
	private IntroductionConditionConfigUI autoCloseCondition;

	public bool IsVisible
	{
		get
		{
			if (!(condition == null))
			{
				return condition.IsValid();
			}
			return true;
		}
	}

	public IntroductionConditionConfigUI AutoCloseCondition
	{
		get
		{
			if (LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText)
			{
				return null;
			}
			return autoCloseCondition;
		}
	}

	public CLevelMessage ToMessage()
	{
		return new CLevelMessage(null, LayoutType, (LayoutType == CLevelMessage.ELevelMessageLayoutType.HelpText) ? LocalizationTextKey : null, 0f, null, new CLevelTrigger
		{
			IsTriggeredByDismiss = true
		}, null, new List<CLevelMessagePage>
		{
			new CLevelMessagePage(LocalizationTextKey, LocalizationTextKeyController)
		}, shouldPause: false, ShowScreenBG, null, (LayoutType == CLevelMessage.ELevelMessageLayoutType.HelpText) ? LocalizationTextKeyController : null);
	}
}
