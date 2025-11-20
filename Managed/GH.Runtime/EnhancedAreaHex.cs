using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class EnhancedAreaHex : EnhancementButtonBase
{
	[SerializeField]
	private Image hexImage;

	public void Init(CEnhancement enhancement, string nonEnhanced, string enhanced, string abilityNameLookupText, RectTransform parentContainer)
	{
		base.Init(enhancement, abilityNameLookupText, parentContainer);
		hexImage.sprite = UIInfoTools.Instance.AreaEffectSpriteAtlas.GetSprite("Dot");
	}

	public void ApplyEnhancement()
	{
		EnhancementType = EEnhancement.Area;
		hexImage.sprite = UIInfoTools.Instance.AreaEffectSpriteAtlas.GetSprite("Enhanced");
	}

	public void RemoveEnhancement()
	{
		EnhancementType = EEnhancement.NoEnhancement;
		hexImage.sprite = UIInfoTools.Instance.AreaEffectSpriteAtlas.GetSprite("Dot");
	}
}
