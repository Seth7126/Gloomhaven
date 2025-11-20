using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum EPropType
{
	None,
	GoldPile,
	Chest,
	GoalChest,
	Trap,
	OneHexObstacle,
	TwoHexObstacle,
	Spawner,
	PressurePlate,
	TerrainHotCoals,
	TerrainWater,
	TerrainRubble,
	TerrainThorns,
	ThreeHexObstacle,
	DarkPitObstacle,
	Portal,
	TerrainVisualEffect,
	MonsterGrave,
	ThreeHexCurvedObstacle,
	ThreeHexStraightObstacle,
	QuestItem,
	CharacterResource
}
