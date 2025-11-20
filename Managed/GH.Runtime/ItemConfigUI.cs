using SpriteMemoryManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Item", fileName = "ItemConfig")]
public class ItemConfigUI : ScriptableObject
{
	public string itemName;

	[Header("Card")]
	public ReferenceToSprite BackgroundImage;

	[Header("Use Item")]
	public Sprite miniIcon;

	public PreviewEffectInfo previewEffect;

	public string toggleAudioItem = "PlaySound_ScenarioUIEquipmentToggle_Normal";
}
