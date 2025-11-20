using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;

namespace GLOOM.MainMenu.DLC;

[CreateAssetMenu(menuName = "UI Config/DLC info gamepad")]
public class DLCsContentInfo : ScriptableObject
{
	[SerializeField]
	private List<DlcInfoModel> _dlcList;

	public bool HasDlcModel(DLCRegistry.EDLCKey dlcType)
	{
		return _dlcList.Any((DlcInfoModel x) => x.DlcType == dlcType);
	}

	public string GetDlcDescription(DLCRegistry.EDLCKey dlcType)
	{
		foreach (DlcInfoModel dlc in _dlcList)
		{
			if (dlc.DlcType == dlcType)
			{
				return LocalizationManager.GetTranslation(dlc.DescriptionLocalizationKey);
			}
		}
		throw new Exception("Description can't be empty! Enter description in " + dlcType);
	}

	public Sprite GetDlsPromo(DLCRegistry.EDLCKey dlcType)
	{
		foreach (DlcInfoModel dlc in _dlcList)
		{
			if (dlc.DlcType == dlcType)
			{
				return dlc.DlcPromo;
			}
		}
		throw new Exception("Need set DLC promo sprite in " + dlcType);
	}
}
