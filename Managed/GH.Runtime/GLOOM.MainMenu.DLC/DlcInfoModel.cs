using System;
using ScenarioRuleLibrary;
using UnityEngine;

namespace GLOOM.MainMenu.DLC;

[Serializable]
public struct DlcInfoModel
{
	[field: SerializeField]
	public DLCRegistry.EDLCKey DlcType { get; private set; }

	[field: SerializeField]
	public string DescriptionLocalizationKey { get; private set; }

	[field: SerializeField]
	public Sprite DlcPromo { get; private set; }
}
