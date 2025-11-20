using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SpriteMemoryManagement;
using UnityEngine;

public abstract class DistributeRewardService : IDistributePointsService
{
	public class DistributeMapActor : IDistributePointsActor
	{
		public CMapCharacter Character { get; private set; }

		public bool IsNegative { get; private set; }

		public bool ShowGold { get; private set; }

		public string Name
		{
			get
			{
				if (!Character.CharacterName.IsNOTNullOrEmpty())
				{
					return LocalizationManager.GetTranslation(Character.CharacterYMLData.LocKey);
				}
				return Character.CharacterName;
			}
		}

		public ReferenceToSprite Portrait
		{
			get
			{
				ReferenceToSprite referenceToSprite = new ReferenceToSprite();
				referenceToSprite.SetSpriteInsteadAddressable(UIInfoTools.Instance.GetCharacterDistributionPortrait(Character.CharacterYMLData.Model, Character.CharacterYMLData.CustomCharacterConfig));
				return referenceToSprite;
			}
		}

		public int AssignedPoints { get; internal set; }

		public DistributeMapActor(CMapCharacter character, bool isNegative, bool showGold)
		{
			Character = character;
			IsNegative = isNegative;
			ShowGold = showGold;
		}
	}

	protected List<DistributeMapActor> actors;

	public Reward Reward { get; protected set; }

	public int AvailablePoints { get; protected set; }

	protected DistributeRewardService(List<CMapCharacter> characters, Reward reward)
	{
		Reward = reward;
		AvailablePoints = reward.Amount;
		bool isNegative = reward.IsNegative();
		actors = characters.ConvertAll((CMapCharacter it) => new DistributeMapActor(it, isNegative, reward.Type == ETreasureType.Gold));
	}

	public List<IDistributePointsActor> GetActors()
	{
		return actors.Cast<IDistributePointsActor>().ToList();
	}

	public abstract Sprite GetTitleIcon();

	public abstract string GetTitleText();

	public abstract bool CanRemovePointsFrom(IDistributePointsActor actor);

	public abstract bool CanAddPointsTo(IDistributePointsActor actor);

	public abstract void AddPoint(IDistributePointsActor actor);

	public abstract void RemovePoint(IDistributePointsActor actor);

	public abstract int GetMaxPoints(IDistributePointsActor actor);

	public abstract int GetCurrentPoints(IDistributePointsActor actor);

	public int GetAssignedPoints(IDistributePointsActor actor)
	{
		return ((DistributeMapActor)actor).AssignedPoints;
	}

	public IDistributePointsActor GetActor(int modelInstanceID)
	{
		string characterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(modelInstanceID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(modelInstanceID));
		return actors.SingleOrDefault((DistributeMapActor s) => s.Character.CharacterID == characterID);
	}

	public IDistributePointsActor GetActor(GameAction gameAction)
	{
		string characterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(gameAction.ActorID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(gameAction.ActorID));
		return actors.SingleOrDefault((DistributeMapActor s) => s.Character.CharacterID == characterID);
	}

	public abstract void Apply();
}
